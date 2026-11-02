Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionMouvementTests
    Private Shared Function Creer(Optional montant As Decimal = 1D,
                                  Optional cleSource As String = "document-fictif-001",
                                  Optional sens As PropositionMouvement.SensMouvement = PropositionMouvement.SensMouvement.Debit) As PropositionMouvement
        Return New PropositionMouvement(cleSource, "FACTURE", New DateTime(2026, 1, 2),
                                       montant, sens, "Transaction fictive")
    End Function

    <TestMethod>
    Public Sub Construction_ConserveLesChampsEtAttendUneRevueHumaine()
        For Each sens In {PropositionMouvement.SensMouvement.Debit, PropositionMouvement.SensMouvement.Credit}
            Dim proposition As New PropositionMouvement("source-fictive", "FACTURE", New DateTime(2026, 1, 2),
                                                       12.34D, sens, "Libellé fictif", "0000123")

            Assert.AreEqual("source-fictive", proposition.CleSource)
            Assert.AreEqual("FACTURE", proposition.TypeDocument)
            Assert.AreEqual(New DateTime(2026, 1, 2), proposition.DateProposee)
            Assert.AreEqual(12.34D, proposition.Montant)
            Assert.AreEqual(sens, proposition.Sens)
            Assert.AreEqual("Libellé fictif", proposition.Libelle)
            Assert.AreEqual("0000123", proposition.Reference)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Next
    End Sub

    <TestMethod>
    Public Sub MontantsNulsOuNegatifs_SontRejetes()
        For Each montant In {0D, -0.01D, Decimal.MinValue}
            Dim erreur = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() Creer(montant:=montant))
            Assert.AreEqual("montant", erreur.ParamName)
        Next
    End Sub

    <TestMethod>
    Public Sub BornesPositivesDecimal_SontConserveesSansArrondi()
        For Each montant In {0.0000000000000000000000000001D, Decimal.MaxValue}
            Assert.AreEqual(montant, Creer(montant:=montant).Montant)
        Next
    End Sub

    <TestMethod>
    Public Sub IdentitesSourcesAbsentes_SontRejetees()
        For Each source In New String() {Nothing, "", " ", vbTab}
            Dim erreur = Assert.ThrowsExactly(Of ArgumentException)(Sub() Creer(cleSource:=source))
            Assert.AreEqual("cleSource", erreur.ParamName)
        Next
    End Sub

    <TestMethod>
    Public Sub DatesAbsentesOuParDefaut_SontRejetees()
        For Each dateProposee In New DateTime?() {Nothing, DateTime.MinValue}
            Dim erreur = Assert.ThrowsExactly(Of ArgumentException)(
                Sub()
                    Dim proposition As New PropositionMouvement("source-fictive", "FACTURE", dateProposee,
                                                               1D, PropositionMouvement.SensMouvement.Debit, "Libellé fictif")
                End Sub)
            Assert.AreEqual("dateProposee", erreur.ParamName)
        Next
    End Sub

    <TestMethod>
    Public Sub SensHorsEnumeration_EstRejete()
        For Each valeur In {-1, 2}
            Dim erreur = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
                Sub() Creer(sens:=CType(valeur, PropositionMouvement.SensMouvement)))
            Assert.AreEqual("sens", erreur.ParamName)
        Next
    End Sub

    <TestMethod>
    Public Sub TypeDocumentEtLibelleAbsents_SontRejetes()
        For Each texte In New String() {Nothing, "", " "}
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub()
                    Dim proposition As New PropositionMouvement("source-fictive", texte, New DateTime(2026, 1, 2),
                                                               1D, PropositionMouvement.SensMouvement.Debit, "Libellé fictif")
                End Sub)
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub()
                    Dim proposition As New PropositionMouvement("source-fictive", "FACTURE", New DateTime(2026, 1, 2),
                                                               1D, PropositionMouvement.SensMouvement.Debit, texte)
                End Sub)
        Next
    End Sub

    <TestMethod>
    Public Sub ChampsOptionnelsAbsents_LaPropositionResteEnAttente()
        Dim proposition = Creer()
        Assert.IsNull(proposition.Reference)
        Assert.AreEqual(0, proposition.AvertissementsValidation.Count)
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
    End Sub

    <TestMethod>
    Public Sub Avertissements_SontCopiesEtEnLectureSeule()
        Dim avertissements As New List(Of String) From {"Référence à vérifier"}
        Dim proposition As New PropositionMouvement("source-fictive", "FACTURE", New DateTime(2026, 1, 2),
                                                   1D, PropositionMouvement.SensMouvement.Credit, "Libellé fictif",
                                                   avertissementsValidation:=avertissements)
        avertissements.Clear()

        Assert.AreEqual(1, proposition.AvertissementsValidation.Count)
        Assert.AreEqual("Référence à vérifier", proposition.AvertissementsValidation(0))
        Dim liste = DirectCast(proposition.AvertissementsValidation, IList(Of String))
        Assert.ThrowsExactly(Of NotSupportedException)(Sub() liste.Add("Autre avertissement"))
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
    End Sub
End Class
