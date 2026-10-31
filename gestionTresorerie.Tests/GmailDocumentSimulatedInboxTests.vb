Option Strict On
Option Explicit On

Imports System
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailDocumentSimulatedInboxTests
    Private Const JsonFacture As String =
        "{""type_document"":""Facture"",""donnees_extraites"":{""identite"":{""fournisseur_nom"":""Fournisseur fictif""},""transaction"":{""montant_ttc"":""42.50"",""devise"":""EUR"",""date_facture"":""2026-04-01"",""numero_facture"":""FAUX-1""}}}"
    Private Const JsonCheque As String =
        "{""type_document"":""Cheque"",""donnees_extraites"":{""identite"":{""emetteur_numero_cheque"":""0000789""},""transaction"":{""montant_numerique"":""15.25"",""devise"":""EUR"",""date_emission"":""20260402""}}}"

    <TestMethod>
    Public Sub DeuxMessagesFictifs_PiecesIndexeesRejetsEtRevueSansEcriture()
        Dim compteA = "simulation-a@example.invalid"
        Dim compteB = "simulation-b@example.invalid"
        Dim messages As GmailDocumentSimulatedInbox.MessageFictif() = {
            New GmailDocumentSimulatedInbox.MessageFictif(
                compteA, 5UI, 8UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonFacture, PropositionMouvement.SensMouvement.Debit),
                    Nothing,
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        "{invalide", PropositionMouvement.SensMouvement.Debit),
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonCheque, PropositionMouvement.SensMouvement.Credit)}),
            New GmailDocumentSimulatedInbox.MessageFictif(
                compteB, 5UI, 8UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonFacture, PropositionMouvement.SensMouvement.Debit)})}

        Dim workflow = GmailDocumentSimulatedInbox.PreparerRevue(messages)
        Assert.AreEqual(3, workflow.ResultatInitial.Propositions.Count)
        Assert.AreEqual(2, workflow.ResultatInitial.Rejets.Count)
        Assert.AreEqual(1, workflow.ResultatInitial.Rejets(0).IndexEntree)
        Assert.AreEqual(2, workflow.ResultatInitial.Rejets(1).IndexEntree)
        Dim cleFacture = GmailDocumentSourceKey.MakeKey(compteA, 5UI, 8UI, 0)
        Dim cleCheque = GmailDocumentSourceKey.MakeKey(compteA, 5UI, 8UI, 3)
        Dim cleAutreCompte = GmailDocumentSourceKey.MakeKey(compteB, 5UI, 8UI, 0)
        Assert.AreNotEqual(cleFacture, cleAutreCompte)
        Assert.AreEqual(cleFacture, workflow.ResultatInitial.Propositions(0).CleSource)
        Assert.AreEqual(cleCheque, workflow.ResultatInitial.Propositions(1).CleSource)
        Assert.AreEqual(cleAutreCompte, workflow.ResultatInitial.Propositions(2).CleSource)
        Assert.AreEqual(0, workflow.SessionRevue.Audits.Count)
        For Each cle In {cleFacture, cleCheque, cleAutreCompte}
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() workflow.SessionRevue.MarquerApprouvee(cle))
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() workflow.SessionRevue.MarquerPersistee(cle))
        Next
    End Sub

    <TestMethod>
    Public Sub SourceFictiveAbsente_EstRefuseeExplicitement()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() GmailDocumentSimulatedInbox.PreparerRevue(Nothing))
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim message = New GmailDocumentSimulatedInbox.MessageFictif(
                    "simulation@example.invalid", 1UI, 2UI, Nothing)
            End Sub)
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailDocumentSimulatedInbox.PreparerRevue(
                New GmailDocumentSimulatedInbox.MessageFictif() {Nothing}))
    End Sub

    <TestMethod>
    Public Sub MessagesRejoues_MemeUidEtCompteNormalise_NeCreentPasDeuxPropositions()
        ' Un même UID IMAP rejoué ne doit jamais produire une deuxième opération.
        ' Un autre UID du même compte reste traitable dans le même lot fictif.
        Dim messages As GmailDocumentSimulatedInbox.MessageFictif() = {
            New GmailDocumentSimulatedInbox.MessageFictif(
                "SIMULATION@example.invalid", 40UI, 50UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonFacture, PropositionMouvement.SensMouvement.Debit)}),
            New GmailDocumentSimulatedInbox.MessageFictif(
                " simulation@example.invalid ", 40UI, 50UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonCheque, PropositionMouvement.SensMouvement.Credit)}),
            New GmailDocumentSimulatedInbox.MessageFictif(
                "simulation@example.invalid", 40UI, 51UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonCheque, PropositionMouvement.SensMouvement.Credit)})}

        Dim workflow = GmailDocumentSimulatedInbox.PreparerRevue(messages)
        Assert.AreEqual(2, workflow.ResultatInitial.Propositions.Count)
        Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)
        Dim rejet = workflow.ResultatInitial.Rejets(0)
        Assert.AreEqual(1, rejet.IndexEntree)
        Assert.AreEqual("Cle source dupliquee dans le lot.", rejet.Motif)
        Assert.AreEqual(workflow.ResultatInitial.Propositions(0).CleSource, rejet.CleSource)
        Assert.AreNotEqual(workflow.ResultatInitial.Propositions(0).CleSource,
                           workflow.ResultatInitial.Propositions(1).CleSource)
        Assert.AreEqual(0, workflow.SessionRevue.Audits.Count)
        For Each proposition In workflow.ResultatInitial.Propositions
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            proposition.Etat)
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() workflow.SessionRevue.MarquerApprouvee(proposition.CleSource))
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() workflow.SessionRevue.MarquerPersistee(proposition.CleSource))
        Next
    End Sub

    <TestMethod>
    Public Sub UidValidityRenouvele_MemeUidResteDeuxDocumentsDistincts()
        ' Apres recreation d'une boite IMAP, un UID peut etre reutilise.
        ' UIDVALIDITY doit donc rester une partie de l'identite de source.
        Dim compte = "simulation@example.invalid"
        Dim messages As GmailDocumentSimulatedInbox.MessageFictif() = {
            New GmailDocumentSimulatedInbox.MessageFictif(
                compte, 40UI, 50UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonFacture, PropositionMouvement.SensMouvement.Debit)}),
            New GmailDocumentSimulatedInbox.MessageFictif(
                compte, 41UI, 50UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        JsonCheque, PropositionMouvement.SensMouvement.Credit)})}

        Dim workflow = GmailDocumentSimulatedInbox.PreparerRevue(messages)
        Dim premiereCle = GmailDocumentSourceKey.MakeKey(compte, 40UI, 50UI, 0)
        Dim secondeCle = GmailDocumentSourceKey.MakeKey(compte, 41UI, 50UI, 0)
        Assert.AreNotEqual(premiereCle, secondeCle)
        Assert.AreEqual(2, workflow.ResultatInitial.Propositions.Count)
        Assert.AreEqual(0, workflow.ResultatInitial.Rejets.Count)
        Assert.AreEqual(premiereCle, workflow.ResultatInitial.Propositions(0).CleSource)
        Assert.AreEqual(secondeCle, workflow.ResultatInitial.Propositions(1).CleSource)
        Assert.AreEqual(0, workflow.SessionRevue.Audits.Count)
        For Each cle In {premiereCle, secondeCle}
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            workflow.SessionRevue.Propositions(cle).Etat)
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() workflow.SessionRevue.MarquerApprouvee(cle))
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() workflow.SessionRevue.MarquerPersistee(cle))
        Next
    End Sub

    <TestMethod>
    Public Sub IdentitesImapInvalides_SontRefuseesAvantLaRevue()
        ' Les identites IMAP non fiables ne doivent produire aucune proposition.
        Dim pieces As GmailDocumentSimulatedInbox.PieceFictive() = {
            New GmailDocumentSimulatedInbox.PieceFictive(
                JsonFacture, PropositionMouvement.SensMouvement.Debit)}
        For Each compte As String In New String() {Nothing, "", "  "}
            Dim message = New GmailDocumentSimulatedInbox.MessageFictif(
                compte, 1UI, 2UI, pieces)
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() GmailDocumentSimulatedInbox.PreparerRevue(
                    New GmailDocumentSimulatedInbox.MessageFictif() {message}))
        Next
        Dim sansUidValidity = New GmailDocumentSimulatedInbox.MessageFictif(
            "simulation@example.invalid", 0UI, 2UI, pieces)
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() GmailDocumentSimulatedInbox.PreparerRevue(
                New GmailDocumentSimulatedInbox.MessageFictif() {sansUidValidity}))
        Dim sansUid = New GmailDocumentSimulatedInbox.MessageFictif(
            "simulation@example.invalid", 1UI, 0UI, pieces)
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() GmailDocumentSimulatedInbox.PreparerRevue(
                New GmailDocumentSimulatedInbox.MessageFictif() {sansUid}))
    End Sub

    ' Ne jamais afficher FrmPrincipale : son chargement peut acceder a SQL.
    Private Shared Sub SurThreadSta(action As Action)
        Dim erreur As Exception = Nothing
        Dim thread As New Thread(Sub()
                                     Try
                                         action()
                                     Catch ex As Exception
                                         erreur = ex
                                     End Try
                                 End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        thread.Join()
        If erreur IsNot Nothing Then ExceptionDispatchInfo.Capture(erreur).Throw()
    End Sub

    <TestMethod>
    Public Sub MessagesSimules_VersFenetreCorrectionReouvertureSansEcriture()
        ' Parcours de bout en bout en memoire, sans Gmail reel ni base SQL.
        SurThreadSta(Sub()
                         Dim messages As GmailDocumentSimulatedInbox.MessageFictif() = {
                             New GmailDocumentSimulatedInbox.MessageFictif(
                                 "simulation@example.invalid", 23UI, 24UI,
                                 New GmailDocumentSimulatedInbox.PieceFictive() {
                                     New GmailDocumentSimulatedInbox.PieceFictive(
                                         JsonFacture, PropositionMouvement.SensMouvement.Debit),
                                     Nothing,
                                     New GmailDocumentSimulatedInbox.PieceFictive(
                                         JsonCheque, PropositionMouvement.SensMouvement.Credit)})}
                         Dim workflow = GmailDocumentSimulatedInbox.PreparerRevue(messages)
                         Dim cle = GmailDocumentSourceKey.MakeKey(
                             "simulation@example.invalid", 23UI, 24UI, 0)
                         Assert.AreEqual(2, workflow.ResultatInitial.Propositions.Count)
                         Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)
                         Assert.AreEqual(1, workflow.ResultatInitial.Rejets(0).IndexEntree)

                         Using premiere = workflow.CreerFenetre()
                             Dim grille = DirectCast(
                                 premiere.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Dim rejets = DirectCast(
                                 premiere.Controls.Find("grilleRejets", True)(0), DataGridView)
                             Assert.AreEqual(2, grille.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows.Count)
                             Assert.AreEqual(cle, grille.Rows(0).Cells("CleSource").Value)
                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(premiere.CorrigerSelection(Function(dialogue)
                                                                         DirectCast(
                                                                             dialogue.Controls.Find("montant", True)(0),
                                                                             TextBox).Text = "46"
                                                                         Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                         Return DialogResult.OK
                                                                     End Function))
                             Assert.IsFalse(premiere.IsHandleCreated)
                         End Using

                         Using seconde = workflow.CreerFenetre()
                             Dim grille = DirectCast(
                                 seconde.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Dim rejets = DirectCast(
                                 seconde.Controls.Find("grilleRejets", True)(0), DataGridView)
                             Assert.AreEqual(46D, grille.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual("En attente de revue humaine",
                                             grille.Rows(0).Cells("Statut").Value)
                             Assert.AreEqual(1, rejets.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows(0).Cells("IndexEntree").Value)
                             Assert.IsFalse(seconde.IsHandleCreated)
                         End Using

                         Assert.AreEqual(46D, workflow.SessionRevue.Propositions(cle).Montant)
                         Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                         Assert.AreEqual(42.5D, workflow.ResultatInitial.Propositions(0).Montant)
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerApprouvee(cle))
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerPersistee(cle))
                     End Sub)
    End Sub
End Class
