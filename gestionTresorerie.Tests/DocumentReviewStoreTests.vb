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

End Class
