Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionMouvementValidationTests
    Private Shared Function Creer(Optional reference As String = "0000123",
                                  Optional avertissements As IEnumerable(Of String) = Nothing,
                                  Optional sens As PropositionMouvement.SensMouvement = PropositionMouvement.SensMouvement.Debit) As PropositionMouvement
        Return New PropositionMouvement("document-fictif-1", "CHEQUE", New DateTime(2026, 2, 3),
                                       42.1234D, sens, "Libellé fictif", reference, avertissements)
    End Function

    <TestMethod>
    Public Sub ApprobationAbsenteOuRefusee_NeProduitPasDeMouvement()
        Dim proposition = Creer()
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(proposition, 7, Array.Empty(Of String)(), referenceAmbigue:=False))
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(proposition, 7, Array.Empty(Of String)(), False, False))
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
    End Sub

    <TestMethod>
    Public Sub PropositionOuEnsembleAbsent_EstRefuse()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() PropositionMouvementValidation.Convertir(Nothing, 7, Array.Empty(Of String)(), True, False))
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() PropositionMouvementValidation.Convertir(Creer(), 7, Nothing, True, False))
    End Sub

    <TestMethod>
    Public Sub DocumentSourceAbsent_EstRefuse()
        For Each identifiant In {0, -1}
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() PropositionMouvementValidation.Convertir(Creer(), identifiant, Array.Empty(Of String)(), True, False))
        Next
        For Each source In New String() {Nothing, "", " "}
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub()
                    Dim proposition As New PropositionMouvement(source, "CHEQUE", New DateTime(2026, 2, 3),
                                                               1D, PropositionMouvement.SensMouvement.Debit, "Fictif")
                End Sub)
        Next
    End Sub

    <TestMethod>
    Public Sub MontantsNonPositifs_SontRefusesAvantConversion()
        ' Le modèle immuable interdit déjà ces valeurs à la construction.
        For Each montant In {0D, -0.01D, Decimal.MinValue}
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
                Sub()
                    Dim proposition As New PropositionMouvement("source-fictive", "CHEQUE", New DateTime(2026, 2, 3),
                                                               montant, PropositionMouvement.SensMouvement.Debit, "Fictif")
                End Sub)
        Next
    End Sub

    <TestMethod>
    Public Sub AvertissementsMemeVides_SontBloquantsMalgreApprobation()
        For Each avertissement In New String() {"Sens incertain", "Référence incertaine", "", Nothing}
            Dim proposition = Creer(avertissements:=New String() {avertissement})
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() PropositionMouvementValidation.Convertir(proposition, 7, Array.Empty(Of String)(), True, False))
            Assert.AreEqual(1, proposition.AvertissementsValidation.Count)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Next
    End Sub

    <TestMethod>
    Public Sub ReferenceAbsenteAmbigueOuNonConfirmee_EstRefusee()
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(Creer(), 7, Array.Empty(Of String)(), True))
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() PropositionMouvementValidation.Convertir(Creer(), 7, Array.Empty(Of String)(), True, True))
        For Each reference In New String() {Nothing, "", " ", vbTab}
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() PropositionMouvementValidation.Convertir(Creer(reference), 7, Array.Empty(Of String)(), True, False))
        Next
    End Sub

    <TestMethod>
    Public Sub IdentiteReutilisee_EstRefuseeIndependammentDeLaReference()
        For Each identite In {"document-fictif-1", " DOCUMENT-FICTIF-1 "}
            Dim utilisees As New HashSet(Of String)(StringComparer.Ordinal) From {identite}
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() PropositionMouvementValidation.Convertir(Creer("AUTRE"), 7, utilisees, True, False))
            Assert.AreEqual(1, utilisees.Count)
        Next
    End Sub

    <TestMethod>
    Public Sub ConversionApprouvee_ConserveValeursEtTracabiliteSansMutation()
        For Each sens In {PropositionMouvement.SensMouvement.Debit, PropositionMouvement.SensMouvement.Credit}
            Dim proposition = Creer(sens:=sens)
            Dim utilisees As New HashSet(Of String) From {"autre-document"}
            Dim mouvement = PropositionMouvementValidation.Convertir(proposition, 7, utilisees, True, False)
            Assert.AreEqual(proposition.DateProposee, mouvement.DateMvt)
            Assert.AreEqual(proposition.Montant, mouvement.Montant)
            Assert.AreEqual(sens = PropositionMouvement.SensMouvement.Credit, mouvement.Sens)
            Assert.AreEqual("0000123", mouvement.reference)
            Assert.AreEqual(proposition.TypeDocument, mouvement.typeReference)
            Assert.AreEqual(proposition.TypeDocument, mouvement.TypeMouvement)
            Assert.AreEqual(7, mouvement.idDoc)
            Assert.AreEqual(proposition.Libelle & vbLf & "Source : " & proposition.CleSource, mouvement.Note)
            Assert.IsFalse(mouvement.Etat)
            Assert.AreEqual(1, utilisees.Count)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
            utilisees.Add(proposition.CleSource)
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() PropositionMouvementValidation.Convertir(proposition, 7, utilisees, True, False))
        Next
    End Sub
End Class
