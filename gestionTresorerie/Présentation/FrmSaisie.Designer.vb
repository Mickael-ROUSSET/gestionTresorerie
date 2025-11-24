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
        lblRemise = New Label()
        txtRemise = New TextBox()
        txtNote = New TextBox()
        lblNote = New Label()
        cbEvénement = New ComboBox()
        TiersBindingSource = New BindingSource(components)
        MouvementsBindingSource1 = New BindingSource(components)
        MouvementsBindingSource = New BindingSource(components)
        txtRechercheTiers = New TextBox()
        btnSelDoc = New Button()
        btnCreerTiers = New Button()
        btnListeChqRemise = New Button()
        lblChercheTiers = New Label()
        btnNouveauChq = New Button()
        lblTypeDocument = New Label()
        TypeDocImplBindingSource = New BindingSource(components)
        btnSelTiers = New Button()
        txtTiers = New TextBox()
        btnSelCat = New Button()
        txtCategorie = New TextBox()
        btnSelSousCategorie = New Button()
        txtSousCategorie = New TextBox()
        txtTypeDoc = New TextBox()
        btnSelTypeDoc = New Button()
        btnSelTypeMvt = New Button()
        txtTypeMvt = New TextBox()
        txtEvenement = New TextBox()
        btnSelEvenement = New Button()
        btnAjouteCoord = New Button()
        grpSens.SuspendLayout()
        GroupBox1.SuspendLayout()
        CType(TiersBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(TypeDocImplBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblType
        ' 
        lblType.AutoSize = True
        lblType.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblType.Location = New Point(20, 416)
        lblType.Name = "lblType"
        lblType.Size = New Size(33, 15)
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
        lblTiers.Location = New Point(17, 81)
        lblTiers.Name = "lblTiers"
        lblTiers.Size = New Size(34, 15)
        lblTiers.TabIndex = 4
        lblTiers.Text = "Tiers"
        ' 
        ' lblCategorie
        ' 
        lblCategorie.AutoSize = True
        lblCategorie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCategorie.Location = New Point(17, 176)
        lblCategorie.Name = "lblCategorie"
        lblCategorie.Size = New Size(61, 15)
        lblCategorie.TabIndex = 5
        lblCategorie.Text = "Catégorie"
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblSousCategorie.Location = New Point(20, 261)
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
        grpSens.Location = New Point(374, 29)
        grpSens.Name = "grpSens"
        grpSens.Size = New Size(154, 63)
        grpSens.TabIndex = 12
        grpSens.TabStop = False
        grpSens.Text = "Sens"
        ' 
        ' btnValider
        ' 
        btnValider.AutoSize = True
        btnValider.Location = New Point(21, 613)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(75, 25)
        btnValider.TabIndex = 13
        btnValider.Text = "Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' lblMontant
        ' 
        lblMontant.AutoSize = True
        lblMontant.Location = New Point(237, 34)
        lblMontant.Name = "lblMontant"
        lblMontant.Size = New Size(53, 15)
        lblMontant.TabIndex = 14
        lblMontant.Text = "Montant"
        ' 
        ' txtMontant
        ' 
        txtMontant.Location = New Point(237, 58)
        txtMontant.Name = "txtMontant"
        txtMontant.Size = New Size(128, 23)
        txtMontant.TabIndex = 15
        ' 
        ' lblEvénement
        ' 
        lblEvénement.AutoSize = True
        lblEvénement.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblEvénement.Location = New Point(20, 515)
        lblEvénement.Name = "lblEvénement"
        lblEvénement.Size = New Size(71, 15)
        lblEvénement.TabIndex = 17
        lblEvénement.Text = "Evénement"
        ' 
        ' GroupBox1
        ' 
        GroupBox1.AutoSize = True
        GroupBox1.Controls.Add(rbCree)
        GroupBox1.Controls.Add(rbRapproche)
        GroupBox1.Location = New Point(570, 27)
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
        rbRapproche.Location = New Point(92, 22)
        rbRapproche.Name = "rbRapproche"
        rbRapproche.Size = New Size(82, 19)
        rbRapproche.TabIndex = 10
        rbRapproche.Text = "Rapproché"
        rbRapproche.UseVisualStyleBackColor = True
        ' 
        ' btnInsereTiers
        ' 
        btnInsereTiers.Location = New Point(643, 110)
        btnInsereTiers.Name = "btnInsereTiers"
        btnInsereTiers.Size = New Size(90, 23)
        btnInsereTiers.TabIndex = 19
        btnInsereTiers.Text = "Insère un tiers"
        btnInsereTiers.UseVisualStyleBackColor = True
        ' 
        ' lblRemise
        ' 
        lblRemise.AutoSize = True
        lblRemise.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblRemise.Location = New Point(22, 556)
        lblRemise.Name = "lblRemise"
        lblRemise.Size = New Size(48, 15)
        lblRemise.TabIndex = 23
        lblRemise.Text = "Remise"
        ' 
        ' txtRemise
        ' 
        txtRemise.Location = New Point(133, 556)
        txtRemise.Name = "txtRemise"
        txtRemise.Size = New Size(328, 23)
        txtRemise.TabIndex = 24
        ' 
        ' txtNote
        ' 
        txtNote.Location = New Point(233, 479)
        txtNote.Name = "txtNote"
        txtNote.Size = New Size(328, 23)
        txtNote.TabIndex = 26
        ' 
        ' lblNote
        ' 
        lblNote.AutoSize = True
        lblNote.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblNote.Location = New Point(21, 479)
        lblNote.Name = "lblNote"
        lblNote.Size = New Size(35, 15)
        lblNote.TabIndex = 25
        lblNote.Text = "Note"
        ' 
        ' cbEvénement
        ' 
        cbEvénement.FormattingEnabled = True
        cbEvénement.Location = New Point(233, 515)
        cbEvénement.Name = "cbEvénement"
        cbEvénement.Size = New Size(328, 23)
        cbEvénement.TabIndex = 31
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
        ' txtRechercheTiers
        ' 
        txtRechercheTiers.Location = New Point(297, 110)
        txtRechercheTiers.Name = "txtRechercheTiers"
        txtRechercheTiers.Size = New Size(100, 23)
        txtRechercheTiers.TabIndex = 36
        ' 
        ' btnSelDoc
        ' 
        btnSelDoc.Location = New Point(582, 365)
        btnSelDoc.Name = "btnSelDoc"
        btnSelDoc.Size = New Size(168, 23)
        btnSelDoc.TabIndex = 38
        btnSelDoc.Text = "Sélectionne document"
        btnSelDoc.UseVisualStyleBackColor = True
        ' 
        ' btnCreerTiers
        ' 
        btnCreerTiers.Location = New Point(20, 110)
        btnCreerTiers.Name = "btnCreerTiers"
        btnCreerTiers.Size = New Size(75, 23)
        btnCreerTiers.TabIndex = 39
        btnCreerTiers.Text = "Créer"
        btnCreerTiers.UseVisualStyleBackColor = True
        ' 
        ' btnListeChqRemise
        ' 
        btnListeChqRemise.Location = New Point(493, 559)
        btnListeChqRemise.Name = "btnListeChqRemise"
        btnListeChqRemise.Size = New Size(159, 23)
        btnListeChqRemise.TabIndex = 44
        btnListeChqRemise.Text = "Entrer la liste des chèques"
        btnListeChqRemise.UseVisualStyleBackColor = True
        ' 
        ' lblChercheTiers
        ' 
        lblChercheTiers.AutoSize = True
        lblChercheTiers.Location = New Point(195, 110)
        lblChercheTiers.Name = "lblChercheTiers"
        lblChercheTiers.Size = New Size(76, 15)
        lblChercheTiers.TabIndex = 45
        lblChercheTiers.Text = "Cherche tiers"
        ' 
        ' btnNouveauChq
        ' 
        btnNouveauChq.Location = New Point(206, 614)
        btnNouveauChq.Name = "btnNouveauChq"
        btnNouveauChq.Size = New Size(120, 23)
        btnNouveauChq.TabIndex = 46
        btnNouveauChq.Text = "Nouveau chèque"
        btnNouveauChq.UseVisualStyleBackColor = True
        ' 
        ' lblTypeDocument
        ' 
        lblTypeDocument.AutoSize = True
        lblTypeDocument.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblTypeDocument.Location = New Point(20, 332)
        lblTypeDocument.Name = "lblTypeDocument"
        lblTypeDocument.Size = New Size(92, 15)
        lblTypeDocument.TabIndex = 47
        lblTypeDocument.Text = "TypeDocument"
        ' 
        ' TypeDocImplBindingSource
        ' 
        TypeDocImplBindingSource.DataSource = GetType(TypeDocImpl)
        ' 
        ' btnSelTiers
        ' 
        btnSelTiers.Location = New Point(20, 139)
        btnSelTiers.Name = "btnSelTiers"
        btnSelTiers.Size = New Size(110, 23)
        btnSelTiers.TabIndex = 51
        btnSelTiers.Text = "Sélection du tiers"
        btnSelTiers.UseVisualStyleBackColor = True
        ' 
        ' txtTiers
        ' 
        txtTiers.Location = New Point(195, 140)
        txtTiers.Name = "txtTiers"
        txtTiers.Size = New Size(366, 23)
        txtTiers.TabIndex = 52
        ' 
        ' btnSelCat
        ' 
        btnSelCat.Location = New Point(17, 205)
        btnSelCat.Name = "btnSelCat"
        btnSelCat.Size = New Size(156, 23)
        btnSelCat.TabIndex = 53
        btnSelCat.Text = "Sélection de la catégorie"
        btnSelCat.UseVisualStyleBackColor = True
        ' 
        ' txtCategorie
        ' 
        txtCategorie.Location = New Point(195, 206)
        txtCategorie.Name = "txtCategorie"
        txtCategorie.Size = New Size(366, 23)
        txtCategorie.TabIndex = 54
        ' 
        ' btnSelSousCategorie
        ' 
        btnSelSousCategorie.Location = New Point(20, 283)
        btnSelSousCategorie.Name = "btnSelSousCategorie"
        btnSelSousCategorie.Size = New Size(168, 23)
        btnSelSousCategorie.TabIndex = 55
        btnSelSousCategorie.Text = "Sélection sous-catégorie"
        btnSelSousCategorie.UseVisualStyleBackColor = True
        ' 
        ' txtSousCategorie
        ' 
        txtSousCategorie.Location = New Point(206, 282)
        txtSousCategorie.Name = "txtSousCategorie"
        txtSousCategorie.Size = New Size(355, 23)
        txtSousCategorie.TabIndex = 56
        ' 
        ' txtTypeDoc
        ' 
        txtTypeDoc.Location = New Point(256, 365)
        txtTypeDoc.Name = "txtTypeDoc"
        txtTypeDoc.Size = New Size(305, 23)
        txtTypeDoc.TabIndex = 57
        ' 
        ' btnSelTypeDoc
        ' 
        btnSelTypeDoc.Location = New Point(20, 364)
        btnSelTypeDoc.Name = "btnSelTypeDoc"
        btnSelTypeDoc.Size = New Size(215, 23)
        btnSelTypeDoc.TabIndex = 58
        btnSelTypeDoc.Text = "Sélection du tye de document"
        btnSelTypeDoc.UseVisualStyleBackColor = True
        ' 
        ' btnSelTypeMvt
        ' 
        btnSelTypeMvt.Location = New Point(20, 445)
        btnSelTypeMvt.Name = "btnSelTypeMvt"
        btnSelTypeMvt.Size = New Size(211, 23)
        btnSelTypeMvt.TabIndex = 59
        btnSelTypeMvt.Text = "Sélection du type de mouvement"
        btnSelTypeMvt.UseVisualStyleBackColor = True
        ' 
        ' txtTypeMvt
        ' 
        txtTypeMvt.Location = New Point(256, 446)
        txtTypeMvt.Name = "txtTypeMvt"
        txtTypeMvt.Size = New Size(305, 23)
        txtTypeMvt.TabIndex = 60
        ' 
        ' txtEvenement
        ' 
        txtEvenement.Location = New Point(643, 507)
        txtEvenement.Name = "txtEvenement"
        txtEvenement.Size = New Size(309, 23)
        txtEvenement.TabIndex = 61
        ' 
        ' btnSelEvenement
        ' 
        btnSelEvenement.Location = New Point(643, 479)
        btnSelEvenement.Name = "btnSelEvenement"
        btnSelEvenement.Size = New Size(159, 23)
        btnSelEvenement.TabIndex = 62
        btnSelEvenement.Text = "Sélection de l'événement"
        btnSelEvenement.UseVisualStyleBackColor = True
        ' 
        ' btnAjouteCoord
        ' 
        btnAjouteCoord.Location = New Point(645, 146)
        btnAjouteCoord.Name = "btnAjouteCoord"
        btnAjouteCoord.Size = New Size(105, 23)
        btnAjouteCoord.TabIndex = 63
        btnAjouteCoord.Text = "Coordonnées..."
        btnAjouteCoord.UseVisualStyleBackColor = True
        ' 
        ' FrmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(973, 660)
        Controls.Add(btnAjouteCoord)
        Controls.Add(btnSelEvenement)
        Controls.Add(txtEvenement)
        Controls.Add(txtTypeMvt)
        Controls.Add(btnSelTypeMvt)
        Controls.Add(btnSelTypeDoc)
        Controls.Add(txtTypeDoc)
        Controls.Add(txtSousCategorie)
        Controls.Add(btnSelSousCategorie)
        Controls.Add(txtCategorie)
        Controls.Add(btnSelCat)
        Controls.Add(txtTiers)
        Controls.Add(btnSelTiers)
        Controls.Add(lblTypeDocument)
        Controls.Add(btnNouveauChq)
        Controls.Add(lblChercheTiers)
        Controls.Add(btnListeChqRemise)
        Controls.Add(btnCreerTiers)
        Controls.Add(btnSelDoc)
        Controls.Add(txtRechercheTiers)
        Controls.Add(cbEvénement)
        Controls.Add(txtNote)
        Controls.Add(lblNote)
        Controls.Add(txtRemise)
        Controls.Add(lblRemise)
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
        CType(TiersBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(MouvementsBindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(TypeDocImplBindingSource, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents lblRemise As Label
    Friend WithEvents txtRemise As TextBox
    Friend WithEvents txtNote As TextBox
    Friend WithEvents lblNote As Label
    Friend WithEvents cbEvénement As ComboBox
    Friend WithEvents MouvementsBindingSource1 As BindingSource
    Friend WithEvents MouvementsBindingSource As BindingSource
    Friend WithEvents TiersBindingSource As BindingSource
    Friend WithEvents txtRechercheTiers As TextBox
    Friend WithEvents btnSelDoc As Button
    Friend WithEvents btnCreerTiers As Button
    Friend WithEvents btnListeChqRemise As Button
    Friend WithEvents lblChercheTiers As Label
    Friend WithEvents btnNouveauChq As Button
    Friend WithEvents lblTypeDocument As Label
    Friend WithEvents TypeDocImplBindingSource As BindingSource
    Friend WithEvents ClasseTypeDocDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents btnSelTiers As Button
    Friend WithEvents txtTiers As TextBox
    Friend WithEvents btnSelCat As Button
    Friend WithEvents txtCategorie As TextBox
    Friend WithEvents btnSelSousCategorie As Button
    Friend WithEvents txtSousCategorie As TextBox
    Friend WithEvents txtTypeDoc As TextBox
    Friend WithEvents btnSelTypeDoc As Button
    Friend WithEvents btnSelTypeMvt As Button
    Friend WithEvents txtTypeMvt As TextBox
    Friend WithEvents txtEvenement As TextBox
    Friend WithEvents btnSelEvenement As Button
    Friend WithEvents btnAjouteCoord As Button

End Class
