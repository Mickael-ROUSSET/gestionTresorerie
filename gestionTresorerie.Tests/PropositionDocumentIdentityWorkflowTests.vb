Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Parcours purement en memoire avec une facture et une provenance fictives.</summary>
<TestClass>
Public Class PropositionDocumentIdentityWorkflowTests
    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub IdentiteFictiveEtPrevalidation_NeValentJamaisApprobation()
        Dim facture As New Facture With {
            .IdDoc = 451, .montant_ttc = 120.42D, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-SYNTHETIQUE-451"}
        Dim cle = "gmail-simule:piece-451"
        Dim provenance = "Gmail simule / piece 451"
        Dim entree As New PropositionDocumentService.Entree(
            facture, cle, PropositionMouvement.SensMouvement.Debit, provenance)

        Dim resultat = New PropositionAnalyseAdapter().Proposer({entree})
        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Dim revue As New PropositionRevue(resultat)
        Assert.AreEqual(1, revue.Lignes.Count)
        Assert.AreEqual(cle, revue.Lignes(0).CleSource)
        Assert.AreEqual(451, revue.Lignes(0).IdDocumentSource)
        Assert.AreEqual(provenance, revue.Lignes(0).Provenance)
        Assert.IsTrue(resultat.CorrespondanceIdentiteDocumentaire(cle, 451, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(cle, 451, "Gmail simule / autre piece"))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(cle, 452, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("autre-cle", 451, provenance))

        Dim session As New PropositionSessionRevue(resultat.Propositions)
        Dim originale = session.Propositions(cle)
        Dim confirmations As New List(Of PropositionCorrection.ConfirmationAvertissement)()
        For Each avertissement In originale.AvertissementsValidation
            confirmations.Add(New PropositionCorrection.ConfirmationAvertissement(
                PropositionCorrection.Champ.Sens, avertissement))
        Next
        Dim correction = PropositionCorrection.Corriger(
            originale, originale.DateProposee, originale.Montant, originale.Sens,
            originale.Libelle, originale.Reference, confirmations)
        session.AppliquerCorrection(cle, correction)
        Assert.IsTrue(session.Prevalider(cle, 451, Array.Empty(Of String)(), False).EstPrete)

        ' La correspondance en memoire et la prevalidation ne prouvent pas
        ' l'authenticite externe et ne peuvent ni approuver ni persister.
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(cle))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(cle))
        Assert.AreEqual(0, facture.IdMvtDoc)
        Assert.AreEqual(120.42D, facture.montant_ttc)
        Assert.AreEqual("F-SYNTHETIQUE-451", facture.numero_facture)
        Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)
    End Sub
End Class
