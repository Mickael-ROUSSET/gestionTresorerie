<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmSelectionGenerique
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
        dgvResultats = New DataGridView()
        Panel1 = New Panel()
        btnAnnuler = New Button()
        btnOK = New Button()
        txtFiltre = New TextBox()
        lblStatus = New Label()
        btnActualiser = New Button()
        CType(dgvResultats, ComponentModel.ISupportInitialize).BeginInit()
        Panel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' dgvResultats
        ' 
        dgvResultats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvResultats.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvResultats.Location = New Point(29, 32)
        dgvResultats.Name = "dgvResultats"
        dgvResultats.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResultats.Size = New Size(759, 200)
        dgvResultats.TabIndex = 0
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(btnAnnuler)
        Panel1.Controls.Add(btnOK)
        Panel1.Location = New Point(42, 273)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(321, 49)
        Panel1.TabIndex = 1
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(158, 13)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 1
        btnAnnuler.Text = "Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' btnOK
        ' 
        btnOK.Location = New Point(17, 13)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(99, 23)
        btnOK.TabIndex = 0
        btnOK.Text = "Sélectionner"
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' txtFiltre
        ' 
        txtFiltre.Location = New Point(215, 3)
        txtFiltre.Name = "txtFiltre"
        txtFiltre.Size = New Size(100, 23)
        txtFiltre.TabIndex = 2
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(37, 356)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(101, 15)
        lblStatus.TabIndex = 3
        lblStatus.Text = "Nombre de lignes"
        ' 
        ' btnActualiser
        ' 
        btnActualiser.Location = New Point(39, 385)
        btnActualiser.Name = "btnActualiser"
        btnActualiser.Size = New Size(75, 23)
        btnActualiser.TabIndex = 4
        btnActualiser.Text = "Actualiser"
        btnActualiser.UseVisualStyleBackColor = True
        ' 
        ' FrmSelectionGenerique
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnActualiser)
        Controls.Add(lblStatus)
        Controls.Add(txtFiltre)
        Controls.Add(Panel1)
        Controls.Add(dgvResultats)
        Name = "FrmSelectionGenerique"
        Text = "Form1"
        CType(dgvResultats, ComponentModel.ISupportInitialize).EndInit()
        Panel1.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents dgvResultats As DataGridView
    Friend WithEvents Panel1 As Panel
    Friend WithEvents btnAnnuler As Button
    Friend WithEvents btnOK As Button
    Friend WithEvents txtFiltre As TextBox
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnActualiser As Button
End Class
