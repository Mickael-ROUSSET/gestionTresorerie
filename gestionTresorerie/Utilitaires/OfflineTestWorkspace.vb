Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json

' Chaque remise à zéro prépare une nouvelle session. Aucune suppression récursive.
Public NotInheritable Class OfflineTestWorkspace
    Private Const Marker As String = "gestionTresorerie TEST hors ligne v1"
    Private ReadOnly _root As String
    Private ReadOnly _productionDirectory As String
    Private _activeWorkflow As GmailDocumentReviewWorkflow
    Private _sourceHash As String
    Private _reviewRevision As String
    Private Shared ReadOnly SourceNames As String() = {
        "facture.json", "cheque.json", "inconnu.json", "invalide.json", "doublon.json"}

    Public Sub New(productionDirectory As String, testDirectory As String)
        _root = TestEnvironmentIsolation.GetInstallationDirectory(productionDirectory, testDirectory)
        _productionDirectory = Path.GetFullPath(productionDirectory)
    End Sub

    Public Property SessionDirectory As String
        Get
            Return _sessionDirectory
        End Get
        Private Set(value As String)
            _sessionDirectory = value
        End Set
    End Property
    Private _sessionDirectory As String

    Public Function PrepareNewSession() As String
        RejectLinks(_productionDirectory)
        RejectLinks(_root)
        Dim ownership = Path.Combine(_root, "ESPACE-TEST.txt")
        If Directory.Exists(_root) Then
            If ReadOwnedFile(ownership) <> Marker Then
                Throw New InvalidOperationException("Le dossier existant n'est pas un espace TEST reconnu.")
            End If
        Else
            Directory.CreateDirectory(_root)
            RejectLinks(_root)
            WriteNewFile(ownership, Marker)
        End If

        Dim session = Path.Combine(_root, "session-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(session)
        RejectLinks(session)
        WriteNewFile(Path.Combine(session, "facture.json"), DemoDocumentSamples.Invoice)
        WriteNewFile(Path.Combine(session, "cheque.json"), DemoDocumentSamples.Cheque)
        WriteNewFile(Path.Combine(session, "inconnu.json"), DemoDocumentSamples.Unknown)
        WriteNewFile(Path.Combine(session, "invalide.json"), DemoDocumentSamples.Invalid)
        WriteNewFile(Path.Combine(session, "doublon.json"), DemoDocumentSamples.Invoice)
        ' Une session ne devient active qu'après préparation complète.
        WriteNewFile(Path.Combine(session, "SESSION-PRETE.txt"), Marker)
        WriteAtomicFile(Path.Combine(_root, "SESSION-ACTIVE.txt"), Path.GetFileName(session))
        SessionDirectory = session
        _activeWorkflow = Nothing
        Return session
    End Function

    Public Function ReadMessages() As IReadOnlyList(Of GmailDocumentSimulatedInbox.MessageFictif)
        Return BuildMessages(ReadSources())
    End Function

    Private Sub ValidateSession()
        RejectLinks(_productionDirectory)
        If String.IsNullOrEmpty(SessionDirectory) Then
            Throw New InvalidOperationException("Préparez d'abord une session TEST.")
        End If
        If ReadOwnedFile(Path.Combine(_root, "ESPACE-TEST.txt")) <> Marker OrElse
           ReadOwnedFile(Path.Combine(SessionDirectory, "SESSION-PRETE.txt")) <> Marker Then
            Throw New InvalidOperationException("La session TEST est incomplète.")
        End If
    End Sub

    Friend Function CreateDocumentTestInvoice() As String
        If SessionDirectory Is Nothing Then PrepareNewSession()
        ValidateSession()
        Dim source = Path.Combine(SessionDirectory, "facture.png")
        Using sessionLock = AcquireReviewLock()
            RejectLinks(source)
            If Not File.Exists(source) Then
                Using bitmap As New Drawing.Bitmap(640, 240)
                    Using graphics = Drawing.Graphics.FromImage(bitmap)
                        graphics.Clear(Drawing.Color.White)
                        graphics.DrawString("FACTURE FICTIVE — DEMO-001" & vbLf &
                            "Exemple fictif" & vbLf & "01/04/2026" & vbLf & "Total TTC : 42,50 EUR",
                            Drawing.SystemFonts.DefaultFont, Drawing.Brushes.Black, 20, 20)
                    End Using
                    Using stream = File.Open(source, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                        bitmap.Save(stream, Drawing.Imaging.ImageFormat.Png)
                    End Using
                End Using
            End If
        End Using
        Return source
    End Function

    Friend Function GetDocumentTestDirectory() As String
        ValidateSession()
        Return SessionDirectory
    End Function

    Friend Function ReadDocumentExtraction(source As String) As String
        ValidateSession()
        Dim companion = Path.ChangeExtension(source, ".json")
        If Not File.Exists(companion) Then
            Throw New InvalidOperationException("En TEST, placez un JSON d’extraction fictif à côté du document, avec le même nom de base.")
        End If
        Return ReadOwnedFile(companion)
    End Function

    Friend Shared Function DocumentArchiveTestSetting(name As String) As String
        Select Case name
            Case "repRacineComptabilité" : Return "Archives-documents-TEST"
            Case "repFichiersDocumentsFactures" : Return "Factures"
            Case "repFichiersFacturesEmises" : Return "Emises"
            Case "repFichiersFacturesRecues" : Return "Recues"
            Case "repFichiersDocumentsChèques" : Return "Chèques"
            Case "numCptAgumaaa" : Return "COMPTE-AGUMAAA-TEST"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(name))
        End Select
    End Function

    Friend Sub ValidateDocumentTestDestination(destination As String)
        ValidateSession()
        RejectLinks(destination)
    End Sub

    Friend Sub SaveDocumentTestRecord(doc As DocumentAgumaaa)
        ValidateDocumentTestDestination(doc.CheminDoc)
        Using sessionLock = AcquireReviewLock()
            Dim record = doc.CheminDoc & ".fiche-TEST.json"
            RejectLinks(record)
            Dim expected = JsonConvert.SerializeObject(New With {
                doc.DateDoc, doc.CheminDoc, doc.CategorieDoc,
                .MetaDonnees = Newtonsoft.Json.Linq.JObject.Parse(doc.metaDonnees)}, Formatting.Indented)
            If File.Exists(record) Then
                ' Une reprise identique est autorisée, mais jamais une fiche différente
                ' sous le même nom : conserver la preuve initiale et signaler le conflit.
                If Not String.Equals(ReadOwnedFile(record), expected, StringComparison.Ordinal) Then
                    Throw New IOException("Une fiche TEST différente existe déjà pour cette archive.")
                End If
            Else
                WriteAtomicFile(record, expected)
            End If
        End Using
    End Sub

    Private Function ReadSources() As String()
        ValidateSession()
        Return SourceNames.Select(Function(name) ReadOwnedFile(Path.Combine(SessionDirectory, name))).ToArray()
    End Function

    Private Shared Function BuildMessages(contents As String()) As IReadOnlyList(Of GmailDocumentSimulatedInbox.MessageFictif)
        Dim pieces As New List(Of GmailDocumentSimulatedInbox.PieceFictive) From {
            New GmailDocumentSimulatedInbox.PieceFictive(contents(0), PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentSimulatedInbox.PieceFictive(contents(1), PropositionMouvement.SensMouvement.Credit),
            New GmailDocumentSimulatedInbox.PieceFictive(contents(2), PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentSimulatedInbox.PieceFictive(contents(3), PropositionMouvement.SensMouvement.Debit)}
        Return New List(Of GmailDocumentSimulatedInbox.MessageFictif) From {
            New GmailDocumentSimulatedInbox.MessageFictif("test@example.invalid", 1UI, 1UI, pieces),
            New GmailDocumentSimulatedInbox.MessageFictif("test@example.invalid", 1UI, 1UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(contents(4), PropositionMouvement.SensMouvement.Debit)})}
    End Function

    Public Function ResumeLastSession() As String
        _activeWorkflow = Nothing
        RejectLinks(_productionDirectory)
        If ReadOwnedFile(Path.Combine(_root, "ESPACE-TEST.txt")) <> Marker Then
            Throw New InvalidOperationException("Espace TEST inconnu.")
        End If
        Dim pointer = Path.Combine(_root, "SESSION-ACTIVE.txt")
        RejectLinks(pointer)
        Dim name As String
        If File.Exists(pointer) Then
            name = ReadOwnedFile(pointer)
        Else
            ' Compatibilité des sessions créées avant la sauvegarde des revues.
            name = Directory.EnumerateDirectories(_root, "session-*").
                Where(Function(folder) File.Exists(Path.Combine(folder, "SESSION-PRETE.txt"))).
                OrderByDescending(Function(folder) File.GetLastWriteTimeUtc(Path.Combine(folder, "SESSION-PRETE.txt"))).
                Select(Function(folder) Path.GetFileName(folder)).FirstOrDefault()
        End If
        If name Is Nothing OrElse Not Regex.IsMatch(name, "\Asession-[0-9a-f]{32}\z") Then
            Throw New InvalidOperationException("Aucune session TEST complète à reprendre.")
        End If
        Dim candidate = Path.Combine(_root, name)
        If ReadOwnedFile(Path.Combine(candidate, "SESSION-PRETE.txt")) <> Marker Then
            Throw New InvalidOperationException("Session TEST incomplète.")
        End If
        SessionDirectory = candidate
        Return candidate
    End Function

    Public Function LoadReview() As GmailDocumentReviewWorkflow
        _activeWorkflow = Nothing
        ValidateSession()
        Using sessionLock = AcquireReviewLock()
            Dim sources = ReadSources()
            Dim sourceHash = HashText(JsonConvert.SerializeObject(sources))
            Dim workflow = GmailDocumentSimulatedInbox.PreparerRevue(BuildMessages(sources))
            Dim json = ReadReviewFile()
            If json IsNot Nothing Then
                OfflineTestReviewState.Restore(workflow, Path.GetFileName(SessionDirectory), sourceHash, json)
            End If
            _sourceHash = sourceHash
            _reviewRevision = HashText(json)
            _activeWorkflow = workflow
            Return workflow
        End Using
    End Function

    Public Sub SaveReview(workflow As GmailDocumentReviewWorkflow)
        ValidateSession()
        If workflow Is Nothing OrElse Not Object.ReferenceEquals(workflow, _activeWorkflow) Then
            Throw New InvalidOperationException("Rechargez la revue TEST avant de l'enregistrer.")
        End If
        Using sessionLock = AcquireReviewLock()
            If Not String.Equals(_sourceHash, HashText(JsonConvert.SerializeObject(ReadSources())),
                                 StringComparison.Ordinal) Then
                Throw New InvalidOperationException("Les documents TEST ont changé. Rechargez une nouvelle session.")
            End If
            If Not String.Equals(_reviewRevision, HashText(ReadReviewFile()), StringComparison.Ordinal) Then
                Throw New InvalidOperationException("Une autre fenêtre a enregistré cette revue. Rechargez-la.")
            End If
            Dim json = OfflineTestReviewState.Capture(workflow, Path.GetFileName(SessionDirectory), _sourceHash)
            If Encoding.UTF8.GetByteCount(json) > 1024 * 1024 Then
                Throw New InvalidOperationException("La revue TEST dépasse la taille autorisée.")
            End If
            WriteAtomicFile(Path.Combine(SessionDirectory, "REVUE-TEST.json"), json)
            _reviewRevision = HashText(json)
        End Using
    End Sub

    Private Function AcquireReviewLock() As FileStream
        Dim lockPath = Path.Combine(SessionDirectory, "REVUE-TEST.lock")
        RejectLinks(lockPath)
        Return New FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
    End Function

    Private Function ReadReviewFile() As String
        Dim statePath = Path.Combine(SessionDirectory, "REVUE-TEST.json")
        RejectLinks(statePath)
        If Not File.Exists(statePath) Then Return Nothing
        Return ReadOwnedFile(statePath)
    End Function

    Private Shared Function HashText(value As String) As String
        If value Is Nothing Then Return Nothing
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
    End Function

    Private Shared Sub WriteAtomicFile(target As String, content As String)
        Dim temporary = target & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            WriteNewFile(temporary, content)
            RejectLinks(target)
            If File.Exists(target) Then
                File.Replace(temporary, target, Nothing)
            Else
                File.Move(temporary, target)
            End If
        Finally
            If File.Exists(temporary) Then File.Delete(temporary)
        End Try
    End Sub

    Private Shared Sub WriteNewFile(filePath As String, content As String)
        RejectLinks(filePath)
        Using stream As New FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None)
            Using writer As New StreamWriter(stream, New UTF8Encoding(False))
                writer.Write(content)
            End Using
        End Using
    End Sub

    Private Shared Function ReadOwnedFile(filePath As String) As String
        RejectLinks(filePath)
        Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            If stream.Length > 1024 * 1024 Then
                Throw New InvalidOperationException("Un fichier TEST dépasse la taille autorisée.")
            End If
            Using reader As New StreamReader(stream, Encoding.UTF8, True)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function

    Private Shared Sub RejectLinks(value As String)
        Dim current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(value))
        While Not String.IsNullOrEmpty(current)
            ' GetAttributes détecte aussi les liens pendants, contrairement à File.Exists.
            Try
                If (File.GetAttributes(current) And FileAttributes.ReparsePoint) <> 0 Then
                    Throw New InvalidOperationException("Les liens et jonctions sont refusés pour l'espace TEST.")
                End If
            Catch ex As FileNotFoundException
                ' Le chemin sera créé par l'opération appelante.
            Catch ex As DirectoryNotFoundException
                ' Le chemin sera créé par l'opération appelante.
            End Try
            current = Path.GetDirectoryName(current)
        End While
    End Sub
End Class
