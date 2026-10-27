Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

' Parcours entierement hors ligne : aucune connexion Gmail, aucun appel IA, aucune base SQL.
<TestClass>
Public Class ImportedAttachmentReviewFlowTests
    Private Class FakeExtractionEngine
        Implements IDocumentExtractionEngine

        Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
            Get
                Return "moteur-factice"
            End Get
        End Property

        Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
            Return Task.FromResult("{""type_document"":""Facture"",""montant"":12.5}")
        End Function
    End Class

    <TestMethod>
    Public Async Function PieceJointeFictiveDevientPropositionPuisRevueExplicite() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "gmail-review-fictif-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim contenu = "Document entierement fictif - aucun renseignement comptable reel"
            Dim temporary = Path.Combine(folder, ".gmail-fictif.tmp")
            File.WriteAllText(temporary, contenu)

            ' Simule le stockage d'une PJ deja decodee, sans ouvrir de boite Gmail.
            Dim storedPath = AttachmentStorage.StoreTemporaryFile(temporary, Path.Combine(folder, "facture-factice.pdf"))
            Assert.IsNotNull(storedPath)
            Assert.IsFalse(File.Exists(temporary))

            Dim queue = Path.Combine(folder, "_PropositionsAValider")
            Dim workflow As New DocumentProposalWorkflow(New FakeExtractionEngine(), New DocumentProposalStore(queue))
            Assert.IsTrue(Await workflow.ProposeAsync(storedPath))
            Assert.IsFalse(Await workflow.ProposeAsync(storedPath), "La reprise ne doit pas creer de doublon.")

            Dim sha = AttachmentStorage.ComputeSha256(storedPath)
            Dim proposalPath = Path.Combine(queue, sha & ".json")
            Dim proposalBefore = File.ReadAllText(proposalPath)
            Dim proposal = JObject.Parse(proposalBefore)
            Assert.AreEqual("EnAttenteValidation", proposal("statut").ToString())
            Assert.AreEqual("moteur-factice", proposal("moteur").ToString())
            Assert.AreEqual("Facture", proposal("extraction")("type_document").ToString())

            Dim review As New DocumentReviewStore(queue)
            CollectionAssert.AreEqual(New String() {sha}, review.ListPending().ToArray())
            Assert.IsFalse(File.Exists(Path.Combine(queue, sha & ".review.json")),
                           "Aucune validation ne doit etre automatique.")

            ' L'utilisateur corrige et confirme explicitement, sans creation d'operation SQL.
            Dim corrected As New JObject From {{"montant", 12.5D}, {"tiers", "Fournisseur fictif"}}
            Assert.IsTrue(review.RecordReview(sha, "validateur-fictif",
                                              DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Assert.AreEqual(0, review.ListPending().Count)
            Assert.IsFalse(review.RecordReview(sha, "autre-validateur",
                                               DocumentReviewStore.ReviewDecision.Rejete, Nothing))

            Dim receipt = JObject.Parse(File.ReadAllText(Path.Combine(queue, sha & ".review.json")))
            Assert.AreEqual("ApprouvePourSaisie", receipt("decision").ToString())
            Assert.AreEqual("Fournisseur fictif", receipt("donnees_corrigees")("tiers").ToString())
            Assert.AreEqual(proposalBefore, File.ReadAllText(proposalPath))
            Assert.AreEqual(contenu, File.ReadAllText(storedPath))
            Assert.AreEqual(2, Directory.GetFiles(queue, "*.json").Length)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Function
End Class
