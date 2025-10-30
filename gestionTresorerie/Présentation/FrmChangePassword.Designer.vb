<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmChangePassword
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
        lblAncien = New Label()
        txtAncien = New TextBox()
        lblNouveau = New Label()
        txtNouveau = New TextBox()
        lblConfirm = New Label()
        txtConfirm = New TextBox()
        btnValider = New Button()
        btnAnnuler = New Button()
        SuspendLayout()
        ' 
        ' lblAncien
        ' 
        lblAncien.AutoSize = True
        lblAncien.Location = New Point(55, 60)
        lblAncien.Name = "lblAncien"
        lblAncien.Size = New Size(123, 15)
        lblAncien.TabIndex = 0
        lblAncien.Text = "Ancien mot de passe :"
        ' 
        ' txtAncien
        ' 
        txtAncien.Location = New Point(200, 59)
        txtAncien.Name = "txtAncien"
        txtAncien.Size = New Size(100, 23)
        txtAncien.TabIndex = 1
        txtAncien.UseSystemPasswordChar = True
        ' 
        ' lblNouveau
        ' 
        lblNouveau.AutoSize = True
        lblNouveau.Location = New Point(65, 105)
        lblNouveau.Name = "lblNouveau"
        lblNouveau.Size = New Size(134, 15)
        lblNouveau.TabIndex = 2
        lblNouveau.Text = "Nouveau mot de passe :"
        ' 
        ' txtNouveau
        ' 
        txtNouveau.Location = New Point(210, 100)
        txtNouveau.Name = "txtNouveau"
        txtNouveau.Size = New Size(100, 23)
        txtNouveau.TabIndex = 3
        txtNouveau.UseSystemPasswordChar = True
        ' 
        ' lblConfirm
        ' 
        lblConfirm.AutoSize = True
        lblConfirm.Location = New Point(55, 150)
        lblConfirm.Name = "lblConfirm"
        lblConfirm.Size = New Size(153, 15)
        lblConfirm.TabIndex = 4
        lblConfirm.Text = "Confirmez le mot de passe :"
        ' 
        ' txtConfirm
        ' 
        txtConfirm.Location = New Point(232, 149)
        txtConfirm.Name = "txtConfirm"
        txtConfirm.Size = New Size(100, 23)
        txtConfirm.TabIndex = 5
        txtConfirm.UseSystemPasswordChar = True
        ' 
        ' btnValider
        ' 
        btnValider.Location = New Point(62, 207)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(75, 23)
        btnValider.TabIndex = 6
        btnValider.Text = "&Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(200, 212)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 7
        btnAnnuler.Text = "&Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' FrmChangePassword
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnAnnuler)
        Controls.Add(btnValider)
        Controls.Add(txtConfirm)
        Controls.Add(lblConfirm)
        Controls.Add(txtNouveau)
        Controls.Add(lblNouveau)
        Controls.Add(txtAncien)
        Controls.Add(lblAncien)
        Name = "FrmChangePassword"
        Text = "Change Password"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblAncien As Label
    Friend WithEvents txtAncien As TextBox
    Friend WithEvents lblNouveau As Label
    Friend WithEvents txtNouveau As TextBox
    Friend WithEvents lblConfirm As Label
    Friend WithEvents txtConfirm As TextBox
    Friend WithEvents btnValider As Button
    Friend WithEvents btnAnnuler As Button
End Class
