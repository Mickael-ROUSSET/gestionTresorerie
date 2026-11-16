Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Instantane TEST en lecture seule destine au futur ecran de revue WinForms.
''' Reutilise le contrat de bilan et sa file d'examen, sans Gmail ni SQL.
''' </summary>
Public NotInheritable Class DocumentBatchTestReviewSnapshot
    Private Sub New(summary As DocumentBatchTestSummary,
                    pending As IReadOnlyList(Of DocumentBatchTestSummary.DocumentStatus))
        Total = summary.Total
        Valid = summary.Valid
        Review = summary.Review
        Skipped = summary.Skipped
        RequiresReview = summary.RequiresReview
        Pending = pending
    End Sub

    Public ReadOnly Property Total As Integer
    Public ReadOnly Property Valid As Integer
    Public ReadOnly Property Review As Integer
    Public ReadOnly Property Skipped As Integer
    Public ReadOnly Property RequiresReview As Boolean
    Public ReadOnly Property Pending As IReadOnlyList(Of DocumentBatchTestSummary.DocumentStatus)

    Public Shared Function FromSummary(summary As DocumentBatchTestSummary) As DocumentBatchTestReviewSnapshot
        If summary Is Nothing Then Throw New ArgumentNullException(NameOf(summary))
        Return New DocumentBatchTestReviewSnapshot(summary, DocumentBatchTestReviewQueue.FromSummary(summary))
    End Function

    Public Shared Function FromFile(path As String) As DocumentBatchTestReviewSnapshot
        Return FromSummary(DocumentBatchTestSummaryFileReader.Read(path))
    End Function
End Class
