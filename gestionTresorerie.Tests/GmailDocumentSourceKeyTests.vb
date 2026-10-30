Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailDocumentSourceKeyTests
    <TestMethod>
    Public Sub DeuxPiecesDuMemeMessage_IdentitesDistinctesEtStables()
        Dim premiere = GmailDocumentSourceKey.MakeKey("FAUX@EXAMPLE.INVALID", 12UI, 34UI, 0)
        Dim seconde = GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, 1)

        Assert.AreNotEqual(premiere, seconde)
        Assert.AreEqual(premiere, GmailDocumentSourceKey.MakeKey(" faux@example.invalid ", 12UI, 34UI, 0))
        Assert.AreNotEqual(premiere, GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 35UI, 0))
        Assert.AreNotEqual(premiere, GmailDocumentSourceKey.MakeKey("faux@example.invalid", 13UI, 34UI, 0))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, -1))
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailDocumentSourceKey.MakeKey(" ", 12UI, 34UI, 0))
    End Sub

    <TestMethod>
    Public Sub PiecesFictives_MemeMessageDeuxPropositions_JamaisApprouvees()
        Dim cleFacture = GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, 0)
        Dim cleCheque = GmailDocumentSourceKey.MakeKey("faux@example.invalid", 12UI, 34UI, 1)
        Dim facture As New Facture With {.IdDoc = 101, .montant_ttc = 12.34D,
            .devise = "EUR", .date_facture = "2026-02-03", .numero_facture = "F-TEST"}
        Dim cheque As New Cheque With {.IdDoc = 102, .montant_numerique = 56.78D,
            .devise = "EUR", .dateChq = "20260203", .numero_du_cheque = "0000123"}
        Dim resultat = New PropositionAnalyseAdapter().Proposer({
            New PropositionDocumentService.Entree(facture, cleFacture, PropositionMouvement.SensMouvement.Debit),
            New PropositionDocumentService.Entree(cheque, cleCheque, PropositionMouvement.SensMouvement.Debit)})
        Assert.AreEqual(0, resultat.Rejets.Count)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Dim session As New PropositionSessionRevue(resultat.Propositions)
        For Each cle In {cleFacture, cleCheque}
            Assert.IsTrue(session.Propositions.ContainsKey(cle))
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                            session.Propositions(cle).Etat)
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(cle))
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(cle))
        Next
        Assert.AreEqual(0, session.Audits.Count)
        Assert.AreEqual(0, facture.IdMvtDoc)
        Assert.AreEqual(0, cheque.IdMvtDoc)
    End Sub
End Class
