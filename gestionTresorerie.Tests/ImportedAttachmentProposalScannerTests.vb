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

    <TestMethod>
    Public Async Function CheminsImportesExplicitesNeParcourentPasLesAutresDocuments() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "handoff-fictif-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(folder, "import")
        Dim outsideFolder = Path.Combine(folder, "import-autre")
        Dim queue = Path.Combine(imported, "_PropositionsAValider")
        Directory.CreateDirectory(imported)
        Directory.CreateDirectory(outsideFolder)
        Directory.CreateDirectory(queue)
        Try
            Dim selected = Path.Combine(imported, "selection.pdf")
            Dim unselected = Path.Combine(imported, "non-selection.png")
            Dim outside = Path.Combine(outsideFolder, "externe.pdf")
            Dim queueDocument = Path.Combine(queue, "archive.pdf")
            Dim unsupported = Path.Combine(imported, "notes.txt")
            File.WriteAllText(selected, "Facture fictive selectionnee")
            File.WriteAllText(unselected, "Image fictive non selectionnee")
            File.WriteAllText(outside, "Document externe fictif")
            File.WriteAllText(queueDocument, "Archive de revue fictive")
            File.WriteAllText(unsupported, "Notes fictives")
            Dim scanner As New ImportedAttachmentProposalScanner(New FakeEngine(), queue)

            Dim first = Await scanner.ScanSavedPathsAsync(imported,
                New String() {selected, selected, outside, queueDocument, unsupported})
            Assert.AreEqual(1, first.Created)
            Assert.AreEqual(0, first.AlreadyPresent)
            Assert.AreEqual(1, first.Errors.Count)
            StringAssert.Contains(first.Errors(0), "hors dossier d'import")
            Assert.AreEqual(1, Directory.GetFiles(queue, "*.json").Length)
            Assert.AreEqual(0, Directory.GetFiles(queue, "*.review.json").Length)
            Assert.AreEqual("Document externe fictif", File.ReadAllText(outside))
            Assert.AreEqual("Image fictive non selectionnee", File.ReadAllText(unselected))

            Dim second = Await scanner.ScanSavedPathsAsync(imported, New String() {selected})
            Assert.AreEqual(0, second.Created)
            Assert.AreEqual(1, second.AlreadyPresent)
            Assert.AreEqual(0, second.Errors.Count)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Function

    <TestMethod>
    Public Async Function CheminsRelatifsOuVidesSontRefusesSansProposition() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "handoff-invalid-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            File.WriteAllText(Path.Combine(folder, "facture.pdf"), "Facture fictive")
            Dim queue = Path.Combine(folder, "_PropositionsAValider")
            Dim scanner As New ImportedAttachmentProposalScanner(New FakeEngine(), queue)

            Dim result = Await scanner.ScanSavedPathsAsync(folder, New String() {Nothing, "facture.pdf"})
            Assert.AreEqual(0, result.Created)
            Assert.AreEqual(2, result.Errors.Count)
            Assert.IsFalse(Directory.Exists(queue))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Function
End Class
