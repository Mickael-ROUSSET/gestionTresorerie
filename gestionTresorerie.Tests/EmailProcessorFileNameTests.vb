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

    Private Shared Function OuvrirSansEcraser(chemin As String, ByRef cheminCree As String) As FileStream
        Dim methode = GetType(EmailProcessor).GetMethod("CreateUniqueAttachmentStream", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(methode, "La creation atomique des pieces jointes est absente.")
        Dim arguments As Object() = {chemin, Nothing}
        Dim flux = DirectCast(methode.Invoke(Nothing, arguments), FileStream)
        cheminCree = CStr(arguments(1))
        Return flux
    End Function

    <TestMethod>
    Public Sub CreateUniqueAttachmentStream_ConserveLeNomLibre()
        Dim dossier = Path.Combine(Path.GetTempPath(), "gt-email-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(dossier)
        Try
            Dim chemin = Path.Combine(dossier, "facture.pdf")
            Dim cheminCree As String = Nothing
            Using flux = OuvrirSansEcraser(chemin, cheminCree)
                Assert.AreEqual(chemin, cheminCree)
                flux.WriteByte(&H42)
            End Using
            Assert.AreEqual(&H42, File.ReadAllBytes(chemin)(0))
        Finally
            ' Nettoyage exclusif des fichiers temporaires crees par ce test.
            Directory.Delete(dossier, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CreateUniqueAttachmentStream_CollisionConcurrente_NecrasePasLePremierFichier()
        Dim dossier = Path.Combine(Path.GetTempPath(), "gt-email-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(dossier)
        Try
            Dim chemin = Path.Combine(dossier, "facture.pdf")
            Dim cheminCree As String = Nothing
            Using premier = New FileStream(chemin, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                premier.WriteByte(&H41)
                Using second = OuvrirSansEcraser(chemin, cheminCree)
                    Assert.AreNotEqual(chemin, cheminCree)
                    Assert.AreEqual(dossier, Path.GetDirectoryName(cheminCree))
                    second.WriteByte(&H42)
                End Using
            End Using
            Assert.AreEqual(&H41, File.ReadAllBytes(chemin)(0), "Le fichier existant a ete modifie.")
            Assert.AreEqual(&H42, File.ReadAllBytes(cheminCree)(0))
        Finally
            ' Nettoyage exclusif des fichiers temporaires crees par ce test.
            Directory.Delete(dossier, True)
        End Try
    End Sub
End Class
