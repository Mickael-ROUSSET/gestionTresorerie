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
        TiersBindingSource = New BindingSource(components)
        MouvementsBindingSource1 = New BindingSource(components)
        MouvementsBindingSource = New BindingSource(components)
        btnSelDoc = New Button()
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
        Document = New Label()
        txtDocument = New TextBox()
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
        lblType.Location = New Point(23, 364)
        lblType.Name = "lblType"
        lblType.Size = New Size(128, 15)
        lblType.TabIndex = 0
        lblType.Text = "TypeMouvement mvt"
        ' 
        ' dateMvt
        ' 
        dateMvt.AllowDrop = True
        dateMvt.Location = New Point(23, 17)
        dateMvt.Name = "dateMvt"
        dateMvt.Size = New Size(194, 23)
        dateMvt.TabIndex = 1
        ' 
        ' lblTiers
        ' 
        lblTiers.AutoSize = True
        lblTiers.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblTiers.Location = New Point(23, 114)
        lblTiers.Name = "lblTiers"
        lblTiers.Size = New Size(34, 15)
        lblTiers.TabIndex = 4
        lblTiers.Text = "Tiers"
        ' 
        ' lblCategorie
        ' 
        lblCategorie.AutoSize = True
        lblCategorie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCategorie.Location = New Point(23, 164)
        lblCategorie.Name = "lblCategorie"
        lblCategorie.Size = New Size(61, 15)
        lblCategorie.TabIndex = 5
        lblCategorie.Text = "Catégorie"
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblSousCategorie.Location = New Point(23, 214)
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
        grpSens.Location = New Point(302, 6)
        grpSens.Name = "grpSens"
        grpSens.Size = New Size(154, 63)
        grpSens.TabIndex = 12
        grpSens.TabStop = False
        grpSens.Text = "Sens"
        ' 
        ' btnValider
        ' 
        btnValider.AutoSize = True
        btnValider.Location = New Point(23, 560)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(92, 25)
        btnValider.TabIndex = 7
        btnValider.Text = "&Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' lblMontant
        ' 
        lblMontant.AutoSize = True
        lblMontant.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblMontant.Location = New Point(23, 63)
        lblMontant.Name = "lblMontant"
        lblMontant.Size = New Size(55, 15)
        lblMontant.TabIndex = 14
        lblMontant.Text = "Montant"
        ' 
        ' txtMontant
        ' 
        txtMontant.Location = New Point(125, 60)
        txtMontant.Name = "txtMontant"
        txtMontant.Size = New Size(128, 23)
        txtMontant.TabIndex = 15
        ' 
        ' lblEvénement
        ' 
        lblEvénement.AutoSize = True
        lblEvénement.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblEvénement.Location = New Point(23, 464)
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
        GroupBox1.Location = New Point(482, 4)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(172, 63)
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
        rbRapproche.Location = New Point(67, 22)
        rbRapproche.Name = "rbRapproche"
        rbRapproche.Size = New Size(82, 19)
        rbRapproche.TabIndex = 10
        rbRapproche.Text = "Rapproché"
        rbRapproche.UseVisualStyleBackColor = True
        ' 
        ' btnInsereTiers
        ' 
        btnInsereTiers.Location = New Point(552, 110)
        btnInsereTiers.Name = "btnInsereTiers"
        btnInsereTiers.Size = New Size(105, 23)
        btnInsereTiers.TabIndex = 19
        btnInsereTiers.Text = "Nouveau tiers"
        btnInsereTiers.UseVisualStyleBackColor = True
        ' 
        ' lblRemise
        ' 
        lblRemise.AutoSize = True
        lblRemise.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblRemise.Location = New Point(23, 514)
        lblRemise.Name = "lblRemise"
        lblRemise.Size = New Size(48, 15)
        lblRemise.TabIndex = 23
        lblRemise.Text = "Remise"
        ' 
        ' txtRemise
        ' 
        txtRemise.Location = New Point(125, 510)
        txtRemise.Name = "txtRemise"
        txtRemise.Size = New Size(372, 23)
        txtRemise.TabIndex = 24
        ' 
        ' txtNote
        ' 
        txtNote.Location = New Point(125, 410)
        txtNote.Name = "txtNote"
        txtNote.Size = New Size(372, 23)
        txtNote.TabIndex = 26
        ' 
        ' lblNote
        ' 
        lblNote.AutoSize = True
        lblNote.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblNote.Location = New Point(23, 414)
        lblNote.Name = "lblNote"
        lblNote.Size = New Size(35, 15)
        lblNote.TabIndex = 25
        lblNote.Text = "Note"
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
        ' btnSelDoc
        ' 
        btnSelDoc.Location = New Point(508, 310)
        btnSelDoc.Name = "btnSelDoc"
        btnSelDoc.Size = New Size(38, 23)
        btnSelDoc.TabIndex = 5
        btnSelDoc.Text = "..."
        btnSelDoc.UseVisualStyleBackColor = True
        ' 
        ' lblTypeDocument
        ' 
        lblTypeDocument.AutoSize = True
        lblTypeDocument.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblTypeDocument.Location = New Point(23, 264)
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
        btnSelTiers.Location = New Point(508, 110)
        btnSelTiers.Name = "btnSelTiers"
        btnSelTiers.Size = New Size(38, 23)
        btnSelTiers.TabIndex = 0
        btnSelTiers.Text = "..."
        btnSelTiers.UseVisualStyleBackColor = True
        ' 
        ' txtTiers
        ' 
        txtTiers.Location = New Point(125, 110)
        txtTiers.Name = "txtTiers"
        txtTiers.Size = New Size(372, 23)
        txtTiers.TabIndex = 0
        ' 
        ' btnSelCat
        ' 
        btnSelCat.Location = New Point(508, 160)
        btnSelCat.Name = "btnSelCat"
        btnSelCat.Size = New Size(38, 23)
        btnSelCat.TabIndex = 2
        btnSelCat.Text = "..."
        btnSelCat.UseVisualStyleBackColor = True
        ' 
        ' txtCategorie
        ' 
        txtCategorie.Location = New Point(125, 160)
        txtCategorie.Name = "txtCategorie"
        txtCategorie.Size = New Size(372, 23)
        txtCategorie.TabIndex = 54
        ' 
        ' btnSelSousCategorie
        ' 
        btnSelSousCategorie.Location = New Point(508, 210)
        btnSelSousCategorie.Name = "btnSelSousCategorie"
        btnSelSousCategorie.Size = New Size(38, 23)
        btnSelSousCategorie.TabIndex = 3
        btnSelSousCategorie.Text = "..."
        btnSelSousCategorie.UseVisualStyleBackColor = True
        ' 
        ' txtSousCategorie
        ' 
        txtSousCategorie.Location = New Point(125, 210)
        txtSousCategorie.Name = "txtSousCategorie"
        txtSousCategorie.Size = New Size(372, 23)
        txtSousCategorie.TabIndex = 56
        ' 
        ' txtTypeDoc
        ' 
        txtTypeDoc.Location = New Point(125, 260)
        txtTypeDoc.Name = "txtTypeDoc"
        txtTypeDoc.Size = New Size(372, 23)
        txtTypeDoc.TabIndex = 57
        ' 
        ' btnSelTypeDoc
        ' 
        btnSelTypeDoc.Location = New Point(508, 260)
        btnSelTypeDoc.Name = "btnSelTypeDoc"
        btnSelTypeDoc.Size = New Size(38, 23)
        btnSelTypeDoc.TabIndex = 4
        btnSelTypeDoc.Text = "..."
        btnSelTypeDoc.UseVisualStyleBackColor = True
        ' 
        ' btnSelTypeMvt
        ' 
        btnSelTypeMvt.Location = New Point(508, 360)
        btnSelTypeMvt.Name = "btnSelTypeMvt"
        btnSelTypeMvt.Size = New Size(38, 23)
        btnSelTypeMvt.TabIndex = 6
        btnSelTypeMvt.Text = "..."
        btnSelTypeMvt.UseVisualStyleBackColor = True
        ' 
        ' txtTypeMvt
        ' 
        txtTypeMvt.Location = New Point(125, 360)
        txtTypeMvt.Name = "txtTypeMvt"
        txtTypeMvt.Size = New Size(372, 23)
        txtTypeMvt.TabIndex = 60
        ' 
        ' txtEvenement
        ' 
        txtEvenement.Location = New Point(125, 460)
        txtEvenement.Name = "txtEvenement"
        txtEvenement.Size = New Size(372, 23)
        txtEvenement.TabIndex = 61
        ' 
        ' btnSelEvenement
        ' 
        btnSelEvenement.Location = New Point(508, 460)
        btnSelEvenement.Name = "btnSelEvenement"
        btnSelEvenement.Size = New Size(38, 23)
        btnSelEvenement.TabIndex = 8
        btnSelEvenement.Text = "..."
        btnSelEvenement.UseVisualStyleBackColor = True
        ' 
        ' Document
        ' 
        Document.AutoSize = True
        Document.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Document.Location = New Point(23, 314)
        Document.Name = "Document"
        Document.Size = New Size(79, 15)
        Document.TabIndex = 64
        Document.Text = "id Document"
        ' 
        ' txtDocument
        ' 
        txtDocument.Location = New Point(125, 310)
        txtDocument.Name = "txtDocument"
        txtDocument.Size = New Size(372, 23)
        txtDocument.TabIndex = 65
        ' 
        ' frmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(672, 606)
        Controls.Add(txtDocument)
        Controls.Add(Document)
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
        Controls.Add(btnSelDoc)
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
        Name = "frmSaisie"
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
    Friend WithEvents MouvementsBindingSource1 As BindingSource
    Friend WithEvents MouvementsBindingSource As BindingSource
    Friend WithEvents TiersBindingSource As BindingSource
    Friend WithEvents btnSelDoc As Button
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
    Friend WithEvents Document As Label
    Friend WithEvents txtDocument As TextBox

End Class
