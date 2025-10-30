<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmEditUtilisateur
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
        txtNom = New TextBox()
        txtMdp = New TextBox()
        cboRole = New ComboBox()
        chkActif = New CheckBox()
        btnValider = New Button()
        Button1 = New Button()
        btnAnnuler = New Button()
        SuspendLayout()
        ' 
        ' txtNom
        ' 
        txtNom.Location = New Point(74, 28)
        txtNom.Name = "txtNom"
        txtNom.Size = New Size(100, 23)
        txtNom.TabIndex = 0
        ' 
        ' txtMdp
        ' 
        txtMdp.Location = New Point(75, 79)
        txtMdp.Name = "txtMdp"
        txtMdp.Size = New Size(100, 23)
        txtMdp.TabIndex = 1
        ' 
        ' cboRole
        ' 
        cboRole.FormattingEnabled = True
        cboRole.Location = New Point(77, 140)
        cboRole.Name = "cboRole"
        cboRole.Size = New Size(121, 23)
        cboRole.TabIndex = 2
        ' 
        ' chkActif
        ' 
        chkActif.AutoSize = True
        chkActif.Location = New Point(273, 146)
        chkActif.Name = "chkActif"
        chkActif.Size = New Size(51, 19)
        chkActif.TabIndex = 3
        chkActif.Text = "Actif"
        chkActif.UseVisualStyleBackColor = True
        ' 
        ' btnValider
        ' 
        btnValider.Location = New Point(82, 206)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(75, 23)
        btnValider.TabIndex = 4
        btnValider.Text = "&Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(0, 0)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 23)
        Button1.TabIndex = 5
        Button1.Text = "Button1"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(229, 206)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 6
        btnAnnuler.Text = "&Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' FrmEditUtilisateur
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnAnnuler)
        Controls.Add(Button1)
        Controls.Add(btnValider)
        Controls.Add(chkActif)
        Controls.Add(cboRole)
        Controls.Add(txtMdp)
        Controls.Add(txtNom)
        Name = "FrmEditUtilisateur"
        Text = "Edite un utilisateur"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents txtNom As TextBox
    Friend WithEvents txtMdp As TextBox
    Friend WithEvents cboRole As ComboBox
    Friend WithEvents chkActif As CheckBox
    Friend WithEvents btnValider As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents btnAnnuler As Button
End Class
