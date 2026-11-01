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
    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    <DataRow("environnement", "envAgumaaa", 1)>
    <DataRow("configuration", "niveau de log", 2)>
    <DataRow("secrets", "SmtpPassword", 3)>
    <DataRow("grille SQL", "droits de lecture", 4)>
    Public Sub Demarrage_EchecDiagnostiqueSansDetailsSensiblesNiEtapesSuivantes(
        etape As String, conseil As String, nombreEtapes As Integer)
        SurThreadSta(Sub()
                         Using principale As New PrincipaleDemarrageIsolee With {.EtapeEnEchec = etape}
                             Dim grille = DirectCast(principale.Controls.Find("dgvPrincipale", True)(0), DataGridView)
                             Dim sourceInitiale = grille.DataSource
                             principale.Show()
                             Assert.IsTrue(principale.Visible)
                             CollectionAssert.AreEqual(
                                 New String() {"environnement", "configuration", "secrets", "grille SQL"}.Take(nombreEtapes).ToArray(),
                                 principale.Etapes.ToArray())
                             Assert.AreEqual(1, principale.Diagnostics.Count)
                             Dim diagnostic = principale.Diagnostics.Single()
                             StringAssert.Contains(diagnostic, "étape : " & etape)
                             StringAssert.Contains(diagnostic, conseil)
                             StringAssert.Contains(diagnostic, "Fermez la fenêtre")
                             Assert.IsFalse(diagnostic.Contains("secret-fictif"))
                             Assert.IsFalse(diagnostic.Contains("serveur-fictif"))
                             Assert.AreSame(sourceInitiale, grille.DataSource)
                             principale.Close()
                             Assert.IsTrue(principale.IsDisposed)
                             Assert.AreEqual(nombreEtapes, principale.Etapes.Count)
                             Assert.AreEqual(1, principale.Diagnostics.Count)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub Demarrage_ReussiteChargeLaGrilleFictiveSansDiagnosticEtFerme()
        SurThreadSta(Sub()
                         Using principale As New PrincipaleDemarrageIsolee()
                             principale.Show()
                             CollectionAssert.AreEqual(
                                 New String() {"environnement", "configuration", "secrets", "grille SQL"},
                                 principale.Etapes.ToArray())
                             Assert.AreEqual(0, principale.Diagnostics.Count)
                             Dim grille = DirectCast(principale.Controls.Find("dgvPrincipale", True)(0), DataGridView)
                             Assert.AreSame(principale.Donnees, grille.DataSource)
                             Assert.IsTrue(principale.Visible)
                             principale.Close()
                             Assert.IsTrue(principale.IsDisposed)
                             Assert.AreEqual(4, principale.Etapes.Count)
                         End Using
                     End Sub)
    End Sub

    Private Class PrincipaleDemarrageIsolee
        Inherits FrmPrincipale

        Public Property EtapeEnEchec As String
        Public ReadOnly Etapes As New System.Collections.Generic.List(Of String)
        Public ReadOnly Diagnostics As New System.Collections.Generic.List(Of String)
        Public ReadOnly Donnees As New System.Data.DataTable()

        Private Sub Executer(etape As String)
            Etapes.Add(etape)
            If etape = EtapeEnEchec Then
                Throw New InvalidOperationException("Password=secret-fictif;Server=serveur-fictif",
                    New Exception("secret-fictif"))
            End If
        End Sub

        Protected Overrides Sub ChargerEnvironnementDemarrage()
            Executer("environnement")
        End Sub

        Protected Overrides Sub ChargerConfigurationDemarrage()
            Executer("configuration")
        End Sub

        Protected Overrides Sub ChargerSecretsDemarrage()
            Executer("secrets")
        End Sub

        Protected Overrides Function ChargerDonneesPrincipales() As System.Data.DataTable
            Executer("grille SQL")
            Return Donnees
        End Function

        Protected Overrides Sub PresenterDiagnosticDemarrage(message As String)
            Diagnostics.Add(message)
        End Sub
    End Class

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub LotAvecDoublonEtTypeInconnu_DiagnosticsConsultablesSansChargerLeParent()
        SurThreadSta(Sub()
                         Dim cheque As New Cheque With {.IdDoc = 901, .montant_numerique = 12.34D,
                             .devise = "EUR", .dateChq = "20260204", .numero_du_cheque = "0000123"}
                         ' Le nom facture.pdf ne suffit pas pour reconnaitre le type du document.
                         Dim inconnu = New Moq.Mock(Of DocumentAgumaaa)(Moq.MockBehavior.Strict).Object
                         inconnu.CheminDoc = "facture.pdf"
                         Dim resultat = New PropositionAnalyseAdapter().Proposer({
                             New PropositionDocumentService.Entree(cheque, "valide", PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(cheque, "doublon", PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(inconnu, "inconnu", PropositionMouvement.SensMouvement.Debit)})
                         Assert.AreEqual(1, resultat.Propositions.Count)
                         Assert.AreEqual(2, resultat.Rejets.Count)
                         StringAssert.Contains(resultat.Rejets(0).Motif, "Document deja present")
                         StringAssert.Contains(resultat.Rejets(1).Motif, "Type de document inconnu ou ambigu")
                         Using principale As New PrincipaleIsolee()
                             principale.Examiner = Sub(revue)
                                                       VerifierLectureSeule(revue)
                                                       Assert.IsNull(revue.AcceptButton)
                                                       Dim rejets = DirectCast(revue.Controls.Find("grilleRejets", True)(0), DataGridView)
                                                       Assert.AreEqual(2, rejets.Rows.Count)
                                                       For i = 0 To resultat.Rejets.Count - 1
                                                           Assert.AreEqual(resultat.Rejets(i).IndexEntree, rejets.Rows(i).Cells("IndexEntree").Value)
                                                           Assert.AreEqual(resultat.Rejets(i).CleSource, rejets.Rows(i).Cells("CleSource").Value)
                                                           Assert.AreEqual(resultat.Rejets(i).Motif, rejets.Rows(i).Cells("Motif").Value)
                                                       Next
                                                       Dim grille = DirectCast(revue.Controls.Find("grillePropositions", True)(0), DataGridView)
                                                       Assert.AreEqual(1, grille.Rows.Count)
                                                       grille.CurrentCell = grille.Rows(0).Cells("Reference")
                                                       Assert.IsFalse(revue.CorrigerSelection())
                                                       revue.Show()
                                                       DirectCast(revue.CancelButton, Button).PerformClick()
                                                       Assert.IsFalse(revue.Visible)
                                                       Assert.IsFalse(principale.IsHandleCreated)
                                                   End Sub
                             principale.OuvrirRevuePropositions(resultat)
                             CliquerRevue(principale)
                             Assert.AreEqual(2, principale.NombreOuvertures)
                             Assert.IsTrue(principale.DerniereRevue.IsDisposed)
                         End Using
                         Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, resultat.Propositions(0).Etat)
                         Assert.AreEqual(0, cheque.IdMvtDoc)
                         Assert.AreEqual(0, inconnu.IdMvtDoc)
                         Assert.AreEqual(12.34D, cheque.montant_numerique)
                         Assert.AreEqual("0000123", cheque.numero_du_cheque)
                         Assert.AreEqual("facture.pdf", inconnu.CheminDoc)
                     End Sub)
    End Sub

    ' Les tests de revue n'affichent pas le parent ; seuls les tests de démarrage isolent Load.
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
                             Assert.AreEqual("&Fermer (sans enregistrer)", fermer.Text)
                             Assert.IsFalse(boutons.Where(Function(b) b IsNot fermer).Any(Function(b) b.Text.ToLowerInvariant().Contains("approuver") OrElse b.Text.ToLowerInvariant().Contains("enregistrer")))
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
                                                       Dim aide = DirectCast(revue.Controls.Find("etatRevue", True)(0), Label)
                                                       For Each instruction In {"documents déjà analysés", "clé source", "débit/crédit",
                                                           "PropositionAnalyseAdapter.Proposer", "FrmPrincipale.OuvrirRevuePropositions(resultat)",
                                                           "ne permet pas de sélectionner", "sans approbation ni enregistrement"}
                                                           StringAssert.Contains(aide.Text, instruction,
                                                               "Manuel hors ligne : Analyse > Revue ; lire comment faire fournir un lot vérifié, puis Fermer sans enregistrer.")
                                                       Next
                                                       Assert.AreSame(revue.Controls.Find("etatRevue", True)(0),
                                                                      disposition.GetControlFromPosition(0, 0))
                                                       Assert.IsNull(revue.AcceptButton)
                                                       Assert.AreSame(DirectCast(revue.Controls.Find("boutonFermer", True)(0), IButtonControl), revue.CancelButton)
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
