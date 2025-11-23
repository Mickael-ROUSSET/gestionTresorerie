<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmConfiguration
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
        txtCheminBDD = New TextBox()
        lblCheminBDD = New Label()
        lblDossierRacine = New Label()
        txtDossierRacine = New TextBox()
        btnParcourirBDD = New Button()
        btnParcourirDossier = New Button()
        btnOK = New Button()
        SuspendLayout()
        ' 
        ' txtCheminBDD
        ' 
        txtCheminBDD.Location = New Point(310, 63)
        txtCheminBDD.Name = "txtCheminBDD"
        txtCheminBDD.Size = New Size(100, 23)
        txtCheminBDD.TabIndex = 0
        ' 
        ' lblCheminBDD
        ' 
        lblCheminBDD.AutoSize = True
        lblCheminBDD.Location = New Point(35, 63)
        lblCheminBDD.Name = "lblCheminBDD"
        lblCheminBDD.Size = New Size(168, 15)
        lblCheminBDD.TabIndex = 1
        lblCheminBDD.Text = "Chemin de la base de données"
        ' 
        ' lblDossierRacine
        ' 
        lblDossierRacine.AutoSize = True
        lblDossierRacine.Location = New Point(41, 115)
        lblDossierRacine.Name = "lblDossierRacine"
        lblDossierRacine.Size = New Size(228, 15)
        lblDossierRacine.TabIndex = 2
        lblDossierRacine.Text = "Dossier racine du drive google AGUMAAA"
        ' 
        ' txtDossierRacine
        ' 
        txtDossierRacine.Location = New Point(310, 112)
        txtDossierRacine.Name = "txtDossierRacine"
        txtDossierRacine.Size = New Size(100, 23)
        txtDossierRacine.TabIndex = 3
        ' 
        ' btnParcourirBDD
        ' 
        btnParcourirBDD.Location = New Point(474, 67)
        btnParcourirBDD.Name = "btnParcourirBDD"
        btnParcourirBDD.Size = New Size(89, 23)
        btnParcourirBDD.TabIndex = 4
        btnParcourirBDD.Text = "Sélectionner"
        btnParcourirBDD.UseVisualStyleBackColor = True
        ' 
        ' btnParcourirDossier
        ' 
        btnParcourirDossier.Location = New Point(474, 115)
        btnParcourirDossier.Name = "btnParcourirDossier"
        btnParcourirDossier.Size = New Size(143, 23)
        btnParcourirDossier.TabIndex = 5
        btnParcourirDossier.Text = "Sélectionner un dossier"
        btnParcourirDossier.UseVisualStyleBackColor = True
        ' 
        ' btnOK
        ' 
        btnOK.Location = New Point(100, 204)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(75, 23)
        btnOK.TabIndex = 6
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' frmConfiguration
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnOK)
        Controls.Add(btnParcourirDossier)
        Controls.Add(btnParcourirBDD)
        Controls.Add(txtDossierRacine)
        Controls.Add(lblDossierRacine)
        Controls.Add(lblCheminBDD)
        Controls.Add(txtCheminBDD)
        Name = "frmConfiguration"
        Text = "Form1"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents txtCheminBDD As TextBox
    Friend WithEvents lblCheminBDD As Label
    Friend WithEvents lblDossierRacine As Label
    Friend WithEvents txtDossierRacine As TextBox
    Friend WithEvents btnParcourirBDD As Button
    Friend WithEvents btnParcourirDossier As Button
    Friend WithEvents btnOK As Button
End Class
