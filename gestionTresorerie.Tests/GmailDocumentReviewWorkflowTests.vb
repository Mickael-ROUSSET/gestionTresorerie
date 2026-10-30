Option Strict On
Option Explicit On

Imports System
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailDocumentReviewWorkflowTests
    ' Tests hors ligne, sans afficher FrmPrincipale (Load/Closing peuvent acceder a SQL).
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
    Public Sub Reouverture_ConserveCorrectionsAuditEtRejetsSansValidationAutomatique()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {
                             .IdDoc = 801, .montant_ttc = 42D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "FICTIVE-801"}
                         Dim cheque As New Cheque With {
                             .IdDoc = 802, .montant_numerique = 15D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "FICTIF-802"}
                         Dim pieces As GmailDocumentPropositionAdapter.PieceAnalysee() = {
                             New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "faux@example.invalid", 12UI, 46UI, 0, facture,
                                 PropositionMouvement.SensMouvement.Debit),
                             New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "faux@example.invalid", 12UI, 46UI, 1, Nothing,
                                 PropositionMouvement.SensMouvement.Debit),
                             New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "faux@example.invalid", 12UI, 46UI, 2, cheque,
                                 PropositionMouvement.SensMouvement.Credit)}
                         Dim workflow = GmailDocumentReviewWorkflow.Preparer(pieces)
                         Assert.AreEqual(2, workflow.ResultatInitial.Propositions.Count)
                         Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)
                         Dim cle = pieces(0).CleSource

                         Using premiere = workflow.CreerFenetre()
                             Dim grille = DirectCast(premiere.Controls.Find("grillePropositions", True)(0), DataGridView)
                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(premiere.CorrigerSelection(Function(dialogue)
                                                                         DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "50"
                                                                         Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                         Return DialogResult.OK
                                                                     End Function))
                             Assert.AreEqual(50D, workflow.SessionRevue.Propositions(cle).Montant)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                             Assert.IsFalse(premiere.IsHandleCreated)
                         End Using

                         Using seconde = workflow.CreerFenetre()
                             Dim grille = DirectCast(seconde.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Dim rejets = DirectCast(seconde.Controls.Find("grilleRejets", True)(0), DataGridView)
                             Assert.AreEqual(2, grille.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows(0).Cells("IndexEntree").Value)
                             Assert.AreEqual(cle, grille.Rows(0).Cells("CleSource").Value)
                             Assert.AreEqual(50D, grille.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(15D, grille.Rows(1).Cells("Montant").Value)
                             Assert.AreEqual("En attente de revue humaine", grille.Rows(0).Cells("Statut").Value)
                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(seconde.CorrigerSelection(Function(dialogue)
                                                                        DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "51"
                                                                        Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                        Return DialogResult.OK
                                                                    End Function))
                             Assert.IsFalse(seconde.IsHandleCreated)
                         End Using
                         Assert.AreEqual(51D, workflow.SessionRevue.Propositions(cle).Montant)
                         Assert.AreEqual(2, workflow.SessionRevue.Audits.Count)
                         Assert.AreEqual(42D, facture.montant_ttc)
                         Assert.AreEqual(15D, cheque.montant_numerique)
                         Assert.AreEqual(42D, workflow.ResultatInitial.Propositions(0).Montant)
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerApprouvee(cle))
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerPersistee(cle))
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub LotAbsentRefuse_LotVideAfficheSansEcriture()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() GmailDocumentReviewWorkflow.Preparer(Nothing))
        SurThreadSta(Sub()
                         Dim workflow = GmailDocumentReviewWorkflow.Preparer(
                             Array.Empty(Of GmailDocumentPropositionAdapter.PieceAnalysee)())
                         Assert.AreEqual(0, workflow.ResultatInitial.Propositions.Count)
                         Assert.AreEqual(0, workflow.SessionRevue.Audits.Count)
                         Using fenetre = workflow.CreerFenetre()
                             Dim grille = DirectCast(fenetre.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Assert.AreEqual(0, grille.Rows.Count)
                             Assert.IsFalse(fenetre.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub
End Class
