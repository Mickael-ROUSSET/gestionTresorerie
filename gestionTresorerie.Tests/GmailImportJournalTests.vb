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
            Assert.ThrowsExactly(Of InvalidDataException)(Sub() New GmailImportJournal(fileName))
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
            Assert.ThrowsExactly(Of InvalidDataException)(Sub() New GmailImportJournal(fileName))
            Assert.AreEqual(contents, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
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
End Class
