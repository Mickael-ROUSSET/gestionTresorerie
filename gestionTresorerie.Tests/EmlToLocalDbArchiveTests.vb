Option Strict On
Option Explicit On

Imports System
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports MimeKit

''' <summary>
''' Integration TEST du fichier EML reellement extrait au classement explicite LocalDB.
''' Aucun serveur IMAP/Gmail, aucune base historique, aucun document reel.
''' </summary>
<TestClass>
Public Class EmlToLocalDbArchiveTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub EmlFictif_ExtractionPuisValidationExplicite_CreeUneSeuleLigneEtArchive()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB TEST jetable requis ; recette interactive non couverte.")
        End If

        Dim root = Directory.CreateTempSubdirectory("eml-localdb-test-").FullName
        Try
            Dim inbox = Path.Combine(root, "inbox")
            Dim extracted = Path.Combine(root, "extracted")
            Dim archive = Path.Combine(root, "archive-test")
            Directory.CreateDirectory(inbox)
            Dim eml = Path.Combine(inbox, "fictif.eml")
            Using message As New MimeMessage()
                message.From.Add(New MailboxAddress("Simulation", "simulation@example.invalid"))
                message.To.Add(New MailboxAddress("TEST", "test@example.invalid"))
                message.Date = New DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero)
                message.Subject = "Facture fictive"
                Dim builder As New BodyBuilder With {.TextBody = "TEST sans Gmail"}
                builder.Attachments.Add("facture-test.pdf", Encoding.UTF8.GetBytes("%PDF-1.4" & vbLf & "FICTIF"),
                                        ContentType.Parse("application/pdf"))
                message.Body = builder.ToMessageBody()
                Using stream = File.Create(eml)
                    message.WriteTo(stream)
                End Using
            End Using

            Dim files = GmailSimulatedInbox.Extract(inbox, extracted, New DateTime(2026, 11, 1),
                                                    "simulation@example.invalid")
            Assert.AreEqual(1, files.Count)
            Assert.IsTrue(File.Exists(files(0)))
            Dim json = "{""type_document"":""Facture"",""donnees_extraites"":{""identite"":{""fournisseur_nom"":""Fournisseur fictif""},""transaction"":{""montant_ttc"":""42.50"",""date_facture"":""2026-11-20"",""numero_facture"":""EML-LOCALDB-TEST""}}}"
            Dim proposition = DocumentArchiveProposal.PrepareTest(
                files(0), DocumentArchiveProposal.HashSource(files(0)), json, archive, Nothing)
            Assert.IsFalse(File.Exists(proposition.Destination), "Extraire ne doit pas archiver.")
            Dim store As New SqlDocumentTestStore()
            store.Initialize()
            Using before As DataTable = store.ReadDocuments()
                Assert.AreEqual(0, before.Rows.Cast(Of DataRow)().Count(
                    Function(row) Convert.ToString(row("cheminDoc")) = proposition.Destination))
            End Using

            Dim id = store.SaveVerified(proposition, Function(changements As String) CType(Nothing, Boolean?))
            Assert.IsTrue(id.HasValue AndAlso id.Value > 0, "idDoc attendu apres validation.")
            Assert.IsTrue(File.Exists(proposition.Destination))
            Assert.IsTrue(File.Exists(files(0)), "L'extraction source doit rester intacte.")
            Assert.IsTrue(File.Exists(eml), "Le message EML source doit rester intact.")
            Assert.AreEqual(id, store.SaveVerified(proposition,
                Function(changements As String) CType(Nothing, Boolean?)), "Rejeu sans doublon.")
            Using after As DataTable = store.ReadDocuments()
                Dim rows = after.Rows.Cast(Of DataRow)().
                    Where(Function(row) Convert.ToString(row("cheminDoc")) = proposition.Destination).ToArray()
                Assert.AreEqual(1, rows.Length)
                Assert.AreEqual(id.Value, Convert.ToInt32(rows(0)("idDoc")))
                StringAssert.Contains(Convert.ToString(rows(0)("metaDonnees")), "EML-LOCALDB-TEST")
            End Using
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Sub
End Class
