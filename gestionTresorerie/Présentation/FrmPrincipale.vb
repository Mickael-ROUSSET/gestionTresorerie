Imports System.Data.SqlClient

Public Class FrmPrincipale
    Inherits System.Windows.Forms.Form

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

    Private Sub FrmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            EnvironmentManager.LoadEnvironment()
            Logger.INFO("Environnement actif : " & EnvironmentManager.GetEnvironmentLabel())
            StatusLabelEnv.Text = "Environnement : " & EnvironmentManager.GetEnvironmentLabel

            'Initialisation de la lecture des propriétés
            Dim lectureProprietes As New LectureProprietes()
            'Lecture du niveau de log demandé
            Logger.SetLogLevel(LectureProprietes.GetVariable(Constantes.paramNiveauLog))
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
            ' Créer une commande SQL 
            Dim cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "sqlSelectMouvementsLibelles")
            ' Créer un DataAdapter pour remplir le DataTable
            Using adapter As New SqlDataAdapter(cmd)
                ' Créer un DataTable pour stocker les données
                Dim dataTable As New DataTable()
                ' Remplir le DataTable avec les données de la base de données
                adapter.Fill(dataTable)
                ' Lier le DataTable au DataGridView
                dgvPrincipale.DataSource = dataTable
            End Using
            ' Ajouter la colonne d'image pour l'état du mouvement
            AjouterColonneEtatImage()

            ' Écrire un log d'information
            Logger.INFO("Chargement des données dans dgvPrincipale réussi.")
        Catch ex As SqlException
            ' Écrire un log d'erreur en cas d'exception SQL
            Logger.ERR($"Erreur SQL lors du chargement des données dans dgvPrincipale : {ex.Message}")
        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception générale
            Logger.ERR($"Erreur lors du chargement des données dans dgvPrincipale : {ex.Message}")
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
        ConnexionDB.GetInstance(Constantes.bddAgumaaa).Dispose()
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
        GestionBDD.SauvegarderBase()
    End Sub

    Private Sub RestaurerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RestaurerToolStripMenuItem.Click
        Dim ofd As New OpenFileDialog With {
    .Filter = "Sauvegardes SQL (*.bak)|*.bak",
    .InitialDirectory = GestionBDD.DossierSauvegarde
}
        If ofd.ShowDialog() = DialogResult.OK Then
            GestionBDD.RestaurerBase(ofd.FileName)
        End If
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

    '-------------------------------------------------------------
    ' 📌 Procédure mutualisée : création ou recréation de l’agent
    '-------------------------------------------------------------
    Private Async Function ExecuterCreationAgentMistral(forceRecreation As Boolean) As Task
        Try
            Cursor = Cursors.WaitCursor

            If forceRecreation Then
                Logger.INFO("♻️ Recréation forcée de l’agent Mistral...")
            Else
                Logger.INFO("⏳ Vérification / création de l’agent Mistral...")
            End If

            ' Appel à la méthode partagée (par ex. dans TestMistral)
            Dim agentId As String = Await TestMistral.CreeAgent(forceRecreation)

            If Not String.IsNullOrEmpty(agentId) Then
                Dim msg = If(forceRecreation,
                         $"✅ Nouvel agent Mistral créé : {agentId}",
                         $"✅ Agent Mistral prêt : {agentId}")
                MessageBox.Show(msg, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show("⚠️ Impossible de créer ou recharger l’agent Mistral.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

        Catch ex As Exception
            Logger.ERR("❌ Erreur dans ExecuterCreationAgentMistral : " & ex.Message)
            MessageBox.Show("❌ Erreur : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
        End Try
    End Function

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

    Private Sub EnvironnementToolStripMenuItem_Click(sender As Object, e As EventArgs)

        Dim choix = InputBox(
                            "Entrez l'environnement (DEV / TEST / PROD)." & vbCrLf &
                            "Environnement actuel : " & GetEnvironmentLabel(),
                            "Changer d'environnement"
                            )

        If String.IsNullOrWhiteSpace(choix) Then Exit Sub

        Select Case choix.ToUpper
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

        MessageBox.Show("Nouvel environnement actif : " & GetEnvironmentLabel() &
                    vbCrLf &
                    "Redémarrez l'application pour appliquer les changements.",
                    "Environnement modifié",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
    End Sub

    Private Async Sub CréerToolStripMenuItem1_Click(sender As Object, e As EventArgs)
        Dim menu = DirectCast(sender, ToolStripMenuItem)
        Dim force = menu.Name = "RecréerToolStripMenuItem" ' ou menu.Tag=True
        Await ExecuterCreationAgentMistral(False)
    End Sub

    Private Async Sub RecréerforçageToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim menu = DirectCast(sender, ToolStripMenuItem)
        Dim force = menu.Name = "RecréerToolStripMenuItem" ' ou menu.Tag=True
        Await ExecuterCreationAgentMistral(True)
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
        EmailProcessor.ProcessAttachments()
    End Sub
    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
    '    'Call CreeBilans()
    '    'Call genereBilans.AjouteImage()
    '    Call genereBilans.GenereBilanStructure()
    'End Sub
End Class