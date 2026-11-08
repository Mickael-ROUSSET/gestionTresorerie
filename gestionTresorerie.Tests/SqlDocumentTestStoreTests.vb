Option Strict On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Data.SqlClient
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlDocumentTestStoreTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub CreeLeCatalogueTestEtRelitUneArchiveSansDoublon()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("Test SQL réel réservé au runner jetable.")
        End If

        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "facture.png")
            Using image As New Bitmap(10, 10)
                image.Save(source, Imaging.ImageFormat.Png)
            End Using
            Dim store As New SqlDocumentTestStore()
            store.Initialize()
            store.Initialize()
            Using connection As New SqlConnection("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaaTest;Integrated Security=True")
                connection.Open()
                Using command = connection.CreateCommand()
                    command.CommandText = "SELECT COUNT(*) FROM dbo.SchemaMigrations WHERE Version=1 AND Description=N'Initialisation documentaire TEST';"
                    Assert.AreEqual(1, CInt(command.ExecuteScalar()), "La migration initiale doit être idempotente.")
                End Using
            End Using
            ' Simule une ligne de migration altérée dans le catalogue TEST jetable.
            Using connection As New SqlConnection("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaaTest;Integrated Security=True")
                connection.Open()
                Using command = connection.CreateCommand()
                    command.CommandText = "UPDATE dbo.SchemaMigrations SET Description=N'Migration inconnue' WHERE Version=1;"
                    Assert.AreEqual(1, command.ExecuteNonQuery())
                    Try
                        Assert.ThrowsExactly(Of SqlException)(Sub() store.Initialize())
                        Assert.ThrowsExactly(Of SqlException)(Sub() store.ReadDocuments())
                    Finally
                        command.CommandText = "UPDATE dbo.SchemaMigrations SET Description=N'Initialisation documentaire TEST' WHERE Version=1;"
                        command.ExecuteNonQuery()
                    End Try
                End Using
            End Using
            ' Une migration future ne doit pas permettre de consulter les documents TEST.
            Using connection As New SqlConnection("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaaTest;Integrated Security=True")
                connection.Open()
                Using command = connection.CreateCommand()
                    command.CommandText = "INSERT INTO dbo.SchemaMigrations (Version, Description) VALUES (2, N'Version future fictive');"
                    Assert.AreEqual(1, command.ExecuteNonQuery())
                    Try
                        Assert.ThrowsExactly(Of SqlException)(Sub() store.ReadDocuments())
                        Assert.ThrowsExactly(Of SqlException)(Sub() store.Initialize())
                    Finally
                        command.CommandText = "DELETE FROM dbo.SchemaMigrations WHERE Version=2;"
                        Assert.AreEqual(1, command.ExecuteNonQuery())
                    End Try
                End Using
            End Using
            store.Initialize()
            Dim proposal = DocumentArchiveProposal.Prepare(source, DocumentArchiveProposal.HashSource(source),
                DemoDocumentSamples.Invoice, root, AddressOf TestSetting)

            proposal.Confirm(AddressOf store.Save)
            proposal.Confirm(AddressOf store.Save)
            Dim cheque = SqlTestDocumentEntry.Prepare(source, DocumentArchiveProposal.HashSource(source), root,
                True, "000042", "Association exemple", "03/04/2026", "52,90", chequeEmis:=False)
            cheque.Confirm(AddressOf store.Save)
            cheque.Confirm(AddressOf store.Save)
            Assert.IsTrue(File.Exists(cheque.Destination))
            Assert.IsTrue(File.Exists(proposal.Destination))
            Assert.IsTrue(File.Exists(source))
            Using documents = store.ReadDocuments()
                Dim rows = documents.Rows.Cast(Of DataRow)().
                    Where(Function(row) CStr(row("cheminDoc")) = proposal.Destination).ToArray()
                Assert.HasCount(1, rows)
                Assert.AreEqual("Facture", CStr(rows(0)("categorieDoc")))
                StringAssert.Contains(CStr(rows(0)("metaDonnees")), "DEMO-001")
                Dim cheques = documents.Rows.Cast(Of DataRow)().
                    Where(Function(row) CStr(row("cheminDoc")) = cheque.Destination).ToArray()
                Assert.HasCount(1, cheques)
                Assert.AreEqual("Cheque", CStr(cheques(0)("categorieDoc")))
                Assert.AreEqual("Reçus", CStr(cheques(0)("sousCategorieDoc")))
                StringAssert.Contains(CStr(cheques(0)("metaDonnees")), "000042")
                StringAssert.Contains(CStr(cheques(0)("metaDonnees")), "Association exemple")
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LaSaisieRefuseUnChequeSansSensEtUnMontantAmbigu()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "cheque.png")
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() SqlTestDocumentEntry.Prepare(source, "hash", root, True,
                    "000042", "Association exemple", "03/04/2026", "52,90"))
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() SqlTestDocumentEntry.Prepare(source, "hash", root, False,
                    "F-1", "Fournisseur exemple", "03/04/2026", "1.234"))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    Private Shared Function TestSetting(name As String) As String
        Select Case name
            Case "repRacineComptabilité" : Return "Archives-TEST"
            Case "repFichiersDocumentsFactures" : Return "Factures"
            Case "repFichiersFacturesRecues" : Return "Recues"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(name))
        End Select
    End Function
End Class
