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
        Assert.IsFalse(journal.HasEntries)
        Assert.IsFalse(journal.IsProcessed(GmailImportJournal.MakeKey("a@gmail.com", 1UI, 1UI)))
    End Sub

    <TestMethod>
    Public Sub JournalCompletEstReluEtDetecteCommeNonVide()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim key = GmailImportJournal.MakeKey("fictif@example.org", 1UI, 2UI)
            File.WriteAllText(fileName, key & vbCrLf)
            Dim journal As New GmailImportJournal(fileName)
            Assert.IsTrue(journal.HasEntries)
            Assert.IsTrue(journal.IsProcessed(key))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub JournalTronqueSansFinDeLigneEstRefuse()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim key = GmailImportJournal.MakeKey("fictif@example.org", 1UI, 2UI)
            File.WriteAllText(fileName, key)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub()
                    Dim ignored As New GmailImportJournal(fileName)
                End Sub)
            Assert.AreEqual(key, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub JournalAvecCleMalformeeEstRefuseSansReecriture()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim valid = GmailImportJournal.MakeKey("fictif@example.org", 1UI, 2UI)
            Dim contents = valid & vbLf & "cle-invalide" & vbLf
            File.WriteAllText(fileName, contents)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub()
                    Dim ignored As New GmailImportJournal(fileName)
                End Sub)
            Assert.AreEqual(contents, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub JournalAvecOctetsUtf8InvalidesEstRefuse()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            File.WriteAllBytes(fileName, New Byte() {&HFF, &HA})
            Assert.ThrowsExactly(Of System.Text.DecoderFallbackException)(
                Sub()
                    Dim ignored As New GmailImportJournal(fileName)
                End Sub)
            CollectionAssert.AreEqual(New Byte() {&HFF, &HA}, File.ReadAllBytes(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub EcritureCleInvalideNeCreePasDeJournal()
        Dim fileName = TemporaryJournal()
        Dim journal As New GmailImportJournal(fileName)
        Assert.ThrowsExactly(Of ArgumentException)(Sub() journal.MarkProcessed("cle-invalide"))
        Assert.IsFalse(File.Exists(fileName))
        Assert.IsFalse(journal.HasEntries)
    End Sub

    <TestMethod>
    Public Sub JournalExclusivementVideResteValide()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            File.WriteAllText(fileName, String.Empty)
            Dim journal As New GmailImportJournal(fileName)
            Assert.IsFalse(journal.HasEntries)
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DeuxInstancesNEnregistrentPasDeuxFoisLaMemeCle()
        Dim fileName = TemporaryJournal()
        Try
            Dim key = GmailImportJournal.MakeKey("fictif@example.org", 11UI, 12UI)
            Dim first As New GmailImportJournal(fileName)
            Dim second As New GmailImportJournal(fileName)
            first.MarkProcessed(key)
            second.MarkProcessed(key)
            Assert.AreEqual(1, File.ReadAllLines(fileName).Length)
            Assert.IsTrue(second.IsProcessed(key))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DeuxInstancesConserventLesClesAjouteesEntreTemps()
        Dim fileName = TemporaryJournal()
        Try
            Dim firstKey = GmailImportJournal.MakeKey("fictif@example.org", 11UI, 12UI)
            Dim secondKey = GmailImportJournal.MakeKey("fictif@example.org", 11UI, 13UI)
            Dim first As New GmailImportJournal(fileName)
            Dim second As New GmailImportJournal(fileName)
            first.MarkProcessed(firstKey)
            second.MarkProcessed(secondKey)
            Assert.AreEqual(2, File.ReadAllLines(fileName).Length)
            Dim restarted As New GmailImportJournal(fileName)
            Assert.IsTrue(restarted.IsProcessed(firstKey))
            Assert.IsTrue(restarted.IsProcessed(secondKey))
            Assert.IsTrue(second.IsProcessed(firstKey))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CorruptionApresCreationEstRefuseeAvantAjout()
        Dim fileName = TemporaryJournal()
        Try
            Dim firstKey = GmailImportJournal.MakeKey("fictif@example.org", 11UI, 12UI)
            Dim secondKey = GmailImportJournal.MakeKey("fictif@example.org", 11UI, 13UI)
            Dim journal As New GmailImportJournal(fileName)
            journal.MarkProcessed(firstKey)
            File.AppendAllText(fileName, "ligne-incomplete")
            Dim contentsBefore = File.ReadAllText(fileName)
            Assert.ThrowsExactly(Of InvalidDataException)(Sub() journal.MarkProcessed(secondKey))
            Assert.AreEqual(contentsBefore, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub JournalHistoriqueAvecBOMEtLignesVidesResteLisible()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim firstKey = GmailImportJournal.MakeKey("fictif@example.invalid", 11UI, 12UI)
            Dim secondKey = GmailImportJournal.MakeKey("fictif@example.invalid", 11UI, 13UI)
            Dim contents = vbCrLf & firstKey & vbCrLf & vbCrLf & firstKey & vbCrLf
            File.WriteAllText(fileName, contents, New System.Text.UTF8Encoding(True))
            Dim originalBytes = File.ReadAllBytes(fileName)

            Dim journal As New GmailImportJournal(fileName)
            Assert.IsTrue(journal.HasEntries)
            Assert.IsTrue(journal.IsProcessed(firstKey))
            Assert.IsFalse(journal.IsProcessed(secondKey))
            CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(fileName))

            journal.MarkProcessed(secondKey)
            Dim restarted As New GmailImportJournal(fileName)
            Assert.IsTrue(restarted.IsProcessed(firstKey))
            Assert.IsTrue(restarted.IsProcessed(secondKey))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LignesVidesAccepteesMaisLigneBlancheInvalideRefusee()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            File.WriteAllText(fileName, vbCrLf & vbLf)
            Dim journal As New GmailImportJournal(fileName)
            Assert.IsFalse(journal.HasEntries)

            File.WriteAllText(fileName, vbCrLf & " " & vbLf)
            Dim originalBytes = File.ReadAllBytes(fileName)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub()
                    Dim ignored As New GmailImportJournal(fileName)
                End Sub)
            CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CompteHexAvecUtf8InvalideEstRefuseSansModifierLeJournal()
        Dim fileName = TemporaryJournal()
        Try
            Dim invalidKey = "FF:1:2"
            Dim journal As New GmailImportJournal(fileName)
            Assert.ThrowsExactly(Of ArgumentException)(Sub() journal.MarkProcessed(invalidKey))
            Assert.IsFalse(File.Exists(fileName))

            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim original = invalidKey & vbLf
            File.WriteAllText(fileName, original)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub()
                    Dim ignored As New GmailImportJournal(fileName)
                End Sub)
            Assert.AreEqual(original, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CompteUnicodeValideEstConserveApresRedemarrage()
        Dim fileName = TemporaryJournal()
        Try
            Dim key = GmailImportJournal.MakeKey("ÉLÈVE@example.invalid", 11UI, 12UI)
            Dim journal As New GmailImportJournal(fileName)
            journal.MarkProcessed(key)
            Assert.IsTrue(New GmailImportJournal(fileName).IsProcessed(key))
            Assert.AreEqual(key, File.ReadAllLines(fileName)(0))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CompteHexNulOuBlancEstRefuse()
        Dim fileName = TemporaryJournal()
        Dim journal As New GmailImportJournal(fileName)
        Assert.ThrowsExactly(Of ArgumentException)(Sub() journal.MarkProcessed("00:1:2"))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() journal.MarkProcessed("20:1:2"))
        Assert.IsFalse(File.Exists(fileName))
    End Sub
    <TestMethod>
    Public Sub MakeKeyRefuseLesCaracteresDeControleAvantToutJournal()
        Dim invalidAccounts = New String() {
            "fictif" & ChrW(0) & "@example.invalid",
            "fictif" & vbLf & "@example.invalid",
            "fictif" & vbTab & "@example.invalid"
        }
        For Each invalidAccount In invalidAccounts
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() GmailImportJournal.MakeKey(invalidAccount, 11UI, 12UI))
        Next
    End Sub

    <TestMethod>
    Public Sub MakeKeyRefuseUneSequenceUnicodeInvalide()
        Dim invalidAccount = "fictif" & ChrW(&HD800) & "@example.invalid"
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() GmailImportJournal.MakeKey(invalidAccount, 11UI, 12UI))
    End Sub

    <TestMethod>
    Public Sub ClesHexAvecCaracteresDeControleSontRefuseesSansAltererLeJournal()
        Dim fileName = TemporaryJournal()
        Try
            Dim invalidKeys = New String() {"09:11:12", "0A:11:12", "0D:11:12"}
            For Each invalidKey In invalidKeys
                Dim journal As New GmailImportJournal(fileName)
                Assert.ThrowsExactly(Of ArgumentException)(Sub() journal.MarkProcessed(invalidKey))
                Assert.IsFalse(File.Exists(fileName))

                Directory.CreateDirectory(Path.GetDirectoryName(fileName))
                Dim original = invalidKey & vbLf
                File.WriteAllText(fileName, original)
                Assert.ThrowsExactly(Of InvalidDataException)(
                    Sub()
                        Dim ignored As New GmailImportJournal(fileName)
                    End Sub)
                Assert.AreEqual(original, File.ReadAllText(fileName))
                File.Delete(fileName)
            Next
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub MakeKeyRefuseLesControlesMemeEnBordureDuCompte()
        Dim invalidAccounts = New String() {
            vbTab & "fictif@example.invalid",
            "fictif@example.invalid" & vbCr,
            vbLf & "fictif@example.invalid"
        }
        For Each invalidAccount In invalidAccounts
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() GmailImportJournal.MakeKey(invalidAccount, 11UI, 12UI))
        Next
    End Sub

    <TestMethod>
    Public Sub CheminJournalOccupéParUnDossierEstRefuseSansLeModifier()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(fileName)
            Dim refused As Boolean = False
            Try
                Dim ignored As New GmailImportJournal(fileName)
            Catch ex As UnauthorizedAccessException
                refused = True
            Catch ex As IOException
                refused = True
            End Try
            Assert.IsTrue(refused, "Un dossier ne doit jamais etre traite comme un journal absent.")
            Assert.IsTrue(Directory.Exists(fileName))
            Assert.AreEqual(0, Directory.GetFileSystemEntries(fileName).Length)
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub JournalVerrouilleEstRefuseSansEcriture()
        Dim fileName = TemporaryJournal()
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(fileName))
            Dim key = GmailImportJournal.MakeKey("fictif@example.invalid", 11UI, 12UI)
            Dim original = key & vbLf
            File.WriteAllText(fileName, original)
            Using locked As New FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Assert.ThrowsExactly(Of IOException)(
                    Sub()
                        Dim ignored As New GmailImportJournal(fileName)
                    End Sub)
            End Using
            Assert.AreEqual(original, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub
End Class
