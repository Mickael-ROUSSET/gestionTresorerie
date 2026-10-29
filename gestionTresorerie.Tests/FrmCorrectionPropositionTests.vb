Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmCorrectionPropositionTests
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

    Private Shared Function Source() As PropositionMouvement
        Return New PropositionMouvement(" Source & 001 ", " CHEQUE ", New DateTime(2026, 1, 2),
            12.34D, PropositionMouvement.SensMouvement.Debit, "Fictif", "00001", {"À vérifier"})
    End Function

    Private Shared Function Controle(Of T As Control)(f As Form, nom As String) As T
        Return DirectCast(f.Controls.Find(nom, True)(0), T)
    End Function

    <TestMethod>
    Public Sub Constructeur_RefuseSourceAbsente()
        SurThreadSta(Sub()
                         Assert.ThrowsExactly(Of ArgumentNullException)(Sub()
                                                                             Using f As New FrmCorrectionProposition(Nothing)
                                                                             End Using
                                                                         End Sub)
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Correction_ConserveIdentiteAvertissementsEtEtatSansModifierSource()
        SurThreadSta(Sub()
                         Dim original = Source()
                         Using f As New FrmCorrectionProposition(original)
                             Assert.IsNull(f.PropositionCorrigee)
                             Assert.AreEqual(original.CleSource, Controle(Of Label)(f, "cleSource").Text)
                             Assert.AreEqual(original.TypeDocument, Controle(Of Label)(f, "typeDocument").Text)
                             Controle(Of TextBox)(f, "dateProposee").Text = "2024-02-29"
                             Controle(Of TextBox)(f, "montant").Text = 42.05D.ToString(CultureInfo.CurrentCulture)
                             Controle(Of ComboBox)(f, "sens").SelectedIndex = 1
                             Controle(Of TextBox)(f, "libelle").Text = "Corrigé"
                             Controle(Of TextBox)(f, "reference").Text = "000002"
                             Assert.IsTrue(f.AppliquerCorrection())
                             Dim p = f.PropositionCorrigee
                             Assert.AreSame(original, f.ResultatCorrection.Avant)
                             Assert.AreNotSame(original, p)
                             Assert.AreEqual(original.CleSource, p.CleSource)
                             Assert.AreEqual(original.TypeDocument, p.TypeDocument)
                             Assert.AreEqual(New DateTime(2024, 2, 29), p.DateProposee)
                             Assert.AreEqual(42.05D, p.Montant)
                             Assert.AreEqual(PropositionMouvement.SensMouvement.Credit, p.Sens)
                             Assert.AreEqual("Corrigé", p.Libelle)
                             Assert.AreEqual("000002", p.Reference)
                             Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, p.Etat)
                             CollectionAssert.AreEqual(New List(Of String)(original.AvertissementsValidation), New List(Of String)(p.AvertissementsValidation))
                             Assert.AreEqual(0, f.ResultatCorrection.Confirmations.Count)
                             Assert.AreEqual(12.34D, original.Montant)
                             Assert.AreEqual("00001", original.Reference)
                             Assert.IsFalse(f.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Correction_AccepteHeureEtFractionsSansApprobation()
        SurThreadSta(Sub()
                         Dim cas As New Dictionary(Of String, DateTime) From {
                             {"2026-10-29 14:05", New DateTime(2026, 10, 29, 14, 5, 0)},
                             {"2026-10-29 14:05:09", New DateTime(2026, 10, 29, 14, 5, 9)},
                             {"2026-10-29 14:05:09.1", New DateTime(2026, 10, 29, 14, 5, 9).AddTicks(1000000)},
                             {"2026-10-29 14:05:09.123", New DateTime(2026, 10, 29, 14, 5, 9).AddTicks(1230000)},
                             {"2026-10-29 14:05:09.123456", New DateTime(2026, 10, 29, 14, 5, 9).AddTicks(1234560)}}
                         For Each entree In cas
                             Dim original = Source()
                             Using f As New FrmCorrectionProposition(original)
                                 Controle(Of TextBox)(f, "dateProposee").Text = entree.Key
                                 Assert.IsTrue(f.AppliquerCorrection(), entree.Key)
                                 Assert.AreEqual(entree.Value, f.PropositionCorrigee.DateProposee)
                                 Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                                                 f.PropositionCorrigee.Etat)
                                 Assert.AreEqual(0, f.ResultatCorrection.Confirmations.Count)
                                 Assert.AreEqual(New DateTime(2026, 1, 2), original.DateProposee)
                                 Assert.IsFalse(f.IsHandleCreated)
                             End Using
                         Next
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Validation_RejetteValeursInvalidesSansResultatNiDialogue()
        SurThreadSta(Sub()
                         For Each cas In New String()() {
                             New String() {"montant", "0"}, New String() {"montant", "-1"},
                             New String() {"montant", "abc"}, New String() {"montant", "79228162514264337593543950336"},
                             New String() {"dateProposee", "2026-02-30"}, New String() {"dateProposee", ""},
                             New String() {"dateProposee", "2026-10-29 14:05:09.12345678"},
                             New String() {"dateProposee", "2026-10-29 24:01"},
                             New String() {"libelle", " "}}
                             Using f As New FrmCorrectionProposition(Source())
                                 Controle(Of TextBox)(f, cas(0)).Text = cas(1)
                                 Assert.IsFalse(f.AppliquerCorrection(), String.Join(":", cas))
                                 Assert.IsNull(f.PropositionCorrigee)
                                 Assert.IsNull(f.ResultatCorrection)
                                 Assert.IsFalse(String.IsNullOrWhiteSpace(Controle(Of Label)(f, "erreurValidation").Text))
                                 Assert.IsFalse(f.IsHandleCreated)
                             End Using
                         Next
                         Using f As New FrmCorrectionProposition(Source())
                             Controle(Of ComboBox)(f, "sens").SelectedIndex = -1
                             Assert.IsFalse(f.AppliquerCorrection())
                             Assert.IsNull(f.PropositionCorrigee)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Boutons_AppliquentOuAnnulentSansAfficherLaFenetre()
        SurThreadSta(Sub()
                         For Each nom In {"boutonAppliquer", "boutonAnnuler"}
                             Using f As New FrmCorrectionProposition(Source())
                                 ' Déclenche Click sans Show ni PerformClick (qui dépend de la visibilité).
                                 GetType(Button).GetMethod("OnClick", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic).
                                     Invoke(Controle(Of Button)(f, nom), New Object() {EventArgs.Empty})
                                 If nom = "boutonAppliquer" Then
                                     Assert.AreEqual(DialogResult.OK, f.DialogResult)
                                     Assert.IsNotNull(f.PropositionCorrigee)
                                     Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, f.PropositionCorrigee.Etat)
                                 Else
                                     Assert.AreEqual(DialogResult.Cancel, f.DialogResult)
                                     Assert.IsNull(f.PropositionCorrigee)
                                 End If
                             End Using
                         Next
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub SansModification_ConservePrecisionEtReferenceAbsente()
        SurThreadSta(Sub()
                         Dim dateOriginale = New DateTime(2026, 1, 2).AddTicks(1234567)
                         For Each montant In {Decimal.MaxValue, 0.0000000000000000000000000001D}
                             Dim original As New PropositionMouvement("fictif", "FACTURE", dateOriginale,
                                 montant, PropositionMouvement.SensMouvement.Credit, "Fictif")
                             Using f As New FrmCorrectionProposition(original)
                                 Assert.IsTrue(f.AppliquerCorrection())
                                 Assert.AreEqual(dateOriginale, f.PropositionCorrigee.DateProposee)
                                 Assert.AreEqual(montant, f.PropositionCorrigee.Montant)
                                 Assert.IsNull(f.PropositionCorrigee.Reference)
                                 Controle(Of TextBox)(f, "libelle").Text = ""
                                 Assert.IsFalse(f.AppliquerCorrection())
                                 Assert.IsNull(f.ResultatCorrection)
                             End Using
                         Next
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub Annulation_ApresSaisieInvalideOuCorrection_NeRetourneRien()
        SurThreadSta(Sub()
                         For Each appliquer In {False, True}
                             Using f As New FrmCorrectionProposition(Source())
                                 Assert.AreSame(Controle(Of Button)(f, "boutonAnnuler"), f.CancelButton)
                                 Assert.AreSame(Controle(Of Button)(f, "boutonAppliquer"), f.AcceptButton)
                                 If appliquer Then
                                     Assert.IsTrue(f.AppliquerCorrection())
                                 Else
                                     Controle(Of TextBox)(f, "montant").Text = "invalide"
                                 End If
                                 f.AnnulerCorrection()
                                 Assert.AreEqual(DialogResult.Cancel, f.DialogResult)
                                 Assert.IsNull(f.PropositionCorrigee)
                                 Assert.IsNull(f.ResultatCorrection)
                             End Using
                         Next
                     End Sub)
    End Sub

    Private Shared Function SessionFictive() As SessionCorrectionPropositions
        Dim facture As New Facture With {.montant_ttc = 120.42D, .devise = "EUR",
            .date_facture = "2026-02-03", .numero_facture = "F-FICTIVE"}
        Dim lot = New PropositionDocumentService().Proposer({
            New PropositionDocumentService.Entree(facture, "facture-fictive",
                                                  PropositionMouvement.SensMouvement.Debit)})
        Return New SessionCorrectionPropositions(lot)
    End Function

    <TestMethod>
    Public Sub FormulaireLie_SauvegardeLaCorrectionDansLaSessionSansApprobation()
        SurThreadSta(Sub()
                         Dim session = SessionFictive()
                         Dim original = session.Selectionner("facture-fictive")
                         Using f As New FrmCorrectionProposition(session, "facture-fictive")
                             Controle(Of TextBox)(f, "montant").Text = 51.25D.ToString(CultureInfo.CurrentCulture)
                             Assert.IsTrue(f.AppliquerCorrection())
                             Assert.AreSame(f.PropositionCorrigee, session.Selectionner("facture-fictive"))
                             Assert.AreEqual(51.25D, session.Selectionner("facture-fictive").Montant)
                             Assert.AreEqual(120.42D, original.Montant)
                             Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine,
                                             session.Selectionner("facture-fictive").Etat)
                             Assert.IsFalse(f.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub FormulaireLie_RefuseCorrectionObsoleteSansEcraserLaSession()
        SurThreadSta(Sub()
                         Dim session = SessionFictive()
                         Dim original = session.Selectionner("facture-fictive")
                         Using f As New FrmCorrectionProposition(session, "facture-fictive")
                             Dim correctionConcurrente = session.Corriger("facture-fictive", original.DateProposee,
                                 77D, original.Sens, original.Libelle, original.Reference)
                             Controle(Of TextBox)(f, "montant").Text = 15D.ToString(CultureInfo.CurrentCulture)
                             Assert.IsFalse(f.AppliquerCorrection())
                             Assert.IsNull(f.ResultatCorrection)
                             Assert.AreSame(correctionConcurrente.Proposition,
                                            session.Selectionner("facture-fictive"))
                             Assert.AreEqual(77D, session.Selectionner("facture-fictive").Montant)
                             Assert.IsFalse(String.IsNullOrWhiteSpace(Controle(Of Label)(f, "erreurValidation").Text))
                             Assert.IsFalse(f.IsHandleCreated)
                         End Using
                     End Sub)
    End Sub

    <TestMethod>
    Public Sub FormulaireLie_AnnulationEtSelectionInconnueNeModifientRien()
        SurThreadSta(Sub()
                         Dim session = SessionFictive()
                         Dim original = session.Selectionner("facture-fictive")
                         Assert.ThrowsExactly(Of ArgumentNullException)(
                             Sub()
                                 Using f As New FrmCorrectionProposition(
                                     DirectCast(Nothing, SessionCorrectionPropositions), "facture-fictive")
                                 End Using
                             End Sub)
                         Assert.ThrowsExactly(Of KeyNotFoundException)(
                             Sub()
                                 Using f As New FrmCorrectionProposition(session, "inconnue")
                                 End Using
                             End Sub)
                         Using f As New FrmCorrectionProposition(session, "facture-fictive")
                             Controle(Of TextBox)(f, "montant").Text = 1D.ToString(CultureInfo.CurrentCulture)
                             f.AnnulerCorrection()
                             Assert.IsNull(f.ResultatCorrection)
                         End Using
                         Assert.AreSame(original, session.Selectionner("facture-fictive"))
                     End Sub)
    End Sub
End Class
