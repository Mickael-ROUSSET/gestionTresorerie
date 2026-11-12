Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports gestionTresorerie

<TestClass>
Public Class FrmNouveauTiersVerificationDoublonTests
    <TestMethod>
    Public Sub RechercheSansDoublonAutoriseLaSuite()
        Dim resultat As Boolean? = FrmNouveauTiers.VerifierDoublonOuBloquer(Function() False)
        Assert.IsTrue(resultat.HasValue)
        Assert.IsFalse(resultat.Value)
    End Sub

    <TestMethod>
    Public Sub RechercheAvecDoublonSignaleLeDoublon()
        Dim resultat As Boolean? = FrmNouveauTiers.VerifierDoublonOuBloquer(Function() True)
        Assert.IsTrue(resultat.HasValue)
        Assert.IsTrue(resultat.Value)
    End Sub

    <TestMethod>
    Public Sub RechercheEnErreurInterditTouteCreation()
        Dim resultat As Boolean? = FrmNouveauTiers.VerifierDoublonOuBloquer(
            Function() Throw New InvalidOperationException("Panne SQL fictive"))
        Assert.IsFalse(resultat.HasValue)
    End Sub

    <TestMethod>
    Public Sub RechercheAbsenteEstRefusee()
        Dim exceptionLevee As Boolean = False
        Try
            FrmNouveauTiers.VerifierDoublonOuBloquer(Nothing)
        Catch ex As ArgumentNullException
            exceptionLevee = True
        End Try
        Assert.IsTrue(exceptionLevee)
    End Sub
End Class
