Imports System.IO

''' <summary>
''' Prepare des propositions a partir de pieces jointes deja enregistrees localement.
''' Ne se connecte ni a Gmail, ni a une base SQL et ne valide aucune operation.
''' </summary>
Public NotInheritable Class ImportedAttachmentProposalScanner
    Private ReadOnly _workflow As DocumentProposalWorkflow
    Private ReadOnly _queueFolder As String

    Public Sub New(engine As IDocumentExtractionEngine, queueFolder As String, Optional enableLocalInvoiceRules As Boolean = False)
        If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
        If String.IsNullOrWhiteSpace(queueFolder) Then Throw New ArgumentException("Dossier de propositions requis.", NameOf(queueFolder))
        _queueFolder = Path.GetFullPath(queueFolder)
        ' Les regles de facture restent opt-in : aucune analyse automatique a l'import Gmail.
        Dim selectedEngine As IDocumentExtractionEngine = engine
        If enableLocalInvoiceRules Then selectedEngine = New RuleBasedInvoiceExtractionEngine(engine)
        _workflow = New DocumentProposalWorkflow(selectedEngine, New DocumentProposalStore(_queueFolder))
    End Sub

    ''' <summary>
    ''' Parcourt uniquement les documents compatibles. Les erreurs sont isolees par fichier,
    ''' afin qu'un document illisible ne bloque pas les suivants.
    ''' </summary>
    Public Async Function ScanAsync(importRoot As String) As Task(Of ImportedAttachmentProposalScanResult)
        If String.IsNullOrWhiteSpace(importRoot) Then Throw New ArgumentException("Dossier d'import requis.", NameOf(importRoot))
        Dim root = Path.GetFullPath(importRoot)
        If Not Directory.Exists(root) Then Throw New DirectoryNotFoundException("Dossier d'import introuvable : " & root)

        Dim result As New ImportedAttachmentProposalScanResult()
        For Each documentPath In Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            If IsInsideFolder(documentPath, _queueFolder) OrElse Not IsSupportedDocument(documentPath) Then Continue For
            Try
                If Await _workflow.ProposeAsync(documentPath).ConfigureAwait(False) Then
                    result.Created += 1
                Else
                    result.AlreadyPresent += 1
                End If
            Catch ex As Exception
                result.Errors.Add(Path.GetFileName(documentPath) & " : " & ex.Message)
            End Try
        Next
        Return result
    End Function

    ''' <summary>
    ''' Point de raccordement explicite pour les chemins effectivement sauvegardes par un import.
    ''' Aucune connexion Gmail, aucun balayage du disque et aucune validation automatique.
    ''' Les chemins hors du dossier d'import sont rejetes avant toute lecture.
    ''' </summary>
    Public Async Function ScanSavedPathsAsync(importRoot As String, savedPaths As IEnumerable(Of String)) As Task(Of ImportedAttachmentProposalScanResult)
        If String.IsNullOrWhiteSpace(importRoot) Then Throw New ArgumentException("Dossier d'import requis.", NameOf(importRoot))
        If savedPaths Is Nothing Then Throw New ArgumentNullException(NameOf(savedPaths))
        Dim root = Path.GetFullPath(importRoot)
        If Not Directory.Exists(root) Then Throw New DirectoryNotFoundException("Dossier d'import introuvable : " & root)

        Dim result As New ImportedAttachmentProposalScanResult()
        Dim visited As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each suppliedPath In savedPaths
            Try
                If String.IsNullOrWhiteSpace(suppliedPath) OrElse Not Path.IsPathRooted(suppliedPath) Then
                    Throw New ArgumentException("Chemin absolu de piece jointe requis.")
                End If
                Dim documentPath = Path.GetFullPath(suppliedPath)
                If Not IsInsideFolder(documentPath, root) Then
                    Throw New InvalidOperationException("Document hors dossier d'import.")
                End If
                If IsInsideFolder(documentPath, _queueFolder) OrElse Not IsSupportedDocument(documentPath) Then Continue For
                If Not visited.Add(documentPath) Then Continue For

                If Await _workflow.ProposeAsync(documentPath).ConfigureAwait(False) Then
                    result.Created += 1
                Else
                    result.AlreadyPresent += 1
                End If
            Catch ex As Exception
                result.Errors.Add("Piece jointe ignoree : " & ex.Message)
            End Try
        Next
        Return result
    End Function

    Private Shared Function IsSupportedDocument(documentPath As String) As Boolean
        Select Case Path.GetExtension(documentPath).ToLowerInvariant()
            Case ".pdf", ".jpg", ".jpeg", ".png"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function IsInsideFolder(documentPath As String, folder As String) As Boolean
        Dim fullPath = Path.GetFullPath(documentPath)
        Return fullPath.StartsWith(folder.TrimEnd(Path.DirectorySeparatorChar) &
                                   Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
    End Function
End Class

Public NotInheritable Class ImportedAttachmentProposalScanResult
    Public Property Created As Integer
    Public Property AlreadyPresent As Integer
    Public ReadOnly Property Errors As List(Of String) = New List(Of String)()
End Class
