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

    <TestMethod>
    Public Sub MessageEmlInaccessible_NeBloquePasLesAutresPieces()
        Dim root = Path.Combine(Path.GetTempPath(), "gmail-eml-verrou-" & Guid.NewGuid().ToString("N"))
        Dim inbox = Path.Combine(root, "entree")
        Dim output = Path.Combine(root, "sortie")
        Directory.CreateDirectory(inbox)
        Try
            Dim lockedPath = Path.Combine(inbox, "a-verrouille.eml")
            File.WriteAllText(lockedPath, "Message fictif inaccessible")
            CreerMessage(Path.Combine(inbox, "b-valide.eml"), "fictif@example.invalid",
                         New DateTime(2026, 11, 20), "valide.pdf")
            Dim rejected As New List(Of String)()
            Using locked As New FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Dim files = GmailSimulatedInbox.Extract(inbox, output, Date.MinValue,
                                                        rejectedMessages:=rejected)
                Assert.AreEqual(1, files.Count)
                Assert.AreEqual("valide.pdf", Path.GetFileName(files(0)))
                CollectionAssert.AreEqual(New String() {"a-verrouille.eml"}, rejected)
            End Using
            Assert.IsTrue(File.Exists(lockedPath), "Le message source doit rester intact.")
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub SortieDansLeDossierSource_RefuseeSansModifierLesMessages()
        Dim root = Path.Combine(Path.GetTempPath(), "gmail-eml-isolation-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(root)
        Try
            Dim source = Path.Combine(root, "source.eml")
            CreerMessage(source, "fictif@example.invalid", New DateTime(2026, 11, 20), "piece.pdf")
            Dim before = File.ReadAllBytes(source)
            Dim exceptionLevee As Boolean = False
            Try
                GmailSimulatedInbox.Extract(root, Path.Combine(root, "extraction"), Date.MinValue)
            Catch ex As ArgumentException
                exceptionLevee = True
            End Try
            Assert.IsTrue(exceptionLevee, "Une sortie imbriquée dans le dossier source doit être refusée.")
            CollectionAssert.AreEqual(before, File.ReadAllBytes(source))
            Assert.IsFalse(Directory.Exists(Path.Combine(root, "extraction")))
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub PieceImpossibleAStocker_PoursuitLeLotEtSignaleLeMessage()
        Dim root = Directory.CreateTempSubdirectory("gmail-eml-stockage-").FullName
        Try
            Dim inbox = Path.Combine(root, "entree")
            Dim output = Path.Combine(root, "sortie")
            Directory.CreateDirectory(inbox)
            Directory.CreateDirectory(output)
            ' Un répertoire portant le nom du fichier empêche File.Move sans écraser quoi que ce soit.
            Directory.CreateDirectory(Path.Combine(output, "bloque.pdf"))
            CreerMessage(Path.Combine(inbox, "a-bloque.eml"), "fictif@example.invalid",
                         New DateTime(2026, 11, 20), "bloque.pdf")
            CreerMessage(Path.Combine(inbox, "b-valide.eml"), "fictif@example.invalid",
                         New DateTime(2026, 11, 20), "valide.pdf")
            Dim rejected As New List(Of String)()
            Dim files = GmailSimulatedInbox.Extract(inbox, output, Date.MinValue,
                                                    rejectedMessages:=rejected)
            Assert.AreEqual(1, files.Count, "La seconde pièce doit être conservée.")
            Assert.AreEqual("valide.pdf", Path.GetFileName(files(0)))
            CollectionAssert.AreEqual(New String() {"a-bloque.eml"}, rejected)
            Assert.IsTrue(Directory.Exists(Path.Combine(output, "bloque.pdf")))
            Assert.AreEqual(0, Directory.GetFiles(output, "*.tmp").Length)
            Assert.AreEqual(2, Directory.GetFiles(inbox, "*.eml").Length)
        Finally
            If Directory.Exists(root) Then Directory.Delete(root, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DeuxPiecesDansUnMessage_PremiereBloquee_SecondeExtraite()
        Dim root = Directory.CreateTempSubdirectory("gmail-eml-multipieces-").FullName
        Try
            Dim inbox = Path.Combine(root, "entree")
            Dim output = Path.Combine(root, "sortie")
            Directory.CreateDirectory(inbox)
            Directory.CreateDirectory(output)
            Directory.CreateDirectory(Path.Combine(output, "bloque.pdf"))
            Dim source = Path.Combine(inbox, "multi.eml")
            Using message As New MimeMessage()
                message.From.Add(New MailboxAddress("Fictif", "fictif@example.invalid"))
                message.To.Add(New MailboxAddress("TEST", "test@example.invalid"))
                message.Date = New DateTimeOffset(New DateTime(2026, 11, 20), TimeSpan.Zero)
                Dim builder As New BodyBuilder With {.TextBody = "Deux pièces fictives"}
                builder.Attachments.Add("bloque.pdf", Encoding.UTF8.GetBytes("PDF fictif bloqué"), ContentType.Parse("application/pdf"))
                builder.Attachments.Add("valide.pdf", Encoding.UTF8.GetBytes("PDF fictif valide"), ContentType.Parse("application/pdf"))
                message.Body = builder.ToMessageBody()
                Using stream = File.Create(source)
                    message.WriteTo(stream)
                End Using
            End Using
            Dim before = File.ReadAllBytes(source)
            Dim rejected As New List(Of String)()
            Dim files = GmailSimulatedInbox.Extract(inbox, output, Date.MinValue, rejectedMessages:=rejected)
            Assert.AreEqual(1, files.Count)
            Assert.AreEqual("valide.pdf", Path.GetFileName(files(0)))
            CollectionAssert.AreEqual(New String() {"multi.eml"}, rejected)
            CollectionAssert.AreEqual(before, File.ReadAllBytes(source))
            Assert.AreEqual(0, Directory.GetFiles(output, "*.tmp").Length)
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
