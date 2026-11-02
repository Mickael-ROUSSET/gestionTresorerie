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

    Private Shared Function NewTempPath(folder As String) As String
        Return Path.Combine(folder, ".gmail-" & Guid.NewGuid().ToString("N") & ".tmp")
    End Function

    <TestMethod>
    Public Sub StockePremierFichierSansEcraser()
        Dim folder = NewFolder()
        Try
            Dim temp = NewTempPath(folder)
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
            Dim temp = NewTempPath(folder)
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
            Dim temp = NewTempPath(folder)
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
    Public Sub RefuseLeMemeCheminSansSupprimerLeFichier()
        Dim folder = NewFolder()
        Try
            Dim original = Path.Combine(folder, "facture.pdf")
            File.WriteAllText(original, "contenu a conserver")
            Dim refuseExact As Boolean = False
            Try
                AttachmentStorage.StoreTemporaryFile(original, original)
            Catch ex As ArgumentException
                refuseExact = True
            End Try
            Assert.IsTrue(refuseExact, "Le chemin identique doit etre refuse.")

            Dim refuseNormalise As Boolean = False
            Try
                AttachmentStorage.StoreTemporaryFile(original, Path.Combine(folder, ".", "facture.pdf"))
            Catch ex As ArgumentException
                refuseNormalise = True
            End Try
            Assert.IsTrue(refuseNormalise, "Le meme chemin normalise doit etre refuse.")
            Assert.IsTrue(File.Exists(original))
            Assert.AreEqual("contenu a conserver", File.ReadAllText(original))
            Assert.AreEqual(1, Directory.GetFiles(folder).Length)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RefuseUnFichierExistantQuiNestPasUnTemporaireGmail()
        Dim folder = NewFolder()
        Try
            Dim original = Path.Combine(folder, "document-original.pdf")
            Dim doublon = Path.Combine(folder, "document-deja-present.pdf")
            File.WriteAllText(original, "contenu identique")
            File.WriteAllText(doublon, "contenu identique")
            Dim refuse As Boolean = False
            Try
                AttachmentStorage.StoreTemporaryFile(original, Path.Combine(folder, "nouveau.pdf"))
            Catch ex As ArgumentException
                refuse = True
            End Try
            Assert.IsTrue(refuse, "Une source non temporaire ne doit jamais etre supprimee.")
            Assert.AreEqual("contenu identique", File.ReadAllText(original))
            Assert.AreEqual("contenu identique", File.ReadAllText(doublon))
            Assert.AreEqual(2, Directory.GetFiles(folder).Length)

            Dim fauxTemp = Path.Combine(folder, ".gmail-invalide.tmp")
            File.WriteAllText(fauxTemp, "a conserver")
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() AttachmentStorage.StoreTemporaryFile(fauxTemp, Path.Combine(folder, "autre.pdf")))
            Assert.AreEqual("a conserver", File.ReadAllText(fauxTemp))
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
