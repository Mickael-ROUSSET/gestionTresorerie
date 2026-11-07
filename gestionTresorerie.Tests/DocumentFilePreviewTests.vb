Option Strict On

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentFilePreviewTests
    <TestMethod>
    Public Sub LitLeMemePdfDepuisUnDossierLocalOuLeStockageGmail()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim local = Directory.CreateDirectory(Path.Combine(root, "Documents locaux")).FullName
            Dim mail = Directory.CreateDirectory(Path.Combine(root, "Gmail", "2026", "11")).FullName
            Dim fichier = Path.Combine(local, "facture.PDF")
            EcrirePdf(fichier, "BT /F1 12 Tf 15 80 Td (Facture TEST) Tj ET")

            Dim temporaire = Path.Combine(mail, "piece.tmp")
            File.Copy(fichier, temporaire)
            Dim enregistre = AttachmentStorage.StoreTemporaryFile(temporaire, Path.Combine(mail, "facture.pdf"))

            Assert.IsTrue(DocumentFilePreview.Supports(fichier))
            Assert.IsFalse(DocumentFilePreview.Supports("rapport.txt"))
            Dim apercu = DocumentFilePreview.ReadPdfText(fichier)
            StringAssert.Contains(apercu, "Facture TEST")
            Assert.AreEqual(apercu, DocumentFilePreview.ReadPdfText(enregistre))
            CollectionAssert.AreEqual(File.ReadAllBytes(fichier), File.ReadAllBytes(enregistre))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub SignaleUnPdfSansTexteEtLibereLeFichierImage()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim pdf = Path.Combine(root, "sans-texte.pdf")
            EcrirePdf(pdf, "q Q")
            StringAssert.Contains(DocumentFilePreview.ReadPdfText(pdf), "OCR")

            Dim scan = Path.Combine(root, "scan.png")
            Using image As New Bitmap(5, 7)
                image.Save(scan, Imaging.ImageFormat.Png)
            End Using
            Using apercu = DocumentFilePreview.ReadImage(scan)
                File.Delete(scan)
                Assert.IsFalse(File.Exists(scan))
                Assert.AreEqual(7, apercu.Height)
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DemoOuTestIgnoreGmailAvantDeDemanderLaDate()
        Dim original = EnvironmentManager.CurrentEnvironment
        Try
            EnvironmentManager.CurrentEnvironment = EnvironmentManager.EnvironnementType.TEST
            Dim fichiers = EmailProcessor.DownloadAttachments(requestStartDate:=
                Function()
                    Throw New InvalidOperationException("Le téléchargement Gmail doit rester inaccessible en TEST.")
                End Function)
            Assert.AreEqual(0, fichiers.Count)
        Finally
            EnvironmentManager.CurrentEnvironment = original
        End Try
    End Sub

    <TestMethod>
    Public Sub AfficheUnePageSansTexteEtLibereLePdf()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim pdf = Path.Combine(root, "scan.pdf")
            EcrirePdf(pdf, "0 g 0 0 200 100 re f")
            Assert.AreEqual(1, DocumentFilePreview.PdfPageCount(pdf))
            Using bitmap = DocumentFilePreview.RenderPdfPage(pdf, 0)
                Assert.AreEqual(1800, bitmap.Width)
                Assert.AreEqual(900, bitmap.Height)
                Assert.AreEqual(Color.Black.ToArgb(), bitmap.GetPixel(bitmap.Width \ 2, bitmap.Height \ 2).ToArgb())
                File.Delete(pdf)
                Assert.IsFalse(File.Exists(pdf))
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AnalyseLesChampsDUnVraiPdfSansFichierJson()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim pdf = Path.Combine(root, "facture.pdf")
            EcrirePdf(pdf, "BT /F1 8 Tf 11 TL 10 80 Td (Fournisseur: Exemple) Tj T* " &
                "(Numero de facture: F-001) Tj T* (Date facture: 01/04/2026) Tj T* (Total TTC: 42,50 EUR) Tj ET")
            Dim data = LocalPdfInvoiceAnalysis.Extract(pdf)("donnees_extraites")
            Dim transaction = DirectCast(data("transaction"), Newtonsoft.Json.Linq.JObject)
            Assert.AreEqual(42.5D, transaction.Value(Of Decimal)("montant_ttc"))
            Assert.AreEqual("2026-04-01", transaction.Value(Of String)("date_facture"))
            Assert.AreEqual("F-001", transaction.Value(Of String)("numero_facture"))
            Assert.AreEqual("Exemple", data("identite")("fournisseur_nom").ToString())
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    Private Shared Sub EcrirePdf(filePath As String, pageContent As String)
        Dim objets As String() = {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 100] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {pageContent.Length} >>" & vbLf & "stream" & vbLf & pageContent & vbLf & "endstream"
        }
        Dim pdf As New StringBuilder("%PDF-1.4" & vbLf)
        Dim offsets As New List(Of Integer)
        For index = 0 To objets.Length - 1
            offsets.Add(pdf.Length)
            pdf.AppendLine($"{index + 1} 0 obj").AppendLine(objets(index)).AppendLine("endobj")
        Next
        Dim debutIndex = pdf.Length
        pdf.Append("xref").Append(vbLf).Append($"0 {objets.Length + 1}").Append(vbLf)
        pdf.Append("0000000000 65535 f ").Append(vbLf)
        For Each offset In offsets
            pdf.Append($"{offset:0000000000} 00000 n ").Append(vbLf)
        Next
        pdf.AppendLine("trailer").AppendLine($"<< /Size {objets.Length + 1} /Root 1 0 R >>")
        pdf.AppendLine("startxref").AppendLine(debutIndex.ToString()).AppendLine("%%EOF")
        File.WriteAllText(filePath, pdf.ToString(), Encoding.ASCII)
    End Sub
End Class
