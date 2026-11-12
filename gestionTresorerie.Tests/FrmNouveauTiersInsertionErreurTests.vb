Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports gestionTresorerie

<TestClass>
Public Class FrmNouveauTiersInsertionErreurTests
    <TestMethod>
    Public Sub InsertionFictiveReussieRetourneIdentifiant()
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(Function() 42)
        Assert.IsTrue(resultat.HasValue)
        Assert.AreEqual(42, resultat.Value)
    End Sub

    Private Shared Function InsertionEnPanne() As Integer
        Throw New InvalidOperationException("Panne SQL fictive")
    End Function

    <TestMethod>
    Public Sub InsertionFictiveEnErreurRetourneNothing()
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(AddressOf InsertionEnPanne)
        Assert.IsFalse(resultat.HasValue)
    End Sub

    <TestMethod>
    Public Sub InsertionAbsenteEstRefusee()
        Dim exceptionLevee As Boolean = False
        Try
            FrmNouveauTiers.InsererTiersOuSignalerErreur(Nothing)
        Catch ex As ArgumentNullException
            exceptionLevee = True
        End Try
        Assert.IsTrue(exceptionLevee)
    End Sub
End Class
