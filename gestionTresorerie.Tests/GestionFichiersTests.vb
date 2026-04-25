Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
<TestClass>
Public Class GestionFichiersTests

    Private ReadOnly _testDir As String = Path.Combine(Path.GetTempPath(), "gestionTresorerieTests")

    Public Sub New()
        ' Crée un dossier temporaire pour les tests
        If Not Directory.Exists(_testDir) Then Directory.CreateDirectory(_testDir)
    End Sub

    <TestMethod>
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

        Assert.AreEqual(hash1, hash2)
    End Sub

    <TestMethod>
    Public Async Function DeplacerDoublons_ShouldMoveDuplicateFiles() As Task
        ' Arrange
        Dim repertoireTest As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        Directory.CreateDirectory(repertoireTest)

        Dim fichier1 As String = Path.Combine(repertoireTest, "fichier1.txt")
        Dim fichier2 As String = Path.Combine(repertoireTest, "fichier2.txt")

        File.WriteAllText(fichier1, "contenu identique")
        File.WriteAllText(fichier2, "contenu identique")

        ' Act
        Dim fichiersDeplaces As List(Of String) =
        Await GestionDoublons.ExecuterTraitementAsync(repertoireTest)

        ' Assert
        Dim dossierDoublons As String = Path.Combine(repertoireTest, "Doublons")

        Assert.IsTrue(Directory.Exists(dossierDoublons))
        Assert.AreEqual(1, fichiersDeplaces.Count)
        Assert.AreEqual(1, Directory.GetFiles(dossierDoublons).Length)
    End Function
End Class

