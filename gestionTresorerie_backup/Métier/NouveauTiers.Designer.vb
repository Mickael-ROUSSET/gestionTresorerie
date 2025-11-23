<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmNouveauTiers
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
        lblNom = New Label()
        rbPersonneMorale = New RadioButton()
        grpPersonne = New GroupBox()
        rbPersonnePhysique = New RadioButton()
        txtNom = New TextBox()
        txtPrenom = New TextBox()
        lblPrenom = New Label()
        txtRaisonSociale = New TextBox()
        lblRaisonSociale = New Label()
        txtCategorie = New TextBox()
        lblCatégorie = New Label()
        txtSousCategorie = New TextBox()
        lblSousCategorie = New Label()
        btnCreerTiers = New Button()
        dgvNTCategorie = New DataGridView()
        dgvNTSousCategorie = New DataGridView()
        grpPersonne.SuspendLayout()
        CType(dgvNTCategorie, ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvNTSousCategorie, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblNom
        ' 
        lblNom.AutoSize = True
        lblNom.Location = New Point(45, 109)
        lblNom.Name = "lblNom"
        lblNom.Size = New Size(34, 15)
        lblNom.TabIndex = 0
        lblNom.Text = "Nom"
        ' 
        ' rbPersonneMorale
        ' 
        rbPersonneMorale.AutoSize = True
        rbPersonneMorale.Location = New Point(26, 22)
        rbPersonneMorale.Name = "rbPersonneMorale"
        rbPersonneMorale.Size = New Size(114, 19)
        rbPersonneMorale.TabIndex = 1
        rbPersonneMorale.TabStop = True
        rbPersonneMorale.Text = "Personne morale"
        rbPersonneMorale.UseVisualStyleBackColor = True
        ' 
        ' grpPersonne
        ' 
        grpPersonne.Controls.Add(rbPersonnePhysique)
        grpPersonne.Controls.Add(rbPersonneMorale)
        grpPersonne.Location = New Point(45, 35)
        grpPersonne.Name = "grpPersonne"
        grpPersonne.Size = New Size(418, 60)
        grpPersonne.TabIndex = 2
        grpPersonne.TabStop = False
        ' 
        ' rbPersonnePhysique
        ' 
        rbPersonnePhysique.AutoSize = True
        rbPersonnePhysique.Location = New Point(229, 22)
        rbPersonnePhysique.Name = "rbPersonnePhysique"
        rbPersonnePhysique.Size = New Size(125, 19)
        rbPersonnePhysique.TabIndex = 2
        rbPersonnePhysique.TabStop = True
        rbPersonnePhysique.Text = "Personne physique"
        rbPersonnePhysique.UseVisualStyleBackColor = True
        ' 
        ' txtNom
        ' 
        txtNom.Location = New Point(227, 106)
        txtNom.Name = "txtNom"
        txtNom.Size = New Size(100, 23)
        txtNom.TabIndex = 3
        ' 
        ' txtPrenom
        ' 
        txtPrenom.Location = New Point(227, 154)
        txtPrenom.Name = "txtPrenom"
        txtPrenom.Size = New Size(100, 23)
        txtPrenom.TabIndex = 5
        ' 
        ' lblPrenom
        ' 
        lblPrenom.AutoSize = True
        lblPrenom.Location = New Point(45, 154)
        lblPrenom.Name = "lblPrenom"
        lblPrenom.Size = New Size(49, 15)
        lblPrenom.TabIndex = 4
        lblPrenom.Text = "Prenom"
        ' 
        ' txtRaisonSociale
        ' 
        txtRaisonSociale.Location = New Point(227, 204)
        txtRaisonSociale.Name = "txtRaisonSociale"
        txtRaisonSociale.Size = New Size(100, 23)
        txtRaisonSociale.TabIndex = 7
        ' 
        ' lblRaisonSociale
        ' 
        lblRaisonSociale.AutoSize = True
        lblRaisonSociale.Location = New Point(45, 207)
        lblRaisonSociale.Name = "lblRaisonSociale"
        lblRaisonSociale.Size = New Size(81, 15)
        lblRaisonSociale.TabIndex = 6
        lblRaisonSociale.Text = "Raison sociale"
        ' 
        ' txtCategorie
        ' 
        txtCategorie.Location = New Point(227, 257)
        txtCategorie.Name = "txtCategorie"
        txtCategorie.Size = New Size(100, 23)
        txtCategorie.TabIndex = 9
        ' 
        ' lblCatégorie
        ' 
        lblCatégorie.AutoSize = True
        lblCatégorie.Location = New Point(45, 260)
        lblCatégorie.Name = "lblCatégorie"
        lblCatégorie.Size = New Size(58, 15)
        lblCatégorie.TabIndex = 8
        lblCatégorie.Text = "Catégorie"
        ' 
        ' txtSousCategorie
        ' 
        txtSousCategorie.Location = New Point(227, 463)
        txtSousCategorie.Name = "txtSousCategorie"
        txtSousCategorie.Size = New Size(100, 23)
        txtSousCategorie.TabIndex = 11
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Location = New Point(45, 466)
        lblSousCategorie.Name = "lblSousCategorie"
        lblSousCategorie.Size = New Size(83, 15)
        lblSousCategorie.TabIndex = 10
        lblSousCategorie.Text = "SousCategorie"
        ' 
        ' btnCreerTiers
        ' 
        btnCreerTiers.Location = New Point(56, 527)
        btnCreerTiers.Name = "btnCreerTiers"
        btnCreerTiers.Size = New Size(75, 23)
        btnCreerTiers.TabIndex = 12
        btnCreerTiers.Text = "Créer Tiers"
        btnCreerTiers.UseVisualStyleBackColor = True
        ' 
        ' dgvNTCategorie
        ' 
        dgvNTCategorie.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvNTCategorie.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvNTCategorie.Location = New Point(358, 258)
        dgvNTCategorie.Name = "dgvNTCategorie"
        dgvNTCategorie.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvNTCategorie.Size = New Size(402, 150)
        dgvNTCategorie.TabIndex = 13
        ' 
        ' dgvNTSousCategorie
        ' 
        dgvNTSousCategorie.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvNTSousCategorie.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvNTSousCategorie.Location = New Point(358, 463)
        dgvNTSousCategorie.Name = "dgvNTSousCategorie"
        dgvNTSousCategorie.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvNTSousCategorie.Size = New Size(402, 150)
        dgvNTSousCategorie.TabIndex = 14
        ' 
        ' frmNouveauTiers
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1001, 717)
        Controls.Add(dgvNTSousCategorie)
        Controls.Add(dgvNTCategorie)
        Controls.Add(btnCreerTiers)
        Controls.Add(txtSousCategorie)
        Controls.Add(lblSousCategorie)
        Controls.Add(txtCategorie)
        Controls.Add(lblCatégorie)
        Controls.Add(txtRaisonSociale)
        Controls.Add(lblRaisonSociale)
        Controls.Add(txtPrenom)
        Controls.Add(lblPrenom)
        Controls.Add(txtNom)
        Controls.Add(grpPersonne)
        Controls.Add(lblNom)
        Name = "frmNouveauTiers"
        Text = "Nouveau Tiers"
        grpPersonne.ResumeLayout(False)
        grpPersonne.PerformLayout()
        CType(dgvNTCategorie, ComponentModel.ISupportInitialize).EndInit()
        CType(dgvNTSousCategorie, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblNom As Label
    Friend WithEvents rbPersonneMorale As RadioButton
    Friend WithEvents grpPersonne As GroupBox
    Friend WithEvents rbPersonnePhysique As RadioButton
    Friend WithEvents txtNom As TextBox
    Friend WithEvents txtPrenom As TextBox
    Friend WithEvents lblPrenom As Label
    Friend WithEvents txtRaisonSociale As TextBox
    Friend WithEvents lblRaisonSociale As Label
    Friend WithEvents txtCategorie As TextBox
    Friend WithEvents lblCatégorie As Label
    Friend WithEvents txtSousCategorie As TextBox
    Friend WithEvents lblSousCategorie As Label
    Friend WithEvents btnCreerTiers As Button
    Friend WithEvents dgvNTCategorie As DataGridView
    Friend WithEvents dgvNTSousCategorie As DataGridView
End Class
