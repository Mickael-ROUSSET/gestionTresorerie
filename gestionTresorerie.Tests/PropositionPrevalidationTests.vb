Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionPrevalidationTests
    Private Shared Function Evaluer(Optional montant As Decimal = 1D,
                                    Optional dateProposee As DateTime? = Nothing,
                                    Optional cle As String = "source-fictive",
                                    Optional document As Integer = 1,
                                    Optional reference As String = "00001",
                                    Optional ambigue As Boolean = False,
                                    Optional sens As Integer = 0) As PropositionPrevalidation.Resultat
        Return PropositionPrevalidation.Evaluer(montant, dateProposee, cle, document, reference,
            ambigue, Array.Empty(Of String)(), Array.Empty(Of String)(), CType(sens, PropositionMouvement.SensMouvement))
    End Function

    Private Shared Sub Rejet(resultat As PropositionPrevalidation.Resultat, ParamArray attendues As PropositionPrevalidation.Anomalie())
        Assert.IsFalse(resultat.EstPrete)
        CollectionAssert.AreEqual(attendues, New List(Of PropositionPrevalidation.Anomalie)(resultat.Anomalies))
    End Sub

    <TestMethod>
    Public Sub MontantsNonPositifs()
        For Each montant In {0D, -0.01D, Decimal.MinValue}
            Rejet(Evaluer(montant:=montant, dateProposee:=New DateTime(2026, 1, 1)), PropositionPrevalidation.Anomalie.MontantNonPositif)
        Next
    End Sub

    <TestMethod>
    Public Sub DatesAbsentesOuInvalides()
        For Each valeur As DateTime? In New DateTime?() {Nothing, DateTime.MinValue}
            Rejet(Evaluer(dateProposee:=valeur), PropositionPrevalidation.Anomalie.DateInvalide)
        Next
    End Sub

    <TestMethod>
    Public Sub SourcesDocumentsReferencesEtSensInvalides()
        Dim jour As New DateTime(2026, 1, 1)
        For Each texte In New String() {Nothing, "", " " & vbTab}
            Rejet(Evaluer(dateProposee:=jour, cle:=texte), PropositionPrevalidation.Anomalie.CleSourceAbsente)
            Rejet(Evaluer(dateProposee:=jour, reference:=texte), PropositionPrevalidation.Anomalie.ReferenceVide)
        Next
        For Each document In {0, -1}
            Rejet(Evaluer(dateProposee:=jour, document:=document), PropositionPrevalidation.Anomalie.DocumentSourceAbsent)
        Next
        For Each sens In {-1, 2, Integer.MaxValue}
            Rejet(Evaluer(dateProposee:=jour, sens:=sens), PropositionPrevalidation.Anomalie.SensInvalide)
        Next
        Rejet(Evaluer(dateProposee:=jour, ambigue:=True), PropositionPrevalidation.Anomalie.ReferenceAmbigue)
    End Sub

    Private Shared Function Source(Optional avertissements As IEnumerable(Of String) = Nothing) As PropositionMouvement
        Return New PropositionMouvement(" source-fictive ", "CHEQUE", New DateTime(2026, 1, 1),
            0.01D, PropositionMouvement.SensMouvement.Debit, "Fictif", "00001", avertissements)
    End Function

    <TestMethod>
    Public Sub AvertissementsMemeVidesRestentBloquants()
        For Each texte In New String() {Nothing, "", " ", "A verifier"}
            Rejet(PropositionPrevalidation.Evaluer(Source({texte}), 1, Array.Empty(Of String)(), False),
                  PropositionPrevalidation.Anomalie.AvertissementsNonResolus)
        Next
    End Sub

    <TestMethod>
    Public Sub IdentitesNormaliseesSansMutationEtDiagnosticUnique()
        Dim identites As New List(Of String) From {Nothing, "autre", "SOURCE-FICTIVE", " source-fictive "}
        Dim avant = identites.ToArray()
        Rejet(PropositionPrevalidation.Evaluer(Source(), 1, identites, False), PropositionPrevalidation.Anomalie.IdentiteSourceReutilisee)
        CollectionAssert.AreEqual(avant, identites)
        Rejet(PropositionPrevalidation.Evaluer(Source(), 1, Nothing, False), PropositionPrevalidation.Anomalie.IdentitesUtiliseesAbsentes)
        Rejet(PropositionPrevalidation.Evaluer(Nothing, 1, identites), PropositionPrevalidation.Anomalie.PropositionAbsente)
    End Sub

    <TestMethod>
    Public Sub DiagnosticsCumulesStablesEtImmuables()
        For iteration = 1 To 3
            Dim resultat = PropositionPrevalidation.Evaluer(0D, Nothing, Nothing, 0, Nothing, True,
                Nothing, Nothing, CType(99, PropositionMouvement.SensMouvement))
            Rejet(resultat, PropositionPrevalidation.Anomalie.MontantNonPositif,
                PropositionPrevalidation.Anomalie.DateInvalide, PropositionPrevalidation.Anomalie.CleSourceAbsente,
                PropositionPrevalidation.Anomalie.DocumentSourceAbsent, PropositionPrevalidation.Anomalie.ReferenceVide,
                PropositionPrevalidation.Anomalie.ReferenceAmbigue, PropositionPrevalidation.Anomalie.AvertissementsNonResolus,
                PropositionPrevalidation.Anomalie.IdentitesUtiliseesAbsentes, PropositionPrevalidation.Anomalie.SensInvalide)
            Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(resultat.Anomalies, IList(Of PropositionPrevalidation.Anomalie)).Clear())
        Next
    End Sub

    <TestMethod>
    Public Sub OriginalEtCorrectionPretsRestentEnAttenteSansMutation()
        Dim original = Source()
        Dim corrigee = PropositionCorrection.Corriger(original, original.DateProposee, 2.1234D,
            PropositionMouvement.SensMouvement.Credit, "Corrige", "00002").Proposition
        Dim identites As New List(Of String) From {Nothing, "autre"}
        For Each proposition In {original, corrigee}
            Dim resultat = PropositionPrevalidation.Evaluer(proposition, 1, identites, False)
            Assert.IsTrue(resultat.EstPrete)
            Assert.AreEqual(0, resultat.Anomalies.Count)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
            Rejet(PropositionPrevalidation.Evaluer(proposition, 1, identites), PropositionPrevalidation.Anomalie.ReferenceAmbigue)
        Next
        Assert.AreEqual(0.01D, original.Montant)
        Assert.AreEqual("00001", original.Reference)
        Assert.AreEqual("00002", corrigee.Reference)
        Assert.AreEqual(2, identites.Count)
        Assert.IsTrue(Evaluer(montant:=Decimal.MaxValue, dateProposee:=DateTime.MaxValue, sens:=1).EstPrete)
    End Sub
End Class
