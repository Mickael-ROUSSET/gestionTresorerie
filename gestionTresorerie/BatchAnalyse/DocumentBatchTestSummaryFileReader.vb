Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text

''' <summary>
''' Point d'entree lecture seule pour un bilan JSON de scans fictifs.
''' N'ouvre ni document source, ni Gmail, ni base SQL.
''' </summary>
Public NotInheritable Class DocumentBatchTestSummaryFileReader
    Private Const MaxSummaryBytes As Long = 1024L * 1024L

    Private Sub New()
    End Sub

    Public Shared Function Read(path As String) As DocumentBatchTestSummary
        If String.IsNullOrWhiteSpace(path) Then
            Throw New ArgumentException("Chemin du bilan TEST requis.", NameOf(path))
        End If

        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            If stream.Length > MaxSummaryBytes Then
                Throw New FormatException("Bilan TEST trop volumineux.")
            End If

            Using reader As New StreamReader(stream, New UTF8Encoding(False, True), True)
                Dim json = reader.ReadToEnd()
                If stream.Length > MaxSummaryBytes Then
                    Throw New FormatException("Bilan TEST trop volumineux.")
                End If
                Return DocumentBatchTestSummary.Parse(json)
            End Using
        End Using
    End Function
End Class
