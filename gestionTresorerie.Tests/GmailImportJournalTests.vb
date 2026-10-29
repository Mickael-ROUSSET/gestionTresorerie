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
End Class
