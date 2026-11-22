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
    Private ReadOnly _direction As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Enabled = False}
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

    Private ReadOnly _extra As New Dictionary(Of String, TextBox)
    Private ReadOnly _birth As New TextBox With {.Dock = DockStyle.Fill, .PlaceholderText = "JJ/MM/AAAA"}
    Private ReadOnly _birthLabel As New Label With {.Text = "Date de naissance"}
    Private ReadOnly _extraLabels As New List(Of Label)
    Private ReadOnly _destination As New TextBox With {.ReadOnly = True, .Multiline = True, .Dock = DockStyle.Fill, .ScrollBars = ScrollBars.Vertical}
    Private ReadOnly catalogue As New Data.DataTable()
    Private ReadOnly _nature As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly _sousNature As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private debitMouvement As Boolean?
    Private ReadOnly _eventName As New ComboBox With {.Dock = DockStyle.Fill, .MaxLength = 100}
    Private ReadOnly _attach As New Button With {.AutoSize = True, .Text = "Rattacher à un mouvement…"}
    Private _movement As Integer?
    Private _ready As Boolean

    Public Sub New(source As String, hash As String, root As String, suggested As JObject, note As String,
                   Optional archiveOnDrive As Boolean = False)
        _source = source
        _hash = hash
        _root = root
        _archiveOnDrive = archiveOnDrive
        Text = "Vérifier le document — bddAgumaaaTest"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.Sizable
        MinimizeBox = False
        MaximizeBox = False
        ClientSize = New Size(820, Math.Min(740, Screen.FromControl(Me).WorkingArea.Height - 90))
        AutoScaleMode = AutoScaleMode.Font

        _kind.Items.AddRange(New Object() {"Facture", "Chèque", "Bulletin d’adhésion", "Questionnaire médical", "Reçu"})
        _direction.Items.AddRange(New Object() {"Émis", "Reçu"})
        Dim table As New TableLayoutPanel With {.AutoScroll = True, .Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(12)}
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 155))
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        Dim help As New Label With {.Text = note, .Dock = DockStyle.Fill, .AutoSize = True}
        table.Controls.Add(help, 0, 0)
        table.SetColumnSpan(help, 2)
        table.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        AddField(table, 1, New Label With {.Text = "Type de document"}, _kind)
        AddField(table, 2, _directionLabel, _direction)
        AddField(table, 3, _numberLabel, _number)
        AddField(table, 4, _partyLabel, _party)
        AddField(table, 5, New Label With {.Text = "Date du document"}, _date)
        AddField(table, 6, _amountLabel, _amount)

        AddField(table, 7, _firstNameLabel, _firstName)
        AddField(table, 8, _addressLabel, _address)

        AddField(table, 9, _issuerLabel, _issuer)

        AddIdentityField(table, 10, "nom_usage", "Nom d’usage", 50)
        AddIdentityField(table, 11, "ville", "Ville", 50)
        AddIdentityField(table, 12, "code_postal", "Code postal", 10)
        AddIdentityField(table, 13, "mail", "E-mail", 150)
        AddIdentityField(table, 14, "telephone", "Téléphone", 30)
        AddIdentityField(table, 15, "genre", "Genre", 30)
        AddField(table, 16, _birthLabel, _birth)
        If archiveOnDrive Then
            Try
                Using cn = SqlDocumentTestStore.OpenTest(), natures = ParametresComptables.LireNatures(cn)
                    catalogue.Merge(natures)
                End Using
                Using choices = SqlDocumentTestStore.ClassificationChoices()
                    For Each row As Data.DataRow In choices.Rows
                        Dim label = Convert.ToString(row("libelle")).Trim()
                        If CStr(row("famille")) <> "evenement" Then Continue For
                        Dim target = _eventName
                        If label.Length > 0 AndAlso label.Length <= 100 AndAlso Not target.Items.Contains(label) Then target.Items.Add(label)
                    Next
                End Using
            Catch ex As Data.SqlClient.SqlException
                ' La première analyse reste possible avant la création du catalogue TEST.
            End Try
        End If
        AddField(table, 17, New Label With {.Text = "Catégorie"}, _nature)
        AddField(table, 18, New Label With {.Text = "Sous-catégorie"}, _sousNature)
        AddHandler _nature.SelectedIndexChanged, Sub() ChargerSousCategories()
        AddHandler _direction.SelectedIndexChanged, Sub() ChargerCategories()
        ChargerCategories()
        Dim eventEditor As New TableLayoutPanel With {.Dock = DockStyle.Fill, .AutoSize = True, .ColumnCount = 2}
        eventEditor.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        eventEditor.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        eventEditor.Controls.Add(_eventName, 0, 0)
        Dim createEvent As New Button With {.AutoSize = True, .Text = "Créer un événement…", .Visible = archiveOnDrive}
        eventEditor.Controls.Add(createEvent, 1, 0)
        AddHandler createEvent.Click, Sub()
            Dim libelle = SaisieEvenement.Demander(Me, _eventName.Text, AddressOf SqlDocumentTestStore.CreerEvenement)
            If libelle Is Nothing Then Return
            If Not _eventName.Items.Contains(libelle) Then _eventName.Items.Add(libelle)
            _eventName.SelectedItem = libelle
        End Sub
        AddField(table, 19, New Label With {.Text = "Événement ou activité"}, eventEditor)
        table.Controls.Add(New Label With {.Text = "Destination de la copie", .AutoSize = True}, 0, 20)
        table.Controls.Add(_destination, 1, 20)
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 54))

        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim cancel As New Button With {.Text = "Annuler", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        Dim validate As New Button With {.Text = "Enregistrer et archiver", .AutoSize = True}
        Dim importAnalysis As New Button With {.Text = "Importer une analyse ChatGPT", .AutoSize = True}
        AddHandler importAnalysis.Click, AddressOf ImportChatGptAnalysis
        buttons.Controls.AddRange(New Control() {cancel, validate, _attach, importAnalysis})
        AddHandler _attach.Click, AddressOf PickMovement
        AddHandler _date.TextChanged, Sub() ClearMovement()
        AddHandler _amount.TextChanged, Sub() ClearMovement()
        table.Controls.Add(buttons, 0, 21)
        table.SetColumnSpan(buttons, 2)
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 39))
        Dim prompt As New Button With {.Text = "Copier la consigne ChatGPT", .AutoSize = True, .Anchor = AnchorStyles.Left}
        AddHandler prompt.Click, AddressOf CopyChatGptPrompt
        table.Controls.Add(prompt, 0, 22)
        table.SetColumnSpan(prompt, 2)
        table.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        Dim renommer As New Button With {.Text = "Renommer le fichier source…", .AutoSize = True, .Visible = archiveOnDrive}
        table.Controls.Add(renommer, 1, 23)
        AddHandler renommer.Click, AddressOf RenommerSource
        Controls.Add(table)
        CancelButton = cancel
        AcceptButton = validate

        AddHandler _kind.SelectedIndexChanged, AddressOf UpdateKind
        AddHandler validate.Click, AddressOf ValidateFields
        AddHandler _issuer.TextChanged, AddressOf UpdateChequeDirection
        FillFields(suggested)
        AddHandler _kind.SelectedIndexChanged,
            Sub()
                _nature.Text = ""
                _eventName.Text = ""
                _number.Clear()
                _party.Clear()
                _issuer.Clear()
                _birth.Clear()
        _firstName.Clear()
                _address.Clear()
                For Each field In _extra.Values
                    field.Clear()
                Next
                _date.Text = Today.ToString("dd/MM/yyyy")
                _amount.Clear()
                _direction.SelectedIndex = -1
            End Sub
        For Each field In table.Controls.OfType(Of TextBox)().Where(Function(t) t IsNot _destination)
            AddHandler field.TextChanged, Sub() RefreshDestination()
        Next
        AddHandler _kind.SelectedIndexChanged, Sub() RefreshDestination()
        _ready = True
        RefreshDestination()
    End Sub

    Private Sub ChargerCategories()
        If Not catalogue.Columns.Contains("cat") Then Return
        Dim debit As Boolean? = debitMouvement
        If SelectedKind = TypeDocument.Cheque Then
            debit = If(_direction.SelectedIndex < 0, CType(Nothing, Boolean?), _direction.SelectedIndex = 0)
        End If
        Dim vue As New Data.DataView(catalogue)
        If debit.HasValue Then
            vue.RowFilter = "debit=" & If(debit.Value, "1", "0")
        ElseIf SelectedKind = TypeDocument.Cheque Then
            vue.RowFilter = "1=0"
        End If
        _nature.DisplayMember = "categorie"
        _nature.ValueMember = "cat"
        _nature.DataSource = vue.ToTable(True, "cat", "categorie")
        _nature.SelectedIndex = -1
        ChargerSousCategories()
    End Sub

    Private Sub ChargerSousCategories()
        _sousNature.DataSource = Nothing
        Dim selected = TryCast(_nature.SelectedItem, Data.DataRowView)
        If selected Is Nothing Then Return
        Dim vue As New Data.DataView(catalogue) With {.RowFilter = "cat=" & CInt(selected("cat")) & " AND sousCat IS NOT NULL"}
        _sousNature.DisplayMember = "sousCategorie"
        _sousNature.ValueMember = "sousCat"
        _sousNature.DataSource = vue.ToTable(True, "sousCat", "sousCategorie")
        _sousNature.SelectedIndex = -1
    End Sub

    Private Sub AddIdentityField(table As TableLayoutPanel, row As Integer, key As String, caption As String, capacity As Integer)
        Dim field As New TextBox With {.Dock = DockStyle.Fill, .MaxLength = capacity}
        Dim label As New Label With {.Text = caption}
        _extra.Add(key, field)
        _extraLabels.Add(label)
        AddField(table, row, label, field)
    End Sub

    Friend Property RenamedPath As String

    Private Sub RenommerSource(sender As Object, args As EventArgs)
        Try
            Dim dossier = IO.Path.GetDirectoryName(IO.Path.GetFullPath(_source))
            If IO.Path.GetFullPath(_source).StartsWith(IO.Path.TrimEndingDirectorySeparator(IO.Path.GetFullPath(_root)) & IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) Then
                Throw New ArgumentException("Le document se trouve déjà dans le dossier d’archives.")
            End If
            Dim nom = IO.Path.GetFileName(BuildProposal(dossier).Destination)
            Dim cible = IO.Path.Combine(dossier, nom)
            If String.Equals(cible, _source, StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show(Me, "Le fichier porte déjà le nom proposé.")
                Return
            End If
            If MessageBox.Show(Me, IO.Path.GetFileName(_source) & vbCrLf & "→ " & nom & vbCrLf & "Renommer dans le dossier actuel ?", "Renommer le fichier", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Using verrou = IO.File.Open(_source, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read Or IO.FileShare.Delete)
                If DocumentArchiveProposal.HashSource(_source) <> _hash Then Throw New IO.IOException("Le fichier a changé depuis son analyse.")
                IO.File.Move(_source, cible, overwrite:=False)
            End Using
            RenamedPath = cible
            DialogResult = DialogResult.Retry
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Renommage impossible")
        End Try
    End Sub

    Private Sub RefreshDestination()
        If Not _ready Then Return
        Try
            _destination.Text = BuildProposal().Destination
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IO.IOException
            _destination.Text = IO.Path.Combine(_root, If(_archiveOnDrive, "TEST", "")) & Environment.NewLine & ex.Message
        End Try
    End Sub

    Private Sub CopyChatGptPrompt(sender As Object, e As EventArgs)
        Dim prompt = ChatGptDocumentReader.Prompt
        Try
            Clipboard.SetText(prompt)
            MessageBox.Show(Me, "Colle la consigne dans ChatGPT avec le document, puis copie le JSON obtenu.", Text)
        Catch ex As System.Runtime.InteropServices.ExternalException
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub UpdateChequeDirection(sender As Object, e As EventArgs)
        Dim issuer = _issuer.Text.Trim()
        If SelectedKind <> TypeDocument.Cheque OrElse issuer.Length = 0 Then
            _direction.SelectedIndex = -1
            Return
        End If
        ' Certains chèques portent la mention imprimée « ASSOC. AGUMAA ».
        Dim isAgumaaa = System.Text.RegularExpressions.Regex.IsMatch(issuer,
            "(?<![\p{L}\p{N}])A[ .-]*G[ .-]*U[ .-]*M[ .-]*A[ .-]*A(?:[ .-]*A)?(?![\p{L}\p{N}])",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase Or System.Text.RegularExpressions.RegexOptions.CultureInvariant)
        _direction.SelectedIndex = If(isAgumaaa, 0, 1)
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

            Dim result = ChatGptDocumentReader.ParseResult(clipboardText)
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
        For Each field In _extra.Values
            field.Clear()
        Next
        _number.Clear()
        _party.Clear()
        _issuer.Clear()
        _birth.Clear()
        _firstName.Clear()
        _address.Clear()
        _date.Clear()
        _amount.Clear()
        _direction.SelectedIndex = -1
        Dim detectedType = suggested?("type_document")?.ToString()
        _kind.SelectedIndex = Array.FindIndex(DocumentKinds, Function(kind) kind.ToString() = detectedType)
        UpdateKind(Nothing, EventArgs.Empty)
        Dim classification = TryCast(suggested?("classement_comptable"), JObject)
        Dim suggestionNature = If(classification?("nature")?.Type = JTokenType.String, classification("nature").ToString(), "")
        _eventName.Text = If(classification?("evenement")?.Type = JTokenType.String, classification("evenement").ToString(), "")
        If detectedType = "QuestionnaireMedical" Then _nature.Text = ""
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
            If detectedType = "QuestionnaireMedical" Then _birth.Text = If(identity?("date_naissance")?.ToString(), "")
            _firstName.Text = If(identity?("prenom")?.ToString(), "")
            _address.Text = If(identity?("adresse")?.ToString(), "")
            For Each pair In _extra
                pair.Value.Text = If(identity?(pair.Key)?.ToString(), "")
            Next
            _number.Text = If(transaction?("reference")?.ToString(), "")
            _date.Text = If(transaction?("date_document")?.ToString(), "")
            _amount.Text = If(transaction?("montant_numerique")?.ToString(), "")
        End If
        If String.IsNullOrWhiteSpace(_date.Text) Then _date.Text = Today.ToString("dd/MM/yyyy")
        If suggestionNature <> "" AndAlso SelectedKind <> TypeDocument.QuestionnaireMedical Then
            _nature.SelectedIndex = _nature.FindStringExact(suggestionNature)
            If _nature.SelectedIndex < 0 Then
                Dim candidats = catalogue.AsEnumerable().Where(Function(row) String.Equals(Convert.ToString(row("sousCategorie")), suggestionNature, StringComparison.CurrentCultureIgnoreCase) AndAlso
                    _nature.Items.Cast(Of Data.DataRowView)().Any(Function(cat) CInt(cat("cat")) = CInt(row("cat")))).Take(2).ToArray()
                If candidats.Length = 1 Then
                    _nature.SelectedValue = candidats(0)("cat")
                    _sousNature.SelectedValue = candidats(0)("sousCat")
                End If
            End If
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
        table.RowStyles.Add(New RowStyle(SizeType.AutoSize))
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
        _birth.Visible = kind = TypeDocument.QuestionnaireMedical
        _birthLabel.Visible = _birth.Visible OrElse kind = TypeDocument.QuestionnaireMedical
        Dim membership = kind = TypeDocument.FormulaireAdhesion
        If membership Then _partyLabel.Text = "Nom"
        For Each field In _extra.Values
            field.Visible = membership
        Next
        For Each label In _extraLabels
            label.Visible = membership
        Next
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
        _attach.Visible = financial
        ChargerCategories()
    End Sub

    Private Function BuildProposal(Optional racine As String = Nothing) As DocumentArchiveProposal
        racine = If(racine, _root)
        Dim emitted As Boolean? = Nothing
        If _direction.SelectedIndex >= 0 Then emitted = _direction.SelectedIndex = 0
        Dim kind = SelectedKind
        If kind = TypeDocument.Cheque AndAlso Not emitted.HasValue Then
            Throw New ArgumentException("Renseignez l’émetteur pour déterminer le sens du chèque.")
        End If
        If kind = TypeDocument.Aucun Then Throw New ArgumentException("Sélectionnez le type de document.")
        If kind = TypeDocument.Facture OrElse kind = TypeDocument.Cheque Then
            Return SqlTestDocumentEntry.Prepare(_source, _hash, racine, kind = TypeDocument.Cheque,
                _number.Text, _party.Text, _date.Text, _amount.Text, emitted, _archiveOnDrive, _issuer.Text)
        Else
            Dim identity As New JObject()
            If kind = TypeDocument.QuestionnaireMedical Then identity("date_naissance") = _birth.Text.Trim()
            If kind = TypeDocument.FormulaireAdhesion Then
                If _party.Text.Trim().Length > 50 OrElse _firstName.Text.Trim().Length > 50 OrElse _address.Text.Trim().Length > 100 Then Throw New ArgumentException("Limites de la fiche tiers : 50 caractères pour le nom et le prénom, 100 pour la rue.")
                If String.IsNullOrWhiteSpace(_firstName.Text) Then Throw New ArgumentException("Renseignez le prénom pour identifier le tiers.")
                For Each pair In _extra
                    identity(pair.Key) = pair.Value.Text.Trim()
                Next
            End If
            Return DocumentArchiveProposal.PrepareAdministrativeTest(_source, _hash, racine, kind,
                _party.Text, _date.Text, _number.Text, _amount.Text, _firstName.Text, _address.Text, _archiveOnDrive, identity)
        End If
    End Function

    Private Sub ClearMovement()
        _movement = Nothing
        If debitMouvement.HasValue Then
            debitMouvement = Nothing
            ChargerCategories()
        End If
        _attach.Text = "Rattacher à un mouvement…"
    End Sub

    Private Sub PickMovement(sender As Object, e As EventArgs)
        Try
            Dim proposal = BuildProposal()
            Dim amount As Decimal
            If Not Decimal.TryParse(_amount.Text.Replace(" ", "").Replace(","c, "."c), Globalization.NumberStyles.Number, Globalization.CultureInfo.InvariantCulture, amount) Then Throw New ArgumentException("Renseignez le montant avant le rapprochement.")
            Using choices = SqlDocumentTestStore.RelatedMovements(proposal.Document.DateDoc, amount)
                If choices.Rows.Count = 0 Then
                    MessageBox.Show(Me, "Aucun mouvement de ce montant à moins de 91 jours. La pièce peut être classée en attente.")
                    Return
                End If
                Using dialog As New Form With {.Text = "Choisir le mouvement du justificatif", .Width = 680, .Height = 320, .StartPosition = FormStartPosition.CenterParent},
                    list As New ListBox With {.Dock = DockStyle.Fill, .DisplayMember = "description", .ValueMember = "Id", .DataSource = choices},
                    confirm As New Button With {.Dock = DockStyle.Bottom, .Text = "Rattacher le mouvement sélectionné", .DialogResult = DialogResult.OK},
                    remove As New Button With {.Dock = DockStyle.Bottom, .Text = "Laisser la pièce en attente", .DialogResult = DialogResult.No}
                    dialog.Controls.AddRange(New Control() {list, confirm, remove})
                    Select Case dialog.ShowDialog(Me)
                        Case DialogResult.OK
                            _movement = CInt(list.SelectedValue)
                            debitMouvement = Not CBool(DirectCast(list.SelectedItem, Data.DataRowView)("sens"))
                            ChargerCategories()
                            _attach.Text = "Mouvement n° " & _movement.Value
                        Case DialogResult.No
                            ClearMovement()
                    End Select
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Rapprochement impossible")
        End Try
    End Sub

    Private Sub ValidateFields(sender As Object, e As EventArgs)
        Try
            _proposal = BuildProposal()
            _proposal.MouvementChoisi = _movement
            _proposal.NatureComptable = If(_sousNature.SelectedIndex >= 0, _sousNature.Text, _nature.Text)
            If _nature.SelectedIndex >= 0 Then _proposal.IdCategorie = CInt(_nature.SelectedValue)
            If _sousNature.SelectedIndex >= 0 Then _proposal.IdSousCategorie = CInt(_sousNature.SelectedValue)
            _proposal.EvenementComptable = _eventName.Text.Trim()
            DialogResult = DialogResult.OK
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IO.IOException
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
