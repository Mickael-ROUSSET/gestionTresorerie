Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SessionCorrectionPropositionsTests
    Private Shared Function LotFictif() As PropositionDocumentService.Resultat
        Dim facture As New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
        Dim cheque As New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
            .dateChq = "20260204", .numero_du_cheque = "0000123"}
        Return New PropositionDocumentService().Proposer({
            New PropositionDocumentService.Entree(facture, "facture-fictive", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(cheque, "cheque-fictif", PropositionMouvement.SensMouvement.Credit),
            New PropositionDocumentService.Entree(Nothing, "rejete", PropositionMouvement.SensMouvement.Debit)})
    End Function

    <TestMethod>
    Public Sub LotFictif_CorrectionSelectionneeNeModifiePasLesAutresEtResteEnAttente()
        Dim lot = LotFictif()
        Dim session As New SessionCorrectionPropositions(lot)
        Assert.AreEqual(2, session.Propositions.Count)
        Assert.AreEqual(1, lot.Rejets.Count)
        Dim original = session.Selectionner("facture-fictive")
        Dim autre = session.Selectionner("cheque-fictif")
        Dim nouvelleDate = New DateTime(2026, 10, 29, 14, 5, 9).AddTicks(1234560)

        Dim correction = session.Corriger("facture-fictive", nouvelleDate, 99.01D,
            PropositionMouvement.SensMouvement.Credit, "Facture corrigée fictive", "F-CORRIGEE")

        Assert.AreSame(original, correction.Avant)
        Assert.AreSame(correction.Proposition, session.Selectionner("facture-fictive"))
        Assert.AreNotSame(original, correction.Proposition)
        Assert.AreEqual("facture-fictive", correction.Proposition.CleSource)
        Assert.AreEqual(nouvelleDate, correction.Proposition.DateProposee)
        Assert.AreEqual(99.01D, correction.Proposition.Montant)
        Assert.AreEqual("F-CORRIGEE", correction.Proposition.Reference)
        Assert.AreEqual(120.42D, original.Montant)
        Assert.AreEqual("F-FICTIVE", original.Reference)
        Assert.AreSame(autre, session.Selectionner("cheque-fictif"))
        Assert.AreEqual("0000123", autre.Reference)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                        correction.Proposition.Etat)
        Assert.AreEqual(original.AvertissementsValidation.Count,
                        correction.Proposition.AvertissementsValidation.Count)
        Assert.AreEqual(120.42D, lot.Propositions(0).Montant)
        Assert.AreEqual("F-FICTIVE", lot.Propositions(0).Reference)
    End Sub

    <TestMethod>
    Public Sub CleInconnueOuCorrectionInvalide_NeChangePasLaSession()
        Dim session As New SessionCorrectionPropositions(LotFictif())
        Dim avant = session.Selectionner("facture-fictive")
        Assert.ThrowsExactly(Of KeyNotFoundException)(
            Sub() session.Selectionner("absente"))
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() session.Selectionner(" "))
        Assert.ThrowsExactly(Of KeyNotFoundException)(
            Sub() session.Corriger("absente", avant.DateProposee, 10D, avant.Sens, "Fictif", Nothing))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() session.Corriger("facture-fictive", avant.DateProposee, 0D,
                                  avant.Sens, "Fictif", Nothing))
        Assert.AreSame(avant, session.Selectionner("facture-fictive"))
        Assert.AreEqual(2, session.Propositions.Count)
    End Sub

    <TestMethod>
    Public Sub InstantaneEtRelecture_NePermettentPasDeModifierLaListe()
        Dim session As New SessionCorrectionPropositions(LotFictif())
        Dim avant = session.Propositions
        Assert.ThrowsExactly(Of NotSupportedException)(
            Sub() DirectCast(avant, IList(Of PropositionMouvement)).Clear())
        Dim facture = session.Selectionner("facture-fictive")
        session.Corriger("facture-fictive", facture.DateProposee, 33D,
                        facture.Sens, facture.Libelle, facture.Reference)
        Assert.AreSame(facture, avant(0))
        Assert.AreEqual(33D, session.Propositions(0).Montant)
        Assert.AreSame(avant(1), session.Propositions(1))
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim invalide As New SessionCorrectionPropositions(Nothing)
            End Sub)
    End Sub
End Class
