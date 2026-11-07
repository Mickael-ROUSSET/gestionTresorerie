Option Strict On

Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Friend NotInheritable Class FrmDocumentFilePreview
    Inherits Form

    Private ReadOnly _files As New ListBox With {.Dock = DockStyle.Fill, .HorizontalScrollbar = True}
    Private ReadOnly _text As New TextBox With {
        .Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True, .ScrollBars = ScrollBars.Vertical}
    Private ReadOnly _image As New PictureBox With {.Dock = DockStyle.Fill, .SizeMode = PictureBoxSizeMode.Zoom}

    Public Sub New(paths As IEnumerable(Of String))
        Text = "Examen des PDF et des scans (lecture seule)"
        ClientSize = New Size(1000, 640)
        StartPosition = FormStartPosition.CenterParent

        Dim columns As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1}
        columns.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38.0F))
        columns.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 62.0F))
        Dim preview As New Panel With {.Dock = DockStyle.Fill}
        preview.Controls.Add(_image)
        preview.Controls.Add(_text)
        columns.Controls.Add(_files, 0, 0)
        columns.Controls.Add(preview, 1, 0)
        Controls.Add(columns)

        _files.Items.AddRange(paths.Cast(Of Object)().ToArray())
        AddHandler _files.SelectedIndexChanged, AddressOf LoadSelection
        If _files.Items.Count > 0 Then _files.SelectedIndex = 0
    End Sub

    Public Shared Sub OpenFiles(owner As IWin32Window)
        Using picker As New OpenFileDialog With {
            .Title = "Choisir des documents sur C: ou sur un lecteur Drive",
            .Filter = "PDF et scans (*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff)|*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
            .Multiselect = True, .CheckFileExists = True, .RestoreDirectory = True}
            If picker.ShowDialog(owner) <> DialogResult.OK Then Return
            Using preview As New FrmDocumentFilePreview(picker.FileNames)
                preview.ShowDialog(owner)
            End Using
        End Using
    End Sub

    Private Sub LoadSelection(sender As Object, e As EventArgs)
        ClearImage()
        _text.Visible = True
        _text.Text = ""
        Dim filePath = TryCast(_files.SelectedItem, String)
        If filePath Is Nothing Then Return

        Try
            If String.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase) Then
                _text.Text = DocumentFilePreview.ReadPdfText(filePath)
            Else
                _image.Image = DocumentFilePreview.ReadImage(filePath)
                _image.Visible = True
                _text.Visible = False
            End If
        Catch ex As InvalidDataException
            _text.Text = ex.Message
        Catch ex As Exception
            _text.Text = "Impossible de lire ce fichier. Vérifiez qu’il est accessible et dans un format pris en charge."
        End Try
    End Sub

    Private Sub ClearImage()
        Dim previous = _image.Image
        _image.Image = Nothing
        previous?.Dispose()
        _image.Visible = False
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then ClearImage()
        MyBase.Dispose(disposing)
    End Sub
End Class
