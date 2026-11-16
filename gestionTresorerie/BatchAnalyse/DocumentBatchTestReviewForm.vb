Option Strict On
Option Explicit On

Imports System
Imports System.Windows.Forms

''' <summary>
''' Fenetre autonome de consultation des bilans documentaires fictifs TEST.
''' Aucune connexion Gmail, SQL ou production.
''' </summary>
Public NotInheritable Class DocumentBatchTestReviewForm
    Inherits Form

    Private ReadOnly _grid As DataGridView

    Public Sub New()
        Text = "TEST - Documents a examiner"
        Width = 850
        Height = 480
        StartPosition = FormStartPosition.CenterParent

        _grid = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AutoGenerateColumns = True,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
        }
        Controls.Add(_grid)
    End Sub

    Public ReadOnly Property ReviewGrid As DataGridView
        Get
            Return _grid
        End Get
    End Property

    Public Sub LoadTestSummary(path As String)
        DocumentBatchTestReviewGridBinding.LoadFromFile(_grid, path)
    End Sub
End Class
