Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AttachmentStorageTests
    Private Shared Function NewFolder() As String
        Dim folder = Path.Combine(Path.GetTempPath(), "agumaaa-gmail-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Return folder
    End Function

    <TestMethod>
    Public Sub StockePremierFichierSansEcraser()
        Dim folder = NewFolder()
        Try
            Dim temp = Path.Combine(folder, ".temp")
            File.WriteAllText(temp, "facture 1")
            Dim storedPath = AttachmentStorage.StoreTemporaryFile(temp, Path.Combine(folder, "facture.pdf"))
            Assert.AreEqual(Path.Combine(folder, "facture.pdf"), storedPath)
            Assert.AreEqual("facture 1", File.ReadAllText(storedPath))
            Assert.IsFalse(File.Exists(temp))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub IgnoreMemeContenuSousNomDifferent()
        Dim folder = NewFolder()
        Try
            File.WriteAllText(Path.Combine(folder, "premier.pdf"), "contenu identique")
            Dim temp = Path.Combine(folder, ".temp")
            File.WriteAllText(temp, "contenu identique")
            Dim storedPath = AttachmentStorage.StoreTemporaryFile(temp, Path.Combine(folder, "deuxieme.pdf"))
            Assert.IsNull(storedPath)
            Assert.IsFalse(File.Exists(temp))
            Assert.IsFalse(File.Exists(Path.Combine(folder, "deuxieme.pdf")))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ConserveDeuxContenusDifferentsAuMemeNom()
        Dim folder = NewFolder()
        Try
            Dim first = Path.Combine(folder, "facture.pdf")
            File.WriteAllText(first, "facture 1")
            Dim temp = Path.Combine(folder, ".temp")
            File.WriteAllText(temp, "facture 2")
            Dim storedPath = AttachmentStorage.StoreTemporaryFile(temp, first)
            Assert.AreNotEqual(first, storedPath)
            Assert.AreEqual("facture 1", File.ReadAllText(first))
            Assert.AreEqual("facture 2", File.ReadAllText(storedPath))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub Sha256IdentiquePourDeuxFichiersEgaux()
        Dim folder = NewFolder()
        Try
            Dim a = Path.Combine(folder, "a.bin")
            Dim b = Path.Combine(folder, "b.bin")
            File.WriteAllBytes(a, New Byte() {0, 1, 2, 3})
            File.WriteAllBytes(b, New Byte() {0, 1, 2, 3})
            Assert.AreEqual(AttachmentStorage.ComputeSha256(a), AttachmentStorage.ComputeSha256(b))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub
End Class
