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
    Public Shared Sub ProcessAttachments(Optional enableDocumentProposals As Boolean = False,
                                         Optional initialImportCutoff As DateTime? = Nothing)
        ' Création du dossier s'il n'existe pas
        LogFile = Path.Combine(LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineDocuments"),
            "traitement_emails.log")
        _sRepDoc = LectureProprietes.GetVariable("repRacineAgumaaa") &
            LectureProprietes.GetVariable("repRacineDocuments") &
            LectureProprietes.GetVariable("repFichiersDocumentsATrier")
        _sRepDoc = Path.GetFullPath(_sRepDoc)
        ' Création du dossier s'il n'existe pas
        If Not Directory.Exists(_sRepDoc) Then Directory.CreateDirectory(_sRepDoc)

        PrepareDirectory(_sRepDoc)
        ResetSession()
        Dim journal As New GmailImportJournal(Path.Combine(_sRepDoc, "gmail_import_journal.txt"))
        ' Sans borne explicite ou persistante, ne jamais parcourir toute la boite Gmail.
        Dim effectiveCutoff As DateTime
        Try
            effectiveCutoff = ResolveInitialImportCutoff(
                Path.Combine(_sRepDoc, "gmail_import_cutoff.txt"), initialImportCutoff)
        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is InvalidOperationException OrElse
                                   TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is Global.System.Security.SecurityException
            _rapport.AppendLine("[ERREUR] Import Gmail bloque avant connexion : " & ex.Message)
            SaveFinalReport()
            Return
        End Try

        Using client As New ImapClient()
            Try
                ConnectAndAuthenticate(client)
                Dim inbox = client.Inbox
                inbox.Open(FolderAccess.ReadOnly)

                ' Parcourir egalement les emails deja lus : le journal determine le travail restant.
                Dim uids = inbox.Search(SearchQuery.DeliveredAfter(effectiveCutoff))
                Dim account = LectureProprietes.GetVariable("SmtpUsername")
                Dim validity As UInteger = inbox.UidValidity
                For Each uid In uids
                    Dim key = GmailImportJournal.MakeKey(account, validity, uid.Id)
                    If journal.IsProcessed(key) Then Continue For
                    _statsMailsLus += 1
                    Try
                        ' Le journal est valide uniquement apres le succes complet de l'import.
                        Dim savedPaths As New List(Of String)()
                        journal.ProcessOnce(key, Sub() savedPaths = ProcessSingleEmail(inbox, uid))
                        ' Desactive par defaut : jamais d'analyse documentaire implicite.
                        ' La revue est distincte du journal Gmail et ne valide aucune operation.
                        If enableDocumentProposals AndAlso savedPaths.Count > 0 Then
                            Try
                                Dim review = ProposeSavedAttachmentsAsync(_sRepDoc, savedPaths).GetAwaiter().GetResult()
                                _rapport.AppendLine($"[REVUE] Propositions : {review.Created}, deja presentes : {review.AlreadyPresent}, erreurs : {review.Errors.Count}")
                                For Each reviewError In review.Errors
                                    _rapport.AppendLine("[REVUE] " & reviewError)
                                Next
                            Catch reviewEx As Exception
                                ' L'import est deja journalise : conserver la possibilite de reprise hors ligne.
                                _rapport.AppendLine("[REVUE] Echec sans validation automatique : " & reviewEx.Message)
                            End Try
                        End If
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

    ''' <summary>
    ''' Reprise hors ligne sur les seuls chemins sauvegardes, avec moteur injectable pour les tests.
    ''' Aucun acces IMAP et aucune validation ou ecriture comptable.
    ''' </summary>
    Public Shared Async Function ProposeSavedAttachmentsAsync(importRoot As String,
                                                               savedPaths As IEnumerable(Of String),
                                                               Optional engine As IDocumentExtractionEngine = Nothing,
                                                               Optional enableLocalInvoiceRules As Boolean = False) As Task(Of ImportedAttachmentProposalScanResult)
        If String.IsNullOrWhiteSpace(importRoot) Then Throw New ArgumentException("Dossier d'import requis.", NameOf(importRoot))
        Dim selectedEngine As IDocumentExtractionEngine = engine
        If selectedEngine Is Nothing Then selectedEngine = New LocalPdfTextExtractionEngine()
        Dim queue = Path.Combine(Path.GetFullPath(importRoot), "_PropositionsAValider")
        Dim scanner As New ImportedAttachmentProposalScanner(selectedEngine, queue, enableLocalInvoiceRules)
        Return Await scanner.ScanSavedPathsAsync(importRoot, savedPaths).ConfigureAwait(False)
    End Function

    ' --- Sous-Modules Spécialisés ---

    Private Shared Sub ConnectAndAuthenticate(ByRef client As ImapClient)
        client.Connect("imap.gmail.com", 993, True)
        'client.Authenticate(LectureProprietes.GetVariable("SmtpUsername"), LectureProprietes.GetVariable("SmtpPassword"))
        client.Authenticate(LectureProprietes.GetVariable("SmtpUsername"), GlobalSettings.GmailPassword)
    End Sub

    Private Shared Function ProcessSingleEmail(inbox As IMailFolder, uid As UniqueId) As List(Of String)
        Dim message = inbox.GetMessage(uid)
        Dim hasAttachments As Boolean = False
        Dim savedPaths As New List(Of String)()
        Dim dateMail As DateTime = message.Date.DateTime

        ' Détermination du dossier de destination (Année/Mois)
        Dim targetPath As String = GetFolderByDate(dateMail)

        For Each attachment In message.Attachments
            If Not hasAttachments Then
                _statsMailsAvecPJ += 1
                hasAttachments = True
            End If

            Dim savedPath = SaveAttachment(attachment, targetPath, message.From.ToString())
            If savedPath IsNot Nothing Then savedPaths.Add(savedPath)
        Next

        ' Ne pas modifier le statut lu/non lu : seul le journal fait foi.
        Return savedPaths
    End Function

    Private Shared Function SaveAttachment(attachment As MimeEntity, targetFolder As String, senderInfo As String) As String
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
                Return Nothing
            End If
            _statsTotalPJ += 1
            _rapport.AppendLine($"[PJ] Enregistré : {Path.GetFileName(storedPath)} (De: {senderInfo})")
            Return Path.GetFullPath(storedPath)
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
        End Try
    End Function

    ' --- Utilitaires ---

    ''' <summary>
    ''' Fixe une date de debut incluse, exigee meme si un ancien journal existe.
    ''' Une date deja enregistree ne peut pas etre remplacee implicitement.
    ''' </summary>
    Public Shared Function ResolveInitialImportCutoff(path As String, requested As DateTime?) As DateTime
        If String.IsNullOrWhiteSpace(path) Then Throw New ArgumentException("Chemin de borne requis.", NameOf(path))
        If File.Exists(path) Then
            Dim stored As DateTime
            If Not DateTime.TryParseExact(File.ReadAllText(path).Trim(), "yyyy-MM-dd",
                                          Globalization.CultureInfo.InvariantCulture,
                                          Globalization.DateTimeStyles.None, stored) Then
                Throw New InvalidOperationException("Borne Gmail persistante invalide : verifier gmail_import_cutoff.txt.")
            End If
            If requested.HasValue AndAlso requested.Value.Date <> stored.Date Then
                Throw New InvalidOperationException("Borne Gmail differente de la date deja enregistree.")
            End If
            Return stored.Date
        End If
        If Not requested.HasValue Then
            Throw New InvalidOperationException("Borne Gmail absente : fournir initialImportCutoff explicitement.")
        End If
        ' CreateNew empeche une course de reecriture avec un autre processus.
        Using stream As New FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)
            Using writer As New StreamWriter(stream, New UTF8Encoding(False))
                writer.Write(requested.Value.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture))
            End Using
        End Using
        Return requested.Value.Date
    End Function

    Private Shared Function GetFolderByDate(dt As DateTime) As String
        Dim pathByDate = Path.Combine(_sRepDoc, dt.ToString("yyyy"), dt.ToString("MM"))
        If Not Directory.Exists(pathByDate) Then Directory.CreateDirectory(pathByDate)
        Return pathByDate
    End Function

    Private Shared Function CleanFileName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then Return "attachment.bin"

        ' Appliquer les regles Windows, independamment du systeme de test.
        Dim forbidden As String = "<>:" & ChrW(34) & "/\|?*"
        Dim cleaned As New StringBuilder(name.Length)
        For Each c As Char In name
            If Char.IsControl(c) OrElse forbidden.IndexOf(c) >= 0 Then
                cleaned.Append("_"c)
            Else
                cleaned.Append(c)
            End If
        Next
        name = cleaned.ToString().TrimEnd("."c, " "c)
        If String.IsNullOrWhiteSpace(name) Then Return "attachment.bin"

        ' CON, NUL, COM1, LPT1... restent reserves meme avec une extension.
        Dim stem = name.Split("."c)(0).TrimEnd(" "c).ToUpperInvariant()
        Dim reserved = stem = "CON" OrElse stem = "PRN" OrElse
                       stem = "AUX" OrElse stem = "NUL"
        If stem.Length = 4 Then
            reserved = reserved OrElse
                       ((stem.StartsWith("COM", StringComparison.Ordinal) OrElse
                         stem.StartsWith("LPT", StringComparison.Ordinal)) AndAlso
                        stem(3) >= "1"c AndAlso stem(3) <= "9"c)
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