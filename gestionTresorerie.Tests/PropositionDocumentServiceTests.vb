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
    Public Sub IdentifiantDocumentDuplique_RejeteMemeAvecObjetsEtClesDifferents()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        facture.IdDoc = 27
        cheque.IdDoc = 27
        Dim resultat = New PropositionDocumentService().Proposer({Entree(facture, "a"), Entree(cheque, "b")})
        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual(1, resultat.Rejets.Count)
        Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("Identifiant document duplique dans le lot.", resultat.Rejets(0).Motif)
    End Sub

    <TestMethod>
    Public Sub MemeObjetSousDeuxIdentites_RejeteSansEtatEntreAppels()
        Dim document = FactureFictive()
        Dim service As New PropositionDocumentService()
        For repetition = 1 To 2
            Dim resultat = service.Proposer({Entree(document, "a"), Entree(document, "b"), Entree(ChequeFictif(), "c")})
            Assert.AreEqual(2, resultat.Propositions.Count)
            Assert.AreEqual(1, resultat.Rejets.Count)
            Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
            Assert.AreEqual("b", resultat.Rejets(0).CleSource)
            Assert.AreEqual("Document deja present dans le lot sous une autre cle source.", resultat.Rejets(0).Motif)
        Next
    End Sub

    <TestMethod>
    Public Sub IdentifiantsNonAttribuesZero_AccepteDocumentsDistincts()
        ' IdDoc = 0 signifie non attribue : deux objets distincts restent admissibles.
        Dim premiere = FactureFictive()
        Dim seconde = FactureFictive()
        Assert.AreEqual(0, premiere.IdDoc)
        Assert.AreEqual(0, seconde.IdDoc)
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(premiere, "message-1/piece-0"),
            Entree(seconde, "message-2/piece-0")})
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual("message-1/piece-0", resultat.Propositions(0).CleSource)
        Assert.AreEqual("message-2/piece-0", resultat.Propositions(1).CleSource)
        For Each proposition In resultat.Propositions
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Next
    End Sub

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
    <TestMethod>
    Public Sub DocumentInvalideReserveSonIdentifiant_SecondRejeteEtTroisiemeConserve()
        ' Un document fictif invalide ne libere pas son IdDoc pour un autre objet du lot.
        Dim invalide = FactureFictive()
        invalide.IdDoc = 27
        invalide.montant_ttc = -1D
        Dim doublon = ChequeFictif()
        doublon.IdDoc = 27
        Dim distinct = ChequeFictif()
        distinct.IdDoc = 28

        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(invalide, "piece-invalide"),
            Entree(doublon, "piece-meme-id"),
            Entree(distinct, "piece-distincte")})

        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual("piece-distincte", resultat.Propositions(0).CleSource)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual(0, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("piece-invalide", resultat.Rejets(0).CleSource)
        Assert.AreEqual(1, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual("piece-meme-id", resultat.Rejets(1).CleSource)
        Assert.AreEqual("Identifiant document duplique dans le lot.", resultat.Rejets(1).Motif)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                        resultat.Propositions(0).Etat)
        Dim session As New PropositionSessionRevue(resultat.Propositions)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() session.MarquerApprouvee("piece-distincte"))
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() session.MarquerPersistee("piece-distincte"))
        Assert.AreEqual(-1D, invalide.montant_ttc)
        Assert.AreEqual(0, distinct.IdMvtDoc)
    End Sub

    <TestMethod>
    Public Sub DocumentInvalideSansId_ReserveSaReferenceEtIsoleLeDocumentSuivant()
        ' Meme avec IdDoc = 0, une piece invalide reserve sa reference dans ce lot.
        Dim invalide = FactureFictive()
        invalide.montant_ttc = 0D
        Assert.AreEqual(0, invalide.IdDoc)
        Dim distinct = ChequeFictif()

        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(invalide, "invalide"),
            Entree(invalide, "meme-objet-autre-cle"),
            Entree(distinct, "valide")})

        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual("valide", resultat.Propositions(0).CleSource)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual(0, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("invalide", resultat.Rejets(0).CleSource)
        Assert.AreEqual(1, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual("meme-objet-autre-cle", resultat.Rejets(1).CleSource)
        Assert.AreEqual("Document deja present dans le lot sous une autre cle source.",
                        resultat.Rejets(1).Motif)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                        resultat.Propositions(0).Etat)
        Assert.AreEqual(0D, invalide.montant_ttc)
        Assert.AreEqual(0, distinct.IdMvtDoc)
    End Sub

End Class
