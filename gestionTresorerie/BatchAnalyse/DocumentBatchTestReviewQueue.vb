Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Projection en lecture seule des documents fictifs a examiner.
''' Ne declenche ni traitement, ni classement, ni acces a une base.
''' </summary>
Public NotInheritable Class DocumentBatchTestReviewQueue
    Private Sub New()
    End Sub

    Public Shared Function FromSummary(summary As DocumentBatchTestSummary) As IReadOnlyList(Of DocumentBatchTestSummary.DocumentStatus)
        If summary Is Nothing Then Throw New ArgumentNullException(NameOf(summary))

        Dim pending As New List(Of DocumentBatchTestSummary.DocumentStatus)()
        For Each item In summary.Documents
            If String.Equals(item.Status, "review", StringComparison.Ordinal) Then
                pending.Add(item)
            End If
        Next

        If pending.Count <> summary.Review Then
            Throw New FormatException("Nombre de documents TEST a examiner incoherent.")
        End If

        Return pending.AsReadOnly()
    End Function

    Public Shared Function FromFile(path As String) As IReadOnlyList(Of DocumentBatchTestSummary.DocumentStatus)
        Return FromSummary(DocumentBatchTestSummaryFileReader.Read(path))
    End Function
End Class
