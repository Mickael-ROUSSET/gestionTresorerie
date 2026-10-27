Imports System.IO

''' <summary>
''' Prepare des propositions a partir de pieces jointes deja enregistrees localement.
''' Ne se connecte ni a Gmail, ni a une base SQL et ne valide aucune operation.
''' </summary>
Public NotInheritable Class ImportedAttachmentProposalScanner
    Private ReadOnly _workflow As DocumentProposalWorkflow
    Private ReadOnly _queueFolder As String

    Public Sub New(engine As IDocumentExtractionEngine, queueFolder As String)
        If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
        If String.IsNullOrWhiteSpace(queueFolder) Then Throw New ArgumentException("Dossier de propositions requis.", NameOf(queueFolder))
        _queueFolder = Path.GetFullPath(queueFolder)
        _workflow = New DocumentProposalWorkflow(engine, New DocumentProposalStore(_queueFolder))
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

    Private Shared Function IsSupportedDocument(path As String) As Boolean
        Select Case Path.GetExtension(path).ToLowerInvariant()
            Case ".pdf", ".jpg", ".jpeg", ".png"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function IsInsideFolder(path As String, folder As String) As Boolean
        Dim fullPath = Path.GetFullPath(path)
        Return fullPath.StartsWith(folder.TrimEnd(Path.DirectorySeparatorChar) &
                                   Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
    End Function
End Class

Public NotInheritable Class ImportedAttachmentProposalScanResult
    Public Property Created As Integer
    Public Property AlreadyPresent As Integer
    Public ReadOnly Property Errors As List(Of String) = New List(Of String)()
End Class
