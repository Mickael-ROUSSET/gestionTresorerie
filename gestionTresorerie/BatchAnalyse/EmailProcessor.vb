Imports System.IO
Imports System.Net
Imports System.Text
Imports MailKit
Imports MailKit.Net.Imap
Imports MailKit.Search
Imports MimeKit

Public Class EmailProcessor
    ' Configuration
    'Private Shared ReadOnly EmailAddress As String = "agumaaa43@gmail.com"
    'Private Shared ReadOnly AppPassword As String = "votre_mot_de_passe_application" ' 16 caractères
    Private Shared LogFile As String

    ' --- Variables de session (Stats) ---
    Private Shared _statsMailsLus As Integer = 0
    Private Shared _statsMailsAvecPJ As Integer = 0
    Private Shared _statsTotalPJ As Integer = 0
    Private Shared _rapport As New StringBuilder()
    Private Shared _sRepDoc As String

    ''' <summary>
    ''' Point d'entrée principal modularisé
    ''' </summary>
    Public Shared Sub ProcessAttachments()
        ' Création du dossier s'il n'existe pas
        LogFile = Path.Combine(LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineDocuments"),
            "traitement_emails.log")
        _sRepDoc = LectureProprietes.GetVariable("repRacineAgumaaa") &
            LectureProprietes.GetVariable("repRacineDocuments") &
            LectureProprietes.GetVariable("repFichiersDocumentsATrier")
        ' Création du dossier s'il n'existe pas
        If Not Directory.Exists(_sRepDoc) Then Directory.CreateDirectory(_sRepDoc)

        PrepareDirectory(_sRepDoc)
        ResetSession()
        Dim journal As New GmailImportJournal(Path.Combine(_sRepDoc, "gmail_import_journal.txt"))

        Using client As New ImapClient()
            Try
                ConnectAndAuthenticate(client)
                Dim inbox = client.Inbox
                inbox.Open(FolderAccess.ReadOnly)

                ' Parcourir egalement les emails deja lus : le journal determine le travail restant.
                Dim uids = inbox.Search(SearchQuery.All)
                Dim account = LectureProprietes.GetVariable("SmtpUsername")
                Dim validity As UInteger = inbox.UidValidity
                For Each uid In uids
                    Dim key = GmailImportJournal.MakeKey(account, validity, uid.Id)
                    If journal.IsProcessed(key) Then Continue For
                    _statsMailsLus += 1
                    Try
                        ProcessSingleEmail(inbox, uid)
                        journal.MarkProcessed(key)
                    Catch ex As Exception
                        LogAndReportError($"Message UID {uid.Id} non traite : {ex.Message}")
                    End Try
                Next

                client.Disconnect(True)
            Catch ex As Exception
                LogAndReportError(ex.Message)
            Finally
                SaveFinalReport()
            End Try
        End Using
    End Sub

    ' --- Sous-Modules Spécialisés ---

    Private Shared Sub ConnectAndAuthenticate(ByRef client As ImapClient)
        client.Connect("imap.gmail.com", 993, True)
        'client.Authenticate(LectureProprietes.GetVariable("SmtpUsername"), LectureProprietes.GetVariable("SmtpPassword"))
        client.Authenticate(LectureProprietes.GetVariable("SmtpUsername"), GlobalSettings.GmailPassword)
    End Sub

    Private Shared Sub ProcessSingleEmail(inbox As IMailFolder, uid As UniqueId)
        Dim message = inbox.GetMessage(uid)
        Dim hasAttachments As Boolean = False
        Dim dateMail As DateTime = message.Date.DateTime

        ' Détermination du dossier de destination (Année/Mois)
        Dim targetPath As String = GetFolderByDate(dateMail)

        For Each attachment In message.Attachments
            If Not hasAttachments Then
                _statsMailsAvecPJ += 1
                hasAttachments = True
            End If

            SaveAttachment(attachment, targetPath, message.From.ToString())
        Next

        ' Ne pas modifier le statut lu/non lu : seul le journal fait foi.
    End Sub

    Private Shared Sub SaveAttachment(attachment As MimeEntity, targetFolder As String, senderInfo As String)
        ' 1. Récupération et nettoyage du nom
        Dim fileName As String = If(attachment.ContentDisposition?.FileName, attachment.ContentType.Name)
        fileName = CleanFileName(fileName)

        ' Ecrire d'abord dans un fichier temporaire reserve exclusivement.
        Dim desiredPath = Path.Combine(targetFolder, fileName)
        Dim temporaryPath = Path.Combine(targetFolder, ".gmail-" & Guid.NewGuid().ToString("N") & ".tmp")
        Try
            Using stream As New FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                If TypeOf attachment Is MessagePart Then
                    DirectCast(attachment, MessagePart).Message.WriteTo(stream)
                Else
                    DirectCast(attachment, MimePart).Content.DecodeTo(stream)
                End If
            End Using

            Dim storedPath = AttachmentStorage.StoreTemporaryFile(temporaryPath, desiredPath)
            If storedPath Is Nothing Then
                _rapport.AppendLine($"[PJ] Doublon SHA-256 ignoré : {fileName} (De: {senderInfo})")
                Return
            End If
            _statsTotalPJ += 1
            _rapport.AppendLine($"[PJ] Enregistré : {Path.GetFileName(storedPath)} (De: {senderInfo})")
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
        End Try
    End Sub

    ' --- Utilitaires ---

    Private Shared Function GetFolderByDate(dt As DateTime) As String
        Dim pathByDate = Path.Combine(_sRepDoc, dt.ToString("yyyy"), dt.ToString("MM"))
        If Not Directory.Exists(pathByDate) Then Directory.CreateDirectory(pathByDate)
        Return pathByDate
    End Function

    Private Shared Function CleanFileName(name As String) As String
        For Each c In Path.GetInvalidFileNameChars()
            name = name.Replace(c, "_"c)
        Next
        Return name
    End Function

    Private Shared Function EnsureUniqueFileName(pathFile As String) As String
        If Not File.Exists(pathFile) Then Return pathFile

        Dim dir = Path.GetDirectoryName(pathFile)
        Dim name = Path.GetFileNameWithoutExtension(pathFile)
        Dim ext = Path.GetExtension(pathFile)
        Dim candidat As String
        Do
            candidat = Path.Combine(dir, $"{name}_{Guid.NewGuid():N}{ext}")
        Loop While File.Exists(candidat)
        Return candidat
    End Function

    Private Shared Sub LogAndReportError(msg As String)
        _rapport.AppendLine($"[ERREUR] {msg}")
        File.AppendAllText("traitement_emails.log", $"{DateTime.Now} | CRITIQUE: {msg}{vbCrLf}")
    End Sub

    Private Shared Sub ResetSession()
        _statsMailsLus = 0 : _statsMailsAvecPJ = 0 : _statsTotalPJ = 0
        _rapport.Clear()
        _rapport.AppendLine("================================================")
        _rapport.AppendLine($"   RAPPORT DE TRAITEMENT - {DateTime.Now}")
        _rapport.AppendLine("================================================")
    End Sub

    Private Shared Sub PrepareDirectory(path As String)
        If Not Directory.Exists(path) Then Directory.CreateDirectory(path)
    End Sub

    Private Shared Sub SaveFinalReport()
        _rapport.AppendLine("------------------------------------------------")
        _rapport.AppendLine($"- Emails analysés : {_statsMailsLus}")
        _rapport.AppendLine($"- Emails avec PJ  : {_statsMailsAvecPJ}")
        _rapport.AppendLine($"- PJ enregistrées : {_statsTotalPJ}")
        _rapport.AppendLine("================================================")
        File.WriteAllText("Dernier_Rapport_Agumaaa.txt", _rapport.ToString())
        Logger.INFO(_rapport.ToString)
    End Sub

    Private Shared Sub LogMessage(msg As String)
        Dim logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {msg}"

        Dim LogFile = Path.Combine(LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineDocuments"), "BacASable")
        File.AppendAllText(LogFile, logLine & Environment.NewLine)
    End Sub
End Class