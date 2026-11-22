Option Strict On
Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports MimeKit

<TestClass>
Public Class GmailSimulatedInboxTests
    <TestMethod>
    Public Sub ExtraitUniquementPiecesFictivesFiltreesEtSansReseau()
        Dim root = Path.Combine(Path.GetTempPath(), "gmail-eml-test-" & Guid.NewGuid().ToString("N"))
        Dim inbox = Path.Combine(root, "entree")
        Dim output = Path.Combine(root, "sortie")
        Try
            Directory.CreateDirectory(inbox)
            CreerMessage(Path.Combine(inbox, "a.eml"), "fictif@example.invalid", New DateTime(2026, 11, 20), "facture.pdf")
            CreerMessage(Path.Combine(inbox, "b.eml"), "autre@example.invalid", New DateTime(2026, 11, 20), "autre.pdf")
            CreerMessage(Path.Combine(inbox, "c.eml"), "fictif@example.invalid", New DateTime(2026, 10, 1), "ancien.pdf")
            Dim first = GmailSimulatedInbox.Extract(inbox, output, New DateTime(2026, 11, 1), "fictif@example.invalid")
            Assert.AreEqual(1, first.Count)
            Assert.AreEqual("facture.pdf", Path.GetFileName(first(0)))
            Assert.IsTrue(New FileInfo(first(0)).Length > 0)
            Dim again = GmailSimulatedInbox.Extract(inbox, output, New DateTime(2026, 11, 1), "fictif@example.invalid")
            Assert.AreEqual(0, again.Count, "Une pièce identique ne doit pas être réimportée.")
            Assert.AreEqual(3, Directory.GetFiles(inbox).Length, "Les messages fictifs ne doivent pas être modifiés.")
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Sub

    Private Shared Sub CreerMessage(path As String, sender As String, sent As DateTime, filename As String)
        Using message As New MimeMessage()
            message.From.Add(New MailboxAddress("Fictif", sender))
            message.To.Add(New MailboxAddress("TEST", "test@example.invalid"))
            message.Date = New DateTimeOffset(sent, TimeSpan.Zero)
            message.Subject = "Document fictif"
            Dim builder As New BodyBuilder With {.TextBody = "Message simulé"}
            builder.Attachments.Add(filename, Encoding.UTF8.GetBytes("Contenu fictif PDF"), ContentType.Parse("application/pdf"))
            message.Body = builder.ToMessageBody()
            Using stream = File.Create(path)
                message.WriteTo(stream)
            End Using
        End Using
    End Sub
End Class
