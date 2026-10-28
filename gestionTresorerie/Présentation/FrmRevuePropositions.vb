Imports System.IO
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' Revue manuelle hors ligne de propositions documentaires.
''' Aucun acces Gmail ni aucune ecriture comptable ne sont effectues ici.
''' </summary>
Public NotInheritable Class FrmRevuePropositions
    Inherits Form

    Private ReadOnly _store As DocumentReviewStore
    Private ReadOnly _grid As New DataGridView()
    Private ReadOnly _editor As New TextBox()
    Private ReadOnly _status As New Label()
    Private ReadOnly _approve As New Button()
    Private ReadOnly _reject As New Button()

    Public Sub New(proposalsFolder As String)
        _store = New DocumentReviewStore(proposalsFolder)
        Text = "Propositions documentaires - validation humaine"
        StartPosition = FormStartPosition.CenterParent
        Size = New Drawing.Size(1100, 650)
        MinimumSize = New Drawing.Size(800, 450)

        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3}
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))

        Dim actions As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(6)}
        Dim reload As New Button With {.Text = "Actualiser", .AutoSize = True}
        _approve.Text = "Approuver pour saisie"
        _approve.AutoSize = True
        _reject.Text = "Rejeter"
        _reject.AutoSize = True
        _approve.Enabled = False
        _reject.Enabled = False
        actions.Controls.AddRange(New Control() {reload, _approve, _reject})

        Dim body As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1}
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52))
        body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 48))
        Dim pendingGroup As New GroupBox With {.Text = "Propositions en attente", .Dock = DockStyle.Fill}
        Dim editGroup As New GroupBox With {.Text = "Extraction JSON (modifiable avant approbation)", .Dock = DockStyle.Fill}

        _grid.Dock = DockStyle.Fill
        _grid.ReadOnly = True
        _grid.MultiSelect = False
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        _grid.AllowUserToAddRows = False
        _grid.AllowUserToDeleteRows = False
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        _grid.Columns.Add("sha", "Empreinte SHA-256")
        _grid.Columns.Add("source", "Document source")
        _grid.Columns.Add("moteur", "Moteur")
        pendingGroup.Controls.Add(_grid)

        _editor.Dock = DockStyle.Fill
        _editor.Multiline = True
        _editor.ScrollBars = ScrollBars.Both
        _editor.AcceptsTab = True
        _editor.WordWrap = False
        editGroup.Controls.Add(_editor)
        body.Controls.Add(pendingGroup, 0, 0)
        body.Controls.Add(editGroup, 1, 0)

        _status.Dock = DockStyle.Fill
        _status.Text = "Aucune operation comptable ne sera creee par cette fenetre."
        _status.AutoEllipsis = True
        root.Controls.Add(actions, 0, 0)
        root.Controls.Add(body, 0, 1)
        root.Controls.Add(_status, 0, 2)
        Controls.Add(root)

        AddHandler Shown, AddressOf OnShown
        AddHandler reload.Click, AddressOf OnReload
        AddHandler _grid.SelectionChanged, AddressOf OnSelectionChanged
        AddHandler _approve.Click, AddressOf OnApprove
        AddHandler _reject.Click, AddressOf OnReject
    End Sub

    Private Sub OnShown(sender As Object, e As EventArgs)
        ReloadProposals()
    End Sub

    Private Sub OnReload(sender As Object, e As EventArgs)
        ReloadProposals()
    End Sub

    Private Sub ReloadProposals()
        _grid.Rows.Clear()
        _editor.Clear()
        _approve.Enabled = False
        _reject.Enabled = False
        Try
            Dim proposals = _store.ListPendingProposals()
            For Each proposal In proposals
                Dim rowIndex = _grid.Rows.Add(
                    proposal("source_sha256").ToString(),
                    If(proposal("source") Is Nothing, "", proposal("source").ToString()),
                    If(proposal("moteur") Is Nothing, "", proposal("moteur").ToString()))
                _grid.Rows(rowIndex).Tag = proposal
            Next
            _status.Text = proposals.Count.ToString() & " proposition(s) en attente. Aucune ecriture comptable."
            If _grid.Rows.Count > 0 Then
                _grid.Rows(0).Selected = True
                OnSelectionChanged(Me, EventArgs.Empty)
            End If
        Catch ex As Exception
            _status.Text = "Catalogue inaccessible : " & ex.Message
            MessageBox.Show(Me, ex.Message, "Erreur de lecture - aucune decision enregistree",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function SelectedProposal() As JObject
        If _grid.SelectedRows.Count <> 1 Then Return Nothing
        Return TryCast(_grid.SelectedRows(0).Tag, JObject)
    End Function

    Private Sub OnSelectionChanged(sender As Object, e As EventArgs)
        Dim proposal = SelectedProposal()
        _approve.Enabled = proposal IsNot Nothing
        _reject.Enabled = proposal IsNot Nothing
        _editor.Text = If(proposal Is Nothing, "",
                          proposal("extraction").ToString(Formatting.Indented))
    End Sub

    Private Sub OnApprove(sender As Object, e As EventArgs)
        RecordDecision(DocumentReviewStore.ReviewDecision.ApprouvePourSaisie)
    End Sub

    Private Sub OnReject(sender As Object, e As EventArgs)
        RecordDecision(DocumentReviewStore.ReviewDecision.Rejete)
    End Sub

    Private Sub RecordDecision(decision As DocumentReviewStore.ReviewDecision)
        Dim proposal = SelectedProposal()
        If proposal Is Nothing Then Return

        Dim corrected As JObject = Nothing
        If decision = DocumentReviewStore.ReviewDecision.ApprouvePourSaisie Then
            Try
                corrected = JObject.Parse(_editor.Text)
                If Not corrected.HasValues Then Throw New JsonException("Extraction vide.")
            Catch ex As JsonException
                MessageBox.Show(Me, ex.Message, "Correction JSON invalide",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End Try
        End If

        Dim reviewer = Microsoft.VisualBasic.Interaction.InputBox(
            "Indiquez l'identite du validateur :", "Validation humaine")
        If String.IsNullOrWhiteSpace(reviewer) Then Return

        Dim action = If(decision = DocumentReviewStore.ReviewDecision.Rejete,
                        "rejeter", "approuver pour une saisie ulterieure")
        If MessageBox.Show(Me, "Confirmer : " & action & " cette proposition ?" & vbCrLf &
                           "Aucune operation comptable ne sera creee.",
                           "Confirmation obligatoire", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question) <> DialogResult.Yes Then Return

        Try
            Dim saved = _store.RecordReview(proposal("source_sha256").ToString(),
                                            reviewer, decision, corrected)
            If Not saved Then
                MessageBox.Show(Me, "Cette proposition a deja ete examinee.",
                                "Decision deja presente", MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            ' Ne pas effacer une correction saisie par l'utilisateur si la
            ' decision echoue (source modifiee, recu invalide, erreur disque).
            ' Il peut corriger le probleme puis retenter la validation.
            MessageBox.Show(Me, ex.Message, "Decision non enregistree",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        ReloadProposals()
    End Sub
End Class
