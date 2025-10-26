<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmRemiseChq
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
        dgvRemiseChq = New DataGridView()
        Id = New DataGridViewTextBoxColumn()
        Nom = New DataGridViewComboBoxColumn()
        Prénom = New DataGridViewComboBoxColumn()
        RaisonSociale = New DataGridViewComboBoxColumn()
        btnAjoutTiers = New Button()
        btnInsereMvts = New Button()
        btnAnnuler = New Button()
        CType(dgvRemiseChq, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' dgvRemiseChq
        ' 
        dgvRemiseChq.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvRemiseChq.Columns.AddRange(New DataGridViewColumn() {Id, Nom, Prénom, RaisonSociale})
        dgvRemiseChq.Location = New Point(54, 116)
        dgvRemiseChq.Name = "dgvRemiseChq"
        dgvRemiseChq.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvRemiseChq.Size = New Size(553, 208)
        dgvRemiseChq.TabIndex = 0
        ' 
        ' Id
        ' 
        Id.HeaderText = "Id"
        Id.Name = "Id"
        Id.Visible = False
        ' 
        ' Nom
        ' 
        Nom.HeaderText = "Nom"
        Nom.Name = "Nom"
        ' 
        ' Prénom
        ' 
        Prénom.HeaderText = "Prénom"
        Prénom.Name = "Prénom"
        ' 
        ' RaisonSociale
        ' 
        RaisonSociale.HeaderText = "Raison Sociale"
        RaisonSociale.Name = "RaisonSociale"
        ' 
        ' btnAjoutTiers
        ' 
        btnAjoutTiers.Location = New Point(659, 121)
        btnAjoutTiers.Name = "btnAjoutTiers"
        btnAjoutTiers.Size = New Size(75, 23)
        btnAjoutTiers.TabIndex = 1
        btnAjoutTiers.Text = "&Créer tiers"
        btnAjoutTiers.UseVisualStyleBackColor = True
        ' 
        ' btnInsereMvts
        ' 
        btnInsereMvts.Location = New Point(59, 380)
        btnInsereMvts.Name = "btnInsereMvts"
        btnInsereMvts.Size = New Size(156, 23)
        btnInsereMvts.TabIndex = 2
        btnInsereMvts.Text = "&Insertion Mouvements"
        btnInsereMvts.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(450, 380)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 3
        btnAnnuler.Text = "&Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' FrmRemiseChq
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnAnnuler)
        Controls.Add(btnInsereMvts)
        Controls.Add(btnAjoutTiers)
        Controls.Add(dgvRemiseChq)
        Name = "FrmRemiseChq"
        Text = "FrmRemiseChq"
        CType(dgvRemiseChq, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents dgvRemiseChq As DataGridView
    Friend WithEvents btnAjoutTiers As Button
    Friend WithEvents btnInsereMvts As Button
    Friend WithEvents btnAnnuler As Button
    Friend WithEvents Id As DataGridViewTextBoxColumn
    Friend WithEvents Nom As DataGridViewComboBoxColumn
    Friend WithEvents Prénom As DataGridViewComboBoxColumn
    Friend WithEvents RaisonSociale As DataGridViewComboBoxColumn
End Class
