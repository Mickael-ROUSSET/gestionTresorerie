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
    Private ReadOnly _openSummaryButton As Button
    Private ReadOnly _reloadSummaryButton As Button
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
        _openSummaryButton = New Button() With {
            .Dock = DockStyle.Top,
            .Height = 34,
            .Text = "Ouvrir un bilan TEST (JSON)",
            .Name = "OpenTestSummaryButton"
        }
        _reloadSummaryButton = New Button() With {
            .Dock = DockStyle.Top,
            .Height = 34,
            .Text = "Actualiser le bilan TEST",
            .Name = "ReloadTestSummaryButton",
            .Enabled = False
        }
        AddHandler _reloadSummaryButton.Click, AddressOf ReloadSummaryClick
        AddHandler _openSummaryButton.Click, AddressOf OpenSummaryClick
        Controls.Add(_grid)
        Controls.Add(_status)
        Controls.Add(_openSummaryButton)
        Controls.Add(_reloadSummaryButton)
    End Sub

    Public ReadOnly Property OpenSummaryButton As Button
        Get
            Return _openSummaryButton
        End Get
    End Property

    Public ReadOnly Property ReloadSummaryButton As Button
        Get
            Return _reloadSummaryButton
        End Get
    End Property

    Private Sub ReloadSummaryClick(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(_loadedSummaryPath) Then Return
        Try
            LoadTestSummary(_loadedSummaryPath)
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            MessageBox.Show(Me, "Actualisation TEST impossible : " & ex.Message, "Bilan invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub OpenSummaryClick(sender As Object, e As EventArgs)
        Using picker As New OpenFileDialog() With {
            .Title = "Choisir un bilan documentaire fictif TEST",
            .Filter = "Bilans JSON (*.json)|*.json",
            .CheckFileExists = True,
            .Multiselect = False
        }
            If picker.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                LoadTestSummary(picker.FileName)
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
                MessageBox.Show(Me, "Bilan TEST non charge : " & ex.Message, "Bilan invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Using
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
        _reloadSummaryButton.Enabled = True
        _status.Text = "TEST - Documents a examiner : " & _grid.Rows.Count.ToString(Globalization.CultureInfo.InvariantCulture)
    End Sub
End Class
