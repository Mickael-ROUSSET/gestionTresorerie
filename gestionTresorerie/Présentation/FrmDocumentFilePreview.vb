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

    Private ReadOnly _testWorkspace As OfflineTestWorkspace
    Private ReadOnly _analyse As New Button With {
        .Text = "Analyser le document…", .AutoSize = True, .Anchor = AnchorStyles.Left}
    Private _pending As Boolean

    Public Sub New(paths As IEnumerable(Of String), Optional testWorkspace As OfflineTestWorkspace = Nothing)
        _testWorkspace = testWorkspace
        Text = "Examen des PDF et des scans"
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
        If _testWorkspace IsNot Nothing OrElse RealArchiveEnabled() Then
            columns.RowCount = 2
            columns.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            columns.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            columns.Controls.Add(_analyse, 0, 1)
            columns.SetColumnSpan(_analyse, 2)
            AddHandler _analyse.Click, AddressOf AnalyseSelection
        End If

        _files.Items.AddRange(paths.Cast(Of Object)().ToArray())
        AddHandler _files.SelectedIndexChanged, AddressOf LoadSelection
        If _files.Items.Count > 0 Then _files.SelectedIndex = 0
    End Sub

    Public Shared Sub OpenFiles(owner As IWin32Window, Optional testWorkspace As OfflineTestWorkspace = Nothing)
        Using picker As New OpenFileDialog With {
            .Title = "Choisir des documents sur C: ou sur un lecteur Drive",
            .Filter = "PDF et scans (*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff)|*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
            .Multiselect = True, .CheckFileExists = True, .RestoreDirectory = True}
            If picker.ShowDialog(owner) <> DialogResult.OK Then Return
            Using preview As New FrmDocumentFilePreview(picker.FileNames, testWorkspace)
                preview.ShowDialog(owner)
            End Using
        End Using
    End Sub

    Private Shared Function RealArchiveEnabled() As Boolean
#If DEMO_BUILD Then
        Return False
#Else
        Return EnvironmentManager.CurrentEnvironment = EnvironmentManager.EnvironnementType.PROD
#End If
    End Function

    Private Async Sub AnalyseSelection(sender As Object, e As EventArgs)
        Dim source = TryCast(_files.SelectedItem, String)
        If source Is Nothing OrElse _pending Then Return
        Dim fictif = _testWorkspace IsNot Nothing
        If Not fictif Then
            If Not RealArchiveEnabled() Then Return
            Select Case Path.GetExtension(source).ToLowerInvariant()
                Case ".pdf", ".jpg", ".jpeg", ".png"
                Case Else
                    MessageBox.Show(Me, "L’analyse actuelle accepte les PDF, JPEG et PNG. Convertissez le TIFF en PDF avant de l’analyser.")
                    Return
            End Select
            If MessageBox.Show(Me, "Envoyer ce document à Gemini, l’extracteur configuré dans l’application ?", "Analyse du document",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        End If

        _pending = True
        _analyse.Enabled = False
        _files.Enabled = False
        Try
            Dim json As String
            Dim hash As String
            Dim root As String
            Dim settings As Func(Of String, String)
            Dim save As Action(Of DocumentAgumaaa)
            Using sourceLock = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read)
                hash = DocumentArchiveProposal.HashSource(source)
                If fictif Then
                    root = _testWorkspace.GetDocumentTestDirectory()
                    json = _testWorkspace.ReadDocumentExtraction(source)
                    settings = AddressOf OfflineTestWorkspace.DocumentArchiveTestSetting
                    save = AddressOf _testWorkspace.SaveDocumentTestRecord
                Else
                    Dim key = GlobalSettings.GeminiKey
                    If String.IsNullOrWhiteSpace(key) Then Throw New InvalidOperationException("La clé Gemini existante n’est pas configurée.")
                    json = Await GeminiAnalyzer.AnalyserDocument(key, source, rapportDetaille:=False)
                    If String.IsNullOrWhiteSpace(json) Then Throw New InvalidOperationException("L’extracteur n’a pas renvoyé de données exploitables.")
                    root = LectureProprietes.GetVariable("repRacineAgumaaa")
                    settings = AddressOf LectureProprietes.GetVariable
                    save = AddressOf DocumentAgumaaa.EnregistrerArchive
                End If
            End Using

            Dim proposition = DocumentArchiveProposal.Prepare(source, hash, json, root, settings)
            Dim action = If(fictif, "Simuler l’enregistrement et l’archivage", "Enregistrer en base et archiver")
            If MessageBox.Show(Me, proposition.Summary & vbCrLf & vbCrLf & "Archive proposée" & vbCrLf &
                               proposition.Destination & vbCrLf & vbCrLf & action & " ?",
                               action, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                               MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            If fictif Then _testWorkspace.ValidateDocumentTestDestination(proposition.Destination)
            Await Task.Run(Sub() proposition.Confirm(save))
            MessageBox.Show(Me, If(fictif, "Archive et fiche TEST enregistrées.", "Document archivé et référencé en base."),
                            action, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As ArgumentException
            MessageBox.Show(Me, ex.Message, "Document non reconnu ou incomplet", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As IOException
            MessageBox.Show(Me, ex.Message, "Analyse ou archivage impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As InvalidOperationException
            MessageBox.Show(Me, ex.Message, "Traitement indisponible", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            MessageBox.Show(Me, "Traitement interrompu. Si l’archive a été copiée, elle sera réutilisée au prochain essai.",
                            "Enregistrement à vérifier", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _files.Enabled = True
            _analyse.Enabled = True
            _pending = False
        End Try
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        If _pending Then e.Cancel = True
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
