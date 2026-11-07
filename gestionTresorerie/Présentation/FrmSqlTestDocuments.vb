Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Windows.Forms

Friend NotInheritable Class FrmSqlTestDocuments
    Inherits Form

    Private ReadOnly _workspace As OfflineTestWorkspace
    Private ReadOnly _store As New SqlDocumentTestStore()
    Private ReadOnly _init As New Button With {.Text = "Initialiser la base TEST", .AutoSize = True}
    Private ReadOnly _open As New Button With {.Text = "Ouvrir un document…", .AutoSize = True, .Enabled = False}
    Private ReadOnly _example As New Button With {.Text = "Ouvrir une facture fictive", .AutoSize = True, .Enabled = False}
    Private ReadOnly _refresh As New Button With {.Text = "Actualiser", .AutoSize = True, .Enabled = False}
    Private ReadOnly _status As New Label With {.AutoSize = True, .Text = "Base : bddAgumaaaTest sur LocalDB."}
    Private ReadOnly _grid As New DataGridView With {
        .Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells}
    Private _busy As Boolean
    Private _ready As Boolean

    Public Sub New(workspace As OfflineTestWorkspace)
        _workspace = workspace
        Text = "gestionTresorerie — TEST SQL documentaire"
        ClientSize = New Drawing.Size(1080, 610)
        StartPosition = FormStartPosition.CenterParent

        Dim bar As New FlowLayoutPanel With {.AutoSize = True, .Dock = DockStyle.Top}
        bar.Controls.AddRange(New Control() {_init, _open, _example, _refresh, _status})
        Controls.Add(_grid)
        Controls.Add(bar)

        AddHandler _init.Click, AddressOf InitializeTest
        AddHandler _open.Click, AddressOf OpenDocument
        AddHandler _example.Click, AddressOf OpenExample
        AddHandler _refresh.Click, AddressOf RefreshDocuments
    End Sub

    Private Async Sub InitializeTest(sender As Object, e As EventArgs)
        If _busy Then Return
        SetBusy(True)
        Try
            Await Task.Run(Sub() _store.Initialize())
            _workspace.OpenOrPrepareSession()
            _ready = True
            Await LoadDocuments()
        Catch ex As SqlException
            _ready = False
            _status.Text = "LocalDB ou bddAgumaaaTest inaccessible."
            MessageBox.Show(Me, "Vérifiez que SQL Server Express LocalDB est installé pour votre compte Windows." &
                            vbCrLf & ex.Message, "Initialisation SQL TEST", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            _ready = False
            _status.Text = "Initialisation TEST impossible."
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Sub OpenDocument(sender As Object, e As EventArgs)
        FrmDocumentFilePreview.OpenFiles(Me, _workspace, _store)
        Await RefreshGrid()
    End Sub

    Private Async Sub OpenExample(sender As Object, e As EventArgs)
        Try
            Dim source = _workspace.CreateDocumentTestInvoice()
            Using preview As New FrmDocumentFilePreview({source}, _workspace, _store)
                preview.ShowDialog(Me)
            End Using
            Await RefreshGrid()
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Async Sub RefreshDocuments(sender As Object, e As EventArgs)
        Await RefreshGrid()
    End Sub

    Private Async Function RefreshGrid() As Task
        If _busy Then Return
        SetBusy(True)
        Try
            Await LoadDocuments()
        Catch ex As Exception
            _status.Text = "Lecture SQL TEST impossible."
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            SetBusy(False)
        End Try
    End Function

    Private Async Function LoadDocuments() As Task
        Dim result = Await Task.Run(Function() _store.ReadDocuments())
        Dim previous = TryCast(_grid.DataSource, DataTable)
        _grid.DataSource = result
        previous?.Dispose()
        _status.Text = $"{result.Rows.Count} document(s) dans bddAgumaaaTest."
    End Function

    Private Sub SetBusy(value As Boolean)
        _busy = value
        _init.Enabled = Not value
        _open.Enabled = _ready AndAlso Not value
        _example.Enabled = _open.Enabled
        _refresh.Enabled = _open.Enabled
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        If _busy Then e.Cancel = True
    End Sub
End Class
