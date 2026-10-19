Imports System.IO
Imports System.Reflection
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class EmailProcessorFileNameTests
    Private Shared Function AppelerUtilitaire(nom As String, valeur As String) As String
        Dim methode = GetType(EmailProcessor).GetMethod(nom, BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(methode, $"Utilitaire introuvable : {nom}")
        Return CStr(methode.Invoke(Nothing, New Object() {valeur}))
    End Function

    <TestMethod>
    Public Sub CleanFileName_RemplaceLesCaracteresInterdits()
        Dim nettoye = AppelerUtilitaire("CleanFileName", "facture:2026?.pdf")

        Assert.AreEqual("facture_2026_.pdf", nettoye)
    End Sub

    <TestMethod>
    Public Sub EnsureUniqueFileName_ConserveUnCheminLibre()
        Dim chemin = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".pdf")

        Assert.IsFalse(File.Exists(chemin))
        Assert.AreEqual(chemin, AppelerUtilitaire("EnsureUniqueFileName", chemin))
    End Sub

    <TestMethod>
    Public Sub EnsureUniqueFileName_GenereDesNomsDistinctsSansEcraser()
        ' L'assembly de tests existe deja : aucun fichier n'est cree, modifie ou supprime.
        Dim cheminExistant = GetType(EmailProcessorFileNameTests).Assembly.Location
        Assert.IsTrue(File.Exists(cheminExistant))

        Dim candidat1 = AppelerUtilitaire("EnsureUniqueFileName", cheminExistant)
        Dim candidat2 = AppelerUtilitaire("EnsureUniqueFileName", cheminExistant)

        Assert.AreNotEqual(cheminExistant, candidat1)
        Assert.AreNotEqual(candidat1, candidat2)
        Assert.AreEqual(Path.GetDirectoryName(cheminExistant), Path.GetDirectoryName(candidat1))
        Assert.AreEqual(Path.GetExtension(cheminExistant), Path.GetExtension(candidat1))
        Assert.IsFalse(File.Exists(candidat1))
        Assert.IsFalse(File.Exists(candidat2))
    End Sub
End Class
