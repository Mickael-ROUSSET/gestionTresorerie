Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Tests hors ligne : une identite rejetee ne devient jamais verifiable.</summary>
<TestClass>
Public Class PropositionDocumentIdentityRejectionTests
    Private Shared Function FactureFictive(id As Integer, montant As Decimal) As Facture
        Return New Facture With {
            .IdDoc = id, .montant_ttc = montant, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
    End Function

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub RejetsEtIdentifiantsNonAttribues_NeValidentJamaisUneSource()
        Dim service As New PropositionDocumentService()
        Dim provenance = "Gmail simule / lot fictif"
        Dim resultat = service.Proposer({
            New PropositionDocumentService.Entree(FactureFictive(900, -1D),
                "rejet-initial", PropositionMouvement.SensMouvement.Debit, provenance),
            New PropositionDocumentService.Entree(FactureFictive(900, 15D),
                "reprise-id", PropositionMouvement.SensMouvement.Debit, provenance),
            New PropositionDocumentService.Entree(FactureFictive(901, 20D),
                "acceptee", PropositionMouvement.SensMouvement.Debit, provenance),
            New PropositionDocumentService.Entree(FactureFictive(0, 25D),
                "sans-id", PropositionMouvement.SensMouvement.Debit, provenance),
            New PropositionDocumentService.Entree(FactureFictive(902, 30D),
                " ACCEPTEE ", PropositionMouvement.SensMouvement.Debit, provenance)})

        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual(2, resultat.Sources.Count)
        Assert.AreEqual(3, resultat.Rejets.Count)
        Assert.AreEqual(0, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual(1, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual(4, resultat.Rejets(2).IndexEntree)
        Assert.AreEqual("Identifiant document duplique dans le lot.", resultat.Rejets(1).Motif)
        Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(2).Motif)

        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("rejet-initial", 900, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("reprise-id", 900, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(" ACCEPTEE ", 902, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("sans-id", 0, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("sans-id", 901, provenance))
        Assert.IsTrue(resultat.CorrespondanceIdentiteDocumentaire("acceptee", 901, provenance))
        For Each proposition In resultat.Propositions
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            proposition.Etat)
        Next
    End Sub

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub IdentifiantNegatif_RejeteAvantLaRevue_SansBloquerLeLot()
        Dim provenance = "Gmail simule / document fictif"
        Dim resultat = New PropositionDocumentService().Proposer({
            New PropositionDocumentService.Entree(FactureFictive(-7, 12D),
                "id-negatif", PropositionMouvement.SensMouvement.Debit, provenance),
            New PropositionDocumentService.Entree(FactureFictive(905, 19D),
                "id-valide", PropositionMouvement.SensMouvement.Debit, provenance)})

        Assert.AreEqual(1, resultat.Rejets.Count)
        Assert.AreEqual(0, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("id-negatif", resultat.Rejets(0).CleSource)
        Assert.AreEqual(provenance, resultat.Rejets(0).Provenance)
        Assert.AreEqual("Identifiant document negatif invalide.", resultat.Rejets(0).Motif)
        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual("id-valide", resultat.Propositions(0).CleSource)
        Assert.AreEqual(1, resultat.Sources.Count)
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("id-negatif", 905, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("id-negatif", -7, provenance))
        Assert.IsTrue(resultat.CorrespondanceIdentiteDocumentaire("id-valide", 905, provenance))
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                        resultat.Propositions(0).Etat)
    End Sub

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub CorrespondanceIdentite_ExigeValeursExactesEtResteUnInstantane()
        Dim facture = FactureFictive(903, 42.50D)
        Dim cle = " Piece-903 "
        Dim provenance = "Gmail simule / piece 903"
        Dim resultat = New PropositionDocumentService().Proposer({
            New PropositionDocumentService.Entree(
                facture, cle, PropositionMouvement.SensMouvement.Debit, provenance)})

        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.IsTrue(resultat.CorrespondanceIdentiteDocumentaire(cle, 903, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire("Piece-903", 903, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(" piece-903 ", 903, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(cle, 903, "gmail simule / piece 903"))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(cle, 903, " "))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(Nothing, 903, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(cle, -1, provenance))

        ' L'identite conservee en memoire ne doit pas suivre une mutation du document fictif.
        facture.IdDoc = 904
        Assert.IsTrue(resultat.CorrespondanceIdentiteDocumentaire(cle, 903, provenance))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(cle, 904, provenance))
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                        resultat.Propositions(0).Etat)
    End Sub

    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    Public Sub ProvenanceSourceAbsenteOuBlanche_InterditLaCorrespondanceMemeAvecIdPositif()
        Dim factureSansProvenance = FactureFictive(906, 12D)
        Dim factureProvenanceBlanche = FactureFictive(907, 13D)
        Dim factureAvecProvenance = FactureFictive(908, 14D)
        Dim provenance = "Gmail simule / piece 908"
        Dim resultat = New PropositionDocumentService().Proposer({
            New PropositionDocumentService.Entree(factureSansProvenance,
                "sans-provenance", PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(factureProvenanceBlanche,
                "provenance-blanche", PropositionMouvement.SensMouvement.Debit, "  "),
            New PropositionDocumentService.Entree(factureAvecProvenance,
                "avec-provenance", PropositionMouvement.SensMouvement.Debit, provenance)})

        Assert.AreEqual(3, resultat.Propositions.Count)
        Assert.AreEqual(3, resultat.Sources.Count)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(
            "sans-provenance", 906, "origine declaree apres coup"))
        Assert.IsFalse(resultat.CorrespondanceIdentiteDocumentaire(
            "provenance-blanche", 907, "  "))
        Assert.IsTrue(resultat.CorrespondanceIdentiteDocumentaire(
            "avec-provenance", 908, provenance))
        For Each proposition In resultat.Propositions
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            proposition.Etat)
        Next
        Assert.AreEqual(0, factureSansProvenance.IdMvtDoc)
        Assert.AreEqual(0, factureProvenanceBlanche.IdMvtDoc)
        Assert.AreEqual(0, factureAvecProvenance.IdMvtDoc)
    End Sub
End Class
