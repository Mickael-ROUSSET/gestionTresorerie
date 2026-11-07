Option Strict On

Imports System.Drawing
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq

Friend NotInheritable Class FrmSqlTestDocumentEntry
    Inherits Form

    Private ReadOnly _kind As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
    Private ReadOnly _direction As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
    Private ReadOnly _number As New TextBox With {.MaxLength = 60, .Dock = DockStyle.Fill}
    Private ReadOnly _party As New TextBox With {.MaxLength = 80, .Dock = DockStyle.Fill}
    Private ReadOnly _date As New TextBox With {.PlaceholderText = "JJ/MM/AAAA", .Dock = DockStyle.Fill}
    Private ReadOnly _amount As New TextBox With {.PlaceholderText = "Exemple : 42,50", .Dock = DockStyle.Fill}
    Private ReadOnly _partyLabel As New Label With {.AutoSize = True, .Anchor = AnchorStyles.Left}
    Private ReadOnly _source As String
    Private ReadOnly _hash As String
    Private ReadOnly _root As String

    Public Sub New(source As String, hash As String, root As String, suggested As JObject, note As String)
        _source = source
        _hash = hash
        _root = root
        Text = "Vérifier le document — bddAgumaaaTest"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False
        MaximizeBox = False
        ClientSize = New Size(540, 322)
        AutoScaleMode = AutoScaleMode.Font

        _kind.Items.AddRange(New Object() {"Facture", "Chèque"})
        _direction.Items.AddRange(New Object() {"Émis", "Reçu"})
        Dim table As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(12)}
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 155))
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        Dim help As New Label With {.Text = note, .Dock = DockStyle.Fill, .AutoSize = True}
        table.Controls.Add(help, 0, 0)
        table.SetColumnSpan(help, 2)
        table.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        AddField(table, 1, New Label With {.Text = "Type de document"}, _kind)
        AddField(table, 2, New Label With {.Text = "Sens du chèque"}, _direction)
        AddField(table, 3, New Label With {.Text = "Numéro du document"}, _number)
        AddField(table, 4, _partyLabel, _party)
        AddField(table, 5, New Label With {.Text = "Date du document"}, _date)
        AddField(table, 6, New Label With {.Text = "Montant en EUR"}, _amount)

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim cancel As New Button With {.Text = "Annuler", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        Dim validate As New Button With {.Text = "Vérifier le classement…", .AutoSize = True}
        buttons.Controls.AddRange(New Control() {cancel, validate})
        table.Controls.Add(buttons, 0, 7)
        table.SetColumnSpan(buttons, 2)
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 39))
        Controls.Add(table)
        CancelButton = cancel
        AcceptButton = validate

        AddHandler _kind.SelectedIndexChanged, AddressOf UpdateKind
        AddHandler validate.Click, AddressOf ValidateFields
        _kind.SelectedIndex = 0
        Dim fields = suggested?("donnees_extraites")
        _party.Text = If(fields?("identite")?("fournisseur_nom")?.ToString(), "")
        Dim transaction = fields?("transaction")
        _number.Text = If(transaction?("numero_facture")?.ToString(), "")
        _date.Text = If(transaction?("date_facture")?.ToString(), "")
        _amount.Text = If(transaction?("montant_ttc")?.ToString(), "")
    End Sub

    Public ReadOnly Property Proposal As DocumentArchiveProposal
        Get
            Return _proposal
        End Get
    End Property
    Private _proposal As DocumentArchiveProposal

    Private Shared Sub AddField(table As TableLayoutPanel, row As Integer, caption As Label, editor As Control)
        caption.AutoSize = True
        caption.Anchor = AnchorStyles.Left
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        table.Controls.Add(caption, 0, row)
        table.Controls.Add(editor, 1, row)
    End Sub

    Private Sub UpdateKind(sender As Object, e As EventArgs)
        _partyLabel.Text = If(_kind.SelectedIndex = 1, "Bénéficiaire", "Fournisseur")
        _direction.Enabled = _kind.SelectedIndex = 1
    End Sub

    Private Sub ValidateFields(sender As Object, e As EventArgs)
        Dim emitted As Boolean? = Nothing
        If _direction.SelectedIndex >= 0 Then emitted = _direction.SelectedIndex = 0
        Try
            _proposal = SqlTestDocumentEntry.Prepare(_source, _hash, _root, _kind.SelectedIndex = 1,
                _number.Text, _party.Text, _date.Text, _amount.Text, emitted)
            DialogResult = DialogResult.OK
        Catch ex As ArgumentException
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
