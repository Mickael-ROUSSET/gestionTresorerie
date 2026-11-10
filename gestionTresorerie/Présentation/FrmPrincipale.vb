Imports System.Data.SqlClient

Public Class FrmPrincipale
    Inherits System.Windows.Forms.Form

    ' Instantané fourni explicitement par un appelant ; conservé en mémoire uniquement.
    Private _revuePropositions As PropositionRevue
    Private _workflowDocumentsAnalyses As GmailDocumentReviewWorkflow
    Private ReadOnly _menuRevueDocumentsAnalyses As New ToolStripMenuItem With {
        .Name = "RevueDocumentsAnalysesToolStripMenuItem",
        .Text = "Revue des documents préparés (simulation)",
        .Enabled = False}
    Private ReadOnly _menuDemoDocuments As New ToolStripMenuItem With {
        .Name = "DemoDocumentsFictifsToolStripMenuItem",
        .Text = "Ouvrir une démonstration (documents fictifs, hors ligne)"}
    Private ReadOnly _menuEffacerDocumentsFictifs As New ToolStripMenuItem With {
        .Name = "EffacerDocumentsFictifsToolStripMenuItem",
        .Text = "Effacer les propositions fictives en mémoire",
        .Enabled = False}

    Private ReadOnly _menuTestWorkspace As New ToolStripMenuItem With {
        .Name = "EspaceTestLocalToolStripMenuItem",
        .Text = "Espace TEST local (fichiers fictifs)"}

    Private ReadOnly _menuSqlTest As New ToolStripMenuItem With {
        .Name = "EspaceTestSqlToolStripMenuItem",
        .Text = "Espace TEST SQL (documents et classement TEST)…"}

    Private ReadOnly _modeDemonstration As Boolean
    Private _isolatedSqlTest As Boolean

    Public Sub New()
        Me.New(False)
    End Sub

    Public Sub New(modeDemonstration As Boolean)
#If DEMO_BUILD Then
        ' La version de demonstration ne peut pas revenir au mode comptable.
        _modeDemonstration = True
#Else
        _modeDemonstration = modeDemonstration OrElse
            Array.Exists(System.Environment.GetCommandLineArgs(),
                         Function(arg) String.Equals(arg, "--demo", StringComparison.OrdinalIgnoreCase))
#End If
        If _modeDemonstration Then
            ' Le code My.Settings genere peut sauvegarder automatiquement user.config
            ' a la fermeture. Ne jamais toucher aux parametres de l'utilisateur en demo.
            My.Application.SaveMySettingsOnExit = False
        End If
        InitializeComponent()
        If Not _modeDemonstration Then
            Dim menuFichiers As New ToolStripMenuItem("Examiner des PDF et des scans sur disque…")
            AddHandler menuFichiers.Click, Sub() FrmDocumentFilePreview.OpenFiles(Me)
            AnalyseToolStripMenuItem.DropDownItems.Add(menuFichiers)
        End If
        AddHandler _menuTestWorkspace.Click, AddressOf OpenTestWorkspace
        AnalyseToolStripMenuItem.DropDownItems.Add(_menuTestWorkspace)
        AddHandler _menuSqlTest.Click, AddressOf OpenSqlTestWorkspace
        AnalyseToolStripMenuItem.DropDownItems.Add(_menuSqlTest)

        Dim menuRevue As New ToolStripMenuItem With {
            .Name = "RevuePropositionsToolStripMenuItem",
            .Text = "Revue des propositions (lecture seule)"}
        AddHandler menuRevue.Click, AddressOf RevuePropositions_Click
        AnalyseToolStripMenuItem.DropDownItems.Add(menuRevue)
        AddHandler _menuRevueDocumentsAnalyses.Click, AddressOf RevueDocumentsAnalyses_Click
        AnalyseToolStripMenuItem.DropDownItems.Add(_menuRevueDocumentsAnalyses)
        AddHandler _menuDemoDocuments.Click, AddressOf DemoDocumentsFictifs_Click
        AnalyseToolStripMenuItem.DropDownItems.Add(_menuDemoDocuments)
        AddHandler _menuEffacerDocumentsFictifs.Click, AddressOf EffacerDocumentsFictifs_Click
        AnalyseToolStripMenuItem.DropDownItems.Add(_menuEffacerDocumentsFictifs)
    End Sub

    ''' <summary>
    ''' Ouvre une consultation du résultat fourni, sans chargement ni écriture externe.
    ''' L'appelant doit appeler cette méthode sur le thread de l'interface.
    ''' </summary>
    Public Sub OuvrirRevuePropositions(resultat As PropositionDocumentService.Resultat)
        If resultat Is Nothing Then Throw New ArgumentNullException(NameOf(resultat))

        _revuePropositions = New PropositionRevue(resultat)
        AfficherRevuePropositions()
    End Sub

    ''' <summary>
    ''' Ouvre une revue avec corrections suivies dans une session en memoire.
    ''' Aucune approbation, ecriture SQL ou import Gmail n'est effectue ici.
    ''' </summary>
    Public Sub OuvrirRevuePropositions(resultat As PropositionDocumentService.Resultat,
                                      sessionRevue As PropositionSessionRevue)
        Using formulaire = CreerFenetreRevuePropositions(resultat, sessionRevue)
            formulaire.ShowDialog(Me)
        End Using
    End Sub

    ''' <summary>
    ''' Construction separable de l'affichage pour verifier le raccordement hors ligne.
    ''' Le formulaire controle que la session contient les instances exactes du lot.
    ''' </summary>
    Public Shared Function CreerFenetreRevuePropositions(resultat As PropositionDocumentService.Resultat,
                                                         sessionRevue As PropositionSessionRevue) As FrmRevuePropositions
        If resultat Is Nothing Then Throw New ArgumentNullException(NameOf(resultat))
        If sessionRevue Is Nothing Then Throw New ArgumentNullException(NameOf(sessionRevue))
        Return New FrmRevuePropositions(New PropositionRevue(resultat), resultat.Propositions, sessionRevue)
    End Function

    ''' <summary>
    ''' Point d'entree hors ligne pour un lot Gmail deja analyse. La fenetre est
    ''' reconstruite a partir des corrections courantes de la session, sans
    ''' revenir aux propositions initiales apres une fermeture.
    ''' Aucun acces Gmail, approbation ou enregistrement comptable.
    ''' </summary>
    Public Sub OuvrirRevueDocumentsAnalyses(workflow As GmailDocumentReviewWorkflow)
        Using formulaire = CreerFenetreRevuePropositions(workflow)
            PresenterRevueDocumentsAnalyses(formulaire)
        End Using
    End Sub

    ''' <summary>Point de presentation isolable sans afficher le parent ni acceder a SQL.</summary>
    Protected Overridable Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
        formulaire.ShowDialog(Me)
    End Sub

    ''' <summary>Fabrique testable sans afficher FrmPrincipale ni acceder a SQL.</summary>
    Public Shared Function CreerFenetreRevuePropositions(workflow As GmailDocumentReviewWorkflow) As FrmRevuePropositions
        If workflow Is Nothing Then Throw New ArgumentNullException(NameOf(workflow))
        Return workflow.CreerFenetre()
    End Function

    ''' <summary>
    ''' Rend accessible depuis le menu un lot deja prepare en memoire.
    ''' Rien n'est importe, approuve ou enregistre ; Nothing retire le lot.
    ''' </summary>
    Public Sub DefinirWorkflowDocumentsAnalyses(workflow As GmailDocumentReviewWorkflow)
        _workflowDocumentsAnalyses = workflow
        _menuRevueDocumentsAnalyses.Enabled = workflow IsNot Nothing
        _menuEffacerDocumentsFictifs.Enabled = workflow IsNot Nothing
    End Sub

    ''' <summary>
    ''' Raccorde des JSON fictifs deja extraits au menu de revue humaine.
    ''' Aucun message Gmail, fichier ou SQL n'est consulte ou modifie.
    ''' Le resultat reste en memoire, sans approbation ni enregistrement.
    ''' </summary>
    Public Function PreparerRevueDocumentsJsonFictifs(
        pieces As System.Collections.Generic.IEnumerable(Of GmailDocumentExtractionAdapter.PieceJson)
    ) As GmailDocumentReviewWorkflow
        ' Une nouvelle preparation invalide ne doit pas laisser accessible
        ' l'ancien lot dans le menu : comportement de securite fail-closed.
        DefinirWorkflowDocumentsAnalyses(Nothing)
        If pieces Is Nothing Then Throw New ArgumentNullException(NameOf(pieces))
        Dim workflow = GmailDocumentReviewWorkflow.PreparerDepuisJson(pieces)
        DefinirWorkflowDocumentsAnalyses(workflow)
        Return workflow
    End Function

    ''' <summary>
    ''' Raccorde des messages IMAP strictement fictifs au menu de revue.
    ''' Les pieces et JSON sont deja en memoire : aucun acces Gmail, fichier ou SQL.
    ''' Une tentative invalide retire l'ancien lot pour eviter une revue obsolete.
    ''' </summary>
    Public Function PreparerRevueMessagesFictifs(
        messages As System.Collections.Generic.IEnumerable(Of GmailDocumentSimulatedInbox.MessageFictif)
    ) As GmailDocumentReviewWorkflow
        DefinirWorkflowDocumentsAnalyses(Nothing)
        If messages Is Nothing Then Throw New ArgumentNullException(NameOf(messages))
        Dim workflow = GmailDocumentSimulatedInbox.PreparerRevue(messages)
        DefinirWorkflowDocumentsAnalyses(workflow)
        Return workflow
    End Function

    ''' <summary>
    ''' Démonstration activable depuis l'interface : données exclusivement fictives
    ''' en mémoire, sans connexion Gmail, lecture de fichier ni écriture SQL.
    ''' Relancer la démonstration remplace le lot précédent ; la revue reste sans approbation.
    ''' </summary>
    Protected Overridable Sub OpenTestWorkspace(sender As Object, e As EventArgs)
        Dim root = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "gestionTresorerie", "TEST-hors-ligne")
        Try
            Using window As New FrmOfflineTestWorkspace(
                New OfflineTestWorkspace(My.Settings.repInstallationProd, root))
                window.ShowDialog(Me)
            End Using
        Catch ex As Exception
            MessageBox.Show("Ouverture de TEST impossible : " & ex.Message,
                            "Espace TEST", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OpenSqlTestWorkspace(sender As Object, e As EventArgs)
        Try
            Dim root = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "gestionTresorerie", "TEST-SQL")
            Using window As New FrmSqlTestDocuments(New OfflineTestWorkspace(My.Settings.repInstallationProd, root))
                window.ShowDialog(Me)
            End Using
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Espace TEST SQL", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DemoDocumentsFictifs_Click(sender As Object, e As EventArgs)
        Dim jsonFacture As String = DemoDocumentSamples.Invoice
        Dim pieces As New System.Collections.Generic.List(Of GmailDocumentSimulatedInbox.PieceFictive) From {
            New GmailDocumentSimulatedInbox.PieceFictive(
                jsonFacture, PropositionMouvement.SensMouvement.Debit)}
        If _modeDemonstration Then
            Dim jsonCheque As String = DemoDocumentSamples.Cheque
            pieces.Add(New GmailDocumentSimulatedInbox.PieceFictive(
                jsonCheque, PropositionMouvement.SensMouvement.Credit))
            pieces.Add(New GmailDocumentSimulatedInbox.PieceFictive(
                DemoDocumentSamples.Unknown,
                PropositionMouvement.SensMouvement.Debit))
        End If
        pieces.Add(New GmailDocumentSimulatedInbox.PieceFictive(
            DemoDocumentSamples.Invalid, PropositionMouvement.SensMouvement.Debit))
        Dim message As New GmailDocumentSimulatedInbox.MessageFictif(
            "demo@example.invalid", 1UI, 1UI, pieces)
        Dim messages As New System.Collections.Generic.List(Of GmailDocumentSimulatedInbox.MessageFictif) From {message}
        If _modeDemonstration Then
            ' Meme identite source que la premiere facture : doublon a rejeter.
            messages.Add(New GmailDocumentSimulatedInbox.MessageFictif(
                "demo@example.invalid", 1UI, 1UI,
                New GmailDocumentSimulatedInbox.PieceFictive() {
                    New GmailDocumentSimulatedInbox.PieceFictive(
                        jsonFacture, PropositionMouvement.SensMouvement.Debit)}))
        End If
        Dim workflow = PreparerRevueMessagesFictifs(messages)
        OuvrirRevueDocumentsAnalyses(workflow)
    End Sub

    ''' <summary>Oublie le lot fictif et ses corrections locales, sans accès externe.</summary>
    Private Sub EffacerDocumentsFictifs_Click(sender As Object, e As EventArgs)
        DefinirWorkflowDocumentsAnalyses(Nothing)
    End Sub

    Private Sub RevueDocumentsAnalyses_Click(sender As Object, e As EventArgs)
        If _workflowDocumentsAnalyses Is Nothing Then Return
        OuvrirRevueDocumentsAnalyses(_workflowDocumentsAnalyses)
    End Sub

    Private Sub RevuePropositions_Click(sender As Object, e As EventArgs)
        If _revuePropositions Is Nothing Then
            MessageBox.Show(Me,
                            "Aucune proposition n'a été fournie actuellement par un appelant autorisé. " &
                            "Cette consultation en lecture seule ne charge aucune donnée.",
                            "Revue des propositions (lecture seule)",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        AfficherRevuePropositions()
    End Sub

    Private Sub AfficherRevuePropositions()
        Using formulaire As New FrmRevuePropositions(_revuePropositions)
            formulaire.ShowDialog(Me)
        End Using
    End Sub

    ' Variables de classe pour utiliser l'index des colonnes du datagridview
    Private _icolEtat As Integer
    Private _icolEtatMasque As Integer
    Private _icolTiers As Integer
    Private _icolCategorie As Integer
    Private _icolSousCategorie As Integer
    Private _icolDateMvt As Integer
    Private _icolMontant As Integer
    Private _icolSens As Integer
    Private _icolEvenement As Integer
    Private _icolNote As Integer
    Private _icolType As Integer
    Private _icolModifiable As Integer
    Private _icolNumeroRemise As Integer

    ''' <summary>
    ''' Démarrage hors ligne : ne charge ni paramètres, ni secrets, ni SQL.
    ''' Tous les menus de production sont inaccessibles dans cette session.
    ''' </summary>
    Private Sub InitialiserModeDemonstration()
        Text = "gestionTresorerie — DÉMONSTRATION HORS LIGNE"
        StatusLabelEnv.Items.Clear()
        StatusLabelEnv.Items.Add(New ToolStripStatusLabel("DÉMONSTRATION — documents fictifs, mémoire uniquement"))
        For Each menu As ToolStripItem In MenuStrip1.Items
            menu.Enabled = Object.ReferenceEquals(menu, AnalyseToolStripMenuItem)
        Next
        For Each action As ToolStripItem In AnalyseToolStripMenuItem.DropDownItems
            action.Enabled = Object.ReferenceEquals(action, _menuTestWorkspace) OrElse
                             Object.ReferenceEquals(action, _menuSqlTest) OrElse
                             Object.ReferenceEquals(action, _menuDemoDocuments) OrElse
                             Object.ReferenceEquals(action, _menuRevueDocumentsAnalyses) OrElse
                             Object.ReferenceEquals(action, _menuEffacerDocumentsFictifs)
        Next
        _menuRevueDocumentsAnalyses.Enabled = False
        _menuEffacerDocumentsFictifs.Enabled = False
        dgvPrincipale.DataSource = Nothing
        dgvPrincipale.Enabled = False
        dgvPrincipale.Visible = False
        pgBar.Visible = False
        Controls.Add(New Label With {
            .Name = "annonceDemonstration",
            .Text = "DÉMONSTRATION — espace documentaire hors ligne ou SQL TEST sur demande." & Environment.NewLine &
                    "Analyse > Ouvrir une démonstration : facture, chèque et documents rejetés." & Environment.NewLine &
                    "La confirmation d'examen ne valide ni n'enregistre d'opération." & Environment.NewLine &
                    "Analyse > Effacer les propositions fictives : recommencer.",
            .Location = New System.Drawing.Point(25, 80),
            .Size = New System.Drawing.Size(1100, 200),
            .Font = New System.Drawing.Font(Font.FontFamily, 13.0F, System.Drawing.FontStyle.Bold)})
    End Sub

    Private Sub FrmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _modeDemonstration Then
            InitialiserModeDemonstration()
            Return
        End If
        Try
            EnvironmentManager.LoadEnvironment()
            If EnvironmentManager.CurrentEnvironment = EnvironmentManager.EnvironnementType.TEST Then
                _isolatedSqlTest = True
                InitialiserModeDemonstration()
                Text = "gestionTresorerie — TEST SQL"
                StatusLabelEnv.Items.Clear()
                StatusLabelEnv.Items.Add(New ToolStripStatusLabel("TEST SQL — bddAgumaaaTest"))
                DirectCast(Controls.Find("annonceDemonstration", True)(0), Label).Text =
                    "TEST SQL — Analyse > Espace TEST SQL pour initialiser la base et traiter des documents."
                Dim switchEnvironment As New ToolStripMenuItem("Changer d’environnement au prochain démarrage…")
                AddHandler switchEnvironment.Click, AddressOf EnvironnementToolStripMenuItem_Click
                AnalyseToolStripMenuItem.DropDownItems.Add(switchEnvironment)
                Return
            End If
            Logger.INFO("Environnement actif : " & EnvironmentManager.GetEnvironmentLabel())
            StatusLabelEnv.Text = "Environnement : " & EnvironmentManager.GetEnvironmentLabel

            'Initialisation de la lecture des propriétés
            Dim lectureProprietes As New LectureProprietes()
            'Lecture du niveau de log demandé
            Logger.SetLogLevel(LectureProprietes.GetVariable(Constantes.Environnement.NiveauLog))
            Logger.DBG("Initialisation de : {lectureProprietes}")
            'Chargement des secrets de l'application
            GlobalSettings.Initialize()
            'Récupère le rang des colonnes du datagridview
            initIndiceColDgv()
            'Charger dgvPrincipale avec le contenu de la table mouvements
            Call ChargerDgvPrincipale()
        Catch ex As Exception
            ' Gestion des erreurs
            MessageBox.Show($"Une erreur est survenue lors de l'initialisation : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.ERR($"Une erreur est survenue lors de l'initialisation : {ex.Message}")
        End Try
    End Sub
    Private Sub initIndiceColDgv()

        'Récupère le rang des colonnes du datagridview
        With dgvPrincipale
            _icolEtat = .Columns("colEtat").Index
            '_icolEtatMasque = .Columns("colEtatMasque").Index
            _icolTiers = .Columns("colTiers").Index
            _icolCategorie = .Columns("colCategorie").Index
            _icolSousCategorie = .Columns("colSousCategorie").Index
            _icolDateMvt = .Columns("colDateMvt").Index
            _icolMontant = .Columns("colMontant").Index
            _icolSens = .Columns("colSens").Index
            _icolEvenement = .Columns("colEvenement").Index
            _icolNote = .Columns("colNote").Index
            _icolType = .Columns("colType").Index
            _icolModifiable = .Columns("colModifiable").Index
            _icolNumeroRemise = .Columns("colNumeroRemise").Index
        End With
    End Sub
    Private Sub ChargerDgvPrincipale()
        Try
            Dim dataTable As DataTable =
            SqlCommandBuilder.ExecuteDataTable(
                Constantes.DataBases.Agumaaa,
                "sqlSelectMouvementsLibelles")

            dgvPrincipale.DataSource = dataTable

            AjouterColonneEtatImage()

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement de la grille principale : {ex.Message}")
            MessageBox.Show("Erreur lors du chargement de la grille principale : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub AjouterColonneEtatImage()
        ' Parcourir les lignes du DataGridView pour définir les images
        For Each ligne As DataGridViewRow In dgvPrincipale.Rows
            If Not ligne.IsNewRow Then
                Try
                    ' On récupère la valeur de la colonne "etat" par son nom
                    Dim etat As Object = ligne.Cells("EtatMasque").Value

                    If etat IsNot Nothing AndAlso TypeOf etat Is Boolean Then
                        ligne.Cells("colEtat").Value = If(CType(ligne.Cells("EtatMasque").Value, Boolean), My.Resources.OK, My.Resources.KO)
                    Else
                        Logger.ERR($"Valeur invalide pour la colonne 'etat' dans la ligne {ligne.Index}: {If(etat, "null")}")
                        ligne.Cells("colEtat").Value = My.Resources.KO
                    End If

                Catch ex As Exception
                    Logger.ERR($"Erreur lors de la définition de l'image pour la colonne 'etat' dans la ligne {ligne.Index}: {ex.Message}")
                    ligne.Cells("colEtat").Value = My.Resources.KO
                End Try
            End If
        Next
    End Sub

    Private Sub FrmMain_Closing(sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        If _modeDemonstration OrElse _isolatedSqlTest Then Return
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).Dispose()
    End Sub
    Private Sub BtnConsultation_Click(sender As Object, e As EventArgs)
        ChargerDgvPrincipale()
    End Sub
    Private Sub FermerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FermerToolStripMenuItem.Click
        Close()
        End
    End Sub
    Private Sub dgvPrincipale_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPrincipale.CellContentClick
        Try
            ' Récupérer les valeurs de la ligne sélectionnée
            Dim currentRow As DataGridViewRow = dgvPrincipale.CurrentRow
            If currentRow Is Nothing Then
                Return
            End If

            Dim note As String = currentRow.Cells(_icolNote).Value?.ToString()
            Dim dateMvt As Date = Utilitaires.ConvertToDate(currentRow.Cells(_icolDateMvt).Value)
            Dim montant As Double = Utilitaires.ConvertToDouble(currentRow.Cells(_icolMontant).Value)
            Dim sens As Boolean = Utilitaires.ConvertToBoolean(currentRow.Cells(_icolSens).Value)
            Dim rapproche As Boolean = Utilitaires.ConvertToBoolean(currentRow.Cells(_icolModifiable).Value)
            Dim remise As String = currentRow.Cells(_icolNote).Value?.ToString()

            ' Charger les valeurs dans le formulaire de saisie
            With FrmSaisie
                .dateMvt.Value = dateMvt
                .txtNote.Text = note
                .rbDebit.Checked = sens
                .txtMontant.Text = montant.ToString()
                .txtRemise.Text = remise
                .rbRapproche.Checked = rapproche
                .Show()
            End With
        Catch ex As Exception
            MsgBox($"Une erreur est survenue : {ex.Message}", MsgBoxStyle.Critical)
            Logger.ERR($"Une erreur est survenue : {ex.Message}")
        End Try
    End Sub
    Private Sub SauvegarderToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SauvegarderToolStripMenuItem.Click
        Dim dbName As String = Constantes.DataBases.Agumaaa
        Dim dossier As String = "D:\Sauvegardes"
        Dim factory As IConnectionFactory = RepositoryFactory.CreateConnectionFactory(Constantes.DataBases.Agumaaa)
        Dim sqlAdmin As New SqlAdminService(factory)
        Dim maintenance As New DatabaseMaintenanceService(sqlAdmin)

        Dim chemin = maintenance.Sauvegarder(dbName, dossier)
    End Sub

    Private Sub RestaurerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RestaurerToolStripMenuItem.Click
        Dim dossierSauvegarde As String = Constantes.Dossiers.Sauvegardes

        Dim ofd As New OpenFileDialog With {
        .Filter = "Sauvegardes SQL (*.bak)|*.bak",
        .InitialDirectory = dossierSauvegarde
    }

        If ofd.ShowDialog() <> DialogResult.OK Then
            Exit Sub
        End If

        Try
            Dim dbName As String = Constantes.DataBases.Agumaaa
            Dim factory As IConnectionFactory = RepositoryFactory.CreateConnectionFactory(Constantes.DataBases.Agumaaa)
            Dim sqlAdmin As New SqlAdminService(factory)

            sqlAdmin.RestaurerBase(dbName, ofd.FileName)

            MessageBox.Show("Restauration terminée avec succès.",
                        "Restauration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

        Catch ex As Exception
            Logger.ERR("Erreur lors de la restauration : " & ex.Message)

            MessageBox.Show("Erreur lors de la restauration : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ConsoleToolStripMenuItem_Click(sender As Object, e As EventArgs)

        If UtilisateurActif Is Nothing OrElse Not UtilisateurActif.EstAdmin Then
            MessageBox.Show("Accès réservé aux administrateurs.")
        End If

        Dim frm As New FrmGestionUtilisateurs
        frm.ShowDialog()
    End Sub

    Private Sub ChangeMdPToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim frm As New FrmChangePassword
        frm.ShowDialog()
    End Sub

    Private Async Sub AnalyseDocumentsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AnalyseDocumentsToolStripMenuItem.Click
        Dim lanceur As New Lanceur
        Await lanceur.LanceTrt()
    End Sub

    Private Sub GénérerBilanToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles GénérerBilanToolStripMenuItem1.Click
        'Création du fichier LibreOffice Writer
        CreePresentation.LectureBase()
    End Sub

    Private Sub DocumentsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DocumentsToolStripMenuItem.Click
        'Afficher frmSelectionneDocument pour ramener tous les documents

        Dim selectionneDocument As New FrmSelectionneDocument()
        selectionneDocument.Show()
    End Sub

    Private Sub CopierVersLeDriveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopierVersLeDriveToolStripMenuItem.Click
        Call Utilitaires.SauvegarderBaseVersDrive("C2Drive")
    End Sub

    Private Sub RécupérerDuDriveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RécupérerDuDriveToolStripMenuItem.Click
        Call Utilitaires.SauvegarderBaseVersDrive("Drive2C")
    End Sub

    Private Sub ChargerFichierToolStripMenuItem_Click(sender As Object, e As EventArgs)
        'Chargement de la base à partir du fichier Excel des entrées cinéma
        'Les fichiers annuels se trouvent dans G:\Mon Drive\AGUMAAA\Documents\Cinéma\Entrées
        Dim entreeCinema As New EntreeCinema
        entreeCinema.ImporterEntreesDepuisExcel()
    End Sub

    Private Sub GénérerStatsToolStripMenuItem_Click(sender As Object, e As EventArgs)
        frmStatsCinema.Show()
    End Sub

    Private Async Sub GénérerProgrammeToolStripMenuItem_Click(sender As Object, e As EventArgs)
        ' Crée le programme du cinéma à partir d'un fichier fourni par Hélène
        Using ofd As New OpenFileDialog
            ofd.Title = "Sélectionnez le fichier du programme"
            ofd.Filter = "Fichiers TXT|*.txt|Fichiers CSV|*.csv|Tous les fichiers|*.*"

            ' Si l'utilisateur choisit un fichier
            If ofd.ShowDialog = DialogResult.OK Then
                Dim sCheminFichier = ofd.FileName

                Dim service As New ProgrammeCinemaCreator

                ' On attend que la liste soit enrichie / chargée
                Await service.ChargerProgramme(sCheminFichier)

                Logger.INFO($"Programme {sCheminFichier} chargé et enrichi avec succès !")
                MessageBox.Show($"Programme {sCheminFichier} chargé et enrichi avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                ' L'utilisateur a annulé
                Logger.INFO("Opération annulée.")
                MessageBox.Show("Opération annulée.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Using
    End Sub

    Private Sub ImporterExcelToolStripMenuItem_Click(sender As Object, e As EventArgs)
        GenereAttestationNonParticipation.ImporterParticipantsDepuisExcel()
        MsgBox("Import terminé. CR dans les logs")
    End Sub

    Private Sub GénérerAttestationsToolStripMenuItem_Click(sender As Object, e As EventArgs)
        ' Appel direct via la classe (plus besoin de Dim mdn As New...)
        GenereAttestationNonParticipation.GenererEtEnvoyerAttestations()

        MsgBox("Génération terminée. CR dans les logs")
    End Sub

    Protected Overridable Function DemanderEnvironnement() As String
        Return InputBox(
            "Entrez l'environnement (DEV / TEST / PROD)." & vbCrLf &
            "Environnement actuel : " & GetEnvironmentLabel(),
            "Changer d'environnement")
    End Function

    Private Sub EnvironnementToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EnvironnementToolStripMenuItem.Click
        If _modeDemonstration Then Return

        Dim choix = DemanderEnvironnement()

        If String.IsNullOrWhiteSpace(choix) Then Exit Sub

        Dim previousEnvironment = EnvironmentManager.CurrentEnvironment
        Select Case choix.Trim().ToUpperInvariant()
            Case "DEV"
                SetEnvironment(EnvironnementType.DEV)
            Case "TEST"
                SetEnvironment(EnvironnementType.TEST)
            Case "PROD"
                SetEnvironment(EnvironnementType.PROD)
            Case Else
                MessageBox.Show("Valeur non reconnue. Utilisez DEV, TEST ou PROD.")
                Exit Sub
        End Select

        Dim nextEnvironmentLabel = GetEnvironmentLabel()
        EnvironmentManager.CurrentEnvironment = previousEnvironment
        MessageBox.Show("Environnement au prochain démarrage : " & nextEnvironmentLabel &
                    vbCrLf &
                    "Redémarrez l'application pour appliquer les changements.",
                    "Environnement modifié",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
    End Sub


    Private Sub ChargerRelevéToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ChargerRelevéToolStripMenuItem.Click
        FrmChargeRelevé.Show()
    End Sub

    Private Sub TraiteRelevéToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TraiteRelevéToolStripMenuItem.Click
        Call FrmChargeRelevé.AlimenteLstMvtCA(LectureProprietes.GetCheminEtVariable("ficRelevéTraité"))
        FrmChargeRelevé.Show()
    End Sub

    Private Sub ConsultationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConsultationToolStripMenuItem.Click
        Call ChargerDgvPrincipale()
    End Sub

    Private Sub SaisieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaisieToolStripMenuItem.Click
        frmSaisie.Show()
    End Sub

    Private Sub BatchGénériqueToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BatchGénériqueToolStripMenuItem.Click
        Dim config = BatchMailConfig.Load(selectionneJson)
        Dim batch As New BatchMailSender()
        Dim rapport = batch.RunBatch(config)

        Logger.INFO(rapport.ToText())

    End Sub
    Private Function selectionneJson() As String
        Dim dlg As New OpenFileDialog With {
            .Title = "Sélectionnez le fichier de configuration JSON",
            .Filter = "Fichiers JSON (*.json)|*.json|Tous les fichiers|*.*",
            .InitialDirectory = "G:\Mon Drive\AGUMAAA\Documents",
            .Multiselect = False
        }

        If dlg.ShowDialog() = DialogResult.OK Then
            Return dlg.FileName
            Logger.INFO($"Fichier paramètre batch sélectionné : {dlg.FileName}")
        Else
            MessageBox.Show("Aucun fichier sélectionné.", "Annulé", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End
        End If

    End Function

    Private Sub ListeUtilisateursToolStripMenuItem_Click(sender As Object, e As EventArgs)
        AppelIReseau.TestIReseau()
    End Sub

    Private Sub NouveauTiersToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NouveauTiersToolStripMenuItem.Click
        FrmNouveauTiers.Show()
    End Sub

    Private Sub ExportDesDonnéesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportDesDonnéesToolStripMenuItem.Click
        ' Appel de la procédure
        '_ServiceExport.ExecuterExportSQL(LectureProprietes.GetVariable("dossierDestinationExportData"))
        DatabaseAutomation.ExportDatabase()
    End Sub

    Private Sub ImportDesDonnéesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportDesDonnéesToolStripMenuItem.Click
        Dim confirm = MessageBox.Show("Attention : L'importation écrasera les données actuelles. Continuer ?",
                                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If confirm = DialogResult.Yes Then
            DatabaseAutomation.ImportDatabase()
        End If
    End Sub

    Private Sub FichierToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FichierToolStripMenuItem.Click

    End Sub

    Private Sub RécupDesPJReçuesParMailToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RécupDesPJReçuesParMailToolStripMenuItem.Click
        If _modeDemonstration OrElse EnvironmentManager.CurrentEnvironment = EnvironmentManager.EnvironnementType.TEST Then Return

        Dim fichiers = EmailProcessor.DownloadAttachments(requestStartDate:=AddressOf DemanderDateInitialeGmail).
            Where(AddressOf DocumentFilePreview.Supports).ToArray()
        If fichiers.Length = 0 Then
            MessageBox.Show(Me, "Aucun nouveau PDF ou scan à examiner. Consultez le rapport d'import Gmail pour le détail.",
                            "Téléchargement Gmail", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using formulaire As New FrmDocumentFilePreview(fichiers)
            formulaire.ShowDialog(Me)
        End Using
    End Sub
    Private Function DemanderDateInitialeGmail() As DateTime?
        Using dialogue As New Form With {
            .Text = "Premier téléchargement Gmail", .ClientSize = New Drawing.Size(420, 125),
            .FormBorderStyle = FormBorderStyle.FixedDialog, .StartPosition = FormStartPosition.CenterParent,
            .MinimizeBox = False, .MaximizeBox = False}
            Dim indication As New Label With {
                .Text = "Télécharger les pièces jointes reçues à partir de cette date (incluse).",
                .Left = 12, .Top = 12, .Width = 396}
            Dim dateDebut As New DateTimePicker With {.Left = 12, .Top = 42, .Width = 240, .MaxDate = DateTime.Today}
            Dim bouton As New Button With {
                .Text = "Télécharger", .Left = 190, .Top = 86, .Width = 105, .DialogResult = DialogResult.OK}
            Dim annuler As New Button With {
                .Text = "Annuler", .Left = 303, .Top = 86, .Width = 105, .DialogResult = DialogResult.Cancel}
            dialogue.Controls.AddRange(New Control() {indication, dateDebut, bouton, annuler})
            dialogue.AcceptButton = bouton
            dialogue.CancelButton = annuler
            If dialogue.ShowDialog(Me) = DialogResult.OK Then Return dateDebut.Value.Date
            Return Nothing
        End Using
    End Function

    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
    '    'Call CreeBilans()
    '    'Call genereBilans.AjouteImage()
    '    Call genereBilans.GenereBilanStructure()
    'End Sub
End Class
