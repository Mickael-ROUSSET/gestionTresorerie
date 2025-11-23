<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmGestionUtilisateurs
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
        dgvUtilisateurs = New DataGridView()
        btnAjouter = New Button()
        btnModifier = New Button()
        btnSupprimer = New Button()
        btnRecharger = New Button()
        btnFermer = New Button()
        CType(dgvUtilisateurs, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' dgvUtilisateurs
        ' 
        dgvUtilisateurs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvUtilisateurs.Location = New Point(56, 42)
        dgvUtilisateurs.Name = "dgvUtilisateurs"
        dgvUtilisateurs.Size = New Size(643, 173)
        dgvUtilisateurs.TabIndex = 0
        ' 
        ' btnAjouter
        ' 
        btnAjouter.Location = New Point(56, 241)
        btnAjouter.Name = "btnAjouter"
        btnAjouter.Size = New Size(75, 23)
        btnAjouter.TabIndex = 1
        btnAjouter.Text = "&Ajouter"
        btnAjouter.UseVisualStyleBackColor = True
        ' 
        ' btnModifier
        ' 
        btnModifier.Location = New Point(56, 286)
        btnModifier.Name = "btnModifier"
        btnModifier.Size = New Size(75, 23)
        btnModifier.TabIndex = 2
        btnModifier.Text = "&Modifier"
        btnModifier.UseVisualStyleBackColor = True
        ' 
        ' btnSupprimer
        ' 
        btnSupprimer.Location = New Point(56, 325)
        btnSupprimer.Name = "btnSupprimer"
        btnSupprimer.Size = New Size(75, 23)
        btnSupprimer.TabIndex = 3
        btnSupprimer.Text = "&Supprimer"
        btnSupprimer.UseVisualStyleBackColor = True
        ' 
        ' btnRecharger
        ' 
        btnRecharger.Location = New Point(371, 232)
        btnRecharger.Name = "btnRecharger"
        btnRecharger.Size = New Size(75, 23)
        btnRecharger.TabIndex = 4
        btnRecharger.Text = "&Recharger"
        btnRecharger.UseVisualStyleBackColor = True
        ' 
        ' btnFermer
        ' 
        btnFermer.Location = New Point(375, 275)
        btnFermer.Name = "btnFermer"
        btnFermer.Size = New Size(75, 23)
        btnFermer.TabIndex = 5
        btnFermer.Text = "&Fermer"
        btnFermer.UseVisualStyleBackColor = True
        ' 
        ' FrmGestionUtilisateurs
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnFermer)
        Controls.Add(btnRecharger)
        Controls.Add(btnSupprimer)
        Controls.Add(btnModifier)
        Controls.Add(btnAjouter)
        Controls.Add(dgvUtilisateurs)
        Name = "FrmGestionUtilisateurs"
        Text = "Gestion des utilisateurs"
        CType(dgvUtilisateurs, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents dgvUtilisateurs As DataGridView
    Friend WithEvents btnAjouter As Button
    Friend WithEvents btnModifier As Button
    Friend WithEvents btnSupprimer As Button
    Friend WithEvents btnRecharger As Button
    Friend WithEvents btnFermer As Button
End Class
