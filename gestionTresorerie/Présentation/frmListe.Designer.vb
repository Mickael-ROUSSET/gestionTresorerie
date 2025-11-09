<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmListe
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
        dgvListe = New DataGridView()
        btnSel = New Button()
        btnAnnuler = New Button()
        btnSansSelection = New Button()
        CType(dgvListe, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' dgvListe
        ' 
        dgvListe.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvListe.Location = New Point(36, 119)
        dgvListe.Name = "dgvListe"
        dgvListe.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvListe.Size = New Size(707, 234)
        dgvListe.TabIndex = 0
        ' 
        ' btnSel
        ' 
        btnSel.Location = New Point(55, 381)
        btnSel.Name = "btnSel"
        btnSel.Size = New Size(111, 23)
        btnSel.TabIndex = 1
        btnSel.Text = "&Envoyer sélection"
        btnSel.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(407, 381)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 2
        btnAnnuler.Text = "&Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' btnSansSelection
        ' 
        btnSansSelection.Location = New Point(215, 382)
        btnSansSelection.Name = "btnSansSelection"
        btnSansSelection.Size = New Size(128, 23)
        btnSansSelection.TabIndex = 3
        btnSansSelection.Text = "Ne rien sélectionner"
        btnSansSelection.UseVisualStyleBackColor = True
        ' 
        ' FrmListe
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnSansSelection)
        Controls.Add(btnAnnuler)
        Controls.Add(btnSel)
        Controls.Add(dgvListe)
        Name = "FrmListe"
        Text = "frmListe"
        CType(dgvListe, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents dgvListe As DataGridView
    Friend WithEvents btnSel As Button
    Friend WithEvents btnAnnuler As Button
    Friend WithEvents btnSansSelection As Button
End Class
