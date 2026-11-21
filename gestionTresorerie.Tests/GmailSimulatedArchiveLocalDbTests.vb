Option Strict On
Option Explicit On

Imports System
Imports System.Data
Imports System.Globalization
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Contrat d'integration TEST : un JSON provenant d'une boite Gmail simulee
''' peut etre classe par le service documentaire existant, uniquement apres
''' une decision explicite. Ce test ne raccorde PAS encore le bouton de revue.
''' </summary>
<TestClass>
Public Class GmailSimulatedArchiveLocalDbTests
    Private Const InvoiceJson As String =
        "{""type_document"":""Facture"",""donnees_extraites"":{""identite"":{""fournisseur_nom"":""Fournisseur fictif""},""transaction"":{""montant_ttc"":""42.50"",""devise"":""EUR"",""date_facture"":""2026-04-01"",""numero_facture"":""GMAIL-TEST-001""}}}"

    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub PieceGmailFictive_ClassementExpliciteRetourneIdEtArchiveDansTest()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB requis sur runner TEST jetable.")
        End If

        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "piece-fictive.png")
            Using bitmap As New Bitmap(10, 10)
                bitmap.Save(source, Imaging.ImageFormat.Png)
            End Using

            Dim messages As GmailDocumentSimulatedInbox.MessageFictif() = {
                New GmailDocumentSimulatedInbox.MessageFictif(
                    "simulation@example.invalid", 7UI, 15UI,
                    {New GmailDocumentSimulatedInbox.PieceFictive(
                        InvoiceJson, PropositionMouvement.SensMouvement.Debit)})}
            Dim revue = GmailDocumentSimulatedInbox.PreparerRevue(messages)
            Assert.AreEqual(1, revue.ResultatInitial.Propositions.Count)
            Assert.AreEqual(0, revue.ResultatInitial.Rejets.Count)

            Dim pieceAnalysee = revue.ResultatInitial.Propositions(0)
            Assert.AreEqual("GMAIL-TEST-001", pieceAnalysee.Reference)
            Assert.AreEqual(42.5D, pieceAnalysee.Montant)
            Assert.AreEqual(New DateTime(2026, 4, 1), pieceAnalysee.DateProposee)
            Dim culture = CultureInfo.GetCultureInfo("fr-FR")
            Dim empreinte = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source)))
            Dim proposition = SqlTestDocumentEntry.Prepare(
                source, empreinte, root, False,
                pieceAnalysee.Reference, "Fournisseur fictif",
                pieceAnalysee.DateProposee.ToString("dd/MM/yyyy", culture),
                pieceAnalysee.Montant.ToString("0.00", culture), Nothing)
            Assert.IsFalse(File.Exists(proposition.Destination),
                           "La simple extraction/revue ne doit jamais archiver.")
            Dim store As New SqlDocumentTestStore()
            store.Initialize()
            ' La revue Gmail seule ne doit creer ni archive ni ligne SQL TEST.
            Using avantValidation As DataTable = store.ReadDocuments()
                Assert.AreEqual(0, avantValidation.Rows.Cast(Of DataRow)().Count(
                    Function(row) Convert.ToString(row("cheminDoc")) = proposition.Destination),
                    "La proposition non validee ne doit pas etre persistée.")
            End Using
            Assert.IsFalse(File.Exists(proposition.Destination),
                           "La revue et l'initialisation SQL ne doivent pas archiver.")

            Dim id = store.SaveVerified(proposition,
                Function(changements As String) CType(Nothing, Boolean?))
            Assert.IsTrue(id.HasValue, "Le classement explicite doit retourner idDoc.")
            Assert.IsTrue(id.Value > 0)
            Assert.IsTrue(File.Exists(proposition.Destination))
            Assert.IsTrue(File.Exists(source))
            Assert.AreEqual(id, store.SaveVerified(proposition,
                Function(changements As String) CType(Nothing, Boolean?)),
                "Le rejeu ne doit pas creer de doublon.")

            Using documents As DataTable = store.ReadDocuments()
                Dim lignes As New List(Of DataRow)()
                For Each row As DataRow In documents.Rows
                    If String.Equals(Convert.ToString(row("cheminDoc")), proposition.Destination, StringComparison.OrdinalIgnoreCase) Then
                        lignes.Add(row)
                    End If
                Next
                Assert.AreEqual(1, lignes.Count)
                Assert.AreEqual(id.Value, Convert.ToInt32(lignes(0)("idDoc")))
                Assert.AreEqual("Facture", Convert.ToString(lignes(0)("categorieDoc")))
                StringAssert.Contains(Convert.ToString(lignes(0)("metaDonnees")), "GMAIL-TEST-001")
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub
End Class
