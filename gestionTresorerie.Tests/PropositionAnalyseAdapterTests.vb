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
    Public Sub DocumentsAnalyses_TypeInconnuEtSourceEtrangereConserventLeurProvenance()
        Dim inconnu As New DocumentNonPrisEnCharge With {
            .CategorieDoc = "Facture", .CheminDoc = "facture.pdf"}
        Dim resultat = New PropositionAnalyseAdapter().ProposerDocuments({inconnu}, {
            New PropositionDocumentService.Entree(inconnu, " inconnu ", PropositionMouvement.SensMouvement.Debit, " analyse "),
            New PropositionDocumentService.Entree(ChequeFictif(), "etranger", PropositionMouvement.SensMouvement.Credit, "autre analyse")})
        Assert.AreEqual(0, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Sources.Count)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual(" inconnu ", resultat.Rejets(0).CleSource)
        Assert.AreEqual(" analyse ", resultat.Rejets(0).Provenance)
        StringAssert.Contains(resultat.Rejets(0).Motif, "mapping explicite")
        Assert.AreEqual("etranger", resultat.Rejets(1).CleSource)
        Assert.AreEqual("autre analyse", resultat.Rejets(1).Provenance)
        Assert.AreEqual("Correspondance sans document du lot.", resultat.Rejets(1).Motif)
        Assert.AreEqual(0, inconnu.IdDoc)
        Assert.AreEqual("facture.pdf", inconnu.CheminDoc)
    End Sub

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
    Public Sub Doublons_NormalisesEtAucunEtatEntreAppels()
        Dim document = FactureFictive()
        Dim lot = {
            New PropositionDocumentService.Entree(document, "source", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(document, "source", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(FactureFictive(), "SOURCE", PropositionMouvement.SensMouvement.Credit)}
        Dim adapter As New PropositionAnalyseAdapter()
        For repetition = 1 To 2
            Dim revue = adapter.PreparerRevue(lot)
            Assert.AreEqual(1, revue.Lignes.Count)
            Assert.AreEqual(2, revue.Rejets.Count)
            Assert.AreEqual(1, revue.Rejets(0).IndexEntree)
            Assert.AreEqual(2, revue.Rejets(1).IndexEntree)
            Assert.AreEqual("source", revue.Lignes(0).CleSource)
            Assert.AreEqual("SOURCE", revue.Rejets(1).CleSource)
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

    <TestMethod>
    Public Sub DocumentsAnalyses_CorrespondanceExacteEtProvenanceConservee()
        Dim facture = FactureFictive()
        facture.IdDoc = 42
        Dim cheque = ChequeFictif()
        Dim adapter As New PropositionAnalyseAdapter()
        Dim resultat = adapter.ProposerDocuments({facture, cheque}, {
            New PropositionDocumentService.Entree(cheque, " C-001 ", PropositionMouvement.SensMouvement.Credit, "origine cheque"),
            New PropositionDocumentService.Entree(facture, "F-001", PropositionMouvement.SensMouvement.Debit, "origine facture")})
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual("F-001", resultat.Propositions(0).CleSource)
        Assert.AreEqual(" C-001 ", resultat.Propositions(1).CleSource)
        Assert.AreEqual("0000123", resultat.Propositions(1).Reference)
        facture.IdDoc = 999
        facture.CheminDoc = "modifie"
        Assert.AreEqual(42, resultat.Sources(0).IdDoc)
        Assert.AreEqual("chemin-fictif-inutilise", resultat.Sources(0).CheminDoc)
        Assert.AreEqual("origine facture", resultat.Sources(0).Provenance)
        Assert.AreEqual(" C-001 ", resultat.Sources(1).CleSource)
    End Sub

    <TestMethod>
    Public Sub DocumentsAnalyses_RejetteCorrespondancesAbsentesAmbiguesEtEtrangeres()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim resultat = New PropositionAnalyseAdapter().ProposerDocuments({facture, cheque}, {
            New PropositionDocumentService.Entree(facture, "a", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(facture, "b", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(FactureFictive(), "etranger", PropositionMouvement.SensMouvement.Debit)})
        Assert.AreEqual(0, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Sources.Count)
        Assert.AreEqual(3, resultat.Rejets.Count)
        Assert.AreEqual("Correspondance source ambigue.", resultat.Rejets(0).Motif)
        Assert.AreEqual("Correspondance source absente.", resultat.Rejets(1).Motif)
        Assert.AreEqual("etranger", resultat.Rejets(2).CleSource)
        Assert.AreEqual("Correspondance sans document du lot.", resultat.Rejets(2).Motif)
    End Sub

    <TestMethod>
    Public Sub DocumentsAnalyses_DoublonsEtAppelsRepetes()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim adapter As New PropositionAnalyseAdapter()
        Dim mappings = {
            New PropositionDocumentService.Entree(facture, "exact", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(cheque, "exact", PropositionMouvement.SensMouvement.Credit)}
        For repetition = 1 To 2
            Dim resultat = adapter.ProposerDocuments({facture, cheque, facture}, mappings)
            Assert.AreEqual(1, resultat.Propositions.Count)
            Assert.AreEqual(2, resultat.Rejets.Count)
            Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
            Assert.AreEqual(2, resultat.Rejets(1).IndexEntree)
            Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(0).Motif)
            Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(1).Motif)
        Next
    End Sub

    <TestMethod>
    Public Sub CorrespondancesEtrangeres_ReserventLeurCleMemeRejetees()
        Dim adapter As New PropositionAnalyseAdapter()
        Dim rejetees = adapter.ProposerDocuments(Array.Empty(Of DocumentAgumaaa)(), {
            New PropositionDocumentService.Entree(FactureFictive(), " SOURCE ", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(ChequeFictif(), "source", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(FactureFictive(), "distincte", PropositionMouvement.SensMouvement.Debit)})
        Assert.AreEqual(0, rejetees.Propositions.Count)
        Assert.AreEqual(3, rejetees.Rejets.Count)
        Assert.AreEqual("Correspondance sans document du lot.", rejetees.Rejets(0).Motif)
        Assert.AreEqual("Cle source dupliquee dans le lot.", rejetees.Rejets(1).Motif)
        Assert.AreEqual("source", rejetees.Rejets(1).CleSource)
        Assert.AreEqual("Correspondance sans document du lot.", rejetees.Rejets(2).Motif)

        Dim facture = FactureFictive()
        Dim mixte = adapter.ProposerDocuments({facture}, {
            New PropositionDocumentService.Entree(facture, "source", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(ChequeFictif(), " SOURCE ", PropositionMouvement.SensMouvement.Credit)})
        Assert.AreEqual(1, mixte.Propositions.Count)
        Assert.AreEqual(1, mixte.Rejets.Count)
        Assert.AreEqual("Cle source dupliquee dans le lot.", mixte.Rejets(0).Motif)
        Assert.AreEqual(" SOURCE ", mixte.Rejets(0).CleSource)
    End Sub

    <TestMethod>
    Public Sub DocumentsAnalyses_CollectionsNullesVidesEtElementsNuls()
        Dim adapter As New PropositionAnalyseAdapter()
        Dim vide = Array.Empty(Of PropositionDocumentService.Entree)()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() adapter.ProposerDocuments(Nothing, vide))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() adapter.ProposerDocuments({FactureFictive()}, Nothing))
        Dim resultat = adapter.ProposerDocuments(Array.Empty(Of DocumentAgumaaa)(), vide)
        Assert.AreEqual(0, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Sources.Count)
        Assert.AreEqual(0, resultat.Rejets.Count)
        resultat = adapter.ProposerDocuments(New DocumentAgumaaa() {Nothing}, New PropositionDocumentService.Entree() {Nothing})
        Assert.AreEqual(0, resultat.Propositions.Count)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual("Correspondance source absente.", resultat.Rejets(0).Motif)
        Assert.AreEqual("Correspondance sans document du lot.", resultat.Rejets(1).Motif)
    End Sub

    Private NotInheritable Class DocumentNonPrisEnCharge
        Inherits DocumentAgumaaa

        Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
            Throw New InvalidOperationException("Aucun renommage autorise dans ce test.")
        End Function
    End Class
End Class
