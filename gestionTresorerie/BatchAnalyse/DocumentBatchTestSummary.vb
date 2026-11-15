Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Text.Json

''' <summary>
''' Contrat de lecture du bilan de scans TEST (schema_version=1).
''' Lecture seule : aucune connexion Gmail, SQL ou ecriture sur disque.
''' </summary>
Public NotInheritable Class DocumentBatchTestSummary
    Private Sub New(total As Integer, valid As Integer, review As Integer, skipped As Integer,
                    documents As IReadOnlyList(Of DocumentStatus))
        Me.Total = total
        Me.Valid = valid
        Me.Review = review
        Me.Skipped = skipped
        Me.Documents = documents
    End Sub

    Public ReadOnly Property Total As Integer
    Public ReadOnly Property Valid As Integer
    Public ReadOnly Property Review As Integer
    Public ReadOnly Property Skipped As Integer
    Public ReadOnly Property Documents As IReadOnlyList(Of DocumentStatus)
    Public ReadOnly Property RequiresReview As Boolean
        Get
            Return Review > 0
        End Get
    End Property

    Public NotInheritable Class DocumentStatus
        Public Sub New(name As String, hash As String, status As String, reason As String)
            Me.Name = name
            Me.Hash = hash
            Me.Status = status
            Me.Reason = reason
        End Sub
        Public ReadOnly Property Name As String
        Public ReadOnly Property Hash As String
        Public ReadOnly Property Status As String
        Public ReadOnly Property Reason As String
    End Class

    ' System.Text.Json accepte par defaut les noms de proprietes repetes.
    ' Refuser ces ambiguïtes avant d'interpreter un bilan TEST.
    Private Shared Sub RejectDuplicateProperties(element As JsonElement)
        Select Case element.ValueKind
            Case JsonValueKind.Object
                Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
                For Each propertyItem In element.EnumerateObject()
                    If Not seen.Add(propertyItem.Name) Then
                        Throw New FormatException("Propriete JSON TEST dupliquee : " & propertyItem.Name)
                    End If
                    RejectDuplicateProperties(propertyItem.Value)
                Next
            Case JsonValueKind.Array
                For Each child In element.EnumerateArray()
                    RejectDuplicateProperties(child)
                Next
        End Select
    End Sub

    Private Shared Function Required(root As JsonElement, key As String, kind As JsonValueKind) As JsonElement
        Dim value As JsonElement
        If root.ValueKind <> JsonValueKind.Object OrElse
           Not root.TryGetProperty(key, value) OrElse value.ValueKind <> kind Then
            Throw New FormatException("Contrat TEST invalide : " & key)
        End If
        Return value
    End Function

    Private Shared Function Count(root As JsonElement, key As String) As Integer
        Dim value = Required(root, key, JsonValueKind.Number)
        Dim number As Integer
        If Not value.TryGetInt32(number) OrElse number < 0 Then
            Throw New FormatException("Compteur TEST invalide : " & key)
        End If
        Return number
    End Function

    Public Shared Function Parse(json As String) As DocumentBatchTestSummary
        If String.IsNullOrWhiteSpace(json) Then Throw New FormatException("Bilan TEST vide.")
        Try
            Using doc = JsonDocument.Parse(json)
                Dim root = doc.RootElement
                RejectDuplicateProperties(root)
                If Count(root, "schema_version") <> 1 Then Throw New FormatException("Version de bilan non prise en charge.")
                Dim total = Count(root, "total")
                Dim valid = Count(root, "valid")
                Dim review = Count(root, "review")
                Dim skipped = Count(root, "skipped")
                If CLng(valid) + CLng(review) + CLng(skipped) <> CLng(total) Then
                    Throw New FormatException("Compteurs du bilan incoherents.")
                End If
                Dim flag = Required(root, "requires_review", If(review > 0, JsonValueKind.True, JsonValueKind.False))
                Dim items = Required(root, "documents", JsonValueKind.Array)
                If items.GetArrayLength() <> total Then Throw New FormatException("Nombre de documents incoherent.")
                Dim rows As New List(Of DocumentStatus)()
                Dim counts As New Dictionary(Of String, Integer)(StringComparer.Ordinal) From {
                    {"valid", 0}, {"review", 0}, {"skipped", 0}}
                For Each item In items.EnumerateArray()
                    Dim name = Required(item, "name", JsonValueKind.String).GetString()
                    Dim hash = Required(item, "hash", JsonValueKind.String).GetString()
                    Dim status = Required(item, "status", JsonValueKind.String).GetString()
                    Dim reason = Required(item, "reason", JsonValueKind.String).GetString()
                    If String.IsNullOrWhiteSpace(name) OrElse String.IsNullOrWhiteSpace(reason) OrElse
                       String.IsNullOrWhiteSpace(hash) OrElse hash.Length <> 64 OrElse
                       Not System.Text.RegularExpressions.Regex.IsMatch(hash, "\A[0-9a-fA-F]{64}\z") OrElse
                       Not counts.ContainsKey(status) Then
                        Throw New FormatException("Ligne documentaire TEST invalide.")
                    End If
                    counts(status) += 1
                    rows.Add(New DocumentStatus(name, hash, status, reason))
                Next
                If counts("valid") <> valid OrElse counts("review") <> review OrElse
                   counts("skipped") <> skipped Then
                    Throw New FormatException("Statuts documentaires incoherents.")
                End If
                Return New DocumentBatchTestSummary(total, valid, review, skipped, rows.AsReadOnly())
            End Using
        Catch ex As JsonException
            Throw New FormatException("JSON de bilan TEST invalide.", ex)
        End Try
    End Function
End Class
