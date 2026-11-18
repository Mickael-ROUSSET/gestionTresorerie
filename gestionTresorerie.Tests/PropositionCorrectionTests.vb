Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionCorrectionTests
    Private Shared Function Source() As PropositionMouvement
        Return New PropositionMouvement(" Source / AbC ", " CHEQUE ", New DateTime(2026, 1, 1),
            10D, PropositionMouvement.SensMouvement.Debit, "Ancien", "00001",
            {"Sens incertain", "Reference incertaine", "Inconnu", "Inconnu"})
    End Function

    <TestMethod>
    Public Sub Correction_ConserveIdentiteExacteEtOriginalEtAudit()
        Dim ancien = Source()
        Dim resultat = PropositionCorrection.Corriger(ancien, New DateTime(2026, 2, 28),
            0.01D, PropositionMouvement.SensMouvement.Credit, "Nouveau", "000002")
        Dim nouveau = resultat.Proposition
        Assert.AreNotSame(ancien, nouveau)
        Assert.AreSame(ancien, resultat.Avant)
        Assert.AreEqual(ancien.CleSource, nouveau.CleSource)
        Assert.AreEqual(ancien.TypeDocument, nouveau.TypeDocument)
        Assert.AreEqual(New DateTime(2026, 2, 28), nouveau.DateProposee)
        Assert.AreEqual(0.01D, nouveau.Montant)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Credit, nouveau.Sens)
        Assert.AreEqual("Nouveau", nouveau.Libelle)
        Assert.AreEqual("000002", nouveau.Reference)
        Assert.AreEqual(10D, ancien.Montant)
        Assert.AreEqual(New DateTime(2026, 1, 1), ancien.DateProposee)
        Assert.AreEqual(PropositionMouvement.SensMouvement.Debit, ancien.Sens)
        Assert.AreEqual("Ancien", ancien.Libelle)
        Assert.AreEqual("00001", ancien.Reference)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, nouveau.Etat)
        Assert.AreEqual(ancien.Etat, nouveau.Etat)
        CollectionAssert.AreEqual(New List(Of String)(ancien.AvertissementsValidation), New List(Of String)(nouveau.AvertissementsValidation))
        Assert.AreEqual(0, resultat.Confirmations.Count)
        CollectionAssert.AreEqual({PropositionCorrection.Champ.DateProposee, PropositionCorrection.Champ.Montant,
            PropositionCorrection.Champ.Sens, PropositionCorrection.Champ.Libelle, PropositionCorrection.Champ.Reference},
            New List(Of PropositionCorrection.Champ)(resultat.ChampsModifies))
    End Sub

    <TestMethod>
    Public Sub ConfirmationExplicite_NeffaceQueLeTexteExactEtCopieLaListe()
        Dim ancien = Source()
        Dim confirmations As New List(Of PropositionCorrection.ConfirmationAvertissement) From {
            New PropositionCorrection.ConfirmationAvertissement(PropositionCorrection.Champ.Sens, "Sens incertain")}
        Dim resultat = PropositionCorrection.Corriger(ancien, ancien.DateProposee, ancien.Montant,
            ancien.Sens, ancien.Libelle, ancien.Reference, confirmations)
        confirmations.Clear()
        CollectionAssert.AreEqual({"Reference incertaine", "Inconnu", "Inconnu"}, New List(Of String)(resultat.Proposition.AvertissementsValidation))
        Assert.AreEqual(4, ancien.AvertissementsValidation.Count)
        Assert.AreEqual(1, resultat.Confirmations.Count)
        Assert.AreEqual(0, resultat.ChampsModifies.Count)
        Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(resultat.ChampsModifies, IList(Of PropositionCorrection.Champ)).Clear())
        Assert.AreEqual(PropositionCorrection.Champ.Sens, resultat.Confirmations(0).Champ)
        Assert.AreEqual("Sens incertain", resultat.Confirmations(0).Avertissement)
        Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(resultat.Confirmations, IList(Of PropositionCorrection.ConfirmationAvertissement)).Clear())
        Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(resultat.Proposition.AvertissementsValidation, IList(Of String)).Clear())
        For Each type In {GetType(PropositionCorrection.Resultat), GetType(PropositionCorrection.ConfirmationAvertissement), GetType(PropositionMouvement)}
            For Each propriete In type.GetProperties()
                Assert.IsNull(propriete.GetSetMethod(), propriete.Name)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub ValeursInvalides_SontRejetees()
        Dim p = Source()
        For Each montant In {0D, -0.01D, Decimal.MinValue}
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PropositionCorrection.Corriger(p, p.DateProposee, montant, p.Sens, p.Libelle, p.Reference))
        Next
        For Each dateInvalide As DateTime? In New DateTime?() {Nothing, DateTime.MinValue}
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionCorrection.Corriger(p, dateInvalide, p.Montant, p.Sens, p.Libelle, p.Reference))
        Next
        For Each libelle In {Nothing, "", " " & vbTab}
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionCorrection.Corriger(p, p.DateProposee, p.Montant, p.Sens, libelle, p.Reference))
        Next
        For Each sens In {-1, 2, Integer.MaxValue}
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PropositionCorrection.Corriger(p, p.DateProposee, p.Montant, CType(sens, PropositionMouvement.SensMouvement), p.Libelle, p.Reference))
        Next
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() PropositionCorrection.Corriger(Nothing, p.DateProposee, p.Montant, p.Sens, p.Libelle, p.Reference))
        Assert.AreEqual(4, p.AvertissementsValidation.Count)
    End Sub

    <TestMethod>
    Public Sub BornesValidesEtReferenceAbsente_RestentEnAttente()
        Dim p = Source()
        For Each dateValide In {DateTime.MinValue.AddTicks(1), New DateTime(2024, 2, 29), DateTime.MaxValue}
            For Each montant In {0.0000000000000000000000000001D, Decimal.MaxValue}
                Dim resultat = PropositionCorrection.Corriger(p, dateValide, montant, p.Sens, p.Libelle, Nothing)
                Assert.AreEqual(dateValide, resultat.Proposition.DateProposee)
                Assert.AreEqual(montant, resultat.Proposition.Montant)
                Assert.IsNull(resultat.Proposition.Reference)
                Assert.AreEqual(4, resultat.Proposition.AvertissementsValidation.Count)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub ConfirmationInvalideOuTexteDifferent_EstRejetee()
        Dim p = Source()
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub()
            Dim c = New PropositionCorrection.ConfirmationAvertissement(CType(99, PropositionCorrection.Champ), "Inconnu")
        End Sub)
        For Each texte In {Nothing, "", " "}
            Assert.ThrowsExactly(Of ArgumentException)(Sub()
                Dim c = New PropositionCorrection.ConfirmationAvertissement(PropositionCorrection.Champ.Reference, texte)
            End Sub)
        Next
        For Each texte In {"sens incertain", "Sens incertain ", "Absent"}
            Dim c = New PropositionCorrection.ConfirmationAvertissement(PropositionCorrection.Champ.Sens, texte)
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionCorrection.Corriger(p, p.DateProposee, p.Montant, p.Sens, p.Libelle, p.Reference, {c}))
        Next
        Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionCorrection.Corriger(p, p.DateProposee, p.Montant, p.Sens, p.Libelle, p.Reference, {DirectCast(Nothing, PropositionCorrection.ConfirmationAvertissement)}))
    End Sub
End Class
