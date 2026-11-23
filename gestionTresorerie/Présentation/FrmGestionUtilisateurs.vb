Imports System.Data.SqlClient

Public Class FrmGestionUtilisateurs
    Friend Property Connexion As String

    Private Sub FrmGestionUtilisateurs_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dgvUtilisateurs.ReadOnly = True
        dgvUtilisateurs.AllowUserToAddRows = False
        dgvUtilisateurs.MultiSelect = False
        dgvUtilisateurs.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        btnSupprimer.Text = "Désactiver"
        ChargerUtilisateurs()
    End Sub

    Private Sub ChargerUtilisateurs() Handles btnRecharger.Click
        Try
            Using cn As New SqlConnection(Connexion), cmd = cn.CreateCommand()
                cn.Open()
                cmd.CommandText = "SELECT Id,NomUtilisateur,Role,Actif FROM dbo.Utilisateurs ORDER BY NomUtilisateur"
                Using r = cmd.ExecuteReader()
                    Dim table As New DataTable()
                    table.Load(r)
                    dgvUtilisateurs.DataSource = table
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,Text)
        End Try
    End Sub

    Private Sub Ajouter() Handles btnAjouter.Click
        Using f As New FrmEditUtilisateur With {.Connexion = Connexion}
            If f.ShowDialog(Me) = DialogResult.OK Then ChargerUtilisateurs()
        End Using
    End Sub

    Private Sub Modifier() Handles btnModifier.Click
        Dim ligne = TryCast(dgvUtilisateurs.CurrentRow?.DataBoundItem, DataRowView)
        If ligne Is Nothing Then Return
        Using f As New FrmEditUtilisateur(CInt(ligne("Id")),CStr(ligne("NomUtilisateur")),CStr(ligne("Role")),CBool(ligne("Actif"))) With {.Connexion=Connexion}
            If f.ShowDialog(Me) = DialogResult.OK Then ChargerUtilisateurs()
        End Using
    End Sub

    Private Sub Desactiver() Handles btnSupprimer.Click
        Dim ligne = TryCast(dgvUtilisateurs.CurrentRow?.DataBoundItem, DataRowView)
        If ligne Is Nothing OrElse Not CBool(ligne("Actif")) Then Return
        If MessageBox.Show(Me,"Désactiver " & CStr(ligne("NomUtilisateur")) & " ?",Text,MessageBoxButtons.YesNo) <> DialogResult.Yes Then Return
        Try
            FrmEditUtilisateur.Enregistrer(Connexion,CInt(ligne("Id")),CStr(ligne("NomUtilisateur")),CStr(ligne("Role")),False,Nothing)
            ChargerUtilisateurs()
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,Text)
        End Try
    End Sub

    Private Sub Fermer() Handles btnFermer.Click
        Close()
    End Sub
End Class
