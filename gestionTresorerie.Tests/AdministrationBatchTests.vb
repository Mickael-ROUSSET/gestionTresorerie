Option Strict On
Imports System.Data.SqlClient
Imports System.Diagnostics
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports gestionTresorerie

<TestClass>
Public Class AdministrationBatchTests
    <STATestMethod>
    Public Sub AssistantContientLesTroisEtapesEtLaRequeteDeCourriers()
        Using assistant As New AssistantBatchCourrier()
            Dim pages = assistant.Controls.OfType(Of System.Windows.Forms.TabControl)().Single()
            Assert.AreEqual(3, pages.TabPages.Count)
            Assert.IsTrue(AssistantBatchCourrier.Requetes().Contains("reqIdentiteCoordonnees"))
        End Using
    End Sub

    Private Shared Sub Instance(arguments As String)
        Using p = Process.Start(New ProcessStartInfo("sqllocaldb", arguments) With {.UseShellExecute=False,.CreateNoWindow=True})
            p.WaitForExit()
            Assert.AreEqual(0,p.ExitCode)
        End Using
    End Sub

    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub SauvegardeVideEtRestaureUneInstanceJetable()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then Assert.Inconclusive()
        Dim id = "AgTest" & Guid.NewGuid().ToString("N").Substring(0,10)
        Dim base = "bddAgumaaaTest"
        Dim builder As New SqlConnectionStringBuilder With {.DataSource="(localdb)\" & id,.InitialCatalog="master",.IntegratedSecurity=True,.Pooling=False}
        Dim dossier = Directory.CreateTempSubdirectory().FullName
        Dim sauvegarde = Path.Combine(dossier,"test.bak")
        Instance("create " & id & " -s")
        Try
            Using cn As New SqlConnection(builder.ConnectionString), cmd=cn.CreateCommand()
                cn.Open()
                cmd.CommandText = "CREATE DATABASE [bddAgumaaaTest] ON (NAME=N'fixture', FILENAME=N'" & Path.Combine(dossier,"fixture.mdf").Replace("'","''") & "') LOG ON (NAME=N'fixture_log', FILENAME=N'" & Path.Combine(dossier,"fixture.ldf").Replace("'","''") & "')"
                cmd.ExecuteNonQuery()
            End Using
            builder.InitialCatalog=base
            Dim cs=builder.ConnectionString
            Using cn As New SqlConnection(cs), cmd=cn.CreateCommand()
                cn.Open()
                cmd.CommandText="CREATE TABLE dbo.Documents(id int); INSERT dbo.Documents VALUES(7);"
                cmd.ExecuteNonQuery()
            End Using
            Dim fichier=Path.Combine(dossier,"export.json")
            EchangeDonnees.Exporter(cs,fichier)
            AdministrationBase.Vider(cs,AdministrationBase.TablesMetier(cs))
            EchangeDonnees.Importer(cs,fichier)
            Dim admin As New SqlAdminService(New AgumaaaConnectionFactory(cs))
            admin.SauvegarderBase(base,sauvegarde)
            AdministrationBase.Vider(cs,AdministrationBase.TablesMetier(cs))
            Using cn As New SqlConnection(cs), cmd=cn.CreateCommand()
                cn.Open()
                cmd.CommandText="SELECT COUNT(*) FROM dbo.Documents"
                Assert.AreEqual(0,CInt(cmd.ExecuteScalar()))
            End Using
            AdministrationBase.Restaurer(cs,sauvegarde)
            Using cn As New SqlConnection(cs), cmd=cn.CreateCommand()
                cn.Open()
                cmd.CommandText="SELECT id FROM dbo.Documents"
                Assert.AreEqual(7,CInt(cmd.ExecuteScalar()))
            End Using
        Finally
            Instance("stop " & id & " -k")
            Instance("delete " & id)
            Directory.Delete(dossier, True)
        End Try
    End Sub
End Class
