Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Aucun acces Gmail, aucune donnee comptable, aucun appel IA.
<TestClass>
Public Class ImportedAttachmentProposalScannerTests
    Private NotInheritable Class FakeEngine
        Implements IDocumentExtractionEngine

        Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
            Get
                Return "extraction-factice"
            End Get
        End Property

        Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
            If Path.GetFileName(documentPath).StartsWith("erreur", StringComparison.Ordinal) Then
                Throw New InvalidDataException("Erreur de test")
            End If
            Return Task.FromResult("{""type_document"":""Facture"",""montant"":12.5}")
        End Function
    End Class

    <TestMethod>
    Public Async Function DocumentsImportesSontProposesSansDoublonEtSansApprobation() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "scanner-fictif-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(folder, "import")
        Dim month = Path.Combine(imported, "2026", "10")
        Directory.CreateDirectory(month)
        Try
            Dim invoice = Path.Combine(month, "facture.pdf")
            Dim scan = Path.Combine(month, "scan.jpg")
            File.WriteAllText(invoice, "Facture fictive")
            File.WriteAllText(scan, "Scan fictif")
            File.WriteAllText(Path.Combine(month, "notes.txt"), "Ne pas traiter")
            Dim queue = Path.Combine(imported, "_PropositionsAValider")
            Dim scanner As New ImportedAttachmentProposalScanner(New FakeEngine(), queue)

            Dim first = Await scanner.ScanAsync(imported)
            Assert.AreEqual(2, first.Created)
            Assert.AreEqual(0, first.AlreadyPresent)
            Assert.AreEqual(0, first.Errors.Count)
            Assert.AreEqual(2, Directory.GetFiles(queue, "*.json").Length)
            Assert.AreEqual(0, Directory.GetFiles(queue, "*.review.json").Length)

            Dim second = Await scanner.ScanAsync(imported)
            Assert.AreEqual(0, second.Created)
            Assert.AreEqual(2, second.AlreadyPresent)
            Assert.AreEqual(0, second.Errors.Count)
            Assert.AreEqual("Facture fictive", File.ReadAllText(invoice))
            Assert.AreEqual("Scan fictif", File.ReadAllText(scan))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Function

    <TestMethod>
    Public Async Function ErreurExtractionEstSignaleeSansBloquerLesAutresDocuments() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "scanner-erreur-fictif-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            File.WriteAllText(Path.Combine(folder, "erreur.pdf"), "Erreur simulee")
            File.WriteAllText(Path.Combine(folder, "valide.png"), "Document fictif valide")
            Dim queue = Path.Combine(folder, "_PropositionsAValider")
            Dim scanner As New ImportedAttachmentProposalScanner(New FakeEngine(), queue)

            Dim result = Await scanner.ScanAsync(folder)
            Assert.AreEqual(1, result.Created)
            Assert.AreEqual(1, result.Errors.Count)
            StringAssert.Contains(result.Errors(0), "erreur.pdf")
            Assert.AreEqual(1, Directory.GetFiles(queue, "*.json").Length)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Function
End Class
