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
    Private ReadOnly _status As Label
    Private _loadedSummaryPath As String

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
        _status = New Label() With {
            .Dock = DockStyle.Top,
            .AutoSize = False,
            .Height = 32,
            .TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            .Text = "TEST - Aucun bilan charge"
        }
        Controls.Add(_grid)
        Controls.Add(_status)
    End Sub

    Public ReadOnly Property ReviewGrid As DataGridView
        Get
            Return _grid
        End Get
    End Property

    Public ReadOnly Property StatusText As String
        Get
            Return _status.Text
        End Get
    End Property

    Public ReadOnly Property LoadedSummaryPath As String
        Get
            Return _loadedSummaryPath
        End Get
    End Property

    Public Sub LoadTestSummary(path As String)
        DocumentBatchTestReviewGridBinding.LoadFromFile(_grid, path)
        _loadedSummaryPath = path
        _status.Text = "TEST - Documents a examiner : " & _grid.Rows.Count.ToString(Globalization.CultureInfo.InvariantCulture)
    End Sub
End Class
