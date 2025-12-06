Imports System.Data.SqlClient
Imports System.Windows.Forms

Public Class FrmNouveauTiers
    Private _categorieSelectionne As Categorie
    Private _sousCategorieSelectionne As SousCategorie
    Private _pendingCoordonnees As Coordonnees
    Private toolTip1 As ToolTip

    Private Sub btnCreerTiers_Click(sender As Object, e As EventArgs) Handles btnCreerTiers.Click
        If String.IsNullOrWhiteSpace(txtNom.Text) AndAlso String.IsNullOrWhiteSpace(txtPrenom.Text) AndAlso String.IsNullOrWhiteSpace(txtRaisonSociale.Text) Then
            MessageBox.Show($"Veuillez remplir le nom : {txtNom.Text} et le prénom {txtPrenom.Text} ou la raison sociale {txtRaisonSociale.Text}", "Champs obligatoires", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim listeTiers As ListeTiers
        ' Vérifier que le tiers n'existe pas déjà
        If TiersExisteDeJa() Then
            MessageBox.Show($"Ce tiers existe déjà : nom = {txtNom.Text}, prénom = {txtPrenom.Text}, raison sociale = {txtRaisonSociale.Text}", "Tiers existant", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Logger.INFO($"Ce tiers existe déjà : nom = {txtNom.Text}, prénom = {txtPrenom.Text}, raison sociale = {txtRaisonSociale.Text}")
            Return
        End If

        If listeTiers Is Nothing Then
            Dim unused2 As New ListeTiers()
        End If

        ' Insérer le tiers et récupérer son Id
        Dim newId As Integer = insereNouveauTiers(txtRaisonSociale.Text, txtNom.Text, txtPrenom.Text, _categorieSelectionne.Id, _sousCategorieSelectionne.Id)

        ' Si des coordonnées ont été saisies, les persister maintenant avec l'Id du tiers
        If _pendingCoordonnees IsNot Nothing Then
            Try
                _pendingCoordonnees.IdTiers = newId
                SaveCoordonnees(_pendingCoordonnees)
                Logger.INFO($"Coordonnées sauvegardées pour IdTiers={newId} : {_pendingCoordonnees.ToString}")
            Catch ex As Exception
                Logger.ERR($"Erreur lors de la sauvegarde des coordonnées pour IdTiers={newId} : {ex.Message}")
            End Try
            _pendingCoordonnees = Nothing
        End If

        initChamps()
    End Sub

    Private Function verifCatSousCat(text As String) As Integer
        Dim indice As Integer = 0
        If Not String.IsNullOrWhiteSpace(text) Then
            Dim tmp As Integer
            If Integer.TryParse(text.Trim(), tmp) Then
                indice = tmp
            End If
        End If
        Return indice
    End Function

    Private Function GetSelectedRowValue(dataGridView As DataGridView, columnIndex As Integer) As Integer
        If dataGridView.SelectedRows.Count > 0 Then
            Return CInt(dataGridView.SelectedRows(0).Cells(columnIndex).Value)
        Else
            Return -1 ' Retourne une valeur par défaut pour indiquer une erreur
        End If
    End Function

    Private Function TiersExisteDeJa() As Boolean
        Dim count As Integer

        Try
            count = CInt(SqlCommandBuilder.
                     CreateSqlCommand(Constantes.bddAgumaaa, "cptTiers",
                     New Dictionary(Of String, Object) From {{"@nom", txtNom.Text.Trim()},
                                                             {"@prenom", txtPrenom.Text.Trim()},
                                                             {"@raisonSociale", txtRaisonSociale.Text.Trim()}}
                     ).
                     ExecuteScalar()
                     )
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la vérification de l'existence du tiers : {ex.Message}")
            Throw
        End Try

        Return count > 0
    End Function

    ' Retourne l'Id du tiers inséré (suppose que insertTiers renvoie SCOPE_IDENTITY())
    Public Shared Function insereNouveauTiers(sRaisonSociale As String, sPrenom As String, sNom As String, iCategorie As Integer?, iSousCategorie As Integer?) As Integer
        Try
            Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "insertTiers",
                             New Dictionary(Of String, Object) From {{"@nom", sNom.Trim()},
                                                                     {"@prenom", sPrenom.Trim()},
                                                                     {"@raisonSociale", sRaisonSociale},
                                                                     {"@categorieDefaut", iCategorie},
                                                                     {"@sousCategorieDefaut", iSousCategorie},
                                                                     {"@dateCreation", DateTime.Now},
                                                                     {"@dateModification", DateTime.Now}}
                             )
                Dim result = cmd.ExecuteScalar()
                Dim newId As Integer = 0
                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                    Integer.TryParse(result.ToString(), newId)
                End If
                Logger.INFO("Nouveau tiers inséré avec succès. Id=" & newId)
                Return newId
            End Using
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'insertion du nouveau tiers : {ex.Message}")
            Throw
        End Try
    End Function

    ' Sauvegarde des coordonnées (réutilise les noms de paramètres attendus par insertCoordonnees/updateCoordonnees)
    Private Sub SaveCoordonnees(coordonnee As Coordonnees)
        Dim parametres As New Dictionary(Of String, Object) From {
            {"@Id", coordonnee.Id},
            {"@IdTiers", coordonnee.IdTiers},
            {"@Rue1", coordonnee.Rue1},
            {"@Rue2", coordonnee.Rue2},
            {"@CodePostal", coordonnee.CodePostal},
            {"@Ville", coordonnee.Ville},
            {"@Pays", coordonnee.Pays},
            {"@Email", If(String.IsNullOrWhiteSpace(coordonnee.Email), DBNull.Value, coordonnee.Email)},
            {"@Telephone", If(String.IsNullOrWhiteSpace(coordonnee.Telephone), DBNull.Value, coordonnee.Telephone)}
        }
        Dim reqSqlCoord As String = If(coordonnee.Id = 0, "insertCoordonnees", "updateCoordonnees")
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, reqSqlCoord, parametres)
            If coordonnee.Id = 0 Then
                coordonnee.Id = Convert.ToInt32(cmd.ExecuteScalar())
            Else
                cmd.ExecuteNonQuery()
            End If
        End Using
    End Sub

    Private Sub rbPersonneMorale_CheckedChanged(sender As Object, e As EventArgs) Handles rbPersonneMorale.CheckedChanged
        UpdateControlVisibility(rbPersonneMorale.Checked)
    End Sub

    Private Sub UpdateControlVisibility(isPersonneMorale As Boolean)
        txtRaisonSociale.Visible = isPersonneMorale
        txtNom.Visible = Not isPersonneMorale
        txtPrenom.Visible = Not isPersonneMorale
        lblRaisonSociale.Visible = isPersonneMorale
        lblNom.Visible = Not isPersonneMorale
        lblPrenom.Visible = Not isPersonneMorale
    End Sub

    Private Shared Sub ValidateNumericField(datagridview As DataGridView, fieldName As String)
        ' Vérifier que le champ est renseigné et numérique
        Dim fieldText As String = datagridview.SelectedRows(0).Cells.ToString.Trim

        If String.IsNullOrEmpty(fieldText) Then
            MessageBox.Show($"Le champ {fieldName} ne peut pas être vide.", "Champ obligatoire", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            datagridview.Focus() ' Remettre le focus sur le champ pour corriger l'erreur
            Return
        End If

        Dim fieldValue As Integer
        If Not Integer.TryParse(fieldText, fieldValue) Then
            MessageBox.Show($"Le champ {fieldName} doit être un nombre entier : {fieldValue}", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            datagridview.Focus() ' Remettre le focus sur le champ pour corriger l'erreur
            Return
        End If
    End Sub

    Private Sub frmNouveauTiers_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        initChamps()
        ' Initialiser le tooltip pour btnCreerTiers
        Try
            ttCoordonnes = New ToolTip()
            ttCoordonnes.IsBalloon = False
            ttCoordonnes.ToolTipIcon = ToolTipIcon.Info
            ttCoordonnes.AutoPopDelay = 5000
            ttCoordonnes.InitialDelay = 500
            ttCoordonnes.ReshowDelay = 250
            ttCoordonnes.SetToolTip(btnCreerTiers, "Téléphone et e‑mail sont saisis dans la fenêtre 'Coordonnées'.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'initialisation du tooltip : {ex.Message}")
        End Try
    End Sub
    Private Sub initChamps()
        txtNom.Text = String.Empty
        txtPrenom.Text = String.Empty
        txtRaisonSociale.Text = String.Empty
    End Sub
    Private Sub ChargerDonneesNouveauTiers(dataService As IDataService, query As String, dataGridView As DataGridView)
        'TODO : utiliser la méthode de la classe UtilitairesDgv
        Try
            Dim dt As DataTable = dataService.ExecuterRequete(query)

            dataGridView.DataSource = dt
            dataGridView.Columns("Id").Visible = False
            dataGridView.Columns("libelle").Visible = True

            ' Effacer toute sélection initiale
            dataGridView.ClearSelection()

            Logger.INFO($"Chargement des données réussi : {dataGridView.RowCount}")

            ' Vérifie si le DataGridView est vide
            If dataGridView.Rows.Count = 0 Then
                Logger.INFO($"Aucune ligne n'a été trouvée pour la requête spécifiée.")
                Return
            End If
        Catch ex As SqlException
            Logger.ERR($"Erreur SQL lors du chargement des données. Message: {ex.Message}")
            MessageBox.Show($"Une erreur SQL s'est produite lors du chargement des données !{vbCrLf}{ex}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors du chargement des données. Message: {ex.Message}")
            MessageBox.Show($"Une erreur inattendue s'est produite !{vbCrLf}{ex}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Private Sub txtTelephone_LostFocus(sender As Object, e As EventArgs)
    '    'Vérifie la validité du téléphone
    '    If Not Coordonnees.ValiderTelephone(txtTelephone.Text) Then
    '        txtTelephone.Focus()
    '    End If
    'End Sub

    'Private Sub txtMail_LostFocus(sender As Object, e As EventArgs)
    '    'Vérifie la validité de l'email
    '    If Not Coordonnees.ValiderEmail(txtMail.Text) Then
    '        txtMail.Focus()
    '    End If
    'End Sub

    Private Sub btnCoordonnees_Click(sender As Object, e As EventArgs) Handles btnCoordonnees.Click
        Dim idTiers As Integer? = Tiers.GetIdTiersByUser(txtNom.Text, txtPrenom.Text)
        If idTiers = 0 Then
            Logger.WARN($"Impossible de récupérer IdTiers pour {txtNom.Text}, {txtPrenom.Text}.")
        End If
        ' Ouvrir le formulaire en mode sans enregistrement ; récupérer les coordonnées saisies
        Dim frm As New frmSaisieCoordonnees(idTiers, False)
        If frm.ShowDialog() = DialogResult.OK Then
            _pendingCoordonnees = frm.Result
            Logger.INFO("Coordonnées en attente pour le nouveau tiers.")
        End If
    End Sub

    Private Sub btnCategorie_Click(sender As Object, e As EventArgs) Handles btnCategorie.Click
        _categorieSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Categorie)(
        nomRequete:="selIdLibCat",
        titreFenetre:="Sélection de la catégorie",
        txtDestination:=txtCategorie
    )
        txtCategorie.Text = _categorieSelectionne.Libelle
    End Sub

    Private Sub btnSousCategorie_Click(sender As Object, e As EventArgs) Handles btnSousCategorie.Click
        _sousCategorieSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of SousCategorie)(
        nomRequete:="sqlSelSousCategoriesTout",
        titreFenetre:="Sélection de la sous-catégorie",
        txtDestination:=txtSousCategorie
    )
        txtSousCategorie.Text = _sousCategorieSelectionne.Libelle
    End Sub

    Private Sub dtpDateNaissance_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateNaissance.ValueChanged
        Try
            Dim selectedDate As DateTime = dtpDateNaissance.Value
            txtDateNaissance.Text = selectedDate.ToString("dd/MM/yyyy")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la mise à jour de txtDateNaissance : {ex.Message}")
        End Try
    End Sub
End Class