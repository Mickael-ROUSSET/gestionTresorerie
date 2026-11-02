Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionRevueTests
    Private Shared Function FactureFictive() As Facture
        Return New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
    End Function

    Private Shared Function Entree(document As DocumentAgumaaa, cle As String,
                                   Optional sens As PropositionMouvement.SensMouvement = PropositionMouvement.SensMouvement.Debit) As PropositionDocumentService.Entree
        Return New PropositionDocumentService.Entree(document, cle, sens)
    End Function

    <TestMethod>
    Public Sub LotMixte_ConserveOrdreValeursEtAttenteDeRevue()
        Dim facture = FactureFictive()
        Dim cheque As New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
            .dateChq = "20260204", .numero_du_cheque = "0000123"}
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(facture, "z"), Entree(cheque, "a", PropositionMouvement.SensMouvement.Credit)})
        Dim revue As New PropositionRevue(resultat)

        Assert.AreEqual(2, revue.Lignes.Count)
        Assert.AreEqual(0, revue.Rejets.Count)
        For i As Integer = 0 To resultat.Propositions.Count - 1
            Dim source = resultat.Propositions(i)
            Dim ligne = revue.Lignes(i)
            Assert.AreEqual(source.CleSource, ligne.CleSource)
            Assert.AreEqual(source.TypeDocument, ligne.TypeDocument)
            Assert.AreEqual(source.DateProposee, ligne.DateProposee)
            Assert.AreEqual(source.Montant, ligne.Montant)
            Assert.IsTrue(ligne.Montant > 0D)
            Assert.AreEqual(source.Sens, ligne.Sens)
            Assert.AreEqual(source.Libelle, ligne.Libelle)
            Assert.AreEqual(source.Reference, ligne.Reference)
            CollectionAssert.AreEqual(New List(Of String)(source.AvertissementsValidation),
                                      New List(Of String)(ligne.AvertissementsValidation))
            Assert.AreNotSame(source.AvertissementsValidation, ligne.AvertissementsValidation)
            Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, ligne.Statut)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, source.Etat)
        Next
        Assert.AreEqual("z", revue.Lignes(0).CleSource)
        Assert.AreEqual("a", revue.Lignes(1).CleSource)
        Assert.AreEqual("0000123", revue.Lignes(1).Reference)
        Assert.AreEqual(120.42D, facture.montant_ttc)
        Assert.AreEqual("0000123", cheque.numero_du_cheque)
        facture.numero_facture = "AUTRE-VALEUR-FICTIVE"
        Assert.AreEqual("F-FICTIVE", revue.Lignes(0).Reference)
    End Sub

    <TestMethod>
    Public Sub DoublonsEtInvalides_ConserventTousLesDiagnosticsSansNouvelleProposition()
        Dim invalide = FactureFictive()
        invalide.montant_ttc = -1D
        Dim resultat = New PropositionDocumentService().Proposer({
            Entree(FactureFictive(), "source"), Entree(FactureFictive(), "source"),
            Entree(invalide, "invalide"), Entree(FactureFictive(), "invalide"),
            Nothing, Entree(FactureFictive(), "SOURCE")})
        Dim revue As New PropositionRevue(resultat)

        Assert.AreEqual(2, revue.Lignes.Count)
        Assert.AreEqual("source", revue.Lignes(0).CleSource)
        Assert.AreEqual("SOURCE", revue.Lignes(1).CleSource)
        Assert.AreEqual(4, revue.Rejets.Count)
        For i As Integer = 0 To resultat.Rejets.Count - 1
            Assert.AreEqual(i + 1, revue.Rejets(i).IndexEntree)
            Assert.AreEqual(resultat.Rejets(i).CleSource, revue.Rejets(i).CleSource)
            Assert.AreEqual(resultat.Rejets(i).Motif, revue.Rejets(i).Motif)
        Next
        Assert.AreEqual("Cle source dupliquee dans le lot.", revue.Rejets(0).Motif)
        Assert.AreEqual("Cle source dupliquee dans le lot.", revue.Rejets(2).Motif)
        Assert.IsNull(revue.Rejets(3).CleSource)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual(-1D, invalide.montant_ttc)
    End Sub

    <TestMethod>
    Public Sub CollectionsEtProprietes_SontEnLectureSeule()
        Dim revue As New PropositionRevue(New PropositionDocumentService().Proposer({
            Entree(FactureFictive(), "source"), Nothing}))
        Assert.ThrowsExactly(Of NotSupportedException)(
            Sub() DirectCast(revue.Lignes, IList(Of PropositionRevue.Ligne)).Clear())
        Assert.ThrowsExactly(Of NotSupportedException)(
            Sub() DirectCast(revue.Rejets, IList(Of PropositionRevue.DiagnosticRejet)).Clear())
        Assert.ThrowsExactly(Of NotSupportedException)(
            Sub() DirectCast(revue.Lignes(0).AvertissementsValidation, IList(Of String)).Clear())
        For Each type In {GetType(PropositionRevue), GetType(PropositionRevue.Ligne),
                          GetType(PropositionRevue.DiagnosticRejet)}
            For Each propriete In type.GetProperties()
                Assert.IsNull(propriete.GetSetMethod(), propriete.Name)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub LotVideEtLotEntierementRejete_SontRepresentables()
        Dim service As New PropositionDocumentService()
        Dim vide As New PropositionRevue(service.Proposer(Array.Empty(Of PropositionDocumentService.Entree)()))
        Assert.AreEqual(0, vide.Lignes.Count)
        Assert.AreEqual(0, vide.Rejets.Count)
        Dim rejete As New PropositionRevue(service.Proposer({Entree(Nothing, "absent")}))
        Assert.AreEqual(0, rejete.Lignes.Count)
        Assert.AreEqual(1, rejete.Rejets.Count)
        Assert.AreEqual("Document absent.", rejete.Rejets(0).Motif)
    End Sub

    <TestMethod>
    Public Sub ResultatAbsent_EstRejeteExplicitement()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub()
                                                          Dim revue As New PropositionRevue(Nothing)
                                                      End Sub)
    End Sub
End Class
