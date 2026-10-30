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
                             Dim panneaux = disposition.Controls.OfType(Of FlowLayoutPanel)().ToArray()
                             Assert.AreEqual(1, panneaux.Length)
                             Dim boutons = panneaux(0).Controls.OfType(Of Button)().ToArray()
                             Assert.AreEqual(2, boutons.Length)
                             Dim fermer = boutons.Single(Function(b) b.Name = "boutonFermer")
                             Assert.AreEqual(DialogResult.Cancel, fermer.DialogResult)
                             Dim corriger = boutons.Single(Function(b) b.Name = "boutonCorriger")
                             Assert.IsFalse(corriger.Enabled)
                             Assert.IsFalse(boutons.Any(Function(b) b.Text.ToLowerInvariant().Contains("approuver") OrElse b.Text.ToLowerInvariant().Contains("enregistrer")))
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
    Public Sub FabriqueRevueAvecSession_TransmetCorrectionsSansApprobation()
        SurThreadSta(Sub()
                         Dim document As New Cheque With {.montant_numerique = 12D,
                             .devise = "EUR", .dateChq = "20260204", .numero_du_cheque = "FICTIF-001"}
                         Dim resultat = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(document, "gmail-fictif-1",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Dim session As New PropositionSessionRevue(resultat.Propositions)
                         Assert.ThrowsExactly(Of ArgumentNullException)(
                             Sub() FrmPrincipale.CreerFenetreRevuePropositions(Nothing, session))
                         Assert.ThrowsExactly(Of ArgumentNullException)(
                             Sub() FrmPrincipale.CreerFenetreRevuePropositions(resultat, Nothing))
                         Using formulaire = FrmPrincipale.CreerFenetreRevuePropositions(resultat, session)
                             Dim grille = DirectCast(formulaire.Controls.Find("grillePropositions", True)(0), DataGridView)
                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(formulaire.CorrigerSelection(Function(dialogue)
                                                                           DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "25"
                                                                           Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                           Return DialogResult.OK
                                                                       End Function))
                             Assert.AreEqual(25D, session.Propositions("gmail-fictif-1").Montant)
                             Assert.AreEqual(1, session.Audits.Count)
                             Assert.AreEqual("En attente de revue humaine", grille.Rows(0).Cells("Statut").Value)
                             Assert.IsFalse(formulaire.IsHandleCreated)
                         End Using
                         Assert.AreEqual(12D, document.montant_numerique)
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() session.MarquerApprouvee("gmail-fictif-1"))
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() session.MarquerPersistee("gmail-fictif-1"))
                         Dim autre = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(document, "gmail-fictif-1",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Dim autreSession As New PropositionSessionRevue(autre.Propositions)
                         Assert.ThrowsExactly(Of ArgumentException)(
                             Sub()
                                 Using formulaire = FrmPrincipale.CreerFenetreRevuePropositions(resultat, autreSession)
                                 End Using
                             End Sub)
                     End Sub)
    End Sub

End Class
