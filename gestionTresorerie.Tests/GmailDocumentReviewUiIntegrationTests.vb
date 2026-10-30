Option Strict On
Option Explicit On

Imports System
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailDocumentReviewUiIntegrationTests
    ' Scenario integralement hors ligne : identites IMAP fictives, documents en memoire.
    ' Ne pas ouvrir FrmPrincipale : ses evenements Load/Closing peuvent acceder a SQL.
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
    Public Sub PiecesGmailFictives_RevueWindowsCorrectionSansApprobationNiPersistance()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {
                             .IdDoc = 601, .montant_ttc = 42.5D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "FACT-FICTIVE"}
                         Dim cheque As New Cheque With {
                             .IdDoc = 602, .montant_numerique = 15D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "0000987"}
                         Dim pieces As GmailDocumentPropositionAdapter.PieceAnalysee() = {
                             New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "faux@example.invalid", 12UI, 34UI, 0, facture,
                                 PropositionMouvement.SensMouvement.Debit),
                             New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "faux@example.invalid", 12UI, 34UI, 1, Nothing,
                                 PropositionMouvement.SensMouvement.Debit),
                             New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "faux@example.invalid", 12UI, 34UI, 2, cheque,
                                 PropositionMouvement.SensMouvement.Credit)}
                         Dim resultat = New GmailDocumentPropositionAdapter().Proposer(pieces)
                         Assert.AreEqual(2, resultat.Propositions.Count)
                         Assert.AreEqual(1, resultat.Rejets.Count)
                         Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
                         Dim revue As New PropositionRevue(resultat)

                         Using formulaire As New FrmRevuePropositions(revue, resultat.Propositions)
                             Dim grille = DirectCast(formulaire.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Dim rejets = DirectCast(formulaire.Controls.Find("grilleRejets", True)(0), DataGridView)
                             Assert.AreEqual(2, grille.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows.Count)
                             Assert.AreEqual("Document absent.", rejets.Rows(0).Cells("Motif").Value)
                             Assert.AreEqual(pieces(0).CleSource, grille.Rows(0).Cells("CleSource").Value)
                             Assert.AreEqual(pieces(2).CleSource, grille.Rows(1).Cells("CleSource").Value)
                             Assert.AreEqual("En attente de revue humaine", grille.Rows(0).Cells("Statut").Value)
                             Assert.AreEqual("En attente de revue humaine", grille.Rows(1).Cells("Statut").Value)

                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(formulaire.CorrigerSelection(Function(dialogue)
                                                                           DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "50"
                                                                           Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                           Return DialogResult.OK
                                                                       End Function))
                             Assert.AreEqual(50D, grille.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(pieces(0).CleSource, grille.Rows(0).Cells("CleSource").Value)
                             Assert.AreEqual("En attente de revue humaine", grille.Rows(0).Cells("Statut").Value)
                             Assert.AreEqual(15D, grille.Rows(1).Cells("Montant").Value)
                             Assert.IsTrue(grille.ReadOnly)
                             Assert.IsFalse(formulaire.IsHandleCreated)
                         End Using

                         Dim session As New PropositionSessionRevue(resultat.Propositions)
                         For Each proposition In resultat.Propositions
                             Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                                             proposition.Etat)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() session.MarquerApprouvee(proposition.CleSource))
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() session.MarquerPersistee(proposition.CleSource))
                         Next
                         Assert.AreEqual(0, session.Audits.Count)
                         Assert.AreEqual(0, facture.IdMvtDoc)
                         Assert.AreEqual(42.5D, facture.montant_ttc)
                         Assert.AreEqual(0, cheque.IdMvtDoc)
                         Assert.AreEqual(15D, cheque.montant_numerique)
                     End Sub)
    End Sub
End Class
