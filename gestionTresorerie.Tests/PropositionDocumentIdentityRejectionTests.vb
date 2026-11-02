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
End Class
