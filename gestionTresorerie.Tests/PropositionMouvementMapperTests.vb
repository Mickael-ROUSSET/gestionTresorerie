Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

<TestClass>
Public Class PropositionMouvementMapperTests
    Private Shared Function ExtractionFictive() As JObject
        Return JObject.Parse("{""date"":""2026-10-01"",""montant"":125.50,""libelle"":""Facture fictive"",""reference"":""TEST-001""}")
    End Function

    <TestMethod>
    Public Sub ExtractionFictiveCreeUnePropositionSansApprobationAutomatique()
        Dim extraction = ExtractionFictive()
        Dim original = extraction.DeepClone()
        Dim proposition = PropositionMouvementMapper.DepuisExtraction(
            "document-fictif-sha", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit)

        Assert.AreEqual("document-fictif-sha", proposition.CleSource)
        Assert.AreEqual("FACTURE", proposition.TypeDocument)
        Assert.AreEqual(New DateTime(2026, 10, 1), proposition.DateProposee)
        Assert.AreEqual(125.5D, proposition.Montant)
        Assert.AreEqual("Facture fictive", proposition.Libelle)
        Assert.AreEqual("TEST-001", proposition.Reference)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Debit, proposition.Sens)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Assert.IsTrue(JToken.DeepEquals(original, extraction))
    End Sub

    <TestMethod>
    Public Sub SensEstExplicitementFourniEtJamaisDeduitDuDocument()
        Dim extraction = ExtractionFictive()
        Dim credit = PropositionMouvementMapper.DepuisExtraction(
            "document-fictif-sha", "FACTURE", extraction, PropositionMouvement.SensMouvement.Credit)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Credit, credit.Sens)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, credit.Etat)
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "document-fictif-sha", "FACTURE", extraction, CType(99, PropositionMouvement.SensMouvement)))
    End Sub

    <TestMethod>
    Public Sub DatesEtMontantsAmbigusSontRefuses()
        Dim extraction = ExtractionFictive()
        extraction("date") = "01/10/2026"
        Assert.ThrowsExactly(Of FormatException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit))

        extraction("date") = "2026-10-01"
        extraction("montant") = "125,50"
        Assert.ThrowsExactly(Of FormatException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit))

        extraction.Remove("montant")
        Assert.ThrowsExactly(Of FormatException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit))

        extraction("montant") = -1
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit))
    End Sub

    <TestMethod>
    Public Sub ChampsObligatoiresEtReferenceMalTypeeSontRefuses()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", Nothing, PropositionMouvement.SensMouvement.Debit))

        Dim extraction = ExtractionFictive()
        extraction.Remove("libelle")
        Assert.ThrowsExactly(Of FormatException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit))

        extraction("libelle") = "Libelle fictif"
        extraction("reference") = 123
        Assert.ThrowsExactly(Of FormatException)(
            Sub() PropositionMouvementMapper.DepuisExtraction(
                "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit))

        extraction.Remove("reference")
        Dim proposition = PropositionMouvementMapper.DepuisExtraction(
            "source-fictive", "FACTURE", extraction, PropositionMouvement.SensMouvement.Debit)
        Assert.IsNull(proposition.Reference)
    End Sub
End Class
