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

    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub AdhesionCreePuisCompareLeTiersAccentueEtConserveLesChampsAbsents()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then Assert.Inconclusive()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "bulletin.png")
            File.WriteAllBytes(source, {CByte(3), CByte(7)})
            Dim store As New SqlDocumentTestStore()
            store.Initialize()
            Dim name = "Fictif" & Guid.NewGuid().ToString("N").Substring(0, 10)
            Dim fields As New Newtonsoft.Json.Linq.JObject From {{"ville", "Ville A"}, {"telephone", "0600000000"}, {"genre", "F"}}
            Dim hash = DocumentArchiveProposal.HashSource(source)
            Dim first = DocumentArchiveProposal.PrepareAdministrativeTest(source, hash, root, gestionTresorerie.Agumaaa.TypeDocument.FormulaireAdhesion,
                name, "01/11/2026", "", "", "Éloïse", "1 rue des Tests", True, fields)
            Assert.IsTrue(store.SaveVerified(first, Function(delta) ThrowUnexpectedReview()).HasValue)
            Assert.IsTrue(File.Exists(first.Destination))
            fields.Remove("telephone")
            fields("ville") = "Ville B"
            Dim second = DocumentArchiveProposal.PrepareAdministrativeTest(source, hash, root, gestionTresorerie.Agumaaa.TypeDocument.FormulaireAdhesion,
                name, "02/11/2026", "", "", "Eloise", "", True, fields)
            Dim cancelled = store.SaveVerified(second, Function(delta)
                StringAssert.Contains(delta, "Ville A → Ville B")
                Return Nothing
            End Function)
            Assert.IsFalse(cancelled.HasValue)
            Assert.IsFalse(File.Exists(second.Destination))
            Dim id = store.SaveVerified(second, Function(delta) True)
            Assert.IsTrue(id.HasValue)
            Using conn As New SqlConnection("Server=(LocalDB)\MSSQLLocalDB;Database=bddAgumaaaTest;Integrated Security=True")
                conn.Open()
                Using cmd As New SqlCommand("SELECT c.Ville,c.Telephone FROM dbo.Tiers t JOIN dbo.Coordonnees c ON c.IdTiers=t.Id JOIN dbo.Documents d ON t.Id=TRY_CONVERT(int,JSON_VALUE(d.metaDonnees,'$.idTiers')) WHERE d.idDoc=@id AND t.nom=@nom", conn)
                    cmd.Parameters.AddWithValue("@id", id.Value)
                    cmd.Parameters.AddWithValue("@nom", name)
                    Using reader = cmd.ExecuteReader()
                        Assert.IsTrue(reader.Read())
                        Assert.AreEqual("Ville B", reader.GetString(0))
                        Assert.AreEqual("0600000000", reader.GetString(1))
                        Assert.IsFalse(reader.Read())
                    End Using
                End Using
            End Using
        Finally
            Directory.Delete(root, True)
        End Try
    End Sub

    Private Shared Function ThrowUnexpectedReview() As Boolean?
        Throw New InvalidOperationException("Une nouvelle identité ne demande pas de fusion.")
    End Function

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

    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub RefuseUneMigrationTestAltereePuisReprend()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("Test SQL réel réservé au runner jetable.")
        End If
        Dim store As New SqlDocumentTestStore()
        store.Initialize()
        Dim connectionString = "(LocalDB)\MSSQLLocalDB"
        Dim builder As New SqlConnectionStringBuilder() With {
            .DataSource = connectionString,
            .InitialCatalog = "bddAgumaaaTest",
            .IntegratedSecurity = True
        }
        Using connection As New SqlConnection(builder.ConnectionString)
            connection.Open()
            Using command = connection.CreateCommand()
                Try
                    command.CommandText = "UPDATE dbo.SchemaMigrations SET Description=N'Alteration fictive' WHERE Version=1;"
                    command.ExecuteNonQuery()
                    Assert.ThrowsExactly(Of SqlException)(Sub() store.ReadDocuments())
                    Assert.ThrowsExactly(Of SqlException)(Sub() store.Initialize())
                Finally
                    command.CommandText = "UPDATE dbo.SchemaMigrations SET Description=N'Initialisation documentaire TEST' WHERE Version=1;"
                    command.ExecuteNonQuery()
                End Try
            End Using
        End Using
        store.Initialize()
        Using rows = store.ReadDocuments()
            Assert.IsNotNull(rows)
        End Using
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
