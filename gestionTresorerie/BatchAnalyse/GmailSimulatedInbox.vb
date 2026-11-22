Option Strict On
Imports System.IO
Imports MimeKit

''' <summary>
''' Source locale de messages fictifs (.eml), sans IMAP, Gmail ni accès à PROD.
''' Le dossier de sortie est fourni explicitement par l'appelant TEST.
''' </summary>
Friend NotInheritable Class GmailSimulatedInbox
    Private Sub New()
    End Sub

    Friend Shared Function Extract(emlFolder As String, destination As String, since As Date,
                                   Optional sender As String = "",
                                   Optional rejectedMessages As IList(Of String) = Nothing) As IReadOnlyList(Of String)
        If Not Directory.Exists(emlFolder) Then Throw New DirectoryNotFoundException("Dossier EML fictif introuvable.")
        If String.IsNullOrWhiteSpace(destination) Then Throw New ArgumentException("Destination TEST obligatoire.", NameOf(destination))
        Dim sourceRoot = Path.GetFullPath(emlFolder)
        Dim targetRoot = Path.GetFullPath(destination)
        If targetRoot.Equals(sourceRoot, StringComparison.OrdinalIgnoreCase) OrElse
           targetRoot.StartsWith(sourceRoot.TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar,
                                 StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException("La sortie doit être hors du dossier des messages fictifs.")
        End If
        sender = EmailProcessor.NormaliserExpediteur(sender)
        Directory.CreateDirectory(targetRoot)
        Dim saved As New List(Of String)
        For Each emlPath In Directory.EnumerateFiles(sourceRoot, "*.eml", SearchOption.TopDirectoryOnly).OrderBy(Function(p) p, StringComparer.OrdinalIgnoreCase)
            If (File.GetAttributes(emlPath) And FileAttributes.ReparsePoint) <> 0 Then Continue For
            Dim message As MimeMessage = Nothing
            Try
                message = MimeMessage.Load(emlPath)
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is FormatException OrElse
                                       TypeOf ex Is MimeKit.ParseException
                If rejectedMessages IsNot Nothing Then rejectedMessages.Add(Path.GetFileName(emlPath))
                Continue For
            End Try
            Using message
                If message.Date.Date < since.Date OrElse Not EmailProcessor.ExpediteurAccepte(message, sender) Then Continue For
                For Each part In message.Attachments.OfType(Of MimePart)()
                    Dim rawName = If(part.FileName, "")
                    Dim fileName = Path.GetFileName(rawName.Replace("\", "/"))
                    If String.IsNullOrWhiteSpace(fileName) Then Continue For
                    Dim extension = Path.GetExtension(fileName).ToLowerInvariant()
                    If Not New String() {".pdf", ".jpg", ".jpeg", ".png", ".tif", ".tiff"}.Contains(extension) Then Continue For
                    Dim safeName = New String(fileName.Select(Function(c) If("<>:""/\|?*".Contains(c) OrElse AscW(c) < 32, "_"c, c)).ToArray()).TrimEnd("."c, " "c)
                    If safeName = "" Then Continue For
                    Dim temporary = Path.Combine(targetRoot, ".fictif-" & Guid.NewGuid().ToString("N") & ".tmp")
                    Try
                        Using stream As New FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                            part.Content.DecodeTo(stream)
                        End Using
                        If New FileInfo(temporary).Length = 0 Then Continue For
                        Dim stored = AttachmentStorage.StoreTemporaryFile(temporary, Path.Combine(targetRoot, safeName))
                        If stored IsNot Nothing Then saved.Add(stored)
                    Finally
                        If File.Exists(temporary) Then File.Delete(temporary)
                    End Try
                Next
            End Using
        Next
        Return saved.AsReadOnly()
    End Function
End Class
