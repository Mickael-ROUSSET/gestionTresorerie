Imports System
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailImportRetryTests
    <TestMethod>
    Public Sub EchecPuisRepriseNeDupliquePasLeTraitement()
        Dim folder = Path.Combine(Path.GetTempPath(), "gmail-retry-" & Guid.NewGuid().ToString("N"))
        Dim journalPath = Path.Combine(folder, "journal.txt")
        Dim key = GmailImportJournal.MakeKey("test@example.org", 10UI, 20UI)
        Dim journal As New GmailImportJournal(journalPath)
        Dim attempts As Integer = 0
        Dim work As Action = Sub()
                                 attempts += 1
                                 If attempts = 1 Then Throw New IOException("Echec de stockage simule")
                             End Sub

        Try
            Dim failed As Boolean = False
            Try
                journal.ProcessOnce(key, work)
            Catch ex As IOException
                failed = True
            End Try

            Assert.IsTrue(failed, "Le premier essai doit echouer.")
            Assert.IsFalse(journal.IsProcessed(key), "Un echec ne doit pas etre journalise.")
            Assert.IsFalse(File.Exists(journalPath), "Aucun checkpoint avant succes.")

            Assert.IsTrue(journal.ProcessOnce(key, work), "Le second essai doit reussir.")
            Assert.AreEqual(2, attempts)
            Assert.IsFalse(journal.ProcessOnce(key, work), "Un message termine doit etre ignore.")
            Assert.AreEqual(2, attempts, "Ne pas retraiter un message deja valide.")
            Assert.AreEqual(1, File.ReadAllLines(journalPath).Length)
            Assert.IsTrue(New GmailImportJournal(journalPath).IsProcessed(key),
                          "Le checkpoint doit survivre au redemarrage.")
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Sub
End Class
