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
                             Dim disposition = DirectCast(formulaire.Controls(0), TableLayoutPanel)
                             Dim nombreBoutons As Integer = 0
                             For Each controle As Control In disposition.Controls
                                 If TypeOf controle Is Button Then nombreBoutons += 1
                             Next
                             Assert.AreEqual(1, nombreBoutons)
                             Assert.IsFalse(formulaire.IsHandleCreated)
                         End Using
                         Assert.AreEqual(120.42D, facture.montant_ttc)
                         Assert.AreEqual("0000123", cheque.numero_du_cheque)
                         Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)
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
                             End Using
                         Next
                     End Sub)
    End Sub
End Class
