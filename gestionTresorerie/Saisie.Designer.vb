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
        btnDessin = New Button()
        picGraph1 = New PictureBox()
        btnHistogramme = New Button()
        lblRemise = New Label()
        txtRemise = New TextBox()
        txtNote = New TextBox()
        lblNote = New Label()
        cbType = New ComboBox()
        cbTiers = New ComboBox()
        cbCategorie = New ComboBox()
        cbSousCategorie = New ComboBox()
        cbEvénement = New ComboBox()
        grpSens.SuspendLayout()
        GroupBox1.SuspendLayout()
        CType(picGraph1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblType
        ' 
        lblType.AutoSize = True
        lblType.Location = New Point(12, 158)
        lblType.Name = "lblType"
        lblType.Size = New Size(31, 15)
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
        lblTiers.Location = New Point(12, 196)
        lblTiers.Name = "lblTiers"
        lblTiers.Size = New Size(31, 15)
        lblTiers.TabIndex = 4
        lblTiers.Text = "Tiers"
        ' 
        ' lblCategorie
        ' 
        lblCategorie.AutoSize = True
        lblCategorie.Location = New Point(12, 234)
        lblCategorie.Name = "lblCategorie"
        lblCategorie.Size = New Size(58, 15)
        lblCategorie.TabIndex = 5
        lblCategorie.Text = "Catégorie"
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Location = New Point(12, 272)
        lblSousCategorie.Name = "lblSousCategorie"
        lblSousCategorie.Size = New Size(86, 15)
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
        btnValider.Location = New Point(146, 418)
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
        lblEvénement.Location = New Point(12, 310)
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
        ' btnDessin
        ' 
        btnDessin.Location = New Point(632, 428)
        btnDessin.Name = "btnDessin"
        btnDessin.Size = New Size(75, 23)
        btnDessin.TabIndex = 19
        btnDessin.Text = "Dessin"
        btnDessin.UseVisualStyleBackColor = True
        ' 
        ' picGraph1
        ' 
        picGraph1.Location = New Point(522, 141)
        picGraph1.Name = "picGraph1"
        picGraph1.Size = New Size(200, 145)
        picGraph1.TabIndex = 20
        picGraph1.TabStop = False
        ' 
        ' btnHistogramme
        ' 
        btnHistogramme.AutoSize = True
        btnHistogramme.Location = New Point(632, 322)
        btnHistogramme.Name = "btnHistogramme"
        btnHistogramme.Size = New Size(90, 25)
        btnHistogramme.TabIndex = 21
        btnHistogramme.Text = "Histogramme"
        btnHistogramme.UseVisualStyleBackColor = True
        ' 
        ' lblRemise
        ' 
        lblRemise.AutoSize = True
        lblRemise.Location = New Point(12, 348)
        lblRemise.Name = "lblRemise"
        lblRemise.Size = New Size(45, 15)
        lblRemise.TabIndex = 23
        lblRemise.Text = "Remise"
        ' 
        ' txtRemise
        ' 
        txtRemise.Location = New Point(123, 348)
        txtRemise.Name = "txtRemise"
        txtRemise.Size = New Size(328, 23)
        txtRemise.TabIndex = 24
        ' 
        ' txtNote
        ' 
        txtNote.Location = New Point(123, 386)
        txtNote.Name = "txtNote"
        txtNote.Size = New Size(328, 23)
        txtNote.TabIndex = 26
        ' 
        ' lblNote
        ' 
        lblNote.AutoSize = True
        lblNote.Location = New Point(12, 386)
        lblNote.Name = "lblNote"
        lblNote.Size = New Size(33, 15)
        lblNote.TabIndex = 25
        lblNote.Text = "Note"
        ' 
        ' cbType
        ' 
        cbType.DropDownStyle = ComboBoxStyle.DropDownList
        cbType.FormattingEnabled = True
        cbType.Location = New Point(123, 158)
        cbType.Name = "cbType"
        cbType.Size = New Size(328, 23)
        cbType.TabIndex = 27
        ' 
        ' cbTiers
        ' 
        cbTiers.FormattingEnabled = True
        cbTiers.Location = New Point(123, 196)
        cbTiers.Name = "cbTiers"
        cbTiers.Size = New Size(328, 23)
        cbTiers.TabIndex = 28
        ' 
        ' cbCategorie
        ' 
        cbCategorie.FormattingEnabled = True
        cbCategorie.Location = New Point(123, 234)
        cbCategorie.Name = "cbCategorie"
        cbCategorie.Size = New Size(328, 23)
        cbCategorie.TabIndex = 29
        ' 
        ' cbSousCategorie
        ' 
        cbSousCategorie.FormattingEnabled = True
        cbSousCategorie.Location = New Point(123, 272)
        cbSousCategorie.Name = "cbSousCategorie"
        cbSousCategorie.Size = New Size(328, 23)
        cbSousCategorie.TabIndex = 30
        ' 
        ' cbEvénement
        ' 
        cbEvénement.FormattingEnabled = True
        cbEvénement.Location = New Point(123, 310)
        cbEvénement.Name = "cbEvénement"
        cbEvénement.Size = New Size(328, 23)
        cbEvénement.TabIndex = 31
        ' 
        ' FrmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(873, 494)
        Controls.Add(cbEvénement)
        Controls.Add(cbSousCategorie)
        Controls.Add(cbCategorie)
        Controls.Add(cbTiers)
        Controls.Add(cbType)
        Controls.Add(txtNote)
        Controls.Add(lblNote)
        Controls.Add(txtRemise)
        Controls.Add(lblRemise)
        Controls.Add(btnHistogramme)
        Controls.Add(picGraph1)
        Controls.Add(btnDessin)
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
        CType(picGraph1, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents btnDessin As Button
    Friend WithEvents picGraph1 As PictureBox
    Friend WithEvents btnHistogramme As Button
    Friend WithEvents lblRemise As Label
    Friend WithEvents txtRemise As TextBox
    Friend WithEvents txtNote As TextBox
    Friend WithEvents lblNote As Label
    Friend WithEvents cbType As ComboBox
    Friend WithEvents cbTiers As ComboBox
    Friend WithEvents cbCategorie As ComboBox
    Friend WithEvents cbSousCategorie As ComboBox
    Friend WithEvents cbEvénement As ComboBox

End Class
