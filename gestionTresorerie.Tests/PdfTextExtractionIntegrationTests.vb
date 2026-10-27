Imports System
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

' Integration locale : PDF minimal valide, donnees inventees, aucun Gmail ni SQL.
<TestClass>
Public Class PdfTextExtractionIntegrationTests
    Private Shared Sub WriteSyntheticPdf(destination As String)
        ' PDF 1.4 textuel, construit sans service externe ni fichier de production.
        Dim streamText = String.Join(vbLf, New String() {
            "BT",
            "/F1 14 Tf",
            "50 740 Td",
            "(FACTURE FICTIVE DEMONSTRATION) Tj",
            "0 -24 Td",
            "(Total TTC : 42,50 EUR) Tj",
            "0 -24 Td",
            "(Date facture : 12/10/2026) Tj",
            "ET",
            ""
        })
        Dim objects As String() = {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            "<< /Length " & Encoding.ASCII.GetByteCount(streamText).ToString(CultureInfo.InvariantCulture) &
                " >>" & vbLf & "stream" & vbLf & streamText & "endstream"
        }
        Dim output As New StringBuilder("%PDF-1.4" & vbLf)
        Dim offsets As New List(Of Integer) From {0}
        For index = 0 To objects.Length - 1
            offsets.Add(Encoding.ASCII.GetByteCount(output.ToString()))
            output.Append((index + 1).ToString(CultureInfo.InvariantCulture)).
                Append(" 0 obj").Append(vbLf).Append(objects(index)).
                Append(vbLf).Append("endobj").Append(vbLf)
        Next

        Dim xrefStart = Encoding.ASCII.GetByteCount(output.ToString())
        output.Append("xref").Append(vbLf).
            Append("0 ").Append((objects.Length + 1).ToString(CultureInfo.InvariantCulture)).
            Append(vbLf).Append("0000000000 65535 f ").Append(vbLf)
        For index = 1 To objects.Length
            output.Append(offsets(index).ToString("D10", CultureInfo.InvariantCulture)).
                Append(" 00000 n ").Append(vbLf)
        Next
        output.Append("trailer").Append(vbLf).
            Append("<< /Size ").Append((objects.Length + 1).ToString(CultureInfo.InvariantCulture)).
            Append(" /Root 1 0 R >>").Append(vbLf).
            Append("startxref").Append(vbLf).
            Append(xrefStart.ToString(CultureInfo.InvariantCulture)).
            Append(vbLf).Append("%%EOF").Append(vbLf)
        File.WriteAllBytes(destination, Encoding.ASCII.GetBytes(output.ToString()))
    End Sub

    <TestMethod>
    Public Async Function PdfFictifReel_ExtractionEtPropositionSansValidation() As Task
        Dim root = Path.Combine(Path.GetTempPath(), "pdf-handoff-fictif-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(root, "import")
        Directory.CreateDirectory(imported)
        Try
            Dim pdfPath = Path.Combine(imported, "facture_fictive.pdf")
            WriteSyntheticPdf(pdfPath)
            Dim original = File.ReadAllBytes(pdfPath)
            Dim engine As New LocalPdfTextExtractionEngine()
            Dim extraction = JObject.Parse(Await engine.ExtractAsync(pdfPath))
            Dim texte = CStr(extraction("texte"))
            StringAssert.Contains(texte, "FACTURE FICTIVE")
            StringAssert.Contains(texte, "42,50")
            StringAssert.Contains(texte, "12/10/2026")
            Assert.AreEqual("Texte_a_verifier", CStr(extraction("statut_extraction")))

            Dim first = Await EmailProcessor.ProposeSavedAttachmentsAsync(
                imported, New String() {pdfPath}, engine, enableLocalInvoiceRules:=True)
            Assert.AreEqual(1, first.Created, String.Join(" ; ", first.Errors))
            Assert.AreEqual(0, first.Errors.Count)
            Dim queue = Path.Combine(imported, "_PropositionsAValider")
            Dim proposals = Directory.GetFiles(queue, "*.json")
            Assert.AreEqual(1, proposals.Length)
            Dim proposal = JObject.Parse(File.ReadAllText(proposals(0)))
            Assert.AreEqual("EnAttenteValidation", CStr(proposal("statut")))
            Assert.AreEqual("regles-facture-local/pdf-text-local", CStr(proposal("moteur")))
            Assert.AreEqual("Texte_a_verifier", CStr(proposal("extraction")("statut_extraction")))
            Assert.IsNotNull(proposal("extraction")("champs_proposes"))
            Assert.AreEqual(0, Directory.GetFiles(queue, "*.review.json").Length)
            CollectionAssert.AreEqual(original, File.ReadAllBytes(pdfPath))

            Dim second = Await EmailProcessor.ProposeSavedAttachmentsAsync(
                imported, New String() {pdfPath}, engine, enableLocalInvoiceRules:=True)
            Assert.AreEqual(0, second.Created)
            Assert.AreEqual(1, second.AlreadyPresent)
            Assert.AreEqual(1, Directory.GetFiles(queue, "*.json").Length)
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Function
End Class
