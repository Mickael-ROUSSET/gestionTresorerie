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
        grpSens.SuspendLayout()
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
        rbCredit.Location = New Point(92, 17)
        rbCredit.Name = "rbCredit"
        rbCredit.Size = New Size(57, 19)
        rbCredit.TabIndex = 10
        rbCredit.TabStop = True
        rbCredit.Text = "Crédit"
        rbCredit.UseVisualStyleBackColor = True
        ' 
        ' rbDebit
        ' 
        rbDebit.AutoSize = True
        rbDebit.Location = New Point(20, 17)
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
        grpSens.Size = New Size(166, 58)
        grpSens.TabIndex = 12
        grpSens.TabStop = False
        grpSens.Text = "Sens"
        ' 
        ' btnValider
        ' 
        btnValider.AutoSize = True
        btnValider.Location = New Point(146, 371)
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
        ' FrmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
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

End Class
