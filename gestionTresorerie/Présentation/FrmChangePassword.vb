Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class FrmChangePassword
    Friend Property Connexion As String

    Private Sub FrmChangePassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Text = "Modifier mon mot de passe"
        AcceptButton = btnValider
        CancelButton = btnAnnuler
    End Sub
    Private Shared Function CreateUtilisateurRepository() As UtilisateurRepository
        Dim executor As ISqlExecutor = RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New UtilisateurRepository(executor)
    End Function
    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        If String.IsNullOrWhiteSpace(txtAncien.Text) OrElse
           String.IsNullOrWhiteSpace(txtNouveau.Text) OrElse
           String.IsNullOrWhiteSpace(txtConfirm.Text) Then
            Dim unused5 = MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If txtNouveau.Text <> txtConfirm.Text Then
            Dim unused4 = MessageBox.Show("Le nouveau mot de passe et la confirmation ne correspondent pas.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Try
            ' Vérification de l’ancien mot de passe
            If Not VerifierAncienMotDePasse(UtilisateurActif.NomUtilisateur, txtAncien.Text) Then
                Dim unused3 = MessageBox.Show("Ancien mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            ' Mise à jour du mot de passe
            Dim hash = FrmEditUtilisateur.Encoder(txtNouveau.Text)
            Dim nb As Integer
            Using cn As New SqlConnection(Connexion), cmd As New SqlCommand("UPDATE dbo.Utilisateurs SET MotDePasse=@pwd WHERE Id=@id AND Actif=1",cn)
                cn.Open()
                cmd.Parameters.AddWithValue("@pwd",hash)
                cmd.Parameters.AddWithValue("@id",UtilisateurActif.Id)
                nb=cmd.ExecuteNonQuery()
            End Using
            If nb <= 0 Then
                MessageBox.Show("Aucun mot de passe n'a été mis à jour.",
                    "Avertissement",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim unused1 = MessageBox.Show("Mot de passe mis à jour avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
            DialogResult = DialogResult.OK
            Close()

        Catch ex As Exception
            Dim unused = MessageBox.Show("Erreur lors de la mise à jour du mot de passe : " & ex.Message,
                            "Erreur SQL", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        Close()
    End Sub

    Private Function VerifierAncienMotDePasse(nomUtilisateur As String, ancienMdp As String) As Boolean
        Using cn As New SqlConnection(Connexion), cmd As New SqlCommand("SELECT MotDePasse FROM dbo.Utilisateurs WHERE NomUtilisateur=@nom AND Actif=1",cn)
            cn.Open()
            cmd.Parameters.AddWithValue("@nom",nomUtilisateur)
            Dim valeur = TryCast(cmd.ExecuteScalar(),String)
            Return valeur IsNot Nothing AndAlso FrmEditUtilisateur.Verifier(ancienMdp,valeur)
        End Using
    End Function
End Class