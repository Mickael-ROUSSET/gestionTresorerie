Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

<TestClass>
Public Class DocumentProposalWorkflowTests
    Private Class FakeEngine
        Implements IDocumentExtractionEngine

        Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
            Get
                Return "fake-sans-reseau"
            End Get
        End Property

        Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
            Return Task.FromResult("{""type_document"":""Facture"",""montant"":12.5}")
        End Function
    End Class

    <TestMethod>
    Public Async Function ExtractionProduitPropositionSansModifierLaSource() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "proposal-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim source = Path.Combine(folder, "facture.pdf")
            File.WriteAllText(source, "document factice")
            Dim queue = Path.Combine(folder, "a_valider")
            Dim workflow As New DocumentProposalWorkflow(New FakeEngine(), New DocumentProposalStore(queue))

            Assert.IsTrue(Await workflow.ProposeAsync(source))
            Assert.IsFalse(Await workflow.ProposeAsync(source), "Le meme contenu ne doit pas creer un doublon.")
            Assert.AreEqual("document factice", File.ReadAllText(source), "Ne jamais renommer ou modifier la source.")

            Dim files = Directory.GetFiles(queue, "*.json")
            Assert.AreEqual(1, files.Length)
            Dim proposal = JObject.Parse(File.ReadAllText(files(0)))
            Assert.AreEqual("EnAttenteValidation", proposal("statut").ToString())
            Assert.AreEqual("fake-sans-reseau", proposal("moteur").ToString())
            Assert.AreEqual("Facture", proposal("extraction")("type_document").ToString())
            Assert.AreEqual(AttachmentStorage.ComputeSha256(source), proposal("source_sha256").ToString())
        Finally
            Directory.Delete(folder, True)
        End Try
    End Function

    <TestMethod>
    Public Sub RejetJsonInvalideNeCreePasDeProposition()
        Dim folder = Path.Combine(Path.GetTempPath(), "proposal-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim source = Path.Combine(folder, "scan.jpg")
            File.WriteAllText(source, "contenu factice")
            Dim queue = Path.Combine(folder, "a_valider")
            Assert.ThrowsExactly(Of Newtonsoft.Json.JsonReaderException)(
                Sub() New DocumentProposalStore(queue).SavePending(source, "test", "pas du JSON"))
            Assert.IsFalse(Directory.Exists(queue))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Async Function ImageLocaleResteAExaminerSansServiceExterne() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "proposal-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim source = Path.Combine(folder, "scan.jpg")
            File.WriteAllBytes(source, New Byte() {1, 2, 3})
            Dim engine As New LocalPdfTextExtractionEngine()
            Dim extracted = JObject.Parse(Await engine.ExtractAsync(source))
            Assert.AreEqual("OCR_ou_saisie_manuelle_requis", extracted("statut_extraction").ToString())
            Assert.AreEqual("A_verifier", extracted("type_document").ToString())
        Finally
            Directory.Delete(folder, True)
        End Try
    End Function
End Class
