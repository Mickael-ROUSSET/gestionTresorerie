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

    Public Property Properties As Object

    Private Sub BtnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        'Création du fichier LibreOffice Writer
        CreePresentation.LectureBase()
    End Sub
    Private Sub FrmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            'Initialisation de la lecture des propriétés
            Dim lectureProprietes As New LectureProprietes()
            'Lecture du niveau de log demandé
            Logger.SetLogLevel(LectureProprietes.GetVariable(Constantes.paramNiveauLog))
            Logger.DBG("Initialisation de : {lectureProprietes}")
            ''Charge les couples clé / valeur
            'GererJson.LoadParametersFromFile()
            'Récupère le rang des colonnes du datagridview
            initIndiceColDgv()
            'Charger dgvPrincipale avec le contenu de la table mouvements
            Call ChargerDgvPrincipale()
            ' Chargement des listes dans le formulaire
            FrmSaisie.chargeListes()
        Catch ex As Exception
            ' Gestion des erreurs
            MessageBox.Show("Une erreur est survenue lors de l'initialisation : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.ERR("Une erreur est survenue lors de l'initialisation : " & ex.Message)
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
            Dim cmd = SqlCommandBuilder.CreateSqlCommand("sqlSelectMouvementsLibelles")
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
        ' Alimenter la colonne d'image pour l'état 

        ' Parcourir les lignes du DataGridView pour définir les images
        For Each row As DataGridViewRow In dgvPrincipale.Rows
            If Not row.IsNewRow Then
                Try
                    ' 1 correspond à la colonne "etat" après l'insertion de la nouvelle colonne
                    Dim etat As Object = row.Cells(1).Value
                    If etat IsNot Nothing AndAlso TypeOf etat Is Boolean Then
                        row.Cells("etatImage").Value = If(CType(etat, Boolean), My.Resources.OK, My.Resources.KO)
                    Else
                        Logger.ERR($"Valeur invalide pour la colonne 'etat' dans la ligne {row.Index}: {etat}")
                        row.Cells("etatImage").Value = My.Resources.KO ' Par défaut, si la valeur est invalide
                    End If
                Catch ex As Exception
                    Logger.ERR($"Erreur lors de la définition de l'image pour la colonne 'etat' dans la ligne {row.Index}: {ex.Message}")
                    row.Cells("etatImage").Value = My.Resources.KO ' Par défaut, en cas d'erreur
                End Try
            End If
        Next
    End Sub
    Private Sub FrmMain_Closing(sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        ConnexionDB.GetInstance.Dispose()
    End Sub
    Private Sub BtnSaisie_Click(sender As Object, e As EventArgs) Handles btnSaisie.Click
        FrmSaisie.Show()
    End Sub
    Private Sub BtnChargeRelevé_Click(sender As Object, e As EventArgs) Handles btnChargeRelevé.Click
        FrmChargeRelevé.Show()
    End Sub
    Private Sub BtnConsultation_Click(sender As Object, e As EventArgs) Handles btnConsultation.Click
        Call ChargerDgvPrincipale()
    End Sub
    Private Sub FermerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FermerToolStripMenuItem.Click
        Me.Close()
        End
    End Sub
    Private Sub dgvPrincipale_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPrincipale.CellContentClick
        Try
            ' Récupérer les valeurs de la ligne sélectionnée
            Dim currentRow As DataGridViewRow = Me.dgvPrincipale.CurrentRow
            If currentRow Is Nothing Then
                Return
            End If

            Dim note As String = currentRow.Cells(_icolNote).Value?.ToString()
            Dim dateMvt As Date = Utilitaires.ConvertToDate(currentRow.Cells(_icolDateMvt).Value)
            Dim montant As Double = Utilitaires.ConvertToDouble(currentRow.Cells(_icolMontant).Value)
            Dim sens As Boolean = Utilitaires.ConvertToBoolean(currentRow.Cells(_icolSens).Value)
            Dim rapproche As Boolean = Utilitaires.ConvertToBoolean(currentRow.Cells(_icolModifiable).Value)
            Dim remise As String = currentRow.Cells(_icolNote).Value?.ToString()
            'Dim categorie As String = currentRow.Cells(_icolCategorie).Value?.ToString()
            'Dim sousCategorie As String = currentRow.Cells(_icolSousCategorie).Value?.ToString()
            'Dim tiers As String = currentRow.Cells(_icolTiers).Value?.ToString()
            'Dim evenement As String = currentRow.Cells(_icolEvenement).Value?.ToString()
            'Dim monType As String = currentRow.Cells(_icolType).Value?.ToString()
            'Dim etat As Boolean = ConvertToBoolean(currentRow.Cells(_icolEtat).Value)

            ' Charger les valeurs dans le formulaire de saisie
            With FrmSaisie
                .chargeListes()
                .dateMvt.Value = dateMvt
                .txtNote.Text = note
                .rbDebit.Checked = sens
                .txtMontant.Text = montant.ToString()
                .txtRemise.Text = remise
                .rbRapproche.Checked = rapproche
                .Show()
            End With
        Catch ex As Exception
            MsgBox("Une erreur est survenue : " & ex.Message, MsgBoxStyle.Critical)
            Logger.ERR("Une erreur est survenue : " & ex.Message)
        End Try
    End Sub
    Private Sub btnBatch_Click(sender As Object, e As EventArgs) Handles btnBatch.Click
        Dim batch As New batchAnalyseChq

        Call batch.ParcourirRepertoireEtAnalyser()
    End Sub
    Private Sub btnTraiteRelevé_Click(sender As Object, e As EventArgs) Handles btnTraiteRelevé.Click
        Call FrmChargeRelevé.AlimenteLstMvtCA(LectureProprietes.GetCheminEtVariable("ficRelevéTraité"))
        FrmChargeRelevé.Show()
    End Sub

    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
    '    'Call CreeBilans()
    '    'Call genereBilans.AjouteImage()
    '    Call genereBilans.GenereBilanStructure()
    'End Sub
End Class