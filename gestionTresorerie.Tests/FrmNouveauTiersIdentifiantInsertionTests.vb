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
    <TestMethod>
    Public Sub InsertionQuiEchoueNeLaissePasPasserUnIdentifiant()
        Dim appels As Integer = 0
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(
            Function()
                appels += 1
                Throw New InvalidOperationException("Panne SQL fictive")
                Return 0
            End Function)
        Assert.IsFalse(resultat.HasValue, "Une panne d'insertion ne doit pas autoriser l'enregistrement des coordonnées.")
        Assert.AreEqual(1, appels, "Une panne ne doit pas déclencher de nouvelle insertion automatique.")
    End Sub

    <TestMethod>
    Public Sub InsertionReussieNestExecuteeQuUneFois()
        Dim appels As Integer = 0
        Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(
            Function()
                appels += 1
                Return 73
            End Function)
        Assert.IsTrue(resultat.HasValue)
        Assert.AreEqual(73, resultat.Value)
        Assert.AreEqual(1, appels, "Une insertion réussie ne doit pas être rejouée.")
    End Sub

    <TestMethod>
    Public Sub IdentifiantsInvalidesNeDeclenchentPasDeNouvelleInsertion()
        For Each identifiantInvalide As Integer In New Integer() {0, -1, Integer.MinValue}
            Dim appels As Integer = 0
            Dim resultat As Integer? = FrmNouveauTiers.InsererTiersOuSignalerErreur(
                Function()
                    appels += 1
                    Return identifiantInvalide
                End Function)
            Assert.IsFalse(resultat.HasValue, "Identifiant fictif invalide : " & identifiantInvalide.ToString())
            Assert.AreEqual(1, appels, "Aucune réinsertion automatique ne doit suivre un identifiant SQL invalide.")
        Next
    End Sub

End Class
