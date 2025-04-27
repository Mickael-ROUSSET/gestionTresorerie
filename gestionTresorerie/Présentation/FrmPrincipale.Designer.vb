<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmPrincipale
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPrincipale))
        btnSaisie = New Button()
        btnChargeRelevé = New Button()
        btnHistogramme = New Button()
        btnConsultation = New Button()
        dgvPrincipale = New DataGridView()
        MouvementsBindingSource = New BindingSource(components)
        BindingSource1 = New BindingSource(components)
        btnCreeBilans = New Button()
        MenuStrip1 = New MenuStrip()
        FichierToolStripMenuItem = New ToolStripMenuItem()
        ChargerRelevéToolStripMenuItem = New ToolStripMenuItem()
        GénérerBilanToolStripMenuItem = New ToolStripMenuItem()
        FermerToolStripMenuItem = New ToolStripMenuItem()
        AnalyseToolStripMenuItem = New ToolStripMenuItem()
        ConsulterTrésorerieToolStripMenuItem = New ToolStripMenuItem()
        GénérerBilanToolStripMenuItem1 = New ToolStripMenuItem()
        GérerUnMouvementToolStripMenuItem = New ToolStripMenuItem()
        ParamètresToolStripMenuItem = New ToolStripMenuItem()
        RequêteToolStripMenuItem = New ToolStripMenuItem()
        ParamètresTechniquesToolStripMenuItem = New ToolStripMenuItem()
        FichiersParamètresToolStripMenuItem = New ToolStripMenuItem()
        EnvironnementToolStripMenuItem = New ToolStripMenuItem()
        Button1 = New Button()
        btnBatch = New Button()
        btnTraiteRelevé = New Button()
        EtatDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        DateCréationDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        TiersDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        Categorie = New DataGridViewTextBoxColumn()
        SousCategorie = New DataGridViewTextBoxColumn()
        DateMvtDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        MontantDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        SensDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        ÉvénementDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        NoteDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        TypeDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        ModifiableDataGridViewCheckBoxColumn = New DataGridViewTextBoxColumn()
        NumeroRemiseDataGridViewTextBoxColumn = New DataGridViewTextBoxColumn()
        CType(dgvPrincipale, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnSaisie
        ' 
        btnSaisie.Location = New Point(23, 309)
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
        btnHistogramme.Location = New Point(23, 160)
        btnHistogramme.Name = "btnHistogramme"
        btnHistogramme.Size = New Size(104, 23)
        btnHistogramme.TabIndex = 2
        btnHistogramme.Text = "Histogramme"
        btnHistogramme.UseVisualStyleBackColor = True
        ' 
        ' btnConsultation
        ' 
        btnConsultation.AccessibleDescription = resources.GetString("btnConsultation.AccessibleDescription")
        btnConsultation.Location = New Point(23, 228)
        btnConsultation.Name = "btnConsultation"
        btnConsultation.Size = New Size(104, 23)
        btnConsultation.TabIndex = 3
        btnConsultation.Text = "Consultation"
        btnConsultation.UseVisualStyleBackColor = True
        ' 
        ' dgvPrincipale
        ' 
        dgvPrincipale.AutoGenerateColumns = False
        dgvPrincipale.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvPrincipale.Columns.AddRange(New DataGridViewColumn() {EtatDataGridViewTextBoxColumn, DateCréationDataGridViewTextBoxColumn, TiersDataGridViewTextBoxColumn, Categorie, SousCategorie, DateMvtDataGridViewTextBoxColumn, MontantDataGridViewTextBoxColumn, SensDataGridViewTextBoxColumn, ÉvénementDataGridViewTextBoxColumn, NoteDataGridViewTextBoxColumn, TypeDataGridViewTextBoxColumn, ModifiableDataGridViewCheckBoxColumn, NumeroRemiseDataGridViewTextBoxColumn})
        dgvPrincipale.DataBindings.Add(New Binding("DataContext", MouvementsBindingSource, "note", True))
        dgvPrincipale.DataSource = MouvementsBindingSource
        dgvPrincipale.Location = New Point(143, 67)
        dgvPrincipale.Name = "dgvPrincipale"
        dgvPrincipale.Size = New Size(798, 370)
        dgvPrincipale.TabIndex = 4
        ' 
        ' MouvementsBindingSource
        ' 
        MouvementsBindingSource.DataSource = GetType(Mouvements)
        ' 
        ' btnCreeBilans
        ' 
        btnCreeBilans.Location = New Point(23, 257)
        btnCreeBilans.Name = "btnCreeBilans"
        btnCreeBilans.Size = New Size(75, 23)
        btnCreeBilans.TabIndex = 5
        btnCreeBilans.Text = "Crée Bilans"
        btnCreeBilans.UseVisualStyleBackColor = True
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {FichierToolStripMenuItem, AnalyseToolStripMenuItem, ParamètresToolStripMenuItem, ParamètresTechniquesToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(966, 24)
        MenuStrip1.TabIndex = 6
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FichierToolStripMenuItem
        ' 
        FichierToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ChargerRelevéToolStripMenuItem, GénérerBilanToolStripMenuItem, FermerToolStripMenuItem})
        FichierToolStripMenuItem.Name = "FichierToolStripMenuItem"
        FichierToolStripMenuItem.Size = New Size(54, 20)
        FichierToolStripMenuItem.Text = "&Fichier"
        ' 
        ' ChargerRelevéToolStripMenuItem
        ' 
        ChargerRelevéToolStripMenuItem.Name = "ChargerRelevéToolStripMenuItem"
        ChargerRelevéToolStripMenuItem.Size = New Size(153, 22)
        ChargerRelevéToolStripMenuItem.Text = "Charger &Relevé"
        ' 
        ' GénérerBilanToolStripMenuItem
        ' 
        GénérerBilanToolStripMenuItem.Name = "GénérerBilanToolStripMenuItem"
        GénérerBilanToolStripMenuItem.Size = New Size(153, 22)
        GénérerBilanToolStripMenuItem.Text = "Générer &bilan"
        ' 
        ' FermerToolStripMenuItem
        ' 
        FermerToolStripMenuItem.Name = "FermerToolStripMenuItem"
        FermerToolStripMenuItem.Size = New Size(153, 22)
        FermerToolStripMenuItem.Text = "&Fermer"
        ' 
        ' AnalyseToolStripMenuItem
        ' 
        AnalyseToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ConsulterTrésorerieToolStripMenuItem, GénérerBilanToolStripMenuItem1, GérerUnMouvementToolStripMenuItem})
        AnalyseToolStripMenuItem.Name = "AnalyseToolStripMenuItem"
        AnalyseToolStripMenuItem.Size = New Size(60, 20)
        AnalyseToolStripMenuItem.Text = "Analyse"
        ' 
        ' ConsulterTrésorerieToolStripMenuItem
        ' 
        ConsulterTrésorerieToolStripMenuItem.Name = "ConsulterTrésorerieToolStripMenuItem"
        ConsulterTrésorerieToolStripMenuItem.Size = New Size(187, 22)
        ConsulterTrésorerieToolStripMenuItem.Text = "Consulter &trésorerie"
        ' 
        ' GénérerBilanToolStripMenuItem1
        ' 
        GénérerBilanToolStripMenuItem1.Name = "GénérerBilanToolStripMenuItem1"
        GénérerBilanToolStripMenuItem1.Size = New Size(187, 22)
        GénérerBilanToolStripMenuItem1.Text = "Générer bilan"
        ' 
        ' GérerUnMouvementToolStripMenuItem
        ' 
        GérerUnMouvementToolStripMenuItem.Name = "GérerUnMouvementToolStripMenuItem"
        GérerUnMouvementToolStripMenuItem.Size = New Size(187, 22)
        GérerUnMouvementToolStripMenuItem.Text = "Gérer un &mouvement"
        ' 
        ' ParamètresToolStripMenuItem
        ' 
        ParamètresToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {RequêteToolStripMenuItem})
        ParamètresToolStripMenuItem.Name = "ParamètresToolStripMenuItem"
        ParamètresToolStripMenuItem.Size = New Size(147, 20)
        ParamètresToolStripMenuItem.Text = "Paramètres fonctionnels"
        ' 
        ' RequêteToolStripMenuItem
        ' 
        RequêteToolStripMenuItem.Name = "RequêteToolStripMenuItem"
        RequêteToolStripMenuItem.Size = New Size(117, 22)
        RequêteToolStripMenuItem.Text = "Requête"
        ' 
        ' ParamètresTechniquesToolStripMenuItem
        ' 
        ParamètresTechniquesToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {FichiersParamètresToolStripMenuItem, EnvironnementToolStripMenuItem})
        ParamètresTechniquesToolStripMenuItem.Name = "ParamètresTechniquesToolStripMenuItem"
        ParamètresTechniquesToolStripMenuItem.Size = New Size(139, 20)
        ParamètresTechniquesToolStripMenuItem.Text = "Paramètres techniques"
        ' 
        ' FichiersParamètresToolStripMenuItem
        ' 
        FichiersParamètresToolStripMenuItem.Name = "FichiersParamètresToolStripMenuItem"
        FichiersParamètresToolStripMenuItem.Size = New Size(176, 22)
        FichiersParamètresToolStripMenuItem.Text = "Fichiers paramètres"
        ' 
        ' EnvironnementToolStripMenuItem
        ' 
        EnvironnementToolStripMenuItem.Name = "EnvironnementToolStripMenuItem"
        EnvironnementToolStripMenuItem.Size = New Size(176, 22)
        EnvironnementToolStripMenuItem.Text = "Environnement"
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(48, 430)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 23)
        Button1.TabIndex = 7
        Button1.Text = "Button1"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' btnBatch
        ' 
        btnBatch.Location = New Point(299, 443)
        btnBatch.Name = "btnBatch"
        btnBatch.Size = New Size(156, 23)
        btnBatch.TabIndex = 8
        btnBatch.Text = "lanceBatchAnalyse"
        btnBatch.TextImageRelation = TextImageRelation.TextBeforeImage
        btnBatch.UseVisualStyleBackColor = True
        ' 
        ' btnTraiteRelevé
        ' 
        btnTraiteRelevé.Location = New Point(29, 120)
        btnTraiteRelevé.Name = "btnTraiteRelevé"
        btnTraiteRelevé.Size = New Size(94, 23)
        btnTraiteRelevé.TabIndex = 9
        btnTraiteRelevé.Text = "Traite relevé"
        btnTraiteRelevé.TextAlign = ContentAlignment.BottomCenter
        btnTraiteRelevé.UseVisualStyleBackColor = True
        ' 
        ' EtatDataGridViewTextBoxColumn
        ' 
        EtatDataGridViewTextBoxColumn.DataPropertyName = "Etat"
        EtatDataGridViewTextBoxColumn.HeaderText = "Etat"
        EtatDataGridViewTextBoxColumn.Name = "EtatDataGridViewTextBoxColumn"
        EtatDataGridViewTextBoxColumn.Resizable = DataGridViewTriState.True
        EtatDataGridViewTextBoxColumn.Visible = False
        ' 
        ' DateCréationDataGridViewTextBoxColumn
        ' 
        DateCréationDataGridViewTextBoxColumn.DataPropertyName = "DateCréation"
        DateCréationDataGridViewTextBoxColumn.HeaderText = "DateCréation"
        DateCréationDataGridViewTextBoxColumn.MaxInputLength = 32
        DateCréationDataGridViewTextBoxColumn.Name = "DateCréationDataGridViewTextBoxColumn"
        DateCréationDataGridViewTextBoxColumn.ReadOnly = True
        ' 
        ' TiersDataGridViewTextBoxColumn
        ' 
        TiersDataGridViewTextBoxColumn.DataPropertyName = "Tiers"
        TiersDataGridViewTextBoxColumn.HeaderText = "Tiers"
        TiersDataGridViewTextBoxColumn.Name = "TiersDataGridViewTextBoxColumn"
        ' 
        ' Categorie
        ' 
        Categorie.DataPropertyName = "Catégorie"
        Categorie.HeaderText = "Catégorie"
        Categorie.Name = "Categorie"
        ' 
        ' SousCategorie
        ' 
        SousCategorie.DataPropertyName = "SousCatégorie"
        SousCategorie.HeaderText = "SousCatégorie"
        SousCategorie.Name = "SousCategorie"
        ' 
        ' DateMvtDataGridViewTextBoxColumn
        ' 
        DateMvtDataGridViewTextBoxColumn.DataPropertyName = "DateMvt"
        DateMvtDataGridViewTextBoxColumn.HeaderText = "DateMvt"
        DateMvtDataGridViewTextBoxColumn.Name = "DateMvtDataGridViewTextBoxColumn"
        ' 
        ' MontantDataGridViewTextBoxColumn
        ' 
        MontantDataGridViewTextBoxColumn.DataPropertyName = "Montant"
        MontantDataGridViewTextBoxColumn.HeaderText = "Montant"
        MontantDataGridViewTextBoxColumn.Name = "MontantDataGridViewTextBoxColumn"
        ' 
        ' SensDataGridViewTextBoxColumn
        ' 
        SensDataGridViewTextBoxColumn.DataPropertyName = "Sens"
        SensDataGridViewTextBoxColumn.HeaderText = "Sens"
        SensDataGridViewTextBoxColumn.Name = "SensDataGridViewTextBoxColumn"
        ' 
        ' ÉvénementDataGridViewTextBoxColumn
        ' 
        ÉvénementDataGridViewTextBoxColumn.DataPropertyName = "Événement"
        ÉvénementDataGridViewTextBoxColumn.HeaderText = "Événement"
        ÉvénementDataGridViewTextBoxColumn.Name = "ÉvénementDataGridViewTextBoxColumn"
        ' 
        ' NoteDataGridViewTextBoxColumn
        ' 
        NoteDataGridViewTextBoxColumn.DataPropertyName = "Note"
        NoteDataGridViewTextBoxColumn.HeaderText = "Note"
        NoteDataGridViewTextBoxColumn.Name = "NoteDataGridViewTextBoxColumn"
        ' 
        ' TypeDataGridViewTextBoxColumn
        ' 
        TypeDataGridViewTextBoxColumn.DataPropertyName = "Type"
        TypeDataGridViewTextBoxColumn.HeaderText = "Type"
        TypeDataGridViewTextBoxColumn.Name = "TypeDataGridViewTextBoxColumn"
        ' 
        ' ModifiableDataGridViewCheckBoxColumn
        ' 
        ModifiableDataGridViewCheckBoxColumn.DataPropertyName = "Modifiable"
        ModifiableDataGridViewCheckBoxColumn.HeaderText = "Modifiable"
        ModifiableDataGridViewCheckBoxColumn.Name = "ModifiableDataGridViewCheckBoxColumn"
        ModifiableDataGridViewCheckBoxColumn.Resizable = DataGridViewTriState.True
        ModifiableDataGridViewCheckBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable
        ' 
        ' NumeroRemiseDataGridViewTextBoxColumn
        ' 
        NumeroRemiseDataGridViewTextBoxColumn.DataPropertyName = "NumeroRemise"
        NumeroRemiseDataGridViewTextBoxColumn.HeaderText = "NumeroRemise"
        NumeroRemiseDataGridViewTextBoxColumn.Name = "NumeroRemiseDataGridViewTextBoxColumn"
        ' 
        ' FrmPrincipale
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(966, 541)
        Controls.Add(btnTraiteRelevé)
        Controls.Add(btnBatch)
        Controls.Add(Button1)
        Controls.Add(btnCreeBilans)
        Controls.Add(dgvPrincipale)
        Controls.Add(btnConsultation)
        Controls.Add(btnHistogramme)
        Controls.Add(btnChargeRelevé)
        Controls.Add(btnSaisie)
        Controls.Add(MenuStrip1)
        MainMenuStrip = MenuStrip1
        Name = "FrmPrincipale"
        Text = "Gestion des la  trésoreire de l'AGUMAAA"
        CType(dgvPrincipale, ComponentModel.ISupportInitialize).EndInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).EndInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).EndInit()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents btnSaisie As Button
    Friend WithEvents btnChargeRelevé As Button
    Friend WithEvents btnHistogramme As Button
    Friend WithEvents btnConsultation As Button
    Friend WithEvents dgvPrincipale As DataGridView
    Friend WithEvents MouvementsBindingSource As BindingSource
    Friend WithEvents BindingSource1 As BindingSource
    Friend WithEvents btnCreeBilans As Button
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FichierToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ChargerRelevéToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerBilanToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ParamètresToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AnalyseToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ConsulterTrésorerieToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerBilanToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents RequêteToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ParamètresTechniquesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FichiersParamètresToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents EnvironnementToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GérerUnMouvementToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FermerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Button1 As Button
    Friend WithEvents btnBatch As Button
    Friend WithEvents btnTraiteRelevé As Button
    Friend WithEvents CategorieDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents SousCategorieDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents EtatDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents DateCréationDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents TiersDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents Categorie As DataGridViewTextBoxColumn
    Friend WithEvents SousCategorie As DataGridViewTextBoxColumn
    Friend WithEvents DateMvtDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents MontantDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents SensDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents ÉvénementDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents NoteDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents TypeDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents ModifiableDataGridViewCheckBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents NumeroRemiseDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
End Class
