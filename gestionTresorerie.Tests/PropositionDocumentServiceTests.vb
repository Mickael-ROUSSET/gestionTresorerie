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
    Public Sub LotVideAccepte_LotAbsentRejete()
        Dim service As New PropositionDocumentService()
        Dim resultat = service.Proposer(Array.Empty(Of PropositionDocumentService.Entree)())
        Assert.AreEqual(0, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() service.Proposer(Nothing))
    End Sub
End Class
