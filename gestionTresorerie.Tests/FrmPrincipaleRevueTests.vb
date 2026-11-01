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
                             Assert.AreEqual(6, disposition.Controls.Count)
                             Assert.AreEqual(3, disposition.Controls.OfType(Of Label)().Count())
                             Dim etat = DirectCast(disposition.Controls.Find("etatCorrections", True)(0), Label)
                             StringAssert.Contains(etat.Text, "non conservées après fermeture")
                             StringAssert.Contains(etat.Text, "Aucune opération enregistrée")
                             Dim panneaux = disposition.Controls.OfType(Of FlowLayoutPanel)().ToArray()
                             Assert.AreEqual(1, panneaux.Length)
                             Dim boutons = panneaux(0).Controls.OfType(Of Button)().ToArray()
                             Assert.AreEqual(3, boutons.Length)
                             Assert.IsFalse(boutons.Single(Function(b) b.Name = "boutonConfirmerExamen").Enabled)
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


    Private NotInheritable Class PrincipaleSimulationIsolee
        Inherits FrmPrincipale

        Public Property Examiner As Action(Of FrmRevuePropositions)
        Public Property NombrePresentations As Integer

        Protected Overrides Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
            NombrePresentations += 1
            Examiner.Invoke(formulaire)
        End Sub
    End Class

    <TestMethod>
    Public Sub MenuDocumentsSimules_DesactiveSansLotPuisRouvreCorrectionsSansSQL()
        ' Le presentateur est isole : jamais de Show, Load ou Closing de FrmPrincipale.
        SurThreadSta(Sub()
                         Using principale As New PrincipaleSimulationIsolee()
                             Dim menus = principale.MainMenuStrip.Items.Find(
                                 "RevueDocumentsAnalysesToolStripMenuItem", True)
                             Assert.AreEqual(1, menus.Length)
                             Dim menu = DirectCast(menus(0), ToolStripMenuItem)
                             Assert.AreEqual("Revue des documents préparés (simulation)", menu.Text)
                             Assert.AreEqual("AnalyseToolStripMenuItem", menu.OwnerItem.Name)
                             Assert.IsFalse(menu.Enabled)

                             Dim facture As New Facture With {
                                 .IdDoc = 950, .montant_ttc = 42D, .devise = "EUR",
                                 .date_facture = "2026-04-01", .numero_facture = "FICTIVE-950"}
                             Dim piece As New GmailDocumentPropositionAdapter.PieceAnalysee(
                                 "simulation@example.invalid", 1UI, 2UI, 0, facture,
                                 PropositionMouvement.SensMouvement.Debit)
                             Dim workflow = GmailDocumentReviewWorkflow.Preparer(
                                 New GmailDocumentPropositionAdapter.PieceAnalysee() {piece})
                             principale.DefinirWorkflowDocumentsAnalyses(workflow)
                             Assert.IsTrue(menu.Enabled)
                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(
                                                           formulaire.Controls.Find("grillePropositions", True)(0),
                                                           DataGridView)
                                                       Assert.AreEqual(1, grille.Rows.Count)
                                                       Assert.AreEqual(42D, grille.Rows(0).Cells("Montant").Value)
                                                       grille.ClearSelection()
                                                       grille.Rows(0).Selected = True
                                                       Assert.IsTrue(formulaire.CorrigerSelection(Function(dialogue)
                                                                                                     DirectCast(
                                                                                                         dialogue.Controls.Find("montant", True)(0),
                                                                                                         TextBox).Text = "44"
                                                                                                     Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                                                     Return DialogResult.OK
                                                                                                 End Function))
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(1, principale.NombrePresentations)
                             Assert.AreEqual(44D, workflow.SessionRevue.Propositions(piece.CleSource).Montant)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)

                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(
                                                           formulaire.Controls.Find("grillePropositions", True)(0),
                                                           DataGridView)
                                                       Assert.AreEqual(44D, grille.Rows(0).Cells("Montant").Value)
                                                       Assert.AreEqual("En attente de revue humaine",
                                                                       grille.Rows(0).Cells("Statut").Value)
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                             Assert.AreEqual(42D, facture.montant_ttc)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() workflow.SessionRevue.MarquerApprouvee(piece.CleSource))
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() workflow.SessionRevue.MarquerPersistee(piece.CleSource))
                             principale.DefinirWorkflowDocumentsAnalyses(Nothing)
                             Assert.IsFalse(menu.Enabled)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub


    <TestMethod>
    Public Sub MenuDocumentsSimules_RecoitJsonFictifsEtConserveCorrectionsSansSQL()
        ' JSON deja extrait : aucun compte Gmail ni fichier reel n'est utilise.
        SurThreadSta(Sub()
                         Using principale As New PrincipaleSimulationIsolee()
                             Dim menu = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Assert.IsFalse(menu.Enabled)
                             Assert.ThrowsExactly(Of ArgumentNullException)(
                                 Sub() principale.PreparerRevueDocumentsJsonFictifs(Nothing))
                             Assert.IsFalse(menu.Enabled)

                             Dim jsonFacture As String =
                                 "{""type_document"":""Facture"",""donnees_extraites"":{" &
                                 """identite"":{""fournisseur_nom"":""Fictif""}," &
                                 """transaction"":{""montant_ttc"":""42.50"",""devise"":""EUR""," &
                                 """date_facture"":""2026-04-01"",""numero_facture"":""SIM-42""}}}"
                             Dim valide As New GmailDocumentExtractionAdapter.PieceJson(
                                 "simulation@example.invalid", 8UI, 9UI, 0, jsonFacture,
                                 PropositionMouvement.SensMouvement.Debit)
                             Dim invalide As New GmailDocumentExtractionAdapter.PieceJson(
                                 "simulation@example.invalid", 8UI, 9UI, 1, "{invalide",
                                 PropositionMouvement.SensMouvement.Debit)
                             Dim workflow = principale.PreparerRevueDocumentsJsonFictifs(
                                 New GmailDocumentExtractionAdapter.PieceJson() {valide, invalide})
                             Assert.IsTrue(menu.Enabled)
                             Assert.AreEqual(1, workflow.ResultatInitial.Propositions.Count)
                             Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)

                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Dim rejets = DirectCast(formulaire.Controls.Find(
                                                           "grilleRejets", True)(0), DataGridView)
                                                       Assert.AreEqual(1, grille.Rows.Count)
                                                       Assert.AreEqual(1, rejets.Rows.Count)
                                                       Assert.AreEqual(42.5D, grille.Rows(0).Cells("Montant").Value)
                                                       grille.ClearSelection()
                                                       grille.Rows(0).Selected = True
                                                       Assert.IsTrue(formulaire.CorrigerSelection(Function(dialogue)
                                                                                                     DirectCast(dialogue.Controls.Find(
                                                                                                         "montant", True)(0), TextBox).Text = "45"
                                                                                                     Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                                                     Return DialogResult.OK
                                                                                                 End Function))
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(45D, workflow.SessionRevue.Propositions(valide.CleSource).Montant)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Assert.AreEqual(45D, grille.Rows(0).Cells("Montant").Value)
                                                       Assert.AreEqual("En attente de revue humaine",
                                                                       grille.Rows(0).Cells("Statut").Value)
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() workflow.SessionRevue.MarquerApprouvee(valide.CleSource))
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() workflow.SessionRevue.MarquerPersistee(valide.CleSource))
                             ' Un nouveau lot absent ne doit pas laisser l'ancien lot
                             ' accessible depuis le menu de la fenetre principale.
                             Assert.ThrowsExactly(Of ArgumentNullException)(
                                 Sub() principale.PreparerRevueDocumentsJsonFictifs(Nothing))
                             Assert.IsFalse(menu.Enabled)
                             menu.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub


    <TestMethod>
    Public Sub MenuDocumentsSimules_MessagesFictifsVersRevueEtReouvertureSansSQL()
        ' Source IMAP simulee en memoire : ni reseau, ni SQL, ni approbation.
        SurThreadSta(Sub()
                         Using principale As New PrincipaleSimulationIsolee()
                             Dim menu = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Dim jsonFacture As String =
                                 "{""type_document"":""Facture"",""donnees_extraites"":{" &
                                 """identite"":{""fournisseur_nom"":""Fictif""}," &
                                 """transaction"":{""montant_ttc"":""42.50"",""devise"":""EUR""," &
                                 """date_facture"":""2026-04-01"",""numero_facture"":""SIM-43""}}}"
                             Dim message As New GmailDocumentSimulatedInbox.MessageFictif(
                                 "simulation@example.invalid", 12UI, 34UI,
                                 New GmailDocumentSimulatedInbox.PieceFictive() {
                                     New GmailDocumentSimulatedInbox.PieceFictive(
                                         jsonFacture, PropositionMouvement.SensMouvement.Debit),
                                     New GmailDocumentSimulatedInbox.PieceFictive(
                                         "{invalide", PropositionMouvement.SensMouvement.Debit)})
                             Dim workflow = principale.PreparerRevueMessagesFictifs(
                                 New GmailDocumentSimulatedInbox.MessageFictif() {message})
                             Dim cle = GmailDocumentSourceKey.MakeKey(
                                 "simulation@example.invalid", 12UI, 34UI, 0)
                             Assert.IsTrue(menu.Enabled)
                             Assert.AreEqual(1, workflow.ResultatInitial.Propositions.Count)
                             Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)
                             Assert.AreEqual(1, workflow.ResultatInitial.Rejets(0).IndexEntree)

                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Assert.AreEqual(cle, grille.Rows(0).Cells("CleSource").Value)
                                                       grille.ClearSelection()
                                                       grille.Rows(0).Selected = True
                                                       Assert.IsTrue(formulaire.CorrigerSelection(Function(dialogue)
                                                                                                     DirectCast(dialogue.Controls.Find(
                                                                                                         "montant", True)(0), TextBox).Text = "46"
                                                                                                     Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                                                     Return DialogResult.OK
                                                                                                 End Function))
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(46D, workflow.SessionRevue.Propositions(cle).Montant)
                             Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Assert.AreEqual(46D, grille.Rows(0).Cells("Montant").Value)
                                                       Assert.AreEqual("En attente de revue humaine",
                                                                       grille.Rows(0).Cells("Statut").Value)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() workflow.SessionRevue.MarquerApprouvee(cle))
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() workflow.SessionRevue.MarquerPersistee(cle))
                             Assert.ThrowsExactly(Of ArgumentNullException)(
                                 Sub() principale.PreparerRevueMessagesFictifs(Nothing))
                             Assert.IsFalse(menu.Enabled)
                             menu.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub MenuDocumentsSimules_LotEntierementInvalide_ConserveLesRejetsVisibles()
        ' Un lot sans proposition exploitable doit rester consultable pour
        ' expliquer les rejets, sans autoriser une validation ou une ecriture.
        SurThreadSta(Sub()
                         Using principale As New PrincipaleSimulationIsolee()
                             Dim menu = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Dim piece As New GmailDocumentExtractionAdapter.PieceJson(
                                 "simulation@example.invalid", 8UI, 9UI, 0, "{invalide",
                                 PropositionMouvement.SensMouvement.Debit)
                             Dim workflow = principale.PreparerRevueDocumentsJsonFictifs(
                                 New GmailDocumentExtractionAdapter.PieceJson() {piece})
                             Assert.AreEqual(0, workflow.ResultatInitial.Propositions.Count)
                             Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)
                             Assert.IsTrue(menu.Enabled)

                             principale.Examiner = Sub(formulaire)
                                                       Dim propositions = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Dim rejets = DirectCast(formulaire.Controls.Find(
                                                           "grilleRejets", True)(0), DataGridView)
                                                       Assert.AreEqual(0, propositions.Rows.Count)
                                                       Assert.AreEqual(1, rejets.Rows.Count)
                                                       Assert.AreEqual(0, rejets.Rows(0).Cells("IndexEntree").Value)
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             menu.PerformClick()
                             Assert.AreEqual(1, principale.NombrePresentations)
                             Assert.AreEqual(0, workflow.SessionRevue.Audits.Count)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub MenuDemonstration_OuvreUneRevueFictiveDepuisLEcranPrincipalSansSQL()
        ' Le clic reste entièrement en mémoire : aucune ouverture du parent,
        ' aucun compte Gmail, aucune base comptable et aucune approbation.
        SurThreadSta(Sub()
                         Using principale As New PrincipaleSimulationIsolee()
                             Dim demo = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Dim revue = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Assert.IsTrue(demo.Enabled)
                             Assert.IsFalse(revue.Enabled)
                             Assert.AreEqual("AnalyseToolStripMenuItem", demo.OwnerItem.Name)
                             principale.Examiner = Sub(formulaire)
                                                       Dim propositions = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Dim rejets = DirectCast(formulaire.Controls.Find(
                                                           "grilleRejets", True)(0), DataGridView)
                                                       Assert.AreEqual(1, propositions.Rows.Count)
                                                       Assert.AreEqual(1, rejets.Rows.Count)
                                                       Assert.AreEqual("DEMO-001", propositions.Rows(0).Cells("Reference").Value)
                                                       Assert.AreEqual(42.5D, propositions.Rows(0).Cells("Montant").Value)
                                                       Assert.AreEqual("En attente de revue humaine",
                                                                       propositions.Rows(0).Cells("Statut").Value)
                                                       Assert.AreEqual(1, rejets.Rows(0).Cells("IndexEntree").Value)
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             demo.PerformClick()
                             Assert.AreEqual(1, principale.NombrePresentations)
                             Assert.IsTrue(revue.Enabled)
                             revue.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub


    <TestMethod>
    Public Sub MenuDemonstration_EffacerRetireLeLotEtLesCorrectionsSansSQL()
        ' L'effacement ne valide rien et interdit de rouvrir l'ancien lot.
        ' Relancer la demonstration cree une session vierge en memoire.
        SurThreadSta(Sub()
                         Using principale As New PrincipaleSimulationIsolee()
                             Dim demo = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Dim revue = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Dim effacer = DirectCast(principale.MainMenuStrip.Items.Find(
                                 "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                             Assert.IsFalse(effacer.Enabled)
                             Assert.AreEqual("AnalyseToolStripMenuItem", effacer.OwnerItem.Name)

                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       grille.ClearSelection()
                                                       grille.Rows(0).Selected = True
                                                       Assert.IsTrue(formulaire.CorrigerSelection(Function(dialogue)
                                                                                                     DirectCast(dialogue.Controls.Find(
                                                                                                         "montant", True)(0), TextBox).Text = "55"
                                                                                                     Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                                                     Return DialogResult.OK
                                                                                                 End Function))
                                                   End Sub
                             demo.PerformClick()
                             Assert.AreEqual(1, principale.NombrePresentations)
                             Assert.IsTrue(revue.Enabled)
                             Assert.IsTrue(effacer.Enabled)

                             effacer.PerformClick()
                             Assert.IsFalse(revue.Enabled)
                             Assert.IsFalse(effacer.Enabled)
                             revue.PerformClick()
                             Assert.AreEqual(1, principale.NombrePresentations)

                             principale.Examiner = Sub(formulaire)
                                                       Dim grille = DirectCast(formulaire.Controls.Find(
                                                           "grillePropositions", True)(0), DataGridView)
                                                       Assert.AreEqual(42.5D, grille.Rows(0).Cells("Montant").Value)
                                                       Assert.AreEqual("En attente de revue humaine",
                                                                       grille.Rows(0).Cells("Statut").Value)
                                                       Assert.IsFalse(formulaire.IsHandleCreated)
                                                   End Sub
                             demo.PerformClick()
                             Assert.AreEqual(2, principale.NombrePresentations)
                             Assert.IsTrue(revue.Enabled)
                             Assert.IsTrue(effacer.Enabled)
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

End Class
