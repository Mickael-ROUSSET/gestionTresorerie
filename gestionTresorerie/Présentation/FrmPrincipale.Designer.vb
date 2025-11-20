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
        CopierVersLeDriveToolStripMenuItem = New ToolStripMenuItem()
        RécupérerDuDriveToolStripMenuItem = New ToolStripMenuItem()
        GestionUtilisateurToolStripMenuItem = New ToolStripMenuItem()
        ConsoleToolStripMenuItem = New ToolStripMenuItem()
        ChangeMdPToolStripMenuItem = New ToolStripMenuItem()
        BatchToolStripMenuItem = New ToolStripMenuItem()
        AnalyseDocumentsToolStripMenuItem = New ToolStripMenuItem()
        CinémaToolStripMenuItem = New ToolStripMenuItem()
        ChargerFichierToolStripMenuItem = New ToolStripMenuItem()
        GénérerStatsToolStripMenuItem = New ToolStripMenuItem()
        ParamètresTechniquesToolStripMenuItem = New ToolStripMenuItem()
        EnvironnementToolStripMenuItem = New ToolStripMenuItem()
        AgentMistralToolStripMenuItem = New ToolStripMenuItem()
        RecréerToolStripMenuItem = New ToolStripMenuItem()
        CréerToolStripMenuItem = New ToolStripMenuItem()
        ParamètresToolStripMenuItem1 = New ToolStripMenuItem()
        FichiersParamètresToolStripMenuItem = New ToolStripMenuItem()
        ParamètresTechniquesToolStripMenuItem1 = New ToolStripMenuItem()
        FichierToolStripMenuItem1 = New ToolStripMenuItem()
        NouvelleToolStripMenuItem = New ToolStripMenuItem()
        OuvrirToolStripMenuItem = New ToolStripMenuItem()
        toolStripSeparator = New ToolStripSeparator()
        EnregistrerToolStripMenuItem = New ToolStripMenuItem()
        EnregistrersousToolStripMenuItem = New ToolStripMenuItem()
        toolStripSeparator1 = New ToolStripSeparator()
        ImprimerToolStripMenuItem = New ToolStripMenuItem()
        AperçuavantimpressionToolStripMenuItem = New ToolStripMenuItem()
        toolStripSeparator2 = New ToolStripSeparator()
        QuitterToolStripMenuItem = New ToolStripMenuItem()
        ModifierToolStripMenuItem = New ToolStripMenuItem()
        AnnulerToolStripMenuItem = New ToolStripMenuItem()
        RétablirToolStripMenuItem = New ToolStripMenuItem()
        toolStripSeparator3 = New ToolStripSeparator()
        CouperToolStripMenuItem = New ToolStripMenuItem()
        CopierToolStripMenuItem = New ToolStripMenuItem()
        CollerToolStripMenuItem = New ToolStripMenuItem()
        toolStripSeparator4 = New ToolStripSeparator()
        SélectionnertoutToolStripMenuItem = New ToolStripMenuItem()
        OutilsToolStripMenuItem = New ToolStripMenuItem()
        PersonnaliserToolStripMenuItem = New ToolStripMenuItem()
        OptionsToolStripMenuItem = New ToolStripMenuItem()
        AideToolStripMenuItem = New ToolStripMenuItem()
        ContenuToolStripMenuItem = New ToolStripMenuItem()
        IndexToolStripMenuItem = New ToolStripMenuItem()
        RechercherToolStripMenuItem = New ToolStripMenuItem()
        toolStripSeparator5 = New ToolStripSeparator()
        ÀproposdeToolStripMenuItem = New ToolStripMenuItem()
        btnTraiteRelevé = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        pgBar = New ProgressBar()
        GénérerProgrammeToolStripMenuItem = New ToolStripMenuItem()
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
        MenuStrip1.Items.AddRange(New ToolStripItem() {FichierToolStripMenuItem, AnalyseToolStripMenuItem, GestionBDDToolStripMenuItem, GestionUtilisateurToolStripMenuItem, BatchToolStripMenuItem, CinémaToolStripMenuItem, ParamètresTechniquesToolStripMenuItem, ParamètresToolStripMenuItem1, FichierToolStripMenuItem1, ModifierToolStripMenuItem, OutilsToolStripMenuItem, AideToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.RenderMode = ToolStripRenderMode.System
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
        GestionBDDToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {SauvegarderToolStripMenuItem, RestaurerToolStripMenuItem, CopierVersLeDriveToolStripMenuItem, RécupérerDuDriveToolStripMenuItem})
        GestionBDDToolStripMenuItem.Name = "GestionBDDToolStripMenuItem"
        GestionBDDToolStripMenuItem.Size = New Size(85, 20)
        GestionBDDToolStripMenuItem.Text = "Gestion BDD"
        ' 
        ' SauvegarderToolStripMenuItem
        ' 
        SauvegarderToolStripMenuItem.Name = "SauvegarderToolStripMenuItem"
        SauvegarderToolStripMenuItem.Size = New Size(175, 22)
        SauvegarderToolStripMenuItem.Text = "Sauvegarder"
        ' 
        ' RestaurerToolStripMenuItem
        ' 
        RestaurerToolStripMenuItem.Name = "RestaurerToolStripMenuItem"
        RestaurerToolStripMenuItem.Size = New Size(175, 22)
        RestaurerToolStripMenuItem.Text = "Restaurer"
        ' 
        ' CopierVersLeDriveToolStripMenuItem
        ' 
        CopierVersLeDriveToolStripMenuItem.Name = "CopierVersLeDriveToolStripMenuItem"
        CopierVersLeDriveToolStripMenuItem.Size = New Size(175, 22)
        CopierVersLeDriveToolStripMenuItem.Text = "Copier vers le Drive"
        ' 
        ' RécupérerDuDriveToolStripMenuItem
        ' 
        RécupérerDuDriveToolStripMenuItem.Name = "RécupérerDuDriveToolStripMenuItem"
        RécupérerDuDriveToolStripMenuItem.Size = New Size(175, 22)
        RécupérerDuDriveToolStripMenuItem.Text = "Récupérer du Drive"
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
        ConsoleToolStripMenuItem.Size = New Size(143, 22)
        ConsoleToolStripMenuItem.Text = "Console"
        ' 
        ' ChangeMdPToolStripMenuItem
        ' 
        ChangeMdPToolStripMenuItem.Name = "ChangeMdPToolStripMenuItem"
        ChangeMdPToolStripMenuItem.Size = New Size(143, 22)
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
        ' CinémaToolStripMenuItem
        ' 
        CinémaToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ChargerFichierToolStripMenuItem, GénérerStatsToolStripMenuItem, GénérerProgrammeToolStripMenuItem})
        CinémaToolStripMenuItem.Name = "CinémaToolStripMenuItem"
        CinémaToolStripMenuItem.Size = New Size(60, 20)
        CinémaToolStripMenuItem.Text = "Cinéma"
        ' 
        ' ChargerFichierToolStripMenuItem
        ' 
        ChargerFichierToolStripMenuItem.Name = "ChargerFichierToolStripMenuItem"
        ChargerFichierToolStripMenuItem.Size = New Size(181, 22)
        ChargerFichierToolStripMenuItem.Text = "Charger fichier..."
        ' 
        ' GénérerStatsToolStripMenuItem
        ' 
        GénérerStatsToolStripMenuItem.Name = "GénérerStatsToolStripMenuItem"
        GénérerStatsToolStripMenuItem.Size = New Size(181, 22)
        GénérerStatsToolStripMenuItem.Text = "Générer stats..."
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
        EnvironnementToolStripMenuItem.Size = New Size(155, 22)
        EnvironnementToolStripMenuItem.Text = "Environnement"
        ' 
        ' AgentMistralToolStripMenuItem
        ' 
        AgentMistralToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {RecréerToolStripMenuItem, CréerToolStripMenuItem})
        AgentMistralToolStripMenuItem.Name = "AgentMistralToolStripMenuItem"
        AgentMistralToolStripMenuItem.Size = New Size(155, 22)
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
        ' FichierToolStripMenuItem1
        ' 
        FichierToolStripMenuItem1.DropDownItems.AddRange(New ToolStripItem() {NouvelleToolStripMenuItem, OuvrirToolStripMenuItem, toolStripSeparator, EnregistrerToolStripMenuItem, EnregistrersousToolStripMenuItem, toolStripSeparator1, ImprimerToolStripMenuItem, AperçuavantimpressionToolStripMenuItem, toolStripSeparator2, QuitterToolStripMenuItem})
        FichierToolStripMenuItem1.Name = "FichierToolStripMenuItem1"
        FichierToolStripMenuItem1.Size = New Size(54, 20)
        FichierToolStripMenuItem1.Text = "&Fichier"
        ' 
        ' NouvelleToolStripMenuItem
        ' 
        NouvelleToolStripMenuItem.Image = CType(resources.GetObject("NouvelleToolStripMenuItem.Image"), Image)
        NouvelleToolStripMenuItem.ImageTransparentColor = Color.Magenta
        NouvelleToolStripMenuItem.Name = "NouvelleToolStripMenuItem"
        NouvelleToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.N
        NouvelleToolStripMenuItem.Size = New Size(205, 22)
        NouvelleToolStripMenuItem.Text = "&Nouvelle"
        ' 
        ' OuvrirToolStripMenuItem
        ' 
        OuvrirToolStripMenuItem.Image = CType(resources.GetObject("OuvrirToolStripMenuItem.Image"), Image)
        OuvrirToolStripMenuItem.ImageTransparentColor = Color.Magenta
        OuvrirToolStripMenuItem.Name = "OuvrirToolStripMenuItem"
        OuvrirToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.O
        OuvrirToolStripMenuItem.Size = New Size(205, 22)
        OuvrirToolStripMenuItem.Text = "&Ouvrir"
        ' 
        ' toolStripSeparator
        ' 
        toolStripSeparator.Name = "toolStripSeparator"
        toolStripSeparator.Size = New Size(202, 6)
        ' 
        ' EnregistrerToolStripMenuItem
        ' 
        EnregistrerToolStripMenuItem.Image = CType(resources.GetObject("EnregistrerToolStripMenuItem.Image"), Image)
        EnregistrerToolStripMenuItem.ImageTransparentColor = Color.Magenta
        EnregistrerToolStripMenuItem.Name = "EnregistrerToolStripMenuItem"
        EnregistrerToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.S
        EnregistrerToolStripMenuItem.Size = New Size(205, 22)
        EnregistrerToolStripMenuItem.Text = "Enre&gistrer"
        ' 
        ' EnregistrersousToolStripMenuItem
        ' 
        EnregistrersousToolStripMenuItem.Name = "EnregistrersousToolStripMenuItem"
        EnregistrersousToolStripMenuItem.Size = New Size(205, 22)
        EnregistrersousToolStripMenuItem.Text = "&Enregistrer sous"
        ' 
        ' toolStripSeparator1
        ' 
        toolStripSeparator1.Name = "toolStripSeparator1"
        toolStripSeparator1.Size = New Size(202, 6)
        ' 
        ' ImprimerToolStripMenuItem
        ' 
        ImprimerToolStripMenuItem.Image = CType(resources.GetObject("ImprimerToolStripMenuItem.Image"), Image)
        ImprimerToolStripMenuItem.ImageTransparentColor = Color.Magenta
        ImprimerToolStripMenuItem.Name = "ImprimerToolStripMenuItem"
        ImprimerToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.P
        ImprimerToolStripMenuItem.Size = New Size(205, 22)
        ImprimerToolStripMenuItem.Text = "&Imprimer"
        ' 
        ' AperçuavantimpressionToolStripMenuItem
        ' 
        AperçuavantimpressionToolStripMenuItem.Image = CType(resources.GetObject("AperçuavantimpressionToolStripMenuItem.Image"), Image)
        AperçuavantimpressionToolStripMenuItem.ImageTransparentColor = Color.Magenta
        AperçuavantimpressionToolStripMenuItem.Name = "AperçuavantimpressionToolStripMenuItem"
        AperçuavantimpressionToolStripMenuItem.Size = New Size(205, 22)
        AperçuavantimpressionToolStripMenuItem.Text = "Aperçu a&vant impression"
        ' 
        ' toolStripSeparator2
        ' 
        toolStripSeparator2.Name = "toolStripSeparator2"
        toolStripSeparator2.Size = New Size(202, 6)
        ' 
        ' QuitterToolStripMenuItem
        ' 
        QuitterToolStripMenuItem.Name = "QuitterToolStripMenuItem"
        QuitterToolStripMenuItem.Size = New Size(205, 22)
        QuitterToolStripMenuItem.Text = "&Quitter"
        ' 
        ' ModifierToolStripMenuItem
        ' 
        ModifierToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {AnnulerToolStripMenuItem, RétablirToolStripMenuItem, toolStripSeparator3, CouperToolStripMenuItem, CopierToolStripMenuItem, CollerToolStripMenuItem, toolStripSeparator4, SélectionnertoutToolStripMenuItem})
        ModifierToolStripMenuItem.Name = "ModifierToolStripMenuItem"
        ModifierToolStripMenuItem.Size = New Size(64, 20)
        ModifierToolStripMenuItem.Text = "&Modifier"
        ' 
        ' AnnulerToolStripMenuItem
        ' 
        AnnulerToolStripMenuItem.Name = "AnnulerToolStripMenuItem"
        AnnulerToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.Z
        AnnulerToolStripMenuItem.Size = New Size(164, 22)
        AnnulerToolStripMenuItem.Text = "&Annuler"
        ' 
        ' RétablirToolStripMenuItem
        ' 
        RétablirToolStripMenuItem.Name = "RétablirToolStripMenuItem"
        RétablirToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.Y
        RétablirToolStripMenuItem.Size = New Size(164, 22)
        RétablirToolStripMenuItem.Text = "&Rétablir"
        ' 
        ' toolStripSeparator3
        ' 
        toolStripSeparator3.Name = "toolStripSeparator3"
        toolStripSeparator3.Size = New Size(161, 6)
        ' 
        ' CouperToolStripMenuItem
        ' 
        CouperToolStripMenuItem.Image = CType(resources.GetObject("CouperToolStripMenuItem.Image"), Image)
        CouperToolStripMenuItem.ImageTransparentColor = Color.Magenta
        CouperToolStripMenuItem.Name = "CouperToolStripMenuItem"
        CouperToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.X
        CouperToolStripMenuItem.Size = New Size(164, 22)
        CouperToolStripMenuItem.Text = "&Couper"
        ' 
        ' CopierToolStripMenuItem
        ' 
        CopierToolStripMenuItem.Image = CType(resources.GetObject("CopierToolStripMenuItem.Image"), Image)
        CopierToolStripMenuItem.ImageTransparentColor = Color.Magenta
        CopierToolStripMenuItem.Name = "CopierToolStripMenuItem"
        CopierToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.C
        CopierToolStripMenuItem.Size = New Size(164, 22)
        CopierToolStripMenuItem.Text = "&Copier"
        ' 
        ' CollerToolStripMenuItem
        ' 
        CollerToolStripMenuItem.Image = CType(resources.GetObject("CollerToolStripMenuItem.Image"), Image)
        CollerToolStripMenuItem.ImageTransparentColor = Color.Magenta
        CollerToolStripMenuItem.Name = "CollerToolStripMenuItem"
        CollerToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.V
        CollerToolStripMenuItem.Size = New Size(164, 22)
        CollerToolStripMenuItem.Text = "&Coller"
        ' 
        ' toolStripSeparator4
        ' 
        toolStripSeparator4.Name = "toolStripSeparator4"
        toolStripSeparator4.Size = New Size(161, 6)
        ' 
        ' SélectionnertoutToolStripMenuItem
        ' 
        SélectionnertoutToolStripMenuItem.Name = "SélectionnertoutToolStripMenuItem"
        SélectionnertoutToolStripMenuItem.Size = New Size(164, 22)
        SélectionnertoutToolStripMenuItem.Text = "&Sélectionner tout"
        ' 
        ' OutilsToolStripMenuItem
        ' 
        OutilsToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {PersonnaliserToolStripMenuItem, OptionsToolStripMenuItem})
        OutilsToolStripMenuItem.Name = "OutilsToolStripMenuItem"
        OutilsToolStripMenuItem.Size = New Size(50, 20)
        OutilsToolStripMenuItem.Text = "O&utils"
        ' 
        ' PersonnaliserToolStripMenuItem
        ' 
        PersonnaliserToolStripMenuItem.Name = "PersonnaliserToolStripMenuItem"
        PersonnaliserToolStripMenuItem.Size = New Size(144, 22)
        PersonnaliserToolStripMenuItem.Text = "&Personnaliser"
        ' 
        ' OptionsToolStripMenuItem
        ' 
        OptionsToolStripMenuItem.Name = "OptionsToolStripMenuItem"
        OptionsToolStripMenuItem.Size = New Size(144, 22)
        OptionsToolStripMenuItem.Text = "&Options"
        ' 
        ' AideToolStripMenuItem
        ' 
        AideToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ContenuToolStripMenuItem, IndexToolStripMenuItem, RechercherToolStripMenuItem, toolStripSeparator5, ÀproposdeToolStripMenuItem})
        AideToolStripMenuItem.Name = "AideToolStripMenuItem"
        AideToolStripMenuItem.Size = New Size(43, 20)
        AideToolStripMenuItem.Text = "A&ide"
        ' 
        ' ContenuToolStripMenuItem
        ' 
        ContenuToolStripMenuItem.Name = "ContenuToolStripMenuItem"
        ContenuToolStripMenuItem.Size = New Size(147, 22)
        ContenuToolStripMenuItem.Text = "Conten&u"
        ' 
        ' IndexToolStripMenuItem
        ' 
        IndexToolStripMenuItem.Name = "IndexToolStripMenuItem"
        IndexToolStripMenuItem.Size = New Size(147, 22)
        IndexToolStripMenuItem.Text = "&Index"
        ' 
        ' RechercherToolStripMenuItem
        ' 
        RechercherToolStripMenuItem.Name = "RechercherToolStripMenuItem"
        RechercherToolStripMenuItem.Size = New Size(147, 22)
        RechercherToolStripMenuItem.Text = "&Rechercher"
        ' 
        ' toolStripSeparator5
        ' 
        toolStripSeparator5.Name = "toolStripSeparator5"
        toolStripSeparator5.Size = New Size(144, 6)
        ' 
        ' ÀproposdeToolStripMenuItem
        ' 
        ÀproposdeToolStripMenuItem.Name = "ÀproposdeToolStripMenuItem"
        ÀproposdeToolStripMenuItem.Size = New Size(147, 22)
        ÀproposdeToolStripMenuItem.Text = "À pr&opos de..."
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
        ' pgBar
        ' 
        pgBar.Location = New Point(144, 473)
        pgBar.Name = "pgBar"
        pgBar.Size = New Size(262, 23)
        pgBar.TabIndex = 10
        ' 
        ' GénérerProgrammeToolStripMenuItem
        ' 
        GénérerProgrammeToolStripMenuItem.Name = "GénérerProgrammeToolStripMenuItem"
        GénérerProgrammeToolStripMenuItem.Size = New Size(181, 22)
        GénérerProgrammeToolStripMenuItem.Text = "Générer programme"
        ' 
        ' FrmPrincipale
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(966, 541)
        Controls.Add(pgBar)
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
    Friend WithEvents AnalyseToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ConsulterTrésorerieToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerBilanToolStripMenuItem1 As ToolStripMenuItem
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
    Friend WithEvents CopierVersLeDriveToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RécupérerDuDriveToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FichierToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents NouvelleToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OuvrirToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents toolStripSeparator As ToolStripSeparator
    Friend WithEvents EnregistrerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents EnregistrersousToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents toolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ImprimerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AperçuavantimpressionToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents toolStripSeparator2 As ToolStripSeparator
    Friend WithEvents QuitterToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ModifierToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AnnulerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RétablirToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents toolStripSeparator3 As ToolStripSeparator
    Friend WithEvents CouperToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CopierToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CollerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents toolStripSeparator4 As ToolStripSeparator
    Friend WithEvents SélectionnertoutToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OutilsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents PersonnaliserToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OptionsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AideToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ContenuToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents IndexToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RechercherToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents toolStripSeparator5 As ToolStripSeparator
    Friend WithEvents ÀproposdeToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents pgBar As ProgressBar
    Friend WithEvents CinémaToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ChargerFichierToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerStatsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GénérerProgrammeToolStripMenuItem As ToolStripMenuItem
End Class
