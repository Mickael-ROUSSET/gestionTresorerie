Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionWorkflowTests
    ' Only in-memory constructors and pure workflow APIs are called. No forms,
    ' repositories, configuration, document loading or external services are used.
    ' Renaming is the overridable I/O boundary; static SQL/network APIs cannot
    ' be intercepted by these doubles and are deliberately never invoked.
    Private NotInheritable Class FactureFictive
        Inherits Facture

        Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
            Throw New AssertFailedException("File access is forbidden in this workflow.")
        End Function
    End Class

    Private NotInheritable Class ChequeFictif
        Inherits Cheque

        Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
            Throw New AssertFailedException("File access is forbidden in this workflow.")
        End Function
    End Class

    Private Shared Function NouvelleFacture() As Facture
        Return New FactureFictive With {.IdDoc = 101, .montant_ttc = 120.42D,
            .devise = "EUR", .date_facture = "2026-02-03", .numero_facture = "F-SYNTHETIQUE"}
    End Function

    Private Shared Function NouveauCheque() As Cheque
        Return New ChequeFictif With {.IdDoc = 102, .montant_numerique = 42.12D,
            .devise = "EUR", .dateChq = "20260203", .numero_du_cheque = "0000123"}
    End Function

    Private Shared Function Entree(document As DocumentAgumaaa, cle As String) As PropositionDocumentService.Entree
        Return New PropositionDocumentService.Entree(document, cle, PropositionMouvement.SensMouvement.Debit)
    End Function

    Private Shared Sub VerifierIdentite(proposition As PropositionMouvement, cle As String, typeDocument As String)
        Assert.AreEqual(cle, proposition.CleSource)
        Assert.AreEqual(typeDocument, proposition.TypeDocument)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
    End Sub

    Private Shared Function Corriger(proposition As PropositionMouvement) As PropositionCorrection.Resultat
        Dim confirmations As New List(Of PropositionCorrection.ConfirmationAvertissement)()
        For Each avertissement In proposition.AvertissementsValidation
            confirmations.Add(New PropositionCorrection.ConfirmationAvertissement(PropositionCorrection.Champ.Sens, avertissement))
        Next
        Return PropositionCorrection.Corriger(proposition, proposition.DateProposee.AddDays(1),
            proposition.Montant + 0.01D, proposition.Sens, "Libelle synthetique corrige",
            proposition.Reference, confirmations)
    End Function

    ' Revue is a presentation snapshot, not an input accepted by Correction.
    ' Keep the actual service proposals for Correction and SessionRevue.
    Private Shared Function Parcourir(resultat As PropositionDocumentService.Resultat) As PropositionSessionRevue
        Dim revue As New PropositionRevue(resultat)
        Dim session As New PropositionSessionRevue(resultat.Propositions)
        Assert.AreEqual(resultat.Propositions.Count, revue.Lignes.Count)
        Assert.AreEqual(resultat.Rejets.Count, revue.Rejets.Count)
        For i = 0 To resultat.Rejets.Count - 1
            Assert.AreEqual(resultat.Rejets(i).CleSource, revue.Rejets(i).CleSource)
            Assert.AreEqual(resultat.Rejets(i).IndexEntree, revue.Rejets(i).IndexEntree)
            Assert.AreEqual(resultat.Rejets(i).Motif, revue.Rejets(i).Motif)
        Next
        For i = 0 To resultat.Propositions.Count - 1
            Dim originale = resultat.Propositions(i)
            Dim ligne = revue.Lignes(i)
            VerifierIdentite(originale, ligne.CleSource, ligne.TypeDocument)
            Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, ligne.Statut)
            Assert.AreEqual(originale.Reference, ligne.Reference)
            Assert.AreEqual(originale.Montant, ligne.Montant)
            Assert.AreEqual(originale.DateProposee, ligne.DateProposee)
            Assert.AreEqual(1, originale.AvertissementsValidation.Count)
            Assert.IsFalse(session.Prevalider(originale.CleSource, 100 + i, Array.Empty(Of String)(), False).EstPrete)

            Dim correction = Corriger(originale)
            Assert.AreSame(originale, correction.Avant)
            VerifierIdentite(correction.Avant, ligne.CleSource, ligne.TypeDocument)
            VerifierIdentite(correction.Proposition, ligne.CleSource, ligne.TypeDocument)
            Assert.AreEqual(originale.Reference, correction.Proposition.Reference)
            Assert.AreEqual(0, correction.Proposition.AvertissementsValidation.Count)
            Assert.IsTrue(PropositionPrevalidation.Evaluer(correction.Proposition, 100 + i,
                Array.Empty(Of String)(), False).EstPrete)
            session.AppliquerCorrection(originale.CleSource, correction)
            Assert.AreSame(correction.Proposition, session.Propositions(originale.CleSource))
            Assert.IsTrue(session.Prevalider(originale.CleSource, 100 + i, Array.Empty(Of String)(), False).EstPrete)
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(originale.CleSource))
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(originale.CleSource))
            VerifierIdentite(session.Propositions(originale.CleSource), ligne.CleSource, ligne.TypeDocument)
            Assert.AreEqual(originale.Montant, ligne.Montant, "The review remains a snapshot.")
        Next
        Assert.AreEqual(resultat.Propositions.Count, session.Audits.Count)
        Return session
    End Function

    <TestMethod>
    Public Sub LotMixte_IdentitesEtZerosInitiauxConserves_JamaisApprouve()
        Dim facture = NouvelleFacture()
        Dim cheque = NouveauCheque()
        ' Adapter delegates to PropositionDocumentService; do not map documents twice.
        Dim resultat = New PropositionAnalyseAdapter().Proposer({Entree(facture, "facture-synthetique"),
            Entree(cheque, "cheque-synthetique")})
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(2, resultat.Propositions.Count)
        VerifierIdentite(resultat.Propositions(0), "facture-synthetique", "FACTURE")
        VerifierIdentite(resultat.Propositions(1), "cheque-synthetique", "CHEQUE")
        Dim session = Parcourir(resultat)
        Assert.AreEqual("0000123", session.Propositions("cheque-synthetique").Reference)
        Assert.AreEqual(120.42D, facture.montant_ttc)
        Assert.AreEqual("2026-02-03", facture.date_facture)
        Assert.AreEqual("F-SYNTHETIQUE", facture.numero_facture)
        Assert.AreEqual(42.12D, cheque.montant_numerique)
        Assert.AreEqual("20260203", cheque.dateChq)
        Assert.AreEqual("0000123", cheque.numero_du_cheque)
        Assert.AreEqual(0, facture.IdMvtDoc)
        Assert.AreEqual(0, cheque.IdMvtDoc)
    End Sub

    <TestMethod>
    Public Sub MontantsEtDatesInvalides_RejetesSansPerdreLesSourcesValides()
        Dim zero = NouvelleFacture()
        zero.montant_ttc = 0D
        Dim negatif = NouveauCheque()
        negatif.montant_numerique = -0.01D
        Dim dateImpossible = NouvelleFacture()
        dateImpossible.date_facture = "2026-02-30"
        Dim dateAbsente = NouveauCheque()
        dateAbsente.dateChq = Nothing
        Dim resultat = New PropositionAnalyseAdapter().Proposer({Entree(zero, "zero"),
            Entree(negatif, "negatif"), Entree(dateImpossible, "date-impossible"),
            Entree(dateAbsente, "date-absente"), Entree(NouvelleFacture(), "valide")})
        Assert.AreEqual(4, resultat.Rejets.Count)
        Dim cles = {"zero", "negatif", "date-impossible", "date-absente"}
        For i = 0 To cles.Length - 1
            Assert.AreEqual(cles(i), resultat.Rejets(i).CleSource)
            Assert.AreEqual(i, resultat.Rejets(i).IndexEntree)
            Assert.IsFalse(String.IsNullOrWhiteSpace(resultat.Rejets(i).Motif))
        Next
        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.IsTrue(Parcourir(resultat).Propositions.ContainsKey("valide"))
    End Sub

    <TestMethod>
    Public Sub CorrectionsInvalides_EchecAtomiqueEtPrevalidationBloquante()
        Dim resultat = New PropositionAnalyseAdapter().Proposer({Entree(NouvelleFacture(), "f")})
        Dim session = Parcourir(resultat)
        Dim actuelle = session.Propositions("f")
        For Each montant In {0D, -1D}
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
                Sub() PropositionCorrection.Corriger(actuelle, actuelle.DateProposee, montant,
                    actuelle.Sens, actuelle.Libelle, actuelle.Reference))
        Next
        For Each dateInvalide As DateTime? In New DateTime?() {Nothing, DateTime.MinValue}
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() PropositionCorrection.Corriger(actuelle, dateInvalide, actuelle.Montant,
                    actuelle.Sens, actuelle.Libelle, actuelle.Reference))
        Next
        Dim diagnostic = PropositionPrevalidation.Evaluer(0D, Nothing, actuelle.CleSource,
            101, actuelle.Reference, False, Array.Empty(Of String)(), Array.Empty(Of String)(), actuelle.Sens)
        Assert.IsFalse(diagnostic.EstPrete)
        CollectionAssert.AreEquivalent(New PropositionPrevalidation.Anomalie() {
            PropositionPrevalidation.Anomalie.MontantNonPositif, PropositionPrevalidation.Anomalie.DateInvalide},
            New List(Of PropositionPrevalidation.Anomalie)(diagnostic.Anomalies))
        Assert.AreSame(actuelle, session.Propositions("f"))
        Assert.AreEqual(1, session.Audits.Count)
        VerifierIdentite(actuelle, "f", "FACTURE")
    End Sub

    <TestMethod>
    Public Sub ReferenceAmbigue_OuIdentiteDejaUtilisee_BloquentMemeApresCorrection()
        Dim session = Parcourir(New PropositionAnalyseAdapter().Proposer({Entree(NouveauCheque(), "cheque")}))
        Dim actuelle = session.Propositions("cheque")
        Dim ambigu = session.Prevalider("cheque", 102, Array.Empty(Of String)())
        Assert.IsFalse(ambigu.EstPrete)
        CollectionAssert.AreEqual(New PropositionPrevalidation.Anomalie() {
            PropositionPrevalidation.Anomalie.ReferenceAmbigue},
            New List(Of PropositionPrevalidation.Anomalie)(ambigu.Anomalies))
        Dim reutilisee = session.Prevalider("cheque", 102, New String() {" CHEQUE "}, False)
        Assert.IsFalse(reutilisee.EstPrete)
        CollectionAssert.Contains(New List(Of PropositionPrevalidation.Anomalie)(reutilisee.Anomalies),
            PropositionPrevalidation.Anomalie.IdentiteSourceReutilisee)
        Assert.AreSame(actuelle, session.Propositions("cheque"))
        VerifierIdentite(actuelle, "cheque", "CHEQUE")
        Assert.AreEqual(1, session.Audits.Count)
    End Sub

    <TestMethod>
    Public Sub Doublons_PremiereOccurrenceReserveLaCleMemeInvalide()
        Dim invalide = NouvelleFacture()
        invalide.montant_ttc = -1D
        Dim resultat = New PropositionAnalyseAdapter().Proposer({Entree(NouvelleFacture(), "unique"),
            Entree(NouveauCheque(), "unique"), Entree(invalide, "invalide"),
            Entree(NouveauCheque(), "invalide")})
        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual(3, resultat.Rejets.Count)
        For Each index In {0, 2}
            Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(index).Motif)
        Next
        Dim session = Parcourir(resultat)
        Assert.AreEqual(1, session.Propositions.Count)
        Assert.IsFalse(session.Propositions.ContainsKey("invalide"))
        Dim originale = resultat.Propositions(0)
        Assert.ThrowsExactly(Of ArgumentException)(Sub()
                                                      Dim refusee = New PropositionSessionRevue({originale, originale})
                                                  End Sub)

        ' Service keys are Ordinal; the session must reject normalized collisions.
        Dim ambigu = New PropositionAnalyseAdapter().Proposer({Entree(NouvelleFacture(), "source"),
            Entree(NouveauCheque(), " SOURCE ")})
        Assert.AreEqual(2, ambigu.Propositions.Count)
        Assert.ThrowsExactly(Of ArgumentException)(Sub()
                                                      Dim refusee = New PropositionSessionRevue(ambigu.Propositions)
                                                  End Sub)
    End Sub
End Class
