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
        components = New ComponentModel.Container()
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
        lblSousCategorie = New Label()
        btnCreerTiers = New Button()
        lblCoordonnees = New Label()
        btnCoordonnees = New Button()
        btnCategorie = New Button()
        btnSousCategorie = New Button()
        txtSousCategorie = New TextBox()
        lblDateNaissance = New Label()
        lblLieuNaissance = New Label()
        txtDateNaissance = New TextBox()
        txLieuNaissance = New TextBox()
        dtpDateNaissance = New DateTimePicker()
        ttCoordonnes = New ToolTip(components)
        grpPersonne.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblNom
        ' 
        lblNom.AutoSize = True
        lblNom.Location = New Point(52, 109)
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
        txtNom.Location = New Point(167, 106)
        txtNom.Name = "txtNom"
        txtNom.Size = New Size(296, 23)
        txtNom.TabIndex = 3
        ' 
        ' txtPrenom
        ' 
        txtPrenom.Location = New Point(167, 154)
        txtPrenom.Name = "txtPrenom"
        txtPrenom.Size = New Size(296, 23)
        txtPrenom.TabIndex = 5
        ' 
        ' lblPrenom
        ' 
        lblPrenom.AutoSize = True
        lblPrenom.Location = New Point(52, 154)
        lblPrenom.Name = "lblPrenom"
        lblPrenom.Size = New Size(49, 15)
        lblPrenom.TabIndex = 4
        lblPrenom.Text = "Prenom"
        ' 
        ' txtRaisonSociale
        ' 
        txtRaisonSociale.Location = New Point(167, 276)
        txtRaisonSociale.Name = "txtRaisonSociale"
        txtRaisonSociale.Size = New Size(296, 23)
        txtRaisonSociale.TabIndex = 7
        ' 
        ' lblRaisonSociale
        ' 
        lblRaisonSociale.AutoSize = True
        lblRaisonSociale.Location = New Point(52, 279)
        lblRaisonSociale.Name = "lblRaisonSociale"
        lblRaisonSociale.Size = New Size(81, 15)
        lblRaisonSociale.TabIndex = 6
        lblRaisonSociale.Text = "Raison sociale"
        ' 
        ' txtCategorie
        ' 
        txtCategorie.Location = New Point(167, 363)
        txtCategorie.Name = "txtCategorie"
        txtCategorie.Size = New Size(296, 23)
        txtCategorie.TabIndex = 9
        ' 
        ' lblCatégorie
        ' 
        lblCatégorie.AutoSize = True
        lblCatégorie.Location = New Point(52, 366)
        lblCatégorie.Name = "lblCatégorie"
        lblCatégorie.Size = New Size(58, 15)
        lblCatégorie.TabIndex = 8
        lblCatégorie.Text = "Catégorie"
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Location = New Point(52, 415)
        lblSousCategorie.Name = "lblSousCategorie"
        lblSousCategorie.Size = New Size(83, 15)
        lblSousCategorie.TabIndex = 10
        lblSousCategorie.Text = "SousCategorie"
        ' 
        ' btnCreerTiers
        ' 
        btnCreerTiers.Location = New Point(52, 485)
        btnCreerTiers.Name = "btnCreerTiers"
        btnCreerTiers.Size = New Size(75, 23)
        btnCreerTiers.TabIndex = 12
        btnCreerTiers.Text = "Créer Tiers"
        btnCreerTiers.UseVisualStyleBackColor = True
        ' 
        ' lblCoordonnees
        ' 
        lblCoordonnees.AutoSize = True
        lblCoordonnees.Location = New Point(52, 324)
        lblCoordonnees.Name = "lblCoordonnees"
        lblCoordonnees.Size = New Size(91, 15)
        lblCoordonnees.TabIndex = 20
        lblCoordonnees.Text = "lblCoordonnées"
        ' 
        ' btnCoordonnees
        ' 
        btnCoordonnees.Location = New Point(167, 319)
        btnCoordonnees.Name = "btnCoordonnees"
        btnCoordonnees.Size = New Size(46, 23)
        btnCoordonnees.TabIndex = 21
        btnCoordonnees.Text = "..."
        btnCoordonnees.UseVisualStyleBackColor = True
        ' 
        ' btnCategorie
        ' 
        btnCategorie.Location = New Point(495, 363)
        btnCategorie.Name = "btnCategorie"
        btnCategorie.Size = New Size(39, 23)
        btnCategorie.TabIndex = 23
        btnCategorie.Text = "..."
        btnCategorie.UseVisualStyleBackColor = True
        ' 
        ' btnSousCategorie
        ' 
        btnSousCategorie.Location = New Point(495, 411)
        btnSousCategorie.Name = "btnSousCategorie"
        btnSousCategorie.Size = New Size(39, 23)
        btnSousCategorie.TabIndex = 24
        btnSousCategorie.Text = "..."
        btnSousCategorie.UseVisualStyleBackColor = True
        ' 
        ' txtSousCategorie
        ' 
        txtSousCategorie.Location = New Point(167, 416)
        txtSousCategorie.Name = "txtSousCategorie"
        txtSousCategorie.Size = New Size(296, 23)
        txtSousCategorie.TabIndex = 25
        ' 
        ' lblDateNaissance
        ' 
        lblDateNaissance.AutoSize = True
        lblDateNaissance.Location = New Point(54, 202)
        lblDateNaissance.Name = "lblDateNaissance"
        lblDateNaissance.Size = New Size(101, 15)
        lblDateNaissance.TabIndex = 26
        lblDateNaissance.Text = "Date de naissance"
        ' 
        ' lblLieuNaissance
        ' 
        lblLieuNaissance.AutoSize = True
        lblLieuNaissance.Location = New Point(56, 241)
        lblLieuNaissance.Name = "lblLieuNaissance"
        lblLieuNaissance.Size = New Size(99, 15)
        lblLieuNaissance.TabIndex = 27
        lblLieuNaissance.Text = "Lieu de naissance"
        ' 
        ' txtDateNaissance
        ' 
        txtDateNaissance.Location = New Point(168, 196)
        txtDateNaissance.Name = "txtDateNaissance"
        txtDateNaissance.Size = New Size(100, 23)
        txtDateNaissance.TabIndex = 28
        ' 
        ' txLieuNaissance
        ' 
        txLieuNaissance.Location = New Point(168, 239)
        txLieuNaissance.Name = "txLieuNaissance"
        txLieuNaissance.Size = New Size(295, 23)
        txLieuNaissance.TabIndex = 29
        ' 
        ' dtpDateNaissance
        ' 
        dtpDateNaissance.Location = New Point(274, 196)
        dtpDateNaissance.Name = "dtpDateNaissance"
        dtpDateNaissance.Size = New Size(189, 23)
        dtpDateNaissance.TabIndex = 30
        ' 
        ' FrmNouveauTiers
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(564, 522)
        Controls.Add(dtpDateNaissance)
        Controls.Add(txLieuNaissance)
        Controls.Add(txtDateNaissance)
        Controls.Add(lblLieuNaissance)
        Controls.Add(lblDateNaissance)
        Controls.Add(txtSousCategorie)
        Controls.Add(btnSousCategorie)
        Controls.Add(btnCategorie)
        Controls.Add(btnCoordonnees)
        Controls.Add(lblCoordonnees)
        Controls.Add(btnCreerTiers)
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
        Name = "FrmNouveauTiers"
        Text = "Nouveau Tiers"
        grpPersonne.ResumeLayout(False)
        grpPersonne.PerformLayout()
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
    Friend WithEvents lblSousCategorie As Label
    Friend WithEvents btnCreerTiers As Button
    Friend WithEvents lblCoordonnees As Label
    Friend WithEvents btnCoordonnees As Button
    Friend WithEvents btnCategorie As Button
    Friend WithEvents btnSousCategorie As Button
    Friend WithEvents txtSousCategorie As TextBox
    Friend WithEvents lblDateNaissance As Label
    Friend WithEvents lblLieuNaissance As Label
    Friend WithEvents txtDateNaissance As TextBox
    Friend WithEvents txLieuNaissance As TextBox
    Friend WithEvents dtpDateNaissance As DateTimePicker
    Friend WithEvents ttCoordonnes As ToolTip
End Class
