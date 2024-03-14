<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPrincipale
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPrincipale))
        btnSaisie = New Button()
        btnChargeRelevé = New Button()
        btnHistogramme = New Button()
        btnConsultation = New Button()
        DataGridView1 = New DataGridView()
        NoteDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        CategorieDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        SousCategorieDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        TiersDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        DateCréationDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        DateMvtDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        MontantDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        SensDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        EtatDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        ÉvénementDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        TypeDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        ModifiableDataGridViewCheckBoxColumn = New DataGridViewCheckBoxColumn()
        NumeroRemiseDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        MouvementsBindingSource = New BindingSource(components)
        BindingSource1 = New BindingSource(components)
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnSaisie
        ' 
        btnSaisie.Location = New Point(23, 12)
        btnSaisie.Name = "btnSaisie"
        btnSaisie.Size = New Size(104, 23)
        btnSaisie.TabIndex = 0
        btnSaisie.Text = "Saisie mouvement"
        btnSaisie.UseVisualStyleBackColor = True
        ' 
        ' btnChargeRelevé
        ' 
        btnChargeRelevé.AccessibleDescription = resources.GetString("btnChargeRelevé.AccessibleDescription")
        btnChargeRelevé.Location = New Point(23, 67)
        btnChargeRelevé.Name = "btnChargeRelevé"
        btnChargeRelevé.Size = New Size(104, 23)
        btnChargeRelevé.TabIndex = 1
        btnChargeRelevé.Text = "Charge relevé"
        btnChargeRelevé.UseVisualStyleBackColor = True
        ' 
        ' btnHistogramme
        ' 
        btnHistogramme.AccessibleDescription = resources.GetString("btnHistogramme.AccessibleDescription")
        btnHistogramme.Location = New Point(23, 124)
        btnHistogramme.Name = "btnHistogramme"
        btnHistogramme.Size = New Size(104, 23)
        btnHistogramme.TabIndex = 2
        btnHistogramme.Text = "Histogramme"
        btnHistogramme.UseVisualStyleBackColor = True
        ' 
        ' btnConsultation
        ' 
        btnConsultation.AccessibleDescription = resources.GetString("btnConsultation.AccessibleDescription")
        btnConsultation.Location = New Point(23, 189)
        btnConsultation.Name = "btnConsultation"
        btnConsultation.Size = New Size(104, 23)
        btnConsultation.TabIndex = 3
        btnConsultation.Text = "Consultation"
        btnConsultation.UseVisualStyleBackColor = True
        ' 
        ' DataGridView1
        ' 
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Columns.AddRange(New DataGridViewColumn() {NoteDataGridViewTextBoxColumn, CategorieDataGridViewTextBoxColumn, SousCategorieDataGridViewTextBoxColumn, TiersDataGridViewTextBoxColumn, DateCréationDataGridViewTextBoxColumn, DateMvtDataGridViewTextBoxColumn, MontantDataGridViewTextBoxColumn, SensDataGridViewTextBoxColumn, EtatDataGridViewTextBoxColumn, ÉvénementDataGridViewTextBoxColumn, TypeDataGridViewTextBoxColumn, ModifiableDataGridViewCheckBoxColumn, NumeroRemiseDataGridViewTextBoxColumn})
        DataGridView1.DataBindings.Add(New Binding("DataContext", MouvementsBindingSource, "note", True))
        DataGridView1.DataSource = MouvementsBindingSource
        DataGridView1.Location = New Point(133, 86)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(798, 297)
        DataGridView1.TabIndex = 4
        ' 
        ' NoteDataGridViewTextBoxColumn
        ' 
        NoteDataGridViewTextBoxColumn.DataPropertyName = "note"
        NoteDataGridViewTextBoxColumn.HeaderText = "note"
        NoteDataGridViewTextBoxColumn.Name = "NoteDataGridViewTextBoxColumn"
        ' 
        ' CategorieDataGridViewTextBoxColumn
        ' 
        CategorieDataGridViewTextBoxColumn.DataPropertyName = "categorie"
        CategorieDataGridViewTextBoxColumn.HeaderText = "categorie"
        CategorieDataGridViewTextBoxColumn.Name = "CategorieDataGridViewTextBoxColumn"
        ' 
        ' SousCategorieDataGridViewTextBoxColumn
        ' 
        SousCategorieDataGridViewTextBoxColumn.DataPropertyName = "sousCategorie"
        SousCategorieDataGridViewTextBoxColumn.HeaderText = "sousCategorie"
        SousCategorieDataGridViewTextBoxColumn.Name = "SousCategorieDataGridViewTextBoxColumn"
        ' 
        ' TiersDataGridViewTextBoxColumn
        ' 
        TiersDataGridViewTextBoxColumn.DataPropertyName = "tiers"
        TiersDataGridViewTextBoxColumn.HeaderText = "tiers"
        TiersDataGridViewTextBoxColumn.Name = "TiersDataGridViewTextBoxColumn"
        ' 
        ' DateCréationDataGridViewTextBoxColumn
        ' 
        DateCréationDataGridViewTextBoxColumn.DataPropertyName = "dateCréation"
        DateCréationDataGridViewTextBoxColumn.HeaderText = "dateCréation"
        DateCréationDataGridViewTextBoxColumn.Name = "DateCréationDataGridViewTextBoxColumn"
        ' 
        ' DateMvtDataGridViewTextBoxColumn
        ' 
        DateMvtDataGridViewTextBoxColumn.DataPropertyName = "dateMvt"
        DateMvtDataGridViewTextBoxColumn.HeaderText = "dateMvt"
        DateMvtDataGridViewTextBoxColumn.Name = "DateMvtDataGridViewTextBoxColumn"
        ' 
        ' MontantDataGridViewTextBoxColumn
        ' 
        MontantDataGridViewTextBoxColumn.DataPropertyName = "montant"
        MontantDataGridViewTextBoxColumn.HeaderText = "montant"
        MontantDataGridViewTextBoxColumn.Name = "MontantDataGridViewTextBoxColumn"
        ' 
        ' SensDataGridViewTextBoxColumn
        ' 
        SensDataGridViewTextBoxColumn.DataPropertyName = "sens"
        SensDataGridViewTextBoxColumn.HeaderText = "sens"
        SensDataGridViewTextBoxColumn.Name = "SensDataGridViewTextBoxColumn"
        ' 
        ' EtatDataGridViewTextBoxColumn
        ' 
        EtatDataGridViewTextBoxColumn.DataPropertyName = "etat"
        EtatDataGridViewTextBoxColumn.HeaderText = "etat"
        EtatDataGridViewTextBoxColumn.Name = "EtatDataGridViewTextBoxColumn"
        ' 
        ' ÉvénementDataGridViewTextBoxColumn
        ' 
        ÉvénementDataGridViewTextBoxColumn.DataPropertyName = "événement"
        ÉvénementDataGridViewTextBoxColumn.HeaderText = "événement"
        ÉvénementDataGridViewTextBoxColumn.Name = "ÉvénementDataGridViewTextBoxColumn"
        ' 
        ' TypeDataGridViewTextBoxColumn
        ' 
        TypeDataGridViewTextBoxColumn.DataPropertyName = "type"
        TypeDataGridViewTextBoxColumn.HeaderText = "type"
        TypeDataGridViewTextBoxColumn.Name = "TypeDataGridViewTextBoxColumn"
        ' 
        ' ModifiableDataGridViewCheckBoxColumn
        ' 
        ModifiableDataGridViewCheckBoxColumn.DataPropertyName = "modifiable"
        ModifiableDataGridViewCheckBoxColumn.HeaderText = "modifiable"
        ModifiableDataGridViewCheckBoxColumn.Name = "ModifiableDataGridViewCheckBoxColumn"
        ' 
        ' NumeroRemiseDataGridViewTextBoxColumn
        ' 
        NumeroRemiseDataGridViewTextBoxColumn.DataPropertyName = "numeroRemise"
        NumeroRemiseDataGridViewTextBoxColumn.HeaderText = "numeroRemise"
        NumeroRemiseDataGridViewTextBoxColumn.Name = "NumeroRemiseDataGridViewTextBoxColumn"
        ' 
        ' MouvementsBindingSource
        ' 
        MouvementsBindingSource.DataSource = GetType(Mouvements)
        ' 
        ' BindingSource1
        ' 
        ' 
        ' frmPrincipale
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(966, 541)
        Controls.Add(DataGridView1)
        Controls.Add(btnConsultation)
        Controls.Add(btnHistogramme)
        Controls.Add(btnChargeRelevé)
        Controls.Add(btnSaisie)
        Name = "frmPrincipale"
        Text = "Gestion des la  trésoreire de l'AGUMAAA"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnSaisie As Button
    Friend WithEvents btnChargeRelevé As Button
    Friend WithEvents btnHistogramme As Button
    Friend WithEvents btnConsultation As Button
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents NoteDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents CategorieDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents SousCategorieDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents TiersDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents DateCréationDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents DateMvtDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents MontantDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents SensDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents EtatDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents ÉvénementDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents TypeDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents ModifiableDataGridViewCheckBoxColumn As DataGridViewCheckBoxColumn
    Friend WithEvents NumeroRemiseDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents MouvementsBindingSource As BindingSource
    Friend WithEvents BindingSource1 As BindingSource
End Class
