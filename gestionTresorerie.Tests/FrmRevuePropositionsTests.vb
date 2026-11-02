Option Strict On
Option Explicit On

Imports System
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmRevuePropositionsTests
    ' Exécution STA isolée ; seuls les formulaires de revue sans accès externe sont affichés.
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

    Private Shared Function Grille(formulaire As Form, nom As String) As DataGridView
        Return DirectCast(formulaire.Controls.Find(nom, True)(0), DataGridView)
    End Function

    <TestMethod>
    Public Sub CorrectionEnErreur_AfficheUnMessageSansDetailPriveEtPermetAnnulationEtFermeture()
        SurThreadSta(Sub()
                         Dim resultat = Lot()
                         Dim session As New PropositionSessionRevue(resultat.Propositions)
                         Using f As New FrmRevuePropositions(session, New PropositionRevue(resultat))
                             Dim grilleSource = Grille(f, "grillePropositions")
                             grilleSource.ClearSelection()
                             grilleSource.Rows(0).Selected = True
                             Dim originale = session.Propositions(resultat.Propositions(0).CleSource)
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  Throw New InvalidOperationException("detail-prive-a-ne-pas-afficher")
                                                              End Function))
                             Dim etat = DirectCast(f.Controls.Find("etatRevue", True)(0), Label)
                             StringAssert.Contains(etat.Text, "La correction n'a pas pu être terminée")
                             Assert.IsFalse(etat.Text.Contains("detail-prive"))
                             Assert.AreSame(originale, session.Propositions(originale.CleSource))
                             Assert.AreEqual(0, session.Audits.Count)
                             Assert.AreEqual(originale.Montant, grilleSource.Rows(0).Cells("Montant").Value)
                             f.Show()
                             Assert.IsTrue(etat.Visible)
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue) DialogResult.Cancel))
                             StringAssert.Contains(etat.Text, "Annuler une correction conserve les valeurs précédentes")
                             DirectCast(f.CancelButton, Button).PerformClick()
                             Assert.IsFalse(f.Visible)
                             Assert.AreEqual(0, session.Audits.Count)
                             Assert.AreSame(originale, session.Propositions(originale.CleSource))
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub CorrectionDevenueObsolete_AfficheLeRefusEtLesValeursCourantes()
        SurThreadSta(Sub()
                         Dim resultat = Lot()
                         Dim originale = resultat.Propositions(0)
                         Dim session As New PropositionSessionRevue(resultat.Propositions)
                         Using f As New FrmRevuePropositions(session, New PropositionRevue(resultat))
                             Dim grilleSource = Grille(f, "grillePropositions")
                             grilleSource.ClearSelection()
                             grilleSource.Rows(0).Selected = True
                             Dim correctionPreparee As Boolean
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  correctionPreparee = dialogue.AppliquerCorrection()
                                                                  session.AppliquerCorrection(originale.CleSource,
                                                                      PropositionCorrection.Corriger(originale, originale.DateProposee,
                                                                          77D, originale.Sens, originale.Libelle, originale.Reference))
                                                                  Return DialogResult.OK
                                                              End Function))
                             Assert.IsTrue(correctionPreparee)
                             StringAssert.Contains(DirectCast(f.Controls.Find("etatRevue", True)(0), Label).Text,
                                                   "La correction n'a pas pu être terminée")
                             Assert.AreEqual(1, session.Audits.Count)
                             Assert.AreEqual(77D, session.Propositions(originale.CleSource).Montant)
                             Assert.AreEqual(77D, grilleSource.Rows(0).Cells("Montant").Value)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub ParcoursAnalyseAffichageCorrectionPrevalidation_FermetureConserveLaSession()
        SurThreadSta(Sub()
                         Dim document As New ChequeSansFichier With {.IdDoc = 901,
                             .montant_numerique = 12.34D, .devise = "EUR", .dateChq = "20260204",
                             .numero_du_cheque = "000009", .CheminDoc = "source-synthetique-inexistante"}
                         Dim resultat = New PropositionAnalyseAdapter().ProposerDocuments(
                             New DocumentAgumaaa() {document},
                             {New PropositionDocumentService.Entree(document, "source-synthetique",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Assert.AreEqual(0, resultat.Rejets.Count)
                         Dim originale = resultat.Propositions(0)
                         Dim session As New PropositionSessionRevue(resultat.Propositions)
                         Using f As New FrmRevuePropositions(session, New PropositionRevue(resultat))
                             Dim grilleSource = Grille(f, "grillePropositions")
                             Assert.AreEqual("000009", grilleSource.Rows(0).Cells("Reference").Value)
                             grilleSource.Rows(0).Selected = True
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "20"
                                                                  Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                  Return DialogResult.Cancel
                                                              End Function))
                             Assert.AreSame(originale, session.Propositions(originale.CleSource))
                             Assert.AreEqual(0, session.Audits.Count)
                             Assert.AreEqual(12.34D, grilleSource.Rows(0).Cells("Montant").Value)
                             Assert.IsTrue(f.CorrigerSelection(Function(dialogue)
                                                                 DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "25"
                                                                 Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                 Return DialogResult.OK
                                                             End Function))
                             Assert.AreEqual(25D, grilleSource.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(originale.CleSource, grilleSource.Rows(0).Cells("CleSource").Value)
                             Assert.AreEqual(originale.TypeDocument, grilleSource.Rows(0).Cells("TypeDocument").Value)
                             Dim diagnostic = session.Prevalider(originale.CleSource, document.IdDoc,
                                 Array.Empty(Of String)(), False)
                             Assert.IsFalse(diagnostic.EstPrete)
                             CollectionAssert.Contains(New System.Collections.Generic.List(Of PropositionPrevalidation.Anomalie)(diagnostic.Anomalies),
                                 PropositionPrevalidation.Anomalie.AvertissementsNonResolus)
                             Assert.IsTrue(grilleSource.ReadOnly)
                             Assert.IsNull(f.AcceptButton)
                             Assert.IsFalse(f.IsHandleCreated)
                             f.Close()
                         End Using
                         Dim conservee = session.Propositions(originale.CleSource)
                         Assert.AreEqual(25D, conservee.Montant)
                         Assert.AreEqual("000009", conservee.Reference)
                         Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, conservee.Etat)
                         Assert.AreEqual(1, session.Audits.Count)
                         Assert.AreSame(originale, session.Audits(0).Avant)
                         Assert.AreSame(conservee, session.Audits(0).Proposition)
                         Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(originale.CleSource))
                         Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(originale.CleSource))
                         Assert.AreEqual(12.34D, document.montant_numerique)
                         Assert.AreEqual("000009", document.numero_du_cheque)
                         Assert.AreEqual(0, document.IdMvtDoc)
                         Assert.AreEqual(0, document.Renommages)
                     End Sub)
    End Sub

    Private NotInheritable Class ChequeSansFichier
        Inherits Cheque

        Public Property Renommages As Integer

        Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
            Renommages += 1
            Throw New AssertFailedException("Aucune ecriture de fichier autorisee dans ce parcours.")
        End Function
    End Class

    <TestMethod>
    Public Sub Constructeur_RefuseUneRevueAbsente()
        SurThreadSta(Sub()
                         Dim erreur = Assert.ThrowsExactly(Of ArgumentNullException)(
                             Sub()
                                 Using formulaire As New FrmRevuePropositions(Nothing)
                                 End Using
                             End Sub)
                         Assert.AreEqual("revue", erreur.ParamName)
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub LotMixte_MappeLesValeursEtSepareLesRejets()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
                         Dim cheque As New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "0000123"}
                         Dim revue As New PropositionRevue(New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(facture, "z", PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(cheque, "a", PropositionMouvement.SensMouvement.Credit),
                             New PropositionDocumentService.Entree(Nothing, "absent", PropositionMouvement.SensMouvement.Debit),
                             Nothing}))
                         Using formulaire As New FrmRevuePropositions(revue)
                             Dim propositions = Grille(formulaire, "grillePropositions")
                             Dim rejets = Grille(formulaire, "grilleRejets")
                             Assert.AreEqual(2, propositions.Rows.Count)
                             Assert.AreEqual(2, rejets.Rows.Count)
                             For i As Integer = 0 To revue.Lignes.Count - 1
                                 Dim source = revue.Lignes(i)
                                 Dim cellules = propositions.Rows(i).Cells
                                 Assert.AreEqual(source.CleSource, cellules("CleSource").Value)
                                 Assert.AreEqual(source.TypeDocument, cellules("TypeDocument").Value)
                                 Assert.AreEqual(source.Reference, cellules("Reference").Value)
                                 Assert.AreEqual(source.DateProposee, cellules("DateProposee").Value)
                                 Assert.AreEqual(source.Montant, cellules("Montant").Value)
                                 Assert.IsInstanceOfType(Of Decimal)(cellules("Montant").Value)
                                 Assert.AreEqual(source.Libelle, cellules("Libelle").Value)
                                 Assert.AreEqual("En attente de revue humaine", cellules("Statut").Value)
                                 Assert.AreEqual(String.Join(Environment.NewLine, source.AvertissementsValidation),
                                                 cellules("Avertissements").Value)
                                 Assert.IsTrue(source.AvertissementsValidation.Count > 0)
                             Next
                             Assert.AreEqual("Débit", propositions.Rows(0).Cells("Sens").Value)
                             Assert.AreEqual("Crédit", propositions.Rows(1).Cells("Sens").Value)
                             Assert.AreEqual("0000123", propositions.Rows(1).Cells("Reference").Value)
                             For i As Integer = 0 To revue.Rejets.Count - 1
                                 Assert.AreEqual(revue.Rejets(i).IndexEntree, rejets.Rows(i).Cells("IndexEntree").Value)
                                 Assert.AreEqual(revue.Rejets(i).CleSource, rejets.Rows(i).Cells("CleSource").Value)
                                 Assert.AreEqual(revue.Rejets(i).Motif, rejets.Rows(i).Cells("Motif").Value)
                             Next
                             For Each grilleLecture In {propositions, rejets}
                                 Assert.IsTrue(grilleLecture.ReadOnly)
                                 Assert.IsFalse(grilleLecture.AllowUserToAddRows)
                                 Assert.IsFalse(grilleLecture.AllowUserToDeleteRows)
                                 Assert.IsNull(grilleLecture.DataSource)
                                 Assert.IsFalse(String.IsNullOrWhiteSpace(grilleLecture.AccessibleName))
                                 For Each colonne As DataGridViewColumn In grilleLecture.Columns
                                     Assert.IsTrue(colonne.ReadOnly)
                                     Assert.AreEqual(DataGridViewColumnSortMode.NotSortable, colonne.SortMode)
                                 Next
                             Next
                             Dim fermer = DirectCast(formulaire.Controls.Find("boutonFermer", True)(0), Button)
                             Assert.AreEqual("&Fermer (sans enregistrer)", fermer.Text)
                             Assert.AreSame(fermer, formulaire.CancelButton)
                             Assert.AreEqual(DialogResult.Cancel, fermer.DialogResult)
                             Dim corriger = DirectCast(formulaire.Controls.Find("boutonCorriger", True)(0), Button)
                             Assert.AreEqual("Corriger la sélection (sans valider)", corriger.Text)
                             Assert.IsFalse(corriger.Enabled)
                             Assert.IsNull(formulaire.AcceptButton)
                             Assert.AreEqual(2, Boutons(formulaire))
                             Assert.IsFalse(formulaire.IsHandleCreated)
                         End Using
                         Assert.AreEqual(120.42D, facture.montant_ttc)
                         Assert.AreEqual("0000123", cheque.numero_du_cheque)
                         Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)
                     End Sub)
    End Sub

    Private Shared Function Boutons(parent As Control) As Integer
        Dim total As Integer = 0
        For Each enfant As Control In parent.Controls
            If TypeOf enfant Is Button Then total += 1
            total += Boutons(enfant)
        Next
        Return total
    End Function

    Private Shared Function Lot() As PropositionDocumentService.Resultat
        Return New PropositionDocumentService().Proposer({
            New PropositionDocumentService.Entree(New Cheque With {.montant_numerique = 12D,
                .devise = "EUR", .dateChq = "20260204", .numero_du_cheque = "00001"},
                "fictif-1", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(New Cheque With {.montant_numerique = 24D,
                .devise = "EUR", .dateChq = "20260204", .numero_du_cheque = "00002"},
                "fictif-2", PropositionMouvement.SensMouvement.Credit)})
    End Function

    <TestMethod>
    Public Sub Selection_EtSourcesAbsentes_NouvrentAucunDialogue()
        SurThreadSta(Sub()
                         Dim resultat = Lot()
                         Dim revue As New PropositionRevue(resultat)
                         Using f As New FrmRevuePropositions(revue)
                             Grille(f, "grillePropositions").Rows(0).Selected = True
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue) ThrowDialogueInattendu()))
                         End Using
                         Using f As New FrmRevuePropositions(revue, resultat.Propositions)
                             Grille(f, "grillePropositions").ClearSelection()
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue) ThrowDialogueInattendu()))
                             Assert.IsFalse(DirectCast(f.Controls.Find("boutonCorriger", True)(0), Button).Enabled)
                         End Using
                     End Sub)
    End Sub

    Private Shared Function ThrowDialogueInattendu() As DialogResult
        Assert.Fail("Aucun dialogue ne doit être ouvert.")
        Return DialogResult.Abort
    End Function

    <TestMethod>
    Public Sub Mapping_RefuseSourcesAbsentesInverseesDifferentesOuAmbigues()
        SurThreadSta(Sub()
                         Dim resultat = Lot()
                         Dim revue As New PropositionRevue(resultat)
                         Dim p = resultat.Propositions(0)
                         Dim different = PropositionCorrection.Corriger(p, p.DateProposee, 99D, p.Sens, p.Libelle, p.Reference).Proposition
                         For Each sources As PropositionMouvement() In New PropositionMouvement()() {
                             New PropositionMouvement() {}, New PropositionMouvement() {Nothing, resultat.Propositions(1)},
                             New PropositionMouvement() {resultat.Propositions(1), p},
                             New PropositionMouvement() {different, resultat.Propositions(1)}, New PropositionMouvement() {p, p}}
                             Assert.ThrowsExactly(Of ArgumentException)(Sub()
                                                                          Using f As New FrmRevuePropositions(revue, sources)
                                                                          End Using
                                                                      End Sub)
                         Next
                         Assert.ThrowsExactly(Of ArgumentNullException)(Sub()
                                                                              Using f As New FrmRevuePropositions(revue, Nothing)
                                                                              End Using
                                                                          End Sub)
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Correction_SelectionSeulement_AnnulationIdentiteEtAvertissementsPreserves()
        SurThreadSta(Sub()
                         Dim resultat = Lot()
                         Dim revue As New PropositionRevue(resultat)
                         Dim original = resultat.Propositions(1)
                         Using f As New FrmRevuePropositions(revue, resultat.Propositions)
                             Dim grillePropositions = Grille(f, "grillePropositions")
                             grillePropositions.ClearSelection()
                             grillePropositions.Rows(1).Selected = True
                             Assert.IsTrue(DirectCast(f.Controls.Find("boutonCorriger", True)(0), Button).Enabled)
                             For Each retour In {DialogResult.Cancel, DialogResult.OK}
                                 Assert.IsFalse(f.CorrigerSelection(Function(dialogue) retour))
                             Next
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "99"
                                                                  Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                  Return DialogResult.Cancel
                                                              End Function))
                             Assert.AreEqual(original.Montant, grillePropositions.Rows(1).Cells("Montant").Value)
                             Assert.IsTrue(f.CorrigerSelection(Function(dialogue)
                                                                 DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "99"
                                                                 DirectCast(dialogue.Controls.Find("reference", True)(0), TextBox).Text = "000003"
                                                                 Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                 Return DialogResult.OK
                                                             End Function))
                             Dim cellules = grillePropositions.Rows(1).Cells
                             Assert.AreEqual(99D, cellules("Montant").Value)
                             Assert.AreEqual("000003", cellules("Reference").Value)
                             Assert.AreEqual(original.CleSource, cellules("CleSource").Value)
                             Assert.AreEqual(original.TypeDocument, cellules("TypeDocument").Value)
                             Assert.AreEqual(String.Join(Environment.NewLine, original.AvertissementsValidation), cellules("Avertissements").Value)
                             Assert.AreEqual("En attente de revue humaine", cellules("Statut").Value)
                             Assert.AreEqual(resultat.Propositions(0).Montant, grillePropositions.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(original.Montant, revue.Lignes(1).Montant)
                             Assert.AreEqual("00002", original.Reference)
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  Assert.AreEqual(99D, DialogueMontant(dialogue))
                                                                  Assert.AreEqual(original.CleSource, DirectCast(dialogue.Controls.Find("cleSource", True)(0), Label).Text)
                                                                  Return DialogResult.Cancel
                                                              End Function))
                             Assert.IsTrue(grillePropositions.ReadOnly)
                             Assert.IsFalse(grillePropositions.AllowUserToAddRows)
                             Assert.IsFalse(grillePropositions.AllowUserToDeleteRows)
                             For Each colonne As DataGridViewColumn In grillePropositions.Columns
                                 Assert.IsTrue(colonne.ReadOnly)
                             Next
                             Assert.AreEqual(2, Boutons(f))
                             Assert.IsNull(f.AcceptButton)
                             Assert.IsFalse(f.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    Private Shared Function DialogueMontant(dialogue As FrmCorrectionProposition) As Decimal
        Assert.IsTrue(dialogue.AppliquerCorrection())
        Return dialogue.PropositionCorrigee.Montant
    End Function

    <TestMethod>
    Public Sub Session_RafraichitCorrectionDiagnosticsEtAnnulationSansPersistance()
        SurThreadSta(Sub()
                         Dim lotSource = Lot()
                         Dim original = lotSource.Propositions(0)
                         Dim session As New PropositionSessionRevue(lotSource.Propositions)
                         Using f As New FrmRevuePropositions(session, New PropositionRevue(lotSource))
                             Assert.AreSame(session, f.Session)
                             Dim grilleSource = Grille(f, "grillePropositions")
                             grilleSource.ClearSelection()
                             grilleSource.Rows(0).Selected = True
                             Dim avant = CStr(grilleSource.Rows(0).Cells("Prevalidation").Value)
                             StringAssert.Contains(avant, "DocumentSourceAbsent")
                             StringAssert.Contains(avant, "IdentitesUtiliseesAbsentes")
                             StringAssert.Contains(avant, "ReferenceAmbigue")
                             StringAssert.Contains(avant, "AvertissementsNonResolus")
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                  Return DialogResult.Cancel
                                                              End Function))
                             Assert.AreEqual(0, session.Audits.Count)
                             Assert.AreSame(original, session.Propositions(original.CleSource))
                             Assert.IsTrue(f.CorrigerSelection(Function(dialogue)
                                                                 DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "55"
                                                                 DirectCast(dialogue.Controls.Find("reference", True)(0), TextBox).Text = ""
                                                                 Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                 Return DialogResult.OK
                                                             End Function))
                             Assert.AreEqual(1, session.Audits.Count)
                             Assert.AreSame(original, session.Audits(0).Avant)
                             Assert.AreEqual(55D, grilleSource.Rows(0).Cells("Montant").Value)
                             StringAssert.Contains(CStr(grilleSource.Rows(0).Cells("Prevalidation").Value), "ReferenceVide")
                             Dim actuelle = session.Propositions(original.CleSource)
                             session.AppliquerCorrection(original.CleSource, PropositionCorrection.Corriger(
                                 actuelle, actuelle.DateProposee, 77D, actuelle.Sens, actuelle.Libelle, "000009"))
                             f.ActualiserSession()
                             Assert.AreEqual(77D, grilleSource.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual("000009", grilleSource.Rows(0).Cells("Reference").Value)
                             Assert.AreEqual(avant, grilleSource.Rows(0).Cells("Prevalidation").Value)
                             Assert.AreEqual(original.CleSource, grilleSource.Rows(0).Cells("CleSource").Value)
                             Assert.AreSame(lotSource.Propositions(1), session.Propositions(lotSource.Propositions(1).CleSource))
                             Assert.AreEqual(12D, original.Montant)
                             Assert.AreEqual("00001", original.Reference)
                             Assert.AreEqual(String.Join(Environment.NewLine, original.AvertissementsValidation),
                                             grilleSource.Rows(0).Cells("Avertissements").Value)
                             DirectCast(f.CancelButton, Button).PerformClick()
                             Assert.AreEqual(2, session.Audits.Count)
                             For Each p In session.Propositions.Values
                                 Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, p.Etat)
                                 Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(p.CleSource))
                                 Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(p.CleSource))
                             Next
                             Assert.AreEqual(2, Boutons(f))
                             Assert.IsNull(f.AcceptButton)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Mapping_RefuseCollisionsDeCasseEtEspaces()
        SurThreadSta(Sub()
                         For Each cle In {"source", " SOURCE "}
                             Dim lotSource = New PropositionDocumentService().Proposer({
                                 New PropositionDocumentService.Entree(New Cheque With {.montant_numerique = 12D,
                                     .devise = "EUR", .dateChq = "20260204", .numero_du_cheque = "00001"},
                                     "Source", PropositionMouvement.SensMouvement.Debit),
                                 New PropositionDocumentService.Entree(New Cheque With {.montant_numerique = 24D,
                                     .devise = "EUR", .dateChq = "20260204", .numero_du_cheque = "00002"},
                                     cle, PropositionMouvement.SensMouvement.Debit)})
                             Assert.AreEqual(1, lotSource.Propositions.Count)
                             Assert.AreEqual(1, lotSource.Rejets.Count)
                             Assert.AreEqual("Cle source dupliquee dans le lot.", lotSource.Rejets(0).Motif)
                             Using f As New FrmRevuePropositions(New PropositionRevue(lotSource), lotSource.Propositions)
                                 Assert.AreEqual(1, Grille(f, "grillePropositions").Rows.Count)
                                 Assert.AreEqual(1, Grille(f, "grilleRejets").Rows.Count)
                             End Using
                         Next
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub LotsVideEtEntierementRejete_SontConsultables()
        SurThreadSta(Sub()
                         Dim service As New PropositionDocumentService()
                         For Each resultat In {service.Proposer(Array.Empty(Of PropositionDocumentService.Entree)()),
                                               service.Proposer(New PropositionDocumentService.Entree() {Nothing})}
                             Using formulaire As New FrmRevuePropositions(New PropositionRevue(resultat))
                                 Assert.AreEqual(0, Grille(formulaire, "grillePropositions").Rows.Count)
                                 Assert.AreEqual(resultat.Rejets.Count, Grille(formulaire, "grilleRejets").Rows.Count)
                                 Dim etat = DirectCast(formulaire.Controls.Find("etatRevue", True)(0), Label)
                                 StringAssert.Contains(etat.Text, If(resultat.Rejets.Count = 0,
                                     "aucun lot exploitable", "toutes les entrées du lot ont été rejetées"),
                                     "Manuel hors ligne : ouvrir un lot vide puis rejeté ; distinguer absence de lot et diagnostics à corriger.")
                                 Dim diagnostic = DirectCast(formulaire.Controls.Find("etatDiagnostics", True)(0), Label)
                                 StringAssert.Contains(diagnostic.Text, If(resultat.Rejets.Count = 0,
                                     "Aucune entrée rejetée", "index (base 0)"))
                                 Assert.IsFalse(formulaire.Controls.Find("boutonCorriger", True)(0).Enabled)
                             End Using
                         Next
                     End Sub)
    End Sub

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub ManuelHorsLigne_LireIdentiteSource_CorrigerPuisAnnuler_ConserveInstructionsEtOriginal()
        SurThreadSta(Sub()
                         Dim resultat = Lot()
                         Dim originale = resultat.Propositions(0)
                         Using f As New FrmRevuePropositions(New PropositionRevue(resultat), resultat.Propositions)
                             Dim grilleSource = Grille(f, "grillePropositions")
                             Assert.IsTrue(grilleSource.Columns("CleSource").Visible)
                             Assert.IsTrue(grilleSource.Columns("CleSource").Frozen,
                                 "Manuel : faire défiler horizontalement ; l'identité source reste visible.")
                             Assert.AreEqual(originale.CleSource, grilleSource.Rows(0).Cells("CleSource").Value)
                             Dim instructions = DirectCast(f.Controls.Find("etatRevue", True)(0), Label).Text
                             grilleSource.ClearSelection()
                             grilleSource.Rows(0).Selected = True
                             Assert.IsFalse(f.CorrigerSelection(Function(dialogue)
                                                                  DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "55"
                                                                  Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                  Return DialogResult.Cancel
                                                              End Function),
                                 "Manuel : Corriger la sélection, changer le montant, puis Annuler ; aucune correction retenue.")
                             Assert.AreSame(originale, f.Session.Propositions(originale.CleSource))
                             Assert.AreEqual(0, f.Session.Audits.Count)
                             Assert.AreEqual(originale.Montant, grilleSource.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(instructions, DirectCast(f.Controls.Find("etatRevue", True)(0), Label).Text)
                             f.Show()
                             f.ClientSize = New System.Drawing.Size(700, 450)
                             Assert.IsTrue(f.Controls.Find("etatRevue", True)(0).Visible,
                                 "Manuel : à 700 x 450 et avec le zoom Windows, vérifier le retour à la ligne et les grilles ; fermer sans enregistrer.")
                             DirectCast(f.CancelButton, Button).PerformClick()
                             Assert.IsFalse(f.Visible)
                             Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, originale.Etat)
                         End Using
                     End Sub)
    End Sub
End Class
