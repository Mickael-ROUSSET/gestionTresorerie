Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GestionFichiersTests
    Private _testDir As String

    <TestInitialize>
    Public Sub Initialiser()
        _testDir = Path.Combine(Path.GetTempPath(), "gestionTresorerieTests", Guid.NewGuid().ToString())
        Directory.CreateDirectory(_testDir)
    End Sub

    <TestCleanup>
    Public Sub Nettoyer()
        If Directory.Exists(_testDir) Then Directory.Delete(_testDir, True)
    End Sub

    Private Shared Sub CreerImage(chemin As String)
        Using image As New Bitmap(64, 64)
            Using dessin = Graphics.FromImage(image)
                dessin.Clear(Color.White)
                dessin.FillRectangle(Brushes.Black, 16, 16, 32, 32)
            End Using
            image.Save(chemin, ImageFormat.Png)
        End Using
    End Sub

    <TestMethod>
    Public Sub CalculerHashPerceptuel_SameImage_ShouldReturnSameHash()
        Dim imgPath1 = Path.Combine(_testDir, "img1.png")
        Dim imgPath2 = Path.Combine(_testDir, "img2.png")
        CreerImage(imgPath1)
        File.Copy(imgPath1, imgPath2)

        Dim hash1 = UtilitairesHash.CalculerHashPerceptuel(imgPath1)
        Dim hash2 = UtilitairesHash.CalculerHashPerceptuel(imgPath2)

        Assert.IsFalse(String.IsNullOrEmpty(hash1), "Le hash d'une image valide doit être calculé.")
        Assert.AreEqual(hash1, hash2)
    End Sub

    <TestMethod>
    <Timeout(10000)>
    Public Async Function DeplacerDoublons_ShouldMoveDuplicateFiles() As Task
        Dim fichier1 = Path.Combine(_testDir, "fichier1.png")
        Dim fichier2 = Path.Combine(_testDir, "fichier2.png")
        CreerImage(fichier1)
        File.Copy(fichier1, fichier2)

        Dim fichiersDeplaces = Await GestionDoublons.ExecuterTraitementAsync(_testDir)
        Dim dossierDoublons = Path.Combine(_testDir, "Doublons")

        Assert.IsTrue(Directory.Exists(dossierDoublons))
        Assert.HasCount(1, fichiersDeplaces)
        Assert.HasCount(1, Directory.GetFiles(dossierDoublons))
        Assert.HasCount(1, Directory.GetFiles(_testDir, "*.png"))
        Assert.IsTrue(File.Exists(fichiersDeplaces(0)))

        ' Une deuxième exécution doit exclure le dossier des doublons.
        Dim secondPassage = Await GestionDoublons.ExecuterTraitementAsync(_testDir)
        Assert.IsEmpty(secondPassage)
        Assert.HasCount(1, Directory.GetFiles(dossierDoublons))
    End Function

    <TestMethod>
    <Timeout(10000)>
    Public Async Function DeplacerDoublons_FichierNonImage_EstIgnore() As Task
        Dim chemin = Path.Combine(_testDir, "texte.txt")
        File.WriteAllText(chemin, "Données fictives, pas une image.")

        Dim deplaces = Await GestionDoublons.ExecuterTraitementAsync(_testDir)

        Assert.IsEmpty(deplaces)
        Assert.IsTrue(File.Exists(chemin))
    End Function

    <TestMethod>
    Public Sub CalculerHashPerceptuel_FichierAbsent_RetourneHashVide()
        Assert.AreEqual(String.Empty, UtilitairesHash.CalculerHashPerceptuel(Path.Combine(_testDir, "absent.png")))
    End Sub
End Class
