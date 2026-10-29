Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionAnalyseAdapterTests
    Private Shared Function FactureFictive() As Facture
        Return New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-001",
            .CheminDoc = "chemin-fictif-inutilise", .fournisseur_nom = "Fournisseur fictif"}
    End Function

    Private Shared Function ChequeFictif() As Cheque
        Return New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
            .dateChq = "20260203", .numero_du_cheque = "0000123"}
    End Function

    <TestMethod>
    Public Sub LotMixte_RespecteLesClesEtSensExplicitesSansModifierLesSources()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim resultat = New PropositionAnalyseAdapter().Proposer({
            New PropositionDocumentService.Entree(facture, "source-f", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(cheque, "source-c", PropositionMouvement.SensMouvement.Debit)})
        Dim revue As New PropositionRevue(resultat)
        Assert.AreEqual(0, revue.Rejets.Count)
        Assert.AreEqual(2, revue.Lignes.Count)
        Assert.AreEqual("source-f", revue.Lignes(0).CleSource)
        Assert.AreEqual("source-c", revue.Lignes(1).CleSource)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Credit, revue.Lignes(0).Sens)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Debit, revue.Lignes(1).Sens)
        Assert.AreEqual(120.42D, revue.Lignes(0).Montant)
        Assert.AreEqual(42.12D, revue.Lignes(1).Montant)
        Assert.AreEqual("0000123", revue.Lignes(1).Reference)
        For Each ligne In revue.Lignes
            Assert.AreEqual(New DateTime(2026, 2, 3), ligne.DateProposee)
            Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, ligne.Statut)
            Assert.AreEqual(1, ligne.AvertissementsValidation.Count)
        Next
        Assert.AreEqual(120.42D, facture.montant_ttc)
        Assert.AreEqual("2026-02-03", facture.date_facture)
        Assert.AreEqual("chemin-fictif-inutilise", facture.CheminDoc)
        Assert.AreEqual("Fournisseur fictif", facture.fournisseur_nom)
        Assert.AreEqual("0000123", cheque.numero_du_cheque)
    End Sub

    <TestMethod>
    Public Sub Rejets_ConservesExactementCommeDansLeService()
        Dim invalide = FactureFictive()
        invalide.date_facture = "invalide"
        Dim devise = ChequeFictif()
        devise.devise = "USD"
        Dim montant = FactureFictive()
        montant.montant_ttc = 0D
        Dim lot = {
            New PropositionDocumentService.Entree(invalide, "date", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(FactureFictive(), "date", PropositionMouvement.SensMouvement.Credit),
            Nothing,
            New PropositionDocumentService.Entree(Nothing, "absent", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(FactureFictive(), " ", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(ChequeFictif(), "sens", CType(99, PropositionMouvement.SensMouvement)),
            New PropositionDocumentService.Entree(devise, "devise", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(montant, "montant", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(New DocumentNonPrisEnCharge(), "type", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(ChequeFictif(), "valide", PropositionMouvement.SensMouvement.Credit)}
        Dim attendu = New PropositionDocumentService().Proposer(lot)
        Dim revue = New PropositionAnalyseAdapter().PreparerRevue(lot)
        Assert.AreEqual(9, revue.Rejets.Count)
        Assert.AreEqual(1, revue.Lignes.Count)
        Assert.AreEqual("valide", revue.Lignes(0).CleSource)
        For i = 0 To attendu.Rejets.Count - 1
            Assert.AreEqual(attendu.Rejets(i).IndexEntree, revue.Rejets(i).IndexEntree)
            Assert.AreEqual(attendu.Rejets(i).CleSource, revue.Rejets(i).CleSource)
            Assert.AreEqual(attendu.Rejets(i).Motif, revue.Rejets(i).Motif)
        Next
        Assert.AreEqual("Cle source dupliquee dans le lot.", revue.Rejets(1).Motif)
    End Sub

    <TestMethod>
    Public Sub Doublons_ComparaisonExacteEtAucunEtatEntreAppels()
        Dim document = FactureFictive()
        Dim lot = {
            New PropositionDocumentService.Entree(document, "source", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(document, "source", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(document, "SOURCE", PropositionMouvement.SensMouvement.Credit)}
        Dim adapter As New PropositionAnalyseAdapter()
        For repetition = 1 To 2
            Dim revue = adapter.PreparerRevue(lot)
            Assert.AreEqual(2, revue.Lignes.Count)
            Assert.AreEqual(1, revue.Rejets.Count)
            Assert.AreEqual(1, revue.Rejets(0).IndexEntree)
            Assert.AreEqual("source", revue.Lignes(0).CleSource)
            Assert.AreEqual("SOURCE", revue.Lignes(1).CleSource)
        Next
    End Sub

    <TestMethod>
    Public Sub Revue_ResteIndependanteDesDocumentsSources()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim revue = New PropositionAnalyseAdapter().PreparerRevue({
            New PropositionDocumentService.Entree(facture, "f", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(cheque, "c", PropositionMouvement.SensMouvement.Credit)})
        facture.montant_ttc = 999D
        facture.date_facture = "invalide"
        cheque.numero_du_cheque = "modifie"
        Assert.AreEqual(120.42D, revue.Lignes(0).Montant)
        Assert.AreEqual(New DateTime(2026, 2, 3), revue.Lignes(0).DateProposee)
        Assert.AreEqual("0000123", revue.Lignes(1).Reference)
        For Each type In {GetType(PropositionRevue), GetType(PropositionRevue.Ligne), GetType(PropositionRevue.DiagnosticRejet)}
            For Each champ In type.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.NonPublic)
                Assert.IsFalse(GetType(DocumentAgumaaa).IsAssignableFrom(champ.FieldType))
                Assert.AreNotEqual(GetType(PropositionDocumentService.Resultat), champ.FieldType)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub LotVideEtLotAbsent()
        Dim adapter As New PropositionAnalyseAdapter()
        Dim revue = adapter.PreparerRevue(Array.Empty(Of PropositionDocumentService.Entree)())
        Assert.AreEqual(0, revue.Lignes.Count)
        Assert.AreEqual(0, revue.Rejets.Count)
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() adapter.Proposer(Nothing))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() adapter.PreparerRevue(Nothing))
    End Sub

    Private NotInheritable Class DocumentNonPrisEnCharge
        Inherits DocumentAgumaaa

        Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
            Throw New InvalidOperationException("Aucun renommage autorise dans ce test.")
        End Function
    End Class
End Class
