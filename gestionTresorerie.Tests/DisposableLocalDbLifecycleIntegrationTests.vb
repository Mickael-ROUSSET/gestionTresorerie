Option Strict On

Imports System
Imports System.Data.SqlClient
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Vérifie le cycle SQL réel sur un catalogue jetable appartenant au test.
''' Jamais bddAgumaaa ni bddAgumaaaTest. Opt-in uniquement sur un runner LocalDB dédié.
''' </summary>
<TestClass>
Public Class DisposableLocalDbLifecycleIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub CatalogueJetable_CreationSchemaLectureEtSuppression()
        If Not String.Equals(Environment.GetEnvironmentVariable("AGUMAAA_RUN_DISPOSABLE_LOCALDB_TESTS"),
                             "1", StringComparison.Ordinal) Then
            Assert.Inconclusive("LocalDB jetable non activé sur ce runner.")
        End If

        Assert.ThrowsExactly(Of ArgumentNullException)(Function() SqlDocumentTestStore.OpenDisposable(Nothing))

        Dim catalogName As String
        Using catalog = DisposableLocalDbTestCatalog.Create()
            catalogName = catalog.Name
            Assert.IsTrue(DisposableLocalDbTestCatalog.IsDisposableName(catalogName))
            Dim builder As New SqlConnectionStringBuilder(catalog.ConnectionString)
            Assert.AreEqual(catalogName, builder.InitialCatalog)

            Using connection = SqlDocumentTestStore.OpenDisposable(catalog)
                connection.Open()
                Assert.AreEqual(catalogName, connection.Database)
                Using transaction = connection.BeginTransaction()
                    For Each options In {(True, True), (False, False)}
                        Dim rejected = False
                        Try
                            LocalDbSchema.Apply(connection, transaction,
                                                production:=options.Item1, allowDisposable:=options.Item2)
                        Catch ex As InvalidOperationException
                            rejected = True
                        End Try
                        Assert.IsTrue(rejected, "Un catalogue jetable doit exiger TEST et allowDisposable=True.")
                    Next
                    LocalDbSchema.Apply(connection, transaction, production:=False, allowDisposable:=True)
                    transaction.Commit()
                End Using
                Using command = connection.CreateCommand()
                    command.CommandText = "SELECT DB_NAME(), OBJECT_ID(N'dbo.Documents', N'U');"
                    Using reader = command.ExecuteReader()
                        Assert.IsTrue(reader.Read())
                        Assert.AreEqual(catalogName, reader.GetString(0))
                        Assert.IsFalse(reader.IsDBNull(1), "La table Documents doit exister.")
                    End Using
                End Using
            End Using
        End Using

        ' Le nettoyage appartient exclusivement à DisposableLocalDbTestCatalog.Dispose.
        Using master As New SqlConnection(LocalDbCatalogs.ConnectionString("master"))
            master.Open()
            Using command = master.CreateCommand()
                command.CommandText = "SELECT DB_ID(@catalog);"
                command.Parameters.Add("@catalog", System.Data.SqlDbType.NVarChar, 128).Value = catalogName
                Assert.IsTrue(command.ExecuteScalar() Is DBNull.Value,
                              "Le catalogue jetable doit être supprimé après Dispose.")
            End Using
        End Using
    End Sub
End Class
