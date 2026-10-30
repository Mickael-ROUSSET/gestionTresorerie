Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailDocumentPropositionAdapterTests
    Private Shared Function FactureFictive() As Facture
        Return New Facture With {.IdDoc = 501, .montant_ttc = 25.5D,
            .devise = "EUR", .date_facture = "2026-02-03", .numero_facture = "F-FAUX"}
    End Function

    Private Shared Function ChequeFictif() As Cheque
        Return New Cheque With {.IdDoc = 502, .montant_numerique = 12.75D,
            .devise = "EUR", .dateChq = "20260203", .numero_du_cheque = "0000456"}
    End Function

    <TestMethod>
    Public Sub PiecesFictives_IndicesDOrigineDistincts_RevueSansApprobation()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim pieces = {
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, facture, PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "FAUX@EXAMPLE.INVALID", 12UI, 34UI, 2, cheque, PropositionMouvement.SensMouvement.Credit)}
        ' L'indice 1 correspond a une piece non analysee : les indices ne sont pas renumerotes.
        Dim revue = New GmailDocumentPropositionAdapter().PreparerRevue(pieces)
        Assert.AreEqual(0, revue.Rejets.Count)
        Assert.AreEqual(2, revue.Lignes.Count)
        Assert.AreEqual(GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, 0),
                        revue.Lignes(0).CleSource)
        Assert.AreEqual(GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, 2),
                        revue.Lignes(1).CleSource)
        Assert.AreNotEqual(revue.Lignes(0).CleSource, revue.Lignes(1).CleSource)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Debit, revue.Lignes(0).Sens)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Credit, revue.Lignes(1).Sens)
        For Each ligne In revue.Lignes
            Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, ligne.Statut)
        Next
        Assert.AreEqual(0, facture.IdMvtDoc)
        Assert.AreEqual(0, cheque.IdMvtDoc)
    End Sub

    <TestMethod>
    Public Sub DoublonEtPieceAbsente_RejetsIndexesSansBloquerLesAutres()
        Dim facture = FactureFictive()
        Dim pieces As GmailDocumentPropositionAdapter.PieceAnalysee() = {
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, facture, PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, FactureFictive(), PropositionMouvement.SensMouvement.Credit),
            Nothing,
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 2, ChequeFictif(), PropositionMouvement.SensMouvement.Debit)}
        Dim resultat = New GmailDocumentPropositionAdapter().Proposer(pieces)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(0).Motif)
        Assert.AreEqual(2, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual(0, facture.IdMvtDoc)
        For Each proposition In resultat.Propositions
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            proposition.Etat)
        Next
    End Sub

    <TestMethod>
    Public Sub LotAbsentEtLotVide_AucunAccesExterne()
        Dim adapter As New GmailDocumentPropositionAdapter()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() adapter.PreparerRevue(Nothing))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() adapter.Proposer(Nothing))
        Dim revue = adapter.PreparerRevue(Array.Empty(Of GmailDocumentPropositionAdapter.PieceAnalysee)())
        Assert.AreEqual(0, revue.Lignes.Count)
        Assert.AreEqual(0, revue.Rejets.Count)
    End Sub

    <TestMethod>
    Public Sub PiecesGmailFictives_CorrectionEtPrevalidation_NeValentJamaisApprobation()
        Dim facture = FactureFictive()
        Dim cheque = ChequeFictif()
        Dim pieces = {
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, facture, PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 2, cheque, PropositionMouvement.SensMouvement.Credit)}
        Dim resultat = New GmailDocumentPropositionAdapter().Proposer(pieces)
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Dim session As New PropositionSessionRevue(resultat.Propositions)

        For i = 0 To resultat.Propositions.Count - 1
            Dim originale = resultat.Propositions(i)
            Dim cle = GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, If(i = 0, 0, 2))
            Assert.AreEqual(cle, originale.CleSource)
            Assert.IsFalse(session.Prevalider(cle, 501 + i, Array.Empty(Of String)(), False).EstPrete)

            ' Une confirmation explicite de chaque avertissement est requise.
            Dim confirmations As New System.Collections.Generic.List(Of PropositionCorrection.ConfirmationAvertissement)()
            For Each avertissement In originale.AvertissementsValidation
                confirmations.Add(New PropositionCorrection.ConfirmationAvertissement(
                    PropositionCorrection.Champ.Sens, avertissement))
            Next
            Dim correction = PropositionCorrection.Corriger(originale,
                originale.DateProposee.AddDays(1), originale.Montant + 1D,
                originale.Sens, "Document fictif corrige", originale.Reference, confirmations)
            session.AppliquerCorrection(cle, correction)

            Assert.AreSame(correction.Proposition, session.Propositions(cle))
            Assert.IsTrue(session.Prevalider(cle, 501 + i, Array.Empty(Of String)(), False).EstPrete)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            session.Propositions(cle).Etat)
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(cle))
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(cle))
        Next
        Assert.AreEqual(2, session.Audits.Count)
        Assert.AreEqual(0, facture.IdMvtDoc)
        Assert.AreEqual(0, cheque.IdMvtDoc)
        Assert.AreEqual(25.5D, facture.montant_ttc)
        Assert.AreEqual(12.75D, cheque.montant_numerique)
    End Sub

    <TestMethod>
    Public Sub DeuxComptesEtDeuxMessagesMemeUid_PiecesSansCollision()
        ' Les identifiants IMAP sont locaux a chaque compte et a chaque message.
        Dim pieces = {
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "alpha@example.invalid", 12UI, 34UI, 0, FactureFictive(), PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "beta@example.invalid", 12UI, 34UI, 0, FactureFictive(), PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "alpha@example.invalid", 12UI, 35UI, 0, FactureFictive(), PropositionMouvement.SensMouvement.Debit)}
        Dim adapter As New GmailDocumentPropositionAdapter()
        Dim premiere = adapter.Proposer(pieces)
        Dim reprise = adapter.Proposer(pieces)
        Assert.AreEqual(0, premiere.Rejets.Count)
        Assert.AreEqual(3, premiere.Propositions.Count)
        Assert.AreEqual(0, reprise.Rejets.Count)
        Assert.AreEqual(3, reprise.Propositions.Count)
        Dim cles As New System.Collections.Generic.HashSet(Of String)(StringComparer.Ordinal)
        For i = 0 To premiere.Propositions.Count - 1
            Dim proposition = premiere.Propositions(i)
            Assert.IsTrue(cles.Add(proposition.CleSource))
            Assert.AreEqual(proposition.CleSource, reprise.Propositions(i).CleSource)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Next
        Dim session As New PropositionSessionRevue(premiere.Propositions)
        For Each cle In cles
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(cle))
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(cle))
        Next
    End Sub

    <TestMethod>
    Public Sub DocumentInvalideDansLot_ConserveRejetsEtPropositionsValides()
        Dim valide = FactureFictive()
        Dim deviseInvalide = FactureFictive()
        deviseInvalide.devise = "USD"
        Dim dateInvalide = FactureFictive()
        dateInvalide.date_facture = "date-invalide"
        Dim pieces = {
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, valide, PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 1, deviseInvalide, PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 2, dateInvalide, PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 3, ChequeFictif(), PropositionMouvement.SensMouvement.Credit)}
        Dim resultat = New GmailDocumentPropositionAdapter().Proposer(pieces)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual(2, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual(pieces(1).CleSource, resultat.Rejets(0).CleSource)
        Assert.AreEqual(pieces(2).CleSource, resultat.Rejets(1).CleSource)
        Assert.AreEqual(pieces(0).CleSource, resultat.Propositions(0).CleSource)
        Assert.AreEqual(pieces(3).CleSource, resultat.Propositions(1).CleSource)
        For Each proposition In resultat.Propositions
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Next
        Assert.AreEqual(0, valide.IdMvtDoc)
        Assert.AreEqual(0, deviseInvalide.IdMvtDoc)
        Assert.AreEqual(0, dateInvalide.IdMvtDoc)
        Assert.AreEqual("USD", deviseInvalide.devise)
        Assert.AreEqual("date-invalide", dateInvalide.date_facture)
    End Sub

    <TestMethod>
    Public Sub PieceInvalideReserveSaCle_DoublonRejeteSansContournement()
        ' Une piece invalide ne doit pas permettre de reutiliser sa cle source
        ' pour une autre piece du meme lot ; les autres pieces restent traitables.
        Dim pieces = {
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, Nothing,
                PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 0, FactureFictive(),
                PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentPropositionAdapter.PieceAnalysee(
                "faux@example.invalid", 12UI, 34UI, 1, ChequeFictif(),
                PropositionMouvement.SensMouvement.Credit)}
        Dim resultat = New GmailDocumentPropositionAdapter().Proposer(pieces)
        Assert.AreEqual(2, resultat.Rejets.Count)
        Assert.AreEqual(0, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("Document absent.", resultat.Rejets(0).Motif)
        Assert.AreEqual(1, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(1).Motif)
        Assert.AreEqual(pieces(0).CleSource, resultat.Rejets(0).CleSource)
        Assert.AreEqual(pieces(1).CleSource, resultat.Rejets(1).CleSource)
        Assert.AreEqual(1, resultat.Propositions.Count)
        Assert.AreEqual(pieces(2).CleSource, resultat.Propositions(0).CleSource)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                        resultat.Propositions(0).Etat)
        Dim session As New PropositionSessionRevue(resultat.Propositions)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() session.MarquerApprouvee(pieces(2).CleSource))
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() session.MarquerPersistee(pieces(2).CleSource))
    End Sub
End Class
