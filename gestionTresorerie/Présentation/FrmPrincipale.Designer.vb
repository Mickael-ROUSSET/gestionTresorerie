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
        colEtat = New DataGridViewImageColumn()
        etatImage = New DataGridViewTextBoxColumn()
        EtatMasque = New DataGridViewTextBoxColumn()
        colDateCréation = New DataGridViewTextBoxColumn()
        colTiers = New DataGridViewTextBoxColumn()
        colCategorie = New DataGridViewTextBoxColumn()
        colSousCategorie = New DataGridViewTextBoxColumn()
        colDateMvt = New DataGridViewTextBoxColumn()
        colMontant = New DataGridViewTextBoxColumn()
        colSens = New DataGridViewTextBoxColumn()
        colEvenement = New DataGridViewTextBoxColumn()
        colNote = New DataGridViewTextBoxColumn()
        colType = New DataGridViewTextBoxColumn()
        colModifiable = New DataGridViewTextBoxColumn()
        colNumeroRemise = New DataGridViewTextBoxColumn()
        reference = New DataGridViewTextBoxColumn()
        typeReference = New DataGridViewTextBoxColumn()
        idDoc = New DataGridViewTextBoxColumn()
        MouvementsBindingSource = New BindingSource(components)
        BindingSource1 = New BindingSource(components)
        MenuStrip1 = New MenuStrip()
        FichierToolStripMenuItem = New ToolStripMenuItem()
        ChargerRelevéToolStripMenuItem = New ToolStripMenuItem()
        GénérerBilanToolStripMenuItem = New ToolStripMenuItem()
        FermerToolStripMenuItem = New ToolStripMenuItem()
        AnalyseToolStripMenuItem = New ToolStripMenuItem()
        ConsulterTrésorerieToolStripMenuItem = New ToolStripMenuItem()
        GénérerBilanToolStripMenuItem1 = New ToolStripMenuItem()
        GérerUnMouvementToolStripMenuItem = New ToolStripMenuItem()
        DocumentsToolStripMenuItem = New ToolStripMenuItem()
        GestionBDDToolStripMenuItem = New ToolStripMenuItem()
        SauvegarderToolStripMenuItem = New ToolStripMenuItem()
        RestaurerToolStripMenuItem = New ToolStripMenuItem()
        GestionUtilisateurToolStripMenuItem = New ToolStripMenuItem()
        ConsoleToolStripMenuItem = New ToolStripMenuItem()
        ChangeMdPToolStripMenuItem = New ToolStripMenuItem()
        BatchToolStripMenuItem = New ToolStripMenuItem()
        AnalyseDocumentsToolStripMenuItem = New ToolStripMenuItem()
        ParamètresToolStripMenuItem = New ToolStripMenuItem()
        RequêteToolStripMenuItem = New ToolStripMenuItem()
        ParamètresTechniquesToolStripMenuItem = New ToolStripMenuItem()
        EnvironnementToolStripMenuItem = New ToolStripMenuItem()
        AgentMistralToolStripMenuItem = New ToolStripMenuItem()
        RecréerToolStripMenuItem = New ToolStripMenuItem()
        CréerToolStripMenuItem = New ToolStripMenuItem()
        ParamètresToolStripMenuItem1 = New ToolStripMenuItem()
        FichiersParamètresToolStripMenuItem = New ToolStripMenuItem()
        ParamètresTechniquesToolStripMenuItem1 = New ToolStripMenuItem()
        btnTraiteRelevé = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        CType(dgvPrincipale, ComponentModel.ISupportInitialize).BeginInit()
        CType(MouvementsBindingSource, ComponentModel.ISupportInitialize).BeginInit()
        CType(BindingSource1, ComponentModel.ISupportInitialize).BeginInit()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnSaisie
        ' 
        btnSaisie.Location = New Point(29, 295)
        btnSaisie.Name = "btnSaisie"
        btnSaisie.Size = New Size(94, 23)
        btnSaisie.TabIndex = 0
        btnSaisie.Text = "Saisie mouvement"
        btnSaisie.UseVisualStyleBackColor = True
        ' 
        ' btnChargeRelevé
        ' 
        btnChargeRelevé.AccessibleDescription = resources.GetString("btnChargeRelevé.AccessibleDescription")
        btnChargeRelevé.Location = New Point(29, 67)
        btnChargeRelevé.Name = "btnChargeRelevé"
        btnChargeRelevé.Size = New Size(94, 23)
        btnChargeRelevé.TabIndex = 1
        btnChargeRelevé.Text = "Charge relevé"
        btnChargeRelevé.UseVisualStyleBackColor = True
        ' 
        ' btnHistogramme
        ' 
        btnHistogramme.AccessibleDescription = resources.GetString("btnHistogramme.AccessibleDescription")
        btnHistogramme.Location = New Point(29, 181)
        btnHistogramme.Name = "btnHistogramme"
        btnHistogramme.Size = New Size(94, 23)
        btnHistogramme.TabIndex = 2
        btnHistogramme.Text = "Histogramme"
        btnHistogramme.UseVisualStyleBackColor = True
        ' 
        ' btnConsultation
        ' 
        btnConsultation.AccessibleDescription = resources.GetString("btnConsultation.AccessibleDescription")
        btnConsultation.Location = New Point(29, 238)
        btnConsultation.Name = "btnConsultation"
        btnConsultation.Size = New Size(94, 23)
        btnConsultation.TabIndex = 3
        btnConsultation.Text = "Consultation"
        btnConsultation.UseVisualStyleBackColor = True
        ' 
        ' dgvPrincipale
        ' 
        dgvPrincipale.AutoGenerateColumns = False
        dgvPrincipale.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvPrincipale.Columns.AddRange(New DataGridViewColumn() {colEtat, etatImage, EtatMasque, colDateCréation, colTiers, colCategorie, colSousCategorie, colDateMvt, colMontant, colSens, colEvenement, colNote, colType, colModifiable, colNumeroRemise, reference, typeReference, idDoc})
        dgvPrincipale.DataBindings.Add(New Binding("DataContext", MouvementsBindingSource, "DateCréation", True))
        dgvPrincipale.DataSource = MouvementsBindingSource
        dgvPrincipale.Location = New Point(145, 67)
        dgvPrincipale.Name = "dgvPrincipale"
        dgvPrincipale.Size = New Size(798, 370)
        dgvPrincipale.TabIndex = 4
        ' 
        ' colEtat
        ' 
        colEtat.HeaderText = "État"
        colEtat.ImageLayout = DataGridViewImageCellLayout.Zoom
        colEtat.Name = "colEtat"
        colEtat.SortMode = DataGridViewColumnSortMode.Automatic
        colEtat.Width = 30
        ' 
        ' etatImage
        ' 
        etatImage.HeaderText = "etatImage"
        etatImage.Name = "etatImage"
        ' 
        ' EtatMasque
        ' 
        EtatMasque.DataPropertyName = "Etat"
        EtatMasque.HeaderText = "Etat"
        EtatMasque.Name = "EtatMasque"
        EtatMasque.Resizable = DataGridViewTriState.True
        EtatMasque.Visible = False
        ' 
        ' colDateCréation
        ' 
        colDateCréation.DataPropertyName = "DateCréation"
        colDateCréation.HeaderText = "DateCréation"
        colDateCréation.MaxInputLength = 32
        colDateCréation.Name = "colDateCréation"
        colDateCréation.ReadOnly = True
        ' 
        ' colTiers
        ' 
        colTiers.DataPropertyName = "Tiers"
        colTiers.HeaderText = "Tiers"
        colTiers.Name = "colTiers"
        ' 
        ' colCategorie
        ' 
        colCategorie.DataPropertyName = "Catégorie"
        colCategorie.HeaderText = "Catégorie"
        colCategorie.Name = "colCategorie"
        ' 
        ' colSousCategorie
        ' 
        colSousCategorie.DataPropertyName = "SousCatégorie"
        colSousCategorie.HeaderText = "SousCatégorie"
        colSousCategorie.Name = "colSousCategorie"
        ' 
        ' colDateMvt
        ' 
        colDateMvt.DataPropertyName = "DateMvt"
        colDateMvt.HeaderText = "DateMvt"
        colDateMvt.Name = "colDateMvt"
        ' 
        ' colMontant
        ' 
        colMontant.DataPropertyName = "Montant"
        colMontant.HeaderText = "Montant"
        colMontant.Name = "colMontant"
        ' 
        ' colSens
        ' 
        colSens.DataPropertyName = "Sens"
        colSens.HeaderText = "Sens"
        colSens.Name = "colSens"
        ' 
        ' colEvenement
        ' 
        colEvenement.DataPropertyName = "Événement"
        colEvenement.HeaderText = "Événement"
        colEvenement.Name = "colEvenement"
        ' 
        ' colNote
        ' 
        colNote.DataPropertyName = "Note"
        colNote.HeaderText = "Note"
        colNote.Name = "colNote"
        ' 
        ' colType
        ' 
        colType.DataPropertyName = "Type"
        colType.HeaderText = "Type"
        colType.Name = "colType"
        ' 
        ' colModifiable
        ' 
        colModifiable.DataPropertyName = "Modifiable"
        colModifiable.HeaderText = "Modifiable"
        colModifiable.Name = "colModifiable"
        colModifiable.Resizable = DataGridViewTriState.True
        colModifiable.SortMode = DataGridViewColumnSortMode.NotSortable
        ' 
        ' colNumeroRemise
        ' 
        colNumeroRemise.DataPropertyName = "NumeroRemise"
        colNumeroRemise.HeaderText = "NumeroRemise"
        colNumeroRemise.Name = "colNumeroRemise"
        ' 
        ' reference
        ' 
        reference.DataPropertyName = "DateCréation"
        reference.HeaderText = "Référence"
        reference.Name = "reference"
        ' 
        ' typeReference
        ' 
        typeReference.DataPropertyName = "DateCréation"
        typeReference.HeaderText = "typeRéférence"
        typeReference.Name = "typeReference"
        ' 
        ' idDoc
        ' 
        idDoc.DataPropertyName = "DateCréation"
        idDoc.HeaderText = "idDoc"
        idDoc.Name = "idDoc"
        ' 
        ' MouvementsBindingSource
        ' 
        MouvementsBindingSource.DataSource = GetType(Mouvements)
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.GripStyle = ToolStripGripStyle.Visible
        MenuStrip1.Items.AddRange(New ToolStripItem() {FichierToolStripMenuItem, AnalyseToolStripMenuItem, GestionBDDToolStripMenuItem, GestionUtilisateurToolStripMenuItem, BatchToolStripMenuItem, ParamètresToolStripMenuItem, ParamètresTechniquesToolStripMenuItem, ParamètresToolStripMenuItem1})
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
        AnalyseToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ConsulterTrésorerieToolStripMenuItem, GénérerBilanToolStripMenuItem1, GérerUnMouvementToolStripMenuItem, DocumentsToolStripMenuItem})
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
        ' DocumentsToolStripMenuItem
        ' 
        DocumentsToolStripMenuItem.Name = "DocumentsToolStripMenuItem"
        DocumentsToolStripMenuItem.Size = New Size(187, 22)
        DocumentsToolStripMenuItem.Text = "Documents"
        ' 
        ' GestionBDDToolStripMenuItem
        ' 
        GestionBDDToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {SauvegarderToolStripMenuItem, RestaurerToolStripMenuItem})
        GestionBDDToolStripMenuItem.Name = "GestionBDDToolStripMenuItem"
        GestionBDDToolStripMenuItem.Size = New Size(85, 20)
        GestionBDDToolStripMenuItem.Text = "Gestion BDD"
        ' 
        ' SauvegarderToolStripMenuItem
        ' 
        SauvegarderToolStripMenuItem.Name = "SauvegarderToolStripMenuItem"
        SauvegarderToolStripMenuItem.Size = New Size(139, 22)
        SauvegarderToolStripMenuItem.Text = "Sauvegarder"
        ' 
        ' RestaurerToolStripMenuItem
        ' 
        RestaurerToolStripMenuItem.Name = "RestaurerToolStripMenuItem"
        RestaurerToolStripMenuItem.Size = New Size(139, 22)
        RestaurerToolStripMenuItem.Text = "Restaurer"
        ' 
        ' GestionUtilisateurToolStripMenuItem
        ' 
        GestionUtilisateurToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ConsoleToolStripMenuItem, ChangeMdPToolStripMenuItem})
        GestionUtilisateurToolStripMenuItem.Name = "GestionUtilisateurToolStripMenuItem"
        GestionUtilisateurToolStripMenuItem.Size = New Size(112, 20)
        GestionUtilisateurToolStripMenuItem.Text = "GestionUtilisateur"
        ' 
        ' ConsoleToolStripMenuItem
        ' 
        ConsoleToolStripMenuItem.Name = "ConsoleToolStripMenuItem"
        ConsoleToolStripMenuItem.Size = New Size(180, 22)
        ConsoleToolStripMenuItem.Text = "Console"
        ' 
        ' ChangeMdPToolStripMenuItem
        ' 
        ChangeMdPToolStripMenuItem.Name = "ChangeMdPToolStripMenuItem"
        ChangeMdPToolStripMenuItem.Size = New Size(180, 22)
        ChangeMdPToolStripMenuItem.Text = "Change MdP"
        ' 
        ' BatchToolStripMenuItem
        ' 
        BatchToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {AnalyseDocumentsToolStripMenuItem})
        BatchToolStripMenuItem.Name = "BatchToolStripMenuItem"
        BatchToolStripMenuItem.Size = New Size(49, 20)
        BatchToolStripMenuItem.Text = "Batch"
        ' 
        ' AnalyseDocumentsToolStripMenuItem
        ' 
        AnalyseDocumentsToolStripMenuItem.Name = "AnalyseDocumentsToolStripMenuItem"
        AnalyseDocumentsToolStripMenuItem.Size = New Size(178, 22)
        AnalyseDocumentsToolStripMenuItem.Text = "Analyse documents"
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
        ParamètresTechniquesToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {EnvironnementToolStripMenuItem, AgentMistralToolStripMenuItem})
        ParamètresTechniquesToolStripMenuItem.Name = "ParamètresTechniquesToolStripMenuItem"
        ParamètresTechniquesToolStripMenuItem.Size = New Size(139, 20)
        ParamètresTechniquesToolStripMenuItem.Text = "Paramètres techniques"
        ' 
        ' EnvironnementToolStripMenuItem
        ' 
        EnvironnementToolStripMenuItem.Name = "EnvironnementToolStripMenuItem"
        EnvironnementToolStripMenuItem.Size = New Size(180, 22)
        EnvironnementToolStripMenuItem.Text = "Environnement"
        ' 
        ' AgentMistralToolStripMenuItem
        ' 
        AgentMistralToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {RecréerToolStripMenuItem, CréerToolStripMenuItem})
        AgentMistralToolStripMenuItem.Name = "AgentMistralToolStripMenuItem"
        AgentMistralToolStripMenuItem.Size = New Size(180, 22)
        AgentMistralToolStripMenuItem.Text = "Agent Mistral"
        ' 
        ' RecréerToolStripMenuItem
        ' 
        RecréerToolStripMenuItem.Name = "RecréerToolStripMenuItem"
        RecréerToolStripMenuItem.Size = New Size(164, 22)
        RecréerToolStripMenuItem.Text = "Recréer (forçage)"
        ' 
        ' CréerToolStripMenuItem
        ' 
        CréerToolStripMenuItem.Name = "CréerToolStripMenuItem"
        CréerToolStripMenuItem.Size = New Size(164, 22)
        CréerToolStripMenuItem.Text = "Créer"
        ' 
        ' ParamètresToolStripMenuItem1
        ' 
        ParamètresToolStripMenuItem1.DropDownItems.AddRange(New ToolStripItem() {FichiersParamètresToolStripMenuItem, ParamètresTechniquesToolStripMenuItem1})
        ParamètresToolStripMenuItem1.Name = "ParamètresToolStripMenuItem1"
        ParamètresToolStripMenuItem1.Size = New Size(78, 20)
        ParamètresToolStripMenuItem1.Text = "Paramètres"
        ' 
        ' FichiersParamètresToolStripMenuItem
        ' 
        FichiersParamètresToolStripMenuItem.Name = "FichiersParamètresToolStripMenuItem"
        FichiersParamètresToolStripMenuItem.Size = New Size(194, 22)
        FichiersParamètresToolStripMenuItem.Text = "Fichiers paramètres"
        ' 
        ' ParamètresTechniquesToolStripMenuItem1
        ' 
        ParamètresTechniquesToolStripMenuItem1.Name = "ParamètresTechniquesToolStripMenuItem1"
        ParamètresTechniquesToolStripMenuItem1.Size = New Size(194, 22)
        ParamètresTechniquesToolStripMenuItem1.Text = "Paramètres techniques"
        ' 
        ' btnTraiteRelevé
        ' 
        btnTraiteRelevé.Location = New Point(29, 124)
        btnTraiteRelevé.Name = "btnTraiteRelevé"
        btnTraiteRelevé.Size = New Size(94, 23)
        btnTraiteRelevé.TabIndex = 9
        btnTraiteRelevé.Text = "Traite relevé"
        btnTraiteRelevé.TextAlign = ContentAlignment.BottomCenter
        btnTraiteRelevé.UseVisualStyleBackColor = True
        ' 
        ' FrmPrincipale
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(966, 541)
        Controls.Add(btnTraiteRelevé)
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
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FichierToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ChargerRelevéToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerBilanToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ParamètresToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AnalyseToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ConsulterTrésorerieToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerBilanToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents RequêteToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GérerUnMouvementToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FermerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnTraiteRelevé As Button
    Friend WithEvents CategorieDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents SousCategorieDataGridViewTextBoxColumn As DataGridViewTextBoxColumn
    Friend WithEvents colEtat As DataGridViewImageColumn
    Friend WithEvents etatImage As DataGridViewTextBoxColumn
    Friend WithEvents EtatMasque As DataGridViewTextBoxColumn
    Friend WithEvents colDateCréation As DataGridViewTextBoxColumn
    Friend WithEvents colTiers As DataGridViewTextBoxColumn
    Friend WithEvents colCategorie As DataGridViewTextBoxColumn
    Friend WithEvents colSousCategorie As DataGridViewTextBoxColumn
    Friend WithEvents colDateMvt As DataGridViewTextBoxColumn
    Friend WithEvents colMontant As DataGridViewTextBoxColumn
    Friend WithEvents colSens As DataGridViewTextBoxColumn
    Friend WithEvents colEvenement As DataGridViewTextBoxColumn
    Friend WithEvents colNote As DataGridViewTextBoxColumn
    Friend WithEvents colType As DataGridViewTextBoxColumn
    Friend WithEvents colModifiable As DataGridViewTextBoxColumn
    Friend WithEvents colNumeroRemise As DataGridViewTextBoxColumn
    Friend WithEvents reference As DataGridViewTextBoxColumn
    Friend WithEvents typeReference As DataGridViewTextBoxColumn
    Friend WithEvents idDoc As DataGridViewTextBoxColumn
    Friend WithEvents GestionBDDToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SauvegarderToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RestaurerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GestionUtilisateurToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ConsoleToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ChangeMdPToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BatchToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AnalyseDocumentsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ParamètresToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents DocumentsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ParamètresTechniquesToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ParamètresTechniquesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents EnvironnementToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AgentMistralToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RecréerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CréerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FichiersParamètresToolStripMenuItem As ToolStripMenuItem
End Class
