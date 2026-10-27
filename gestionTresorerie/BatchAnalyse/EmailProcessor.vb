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
    Public Shared Sub ProcessAttachments(Optional initialImportCutoff As DateTime? = Nothing)
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
        Dim journalPath = Path.Combine(_sRepDoc, "gmail_import_journal.txt")
        Dim journal As GmailImportJournal
        Try
            journal = New GmailImportJournal(journalPath)
        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is InvalidDataException OrElse
                                   TypeOf ex Is DecoderFallbackException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is Global.System.Security.SecurityException
            _rapport.AppendLine("[ERREUR] Import Gmail bloque avant connexion : journal illisible ou incomplet. " & ex.Message)
            SaveFinalReport()
            Return
        End Try
        Dim hasJournalEntries = journal.HasEntries
        ' Memoriser la date de demarrage : elle doit rester appliquee apres la premiere session.
        Dim cutoffPath = Path.Combine(_sRepDoc, "gmail_import_cutoff.txt")
        Dim effectiveCutoff As DateTime?
        If Not TryResolveInitialImportCutoff(cutoffPath, initialImportCutoff, effectiveCutoff, _rapport) Then
            SaveFinalReport()
            Return
        End If
        If Not IsImportAllowed(hasJournalEntries, effectiveCutoff) Then
            _rapport.AppendLine("[ERREUR] Import Gmail bloque avant connexion : borne temporelle absente, meme avec un ancien journal. Fournir explicitement initialImportCutoff une fois.")
            SaveFinalReport()
            Return
        End If

        Using client As New ImapClient()
            Try
                ConnectAndAuthenticate(client)
                Dim inbox = client.Inbox
                inbox.Open(FolderAccess.ReadOnly)

                ' Parcourir egalement les emails deja lus, mais jamais avant la borne persistante.
                Dim uids = inbox.Search(SearchQuery.DeliveredAfter(effectiveCutoff.Value.Date))
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
        Dim fileName As String = GetAttachmentFileName(attachment.ContentDisposition?.FileName, attachment.ContentType.Name)
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

    ''' <summary>
    ''' Une borne de debut effective est obligatoire, meme avec un ancien journal.
    ''' Le parametre hasJournalEntries est conserve pour compatibilite avec les appelants.
    ''' La recherche IMAP utilise le jour inclus (date de reception, sans heure).
    ''' </summary>
    Public Shared Function IsImportAllowed(hasJournalEntries As Boolean, initialImportCutoff As DateTime?) As Boolean
        Return initialImportCutoff.HasValue
    End Function

    ' Conserver la borne de debut entre sessions pour ne jamais importer l'historique par accident.
    Private Shared Function TryResolveInitialImportCutoff(path As String, requested As DateTime?,
                                                        ByRef effectiveCutoff As DateTime?, report As StringBuilder) As Boolean
        effectiveCutoff = Nothing
        Try
            effectiveCutoff = ResolveInitialImportCutoff(path, requested)
            Return True
        Catch ex As Exception When TypeOf ex Is FormatException OrElse
                                   TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is Global.System.Security.SecurityException OrElse
                                   TypeOf ex Is InvalidOperationException
            report.AppendLine($"[ERREUR] Import Gmail bloque : date de debut persistante invalide ou inaccessible. Verifier gmail_import_cutoff.txt avant de relancer. {ex.Message}")
            Return False
        End Try
    End Function

    Public Shared Function ResolveInitialImportCutoff(path As String, requested As DateTime?) As DateTime?
        Dim contents As String = Nothing
        Try
            contents = File.ReadAllText(path)
        Catch ex As FileNotFoundException
            ' Aucun cutoff encore enregistre.
        Catch ex As DirectoryNotFoundException
            ' Conserver le comportement initial pour un dossier absent.
        End Try
        If contents Is Nothing AndAlso requested.HasValue Then
            Dim stream As FileStream = Nothing
            Try
                stream = New FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)
            Catch ex As IOException
                ' Un autre processus peut avoir cree le fichier depuis la lecture initiale.
                ' Toute valeur illisible ou incomplete doit continuer a bloquer l'import.
                contents = File.ReadAllText(path)
            End Try
            If stream IsNot Nothing Then
                Using stream
                    Using writer As New StreamWriter(stream, New UTF8Encoding(False), 1024, leaveOpen:=True)
                        writer.Write(requested.Value.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture))
                        writer.Flush()
                        stream.Flush(True)
                    End Using
                End Using
                Return requested.Value.Date
            End If
        End If
        If contents IsNot Nothing Then
            Dim stored = DateTime.ParseExact(contents.Trim(), "yyyy-MM-dd",
                Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None)
            If requested.HasValue AndAlso requested.Value.Date <> stored.Date Then
                Throw New InvalidOperationException("Date de debut Gmail differente de celle deja enregistree.")
            End If
            Return stored.Date
        End If
        Return Nothing
    End Function

    Private Shared Function GetFolderByDate(dt As DateTime) As String
        Dim pathByDate = Path.Combine(_sRepDoc, dt.ToString("yyyy"), dt.ToString("MM"))
        If Not Directory.Exists(pathByDate) Then Directory.CreateDirectory(pathByDate)
        Return pathByDate
    End Function

    Private Shared Function GetAttachmentFileName(dispositionName As String, contentTypeName As String) As String
        If Not String.IsNullOrWhiteSpace(dispositionName) Then Return CleanFileName(dispositionName)
        If Not String.IsNullOrWhiteSpace(contentTypeName) Then Return CleanFileName(contentTypeName)
        Return "attachment.bin"
    End Function

    Private Shared Function CleanFileName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then Return "attachment.bin"

        ' Appliquer les regles Windows meme si les tests tournent sur un autre OS.
        Dim cleaned As New StringBuilder(name.Length)
        For Each c As Char In name
            If AscW(c) >= 0 AndAlso AscW(c) < 32 OrElse "<>:""/\|?*".IndexOf(c) >= 0 Then
                cleaned.Append("_"c)
            Else
                cleaned.Append(c)
            End If
        Next
        name = cleaned.ToString().TrimEnd("."c, " "c)
        If String.IsNullOrWhiteSpace(name) Then Return "attachment.bin"

        ' Les noms de peripheriques restent reserves avec une ou plusieurs extensions.
        Dim basename = name.Split("."c)(0).TrimEnd(" "c)
        Dim reserved = basename.Equals("CON", StringComparison.OrdinalIgnoreCase) OrElse
            basename.Equals("PRN", StringComparison.OrdinalIgnoreCase) OrElse
            basename.Equals("AUX", StringComparison.OrdinalIgnoreCase) OrElse
            basename.Equals("NUL", StringComparison.OrdinalIgnoreCase)
        If basename.Length = 4 Then
            reserved = reserved OrElse
                ((basename.StartsWith("COM", StringComparison.OrdinalIgnoreCase) OrElse
                  basename.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) AndAlso
                 basename(3) >= "1"c AndAlso basename(3) <= "9"c)
        End If
        If reserved Then name = "_" & name
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
        AppendErrorToLogAndReport(LogFile, msg, _rapport)
    End Sub

    Private Shared Sub AppendErrorToLogAndReport(logPath As String, msg As String, report As StringBuilder)
        report.AppendLine($"[ERREUR] {msg}")
        Try
            File.AppendAllText(logPath, $"{DateTime.Now} | CRITIQUE: {msg}{vbCrLf}")
        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is NotSupportedException OrElse
                                   TypeOf ex Is Global.System.Security.SecurityException
            report.AppendLine($"[ERREUR JOURNAL] Ecriture du journal impossible : {ex.Message}")
        End Try
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
        WriteFinalReport(_sRepDoc, _rapport.ToString(), AddressOf Logger.INFO)
    End Sub

    Private Shared Sub WriteFinalReport(reportDirectory As String, report As String, logReport As Action(Of String))
        ' Le rapport est un diagnostic : ses erreurs ne doivent pas masquer l'echec Gmail.
        Try
            If String.IsNullOrWhiteSpace(reportDirectory) Then
                Throw New ArgumentException("Dossier du rapport absent.", NameOf(reportDirectory))
            End If
            File.WriteAllText(Path.Combine(reportDirectory, "Dernier_Rapport_Agumaaa.txt"), report)
        Catch ex As Exception
            ' Conserver le rapport et l'erreur initiale, meme si le fichier est inaccessible.
        End Try

        Try
            logReport(report)
        Catch ex As Exception
            ' La journalisation peut elle aussi echouer pendant le traitement d'une erreur.
        End Try
    End Sub

    Private Shared Sub LogMessage(msg As String)
        Dim logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {msg}"

        Dim LogFile = Path.Combine(LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineDocuments"), "BacASable")
        File.AppendAllText(LogFile, logLine & Environment.NewLine)
    End Sub
End Class
