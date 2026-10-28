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
    Public Sub RefuseDestinationIdentiqueSansSupprimerLeFichier()
        Dim folder = NewFolder()
        Try
            Dim original = Path.Combine(folder, "facture.pdf")
            File.WriteAllText(original, "facture fictive a conserver")

            AssertDestinationIdentiqueRejetee(original, original)
            AssertDestinationIdentiqueRejetee(original, Path.Combine(folder, ".", "facture.pdf"))

            Assert.IsTrue(File.Exists(original))
            Assert.AreEqual("facture fictive a conserver", File.ReadAllText(original))
            Assert.IsFalse(File.Exists(Path.Combine(folder, ".gmail-storage.lock")))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    Private Shared Sub AssertDestinationIdentiqueRejetee(temp As String, desired As String)
        Try
            AttachmentStorage.StoreTemporaryFile(temp, desired)
            Assert.Fail("Un chemin de destination identique doit etre refuse.")
        Catch ex As ArgumentException
            Assert.AreEqual("desiredPath", ex.ParamName)
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

    <TestMethod>
    Public Sub LeFichierDeVerrouNEstPasUnePieceJointeADedoublonner()
        Dim folder = NewFolder()
        Try
            File.WriteAllText(Path.Combine(folder, ".gmail-storage.lock"), "contenu fictif")
            Dim temp = Path.Combine(folder, ".gmail-test.tmp")
            File.WriteAllText(temp, "contenu fictif")
            Dim desired = Path.Combine(folder, "facture.pdf")

            Assert.AreEqual(desired, AttachmentStorage.StoreTemporaryFile(temp, desired))
            Assert.AreEqual("contenu fictif", File.ReadAllText(desired))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DeuxImportsConcurrentsNeDupliquentPasLeMemeContenu()
        Dim folder = NewFolder()
        Try
            Dim firstTemp = Path.Combine(folder, ".gmail-a.tmp")
            Dim secondTemp = Path.Combine(folder, ".gmail-b.tmp")
            File.WriteAllText(firstTemp, "facture fictive identique")
            File.WriteAllText(secondTemp, "facture fictive identique")
            Dim desired = Path.Combine(folder, "facture.pdf")

            Using barrier As New System.Threading.Barrier(2)
                Dim first = System.Threading.Tasks.Task.Run(
                    Function() StoreAfterBarrier(firstTemp, desired, barrier))
                Dim second = System.Threading.Tasks.Task.Run(
                    Function() StoreAfterBarrier(secondTemp, desired, barrier))
                System.Threading.Tasks.Task.WaitAll(first, second)

                Assert.IsTrue((first.Result Is Nothing) Xor (second.Result Is Nothing))
                Dim stored = If(first.Result, second.Result)
                Assert.AreEqual("facture fictive identique", File.ReadAllText(stored))
                Assert.AreEqual(1, Directory.GetFiles(folder, "*.pdf").Length)
                Assert.IsFalse(File.Exists(firstTemp))
                Assert.IsFalse(File.Exists(secondTemp))
            End Using
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DeuxImportsConcurrentsDeContenusDifferentsConserventLesDeuxFichiers()
        Dim folder = NewFolder()
        Try
            Dim firstTemp = Path.Combine(folder, ".gmail-a.tmp")
            Dim secondTemp = Path.Combine(folder, ".gmail-b.tmp")
            File.WriteAllText(firstTemp, "facture fictive A")
            File.WriteAllText(secondTemp, "facture fictive B")
            Dim desired = Path.Combine(folder, "facture.pdf")

            Using barrier As New System.Threading.Barrier(2)
                Dim first = System.Threading.Tasks.Task.Run(
                    Function() StoreAfterBarrier(firstTemp, desired, barrier))
                Dim second = System.Threading.Tasks.Task.Run(
                    Function() StoreAfterBarrier(secondTemp, desired, barrier))
                System.Threading.Tasks.Task.WaitAll(first, second)

                Assert.IsNotNull(first.Result)
                Assert.IsNotNull(second.Result)
                Assert.AreNotEqual(first.Result, second.Result)
                Assert.AreEqual(2, Directory.GetFiles(folder, "*.pdf").Length)
                Dim contents = New String() {File.ReadAllText(first.Result), File.ReadAllText(second.Result)}
                CollectionAssert.AreEquivalent(New String() {"facture fictive A", "facture fictive B"}, contents)
                Assert.IsFalse(File.Exists(firstTemp))
                Assert.IsFalse(File.Exists(secondTemp))
            End Using
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    Private Shared Function StoreAfterBarrier(temp As String, desired As String,
                                             barrier As System.Threading.Barrier) As String
        Assert.IsTrue(barrier.SignalAndWait(TimeSpan.FromSeconds(10)))
        Return AttachmentStorage.StoreTemporaryFile(temp, desired)
    End Function

    <TestMethod>
    Public Sub VerrouIndisponibleNeSupprimePasLeFichierTemporaire()
        Dim folder = NewFolder()
        Try
            Dim temp = Path.Combine(folder, ".gmail-test.tmp")
            File.WriteAllText(temp, "facture fictive a conserver")
            Dim desired = Path.Combine(folder, "facture.pdf")
            Dim lockPath = Path.Combine(folder, ".gmail-storage.lock")

            Using held As New FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
                Try
                    AttachmentStorage.StoreTemporaryFile(temp, desired)
                    Assert.Fail("Le stockage doit refuser un verrou indisponible.")
                Catch ex As IOException
                    StringAssert.Contains(ex.Message, "Verrou de stockage Gmail indisponible")
                End Try
            End Using

            Assert.IsTrue(File.Exists(temp))
            Assert.AreEqual("facture fictive a conserver", File.ReadAllText(temp))
            Assert.IsFalse(File.Exists(desired))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AttendLaLiberationDuVerrouAvantDeSauvegarder()
        Dim folder = NewFolder()
        Try
            Dim temp = Path.Combine(folder, ".gmail-test.tmp")
            File.WriteAllText(temp, "facture fictive")
            Dim desired = Path.Combine(folder, "facture.pdf")
            Dim lockPath = Path.Combine(folder, ".gmail-storage.lock")
            Dim pending As System.Threading.Tasks.Task(Of String)

            Using held As New FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
                Using started As New System.Threading.ManualResetEventSlim(False)
                    pending = System.Threading.Tasks.Task.Run(
                        Function()
                            started.Set()
                            Return AttachmentStorage.StoreTemporaryFile(temp, desired)
                        End Function)
                    Assert.IsTrue(started.Wait(TimeSpan.FromSeconds(5)))
                    Assert.IsFalse(pending.Wait(200), "Le stockage ne doit pas passer outre le verrou.")
                End Using
            End Using

            Assert.AreEqual(desired, pending.GetAwaiter().GetResult())
            Assert.AreEqual("facture fictive", File.ReadAllText(desired))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub
End Class
