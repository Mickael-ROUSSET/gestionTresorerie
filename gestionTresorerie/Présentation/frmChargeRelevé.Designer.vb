<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmChargeRelevé
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
        components = New ComponentModel.Container()
        Dim fldBrowserDialog As FolderBrowserDialog
        btnOuvreFichier = New Button()
        dgvRelevé = New DataGridView()
        dgVDate = New DataGridViewTextBoxColumn()
        dgVNOte = New DataGridViewTextBoxColumn()
        dgVDébit = New DataGridViewTextBoxColumn()
        dgVCrédit = New DataGridViewTextBoxColumn()
        BindingSource1 = New BindingSource(components)
        BindingSource2 = New BindingSource(components)
        btnOuvreSaisie = New Button()
        fldBrowserDialog = New FolderBrowserDialog()
        CType(dgvRelevé, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnOuvreFichier
        ' 
        btnOuvreFichier.Location = New Point(12, 21)
        btnOuvreFichier.Name = "btnOuvreFichier"
        btnOuvreFichier.Size = New Size(97, 23)
        btnOuvreFichier.TabIndex = 2
        btnOuvreFichier.Text = "Ouvre relevé"
        btnOuvreFichier.UseVisualStyleBackColor = True
        ' 
        ' dgvRelevé
        ' 
        dgvRelevé.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvRelevé.Columns.AddRange(New DataGridViewColumn() {dgVDate, dgVNOte, dgVDébit, dgVCrédit})
        dgvRelevé.Location = New Point(12, 33)
        dgvRelevé.Name = "dgvRelevé"
        dgvRelevé.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvRelevé.Size = New Size(976, 364)
        dgvRelevé.TabIndex = 3
        ' 
        ' dgVDate
        ' 
        dgVDate.HeaderText = "Date"
        dgVDate.Name = "dgVDate"
        dgVDate.Resizable = DataGridViewTriState.True
        dgVDate.Width = 75
        ' 
        ' dgVNOte
        ' 
        dgVNOte.HeaderText = "Note"
        dgVNOte.MinimumWidth = 50
        dgVNOte.Name = "dgVNOte"
        dgVNOte.Width = 500
        ' 
        ' dgVDébit
        ' 
        dgVDébit.HeaderText = "Débit"
        dgVDébit.MinimumWidth = 50
        dgVDébit.Name = "dgVDébit"
        dgVDébit.Width = 75
        ' 
        ' dgVCrédit
        ' 
        dgVCrédit.HeaderText = "Crédit"
        dgVCrédit.MinimumWidth = 50
        dgVCrédit.Name = "dgVCrédit"
        dgVCrédit.Width = 75
        ' 
        ' btnOuvreSaisie
        ' 
        btnOuvreSaisie.Location = New Point(310, 451)
        btnOuvreSaisie.Name = "btnOuvreSaisie"
        btnOuvreSaisie.Size = New Size(182, 23)
        btnOuvreSaisie.TabIndex = 4
        btnOuvreSaisie.Text = "Enregistrement du mouvement"
        btnOuvreSaisie.UseVisualStyleBackColor = True
        ' 
        ' FrmChargeRelevé
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1000, 506)
        Controls.Add(btnOuvreSaisie)
        Controls.Add(dgvRelevé)
        Controls.Add(btnOuvreFichier)
        Name = "FrmChargeRelevé"
        Text = "Charge Relevé"
        CType(dgvRelevé, ComponentModel.ISupportInitialize).EndInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(BindingSource2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents btnOuvreFichier As Button
    Friend WithEvents BindingSource1 As BindingSource
    Friend WithEvents BindingSource2 As BindingSource
    Friend WithEvents btnOuvreSaisie As Button
    Friend WithEvents dgVDate As DataGridViewTextBoxColumn
    Friend WithEvents dgVNOte As DataGridViewTextBoxColumn
    Friend WithEvents dgVDébit As DataGridViewTextBoxColumn
    Friend WithEvents dgVCrédit As DataGridViewTextBoxColumn
    Public WithEvents dgvRelevé As DataGridView
End Class
