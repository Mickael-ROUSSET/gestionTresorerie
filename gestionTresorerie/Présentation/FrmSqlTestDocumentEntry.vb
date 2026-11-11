Option Strict On

Imports System.Drawing
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq
Imports gestionTresorerie.Agumaaa

Friend NotInheritable Class FrmSqlTestDocumentEntry
    Inherits Form

    Private Shared ReadOnly DocumentKinds As TypeDocument() = {
        TypeDocument.Facture, TypeDocument.Cheque, TypeDocument.FormulaireAdhesion,
        TypeDocument.QuestionnaireMedical, TypeDocument.Recu}
    Private ReadOnly _directionLabel As New Label With {.Text = "Sens du chèque"}
    Private ReadOnly _numberLabel As New Label
    Private ReadOnly _amountLabel As New Label
    Private ReadOnly _kind As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
    Private ReadOnly _direction As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
    Private ReadOnly _number As New TextBox With {.MaxLength = 60, .Dock = DockStyle.Fill}
    Private ReadOnly _party As New TextBox With {.MaxLength = 80, .Dock = DockStyle.Fill}
    Private ReadOnly _date As New TextBox With {.PlaceholderText = "JJ/MM/AAAA", .Dock = DockStyle.Fill}
    Private ReadOnly _amount As New TextBox With {.PlaceholderText = "Exemple : 42,50", .Dock = DockStyle.Fill}
    Private ReadOnly _issuer As New TextBox With {.MaxLength = 100, .Dock = DockStyle.Fill}
    Private ReadOnly _issuerLabel As New Label With {.Text = "Émetteur du chèque"}
    Private ReadOnly _firstName As New TextBox With {.MaxLength = 80, .Dock = DockStyle.Fill}
    Private ReadOnly _address As New TextBox With {.MaxLength = 240, .Dock = DockStyle.Fill}
    Private ReadOnly _firstNameLabel As New Label With {.Text = "Prénom"}
    Private ReadOnly _addressLabel As New Label With {.Text = "Adresse postale"}
    Private ReadOnly _archiveOnDrive As Boolean
    Private ReadOnly _partyLabel As New Label With {.AutoSize = True, .Anchor = AnchorStyles.Left}
    Private ReadOnly _source As String
    Private ReadOnly _hash As String
    Private ReadOnly _root As String

    Public Sub New(source As String, hash As String, root As String, suggested As JObject, note As String,
                   Optional archiveOnDrive As Boolean = False)
        _source = source
        _hash = hash
        _root = root
        _archiveOnDrive = archiveOnDrive
        Text = "Vérifier le document — bddAgumaaaTest"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False
        MaximizeBox = False
        ClientSize = New Size(580, 420)
        AutoScaleMode = AutoScaleMode.Font

        _kind.Items.AddRange(New Object() {"Facture", "Chèque", "Bulletin d’adhésion", "Questionnaire médical", "Reçu"})
        _direction.Items.AddRange(New Object() {"Émis", "Reçu"})
        Dim table As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(12)}
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 155))
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        Dim help As New Label With {.Text = note, .Dock = DockStyle.Fill, .AutoSize = True}
        table.Controls.Add(help, 0, 0)
        table.SetColumnSpan(help, 2)
        table.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        AddField(table, 1, New Label With {.Text = "Type de document"}, _kind)
        AddField(table, 2, _directionLabel, _direction)
        AddField(table, 3, _numberLabel, _number)
        AddField(table, 4, _partyLabel, _party)
        AddField(table, 5, New Label With {.Text = "Date du document"}, _date)
        AddField(table, 6, _amountLabel, _amount)

        AddField(table, 7, _firstNameLabel, _firstName)
        AddField(table, 8, _addressLabel, _address)

        AddField(table, 9, _issuerLabel, _issuer)

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim cancel As New Button With {.Text = "Annuler", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        Dim validate As New Button With {.Text = "Vérifier le classement…", .AutoSize = True}
        Dim importAnalysis As New Button With {.Text = "Importer une analyse ChatGPT", .AutoSize = True}
        AddHandler importAnalysis.Click, AddressOf ImportChatGptAnalysis
        buttons.Controls.AddRange(New Control() {cancel, validate, importAnalysis})
        table.Controls.Add(buttons, 0, 10)
        table.SetColumnSpan(buttons, 2)
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 39))
        Controls.Add(table)
        CancelButton = cancel
        AcceptButton = validate

        AddHandler _kind.SelectedIndexChanged, AddressOf UpdateKind
        AddHandler validate.Click, AddressOf ValidateFields
        FillFields(suggested)
        AddHandler _kind.SelectedIndexChanged,
            Sub()
                _number.Clear()
                _party.Clear()
                _issuer.Clear()
                _firstName.Clear()
                _address.Clear()
                _date.Clear()
                _amount.Clear()
                _direction.SelectedIndex = -1
            End Sub
    End Sub

    Private Sub ImportChatGptAnalysis(sender As Object, e As EventArgs)
        Try
            Dim clipboardText = Clipboard.GetText().Trim()
            If clipboardText.StartsWith("```json", StringComparison.OrdinalIgnoreCase) AndAlso
                clipboardText.EndsWith("```", StringComparison.Ordinal) AndAlso clipboardText.Length > 10 Then
                clipboardText = clipboardText.Substring(7, clipboardText.Length - 10).Trim()
            End If
            If clipboardText.Length = 0 OrElse clipboardText.Length > 32000 Then
                Throw New ArgumentException("Copiez d’abord le résultat JSON de ChatGPT.")
            End If

            Dim result = JObject.Parse(clipboardText, New JsonLoadSettings With {
                .DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error})
            Dim kind = result("type_document")
            Dim data = TryCast(result("donnees_extraites"), JObject)
            If kind Is Nothing OrElse kind.Type <> JTokenType.String OrElse
                Not Array.Exists(DocumentKinds, Function(value) value.ToString() = kind.ToString()) OrElse
                data Is Nothing Then
                Throw New ArgumentException("Le JSON doit contenir un type de document reconnu et les données extraites.")
            End If
            For Each section In {"identite", "transaction"}
                Dim fields = TryCast(data(section), JObject)
                If fields Is Nothing OrElse fields.Properties().Any(
                    Function(field) field.Value.Type <> JTokenType.String AndAlso
                        field.Value.Type <> JTokenType.Integer AndAlso field.Value.Type <> JTokenType.Float AndAlso
                        field.Value.Type <> JTokenType.Null) Then
                    Throw New ArgumentException("Les champs identité et transaction doivent contenir des valeurs simples.")
                End If
            Next
            If MessageBox.Show(Me, "Remplacer les champs actuels par les données copiées depuis ChatGPT ?",
                               Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                               MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            FillFields(result)
        Catch ex As Exception When TypeOf ex Is Newtonsoft.Json.JsonException OrElse TypeOf ex Is ArgumentException OrElse
            TypeOf ex Is System.Runtime.InteropServices.ExternalException
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub FillFields(suggested As JObject)
        _number.Clear()
        _party.Clear()
        _issuer.Clear()
        _firstName.Clear()
        _address.Clear()
        _date.Clear()
        _amount.Clear()
        _direction.SelectedIndex = -1
        Dim detectedType = suggested?("type_document")?.ToString()
        _kind.SelectedIndex = Array.FindIndex(DocumentKinds, Function(kind) kind.ToString() = detectedType)
        UpdateKind(Nothing, EventArgs.Empty)
        Dim fields = suggested?("donnees_extraites")
        Dim identity = fields?("identite")
        Dim transaction = fields?("transaction")
        If detectedType = "Cheque" Then
            _party.Text = If(identity?("beneficiaire_nom")?.ToString(), "")
            _issuer.Text = If(identity?("emetteur_du_cheque")?.ToString(), "")
            _number.Text = If(identity?("emetteur_numero_cheque")?.ToString(), "")
            _date.Text = If(transaction?("date_emission")?.ToString(), "")
            _amount.Text = If(transaction?("montant_numerique")?.ToString(), "")
        ElseIf detectedType = "Facture" Then
            _party.Text = If(identity?("fournisseur_nom")?.ToString(), "")
            _number.Text = If(transaction?("numero_facture")?.ToString(), "")
            _date.Text = If(transaction?("date_facture")?.ToString(), "")
            _amount.Text = If(transaction?("montant_ttc")?.ToString(), "")
        Else
            _party.Text = If(identity?("nom")?.ToString(), If(identity?("nom_complet")?.ToString(), ""))
            _firstName.Text = If(identity?("prenom")?.ToString(), "")
            _address.Text = If(identity?("adresse")?.ToString(), "")
            _number.Text = If(transaction?("reference")?.ToString(), "")
            _date.Text = If(transaction?("date_document")?.ToString(), "")
            _amount.Text = If(transaction?("montant_numerique")?.ToString(), "")
        End If
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

    Private ReadOnly Property SelectedKind As TypeDocument
        Get
            Return If(_kind.SelectedIndex < 0, TypeDocument.Aucun, DocumentKinds(_kind.SelectedIndex))
        End Get
    End Property

    Private Sub UpdateKind(sender As Object, e As EventArgs)
        Dim kind = SelectedKind
        Dim cheque = kind = TypeDocument.Cheque
        Dim receipt = kind = TypeDocument.Recu
        Dim financial = cheque OrElse receipt OrElse kind = TypeDocument.Facture
        _partyLabel.Text = If(cheque, "Bénéficiaire", If(kind = TypeDocument.Facture, "Fournisseur",
            If(receipt, "Nom ou organisme", "Nom et prénom")))
        Dim membership = kind = TypeDocument.FormulaireAdhesion
        If membership Then _partyLabel.Text = "Nom"
        _firstNameLabel.Visible = membership
        _firstName.Visible = membership
        _addressLabel.Visible = membership
        _address.Visible = membership
        _issuerLabel.Visible = cheque
        _issuer.Visible = cheque
        _directionLabel.Visible = cheque
        _direction.Visible = cheque
        _numberLabel.Text = If(receipt, "Numéro (facultatif)", "Numéro du document")
        _numberLabel.Visible = financial
        _number.Visible = financial
        _amountLabel.Text = If(receipt, "Montant EUR (facultatif)", "Montant en EUR")
        _amountLabel.Visible = financial
        _amount.Visible = financial
    End Sub

    Private Sub ValidateFields(sender As Object, e As EventArgs)
        Dim emitted As Boolean? = Nothing
        If _direction.SelectedIndex >= 0 Then emitted = _direction.SelectedIndex = 0
        Try
            Dim kind = SelectedKind
            If kind = TypeDocument.Aucun Then Throw New ArgumentException("Sélectionnez le type de document.")
            If kind = TypeDocument.Facture OrElse kind = TypeDocument.Cheque Then
                _proposal = SqlTestDocumentEntry.Prepare(_source, _hash, _root, kind = TypeDocument.Cheque,
                    _number.Text, _party.Text, _date.Text, _amount.Text, emitted, _archiveOnDrive, _issuer.Text)
            Else
                _proposal = DocumentArchiveProposal.PrepareAdministrativeTest(_source, _hash, _root, kind,
                    _party.Text, _date.Text, _number.Text, _amount.Text, _firstName.Text, _address.Text, _archiveOnDrive)
            End If
            DialogResult = DialogResult.OK
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IO.IOException
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
