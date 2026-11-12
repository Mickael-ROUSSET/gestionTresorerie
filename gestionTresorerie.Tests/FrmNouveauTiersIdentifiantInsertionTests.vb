Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports gestionTresorerie

<TestClass>
Public Class FrmNouveauTiersIdentifiantInsertionTests
    <TestMethod>
    Public Sub InsertionRetourneZeroEstRefusee()
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(Function() 0)
        Assert.IsFalse(resultat.HasValue, "Un identifiant nul ne doit pas autoriser l'enregistrement des coordonnées.")
    End Sub

    <TestMethod>
    Public Sub InsertionRetourneIdentifiantNegatifEstRefusee()
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(Function() -5)
        Assert.IsFalse(resultat.HasValue, "Un identifiant négatif ne doit pas être accepté.")
    End Sub

    <TestMethod>
    Public Sub InsertionRetourneIdentifiantPositifEstAcceptee()
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(Function() 73)
        Assert.IsTrue(resultat.HasValue)
        Assert.AreEqual(73, resultat.Value)
    End Sub
End Class
