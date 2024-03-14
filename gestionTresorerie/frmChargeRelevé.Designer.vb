<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmChargeRelevé
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
        components = New ComponentModel.Container()
        Dim fldBrowserDialog As FolderBrowserDialog
        lstMouvements = New ListView()
        btnOuvreFichier = New Button()
        dgvRelevé = New DataGridView()
        dgVDate = New DataGridViewTextBoxColumn()
        dgVNOte = New DataGridViewTextBoxColumn()
        dgVCrédit = New DataGridViewTextBoxColumn()
        dgVDébit = New DataGridViewTextBoxColumn()
        BindingSource1 = New BindingSource(components)
        BindingSource2 = New BindingSource(components)
        fldBrowserDialog = New FolderBrowserDialog()
        CType(dgvRelevé, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lstMouvements
        ' 
        lstMouvements.Location = New Point(105, 51)
        lstMouvements.Name = "lstMouvements"
        lstMouvements.ShowItemToolTips = True
        lstMouvements.Size = New Size(664, 62)
        lstMouvements.TabIndex = 1
        lstMouvements.UseCompatibleStateImageBehavior = False
        ' 
        ' btnOuvreFichier
        ' 
        btnOuvreFichier.Location = New Point(12, 51)
        btnOuvreFichier.Name = "btnOuvreFichier"
        btnOuvreFichier.Size = New Size(75, 23)
        btnOuvreFichier.TabIndex = 2
        btnOuvreFichier.Text = "Ouvre relevé"
        btnOuvreFichier.UseVisualStyleBackColor = True
        ' 
        ' dgvRelevé
        ' 
        dgvRelevé.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvRelevé.Columns.AddRange(New DataGridViewColumn() {dgVDate, dgVNOte, dgVCrédit, dgVDébit})
        dgvRelevé.Location = New Point(12, 136)
        dgvRelevé.Name = "dgvRelevé"
        dgvRelevé.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvRelevé.Size = New Size(762, 218)
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
        ' dgVCrédit
        ' 
        dgVCrédit.HeaderText = "Crédit"
        dgVCrédit.MinimumWidth = 50
        dgVCrédit.Name = "dgVCrédit"
        dgVCrédit.Width = 75
        ' 
        ' dgVDébit
        ' 
        dgVDébit.HeaderText = "Débit"
        dgVDébit.MinimumWidth = 50
        dgVDébit.Name = "dgVDébit"
        dgVDébit.Width = 75
        ' 
        ' BindingSource1
        ' 
        ' 
        ' frmChargeRelevé
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(dgvRelevé)
        Controls.Add(btnOuvreFichier)
        Controls.Add(lstMouvements)
        Name = "frmChargeRelevé"
        Text = "Charge Relevé"
        CType(dgvRelevé, ComponentModel.ISupportInitialize).EndInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).EndInit()
        CType(BindingSource2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents lstMouvements As ListView
    Friend WithEvents btnOuvreFichier As Button
    Friend WithEvents dgvRelevé As DataGridView
    Friend WithEvents dgVDate As DataGridViewTextBoxColumn
    Friend WithEvents dgVNOte As DataGridViewTextBoxColumn
    Friend WithEvents dgVCrédit As DataGridViewTextBoxColumn
    Friend WithEvents dgVDébit As DataGridViewTextBoxColumn
    Friend WithEvents BindingSource1 As BindingSource
    Friend WithEvents BindingSource2 As BindingSource
End Class
