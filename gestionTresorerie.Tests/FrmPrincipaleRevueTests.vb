Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmPrincipaleRevueTests
    ' Aucun Show/Close : Load et Closing du formulaire principal accèdent à la base.
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
    Public Sub Construction_AjouteUnMenuDeLectureIndependantSousAnalyse()
        SurThreadSta(Sub()
                         Using principale As New FrmPrincipale()
                             Dim menus = principale.MainMenuStrip.Items.Find(
                                 "RevuePropositionsToolStripMenuItem", True)
                             Assert.AreEqual(1, menus.Length)
                             Dim menu = DirectCast(menus(0), ToolStripMenuItem)
                             Assert.AreEqual("Revue des propositions (lecture seule)", menu.Text)
                             ' Available exprime la visibilité configurée sans afficher le parent.
                             Assert.IsTrue(menu.Available)
                             Assert.IsTrue(menu.Enabled)
                             Assert.AreEqual("AnalyseToolStripMenuItem", menu.OwnerItem.Name)
                             Assert.IsTrue(menu.OwnerItem.Available)
                             Assert.AreEqual(0, menu.DropDownItems.Count)
                             Dim analyse = DirectCast(menu.OwnerItem, ToolStripMenuItem)
                             For Each nom In {"ConsulterTrésorerieToolStripMenuItem",
                                              "GénérerBilanToolStripMenuItem1",
                                              "GérerUnMouvementToolStripMenuItem", "DocumentsToolStripMenuItem"}
                                 Assert.AreEqual(1, analyse.DropDownItems.Find(nom, False).Length)
                             Next
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub OuvrirRevuePropositions_RefuseUnResultatAbsentSansAfficher()
        SurThreadSta(Sub()
                         Using principale As New FrmPrincipale()
                             Dim erreur = Assert.ThrowsExactly(Of ArgumentNullException)(
                                 Sub() principale.OuvrirRevuePropositions(Nothing))
                             Assert.AreEqual("resultat", erreur.ParamName)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub RevueConstruiteEnMemoire_NexposeAucunControleDEcriture()
        SurThreadSta(Sub()
                         Dim resultat = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(
                                 New Cheque With {.montant_numerique = 12.34D, .devise = "EUR",
                                     .dateChq = "20260204", .numero_du_cheque = "0000123"},
                                 "cheque-fictif", PropositionMouvement.SensMouvement.Credit)})
                         Using revue As New FrmRevuePropositions(New PropositionRevue(resultat))
                             Dim disposition = DirectCast(revue.Controls(0), TableLayoutPanel)
                             Assert.AreEqual(1, revue.Controls.Count)
                             Assert.AreEqual(5, disposition.Controls.Count)
                             Assert.AreEqual(2, disposition.Controls.OfType(Of Label)().Count())
                             Dim boutons = disposition.Controls.OfType(Of Button)().ToArray()
                             Assert.AreEqual(1, boutons.Length)
                             Assert.AreEqual("boutonFermer", boutons(0).Name)
                             Assert.AreEqual(DialogResult.Cancel, boutons(0).DialogResult)
                             Dim grilles = disposition.Controls.OfType(Of DataGridView)().ToArray()
                             Assert.AreEqual(2, grilles.Length)
                             For Each grille In grilles
                                 Assert.IsTrue(grille.ReadOnly)
                                 Assert.IsFalse(grille.AllowUserToAddRows)
                                 Assert.IsFalse(grille.AllowUserToDeleteRows)
                                 Assert.IsNull(grille.DataSource)
                                 For Each colonne As DataGridViewColumn In grille.Columns
                                     Assert.IsTrue(colonne.ReadOnly)
                                 Next
                             Next
                             Dim propositions = DirectCast(revue.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Assert.AreEqual(1, propositions.Rows.Count)
                             Assert.AreEqual("0000123", propositions.Rows(0).Cells("Reference").Value)
                             Assert.IsFalse(revue.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub MenuCorrectionFictive_ConserveLaSessionSansModifierLeDocument()
        SurThreadSta(Sub()
                         Dim cheque As New Cheque With {.montant_numerique = 12.34D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "0000123"}
                         Dim resultat = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(cheque, "cheque-fictif",
                                 PropositionMouvement.SensMouvement.Credit)})
                         Using principale As New FrmPrincipale()
                             principale.PreparerRevuePropositions(resultat, True)
                             Dim menu = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevuePropositionsToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Assert.AreEqual("Revue des propositions (correction en mémoire)", menu.Text)
                             Using revue = principale.ConstruireFormulaireRevuePropositions()
                                 Dim grille = DirectCast(revue.Controls.Find("grillePropositions", True)(0), DataGridView)
                                 Assert.AreEqual(1, revue.Controls.Find("boutonCorriger", True).Length)
                                 grille.Rows(0).Selected = True
                                 Using correction = revue.CreerFormulaireCorrectionSelectionnee()
                                     DirectCast(correction.Controls.Find("montant", True)(0), TextBox).Text = "88"
                                     Assert.IsTrue(correction.AppliquerCorrection())
                                 End Using
                             End Using
                             Using revueRouverte = principale.ConstruireFormulaireRevuePropositions()
                                 Dim grille = DirectCast(revueRouverte.Controls.Find(
                                     "grillePropositions", True)(0), DataGridView)
                                 Assert.AreEqual(88D, grille.Rows(0).Cells("Montant").Value)
                                 Assert.AreEqual("En attente de revue humaine",
                                                 grille.Rows(0).Cells("Statut").Value)
                             End Using
                             Assert.AreEqual(12.34D, cheque.montant_numerique)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub RetourLectureSeule_DesactiveCorrectionEtConserveLeMenuHistorique()
        SurThreadSta(Sub()
                         Dim resultat = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(
                                 New Facture With {.montant_ttc = 25D, .devise = "EUR",
                                     .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"},
                                 "facture-fictive", PropositionMouvement.SensMouvement.Debit)})
                         Using principale As New FrmPrincipale()
                             principale.PreparerRevuePropositions(resultat, True)
                             Assert.ThrowsExactly(Of ArgumentNullException)(
                                 Sub() principale.PreparerRevuePropositions(Nothing, True))
                             principale.PreparerRevuePropositions(resultat)
                             Dim menu = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevuePropositionsToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Assert.AreEqual("Revue des propositions (lecture seule)", menu.Text)
                             Using revue = principale.ConstruireFormulaireRevuePropositions()
                                 Assert.AreEqual(0, revue.Controls.Find("boutonCorriger", True).Length)
                                 Assert.AreEqual(1, DirectCast(revue.Controls.Find(
                                     "grillePropositions", True)(0), DataGridView).Rows.Count)
                             End Using
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub
End Class
