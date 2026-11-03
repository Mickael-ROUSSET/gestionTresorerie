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
Public Class DemonstrationWindowsTests
    Private NotInheritable Class PrincipaleFictive
        Inherits FrmPrincipale

        Public Sub New()
            MyBase.New(True)
        End Sub

        Public Property NombrePropositions As Integer
        Public Property NombreRejets As Integer
        Public Property NombreOuvertures As Integer
        Public Property SimulerExamen As Boolean
        Public Property StatutPremiereProposition As String
        Public Property StatutApresExamen As String
        Public Property DemandeValidationApresExamen As String

        Public Sub ChargerSansAfficher()
            MyBase.OnLoad(EventArgs.Empty)
        End Sub

        Protected Overrides Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
            NombreOuvertures += 1
            NombrePropositions = DirectCast(
                formulaire.Controls.Find("grillePropositions", True)(0), DataGridView).Rows.Count
            NombreRejets = DirectCast(
                formulaire.Controls.Find("grilleRejets", True)(0), DataGridView).Rows.Count
            Dim grille = DirectCast(
                formulaire.Controls.Find("grillePropositions", True)(0), DataGridView)
            StatutPremiereProposition = CStr(grille.Rows(0).Cells("Statut").Value)
            If SimulerExamen Then
                grille.ClearSelection()
                grille.Rows(0).Selected = True
                Assert.IsTrue(formulaire.ConfirmerExamenSelection())
                StatutApresExamen = CStr(grille.Rows(0).Cells("Statut").Value)
                DemandeValidationApresExamen =
                    CStr(grille.Rows(0).Cells("DemandeValidation").Value)
            End If
        End Sub
    End Class

    Private Shared Sub EnSta(action As Action)
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

    ' Compile uniquement dans la variante DEMO_BUILD : le constructeur False
    ' ne doit jamais permettre de revenir aux accès comptables de production.
#If DEMO_BUILD Then
    Private NotInheritable Class PrincipaleSansDemandeDeDemo
        Inherits FrmPrincipale

        Public Sub New()
            MyBase.New(False)
        End Sub

        Public Sub ChargerSansAfficher()
            MyBase.OnLoad(EventArgs.Empty)
        End Sub
    End Class

    <TestMethod>
    Public Sub DemoBuild_ForceLeModeFictif_MemeSansOptionDemo()
        EnSta(Sub()
                  Using fenetre As New PrincipaleSansDemandeDeDemo()
                      fenetre.ChargerSansAfficher()
                      StringAssert.Contains(fenetre.Text, "DÉMONSTRATION HORS LIGNE")
                      ' Le demarrage DEMO_BUILD doit neutraliser la sauvegarde automatique
                      ' de My.Settings, meme si le programme est ferme normalement.
                      Dim myProject = GetType(FrmPrincipale).Assembly.GetType(
                          "gestionTresorerie.My.MyProject", True)
                      Dim applicationProperty = myProject.GetProperty(
                          "Application",
                          System.Reflection.BindingFlags.Static Or
                          System.Reflection.BindingFlags.Public Or
                          System.Reflection.BindingFlags.NonPublic)
                      Assert.IsNotNull(applicationProperty)
                      Dim application = DirectCast(
                          applicationProperty.GetValue(Nothing),
                          Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase)
                      Assert.IsFalse(application.SaveMySettingsOnExit)
                      Assert.AreEqual(1, fenetre.Controls.Find("annonceDemonstration", True).Length)
                      Assert.IsFalse(fenetre.Controls.Find("dgvPrincipale", True)(0).Enabled)
                      Assert.IsFalse(fenetre.IsHandleCreated)
                  End Using
              End Sub)
    End Sub
#End If

    <TestMethod>
    Public Sub Demo_DemarreSansInitialisationComptable_EtNeutraliseLesMenusReels()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      StringAssert.Contains(fenetre.Text, "DÉMONSTRATION HORS LIGNE")
                      Assert.AreEqual(1, fenetre.Controls.Find("annonceDemonstration", True).Length)
                      Dim menus = fenetre.MainMenuStrip.Items.OfType(Of ToolStripMenuItem)().ToArray()
                      Assert.AreEqual(1, menus.Count(Function(m) m.Enabled))
                      Assert.AreEqual("AnalyseToolStripMenuItem",
                                      menus.Single(Function(m) m.Enabled).Name)
                      Dim analyse = menus.Single(Function(m) m.Enabled)
                      Dim actions = analyse.DropDownItems.OfType(Of ToolStripMenuItem)().
                          Where(Function(m) m.Enabled).Select(Function(m) m.Name).ToArray()
                      CollectionAssert.AreEquivalent(
                          New String() {"DemoDocumentsFictifsToolStripMenuItem"}, actions)
                      Assert.IsFalse(DirectCast(fenetre.Controls.Find("dgvPrincipale", True)(0),
                                                DataGridView).Enabled)
                      Assert.IsFalse(fenetre.IsHandleCreated)
                  End Using
              End Sub)
    End Sub

    <TestMethod>
    Public Sub Demo_ChargeFactureChequeEtTroisRejets_PuisReinitialiseSansFichier()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      demo.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                      Assert.IsTrue(revue.Enabled)
                      Assert.IsTrue(effacer.Enabled)
                      effacer.PerformClick()
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      demo.PerformClick()
                      Assert.AreEqual(2, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                      Assert.IsFalse(fenetre.IsHandleCreated)
                  End Using
              End Sub)
    End Sub

    <TestMethod>
    Public Sub Demo_ExamenFictifNeValideRien_EtReinitialisationEffaceLaConfirmation()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)

                      fenetre.SimulerExamen = True
                      demo.PerformClick()
                      Assert.AreEqual("En attente de revue humaine", fenetre.StatutPremiereProposition)
                      Assert.AreEqual("Examen confirmé (sans approbation)", fenetre.StatutApresExamen)
                      Assert.AreEqual("Bloquée : identité documentaire non vérifiée",
                                      fenetre.DemandeValidationApresExamen)

                      ' La confirmation reste consultable dans la meme session, sans validation.
                      fenetre.SimulerExamen = False
                      revue.PerformClick()
                      Assert.AreEqual("Examen confirmé (sans approbation)",
                                      fenetre.StatutPremiereProposition)

                      ' Effacer puis relancer cree une nouvelle session non examinee.
                      effacer.PerformClick()
                      Assert.IsFalse(revue.Enabled)
                      demo.PerformClick()
                      Assert.AreEqual("En attente de revue humaine",
                                      fenetre.StatutPremiereProposition)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                  End Using
              End Sub)
    End Sub

    <TestMethod>
    Public Sub Demo_PreparationInvalideRetireLaRevuePrecedente_EtPermetDeRecommencer()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)

                      demo.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreOuvertures)
                      Assert.IsTrue(revue.Enabled)
                      Assert.IsTrue(effacer.Enabled)

                      ' Une préparation invalide doit effacer la session précédente
                      ' avant de lever l'erreur : aucune ancienne proposition accessible.
                      Assert.ThrowsExactly(Of ArgumentNullException)(
                          Sub() fenetre.PreparerRevueMessagesFictifs(Nothing))
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      revue.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreOuvertures)

                      ' Le menu de démonstration reste utilisable après l'échec.
                      demo.PerformClick()
                      Assert.AreEqual(2, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                  End Using
              End Sub)
    End Sub

    ' Une identité IMAP invalide, même sur un message sans pièce jointe,
    ' ne doit jamais laisser accessible une ancienne revue fictive.
    <TestMethod>
    Public Sub Demo_IdentiteImapInvalideEffaceLeLotPrecedent()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)

                      demo.PerformClick()
                      Assert.IsTrue(revue.Enabled)
                      Assert.IsTrue(effacer.Enabled)
                      Assert.AreEqual(1, fenetre.NombreOuvertures)

                      Dim invalide As New GmailDocumentSimulatedInbox.MessageFictif(
                          "demo@example.invalid", 0UI, 1UI,
                          Array.Empty(Of GmailDocumentSimulatedInbox.PieceFictive)())
                      Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
                          Sub() fenetre.PreparerRevueMessagesFictifs(
                              New GmailDocumentSimulatedInbox.MessageFictif() {invalide}))
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      revue.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreOuvertures)

                      demo.PerformClick()
                      Assert.AreEqual(2, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                  End Using
              End Sub)
    End Sub

    ' Captures strictement fictives sur le runner Windows, destinées à une revue
    ' visuelle séparée. Elles ne constituent pas une validation humaine du parcours.
#If DEMO_BUILD Then
    Private Shared Sub CapturerFenetreFictive(fenetre As Form, nom As String)
        Dim espace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")
        If String.IsNullOrWhiteSpace(espace) Then Return
        Dim dossier = System.IO.Path.Combine(espace, "artifacts", "demo-screenshots")
        System.IO.Directory.CreateDirectory(dossier)
        Application.DoEvents()
        Using capture As New System.Drawing.Bitmap(fenetre.Width, fenetre.Height)
            fenetre.DrawToBitmap(capture, New System.Drawing.Rectangle(0, 0, fenetre.Width, fenetre.Height))
            Dim chemin = System.IO.Path.Combine(dossier, nom)
            capture.Save(chemin, System.Drawing.Imaging.ImageFormat.Png)
            Assert.IsTrue(New System.IO.FileInfo(chemin).Length > 0,
                          "Capture de démonstration vide : " & nom)
        End Using
    End Sub
#End If

    ' Contrôle automatisé avec un véritable handle WinForms sur le runner Windows.
    ' Il ne remplace PAS une vérification visuelle et interactive par une personne.
#If DEMO_BUILD Then
    <TestMethod>
    Public Sub DemoBuild_AfficheEtFermeLaFenetrePrincipale_AvecExemplesFictifs()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.Show()
                      Assert.IsTrue(fenetre.IsHandleCreated)
                      Assert.IsTrue(fenetre.Visible)
                      StringAssert.Contains(fenetre.Text, "DÉMONSTRATION HORS LIGNE")
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      demo.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                      fenetre.Close()
                      Assert.IsTrue(fenetre.IsDisposed)
                  End Using
              End Sub)
    End Sub
    ' Affichage non modal réel sur le runner Windows : les contrôles de la revue
    ' sont créés et actionnés sans Gmail, SQL, fichier ni interaction humaine.
    ' Ce test automatisé ne remplace pas une recette visuelle manuelle.
    Private NotInheritable Class PrincipaleAvecRevueAffichee
        Inherits FrmPrincipale

        Public Sub New()
            MyBase.New(True)
        End Sub

        Public Property NombreRevuesAffichees As Integer

        Protected Overrides Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
            formulaire.Show(Me)
            Try
                Assert.IsTrue(formulaire.IsHandleCreated)
                Assert.IsTrue(formulaire.Visible)
                Dim propositions = DirectCast(
                    formulaire.Controls.Find("grillePropositions", True)(0), DataGridView)
                Dim rejets = DirectCast(
                    formulaire.Controls.Find("grilleRejets", True)(0), DataGridView)
                Assert.AreEqual(2, propositions.Rows.Count)
                Assert.AreEqual(3, rejets.Rows.Count)

                ' Contrôler les données réellement visibles, et non seulement les compteurs :
                ' facture et chèque fictifs, type inconnu, JSON invalide et doublon source.
                Assert.AreEqual("FACTURE", CStr(propositions.Rows(0).Cells("TypeDocument").Value))
                Assert.AreEqual(42.5D, CDec(propositions.Rows(0).Cells("Montant").Value))
                Assert.AreEqual("CHEQUE", CStr(propositions.Rows(1).Cells("TypeDocument").Value))
                Assert.AreEqual(18.75D, CDec(propositions.Rows(1).Cells("Montant").Value))
                Assert.AreEqual(2, CInt(rejets.Rows(0).Cells("IndexEntree").Value))
                Assert.AreEqual(3, CInt(rejets.Rows(1).Cells("IndexEntree").Value))
                Assert.AreEqual(4, CInt(rejets.Rows(2).Cells("IndexEntree").Value))
                Assert.AreEqual("Extraction JSON invalide ou non prise en charge.",
                                CStr(rejets.Rows(0).Cells("Motif").Value))
                Assert.AreEqual("Extraction JSON invalide ou non prise en charge.",
                                CStr(rejets.Rows(1).Cells("Motif").Value))
                Assert.AreEqual("Cle source dupliquee dans le lot.",
                                CStr(rejets.Rows(2).Cells("Motif").Value))
                Assert.AreEqual(CStr(propositions.Rows(0).Cells("CleSource").Value),
                                CStr(rejets.Rows(2).Cells("CleSource").Value))

                ' Identités longues consultables en entier au survol, sans écraser
                ' les colonnes métier ; les valeurs sources restent inchangées.
                Assert.IsTrue(CStr(propositions.Rows(0).Cells("CleSource").Value).Length > 50)
                Assert.IsTrue(propositions.Columns("CleSource").Width <= 200)
                Assert.IsTrue(rejets.Columns("CleSource").Width <= 200)
                Assert.AreEqual(CStr(propositions.Rows(0).Cells("CleSource").Value),
                                propositions.Rows(0).Cells("CleSource").ToolTipText)
                Assert.AreEqual(CStr(rejets.Rows(2).Cells("CleSource").Value),
                                rejets.Rows(2).Cells("CleSource").ToolTipText)
                Assert.AreEqual(DataGridViewAutoSizeColumnMode.Fill,
                                rejets.Columns("Motif").AutoSizeMode)

                propositions.ClearSelection()
                propositions.Rows(0).Selected = True
                Dim confirmer = DirectCast(
                    formulaire.Controls.Find("boutonConfirmerExamen", True)(0), Button)
                Assert.IsTrue(confirmer.Enabled)
                confirmer.PerformClick()
                Assert.AreEqual("Examen confirmé (sans approbation)",
                                CStr(propositions.Rows(0).Cells("Statut").Value))
                Assert.AreEqual("Bloquée : identité documentaire non vérifiée",
                                CStr(propositions.Rows(0).Cells("DemandeValidation").Value))

                ' La confirmation est réversible et ne déclenche aucune approbation.
                ' Tester les vrais boutons de la fenêtre, pas uniquement les méthodes.
                Dim annuler = DirectCast(
                    formulaire.Controls.Find("boutonAnnulerExamen", True)(0), Button)
                Assert.IsFalse(confirmer.Enabled)
                Assert.IsTrue(annuler.Enabled)
                annuler.PerformClick()
                Assert.AreEqual("En attente de revue humaine",
                                CStr(propositions.Rows(0).Cells("Statut").Value))
                Assert.AreEqual("Bloquée : examen humain non confirmé",
                                CStr(propositions.Rows(0).Cells("DemandeValidation").Value))
                Assert.IsTrue(confirmer.Enabled)
                Assert.IsFalse(annuler.Enabled)
                Dim etat = DirectCast(
                    formulaire.Controls.Find("etatCorrections", True)(0), Label)
                StringAssert.Contains(etat.Text, "Examens confirmés : 0")

                ' Une nouvelle confirmation reste une simple trace en mémoire.
                confirmer.PerformClick()
                Assert.AreEqual("Examen confirmé (sans approbation)",
                                CStr(propositions.Rows(0).Cells("Statut").Value))
                Assert.AreEqual("Bloquée : identité documentaire non vérifiée",
                                CStr(propositions.Rows(0).Cells("DemandeValidation").Value))
                StringAssert.Contains(etat.Text, "Examens confirmés : 1")
                CapturerFenetreFictive(formulaire, "02-revue-documents-fictifs.png")
                NombreRevuesAffichees += 1
            Finally
                formulaire.Close()
            End Try
        End Sub
    End Class

    <TestMethod>
    Public Sub DemoBuild_AfficheReellementLaRevueEtControleLaConfirmation()
        EnSta(Sub()
                  Using fenetre As New PrincipaleAvecRevueAffichee()
                      fenetre.Show()
                      Assert.IsTrue(fenetre.Visible)
                      StringAssert.Contains(fenetre.Text, "DÉMONSTRATION HORS LIGNE")
                      CapturerFenetreFictive(fenetre, "01-ecran-principal.png")
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      demo.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreRevuesAffichees)
                      Assert.IsTrue(revue.Enabled)
                      effacer.PerformClick()
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      fenetre.Close()
                  End Using
              End Sub)
    End Sub

    ' Une nouvelle instance ne doit jamais récupérer les examens de la session précédente.
    ' La fermeture n'écrit rien : toutes les données de revue restent en mémoire.
    <TestMethod>
    Public Sub DemoBuild_RelancementOublieLaSessionEtLesConfirmations()
        EnSta(Sub()
                  Using premiere As New PrincipaleFictive()
                      premiere.Show()
                      Dim demo = DirectCast(premiere.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(premiere.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      premiere.SimulerExamen = True
                      demo.PerformClick()
                      Assert.AreEqual("Examen confirmé (sans approbation)",
                                      premiere.StatutApresExamen)
                      Assert.IsTrue(revue.Enabled)
                      premiere.Close()
                  End Using

                  Using seconde As New PrincipaleFictive()
                      seconde.Show()
                      Dim demo = DirectCast(seconde.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(seconde.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(seconde.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      Assert.AreEqual(0, seconde.NombreOuvertures)
                      demo.PerformClick()
                      Assert.AreEqual("En attente de revue humaine",
                                      seconde.StatutPremiereProposition)
                      Assert.AreEqual(2, seconde.NombrePropositions)
                      Assert.AreEqual(3, seconde.NombreRejets)
                      seconde.Close()
                  End Using
              End Sub)
    End Sub
#End If
End Class
