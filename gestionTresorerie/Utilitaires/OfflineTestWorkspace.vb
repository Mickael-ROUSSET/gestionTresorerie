Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text

' Chaque remise à zéro prépare une nouvelle session. Aucune suppression récursive.
Public NotInheritable Class OfflineTestWorkspace
    Private Const Marker As String = "gestionTresorerie TEST hors ligne v1"
    Private ReadOnly _root As String
    Private ReadOnly _productionDirectory As String

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
        SessionDirectory = session
        Return session
    End Function

    Public Function ReadMessages() As IReadOnlyList(Of GmailDocumentSimulatedInbox.MessageFictif)
        If String.IsNullOrEmpty(SessionDirectory) Then
            Throw New InvalidOperationException("Préparez d'abord une session TEST.")
        End If
        If ReadOwnedFile(Path.Combine(_root, "ESPACE-TEST.txt")) <> Marker OrElse
           ReadOwnedFile(Path.Combine(SessionDirectory, "SESSION-PRETE.txt")) <> Marker Then
            Throw New InvalidOperationException("La session TEST est incomplète.")
        End If

        Dim pieces As New List(Of GmailDocumentSimulatedInbox.PieceFictive) From {
            New GmailDocumentSimulatedInbox.PieceFictive(
                ReadOwnedFile(Path.Combine(SessionDirectory, "facture.json")),
                PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentSimulatedInbox.PieceFictive(
                ReadOwnedFile(Path.Combine(SessionDirectory, "cheque.json")),
                PropositionMouvement.SensMouvement.Credit),
            New GmailDocumentSimulatedInbox.PieceFictive(
                ReadOwnedFile(Path.Combine(SessionDirectory, "inconnu.json")),
                PropositionMouvement.SensMouvement.Debit),
            New GmailDocumentSimulatedInbox.PieceFictive(
                ReadOwnedFile(Path.Combine(SessionDirectory, "invalide.json")),
                PropositionMouvement.SensMouvement.Debit)}
        Return New List(Of GmailDocumentSimulatedInbox.MessageFictif) From {
            New GmailDocumentSimulatedInbox.MessageFictif("test@example.invalid", 1UI, 1UI, pieces),
            New GmailDocumentSimulatedInbox.MessageFictif("test@example.invalid", 1UI, 1UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        ReadOwnedFile(Path.Combine(SessionDirectory, "doublon.json")),
                        PropositionMouvement.SensMouvement.Debit)})}
    End Function

    ' Journal append-only par session : aucune connexion comptable et aucun fichier partagé avec PROD.
    ' Les décisions sont des simulations, sans effet sur les mouvements réels.
    Public Function RecordSimulatedDecision(documentName As String, accepted As Boolean) As String
        If String.IsNullOrEmpty(SessionDirectory) Then
            Throw New InvalidOperationException("Préparez d'abord une session TEST.")
        End If
        If ReadOwnedFile(Path.Combine(_root, "ESPACE-TEST.txt")) <> Marker OrElse
           ReadOwnedFile(Path.Combine(SessionDirectory, "SESSION-PRETE.txt")) <> Marker Then
            Throw New InvalidOperationException("La session TEST est incomplète.")
        End If
        Dim allowed = New String() {"facture.json", "cheque.json", "inconnu.json", "invalide.json", "doublon.json"}
        If Not Array.Exists(allowed, Function(item) String.Equals(item, documentName, StringComparison.Ordinal)) Then
            Throw New ArgumentException("Document fictif non reconnu.", NameOf(documentName))
        End If
        ' Vérifie aussi que le document n'est pas devenu un lien vers l'extérieur.
        ReadOwnedFile(Path.Combine(SessionDirectory, documentName))
        Dim decisionId = Guid.NewGuid().ToString("N")
        Dim value = documentName & Environment.NewLine &
                    If(accepted, "ACCEPTEE_SIMULATION", "REFUSEE_SIMULATION") & Environment.NewLine
        Dim decisionFile = Path.Combine(SessionDirectory, "decision-" & decisionId & ".txt")
        WriteNewFile(decisionFile, value)
        Return decisionFile
    End Function

    Public Function ReadSimulatedDecisions() As IReadOnlyList(Of String)
        If String.IsNullOrEmpty(SessionDirectory) Then
            Throw New InvalidOperationException("Préparez d'abord une session TEST.")
        End If
        If ReadOwnedFile(Path.Combine(_root, "ESPACE-TEST.txt")) <> Marker OrElse
           ReadOwnedFile(Path.Combine(SessionDirectory, "SESSION-PRETE.txt")) <> Marker Then
            Throw New InvalidOperationException("La session TEST est incomplète.")
        End If
        Dim decisions As New List(Of String)
        For Each path In Directory.GetFiles(SessionDirectory, "decision-*.txt")
            Dim content = ReadOwnedFile(path)
            Dim lines = content.Split(New String() {Environment.NewLine}, StringSplitOptions.None)
            If lines.Length < 2 OrElse
               Not Array.Exists(New String() {"facture.json", "cheque.json", "inconnu.json", "invalide.json", "doublon.json"},
                                Function(item) String.Equals(item, lines(0), StringComparison.Ordinal)) OrElse
               (lines(1) <> "ACCEPTEE_SIMULATION" AndAlso lines(1) <> "REFUSEE_SIMULATION") Then
                Throw New InvalidOperationException("Décision fictive invalide.")
            End If
            decisions.Add(lines(0) & " : " & lines(1))
        Next
        Return decisions.AsReadOnly()
    End Function

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
