Imports System.IO
Imports Xunit
Imports gestionTresorerie

Public Class GestionFichiersTests

    Private ReadOnly _testDir As String = Path.Combine(Path.GetTempPath(), "gestionTresorerieTests")

    Public Sub New()
        ' Crée un dossier temporaire pour les tests
        If Not Directory.Exists(_testDir) Then Directory.CreateDirectory(_testDir)
    End Sub

    <Fact>
    Public Sub CalculerHashPerceptuel_SameImage_ShouldReturnSameHash()
        ' Préparer deux copies identiques d'une image
        Dim imgPath1 As String = Path.Combine(_testDir, "img1.jpg")
        Dim imgPath2 As String = Path.Combine(_testDir, "img2.jpg")

        ' Pour le test, tu peux copier une image existante ou générer un Bitmap
        File.Copy("chemin_vers_image_source.jpg", imgPath1, True)
        File.Copy(imgPath1, imgPath2, True)

        ' Appel à la fonction
        Dim hash1 = UtilitairesHash.CalculerHashPerceptuel(imgPath1)
        Dim hash2 = UtilitairesHash.CalculerHashPerceptuel(imgPath2)

        Assert.Equal(hash1, hash2)
    End Sub

    <Fact>
    Public Sub DeplacerDoublons_ShouldMoveDuplicateFiles()
        ' Préparer des fichiers doublons
        Dim file1 = Path.Combine(_testDir, "fichierA.txt")
        Dim file2 = Path.Combine(_testDir, "fichierB.txt")
        File.WriteAllText(file1, "Contenu identique")
        File.WriteAllText(file2, "Contenu identique")

        ' Appel à la méthode de détection/déplacement
        Dim deplaces = GestionDoublons.DeplacerDoublonsAvecProgress(_testDir)

        ' Vérifie que l'un des fichiers a été déplacé
        Assert.Contains(deplaces, Function(f) Path.GetFileName(f) = "fichierB.txt" Or Path.GetFileName(f) = "fichierA.txt")
    End Sub
End Class

