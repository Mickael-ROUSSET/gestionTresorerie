Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailImportJournalTests
    Private Shared Function TemporaryJournal() As String
        Return Path.Combine(Path.GetTempPath(), "gmail-journal-" & Guid.NewGuid().ToString("N"), "journal.txt")
    End Function

    <TestMethod>
    Public Sub EnregistrementSurvitAuRedemarrage()
        Dim fileName = TemporaryJournal()
        Try
            Dim key = GmailImportJournal.MakeKey("AGUMAAA43@GMAIL.COM", 123UI, 456UI)
            Dim journal As New GmailImportJournal(fileName)
            Assert.IsFalse(journal.IsProcessed(key))
            journal.MarkProcessed(key)
            Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(key))
            journal.MarkProcessed(key)
            Assert.AreEqual(1, File.ReadAllLines(fileName).Length)
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub UidValidityEtCompteFontPartieDeLaCle()
        Dim first = GmailImportJournal.MakeKey("a@gmail.com", 12UI, 34UI)
        Assert.AreNotEqual(first, GmailImportJournal.MakeKey("b@gmail.com", 12UI, 34UI))
        Assert.AreNotEqual(first, GmailImportJournal.MakeKey("a@gmail.com", 13UI, 34UI))
        Assert.AreNotEqual(first, GmailImportJournal.MakeKey("a@gmail.com", 12UI, 35UI))
        Assert.AreEqual(first, GmailImportJournal.MakeKey("A@GMAIL.COM", 12UI, 34UI))
    End Sub

    <TestMethod>
    Public Sub AucunFichierAvantPremierSucces()
        Dim fileName = TemporaryJournal()
        Dim journal As New GmailImportJournal(fileName)
        Assert.IsFalse(File.Exists(fileName))
        Assert.IsFalse(journal.IsProcessed(GmailImportJournal.MakeKey("a@gmail.com", 1UI, 1UI)))
    End Sub

    <TestMethod>
    Public Sub InstancesOuvertesAvantEcriture_NeDupliquentPasLesCles()
        Dim fileName = TemporaryJournal()
        Try
            Dim premiere As New GmailImportJournal(fileName)
            Dim seconde As New GmailImportJournal(fileName)
            Dim cleA = GmailImportJournal.MakeKey("fictif@example.invalid", 1UI, 10UI)
            Dim cleB = GmailImportJournal.MakeKey("fictif@example.invalid", 1UI, 11UI)

            premiere.MarkProcessed(cleA)
            seconde.MarkProcessed(cleA)
            seconde.MarkProcessed(cleB)
            premiere.MarkProcessed(cleB)

            Dim lignes = File.ReadAllLines(fileName)
            Assert.AreEqual(2, lignes.Length)
            CollectionAssert.AreEquivalent(New String() {cleA, cleB}, lignes)
            Assert.IsTrue(premiere.IsProcessed(cleB))
            Assert.IsTrue(seconde.IsProcessed(cleA))
            Dim reouvert As New GmailImportJournal(fileName)
            Assert.IsTrue(reouvert.IsProcessed(cleA))
            Assert.IsTrue(reouvert.IsProcessed(cleB))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub VerrouTemporaire_EnregistrementAttendEtNeDupliquePas()
        Dim fileName = TemporaryJournal()
        Try
            Dim journal As New GmailImportJournal(fileName)
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 2UI, 21UI)
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim verrou As New FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            Dim liberation = System.Threading.Tasks.Task.Run(Sub()
                                                                System.Threading.Thread.Sleep(200)
                                                                verrou.Dispose()
                                                            End Sub)
            Try
                journal.MarkProcessed(cle)
                liberation.Wait()
            Finally
                liberation.Wait()
                verrou.Dispose()
            End Try

            Assert.AreEqual(1, File.ReadAllLines(fileName).Length)
            Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(cle))
            journal.MarkProcessed(cle)
            Assert.AreEqual(1, File.ReadAllLines(fileName).Length)
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DeuxInstancesConcurrentes_UneSeuleLignePourLaMemeCle()
        Dim fileName = TemporaryJournal()
        Dim depart As New System.Threading.ManualResetEventSlim(False)
        Dim prets As New System.Threading.CountdownEvent(2)
        Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 3UI, 31UI)
        Try
            Dim premiere As New GmailImportJournal(fileName)
            Dim seconde As New GmailImportJournal(fileName)
            Dim taches As System.Threading.Tasks.Task() = {
                System.Threading.Tasks.Task.Factory.StartNew(
                    Sub()
                        prets.Signal()
                        If Not depart.Wait(TimeSpan.FromSeconds(5)) Then Throw New TimeoutException("Départ simultané non déclenché.")
                        premiere.MarkProcessed(cle)
                    End Sub, System.Threading.Tasks.TaskCreationOptions.LongRunning),
                System.Threading.Tasks.Task.Factory.StartNew(
                    Sub()
                        prets.Signal()
                        If Not depart.Wait(TimeSpan.FromSeconds(5)) Then Throw New TimeoutException("Départ simultané non déclenché.")
                        seconde.MarkProcessed(cle)
                    End Sub, System.Threading.Tasks.TaskCreationOptions.LongRunning)
            }
            Try
                Assert.IsTrue(prets.Wait(TimeSpan.FromSeconds(5)), "Les deux écritures doivent être prêtes.")
                depart.Set()
                Assert.IsTrue(System.Threading.Tasks.Task.WaitAll(taches, TimeSpan.FromSeconds(10)), "Les deux écritures doivent se terminer.")
                CollectionAssert.AreEqual(New String() {cle}, File.ReadAllLines(fileName))
                Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(cle))
            Finally
                depart.Set()
                System.Threading.Tasks.Task.WaitAll(taches, TimeSpan.FromSeconds(10))
            End Try
        Finally
            prets.Dispose()
            depart.Dispose()
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub VerrouPersistant_EchecSansMarquageEtReprisePossible()
        Dim fileName = TemporaryJournal()
        Try
            Dim journal As New GmailImportJournal(fileName)
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 4UI, 41UI)
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Using verrou As New FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
                Assert.ThrowsExactly(Of IOException)(Sub() journal.MarkProcessed(cle))
            End Using
            Assert.IsFalse(journal.IsProcessed(cle))
            Assert.AreEqual(0, File.ReadAllLines(fileName).Length)
            journal.MarkProcessed(cle)
            CollectionAssert.AreEqual(New String() {cle}, File.ReadAllLines(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LectureSousVerrouTemporaire_AttendEtRetrouveLaCle()
        Dim fileName = TemporaryJournal()
        Try
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 5UI, 51UI)
            Dim premier As New GmailImportJournal(fileName)
            premier.MarkProcessed(cle)
            Dim verrou As New FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            Dim liberation = System.Threading.Tasks.Task.Run(Sub()
                                                                System.Threading.Thread.Sleep(200)
                                                                verrou.Dispose()
                                                            End Sub)
            Try
                Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(cle))
                liberation.Wait()
            Finally
                liberation.Wait()
                verrou.Dispose()
            End Try
            CollectionAssert.AreEqual(New String() {cle}, File.ReadAllLines(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LectureSousVerrouPersistant_EchouePuisPeutReprendre()
        Dim fileName = TemporaryJournal()
        Try
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 6UI, 61UI)
            Dim premier As New GmailImportJournal(fileName)
            premier.MarkProcessed(cle)
            Using verrou As New FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Assert.ThrowsExactly(Of IOException)(
                    Sub() Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(cle)))
            End Using
            Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(cle))
            CollectionAssert.AreEqual(New String() {cle}, File.ReadAllLines(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub MarquageReitere_RecreeLeJournalSupprimeApresLePremierSucces()
        Dim fileName = TemporaryJournal()
        Try
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 7UI, 71UI)
            Dim journal As New GmailImportJournal(fileName)
            journal.MarkProcessed(cle)
            File.Delete(fileName)
            Assert.IsFalse(File.Exists(fileName))

            ' Une cle en cache ne doit pas tenir lieu de preuve durable.
            journal.MarkProcessed(cle)
            CollectionAssert.AreEqual(New String() {cle}, File.ReadAllLines(fileName))
            Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(cle))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub MarquageReitere_RetablitUneCleAbsenteApresRemplacementDuJournal()
        Dim fileName = TemporaryJournal()
        Try
            Dim cleA = GmailImportJournal.MakeKey("fictif@example.invalid", 8UI, 81UI)
            Dim cleB = GmailImportJournal.MakeKey("fictif@example.invalid", 8UI, 82UI)
            Dim journal As New GmailImportJournal(fileName)
            journal.MarkProcessed(cleA)
            File.WriteAllText(fileName, cleB & Environment.NewLine)

            ' Le cache de la premiere instance contient encore cleA.
            journal.MarkProcessed(cleA)
            CollectionAssert.AreEquivalent(New String() {cleA, cleB}, File.ReadAllLines(fileName))
            Dim reouvert As New GmailImportJournal(fileName)
            Assert.IsTrue(reouvert.IsProcessed(cleA))
            Assert.IsTrue(reouvert.IsProcessed(cleB))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub IsProcessed_DetecteUneCleAjouteeParUneAutreInstance()
        Dim fileName = TemporaryJournal()
        Try
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 10UI, 101UI)
            Dim premiere As New GmailImportJournal(fileName)
            Dim seconde As New GmailImportJournal(fileName)
            Assert.IsFalse(seconde.IsProcessed(cle))

            premiere.MarkProcessed(cle)
            Assert.IsTrue(seconde.IsProcessed(cle), "Le cache initial ne doit pas masquer une ecriture externe.")
            Assert.IsTrue(seconde.IsProcessed(cle), "Une consultation repetee doit rester stable.")
            CollectionAssert.AreEqual(New String() {cle}, File.ReadAllLines(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub IsProcessed_NeConservePasUneCleApresSuppressionDuJournal()
        Dim fileName = TemporaryJournal()
        Try
            Dim cle = GmailImportJournal.MakeKey("fictif@example.invalid", 11UI, 111UI)
            Dim journal As New GmailImportJournal(fileName)
            journal.MarkProcessed(cle)
            Assert.IsTrue(journal.IsProcessed(cle))

            File.Delete(fileName)
            Assert.IsFalse(journal.IsProcessed(cle), "Un cache ne prouve pas la persistance d'un journal supprime.")
            Assert.IsFalse(File.Exists(fileName), "La lecture ne doit jamais recreer le journal.")
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub IsProcessed_RafraichitUnJournalRemplace()
        Dim fileName = TemporaryJournal()
        Try
            Dim cleA = GmailImportJournal.MakeKey("fictif@example.invalid", 12UI, 121UI)
            Dim cleB = GmailImportJournal.MakeKey("fictif@example.invalid", 12UI, 122UI)
            Dim journal As New GmailImportJournal(fileName)
            journal.MarkProcessed(cleA)
            Assert.IsTrue(journal.IsProcessed(cleA))

            File.WriteAllText(fileName, cleB & Environment.NewLine)
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow.AddSeconds(2))
            Assert.IsFalse(journal.IsProcessed(cleA))
            Assert.IsTrue(journal.IsProcessed(cleB))
            CollectionAssert.AreEqual(New String() {cleB}, File.ReadAllLines(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub JournalSansFinDeLigne_AjoutConserveDeuxClesDistinctes()
        Dim fileName = TemporaryJournal()
        Try
            Dim cleA = GmailImportJournal.MakeKey("fictif@example.invalid", 9UI, 91UI)
            Dim cleB = GmailImportJournal.MakeKey("fictif@example.invalid", 9UI, 92UI)
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            File.WriteAllText(fileName, cleA, New System.Text.UTF8Encoding(False))
            Dim journal As New GmailImportJournal(fileName)

            ' Un marquage repete ne doit ni dupliquer ni modifier le fichier.
            journal.MarkProcessed(cleA)
            Assert.AreEqual(cleA, File.ReadAllText(fileName))

            journal.MarkProcessed(cleB)
            CollectionAssert.AreEqual(New String() {cleA, cleB}, File.ReadAllLines(fileName))
            Dim reouvert As New GmailImportJournal(fileName)
            Assert.IsTrue(reouvert.IsProcessed(cleA))
            Assert.IsTrue(reouvert.IsProcessed(cleB))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub
End Class
