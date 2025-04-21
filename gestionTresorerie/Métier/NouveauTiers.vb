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
        insereNouveauTiers(listeTiers, txtRaisonSociale.Text, txtNom.Text, txtPrenom.Text, CInt(txtCategorie.Text), CInt(txtSousCategorie.Text))

    End Sub
    Private Function TiersExisteDeja() As Boolean
        Dim query As String = "SELECT COUNT(*) FROM Tiers WHERE (nom = @nom AND prenom = @prenom) OR raisonSociale = @raisonSociale"
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
    Sub insereNouveauTiers(listeTiers As ListeTiers, sRaisonSociale As String, sPrenom As String, sNom As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0)
        Dim neoTiers As Tiers

        neoTiers = listeTiers.Add(sRaisonSociale, iCategorie, iSousCategorie)
        Call InsererNouveauTiers()
    End Sub
    Private Sub InsererNouveauTiers()
        Dim query As String = "INSERT INTO Tiers (nom, prenom, raisonSociale, categorieDefaut, sousCategorieDefaut, dateCreation, dateModification) VALUES (@nom, @prenom, @raisonSociale, @categorieDefaut, @sousCategorieDefaut, @dateCreation, @dateModification)"

        Try
            Using conn As SqlConnection = connexionDB.GetInstance.getConnexion
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@nom", txtNom.Text.Trim())
                    cmd.Parameters.AddWithValue("@prenom", txtPrenom.Text.Trim())
                    cmd.Parameters.AddWithValue("@raisonSociale", txtRaisonSociale.Text.Trim())
                    cmd.Parameters.AddWithValue("@categorieDefaut", Convert.ToInt32(txtCategorie.Text.Trim()))
                    cmd.Parameters.AddWithValue("@sousCategorieDefaut", Convert.ToInt32(txtSousCategorie.Text.Trim()))
                    cmd.Parameters.AddWithValue("@dateCreation", DateTime.Now)
                    cmd.Parameters.AddWithValue("@dateModification", DateTime.Now)

                    cmd.ExecuteNonQuery()
                End Using
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
        ValidateNumericField(txtCategorie, "Catégorie")
    End Sub

    Private Sub txtSousCategorie_Leave(sender As Object, e As EventArgs) Handles txtSousCategorie.Leave
        ValidateNumericField(txtSousCategorie, "Sous-Catégorie")
    End Sub

    Private Shared Sub ValidateNumericField(textBox As TextBox, fieldName As String)
        ' Vérifier que le champ est renseigné et numérique
        Dim fieldText As String = textBox.Text.Trim()

        If String.IsNullOrEmpty(fieldText) Then
            MessageBox.Show($"Le champ {fieldName} ne peut pas être vide.", "Champ obligatoire", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            textBox.Focus() ' Remettre le focus sur le champ pour corriger l'erreur
            Return
        End If

        Dim fieldValue As Integer
        If Not Integer.TryParse(fieldText, fieldValue) Then
            MessageBox.Show($"Le champ {fieldName} doit être un nombre entier.", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            textBox.Focus() ' Remettre le focus sur le champ pour corriger l'erreur
            Return
        End If
    End Sub

End Class