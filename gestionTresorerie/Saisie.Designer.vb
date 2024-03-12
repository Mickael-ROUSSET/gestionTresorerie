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
        lstType = New ListBox()
        lstTiers = New ListBox()
        lblTiers = New Label()
        lblCategorie = New Label()
        lstCategorie = New ListBox()
        lstSousCategorie = New ListBox()
        lblSousCategorie = New Label()
        rbCredit = New RadioButton()
        rbDebit = New RadioButton()
        grpSens = New GroupBox()
        btnValider = New Button()
        lblMontant = New Label()
        txtMontant = New TextBox()
        lblEvénement = New Label()
        lstEvénement = New ListBox()
        GroupBox1 = New GroupBox()
        rbCree = New RadioButton()
        rbRapproche = New RadioButton()
        btnOuvreFichier = New Button()
        btnDessin = New Button()
        picGraph1 = New PictureBox()
        btnHistogramme = New Button()
        lblRemise = New Label()
        txtRemise = New TextBox()
        txtNote = New TextBox()
        lblNote = New Label()
        grpSens.SuspendLayout()
        GroupBox1.SuspendLayout()
        CType(picGraph1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblType
        ' 
        lblType.AutoSize = True
        lblType.Location = New Point(12, 141)
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
        ' lstType
        ' 
        lstType.FormattingEnabled = True
        lstType.ItemHeight = 15
        lstType.Location = New Point(123, 136)
        lstType.Name = "lstType"
        lstType.Size = New Size(328, 34)
        lstType.TabIndex = 2
        ' 
        ' lstTiers
        ' 
        lstTiers.FormattingEnabled = True
        lstTiers.ItemHeight = 15
        lstTiers.Location = New Point(123, 181)
        lstTiers.Name = "lstTiers"
        lstTiers.Size = New Size(328, 34)
        lstTiers.TabIndex = 3
        ' 
        ' lblTiers
        ' 
        lblTiers.AutoSize = True
        lblTiers.Location = New Point(12, 184)
        lblTiers.Name = "lblTiers"
        lblTiers.Size = New Size(31, 15)
        lblTiers.TabIndex = 4
        lblTiers.Text = "Tiers"
        ' 
        ' lblCategorie
        ' 
        lblCategorie.AutoSize = True
        lblCategorie.Location = New Point(12, 227)
        lblCategorie.Name = "lblCategorie"
        lblCategorie.Size = New Size(58, 15)
        lblCategorie.TabIndex = 5
        lblCategorie.Text = "Catégorie"
        ' 
        ' lstCategorie
        ' 
        lstCategorie.AllowDrop = True
        lstCategorie.FormattingEnabled = True
        lstCategorie.ItemHeight = 15
        lstCategorie.Location = New Point(123, 226)
        lstCategorie.Name = "lstCategorie"
        lstCategorie.Size = New Size(328, 34)
        lstCategorie.TabIndex = 6
        ' 
        ' lstSousCategorie
        ' 
        lstSousCategorie.AllowDrop = True
        lstSousCategorie.FormattingEnabled = True
        lstSousCategorie.ItemHeight = 15
        lstSousCategorie.Location = New Point(123, 271)
        lstSousCategorie.Name = "lstSousCategorie"
        lstSousCategorie.Size = New Size(328, 34)
        lstSousCategorie.TabIndex = 7
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Location = New Point(12, 270)
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
        lblEvénement.Location = New Point(12, 311)
        lblEvénement.Name = "lblEvénement"
        lblEvénement.Size = New Size(66, 15)
        lblEvénement.TabIndex = 17
        lblEvénement.Text = "Evénement"
        ' 
        ' lstEvénement
        ' 
        lstEvénement.AllowDrop = True
        lstEvénement.FormattingEnabled = True
        lstEvénement.ItemHeight = 15
        lstEvénement.Location = New Point(123, 311)
        lstEvénement.Name = "lstEvénement"
        lstEvénement.Size = New Size(328, 34)
        lstEvénement.TabIndex = 16
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
        ' btnOuvreFichier
        ' 
        btnOuvreFichier.Location = New Point(426, 420)
        btnOuvreFichier.Name = "btnOuvreFichier"
        btnOuvreFichier.Size = New Size(75, 23)
        btnOuvreFichier.TabIndex = 18
        btnOuvreFichier.Text = "Ouvrir"
        btnOuvreFichier.UseVisualStyleBackColor = True
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
        lblRemise.Location = New Point(12, 351)
        lblRemise.Name = "lblRemise"
        lblRemise.Size = New Size(45, 15)
        lblRemise.TabIndex = 23
        lblRemise.Text = "Remise"
        ' 
        ' txtRemise
        ' 
        txtRemise.Location = New Point(123, 351)
        txtRemise.Name = "txtRemise"
        txtRemise.Size = New Size(328, 23)
        txtRemise.TabIndex = 24
        ' 
        ' txtNote
        ' 
        txtNote.Location = New Point(123, 389)
        txtNote.Name = "txtNote"
        txtNote.Size = New Size(328, 23)
        txtNote.TabIndex = 26
        ' 
        ' lblNote
        ' 
        lblNote.AutoSize = True
        lblNote.Location = New Point(12, 389)
        lblNote.Name = "lblNote"
        lblNote.Size = New Size(33, 15)
        lblNote.TabIndex = 25
        lblNote.Text = "Note"
        ' 
        ' FrmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(814, 495)
        Controls.Add(txtNote)
        Controls.Add(lblNote)
        Controls.Add(txtRemise)
        Controls.Add(lblRemise)
        Controls.Add(btnHistogramme)
        Controls.Add(picGraph1)
        Controls.Add(btnDessin)
        Controls.Add(btnOuvreFichier)
        Controls.Add(GroupBox1)
        Controls.Add(lblEvénement)
        Controls.Add(lstEvénement)
        Controls.Add(txtMontant)
        Controls.Add(lblMontant)
        Controls.Add(btnValider)
        Controls.Add(grpSens)
        Controls.Add(lblSousCategorie)
        Controls.Add(lstSousCategorie)
        Controls.Add(lstCategorie)
        Controls.Add(lblCategorie)
        Controls.Add(lblTiers)
        Controls.Add(lstTiers)
        Controls.Add(lstType)
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
    Friend WithEvents lstType As ListBox
    Friend WithEvents lstTiers As ListBox
    Friend WithEvents lblTiers As Label
    Friend WithEvents lblCategorie As Label
    Friend WithEvents lstCategorie As ListBox
    Friend WithEvents lstSousCategorie As ListBox
    Friend WithEvents lblSousCategorie As Label
    Friend WithEvents rbCredit As RadioButton
    Friend WithEvents rbDebit As RadioButton
    Friend WithEvents grpSens As GroupBox
    Friend WithEvents btnValider As Button
    Friend WithEvents lblMontant As Label
    Friend WithEvents txtMontant As TextBox
    Friend WithEvents lblEvénement As Label
    Friend WithEvents lstEvénement As ListBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents rbCree As RadioButton
    Friend WithEvents rbRapproche As RadioButton
    Friend WithEvents btnOuvreFichier As Button
    Friend WithEvents btnDessin As Button
    Friend WithEvents picGraph1 As PictureBox
    Friend WithEvents btnHistogramme As Button
    Friend WithEvents lblRemise As Label
    Friend WithEvents txtRemise As TextBox
    Friend WithEvents txtNote As TextBox
    Friend WithEvents lblNote As Label

End Class
