Option Strict On

Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Friend NotInheritable Class FrmDocumentFilePreview
    Inherits Form

    Private Const DriveArchiveRoot As String = "G:\Mon Drive\AGUMAAA\Gestion\Trésorerie\Comptabilité\Eléments comptables"

    Private ReadOnly _files As New ListBox With {.Dock = DockStyle.Fill, .HorizontalScrollbar = True, .DisplayMember = "Name", .IntegralHeight = False}
    Private ReadOnly _text As New TextBox With {
        .Dock = DockStyle.Fill, .Multiline = True, .ReadOnly = True, .ScrollBars = ScrollBars.Vertical}
    Private ReadOnly _image As New PictureBox With {.Dock = DockStyle.Fill, .SizeMode = PictureBoxSizeMode.Zoom}

    Private ReadOnly _testWorkspace As OfflineTestWorkspace
    Private ReadOnly _sqlTest As SqlDocumentTestStore
    Private ReadOnly _sqlExample As Boolean
    Private ReadOnly _analyse As New Button With {
        .Text = "Analyser le document…", .AutoSize = True, .Anchor = AnchorStyles.Left}
    Private ReadOnly _pageNumber As New NumericUpDown With {.Minimum = 1, .Width = 72, .Visible = False}
    Private ReadOnly _pageCount As New Label With {.AutoSize = True, .Visible = False, .Margin = New Padding(3, 7, 3, 0)}
    Private ReadOnly _pathTip As New ToolTip()
    Private _pdfSource As String
    Private _pending As Boolean

    Public Sub New(paths As IEnumerable(Of String), Optional testWorkspace As OfflineTestWorkspace = Nothing,
                   Optional sqlTest As SqlDocumentTestStore = Nothing, Optional sqlExample As Boolean = False)
        If sqlTest IsNot Nothing AndAlso testWorkspace Is Nothing Then
            Throw New ArgumentNullException(NameOf(testWorkspace))
        End If
        _testWorkspace = testWorkspace
        _sqlTest = sqlTest
        _sqlExample = sqlExample
        If _sqlTest IsNot Nothing Then _analyse.Text = "Analyser / saisir le document…"
        Dim production = _testWorkspace Is Nothing AndAlso RealArchiveEnabled()
        Dim scope = If(production, "PROD — données réelles",
                    If(_sqlTest IsNot Nothing, "TEST SQL — bddAgumaaaTest", "TEST / DÉMO — analyse locale"))
        Text = scope & " | Examen des PDF et des scans"
        ClientSize = New Size(1000, 640)
        StartPosition = FormStartPosition.CenterParent

        Dim columns As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 3}
        columns.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30.0F))
        columns.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70.0F))
        Dim preview As New Panel With {.Dock = DockStyle.Fill}
        preview.Controls.Add(_image)
        preview.Controls.Add(_text)
        Dim banner As New Label With {
            .Text = scope, .Dock = DockStyle.Fill, .Margin = New Padding(0),
            .TextAlign = ContentAlignment.MiddleCenter, .ForeColor = Color.White,
            .BackColor = If(production, Color.DarkRed, Color.DarkBlue),
            .Font = New Font(Font.FontFamily, 17, FontStyle.Bold)}
        columns.Controls.Add(banner, 0, 0)
        columns.SetColumnSpan(banner, 2)
        Dim documents As New GroupBox With {.Text = "Documents sélectionnés", .Dock = DockStyle.Fill}
        documents.Controls.Add(_files)
        columns.Controls.Add(documents, 0, 1)
        columns.Controls.Add(preview, 1, 1)
        Controls.Add(columns)
        columns.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        columns.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        columns.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Dim commands As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .AutoSize = True}
        commands.Controls.AddRange(New Control() {_analyse, _pageNumber, _pageCount})
        columns.Controls.Add(commands, 0, 2)
        columns.SetColumnSpan(commands, 2)
        AddHandler _analyse.Click, AddressOf AnalyseSelection
        AddHandler _pageNumber.ValueChanged, Sub() LoadPdfPage()

        _files.Items.AddRange(paths.Select(Function(path) New FileInfo(path)).Cast(Of Object)().ToArray())
        AddHandler _files.SelectedIndexChanged, AddressOf LoadSelection
        If _files.Items.Count > 0 Then _files.SelectedIndex = 0
    End Sub

    Public Shared Sub OpenFiles(owner As IWin32Window, Optional testWorkspace As OfflineTestWorkspace = Nothing,
                                Optional sqlTest As SqlDocumentTestStore = Nothing)
        Using picker As New OpenFileDialog With {
            .Title = "Choisir des documents sur C: ou sur un lecteur Drive",
            .Filter = "PDF et scans (*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff)|*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
            .Multiselect = True, .CheckFileExists = True, .RestoreDirectory = True}
            If picker.ShowDialog(owner) <> DialogResult.OK Then Return
            Using preview As New FrmDocumentFilePreview(picker.FileNames, testWorkspace, sqlTest)
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
        Dim source = TryCast(_files.SelectedItem, FileInfo)?.FullName
        If source Is Nothing OrElse _pending Then Return
        Dim fictif = _testWorkspace IsNot Nothing
        Dim local = IsPdf(source) AndAlso (fictif OrElse Not RealArchiveEnabled())
        If Not fictif AndAlso Not local Then
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
        _pageNumber.Enabled = False
        Try
            If _sqlTest IsNot Nothing Then
                Await ReviewSqlTestDocument(source)
                Return
            End If
            If local Then
                Await AnalysePdfLocal(source)
                Return
            End If
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
                    save = TestSaveAction()
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
            Await ConfirmProposal(proposition, save, fictif)
        Catch ex As ArgumentException
            MessageBox.Show(Me, ex.Message, "Document non reconnu ou incomplet", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As IOException
            MessageBox.Show(Me, ex.Message, "Analyse ou archivage impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As TimeoutException
            MessageBox.Show(Me, ex.Message, "Analyse locale interrompue", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As InvalidOperationException
            MessageBox.Show(Me, ex.Message, "Traitement indisponible", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Data.SqlClient.SqlException When _sqlTest IsNot Nothing
            MessageBox.Show(Me, "Écriture SQL TEST impossible. " & ex.Message &
                            vbCrLf & "Si une copie a été créée, relancez la même opération pour la réutiliser.",
                            "bddAgumaaaTest", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            MessageBox.Show(Me, "Traitement interrompu. Si l’archive a été copiée, elle sera réutilisée au prochain essai.",
                            "Enregistrement à vérifier", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _files.Enabled = True
            _pageNumber.Enabled = True
            _analyse.Enabled = True
            _pending = False
        End Try
    End Sub

    Private Async Function ReviewSqlTestDocument(source As String) As Task
        Dim hash As String
        Dim suggested As Newtonsoft.Json.Linq.JObject = Nothing
        Dim readableText As String = Nothing
        Dim note = "Vérifiez les champs proposés à partir du document. Pour un chèque, confirmez son sens."
        Using sourceLock = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read)
            hash = DocumentArchiveProposal.HashSource(source)
            If _sqlExample Then
                suggested = Newtonsoft.Json.Linq.JObject.Parse(DemoDocumentSamples.Invoice)
            Else
                Try
                    readableText = Await Task.Run(Function() LocalDocumentAnalysis.ReadText(source))
                    suggested = LocalDocumentAnalysis.Classify(readableText)
                    If suggested Is Nothing OrElse IsIncompleteMembership(suggested) Then
                        suggested = Await ImproveDocumentExtraction(source, suggested)
                    End If
                    If suggested Is Nothing Then
                        note = "Type non reconnu. Choisissez le type après avoir vérifié le document."
                    ElseIf IsIncompleteMembership(suggested) Then
                        note = "Bulletin d’adhésion détecté. Vérifiez les champs lus et complétez les champs vides."
                    End If
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidOperationException OrElse
                    TypeOf ex Is InvalidDataException OrElse TypeOf ex Is TimeoutException OrElse TypeOf ex Is ArgumentException OrElse
                    TypeOf ex Is Global.System.ComponentModel.Win32Exception
                    note = "Lecture automatique indisponible. " & ex.Message
                End Try
            End If
        End Using

        If suggested Is Nothing Then ShowTranscript(readableText)
        Dim archiveRoot = If(_sqlExample, _testWorkspace.GetDocumentTestDirectory(), DriveArchiveRoot)
        Using editor As New FrmSqlTestDocumentEntry(source, hash, archiveRoot, suggested, note, archiveOnDrive:=Not _sqlExample)
            If editor.ShowDialog(Me) <> DialogResult.OK Then Return
            Await ConfirmProposal(editor.Proposal, TestSaveAction(), True)
        End Using
    End Function

    Private Shared Function IsIncompleteMembership(suggested As Newtonsoft.Json.Linq.JObject) As Boolean
        If suggested("type_document")?.ToString() <> "FormulaireAdhesion" Then Return False
        Dim identity = suggested("donnees_extraites")?("identite")
        Return {"nom", "prenom", "adresse"}.Any(Function(field) String.IsNullOrWhiteSpace(identity?(field)?.ToString()))
    End Function

    Private Shared Async Function ImproveDocumentExtraction(source As String,
        primary As Newtonsoft.Json.Linq.JObject) As Task(Of Newtonsoft.Json.Linq.JObject)
        Dim candidate As Newtonsoft.Json.Linq.JObject
        Try
            Dim sparseText = Await Task.Run(Function() LocalDocumentAnalysis.ReadText(source, sparse:=True))
            candidate = LocalDocumentAnalysis.Classify(sparseText)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException OrElse
            TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is TimeoutException OrElse
            TypeOf ex Is ArgumentException OrElse TypeOf ex Is Global.System.ComponentModel.Win32Exception
            Return primary
        End Try
        If primary Is Nothing Then Return candidate
        If candidate?("type_document")?.ToString() <> primary("type_document")?.ToString() Then Return primary
        Dim identity = DirectCast(primary("donnees_extraites")("identite"), Newtonsoft.Json.Linq.JObject)
        Dim additional = candidate("donnees_extraites")("identite")
        For Each field In {"nom", "prenom", "adresse"}
            If identity(field) Is Nothing AndAlso additional(field) IsNot Nothing Then identity(field) = additional(field).DeepClone()
        Next
        Return primary
    End Function

    Private Async Function AnalysePdfLocal(source As String) As Task
        Dim hash As String
        Dim extracted As Newtonsoft.Json.Linq.JObject
        Dim readableText As String
        Using sourceLock = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read)
            hash = DocumentArchiveProposal.HashSource(source)
            readableText = Await Task.Run(Function() LocalDocumentAnalysis.ReadText(source))
            extracted = LocalDocumentAnalysis.Classify(readableText)
        End Using

        ShowTranscript(readableText)

        If extracted Is Nothing OrElse extracted("type_document")?.ToString() <> "Facture" Then
            MessageBox.Show(Me, "Pour vérifier et classer ce document, ouvrez Analyse → Espace TEST SQL.", Text)
            Return
        End If
        Dim data = extracted("donnees_extraites")
        Dim transaction = data("transaction")
        Dim supplier = data("identite")("fournisseur_nom")
        Dim number = transaction("numero_facture")
        Dim amount = transaction("montant_ttc")
        Dim documentDate = transaction("date_facture")
        If _testWorkspace IsNot Nothing AndAlso _testWorkspace.SessionDirectory IsNot Nothing AndAlso
           supplier IsNot Nothing AndAlso number IsNot Nothing AndAlso amount IsNot Nothing AndAlso documentDate IsNot Nothing Then
            Dim proposal = DocumentArchiveProposal.Prepare(source, hash, extracted.ToString(),
                _testWorkspace.GetDocumentTestDirectory(), AddressOf OfflineTestWorkspace.DocumentArchiveTestSetting)
            Await ConfirmProposal(proposal, TestSaveAction(), True)
            Return
        End If

        Dim message = "Champs de facture proposés par l’analyse locale" & vbCrLf & vbCrLf &
            "Fournisseur : " & If(supplier?.ToString(), "À vérifier") & vbCrLf &
            "Numéro : " & If(number?.ToString(), "À vérifier") & vbCrLf &
            "Date : " & If(documentDate?.ToString(), "À vérifier") & vbCrLf &
            "Montant TTC : " & If(amount Is Nothing, "À vérifier", amount.ToString() & " EUR")
        If _testWorkspace IsNot Nothing AndAlso _testWorkspace.SessionDirectory Is Nothing Then
            message &= vbCrLf & vbCrLf & "Préparez une session TEST pour essayer l’archivage."
        End If
        MessageBox.Show(Me, message, "Analyse locale du PDF", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Function

    Private Sub ShowTranscript(readableText As String)
        If Not String.IsNullOrWhiteSpace(readableText) Then
            Using transcript As New Form With {.Text = "Texte lu localement — à vérifier", .ClientSize = New Size(760, 520),
                                               .StartPosition = FormStartPosition.CenterParent}
                transcript.Controls.Add(New TextBox With {.Text = readableText, .Dock = DockStyle.Fill,
                    .Multiline = True, .ReadOnly = True, .ScrollBars = ScrollBars.Vertical})
                transcript.ShowDialog(Me)
            End Using
        End If
    End Sub

    Private Function TestSaveAction() As Action(Of DocumentAgumaaa)
        If _sqlTest Is Nothing Then Return AddressOf _testWorkspace.SaveDocumentTestRecord
        Return Sub(doc)
                   ValidateTestDestination(doc.CheminDoc)
                   _sqlTest.Save(doc)
               End Sub
    End Function

    Private Sub ValidateTestDestination(destination As String)
        If _sqlTest Is Nothing OrElse _sqlExample Then
            _testWorkspace.ValidateDocumentTestDestination(destination)
            Return
        End If
        Dim allowedRoot = Path.Combine(DriveArchiveRoot, "TEST") & Path.DirectorySeparatorChar
        If Not Path.GetFullPath(destination).StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase) Then
            Throw New IOException("La destination est en dehors du dossier TEST du Drive.")
        End If
    End Sub

    Private Async Function ConfirmProposal(proposition As DocumentArchiveProposal, save As Action(Of DocumentAgumaaa), fictif As Boolean) As Task
        Dim action = If(_sqlTest IsNot Nothing, If(_sqlExample, "Enregistrer dans bddAgumaaaTest et archiver localement",
                            "Enregistrer dans bddAgumaaaTest et archiver sur le Drive TEST"),
                        If(fictif, "Simuler l’enregistrement et l’archivage", "Enregistrer en base et archiver"))
        If MessageBox.Show(Me, proposition.Summary & vbCrLf & vbCrLf & "Nom proposé : " & Path.GetFileName(proposition.Destination) & vbCrLf &
                           "Dossier proposé : " & Path.GetDirectoryName(proposition.Destination) & vbCrLf & vbCrLf & action & " ?",
                           action, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                           MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        If fictif Then ValidateTestDestination(proposition.Destination)
        Await Task.Run(Sub() proposition.Confirm(save))
        MessageBox.Show(Me, If(_sqlTest IsNot Nothing, If(_sqlExample, "Archive locale et fiche SQL TEST enregistrées.",
                                "Archive Drive et fiche SQL TEST enregistrées."),
                               If(fictif, "Archive et fiche TEST enregistrées.", "Document archivé et référencé en base.")),
                        action, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Function

    Private Shared Function IsPdf(filePath As String) As Boolean
        Return String.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase)
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        If _pending Then e.Cancel = True
    End Sub

    Private Sub LoadSelection(sender As Object, e As EventArgs)
        ClearImage()
        _pdfSource = Nothing
        _pageNumber.Visible = False
        _pageCount.Visible = False
        _text.Visible = True
        _text.Text = ""
        Dim filePath = TryCast(_files.SelectedItem, FileInfo)?.FullName
        _analyse.Enabled = filePath IsNot Nothing AndAlso
            (IsPdf(filePath) OrElse _testWorkspace IsNot Nothing OrElse RealArchiveEnabled())
        _pathTip.SetToolTip(_files, If(filePath, ""))
        If filePath Is Nothing Then Return

        Try
            If IsPdf(filePath) Then
                Dim count = DocumentFilePreview.PdfPageCount(filePath)
                If count < 1 Then Throw New InvalidDataException("Ce PDF ne contient aucune page.")
                _pageNumber.Maximum = count
                _pageNumber.Value = 1
                _pageCount.Text = $"sur {count} pages"
                _pageNumber.Visible = True
                _pageCount.Visible = True
                _pdfSource = filePath
                LoadPdfPage()
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

    Private Sub LoadPdfPage()
        If _pdfSource Is Nothing Then Return
        ClearImage()
        Try
            _image.Image = DocumentFilePreview.RenderPdfPage(_pdfSource, CInt(_pageNumber.Value) - 1)
            _image.Visible = True
            _text.Visible = False
        Catch ex As Exception
            _text.Text = "Impossible d’afficher cette page PDF."
            _text.Visible = True
        End Try
    End Sub

    Private Sub ClearImage()
        Dim previous = _image.Image
        _image.Image = Nothing
        previous?.Dispose()
        _image.Visible = False
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            ClearImage()
            _pathTip.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
