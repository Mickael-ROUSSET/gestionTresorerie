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
    Private Class PrincipaleIsolee
        Inherits FrmPrincipale

        Public Property Examiner As Action(Of FrmRevuePropositions)
        Public Property DerniereRevue As FrmRevuePropositions
        Public Property NombreOuvertures As Integer

        Protected Overrides Sub PresenterRevuePropositions(formulaire As FrmRevuePropositions)
            DerniereRevue = formulaire
            NombreOuvertures += 1
            Examiner.Invoke(formulaire)
        End Sub
    End Class

    Private Shared Sub CliquerRevue(principale As FrmPrincipale)
        DirectCast(principale.MainMenuStrip.Items.Find(
            "RevuePropositionsToolStripMenuItem", True)(0), ToolStripMenuItem).PerformClick()
    End Sub

    Private Shared Sub VerifierLectureSeule(revue As FrmRevuePropositions)
        Assert.IsNull(revue.Session)
        Dim disposition = DirectCast(revue.Controls(0), TableLayoutPanel)
        Dim actions = disposition.Controls.OfType(Of FlowLayoutPanel)().Single()
        CollectionAssert.AreEquivalent(New String() {"boutonFermer", "boutonCorriger"},
                                      actions.Controls.Cast(Of Control)().Select(Function(c) c.Name).ToArray())
        Assert.IsFalse(actions.Controls("boutonCorriger").Enabled)
        For Each grille In disposition.Controls.OfType(Of DataGridView)()
            Assert.IsTrue(grille.ReadOnly)
            Assert.IsFalse(grille.AllowUserToAddRows)
            Assert.IsFalse(grille.AllowUserToDeleteRows)
        Next
    End Sub

    <TestMethod>
    Public Sub MenuSansResultat_AfficheUneRevueVideEtSeFermeSansChargerLeParent()
        SurThreadSta(Sub()
                         Using principale As New PrincipaleIsolee()
                             principale.Examiner = Sub(revue)
                                                       VerifierLectureSeule(revue)
                                                       For Each nom In {"grillePropositions", "grilleRejets"}
                                                           Assert.AreEqual(0, DirectCast(revue.Controls.Find(nom, True)(0), DataGridView).Rows.Count)
                                                       Next
                                                       Dim disposition = DirectCast(revue.Controls(0), TableLayoutPanel)
                                                       StringAssert.Contains(DirectCast(disposition.GetControlFromPosition(0, 0), Label).Text,
                                                                             "Aucune proposition")
                                                       revue.Show()
                                                       DirectCast(revue.Controls.Find("boutonFermer", True)(0), Button).PerformClick()
                                                       Assert.IsFalse(revue.Visible)
                                                       Assert.IsFalse(principale.IsHandleCreated)
                                                   End Sub
                             CliquerRevue(principale)
                             Assert.IsTrue(principale.DerniereRevue.IsDisposed)
                             CliquerRevue(principale)
                             Assert.AreEqual(2, principale.NombreOuvertures)
                             Assert.IsTrue(principale.DerniereRevue.IsDisposed)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub ResultatEnMemoire_EstConsultablePuisReouvertDepuisLeMenuSansSourcesDeCorrection()
        SurThreadSta(Sub()
                         Dim service As New PropositionDocumentService()
                         Dim resultat = service.Proposer({New PropositionDocumentService.Entree(
                             New Cheque With {.montant_numerique = 12.34D, .devise = "EUR",
                                 .dateChq = "20260204", .numero_du_cheque = "0000123"},
                             "source-fictive", PropositionMouvement.SensMouvement.Credit)})
                         Using principale As New PrincipaleIsolee()
                             principale.Examiner = Sub(revue)
                                                       VerifierLectureSeule(revue)
                                                       Dim grille = DirectCast(revue.Controls.Find("grillePropositions", True)(0), DataGridView)
                                                       Assert.AreEqual(1, grille.Rows.Count)
                                                       Assert.AreEqual("source-fictive", grille.Rows(0).Cells("CleSource").Value)
                                                       Assert.AreEqual("0000123", grille.Rows(0).Cells("Reference").Value)
                                                       grille.Rows(0).Selected = True
                                                       Assert.IsFalse(revue.CorrigerSelection())
                                                   End Sub
                             principale.OuvrirRevuePropositions(resultat)
                             Dim premiere = principale.DerniereRevue
                             Assert.IsTrue(premiere.IsDisposed)
                             CliquerRevue(principale)
                             Assert.AreNotSame(premiere, principale.DerniereRevue)
                             Assert.AreEqual(2, principale.NombreOuvertures)
                             Assert.IsTrue(principale.DerniereRevue.IsDisposed)
                             Assert.IsFalse(principale.IsHandleCreated)
                             Assert.AreEqual(12.34D, resultat.Propositions(0).Montant)
                             ' Un nouveau lot vide remplace le precedent sans reutiliser ses lignes.
                             principale.Examiner = Sub(revue)
                                                       VerifierLectureSeule(revue)
                                                       Assert.AreEqual(0, DirectCast(revue.Controls.Find("grillePropositions", True)(0), DataGridView).Rows.Count)
                                                       Assert.AreEqual(1, DirectCast(revue.Controls.Find("grilleRejets", True)(0), DataGridView).Rows.Count)
                                                   End Sub
                             principale.OuvrirRevuePropositions(service.Proposer({
                                 New PropositionDocumentService.Entree(Nothing, "source-incertaine", PropositionMouvement.SensMouvement.Credit)}))
                             CliquerRevue(principale)
                         End Using
                     End Sub)
    End Sub

End Class
