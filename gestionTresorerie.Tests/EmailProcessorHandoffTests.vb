Imports System
Imports System.IO
Imports System.Threading.Tasks
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

' Tests hors ligne : aucun client IMAP, aucun vrai message et aucune base comptable.
<TestClass>
Public Class EmailProcessorHandoffTests
    Private NotInheritable Class FakeEngine
        Implements IDocumentExtractionEngine

        Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
            Get
                Return "moteur-fictif"
            End Get
        End Property

        Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
            Return Task.FromResult("{""type_document"":""Facture fictive"",""montant"":42.5}")
        End Function
    End Class

    <TestMethod>
    Public Async Function RepriseHorsLigneCreeUniquementDesPropositionsEnAttente() As Task
        Dim root = Path.Combine(Path.GetTempPath(), "email-handoff-fictif-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(root, "import")
        Directory.CreateDirectory(imported)
        Try
            Dim selected = Path.Combine(imported, "facture.pdf")
            Dim other = Path.Combine(imported, "autre.pdf")
            File.WriteAllText(selected, "Document totalement fictif")
            File.WriteAllText(other, "Autre document fictif")
            Dim first = Await EmailProcessor.ProposeSavedAttachmentsAsync(imported, New String() {selected}, New FakeEngine())
            Assert.AreEqual(1, first.Created)
            Assert.AreEqual(0, first.Errors.Count)
            Dim queue = Path.Combine(imported, "_PropositionsAValider")
            Dim proposals = Directory.GetFiles(queue, "*.json")
            Assert.AreEqual(1, proposals.Length)
            Dim proposal = JObject.Parse(File.ReadAllText(proposals(0)))
            Assert.AreEqual("EnAttenteValidation", CStr(proposal("statut")))
            Assert.AreEqual("moteur-fictif", CStr(proposal("moteur")))
            Assert.AreEqual("Facture fictive", CStr(proposal("extraction")("type_document")))
            Assert.AreEqual(0, Directory.GetFiles(queue, "*.review.json").Length)
            Assert.AreEqual("Document totalement fictif", File.ReadAllText(selected))
            Assert.AreEqual("Autre document fictif", File.ReadAllText(other))

            Dim second = Await EmailProcessor.ProposeSavedAttachmentsAsync(imported, New String() {selected}, New FakeEngine())
            Assert.AreEqual(0, second.Created)
            Assert.AreEqual(1, second.AlreadyPresent)
            Assert.AreEqual(1, Directory.GetFiles(queue, "*.json").Length)
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Function

    <TestMethod>
    Public Async Function RepriseHorsLigneRefuseUnCheminExterieur() As Task
        Dim root = Path.Combine(Path.GetTempPath(), "email-handoff-externe-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(root, "import")
        Directory.CreateDirectory(imported)
        Try
            Dim outside = Path.Combine(root, "hors-import.pdf")
            File.WriteAllText(outside, "Document fictif externe")
            Dim result = Await EmailProcessor.ProposeSavedAttachmentsAsync(imported, New String() {outside}, New FakeEngine())
            Assert.AreEqual(0, result.Created)
            Assert.AreEqual(1, result.Errors.Count)
            StringAssert.Contains(result.Errors(0), "hors dossier d'import")
            Assert.IsFalse(Directory.Exists(Path.Combine(imported, "_PropositionsAValider")))
            Assert.AreEqual("Document fictif externe", File.ReadAllText(outside))
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Function
End Class
