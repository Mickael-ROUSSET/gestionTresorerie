Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionDocumentServiceTests
    Private Shared Function FactureFictive() As Facture
        Return New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-001"}
    End Function

    Private Shared Function ChequeFictif() As Cheque
        Return New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
            .dateChq = "20260203", .numero_du_cheque = "0000123"}
    End Function

    Private Shared Function Entree(document As DocumentAgumaaa, cle As String,
                                   Optional sens As PropositionMouvement.SensMouvement = PropositionMouvement.SensMouvement.Debit) As PropositionDocumentService.Entree
        Return New PropositionDocumentService.Entree(document, cle, sens)
    End Function

    <TestMethod>
    Public Sub LotMixte_RetourneDesPropositionsNonApprouveesSansModifierLesDocuments()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(facture, "f"), Entree(cheque, "c", PropositionMouvement.SensMouvement.Credit)})
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual("FACTURE", resultat.Propositions(0).TypeDocument)
        Assert.AreEqual("CHEQUE", resultat.Propositions(1).TypeDocument)
        Assert.AreEqual(120.42D, resultat.Propositions(0).Montant)
        Assert.AreEqual(42.12D, resultat.Propositions(1).Montant)
        Assert.AreEqual("0000123", resultat.Propositions(1).Reference)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Credit, resultat.Propositions(1).Sens)
        For Each proposition In resultat.Propositions
            Assert.AreEqual(New DateTime(2026, 2, 3), proposition.DateProposee)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
            Assert.AreEqual(1, proposition.AvertissementsValidation.Count)
        Next
        Assert.AreEqual(120.42D, facture.montant_ttc)
        Assert.AreEqual("2026-02-03", facture.date_facture)
        Assert.AreEqual("F-001", facture.numero_facture)
        Assert.AreEqual(42.12D, cheque.montant_numerique)
        Assert.AreEqual("20260203", cheque.dateChq)
        Assert.AreEqual("0000123", cheque.numero_du_cheque)
    End Sub

    <TestMethod>
    Public Sub ValidationsInvalides_NempechentPasLesDocumentsSuivants()
        Dim dateInvalide = FactureFictive()
        dateInvalide.date_facture = "invalide"
        Dim deviseInvalide = ChequeFictif()
        deviseInvalide.devise = "USD"
        Dim montantInvalide = FactureFictive()
        montantInvalide.montant_ttc = 0D
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(FactureFictive(), "avant"), Entree(dateInvalide, "date"),
            Entree(deviseInvalide, "devise"), Entree(montantInvalide, "montant"),
            Entree(ChequeFictif(), "sens", CType(99, PropositionMouvement.SensMouvement)),
            Entree(Nothing, "absent"), Nothing, Entree(FactureFictive(), " "),
            Entree(ChequeFictif(), "apres")})
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual("avant", resultat.Propositions(0).CleSource)
        Assert.AreEqual("apres", resultat.Propositions(1).CleSource)
        Assert.AreEqual(7, resultat.Rejets.Count)
        For i As Integer = 0 To resultat.Rejets.Count - 1
            Assert.AreEqual(i + 1, resultat.Rejets(i).IndexEntree)
            Assert.IsFalse(String.IsNullOrWhiteSpace(resultat.Rejets(i).Motif))
        Next
        Assert.AreEqual("date", resultat.Rejets(0).CleSource)
    End Sub

    <TestMethod>
    Public Sub Doublons_ReservesParPremiereOccurrenceMemeInvalide()
        Dim invalide = FactureFictive()
        invalide.montant_ttc = -1D
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(FactureFictive(), "source"), Entree(ChequeFictif(), "source"),
            Entree(invalide, "erreur"), Entree(ChequeFictif(), "erreur"),
            Entree(ChequeFictif(), "SOURCE")})
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual("FACTURE", resultat.Propositions(0).TypeDocument)
        Assert.AreEqual("SOURCE", resultat.Propositions(1).CleSource)
        Assert.AreEqual(3, resultat.Rejets.Count)
        Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(0).Motif)
        Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(2).Motif)
    End Sub

    <TestMethod>
    Public Sub AppelsRepetes_SansEtatNiDoublonsSupplementaires()
        Dim service As New PropositionDocumentService()
        Dim lot = {Entree(FactureFictive(), "f"), Entree(ChequeFictif(), "c"), Entree(ChequeFictif(), "f")}
        Dim premier = service.Proposer(lot)
        Dim second = service.Proposer(lot)
        Assert.AreEqual(2, second.Propositions.Count)
        Assert.AreEqual(1, second.Rejets.Count)
        For i As Integer = 0 To premier.Propositions.Count - 1
            Dim a = premier.Propositions(i)
            Dim b = second.Propositions(i)
            Assert.AreNotSame(a, b)
            Assert.AreEqual(a.CleSource, b.CleSource)
            Assert.AreEqual(a.TypeDocument, b.TypeDocument)
            Assert.AreEqual(a.Montant, b.Montant)
            Assert.AreEqual(a.DateProposee, b.DateProposee)
            Assert.AreEqual(a.Sens, b.Sens)
            Assert.AreEqual(a.Reference, b.Reference)
            Assert.AreEqual(a.Etat, b.Etat)
            Assert.AreEqual(a.AvertissementsValidation(0), b.AvertissementsValidation(0))
        Next
        Assert.AreEqual(premier.Rejets(0).IndexEntree, second.Rejets(0).IndexEntree)
        Assert.AreEqual(premier.Rejets(0).CleSource, second.Rejets(0).CleSource)
        Assert.AreEqual(premier.Rejets(0).Motif, second.Rejets(0).Motif)
    End Sub

    <TestMethod>
    Public Sub ParcoursFictif_ConversionExigeConfirmationDuSensEtApprobationHumaine()
        ' Parcours strictement en mémoire : aucune connexion Gmail ou écriture comptable.
        Dim facture = FactureFictive()
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(facture, "piece-fictive-001")})
        Dim revue As New PropositionRevue(resultat)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(1, revue.Lignes.Count)
        Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)

        Dim proposition = resultat.Propositions(0)
        Assert.AreEqual(1, proposition.AvertissementsValidation.Count)
        ' L'approbation seule ne contourne pas l'avertissement sur le sens.
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(
                proposition, 42, Array.Empty(Of String)(), True, False))

        ' Confirmer le sens ne vaut pas approbation de la proposition.
        Dim sensConfirme = PropositionMouvementMapper.Mapper(
            facture, "piece-fictive-001", PropositionMouvement.SensMouvement.Debit, True)
        Assert.AreEqual(0, sensConfirme.AvertissementsValidation.Count)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(
                sensConfirme, 42, Array.Empty(Of String)(), False, False))
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(
                sensConfirme, 42, Array.Empty(Of String)(), True, True))

        ' Les deux confirmations explicites produisent uniquement un objet en mémoire.
        Dim mouvement = PropositionMouvementValidation.Convertir(
            sensConfirme, 42, Array.Empty(Of String)(), True, False)
        Assert.AreEqual(120.42D, mouvement.Montant)
        Assert.AreEqual("F-001", mouvement.reference)
        Assert.AreEqual(42, mouvement.idDoc)
        Assert.IsFalse(mouvement.Sens)
        Assert.IsFalse(mouvement.Etat)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, sensConfirme.Etat)
        Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)
        Assert.AreEqual("F-001", facture.numero_facture)
        Assert.AreEqual(120.42D, facture.montant_ttc)
    End Sub

    <TestMethod>
    Public Sub ParcoursChequeFictif_RefuseIdentiteDejaUtiliseeEtConserveLeDocument()
        ' Aucune connexion Gmail ni écriture en base : le mouvement reste en mémoire.
        Dim cheque = ChequeFictif()
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(cheque, "piece-cheque-fictive-002", PropositionMouvement.SensMouvement.Credit)})
        Dim revue As New PropositionRevue(resultat)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(1, revue.Lignes.Count)
        Assert.AreEqual(1, resultat.Propositions(0).AvertissementsValidation.Count)

        Dim confirme = PropositionMouvementMapper.Mapper(
            cheque, "piece-cheque-fictive-002", PropositionMouvement.SensMouvement.Credit, True)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(
                confirme, 43, New String() {"piece-cheque-fictive-002"}, True, False))

        Dim mouvement = PropositionMouvementValidation.Convertir(
            confirme, 43, Array.Empty(Of String)(), True, False)
        Assert.AreEqual(42.12D, mouvement.Montant)
        Assert.AreEqual("0000123", mouvement.reference)
        Assert.AreEqual("CHEQUE", mouvement.TypeMouvement)
        Assert.AreEqual(43, mouvement.idDoc)
        Assert.IsTrue(mouvement.Sens)
        Assert.IsFalse(mouvement.Etat)
        Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, confirme.Etat)
        Assert.AreEqual("0000123", cheque.numero_du_cheque)
        Assert.AreEqual(42.12D, cheque.montant_numerique)
    End Sub

    <TestMethod>
    Public Sub LotVideAccepte_LotAbsentRejete()
        Dim service As New PropositionDocumentService()
        Dim resultat = service.Proposer(Array.Empty(Of PropositionDocumentService.Entree)())
        Assert.AreEqual(0, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() service.Proposer(Nothing))
    End Sub
End Class
