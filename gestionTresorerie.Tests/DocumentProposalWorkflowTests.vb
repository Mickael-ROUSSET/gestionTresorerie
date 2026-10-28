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

    ' Simule une modification concurrente du fichier pendant l'extraction, sans reseau.
    Private Class EngineQuiModifieLaSource
        Implements IDocumentExtractionEngine

        Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
            Get
                Return "fake-modification"
            End Get
        End Property

        Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
            File.AppendAllText(documentPath, " MODIFIE")
            Return Task.FromResult("{""montant"":12.5}")
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
    Public Async Function ModificationPendantExtractionRefuseLaProposition() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "proposal-race-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim source = Path.Combine(folder, "facture-factice.pdf")
            File.WriteAllText(source, "DOCUMENT FICTIF")
            Dim queue = Path.Combine(folder, "a_valider")
            Dim workflow As New DocumentProposalWorkflow(New EngineQuiModifieLaSource(), New DocumentProposalStore(queue))
            Dim refused As Boolean = False
            Try
                Await workflow.ProposeAsync(source)
            Catch ex As InvalidDataException
                refused = True
            End Try

            Assert.IsTrue(refused, "Une extraction ne doit pas etre associee a un fichier modifie.")
            Assert.IsFalse(Directory.Exists(queue), "Aucune proposition ne doit etre publiee.")
            Assert.AreEqual("DOCUMENT FICTIF MODIFIE", File.ReadAllText(source))
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
            Dim store As New DocumentProposalStore(queue)
            Assert.ThrowsExactly(Of Newtonsoft.Json.JsonReaderException)(
                Sub() store.SavePending(source, "test", "pas du JSON"))
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

    <TestMethod>
    Public Sub PublicationsConcurrentesNeCreentQuUnePropositionComplete()
        Dim folder = Path.Combine(Path.GetTempPath(), "proposal-concurrent-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim source = Path.Combine(folder, "facture-factice.pdf")
            File.WriteAllText(source, "Contenu fictif, sans information reelle")
            Dim queue = Path.Combine(folder, "a_valider")
            Dim store As New DocumentProposalStore(queue)
            Dim outcomes As New System.Collections.Concurrent.ConcurrentBag(Of Boolean)()

            Parallel.For(0, 20, Sub(i)
                                    outcomes.Add(store.SavePending(source, "moteur-factice",
                                        "{""montant"":12.5,""tiers"":""fictif""}"))
                                End Sub)

            Assert.AreEqual(1, outcomes.Where(Function(created) created).Count())
            Assert.AreEqual(19, outcomes.Where(Function(created) Not created).Count())
            Dim published = Directory.GetFiles(queue, "*.json")
            Assert.AreEqual(1, published.Length)
            Dim proposal = JObject.Parse(File.ReadAllText(published(0)))
            Assert.AreEqual("EnAttenteValidation", proposal("statut").ToString())
            Assert.AreEqual(AttachmentStorage.ComputeSha256(source), proposal("source_sha256").ToString())
            Assert.AreEqual(0, Directory.GetFiles(queue, ".proposal-tmp-*").Length)
            Assert.AreEqual("Contenu fictif, sans information reelle", File.ReadAllText(source))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub PropositionExistanteCorrompueBloqueLaRepriseSansEcrasement()
        Dim folder = Path.Combine(Path.GetTempPath(), "proposal-corrupt-fictif-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim source = Path.Combine(folder, "facture-fictive.pdf")
            File.WriteAllText(source, "Document entierement fictif")
            Dim queue = Path.Combine(folder, "a_valider")
            Dim store As New DocumentProposalStore(queue)
            Dim extracted = "{""type_document"":""Facture fictive""}"
            Assert.IsTrue(store.SavePending(source, "moteur-fictif", extracted))
            Dim sha = AttachmentStorage.ComputeSha256(source)
            Dim proposalPath = Path.Combine(queue, sha & ".json")
            Dim original = File.ReadAllText(proposalPath)

            ' Une proposition tronquee ne doit pas etre consideree comme un doublon sain.
            File.WriteAllText(proposalPath, "{json-tronque")
            Dim corrupted = File.ReadAllText(proposalPath)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.SavePending(source, "moteur-fictif", extracted))
            Assert.AreEqual(corrupted, File.ReadAllText(proposalPath))

            ' Un JSON syntaxiquement valide avec une autre empreinte est egalement refuse.
            Dim altered = JObject.Parse(original)
            altered("source_sha256") = New String("0"c, 64)
            File.WriteAllText(proposalPath, altered.ToString())
            Dim mismatched = File.ReadAllText(proposalPath)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.SavePending(source, "moteur-fictif", extracted))
            Assert.AreEqual(mismatched, File.ReadAllText(proposalPath))

            ' Une proposition intacte reste idempotente, sans modification du document.
            File.WriteAllText(proposalPath, original)
            Assert.IsFalse(store.SavePending(source, "moteur-fictif", extracted))
            Assert.AreEqual(original, File.ReadAllText(proposalPath))
            Assert.AreEqual("Document entierement fictif", File.ReadAllText(source))
            Assert.AreEqual(0, Directory.GetFiles(queue, ".proposal-tmp-*").Length)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

End Class
