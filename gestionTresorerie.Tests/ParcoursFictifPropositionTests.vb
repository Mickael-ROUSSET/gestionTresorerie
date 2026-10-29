Option Strict On
Option Explicit On

Imports System
Imports System.Globalization
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ParcoursFictifPropositionTests
    ' Parcours intégralement en mémoire : aucun accès Gmail, fichier ou base de données.
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
    Public Sub ExtractionFictive_RevueCorrection_ResteSansApprobationEtSansEffetSurSources()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
                         Dim cheque As New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "0000123"}
                         Dim invalide As New Facture With {.montant_ttc = 8D, .devise = "USD",
                             .date_facture = "2026-02-05", .numero_facture = "F-REJET"}
                         Dim resultat = New PropositionAnalyseAdapter().Proposer({
                             New PropositionDocumentService.Entree(facture, "facture-fictive",
                                 PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(invalide, "devise-invalide",
                                 PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(cheque, "cheque-fictif",
                                 PropositionMouvement.SensMouvement.Credit)})
                         Assert.AreEqual(2, resultat.Propositions.Count)
                         Assert.AreEqual(1, resultat.Rejets.Count)
                         Assert.AreEqual("devise-invalide", resultat.Rejets(0).CleSource)

                         Using principale As New FrmPrincipale()
                             principale.PreparerRevuePropositions(resultat, True)
                             Using revue = principale.ConstruireFormulaireRevuePropositions()
                                 Assert.IsTrue(revue.Text.Contains("correction en mémoire"))
                                 Assert.IsTrue(revue.Text.Contains("sans approbation"))
                                 Dim propositions = DirectCast(revue.Controls.Find(
                                     "grillePropositions", True)(0), DataGridView)
                                 Dim rejets = DirectCast(revue.Controls.Find(
                                     "grilleRejets", True)(0), DataGridView)
                                 Assert.AreEqual(2, propositions.Rows.Count)
                                 Assert.AreEqual(1, rejets.Rows.Count)
                                 Assert.AreEqual("devise-invalide", rejets.Rows(0).Cells("CleSource").Value)
                                 propositions.Rows(0).Selected = True
                                 Using correction = revue.CreerFormulaireCorrectionSelectionnee()
                                     DirectCast(correction.Controls.Find("montant", True)(0), TextBox).Text =
                                         123.45D.ToString(CultureInfo.CurrentCulture)
                                     DirectCast(correction.Controls.Find("dateProposee", True)(0), TextBox).Text =
                                         "2026-10-29 14:05:09.123456"
                                     Assert.IsTrue(correction.AppliquerCorrection())
                                 End Using
                                 revue.ActualiserPropositions()
                                 Assert.AreEqual(123.45D, propositions.Rows(0).Cells("Montant").Value)
                                 Assert.AreEqual("En attente de revue humaine",
                                                 propositions.Rows(0).Cells("Statut").Value)
                                 Assert.AreEqual(42.12D, propositions.Rows(1).Cells("Montant").Value)
                                 Assert.AreEqual("En attente de revue humaine",
                                                 propositions.Rows(1).Cells("Statut").Value)
                                 Assert.IsFalse(revue.IsHandleCreated)
                             End Using
                             Using revueRouverte = principale.ConstruireFormulaireRevuePropositions()
                                 Dim grille = DirectCast(revueRouverte.Controls.Find(
                                     "grillePropositions", True)(0), DataGridView)
                                 Assert.AreEqual(123.45D, grille.Rows(0).Cells("Montant").Value)
                                 Assert.AreEqual(New DateTime(2026, 10, 29, 14, 5, 9).AddTicks(1234560),
                                                 grille.Rows(0).Cells("DateProposee").Value)
                                 Assert.AreEqual(42.12D, grille.Rows(1).Cells("Montant").Value)
                             End Using
                             Assert.IsFalse(principale.IsHandleCreated)
                         End Using

                         Assert.AreEqual(120.42D, facture.montant_ttc)
                         Assert.AreEqual("2026-02-03", facture.date_facture)
                         Assert.AreEqual(42.12D, cheque.montant_numerique)
                         Assert.AreEqual("0000123", cheque.numero_du_cheque)
                         Assert.AreEqual("USD", invalide.devise)
                     End Sub)
    End Sub
End Class
