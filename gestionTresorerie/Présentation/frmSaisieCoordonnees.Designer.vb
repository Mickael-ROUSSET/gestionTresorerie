<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSaisieCoordonnees
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
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

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        txtRue1 = New TextBox()
        txtRue2 = New TextBox()
        txtCodePostal = New TextBox()
        txtPays = New TextBox()
        txtEmail = New TextBox()
        txtTelephone = New TextBox()
        btnValider = New Button()
        btnAnnuler = New Button()
        txtTypeAdresse = New TextBox()
        lblTypeAdresse = New Label()
        lblRue1 = New Label()
        lblRue2 = New Label()
        lblCodePostal = New Label()
        lblVille = New Label()
        lblPays = New Label()
        lblEmail = New Label()
        lblTelephone = New Label()
        cbVilles = New ComboBox()
        btnRetourSansEnregistrer = New Button()
        SuspendLayout()
        ' 
        ' txtRue1
        ' 
        txtRue1.Location = New Point(208, 61)
        txtRue1.Name = "txtRue1"
        txtRue1.Size = New Size(248, 23)
        txtRue1.TabIndex = 0
        ' 
        ' txtRue2
        ' 
        txtRue2.Location = New Point(208, 107)
        txtRue2.Name = "txtRue2"
        txtRue2.Size = New Size(248, 23)
        txtRue2.TabIndex = 1
        ' 
        ' txtCodePostal
        ' 
        txtCodePostal.Location = New Point(208, 153)
        txtCodePostal.Name = "txtCodePostal"
        txtCodePostal.Size = New Size(248, 23)
        txtCodePostal.TabIndex = 2
        ' 
        ' txtPays
        ' 
        txtPays.Location = New Point(208, 270)
        txtPays.Name = "txtPays"
        txtPays.Size = New Size(248, 23)
        txtPays.TabIndex = 4
        txtPays.Text = "France"
        ' 
        ' txtEmail
        ' 
        txtEmail.Location = New Point(208, 310)
        txtEmail.Name = "txtEmail"
        txtEmail.Size = New Size(248, 23)
        txtEmail.TabIndex = 5
        ' 
        ' txtTelephone
        ' 
        txtTelephone.Location = New Point(208, 359)
        txtTelephone.Name = "txtTelephone"
        txtTelephone.Size = New Size(248, 23)
        txtTelephone.TabIndex = 6
        ' 
        ' btnValider
        ' 
        btnValider.Location = New Point(41, 410)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(100, 23)
        btnValider.TabIndex = 7
        btnValider.Text = "&Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(378, 410)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(100, 23)
        btnAnnuler.TabIndex = 9
        btnAnnuler.Text = "&Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' txtTypeAdresse
        ' 
        txtTypeAdresse.Location = New Point(208, 15)
        txtTypeAdresse.Name = "txtTypeAdresse"
        txtTypeAdresse.Size = New Size(248, 23)
        txtTypeAdresse.TabIndex = 9
        txtTypeAdresse.Text = "Principale"
        ' 
        ' lblTypeAdresse
        ' 
        lblTypeAdresse.AutoSize = True
        lblTypeAdresse.Location = New Point(68, 15)
        lblTypeAdresse.Name = "lblTypeAdresse"
        lblTypeAdresse.Size = New Size(73, 15)
        lblTypeAdresse.TabIndex = 10
        lblTypeAdresse.Text = "TypeAdresse"
        ' 
        ' lblRue1
        ' 
        lblRue1.AutoSize = True
        lblRue1.Location = New Point(68, 61)
        lblRue1.Name = "lblRue1"
        lblRue1.Size = New Size(33, 15)
        lblRue1.TabIndex = 11
        lblRue1.Text = "Rue1"
        ' 
        ' lblRue2
        ' 
        lblRue2.AutoSize = True
        lblRue2.Location = New Point(68, 107)
        lblRue2.Name = "lblRue2"
        lblRue2.Size = New Size(33, 15)
        lblRue2.TabIndex = 12
        lblRue2.Text = "Rue2"
        ' 
        ' lblCodePostal
        ' 
        lblCodePostal.AutoSize = True
        lblCodePostal.Location = New Point(68, 153)
        lblCodePostal.Name = "lblCodePostal"
        lblCodePostal.Size = New Size(67, 15)
        lblCodePostal.TabIndex = 13
        lblCodePostal.Text = "CodePostal"
        ' 
        ' lblVille
        ' 
        lblVille.AutoSize = True
        lblVille.Location = New Point(68, 216)
        lblVille.Name = "lblVille"
        lblVille.Size = New Size(29, 15)
        lblVille.TabIndex = 14
        lblVille.Text = "Ville"
        ' 
        ' lblPays
        ' 
        lblPays.AutoSize = True
        lblPays.Location = New Point(68, 270)
        lblPays.Name = "lblPays"
        lblPays.Size = New Size(31, 15)
        lblPays.TabIndex = 15
        lblPays.Text = "Pays"
        ' 
        ' lblEmail
        ' 
        lblEmail.AutoSize = True
        lblEmail.Location = New Point(68, 310)
        lblEmail.Name = "lblEmail"
        lblEmail.Size = New Size(36, 15)
        lblEmail.TabIndex = 16
        lblEmail.Text = "Email"
        ' 
        ' lblTelephone
        ' 
        lblTelephone.AutoSize = True
        lblTelephone.Location = New Point(68, 359)
        lblTelephone.Name = "lblTelephone"
        lblTelephone.Size = New Size(62, 15)
        lblTelephone.TabIndex = 17
        lblTelephone.Text = "Téléphone"
        ' 
        ' cbVilles
        ' 
        cbVilles.FormattingEnabled = True
        cbVilles.Location = New Point(208, 208)
        cbVilles.Name = "cbVilles"
        cbVilles.Size = New Size(248, 23)
        cbVilles.TabIndex = 3
        ' 
        ' btnRetourSansEnregistrer
        ' 
        btnRetourSansEnregistrer.Location = New Point(195, 412)
        btnRetourSansEnregistrer.Name = "btnRetourSansEnregistrer"
        btnRetourSansEnregistrer.Size = New Size(133, 23)
        btnRetourSansEnregistrer.TabIndex = 8
        btnRetourSansEnregistrer.Text = "RetourSansEnregistrer"
        btnRetourSansEnregistrer.UseVisualStyleBackColor = True
        ' 
        ' frmSaisieCoordonnees
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(505, 450)
        Controls.Add(btnRetourSansEnregistrer)
        Controls.Add(cbVilles)
        Controls.Add(lblTelephone)
        Controls.Add(lblEmail)
        Controls.Add(lblPays)
        Controls.Add(lblVille)
        Controls.Add(lblCodePostal)
        Controls.Add(lblRue2)
        Controls.Add(lblRue1)
        Controls.Add(lblTypeAdresse)
        Controls.Add(txtTypeAdresse)
        Controls.Add(btnAnnuler)
        Controls.Add(btnValider)
        Controls.Add(txtTelephone)
        Controls.Add(txtEmail)
        Controls.Add(txtPays)
        Controls.Add(txtCodePostal)
        Controls.Add(txtRue2)
        Controls.Add(txtRue1)
        Name = "frmSaisieCoordonnees"
        Text = "Saisie des coordonnées"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents txtRue1 As TextBox
    Friend WithEvents txtRue2 As TextBox
    Friend WithEvents txtCodePostal As TextBox
    Friend WithEvents txtPays As TextBox
    Friend WithEvents txtEmail As TextBox
    Friend WithEvents txtTelephone As TextBox
    Friend WithEvents btnValider As Button
    Friend WithEvents btnAnnuler As Button
    Friend WithEvents txtTypeAdresse As TextBox
    Friend WithEvents lblTypeAdresse As Label
    Friend WithEvents lblRue1 As Label
    Friend WithEvents lblRue2 As Label
    Friend WithEvents lblCodePostal As Label
    Friend WithEvents lblVille As Label
    Friend WithEvents lblPays As Label
    Friend WithEvents lblEmail As Label
    Friend WithEvents lblTelephone As Label
    Friend WithEvents cbVilles As ComboBox
    Friend WithEvents btnRetourSansEnregistrer As Button
End Class
