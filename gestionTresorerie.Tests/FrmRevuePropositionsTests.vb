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
Public Class FrmRevuePropositionsTests
    ' Construction et inspection sans Show, Application.Run ou boucle de messages.
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

    Private Shared Function Grille(formulaire As Form, nom As String) As DataGridView
        Return DirectCast(formulaire.Controls.Find(nom, True)(0), DataGridView)
    End Function

    <TestMethod>
    Public Sub Constructeur_RefuseUneRevueAbsente()
        SurThreadSta(Sub()
                         Dim erreur = Assert.ThrowsExactly(Of ArgumentNullException)(
                             Sub()
                                 Using formulaire As New FrmRevuePropositions(Nothing)
                                 End Using
                             End Sub)
                         Assert.AreEqual("revue", erreur.ParamName)
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub LotMixte_MappeLesValeursEtSepareLesRejets()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
                         Dim cheque As New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "0000123"}
                         Dim revue As New PropositionRevue(New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(facture, "z", PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(cheque, "a", PropositionMouvement.SensMouvement.Credit),
                             New PropositionDocumentService.Entree(Nothing, "absent", PropositionMouvement.SensMouvement.Debit),
                             Nothing}))
                         Using formulaire As New FrmRevuePropositions(revue)
                             Dim propositions = Grille(formulaire, "grillePropositions")
                             Dim rejets = Grille(formulaire, "grilleRejets")
                             Assert.AreEqual(2, propositions.Rows.Count)
                             Assert.AreEqual(2, rejets.Rows.Count)
                             For i As Integer = 0 To revue.Lignes.Count - 1
                                 Dim source = revue.Lignes(i)
                                 Dim cellules = propositions.Rows(i).Cells
                                 Assert.AreEqual(source.CleSource, cellules("CleSource").Value)
                                 Assert.AreEqual(source.TypeDocument, cellules("TypeDocument").Value)
                                 Assert.AreEqual(source.Reference, cellules("Reference").Value)
                                 Assert.AreEqual(source.DateProposee, cellules("DateProposee").Value)
                                 Assert.AreEqual(source.Montant, cellules("Montant").Value)
                                 Assert.IsInstanceOfType(Of Decimal)(cellules("Montant").Value)
                                 Assert.AreEqual(source.Libelle, cellules("Libelle").Value)
                                 Assert.AreEqual("En attente de revue humaine", cellules("Statut").Value)
                                 Assert.AreEqual(String.Join(Environment.NewLine, source.AvertissementsValidation),
                                                 cellules("Avertissements").Value)
                                 Assert.IsTrue(source.AvertissementsValidation.Count > 0)
                             Next
                             Assert.AreEqual("Débit", propositions.Rows(0).Cells("Sens").Value)
                             Assert.AreEqual("Crédit", propositions.Rows(1).Cells("Sens").Value)
                             Assert.AreEqual("0000123", propositions.Rows(1).Cells("Reference").Value)
                             For i As Integer = 0 To revue.Rejets.Count - 1
                                 Assert.AreEqual(revue.Rejets(i).IndexEntree, rejets.Rows(i).Cells("IndexEntree").Value)
                                 Assert.AreEqual(revue.Rejets(i).CleSource, rejets.Rows(i).Cells("CleSource").Value)
                                 Assert.AreEqual(revue.Rejets(i).Motif, rejets.Rows(i).Cells("Motif").Value)
                             Next
                             For Each grilleLecture In {propositions, rejets}
                                 Assert.IsTrue(grilleLecture.ReadOnly)
                                 Assert.IsFalse(grilleLecture.AllowUserToAddRows)
                                 Assert.IsFalse(grilleLecture.AllowUserToDeleteRows)
                                 Assert.IsNull(grilleLecture.DataSource)
                                 Assert.IsFalse(String.IsNullOrWhiteSpace(grilleLecture.AccessibleName))
                                 For Each colonne As DataGridViewColumn In grilleLecture.Columns
                                     Assert.IsTrue(colonne.ReadOnly)
                                     Assert.AreEqual(DataGridViewColumnSortMode.NotSortable, colonne.SortMode)
                                 Next
                             Next
                             Dim fermer = DirectCast(formulaire.Controls.Find("boutonFermer", True)(0), Button)
                             Assert.AreEqual("&Fermer", fermer.Text)
                             Assert.AreSame(fermer, formulaire.CancelButton)
                             Assert.AreEqual(DialogResult.Cancel, fermer.DialogResult)
                             Dim disposition = DirectCast(formulaire.Controls(0), TableLayoutPanel)
                             Dim nombreBoutons As Integer = 0
                             For Each controle As Control In disposition.Controls
                                 If TypeOf controle Is Button Then nombreBoutons += 1
                             Next
                             Assert.AreEqual(1, nombreBoutons)
                             Assert.IsFalse(formulaire.IsHandleCreated)
                         End Using
                         Assert.AreEqual(120.42D, facture.montant_ttc)
                         Assert.AreEqual("0000123", cheque.numero_du_cheque)
                         Assert.AreEqual(PropositionRevue.StatutRevue.AwaitingHumanReview, revue.Lignes(0).Statut)
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub LotsVideEtEntierementRejete_SontConsultables()
        SurThreadSta(Sub()
                         Dim service As New PropositionDocumentService()
                         For Each resultat In {service.Proposer(Array.Empty(Of PropositionDocumentService.Entree)()),
                                               service.Proposer(New PropositionDocumentService.Entree() {Nothing})}
                             Using formulaire As New FrmRevuePropositions(New PropositionRevue(resultat))
                                 Assert.AreEqual(0, Grille(formulaire, "grillePropositions").Rows.Count)
                                 Assert.AreEqual(resultat.Rejets.Count, Grille(formulaire, "grilleRejets").Rows.Count)
                             End Using
                         Next
                     End Sub)
    End Sub
    <TestMethod>
    Public Sub DateAffichee_ConserveHeureEtFractionsSansArrondir()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 12.34D, .devise = "EUR",
                             .date_facture = "2026-10-29", .numero_facture = "FICTIVE-DATE"}
                         Dim resultat = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(facture, "date-fictive",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Using formulaire As New FrmRevuePropositions(New PropositionRevue(resultat))
                             Dim grilleDates = Grille(formulaire, "grillePropositions")
                             Dim colonne = grilleDates.Columns("DateProposee")
                             Assert.AreEqual(GetType(DateTime), colonne.ValueType)
                             Assert.AreEqual("dd/MM/yyyy HH:mm:ss.FFFFFFF", colonne.DefaultCellStyle.Format)
                             Assert.AreEqual(New DateTime(2026, 10, 29),
                                             grilleDates.Rows(0).Cells("DateProposee").Value)
                             Dim avecFractions = New DateTime(2026, 10, 29, 14, 5, 9).AddTicks(1234560)
                             Assert.AreEqual("29/10/2026 14:05:09.123456",
                                             avecFractions.ToString(colonne.DefaultCellStyle.Format,
                                                                   CultureInfo.GetCultureInfo("fr-FR")))
                             Assert.AreEqual("29/10/2026 00:00:00",
                                             New DateTime(2026, 10, 29).ToString(
                                                 colonne.DefaultCellStyle.Format,
                                                 CultureInfo.GetCultureInfo("fr-FR")))
                             Assert.IsTrue(grilleDates.ReadOnly)
                             Assert.IsFalse(formulaire.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub


    <TestMethod>
    Public Sub SessionFictive_SelectionEtCorrectionRestentEnMemoireSansApprobation()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
                         Dim cheque As New Cheque With {.montant_numerique = 42.12D, .devise = "EUR",
                             .dateChq = "20260204", .numero_du_cheque = "0000123"}
                         Dim resultat = New PropositionDocumentService().Proposer({
                             New PropositionDocumentService.Entree(facture, "facture-fictive", PropositionMouvement.SensMouvement.Debit),
                             New PropositionDocumentService.Entree(cheque, "cheque-fictif", PropositionMouvement.SensMouvement.Credit)})
                         Dim session As New SessionCorrectionPropositions(resultat)
                         Dim originale = session.Selectionner("facture-fictive")
                         Dim autre = session.Selectionner("cheque-fictif")
                         Using revue As New FrmRevuePropositions(New PropositionRevue(resultat), session)
                             Dim grillePropositions = Grille(revue, "grillePropositions")
                             Dim bouton = DirectCast(revue.Controls.Find("boutonCorriger", True)(0), Button)
                             Assert.IsTrue(grillePropositions.ReadOnly)
                             Assert.IsFalse(bouton.Enabled)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub()
                                     Using invalide = revue.CreerFormulaireCorrectionSelectionnee()
                                     End Using
                                 End Sub)
                             grillePropositions.Rows(0).Selected = True
                             Assert.IsTrue(bouton.Enabled)
                             Using correction = revue.CreerFormulaireCorrectionSelectionnee()
                                 DirectCast(correction.Controls.Find("montant", True)(0), TextBox).Text =
                                     99.01D.ToString(CultureInfo.CurrentCulture)
                                 Assert.IsTrue(correction.AppliquerCorrection())
                             End Using
                             Assert.AreEqual(120.42D, grillePropositions.Rows(0).Cells("Montant").Value)
                             revue.ActualiserPropositions()
                             Assert.AreEqual(99.01D, grillePropositions.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual(42.12D, grillePropositions.Rows(1).Cells("Montant").Value)
                             Assert.AreEqual("En attente de revue humaine",
                                             grillePropositions.Rows(0).Cells("Statut").Value)
                             Assert.AreSame(autre, session.Selectionner("cheque-fictif"))
                             Assert.AreEqual(120.42D, originale.Montant)
                             Assert.AreEqual(120.42D, facture.montant_ttc)
                             Assert.IsFalse(revue.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub SessionIncoherente_EstRefuseeEtLectureSeuleNeCorrigePas()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 10D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-TEST"}
                         Dim service As New PropositionDocumentService()
                         Dim premier = service.Proposer({
                             New PropositionDocumentService.Entree(facture, "source-a", PropositionMouvement.SensMouvement.Debit)})
                         Dim second = service.Proposer({
                             New PropositionDocumentService.Entree(facture, "source-b", PropositionMouvement.SensMouvement.Debit)})
                         Dim session As New SessionCorrectionPropositions(second)
                         Assert.ThrowsExactly(Of ArgumentException)(
                             Sub()
                                 Using invalide As New FrmRevuePropositions(New PropositionRevue(premier), session)
                                 End Using
                             End Sub)
                         Using lecture As New FrmRevuePropositions(New PropositionRevue(premier))
                             Assert.AreEqual(0, lecture.Controls.Find("boutonCorriger", True).Length)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub()
                                     Using invalide = lecture.CreerFormulaireCorrectionSelectionnee()
                                     End Using
                                 End Sub)
                             Assert.ThrowsExactly(Of InvalidOperationException)(
                                 Sub() lecture.ActualiserPropositions())
                         End Using
                     End Sub)
    End Sub


    <TestMethod>
    Public Sub SessionAvecMemeCleMaisTypeDocumentDifferent_EstRefusee()
        SurThreadSta(Sub()
                         Dim facture As New Facture With {.montant_ttc = 10D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
                         Dim cheque As New Cheque With {.montant_numerique = 10D, .devise = "EUR",
                             .dateChq = "20260203", .numero_du_cheque = "0000123"}
                         Dim service As New PropositionDocumentService()
                         Dim resultatFacture = service.Proposer({
                             New PropositionDocumentService.Entree(facture, "meme-identite",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Dim resultatCheque = service.Proposer({
                             New PropositionDocumentService.Entree(cheque, "meme-identite",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Assert.AreEqual(1, resultatFacture.Propositions.Count)
                         Assert.AreEqual(1, resultatCheque.Propositions.Count)
                         Dim session As New SessionCorrectionPropositions(resultatCheque)
                         Dim erreur = Assert.ThrowsExactly(Of ArgumentException)(
                             Sub()
                                 Using invalide As New FrmRevuePropositions(
                                     New PropositionRevue(resultatFacture), session)
                                 End Using
                             End Sub)
                         Assert.AreEqual("session", erreur.ParamName)
                         Assert.AreEqual(10D, session.Selectionner("meme-identite").Montant)
                         Assert.AreEqual("0000123", cheque.numero_du_cheque)
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub SessionMemeCleEtTypeMaisDocumentDifferent_EstRefuseeSansPerdreLesCorrectionsLegitimes()
        SurThreadSta(Sub()
                         Dim factureOriginale As New Facture With {.montant_ttc = 10D, .devise = "EUR",
                             .date_facture = "2026-02-03", .numero_facture = "F-ORIGINE"}
                         Dim factureEtrangere As New Facture With {.montant_ttc = 20D, .devise = "EUR",
                             .date_facture = "2026-02-04", .numero_facture = "F-ETRANGERE"}
                         Dim service As New PropositionDocumentService()
                         Dim origine = service.Proposer({
                             New PropositionDocumentService.Entree(factureOriginale, "identite-commune",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Dim etranger = service.Proposer({
                             New PropositionDocumentService.Entree(factureEtrangere, "identite-commune",
                                 PropositionMouvement.SensMouvement.Debit)})
                         Dim revue As New PropositionRevue(origine)
                         Dim sessionEtrangere As New SessionCorrectionPropositions(etranger)
                         Dim avantEtranger = sessionEtrangere.Selectionner("identite-commune")
                         Dim erreur = Assert.ThrowsExactly(Of ArgumentException)(
                             Sub()
                                 Using invalide As New FrmRevuePropositions(revue, sessionEtrangere)
                                 End Using
                             End Sub)
                         Assert.AreEqual("session", erreur.ParamName)
                         Assert.AreSame(avantEtranger, sessionEtrangere.Selectionner("identite-commune"))

                         Dim sessionLegitime As New SessionCorrectionPropositions(origine)
                         Dim avant = sessionLegitime.Selectionner("identite-commune")
                         sessionLegitime.Corriger("identite-commune", avant.DateProposee, 77D,
                             avant.Sens, avant.Libelle, avant.Reference)
                         Using rouverte As New FrmRevuePropositions(revue, sessionLegitime)
                             Dim grillePropositions = Grille(rouverte, "grillePropositions")
                             Assert.AreEqual(77D, grillePropositions.Rows(0).Cells("Montant").Value)
                             Assert.AreEqual("En attente de revue humaine",
                                             grillePropositions.Rows(0).Cells("Statut").Value)
                         End Using
                         Assert.AreSame(avant, sessionLegitime.PropositionsInitiales(0))
                         Assert.AreEqual(10D, sessionLegitime.PropositionsInitiales(0).Montant)
                         Assert.AreEqual(10D, factureOriginale.montant_ttc)
                         Assert.AreEqual(20D, factureEtrangere.montant_ttc)
                     End Sub)
    End Sub
End Class
