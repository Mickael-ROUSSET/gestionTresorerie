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
    Public Sub JournalCorrompuNePeutPasAutoriserUneRepriseSansBorne()
        Dim fileName = TemporaryJournal()
        Directory.CreateDirectory(Path.GetDirectoryName(fileName))
        Try
            File.WriteAllText(fileName, "not-a-checkpoint" & Environment.NewLine)
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub()
                    Dim journal As New GmailImportJournal(fileName)
                End Sub)
            Assert.AreEqual("not-a-checkpoint" & Environment.NewLine, File.ReadAllText(fileName))
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub

    <TestMethod>
    Public Sub SeulesLesClesValidesPeuventInitialiserLeJournal()
        Dim fileName = TemporaryJournal()
        Try
            Dim journal As New GmailImportJournal(fileName)
            Assert.IsFalse(journal.HasProcessedMessages)
            Assert.ThrowsExactly(Of ArgumentException)(Sub() journal.MarkProcessed("cle-invalide"))
            Assert.IsFalse(File.Exists(fileName))

            Dim key = GmailImportJournal.MakeKey("factice@example.invalid", 17UI, 42UI)
            journal.MarkProcessed(key)
            Assert.IsTrue(journal.HasProcessedMessages)
            Assert.IsTrue(New GmailImportJournal(fileName).HasProcessedMessages)
            Assert.AreEqual(1, File.ReadAllLines(fileName).Length)
        Finally
            If Directory.Exists(Path.GetDirectoryName(fileName)) Then Directory.Delete(Path.GetDirectoryName(fileName), True)
        End Try
    End Sub
End Class
