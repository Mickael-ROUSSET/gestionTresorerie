Option Strict On
Option Explicit On

Imports System
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailDocumentExtractionAdapterTests
    Private Const JsonFacture As String =
        "{""type_document"":""Facture"",""donnees_extraites"":{""identite"":{""fournisseur_nom"":""Fournisseur fictif""},""transaction"":{""montant_ttc"":""42.50"",""devise"":""EUR"",""date_facture"":""2026-04-01"",""numero_facture"":""FAUX-1""}}}"
    Private Const JsonCheque As String =
        "{""type_document"":""Cheque"",""donnees_extraites"":{""identite"":{""emetteur_numero_cheque"":""0000789""},""transaction"":{""montant_numerique"":""15.25"",""devise"":""EUR"",""date_emission"":""20260402""}}}"

    Private Shared Sub SurThreadSta(action As Action)
        Dim erreur As Exception = Nothing
        Dim thread As New Thread(Sub()
                                     Try
                                         action()
                                     Catch ex As Exception
                                         erreur = ex
                                     End Try
                                 End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        thread.Join()
        If erreur IsNot Nothing Then ExceptionDispatchInfo.Capture(erreur).Throw()
    End Sub

    <TestMethod>
    Public Sub JsonFictif_VersPropositionsEtFenetreCorrigeable_SansApprobation()
        SurThreadSta(Sub()
                         Dim facture = DirectCast(GmailDocumentExtractionAdapter.Extraire(JsonFacture), Facture)
                         Dim cheque = DirectCast(GmailDocumentExtractionAdapter.Extraire(JsonCheque), Cheque)
                         Assert.AreEqual(42.5D, facture.montant_ttc)
                         Assert.AreEqual("FAUX-1", facture.numero_facture)
                         Assert.AreEqual(15.25D, cheque.montant_numerique)
                         Assert.AreEqual("0000789", cheque.numero_du_cheque)

                         Dim pieceFacture As New GmailDocumentPropositionAdapter.PieceAnalysee(
                             "simulation@example.invalid", 1UI, 5UI, 0, facture,
                             PropositionMouvement.SensMouvement.Debit)
                         Dim pieceCheque As New GmailDocumentPropositionAdapter.PieceAnalysee(
                             "simulation@example.invalid", 1UI, 5UI, 1, cheque,
                             PropositionMouvement.SensMouvement.Credit)
                         Dim workflow = GmailDocumentReviewWorkflow.Preparer(
                             New GmailDocumentPropositionAdapter.PieceAnalysee() {pieceFacture, pieceCheque})
                         Assert.AreEqual(0, workflow.ResultatInitial.Rejets.Count)
                         Assert.AreEqual(2, workflow.ResultatInitial.Propositions.Count)

                         Using fenetre = workflow.CreerFenetre()
                             Dim grille = DirectCast(fenetre.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Assert.AreEqual(42.5D, grille.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(15.25D, grille.Rows(1).Cells("Montant").Value)
                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(fenetre.CorrigerSelection(Function(dialogue)
                                                                        DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "43"
                                                                        Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                        Return DialogResult.OK
                                                                    End Function))
                             Assert.IsFalse(fenetre.IsHandleCreated)
                         End Using
                         Assert.AreEqual(43D, workflow.SessionRevue.Propositions(pieceFacture.CleSource).Montant)
                         Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                         Assert.AreEqual(42.5D, facture.montant_ttc)
                         Assert.AreEqual(0, facture.IdMvtDoc)
                         Assert.AreEqual(0, cheque.IdMvtDoc)
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerApprouvee(pieceFacture.CleSource))
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerPersistee(pieceFacture.CleSource))
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub JsonAbsentInvalideOuTypeNonAutorise_EstRefuseSansEffetExterne()
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailDocumentExtractionAdapter.Extraire(Nothing))
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailDocumentExtractionAdapter.Extraire("{invalide"))
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailDocumentExtractionAdapter.Extraire(
                "{""type_document"":""Autre"",""donnees_extraites"":{}}"))
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailDocumentExtractionAdapter.Extraire(
                "{""type_document"":""Facture""}"))
    End Sub

    <TestMethod>
    Public Sub LotJsonFictif_RejetsIndexesEtDoublonsSansBlocage()
        Dim pieces As GmailDocumentExtractionAdapter.PieceJson() = {
            New GmailDocumentExtractionAdapter.PieceJson(
                "simulation@example.invalid", 2UI, 5UI, 0, JsonFacture,
                PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentExtractionAdapter.PieceJson(
                "simulation@example.invalid", 2UI, 5UI, 1, "{invalide",
                PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentExtractionAdapter.PieceJson(
                "simulation@example.invalid", 2UI, 5UI, 1, JsonCheque,
                PropositionMouvement.SensMouvement.Credit),
            New GmailDocumentExtractionAdapter.PieceJson(
                "simulation@example.invalid", 2UI, 5UI, 2, JsonCheque,
                PropositionMouvement.SensMouvement.Credit),
            New GmailDocumentExtractionAdapter.PieceJson(
                "simulation@example.invalid", 2UI, 5UI, 3,
                "{""type_document"":""Autre"",""donnees_extraites"":{}}",
                PropositionMouvement.SensMouvement.Debit),
            Nothing}
        Dim resultat = GmailDocumentExtractionAdapter.ProposerLot(pieces)
        Assert.AreEqual(2, resultat.Propositions.Count)
        Assert.AreEqual(4, resultat.Rejets.Count)
        Assert.AreEqual(1, resultat.Rejets(0).IndexEntree)
        Assert.AreEqual("Extraction JSON invalide ou non prise en charge.", resultat.Rejets(0).Motif)
        Assert.AreEqual(2, resultat.Rejets(1).IndexEntree)
        Assert.AreEqual("Cle source dupliquee dans le lot.", resultat.Rejets(1).Motif)
        Assert.AreEqual(4, resultat.Rejets(2).IndexEntree)
        Assert.AreEqual("Extraction JSON invalide ou non prise en charge.", resultat.Rejets(2).Motif)
        Assert.AreEqual(5, resultat.Rejets(3).IndexEntree)
        Assert.AreEqual("Entree absente.", resultat.Rejets(3).Motif)
        Assert.AreEqual(pieces(0).CleSource, resultat.Propositions(0).CleSource)
        Assert.AreEqual(pieces(3).CleSource, resultat.Propositions(1).CleSource)
        Assert.IsFalse(resultat.Rejets(0).Motif.Contains("{invalide"))
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() GmailDocumentExtractionAdapter.ProposerLot(Nothing))
    End Sub

    <TestMethod>
    Public Sub LotJsonFictif_VersFenetreEtSessionSansApprobation()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() GmailDocumentReviewWorkflow.PreparerDepuisJson(Nothing))
        SurThreadSta(Sub()
                         Dim pieces As GmailDocumentExtractionAdapter.PieceJson() = {
                             New GmailDocumentExtractionAdapter.PieceJson(
                                 "simulation@example.invalid", 2UI, 6UI, 0, JsonFacture,
                                 PropositionMouvement.SensMouvement.Debit),
                             New GmailDocumentExtractionAdapter.PieceJson(
                                 "simulation@example.invalid", 2UI, 6UI, 1, "{invalide",
                                 PropositionMouvement.SensMouvement.Debit),
                             New GmailDocumentExtractionAdapter.PieceJson(
                                 "simulation@example.invalid", 2UI, 6UI, 2, JsonCheque,
                                 PropositionMouvement.SensMouvement.Credit)}
                         Dim workflow = GmailDocumentReviewWorkflow.PreparerDepuisJson(pieces)
                         Assert.AreEqual(2, workflow.ResultatInitial.Propositions.Count)
                         Assert.AreEqual(1, workflow.ResultatInitial.Rejets.Count)
                         Using fenetre = workflow.CreerFenetre()
                             Dim grille = DirectCast(fenetre.Controls.Find("grillePropositions", True)(0), DataGridView)
                             Dim rejets = DirectCast(fenetre.Controls.Find("grilleRejets", True)(0), DataGridView)
                             Assert.AreEqual(2, grille.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows.Count)
                             Assert.AreEqual(1, rejets.Rows(0).Cells("IndexEntree").Value)
                             grille.ClearSelection()
                             grille.Rows(0).Selected = True
                             Assert.IsTrue(fenetre.CorrigerSelection(Function(dialogue)
                                                                         DirectCast(dialogue.Controls.Find("montant", True)(0), TextBox).Text = "43"
                                                                         Assert.IsTrue(dialogue.AppliquerCorrection())
                                                                         Return DialogResult.OK
                                                                     End Function))
                             Assert.IsFalse(fenetre.IsHandleCreated)
                         End Using
                         Dim cle = pieces(0).CleSource
                         Assert.AreEqual(43D, workflow.SessionRevue.Propositions(cle).Montant)
                         Assert.AreEqual(1, workflow.SessionRevue.Audits.Count)
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerApprouvee(cle))
                         Assert.ThrowsExactly(Of InvalidOperationException)(
                             Sub() workflow.SessionRevue.MarquerPersistee(cle))
                     End Sub)
    End Sub
End Class
