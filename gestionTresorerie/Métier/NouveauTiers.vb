Imports System.Data.SqlClient

Public Class frmNouveauTiers
    Private Sub btnCreerTiers_Click(sender As Object, e As EventArgs) Handles btnCreerTiers.Click
        If String.IsNullOrWhiteSpace(txtNom.Text) AndAlso String.IsNullOrWhiteSpace(txtPrenom.Text) AndAlso String.IsNullOrWhiteSpace(txtRaisonSociale.Text) Then
            MessageBox.Show("Veuillez remplir le nom et le prénom ou la raison sociale.", "Champs obligatoires", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim listeTiers As ListeTiers
        ' Vérifier que le tiers n'existe pas déjà
        If TiersExisteDeja() Then
            MessageBox.Show("Ce tiers existe déjà.", "Tiers existant", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers()
        End If
        ' Vérifier la sélection dans les DataGridView
        Dim categorieId As Integer = GetSelectedRowValue(dgvNTCategorie, 0, "Veuillez sélectionner une catégorie.")
        Dim sousCategorieId As Integer = GetSelectedRowValue(dgvNTSousCategorie, 0, "Veuillez sélectionner une sous-catégorie.")

        insereNouveauTiers(txtRaisonSociale.Text,
                       txtNom.Text,
                       txtPrenom.Text,
                       If(categorieId = -1, Nothing, categorieId),
                       If(sousCategorieId = -1, Nothing, sousCategorieId)
                       )
        'On réinitialise les zones de saisie pour création éventuelle d'un nouveau tiers
        initChamps()
    End Sub

    Private Function GetSelectedRowValue(dataGridView As DataGridView, columnIndex As Integer, errorMessage As String) As Integer
        If dataGridView.SelectedRows.Count > 0 Then
            Return CInt(dataGridView.SelectedRows(0).Cells(columnIndex).Value)
        Else
            Return -1 ' Retourne une valeur par défaut pour indiquer une erreur
        End If
    End Function

    Private Function TiersExisteDeja() As Boolean
        Dim query As String = "SELECT COUNT(*) FROM Tiers WHERE (nom = @nom AND prenom = @prenom and raisonSociale is null) OR raisonSociale = @raisonSociale and nom is null AND prenom is null "
        Dim count As Integer = 0

        Try
            Dim conn As SqlConnection = connexionDB.GetInstance.getConnexion
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@nom", txtNom.Text.Trim())
                cmd.Parameters.AddWithValue("@prenom", txtPrenom.Text.Trim())
                cmd.Parameters.AddWithValue("@raisonSociale", txtRaisonSociale.Text.Trim())

                count = CInt(cmd.ExecuteScalar())
            End Using
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la vérification de l'existence du tiers : {ex.Message}")
            Throw
        End Try

        Return count > 0
    End Function
    Sub insereNouveauTiers(sRaisonSociale As String, sPrenom As String, sNom As String, iCategorie As Integer?, iSousCategorie As Integer?)
        Dim query As String = "INSERT INTO Tiers (nom, prenom, raisonSociale, categorieDefaut, sousCategorieDefaut, dateCreation, dateModification) VALUES (@nom, @prenom, @raisonSociale, @categorieDefaut, @sousCategorieDefaut, @dateCreation, @dateModification)"

        Try
            Dim conn As SqlConnection = connexionDB.GetInstance.getConnexion
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@nom", sNom.Trim())
                cmd.Parameters.AddWithValue("@prenom", sPrenom.Trim())
                cmd.Parameters.AddWithValue("@raisonSociale", sRaisonSociale)
                cmd.Parameters.AddWithValue("@categorieDefaut", iCategorie)
                cmd.Parameters.AddWithValue("@sousCategorieDefaut", iSousCategorie)
                cmd.Parameters.AddWithValue("@dateCreation", DateTime.Now)
                cmd.Parameters.AddWithValue("@dateModification", DateTime.Now)

                cmd.ExecuteNonQuery()
            End Using

            Logger.INFO("Nouveau tiers inséré avec succès.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'insertion du nouveau tiers : {ex.Message}")
            Throw
        End Try
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

    Private Sub txtCategorie_Leave(sender As Object, e As EventArgs) Handles txtCategorie.Leave
        ValidateNumericField(dgvNTCategorie, "Catégorie")
    End Sub

    Private Sub txtSousCategorie_Leave(sender As Object, e As EventArgs) Handles txtSousCategorie.Leave
        ValidateNumericField(dgvNTSousCategorie, "Sous-Catégorie")
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
            MessageBox.Show($"Le champ {fieldName} doit être un nombre entier.", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            datagridview.Focus() ' Remettre le focus sur le champ pour corriger l'erreur
            Return
        End If
    End Sub

    Private Sub frmNouveauTiers_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        initChamps
        ChargeDgvCategorie()
        ChargeDgvSousCategorie()
    End Sub
    Private Sub initChamps()
        txtNom.Text = Constantes.Vide
        txtPrenom.Text = Constantes.Vide
        txtRaisonSociale.Text = Constantes.Vide
        dgvNTCategorie.ClearSelection()
        dgvNTSousCategorie.ClearSelection()
    End Sub
    Private Sub ChargeDgvCategorie()
        Dim categorie As New Categorie()
        Dim query As String = "SELECT id, libelle FROM Categorie"

        ChargerDonneesNouveauTiers(categorie, query, dgvNTCategorie)
    End Sub

    Private Sub ChargeDgvSousCategorie()
        Dim sousCategorie As New SousCategorie()
        Dim query As String = "SELECT id, libelle FROM SousCategorie"

        ChargerDonneesNouveauTiers(sousCategorie, query, dgvNTSousCategorie)
    End Sub
    Private Sub ChargerDonneesNouveauTiers(dataService As IDataService, query As String, dataGridView As DataGridView)

        Try
            Dim dt As DataTable = dataService.ExecuterRequete(query)

            dataGridView.DataSource = dt
            dataGridView.Columns("id").Visible = False
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
            MessageBox.Show($"Une erreur SQL s'est produite lors du chargement des données !{vbCrLf}{ex.ToString()}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors du chargement des données. Message: {ex.Message}")
            MessageBox.Show($"Une erreur inattendue s'est produite !{vbCrLf}{ex.ToString()}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class