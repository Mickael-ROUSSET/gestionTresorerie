<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmReglesAffectationReleve
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        dgvRegles = New DataGridView()
        txtMotCle = New TextBox()
        lblMotCle = New Label()
        txtIdTiers = New TextBox()
        lblIdTiers = New Label()
        txtIdCategorie = New TextBox()
        lblIdCategorie = New Label()
        txtIdSousCategorie = New TextBox()
        lblIdSousCategorie = New Label()
        cboSens = New ComboBox()
        lblSens = New Label()
        numPriorite = New NumericUpDown()
        lblPriorite = New Label()
        chkActif = New CheckBox()
        lblActif = New Label()
        btnNouveau = New Button()
        btnEnregistrer = New Button()
        btnSupprimer = New Button()
        btnFermer = New Button()
        CType(dgvRegles, ComponentModel.ISupportInitialize).BeginInit()
        CType(numPriorite, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' dgvRegles
        ' 
        dgvRegles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvRegles.Location = New Point(78, 29)
        dgvRegles.Name = "dgvRegles"
        dgvRegles.Size = New Size(621, 150)
        dgvRegles.TabIndex = 0
        ' 
        ' txtMotCle
        ' 
        txtMotCle.Location = New Point(290, 198)
        txtMotCle.Name = "txtMotCle"
        txtMotCle.Size = New Size(100, 23)
        txtMotCle.TabIndex = 1
        ' 
        ' lblMotCle
        ' 
        lblMotCle.AutoSize = True
        lblMotCle.Location = New Point(85, 198)
        lblMotCle.Name = "lblMotCle"
        lblMotCle.Size = New Size(59, 15)
        lblMotCle.TabIndex = 2
        lblMotCle.Text = "lblMotCle"
        ' 
        ' txtIdTiers
        ' 
        txtIdTiers.Location = New Point(290, 239)
        txtIdTiers.Name = "txtIdTiers"
        txtIdTiers.Size = New Size(100, 23)
        txtIdTiers.TabIndex = 3
        ' 
        ' lblIdTiers
        ' 
        lblIdTiers.AutoSize = True
        lblIdTiers.Location = New Point(77, 241)
        lblIdTiers.Name = "lblIdTiers"
        lblIdTiers.Size = New Size(55, 15)
        lblIdTiers.TabIndex = 4
        lblIdTiers.Text = "lblIdTiers"
        ' 
        ' txtIdCategorie
        ' 
        txtIdCategorie.Location = New Point(290, 280)
        txtIdCategorie.Name = "txtIdCategorie"
        txtIdCategorie.Size = New Size(100, 23)
        txtIdCategorie.TabIndex = 5
        ' 
        ' lblIdCategorie
        ' 
        lblIdCategorie.AutoSize = True
        lblIdCategorie.Location = New Point(83, 283)
        lblIdCategorie.Name = "lblIdCategorie"
        lblIdCategorie.Size = New Size(81, 15)
        lblIdCategorie.TabIndex = 6
        lblIdCategorie.Text = "lblIdCategorie"
        ' 
        ' txtIdSousCategorie
        ' 
        txtIdSousCategorie.Location = New Point(295, 315)
        txtIdSousCategorie.Name = "txtIdSousCategorie"
        txtIdSousCategorie.Size = New Size(100, 23)
        txtIdSousCategorie.TabIndex = 7
        ' 
        ' lblIdSousCategorie
        ' 
        lblIdSousCategorie.AutoSize = True
        lblIdSousCategorie.Location = New Point(94, 316)
        lblIdSousCategorie.Name = "lblIdSousCategorie"
        lblIdSousCategorie.Size = New Size(106, 15)
        lblIdSousCategorie.TabIndex = 8
        lblIdSousCategorie.Text = "lblIdSousCategorie"
        ' 
        ' cboSens
        ' 
        cboSens.FormattingEnabled = True
        cboSens.Items.AddRange(New Object() {"Tous", "Débit", "Crédit"})
        cboSens.Location = New Point(290, 348)
        cboSens.Name = "cboSens"
        cboSens.Size = New Size(121, 23)
        cboSens.TabIndex = 9
        cboSens.ValueMember = "Tous"
        ' 
        ' lblSens
        ' 
        lblSens.AutoSize = True
        lblSens.Location = New Point(78, 348)
        lblSens.Name = "lblSens"
        lblSens.Size = New Size(44, 15)
        lblSens.TabIndex = 10
        lblSens.Text = "lblSens"
        ' 
        ' numPriorite
        ' 
        numPriorite.Location = New Point(290, 383)
        numPriorite.Name = "numPriorite"
        numPriorite.Size = New Size(120, 23)
        numPriorite.TabIndex = 11
        ' 
        ' lblPriorite
        ' 
        lblPriorite.AutoSize = True
        lblPriorite.Location = New Point(82, 385)
        lblPriorite.Name = "lblPriorite"
        lblPriorite.Size = New Size(45, 15)
        lblPriorite.TabIndex = 12
        lblPriorite.Text = "Priorite"
        ' 
        ' chkActif
        ' 
        chkActif.AutoSize = True
        chkActif.Location = New Point(177, 419)
        chkActif.Name = "chkActif"
        chkActif.Size = New Size(84, 19)
        chkActif.TabIndex = 13
        chkActif.Text = "CheckBox1"
        chkActif.UseVisualStyleBackColor = True
        ' 
        ' lblActif
        ' 
        lblActif.AutoSize = True
        lblActif.Location = New Point(82, 423)
        lblActif.Name = "lblActif"
        lblActif.Size = New Size(32, 15)
        lblActif.TabIndex = 14
        lblActif.Text = "Actif"
        ' 
        ' btnNouveau
        ' 
        btnNouveau.Location = New Point(85, 466)
        btnNouveau.Name = "btnNouveau"
        btnNouveau.Size = New Size(75, 23)
        btnNouveau.TabIndex = 15
        btnNouveau.Text = "Nouveau"
        btnNouveau.UseVisualStyleBackColor = True
        ' 
        ' btnEnregistrer
        ' 
        btnEnregistrer.Location = New Point(223, 477)
        btnEnregistrer.Name = "btnEnregistrer"
        btnEnregistrer.Size = New Size(75, 23)
        btnEnregistrer.TabIndex = 16
        btnEnregistrer.Text = "Enregistrer"
        btnEnregistrer.UseVisualStyleBackColor = True
        ' 
        ' btnSupprimer
        ' 
        btnSupprimer.Location = New Point(364, 473)
        btnSupprimer.Name = "btnSupprimer"
        btnSupprimer.Size = New Size(75, 23)
        btnSupprimer.TabIndex = 17
        btnSupprimer.Text = "Supprimer"
        btnSupprimer.UseVisualStyleBackColor = True
        ' 
        ' btnFermer
        ' 
        btnFermer.Location = New Point(482, 473)
        btnFermer.Name = "btnFermer"
        btnFermer.Size = New Size(75, 23)
        btnFermer.TabIndex = 18
        btnFermer.Text = "Fermer"
        btnFermer.UseVisualStyleBackColor = True
        ' 
        ' frmReglesAffectationReleve
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 531)
        Controls.Add(btnFermer)
        Controls.Add(btnSupprimer)
        Controls.Add(btnEnregistrer)
        Controls.Add(btnNouveau)
        Controls.Add(lblActif)
        Controls.Add(chkActif)
        Controls.Add(lblPriorite)
        Controls.Add(numPriorite)
        Controls.Add(lblSens)
        Controls.Add(cboSens)
        Controls.Add(lblIdSousCategorie)
        Controls.Add(txtIdSousCategorie)
        Controls.Add(lblIdCategorie)
        Controls.Add(txtIdCategorie)
        Controls.Add(lblIdTiers)
        Controls.Add(txtIdTiers)
        Controls.Add(lblMotCle)
        Controls.Add(txtMotCle)
        Controls.Add(dgvRegles)
        Name = "frmReglesAffectationReleve"
        Text = "frmReglesAffectationReleve"
        CType(dgvRegles, ComponentModel.ISupportInitialize).EndInit()
        CType(numPriorite, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents dgvRegles As DataGridView
    Friend WithEvents txtMotCle As TextBox
    Friend WithEvents lblMotCle As Label
    Public WithEvents txtIdTiers As TextBox
    Friend WithEvents lblIdTiers As Label
    Friend WithEvents txtIdCategorie As TextBox
    Friend WithEvents lblIdCategorie As Label
    Friend WithEvents txtIdSousCategorie As TextBox
    Friend WithEvents lblIdSousCategorie As Label
    Friend WithEvents cboSens As ComboBox
    Friend WithEvents lblSens As Label
    Friend WithEvents numPriorite As NumericUpDown
    Friend WithEvents lblPriorite As Label
    Friend WithEvents chkActif As CheckBox
    Friend WithEvents lblActif As Label
    Friend WithEvents btnNouveau As Button
    Friend WithEvents btnEnregistrer As Button
    Friend WithEvents btnSupprimer As Button
    Friend WithEvents btnFermer As Button
End Class
