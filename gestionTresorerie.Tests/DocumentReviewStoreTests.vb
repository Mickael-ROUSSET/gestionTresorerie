Imports System
Imports System.IO
Imports System.Linq
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

<TestClass>
Public Class DocumentReviewStoreTests
    Private Shared Function CreateFixture(folder As String) As (Source As String, Queue As String, Sha As String)
        Directory.CreateDirectory(folder)
        Dim source = Path.Combine(folder, "facture-factice.pdf")
        File.WriteAllText(source, "Facture de test sans information reelle")
        Dim queue = Path.Combine(folder, "a_valider")
        Dim store As New DocumentProposalStore(queue)
        Assert.IsTrue(store.SavePending(source, "moteur-test", "{""montant"":12.5}"))
        Return (source, queue, AttachmentStorage.ComputeSha256(source))
    End Function

    <TestMethod>
    Public Sub ApprobationExpliciteCreeRecuSansModifierSourceNiProposition()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-test-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            Dim pendingBefore = File.ReadAllText(pendingPath)
            Dim store As New DocumentReviewStore(fixture.Queue)
            CollectionAssert.AreEqual(New String() {fixture.Sha}, store.ListPending().ToArray())

            Dim corrected As New JObject From {{"montant", 12.5}, {"tiers", "Fournisseur fictif"}}
            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-test", DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Assert.IsFalse(store.RecordReview(fixture.Sha, "autre-validateur", DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.AreEqual(0, store.ListPending().Count)

            Dim receipt = JObject.Parse(File.ReadAllText(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
            Assert.AreEqual("ApprouvePourSaisie", receipt("decision").ToString())
            Assert.AreEqual("validateur-test", receipt("validateur").ToString())
            Assert.AreEqual("Fournisseur fictif", receipt("donnees_corrigees")("tiers").ToString())
            Assert.AreEqual(pendingBefore, File.ReadAllText(pendingPath))
            Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
            Assert.AreEqual(2, Directory.GetFiles(fixture.Queue, "*.json").Length)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RejetNeNecessitePasDeDonneesCorrigees()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-test-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-test", DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Dim receipt = JObject.Parse(File.ReadAllText(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
            Assert.AreEqual("Rejete", receipt("decision").ToString())
            Assert.IsNull(receipt("donnees_corrigees"))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RevueRefuseIdentiteAbsenteDonneesVidesEtEmpreinteInvalide()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-test-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() store.RecordReview("../invalide", "test", DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() store.RecordReview(fixture.Sha, " ", DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() store.RecordReview(fixture.Sha, "test", DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, Nothing))
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
                Sub() store.RecordReview(fixture.Sha, "test", CType(999, DocumentReviewStore.ReviewDecision), Nothing))
            Assert.AreEqual(1, store.ListPending().Count)
            Assert.AreEqual(1, Directory.GetFiles(fixture.Queue, "*.json").Length)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub PropositionAltereNePeutPasEtreApprouvee()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-test-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            Dim proposal = JObject.Parse(File.ReadAllText(pendingPath))
            proposal("source_sha256") = New String("0"c, 64)
            File.WriteAllText(pendingPath, proposal.ToString())
            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.RecordReview(fixture.Sha, "test", DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ApprobationRefuseSourceModifieeMaisRejetRestePossible()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-test-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            File.AppendAllText(fixture.Source, " document modifie apres extraction")
            Dim store As New DocumentReviewStore(fixture.Queue)
            Dim corrected As New JObject From {{"montant", 12.5}}
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.RecordReview(fixture.Sha, "test", DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
            Assert.IsTrue(store.RecordReview(fixture.Sha, "test", DocumentReviewStore.ReviewDecision.Rejete, Nothing))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ApprobationRefuseSourceSupprimee()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-test-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            File.Delete(fixture.Source)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Dim corrected As New JObject From {{"montant", 12.5}}
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.RecordReview(fixture.Sha, "test", DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LectureEnAttenteExposeLesDonneesSansValiderEtRefuseUneRevueDejaFaite()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-read-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            Dim originalProposal = File.ReadAllText(pendingPath)
            Dim store As New DocumentReviewStore(fixture.Queue)

            Dim proposal = store.ReadPendingProposal(fixture.Sha.ToLowerInvariant())
            Assert.AreEqual("EnAttenteValidation", proposal("statut").ToString())
            Assert.AreEqual("moteur-test", proposal("moteur").ToString())
            Assert.IsNotNull(proposal("extraction")("montant"))
            Assert.AreEqual(originalProposal, File.ReadAllText(pendingPath))
            Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))

            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() store.ReadPendingProposal(fixture.Sha))
            Assert.AreEqual(originalProposal, File.ReadAllText(pendingPath))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LectureRefuseEmpreinteInvalidePropositionAbsenteOuAltere()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-read-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() store.ReadPendingProposal("../hors-dossier"))
            Assert.ThrowsExactly(Of FileNotFoundException)(
                Sub() store.ReadPendingProposal(New String("0"c, 64)))

            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            Dim proposal = JObject.Parse(File.ReadAllText(pendingPath))
            proposal("source_sha256") = New String("0"c, 64)
            File.WriteAllText(pendingPath, proposal.ToString())
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.ReadPendingProposal(fixture.Sha))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
            Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub


    <TestMethod>
    Public Sub LectureEtApprobationRefusentSchemaOuExtractionAlteres()
        For Each invalidField As String In New String() {"schema_version", "extraction"}
            Dim folder = Path.Combine(Path.GetTempPath(), "review-schema-" & Guid.NewGuid().ToString("N"))
            Try
                Dim fixture = CreateFixture(folder)
                Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
                Dim proposal = JObject.Parse(File.ReadAllText(pendingPath))
                If invalidField = "schema_version" Then
                    proposal("schema_version") = 2
                Else
                    proposal("extraction") = "contenu non structure"
                End If
                File.WriteAllText(pendingPath, proposal.ToString())

                Dim store As New DocumentReviewStore(fixture.Queue)
                Assert.ThrowsExactly(Of InvalidDataException)(
                    Sub() store.ReadPendingProposal(fixture.Sha))
                Dim corrected As New JObject From {{"montant", 12.5}}
                Assert.ThrowsExactly(Of InvalidDataException)(
                    Sub() store.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
                Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
                Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
            Finally
                If Directory.Exists(folder) Then Directory.Delete(folder, True)
            End Try
        Next
    End Sub

    <TestMethod>
    Public Sub JSONTronqueRefuseLectureEtDecisionSansRecu()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-json-incorrect-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            Dim malformed = "{""schema_version"":1,""extraction"":"
            File.WriteAllText(pendingPath, malformed)

            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.ReadPendingProposal(fixture.Sha))
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.RecordReview(fixture.Sha, "validateur-fictif",
                                         DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Dim corrected As New JObject From {{"montant", 12.5}}
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() store.RecordReview(fixture.Sha, "validateur-fictif",
                                         DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))

            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
            Assert.AreEqual(malformed, File.ReadAllText(pendingPath))
            Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub


    <TestMethod>
    Public Sub RevuePublieeResteIdempotenteSiLeDocumentSourceEstSupprime()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-replay-absent-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Dim corrected As New JObject From {{"montant", 12.5}}
            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Dim receiptPath = Path.Combine(fixture.Queue, fixture.Sha & ".review.json")
            Dim originalReceipt = File.ReadAllText(receiptPath)
            File.Delete(fixture.Source)

            Assert.IsFalse(store.RecordReview(fixture.Sha, "autre-validateur",
                                              DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Assert.AreEqual(originalReceipt, File.ReadAllText(receiptPath))
            Assert.AreEqual(0, store.ListPending().Count)
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RevuePublieeResteIdempotenteSiLaPropositionEstTronquee()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-replay-json-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Dim receiptPath = Path.Combine(fixture.Queue, fixture.Sha & ".review.json")
            Dim originalReceipt = File.ReadAllText(receiptPath)
            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            File.WriteAllText(pendingPath, "{")

            Assert.IsFalse(store.RecordReview(fixture.Sha, "autre-validateur",
                                              DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.AreEqual(originalReceipt, File.ReadAllText(receiptPath))
            Assert.AreEqual("{", File.ReadAllText(pendingPath))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CatalogueDeRevueExposeLesPropositionsValidesSansDecisionAutomatique()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-catalogue-fictif-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim otherSource = Path.Combine(folder, "seconde-facture-fictive.pdf")
            File.WriteAllText(otherSource, "Seconde facture entierement fictive")
            Dim proposals As New DocumentProposalStore(fixture.Queue)
            Assert.IsTrue(proposals.SavePending(otherSource, "moteur-test", "{""montant"":7.5}"))
            Dim otherSha = AttachmentStorage.ComputeSha256(otherSource)
            Dim review As New DocumentReviewStore(fixture.Queue)
            Dim originalFirst = File.ReadAllText(Path.Combine(fixture.Queue, fixture.Sha & ".json"))
            Dim originalSecond = File.ReadAllText(Path.Combine(fixture.Queue, otherSha & ".json"))

            Dim catalogue = review.ListPendingProposals()
            Assert.AreEqual(2, catalogue.Count)
            Dim expected = New String() {fixture.Sha, otherSha}.OrderBy(Function(sha) sha, StringComparer.Ordinal).ToArray()
            CollectionAssert.AreEqual(expected, catalogue.Select(Function(item) CStr(item("source_sha256"))).ToArray())
            Assert.IsTrue(catalogue.All(Function(item) CStr(item("statut")) = "EnAttenteValidation"))
            Assert.AreEqual(0, Directory.GetFiles(fixture.Queue, "*.review.json").Length)
            Assert.AreEqual(originalFirst, File.ReadAllText(Path.Combine(fixture.Queue, fixture.Sha & ".json")))
            Assert.AreEqual(originalSecond, File.ReadAllText(Path.Combine(fixture.Queue, otherSha & ".json")))

            Assert.IsTrue(review.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Dim remaining = review.ListPendingProposals()
            Assert.AreEqual(1, remaining.Count)
            Assert.AreEqual(otherSha, CStr(remaining(0)("source_sha256")))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, otherSha & ".review.json")))
            Assert.AreEqual("Seconde facture entierement fictive", File.ReadAllText(otherSource))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CatalogueDeRevueRefuseUnePropositionCorrompueSansCreerDeRecu()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-catalogue-invalide-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim pendingPath = Path.Combine(fixture.Queue, fixture.Sha & ".json")
            Dim corrupted = "{""schema_version"":"
            File.WriteAllText(pendingPath, corrupted)
            Dim review As New DocumentReviewStore(fixture.Queue)

            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() review.ListPendingProposals())
            Assert.AreEqual(corrupted, File.ReadAllText(pendingPath))
            Assert.IsFalse(File.Exists(Path.Combine(fixture.Queue, fixture.Sha & ".review.json")))
            Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RecuDeRevueAltereBloqueLeCatalogueEtLaRepriseSansEcrasement()
        Dim folder = Path.Combine(Path.GetTempPath(), "review-receipt-fictif-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Dim receiptPath = Path.Combine(fixture.Queue, fixture.Sha & ".review.json")
            Dim original = File.ReadAllText(receiptPath)
            Dim altered = JObject.Parse(original)
            altered("source_sha256") = New String("0"c, 64)

            For Each invalid In New String() {"{json-tronque", altered.ToString()}
                File.WriteAllText(receiptPath, invalid)
                Assert.ThrowsExactly(Of InvalidDataException)(Sub() store.ListPending())
                Assert.ThrowsExactly(Of InvalidDataException)(Sub() store.ListPendingProposals())
                Assert.ThrowsExactly(Of InvalidDataException)(
                    Sub() store.RecordReview(fixture.Sha, "autre-validateur",
                                             DocumentReviewStore.ReviewDecision.Rejete, Nothing))
                Assert.AreEqual(invalid, File.ReadAllText(receiptPath))
                Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
            Next

            File.WriteAllText(receiptPath, original)
            Assert.AreEqual(0, store.ListPending().Count)
            Assert.IsFalse(store.RecordReview(fixture.Sha, "autre-validateur",
                                              DocumentReviewStore.ReviewDecision.Rejete, Nothing))
            Assert.AreEqual(original, File.ReadAllText(receiptPath))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RecuApprouveSansDonneesCorrigeesValides_EstRefuseSansEcrasement()
        ' Facture fictive : un recu approuve doit conserver un objet de correction non vide.
        Dim folder = Path.Combine(Path.GetTempPath(), "review-correction-receipt-" & Guid.NewGuid().ToString("N"))
        Try
            Dim fixture = CreateFixture(folder)
            Dim store As New DocumentReviewStore(fixture.Queue)
            Dim corrected As New JObject From {{"montant", 12.5}}
            Assert.IsTrue(store.RecordReview(fixture.Sha, "validateur-fictif",
                                             DocumentReviewStore.ReviewDecision.ApprouvePourSaisie, corrected))
            Dim receiptPath = Path.Combine(fixture.Queue, fixture.Sha & ".review.json")
            Dim original = File.ReadAllText(receiptPath)

            For Each variante As Integer In {0, 1, 2}
                Dim altered = JObject.Parse(original)
                Select Case variante
                    Case 0
                        altered.Remove("donnees_corrigees")
                    Case 1
                        altered("donnees_corrigees") = New JObject()
                    Case Else
                        altered("donnees_corrigees") = "contenu non structure"
                End Select
                Dim invalid = altered.ToString()
                File.WriteAllText(receiptPath, invalid)
                Assert.ThrowsExactly(Of InvalidDataException)(Sub() store.ListPending())
                Assert.ThrowsExactly(Of InvalidDataException)(
                    Sub() store.RecordReview(fixture.Sha, "autre-validateur",
                                             DocumentReviewStore.ReviewDecision.Rejete, Nothing))
                Assert.AreEqual(invalid, File.ReadAllText(receiptPath))
            Next

            File.WriteAllText(receiptPath, original)
            Assert.AreEqual(0, store.ListPending().Count)
            Assert.AreEqual(original, File.ReadAllText(receiptPath))
            Assert.AreEqual("Facture de test sans information reelle", File.ReadAllText(fixture.Source))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub

End Class
