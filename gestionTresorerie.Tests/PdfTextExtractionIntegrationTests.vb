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
    Private Shared Sub WriteSyntheticPdf(destination As String, Optional extraLines As Integer = 0,
                                         Optional conflictingTotal As Boolean = False)
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
        If conflictingTotal Then
            ' Ajouter un second total contradictoire avant la date, dans un PDF valide.
            streamText = streamText.Replace("(Date facture : 12/10/2026) Tj",
                "(Montant TTC : 99,99 EUR) Tj" & vbLf & "0 -24 Td" & vbLf &
                "(Date facture : 12/10/2026) Tj")
        End If
        If extraLines > 0 Then
            Dim filler As New StringBuilder()
            For index = 1 To extraLines
                filler.Append("0 -1 Td").Append(vbLf).
                    Append("(").Append(New String("F"c, 80)).Append(") Tj").Append(vbLf)
            Next
            streamText = streamText.Replace("ET" & vbLf, filler.ToString() & "ET" & vbLf)
        End If
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
            Assert.IsNotNull(proposal("extraction")("champs_proposes")("montant_ttc"),
                             "TTC absent du PDF fictif. Texte extrait : " & texte)
            Assert.AreEqual(42.5D, proposal("extraction")("champs_proposes")("montant_ttc").Value(Of Decimal)())
            Assert.AreEqual("2026-10-12", proposal("extraction")("champs_proposes")("date_document").ToString())
            Assert.AreEqual("Champs_a_verifier", proposal("extraction")("statut_champs").ToString())
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
    <TestMethod>
    Public Async Function PdfFictifLong_ReelleTroncatureSansChampsProposes() As Task
        Dim root = Path.Combine(Path.GetTempPath(), "pdf-troncature-fictive-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(root, "import")
        Directory.CreateDirectory(imported)
        Try
            Dim pdfPath = Path.Combine(imported, "facture_longue_fictive.pdf")
            ' Plus de 16 000 caracteres effectivement extraits d'un PDF valide.
            WriteSyntheticPdf(pdfPath, extraLines:=450)
            Dim original = File.ReadAllBytes(pdfPath)
            Dim engine As New LocalPdfTextExtractionEngine()
            Dim extraction = JObject.Parse(Await engine.ExtractAsync(pdfPath))
            Assert.AreEqual("Texte_tronque_a_verifier", CStr(extraction("statut_extraction")))
            Assert.AreEqual(16000, CStr(extraction("texte")).Length)

            Dim report = Await EmailProcessor.ProposeSavedAttachmentsAsync(
                imported, New String() {pdfPath}, engine, enableLocalInvoiceRules:=True)
            Assert.AreEqual(1, report.Created, String.Join(" ; ", report.Errors))
            Assert.AreEqual(0, report.Errors.Count)
            Dim queue = Path.Combine(imported, "_PropositionsAValider")
            Dim proposals = Directory.GetFiles(queue, "*.json")
            Assert.AreEqual(1, proposals.Length)
            Dim proposal = JObject.Parse(File.ReadAllText(proposals(0)))
            Assert.AreEqual("EnAttenteValidation", CStr(proposal("statut")))
            Assert.AreEqual("Texte_tronque_a_verifier",
                            CStr(proposal("extraction")("statut_extraction")))
            Assert.AreEqual("Extraction_incomplete_a_verifier",
                            CStr(proposal("extraction")("statut_champs")))
            Assert.IsFalse(CType(proposal("extraction")("champs_proposes"), JObject).HasValues)
            Assert.AreEqual(0, Directory.GetFiles(queue, "*.review.json").Length)
            CollectionAssert.AreEqual(original, File.ReadAllBytes(pdfPath))
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Function


    <TestMethod>
    Public Async Function PdfFictifTotauxContradictoires_AucuneValeurTtcArbitree() As Task
        Dim root = Path.Combine(Path.GetTempPath(), "pdf-totaux-contradictoires-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(root, "import")
        Directory.CreateDirectory(imported)
        Try
            Dim pdfPath = Path.Combine(imported, "facture_contradictoire_fictive.pdf")
            WriteSyntheticPdf(pdfPath, conflictingTotal:=True)
            Dim original = File.ReadAllBytes(pdfPath)
            Dim engine As New LocalPdfTextExtractionEngine()
            Dim extracted = JObject.Parse(Await engine.ExtractAsync(pdfPath))
            StringAssert.Contains(CStr(extracted("texte")), "Total TTC : 42,50 EUR")
            StringAssert.Contains(CStr(extracted("texte")), "Montant TTC : 99,99 EUR")

            Dim report = Await EmailProcessor.ProposeSavedAttachmentsAsync(
                imported, New String() {pdfPath}, engine, enableLocalInvoiceRules:=True)
            Assert.AreEqual(1, report.Created, String.Join(" ; ", report.Errors))
            Assert.AreEqual(0, report.Errors.Count)
            Dim queue = Path.Combine(imported, "_PropositionsAValider")
            Dim proposals = Directory.GetFiles(queue, "*.json")
            Assert.AreEqual(1, proposals.Length)
            Dim proposal = JObject.Parse(File.ReadAllText(proposals(0)))
            Assert.AreEqual("EnAttenteValidation", CStr(proposal("statut")))
            Assert.AreEqual("Ambigu_a_verifier", CStr(proposal("extraction")("statut_champs")))
            Assert.IsNull(proposal("extraction")("champs_proposes")("montant_ttc"))
            Assert.AreEqual("2026-10-12",
                            CStr(proposal("extraction")("champs_proposes")("date_document")))
            Assert.AreEqual(0, Directory.GetFiles(queue, "*.review.json").Length)
            CollectionAssert.AreEqual(original, File.ReadAllBytes(pdfPath))
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Function

End Class
