Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.SqlClient

Public Class FrmChangePassword

    Private Sub FrmChangePassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Modifier mon mot de passe"
        Me.AcceptButton = btnValider
        Me.CancelButton = btnAnnuler
    End Sub

    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        If String.IsNullOrWhiteSpace(txtAncien.Text) OrElse
           String.IsNullOrWhiteSpace(txtNouveau.Text) OrElse
           String.IsNullOrWhiteSpace(txtConfirm.Text) Then
            MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If txtNouveau.Text <> txtConfirm.Text Then
            MessageBox.Show("Le nouveau mot de passe et la confirmation ne correspondent pas.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Try
            ' Vérification de l’ancien mot de passe
            If Not VerifierAncienMotDePasse(UtilisateurActif.NomUtilisateur, txtAncien.Text) Then
                MessageBox.Show("Ancien mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            ' Mise à jour du mot de passe
            Dim hash = HacherMotDePasse(txtNouveau.Text)
            SqlCommandBuilder.CreateSqlCommand("updateMotDePasse",
                             New Dictionary(Of String, Object) From {
                                 {"@id", UtilisateurActif.Id},
                                 {"@pwd", hash}
                             }).ExecuteNonQuery()

            MessageBox.Show("Mot de passe mis à jour avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Erreur lors de la mise à jour du mot de passe : " & ex.Message,
                            "Erreur SQL", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        Me.Close()
    End Sub

    Private Function HacherMotDePasse(mdp As String) As String
        Using sha = SHA256.Create()
            Dim bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(mdp))
            Return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant()
        End Using
    End Function

    Private Function VerifierAncienMotDePasse(nomUtilisateur As String, ancienMdp As String) As Boolean
        Using cn As New SqlConnection(LectureProprietes.connexionString)
            cn.Open()

            Dim cmd As New SqlCommand("
                SELECT COUNT(*) 
                FROM Utilisateurs 
                WHERE NomUtilisateur = @nom 
                AND MotDePasse = @pwd 
                AND Actif = 1", cn)

            cmd.Parameters.AddWithValue("@nom", nomUtilisateur)
            cmd.Parameters.AddWithValue("@pwd", HacherMotDePasse(ancienMdp))

            Dim count As Integer = CInt(cmd.ExecuteScalar())
            Return count > 0
        End Using
    End Function

End Class
