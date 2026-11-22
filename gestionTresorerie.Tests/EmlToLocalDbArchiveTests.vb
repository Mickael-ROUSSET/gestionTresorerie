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
    ''' <summary>
    ''' Construit un PDF 1.4 minimal avec offsets xref exacts pour tester la prévisualisation.
    ''' Aucune donnée comptable réelle ni dépendance externe.
    ''' </summary>
    Private Shared Function BuildValidFictitiousPdf() As Byte()
        Dim objects As String() = {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 300 200] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            "<< /Length 48 >>" & vbLf & "stream" & vbLf &
                "BT /F1 14 Tf 25 100 Td (DOCUMENT TEST) Tj ET" & vbLf &
                "endstream"
        }
        ' La longueur du flux est calculée, jamais codée en dur.
        Dim contentStream = "BT /F1 14 Tf 25 100 Td (DOCUMENT TEST) Tj ET" & vbLf
        objects(4) = "<< /Length " & Encoding.ASCII.GetByteCount(contentStream).ToString(Globalization.CultureInfo.InvariantCulture) &
                     " >>" & vbLf & "stream" & vbLf & contentStream & "endstream"
        Using output As New MemoryStream()
            Dim writeAscii As Action(Of String) =
                Sub(value As String)
                    Dim bytes = Encoding.ASCII.GetBytes(value)
                    output.Write(bytes, 0, bytes.Length)
                End Sub
            writeAscii("%PDF-1.4" & vbLf)
            Dim offsets As New List(Of Long) From {0}
            For i = 0 To objects.Length - 1
                offsets.Add(output.Position)
                writeAscii((i + 1).ToString(Globalization.CultureInfo.InvariantCulture) &
                           " 0 obj" & vbLf & objects(i) & vbLf & "endobj" & vbLf)
            Next
            Dim xrefPosition = output.Position
            writeAscii("xref" & vbLf & "0 " & offsets.Count.ToString(Globalization.CultureInfo.InvariantCulture) &
                       vbLf & "0000000000 65535 f " & vbLf)
            For i = 1 To offsets.Count - 1
                writeAscii(offsets(i).ToString("D10", Globalization.CultureInfo.InvariantCulture) &
                           " 00000 n " & vbLf)
            Next
            writeAscii("trailer" & vbLf & "<< /Size " &
                       offsets.Count.ToString(Globalization.CultureInfo.InvariantCulture) &
                       " /Root 1 0 R >>" & vbLf & "startxref" & vbLf &
                       xrefPosition.ToString(Globalization.CultureInfo.InvariantCulture) &
                       vbLf & "%%EOF" & vbLf)
            Return output.ToArray()
        End Using
    End Function

    <TestMethod>
    Public Sub PdfFictif_ContientUnXrefEtUneFinValides()
        Dim pdf = BuildValidFictitiousPdf()
        Dim content = Encoding.ASCII.GetString(pdf)
        StringAssert.StartsWith(content, "%PDF-1.4")
        StringAssert.Contains(content, "DOCUMENT TEST")
        Dim marker = "startxref" & vbLf
        Dim markerAt = content.LastIndexOf(marker, StringComparison.Ordinal)
        Assert.IsTrue(markerAt > 0)
        Dim start = markerAt + marker.Length
        Dim finish = content.IndexOf(vbLf, start, StringComparison.Ordinal)
        Dim offset = Integer.Parse(content.Substring(start, finish - start),
                                   Globalization.CultureInfo.InvariantCulture)
        Assert.IsTrue(offset > 0 AndAlso offset < pdf.Length)
        StringAssert.StartsWith(content.Substring(offset), "xref" & vbLf)
        Assert.IsTrue(content.TrimEnd().EndsWith("%%EOF", StringComparison.Ordinal))
    End Sub

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
                builder.Attachments.Add("facture-test.pdf", BuildValidFictitiousPdf(),
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
