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
    ' Construction et inspection sans Show, Application.Run ou boucle de messages.
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
                             Assert.AreEqual("&Fermer", fermer.Text)
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
    Public Sub LotsVideEtEntierementRejete_SontConsultables()
        SurThreadSta(Sub()
                         Dim service As New PropositionDocumentService()
                         For Each resultat In {service.Proposer(Array.Empty(Of PropositionDocumentService.Entree)()),
                                               service.Proposer(New PropositionDocumentService.Entree() {Nothing})}
                             Using formulaire As New FrmRevuePropositions(New PropositionRevue(resultat))
                                 Assert.AreEqual(0, Grille(formulaire, "grillePropositions").Rows.Count)
                                 Assert.AreEqual(resultat.Rejets.Count, Grille(formulaire, "grilleRejets").Rows.Count)
                             End Using
                         Next
                     End Sub)
    End Sub
End Class
