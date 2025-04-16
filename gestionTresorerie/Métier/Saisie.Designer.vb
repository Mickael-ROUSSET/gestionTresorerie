<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSaisie
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        lblType = New Label()
        dateMvt = New DateTimePicker()
        lblTiers = New Label()
        lblCategorie = New Label()
        lblSousCategorie = New Label()
        rbCredit = New RadioButton()
        rbDebit = New RadioButton()
        grpSens = New GroupBox()
        btnValider = New Button()
        lblMontant = New Label()
        txtMontant = New TextBox()
        lblEvénement = New Label()
        GroupBox1 = New GroupBox()
        rbCree = New RadioButton()
        rbRapproche = New RadioButton()
        btnInsereTiers = New Button()
        btnHistogramme = New Button()
        lblRemise = New Label()
        txtRemise = New TextBox()
        txtNote = New TextBox()
        lblNote = New Label()
        cbType = New ComboBox()
        cbEvénement = New ComboBox()
        Button1 = New Button()
        dgvTiers = New DataGridView()
        TiersBindingSource = New BindingSource(components)
        MouvementsBindingSource1 = New BindingSource(components)
        MouvementsBindingSource = New BindingSource(components)
        dgvCategorie = New DataGridView()
        dgvSousCategorie = New DataGridView()
        txtRechercheTiers = New TextBox()
        btnSelChq = New Button()
        grpSens.SuspendLayout()
        GroupBox1.SuspendLayout()
        CType(dgvTiers, ComponentModel.ISupportInitialize).BeginInit()
        CType(TiersBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvCategorie, ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvSousCategorie, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblType
        ' 
        lblType.AutoSize = True
        lblType.Location = New Point(698, 300)
        lblType.Name = "lblType"
        lblType.Size = New Size(32, 15)
        lblType.TabIndex = 0
        lblType.Text = "Type"
        ' 
        ' dateMvt
        ' 
        dateMvt.AllowDrop = True
        dateMvt.Location = New Point(17, 33)
        dateMvt.Name = "dateMvt"
        dateMvt.Size = New Size(194, 23)
        dateMvt.TabIndex = 1
        ' 
        ' lblTiers
        ' 
        lblTiers.AutoSize = True
        lblTiers.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblTiers.Location = New Point(17, 134)
        lblTiers.Name = "lblTiers"
        lblTiers.Size = New Size(34, 15)
        lblTiers.TabIndex = 4
        lblTiers.Text = "Tiers"
        ' 
        ' lblCategorie
        ' 
        lblCategorie.AutoSize = True
        lblCategorie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCategorie.Location = New Point(17, 302)
        lblCategorie.Name = "lblCategorie"
        lblCategorie.Size = New Size(61, 15)
        lblCategorie.TabIndex = 5
        lblCategorie.Text = "Catégorie"
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblSousCategorie.Location = New Point(698, 113)
        lblSousCategorie.Name = "lblSousCategorie"
        lblSousCategorie.Size = New Size(91, 15)
        lblSousCategorie.TabIndex = 8
        lblSousCategorie.Text = "Sous-categorie"
        ' 
        ' rbCredit
        ' 
        rbCredit.AutoSize = True
        rbCredit.Location = New Point(91, 22)
        rbCredit.Name = "rbCredit"
        rbCredit.Size = New Size(57, 19)
        rbCredit.TabIndex = 10
        rbCredit.Text = "Crédit"
        rbCredit.UseVisualStyleBackColor = True
        ' 
        ' rbDebit
        ' 
        rbDebit.AutoSize = True
        rbDebit.Checked = True
        rbDebit.Location = New Point(19, 22)
        rbDebit.Name = "rbDebit"
        rbDebit.Size = New Size(53, 19)
        rbDebit.TabIndex = 11
        rbDebit.TabStop = True
        rbDebit.Text = "Débit"
        rbDebit.UseVisualStyleBackColor = True
        ' 
        ' grpSens
        ' 
        grpSens.AutoSize = True
        grpSens.Controls.Add(rbDebit)
        grpSens.Controls.Add(rbCredit)
        grpSens.Location = New Point(45, 65)
        grpSens.Name = "grpSens"
        grpSens.Size = New Size(176, 63)
        grpSens.TabIndex = 12
        grpSens.TabStop = False
        grpSens.Text = "Sens"
        ' 
        ' btnValider
        ' 
        btnValider.AutoSize = True
        btnValider.Location = New Point(832, 450)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(75, 25)
        btnValider.TabIndex = 13
        btnValider.Text = "Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' lblMontant
        ' 
        lblMontant.AutoSize = True
        lblMontant.Location = New Point(256, 33)
        lblMontant.Name = "lblMontant"
        lblMontant.Size = New Size(53, 15)
        lblMontant.TabIndex = 14
        lblMontant.Text = "Montant"
        ' 
        ' txtMontant
        ' 
        txtMontant.Location = New Point(344, 26)
        txtMontant.Name = "txtMontant"
        txtMontant.Size = New Size(128, 23)
        txtMontant.TabIndex = 15
        ' 
        ' lblEvénement
        ' 
        lblEvénement.AutoSize = True
        lblEvénement.Location = New Point(698, 342)
        lblEvénement.Name = "lblEvénement"
        lblEvénement.Size = New Size(66, 15)
        lblEvénement.TabIndex = 17
        lblEvénement.Text = "Evénement"
        ' 
        ' GroupBox1
        ' 
        GroupBox1.AutoSize = True
        GroupBox1.Controls.Add(rbCree)
        GroupBox1.Controls.Add(rbRapproche)
        GroupBox1.Location = New Point(285, 65)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(180, 63)
        GroupBox1.TabIndex = 13
        GroupBox1.TabStop = False
        GroupBox1.Text = "Etat"
        ' 
        ' rbCree
        ' 
        rbCree.AutoSize = True
        rbCree.Checked = True
        rbCree.Location = New Point(12, 22)
        rbCree.Name = "rbCree"
        rbCree.Size = New Size(49, 19)
        rbCree.TabIndex = 11
        rbCree.TabStop = True
        rbCree.Text = "Créé"
        rbCree.UseVisualStyleBackColor = True
        ' 
        ' rbRapproche
        ' 
        rbRapproche.AutoSize = True
        rbRapproche.Location = New Point(84, 22)
        rbRapproche.Name = "rbRapproche"
        rbRapproche.Size = New Size(82, 19)
        rbRapproche.TabIndex = 10
        rbRapproche.Text = "Rapproché"
        rbRapproche.UseVisualStyleBackColor = True
        ' 
        ' btnInsereTiers
        ' 
        btnInsereTiers.Location = New Point(988, 50)
        btnInsereTiers.Name = "btnInsereTiers"
        btnInsereTiers.Size = New Size(90, 23)
        btnInsereTiers.TabIndex = 19
        btnInsereTiers.Text = "Insère un tiers"
        btnInsereTiers.UseVisualStyleBackColor = True
        ' 
        ' btnHistogramme
        ' 
        btnHistogramme.AutoSize = True
        btnHistogramme.Location = New Point(1261, 476)
        btnHistogramme.Name = "btnHistogramme"
        btnHistogramme.Size = New Size(90, 25)
        btnHistogramme.TabIndex = 21
        btnHistogramme.Text = "Histogramme"
        btnHistogramme.UseVisualStyleBackColor = True
        ' 
        ' lblRemise
        ' 
        lblRemise.AutoSize = True
        lblRemise.Location = New Point(698, 380)
        lblRemise.Name = "lblRemise"
        lblRemise.Size = New Size(45, 15)
        lblRemise.TabIndex = 23
        lblRemise.Text = "Remise"
        ' 
        ' txtRemise
        ' 
        txtRemise.Location = New Point(809, 380)
        txtRemise.Name = "txtRemise"
        txtRemise.Size = New Size(328, 23)
        txtRemise.TabIndex = 24
        ' 
        ' txtNote
        ' 
        txtNote.Location = New Point(809, 418)
        txtNote.Name = "txtNote"
        txtNote.Size = New Size(328, 23)
        txtNote.TabIndex = 26
        ' 
        ' lblNote
        ' 
        lblNote.AutoSize = True
        lblNote.Location = New Point(698, 418)
        lblNote.Name = "lblNote"
        lblNote.Size = New Size(33, 15)
        lblNote.TabIndex = 25
        lblNote.Text = "Note"
        ' 
        ' cbType
        ' 
        cbType.DropDownStyle = ComboBoxStyle.DropDownList
        cbType.FormattingEnabled = True
        cbType.Location = New Point(809, 300)
        cbType.Name = "cbType"
        cbType.Size = New Size(328, 23)
        cbType.TabIndex = 27
        ' 
        ' cbEvénement
        ' 
        cbEvénement.FormattingEnabled = True
        cbEvénement.Location = New Point(809, 342)
        cbEvénement.Name = "cbEvénement"
        cbEvénement.Size = New Size(328, 23)
        cbEvénement.TabIndex = 31
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(1261, 507)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 23)
        Button1.TabIndex = 32
        Button1.Text = "Button1"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' dgvTiers
        ' 
        dgvTiers.AllowDrop = True
        dgvTiers.AllowUserToDeleteRows = False
        dgvTiers.AllowUserToOrderColumns = True
        dgvTiers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvTiers.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedHeaders
        dgvTiers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvTiers.GridColor = SystemColors.MenuHighlight
        dgvTiers.Location = New Point(104, 134)
        dgvTiers.MultiSelect = False
        dgvTiers.Name = "dgvTiers"
        dgvTiers.ReadOnly = True
        dgvTiers.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvTiers.Size = New Size(547, 157)
        dgvTiers.TabIndex = 33
        ' 
        ' TiersBindingSource
        ' 
        TiersBindingSource.DataSource = GetType(Tiers)
        ' 
        ' MouvementsBindingSource1
        ' 
        MouvementsBindingSource1.DataSource = GetType(Mouvements)
        ' 
        ' MouvementsBindingSource
        ' 
        MouvementsBindingSource.DataSource = GetType(Mouvements)
        ' 
        ' dgvCategorie
        ' 
        dgvCategorie.AllowDrop = True
        dgvCategorie.AllowUserToOrderColumns = True
        dgvCategorie.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvCategorie.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells
        dgvCategorie.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvCategorie.Location = New Point(104, 302)
        dgvCategorie.MultiSelect = False
        dgvCategorie.Name = "dgvCategorie"
        dgvCategorie.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvCategorie.Size = New Size(547, 199)
        dgvCategorie.TabIndex = 34
        ' 
        ' dgvSousCategorie
        ' 
        dgvSousCategorie.AllowDrop = True
        dgvSousCategorie.AllowUserToOrderColumns = True
        dgvSousCategorie.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvSousCategorie.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells
        dgvSousCategorie.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvSousCategorie.Location = New Point(698, 134)
        dgvSousCategorie.MultiSelect = False
        dgvSousCategorie.Name = "dgvSousCategorie"
        dgvSousCategorie.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvSousCategorie.Size = New Size(547, 88)
        dgvSousCategorie.TabIndex = 35
        ' 
        ' txtRechercheTiers
        ' 
        txtRechercheTiers.Location = New Point(506, 89)
        txtRechercheTiers.Name = "txtRechercheTiers"
        txtRechercheTiers.Size = New Size(100, 23)
        txtRechercheTiers.TabIndex = 36
        ' 
        ' btnSelChq
        ' 
        btnSelChq.Location = New Point(809, 489)
        btnSelChq.Name = "btnSelChq"
        btnSelChq.Size = New Size(168, 23)
        btnSelChq.TabIndex = 38
        btnSelChq.Text = "Sélectionne chèque"
        btnSelChq.UseVisualStyleBackColor = True
        ' 
        ' FrmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1506, 542)
        Controls.Add(btnSelChq)
        Controls.Add(txtRechercheTiers)
        Controls.Add(dgvSousCategorie)
        Controls.Add(dgvCategorie)
        Controls.Add(dgvTiers)
        Controls.Add(Button1)
        Controls.Add(cbEvénement)
        Controls.Add(cbType)
        Controls.Add(txtNote)
        Controls.Add(lblNote)
        Controls.Add(txtRemise)
        Controls.Add(lblRemise)
        Controls.Add(btnHistogramme)
        Controls.Add(btnInsereTiers)
        Controls.Add(GroupBox1)
        Controls.Add(lblEvénement)
        Controls.Add(txtMontant)
        Controls.Add(lblMontant)
        Controls.Add(btnValider)
        Controls.Add(grpSens)
        Controls.Add(lblSousCategorie)
        Controls.Add(lblCategorie)
        Controls.Add(lblTiers)
        Controls.Add(dateMvt)
        Controls.Add(lblType)
        Name = "FrmSaisie"
        Text = "Saisie d'un mouvement"
        grpSens.ResumeLayout(False)
        grpSens.PerformLayout()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        CType(dgvTiers, ComponentModel.ISupportInitialize).EndInit()
        CType(TiersBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(MouvementsBindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(dgvCategorie, ComponentModel.ISupportInitialize).EndInit()
        CType(dgvSousCategorie, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblType As Label
    Friend WithEvents dateMvt As DateTimePicker
    Friend WithEvents lblTiers As Label
    Friend WithEvents lblCategorie As Label
    Friend WithEvents lblSousCategorie As Label
    Friend WithEvents rbCredit As RadioButton
    Friend WithEvents rbDebit As RadioButton
    Friend WithEvents grpSens As GroupBox
    Friend WithEvents btnValider As Button
    Friend WithEvents lblMontant As Label
    Friend WithEvents txtMontant As TextBox
    Friend WithEvents lblEvénement As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents rbCree As RadioButton
    Friend WithEvents rbRapproche As RadioButton
    Friend WithEvents btnInsereTiers As Button
    Friend WithEvents btnHistogramme As Button
    Friend WithEvents lblRemise As Label
    Friend WithEvents txtRemise As TextBox
    Friend WithEvents txtNote As TextBox
    Friend WithEvents lblNote As Label
    Friend WithEvents cbType As ComboBox
    Friend WithEvents cbEvénement As ComboBox
    Friend WithEvents Button1 As Button
    Friend WithEvents dgvTiers As DataGridView
    Friend WithEvents MouvementsBindingSource1 As BindingSource
    Friend WithEvents MouvementsBindingSource As BindingSource
    Friend WithEvents dgvCategorie As DataGridView
    Friend WithEvents dgvSousCategorie As DataGridView
    Friend WithEvents TiersBindingSource As BindingSource
    Friend WithEvents txtRechercheTiers As TextBox
    Friend WithEvents btnSelChq As Button

End Class
