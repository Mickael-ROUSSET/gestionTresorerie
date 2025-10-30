Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class FrmGestionUtilisateurs

    Private Sub FrmGestionUtilisateurs_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ChargerUtilisateurs()
    End Sub

    Private Sub ChargerUtilisateurs()
        dgvUtilisateurs.Rows.Clear()
        Try
            Using cn As New SqlConnection(LectureProprietes.connexionString)
                cn.Open()

                Dim cmd As New SqlCommand("SELECT Id, NomUtilisateur, Role, Actif FROM Utilisateurs ORDER BY NomUtilisateur", cn)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        dgvUtilisateurs.Rows.Add(rdr.GetInt32(0),
                                                 rdr.GetString(1),
                                                 rdr.GetString(2),
                                                 rdr.GetBoolean(3))
                    End While
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur lors du chargement : " & ex.Message)
        End Try
    End Sub

    Private Sub btnRecharger_Click(sender As Object, e As EventArgs) Handles btnRecharger.Click
        ChargerUtilisateurs()
    End Sub

    ' 🔹 Ajouter un utilisateur
    Private Sub btnAjouter_Click(sender As Object, e As EventArgs) Handles btnAjouter.Click
        Dim frm As New FrmGestionUtilisateurs()
        If frm.ShowDialog() = DialogResult.OK Then
            ChargerUtilisateurs()
        End If
    End Sub

    ' 🔹 Modifier un utilisateur
    Private Sub btnModifier_Click(sender As Object, e As EventArgs) Handles btnModifier.Click
        If dgvUtilisateurs.SelectedRows.Count = 0 Then Return

        Dim id = CInt(dgvUtilisateurs.SelectedRows(0).Cells("Id").Value)
        Dim nom = dgvUtilisateurs.SelectedRows(0).Cells("NomUtilisateur").Value.ToString()
        Dim role = dgvUtilisateurs.SelectedRows(0).Cells("Role").Value.ToString()
        Dim actif = CBool(dgvUtilisateurs.SelectedRows(0).Cells("Actif").Value)

        Dim frm As New FrmGestionUtilisateurs(id, nom, role, actif)
        If frm.ShowDialog() = DialogResult.OK Then
            ChargerUtilisateurs()
        End If
    End Sub

    ' 🔹 Supprimer (désactiver) un utilisateur
    Private Sub btnSupprimer_Click(sender As Object, e As EventArgs) Handles btnSupprimer.Click
        If dgvUtilisateurs.SelectedRows.Count = 0 Then Return

        Dim id = CInt(dgvUtilisateurs.SelectedRows(0).Cells("Id").Value)
        Dim nom = dgvUtilisateurs.SelectedRows(0).Cells("NomUtilisateur").Value.ToString()

        If MessageBox.Show($"Désactiver l'utilisateur {nom} ?", "Confirmation",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                SqlCommandBuilder.CreateSqlCommand("updateUtilisateurActif",
                                 New Dictionary(Of String, Object) From {
                                     {"@id", id},
                                     {"@actif", 0}
                                 }).ExecuteNonQuery()
                ChargerUtilisateurs()
            Catch ex As Exception
                MessageBox.Show("Erreur lors de la suppression : " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub btnFermer_Click(sender As Object, e As EventArgs) Handles btnFermer.Click
        Me.Close()
    End Sub
End Class
