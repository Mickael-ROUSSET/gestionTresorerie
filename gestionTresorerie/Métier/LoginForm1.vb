Imports System.Data.SqlClient

Public Class LoginForm
    Friend Property Connexion As String
    Friend Property Compte As Utilisateur

    Friend Shared Function ComptesConfigures(cs As String) As Boolean
        Dim cible As New SqlConnectionStringBuilder(cs)
        Dim catalogue=cible.InitialCatalog
        cible.InitialCatalog="master"
        Using cn As New SqlConnection(cible.ConnectionString), cmd=cn.CreateCommand()
            cn.Open()
            cmd.CommandText="SELECT DB_ID(@base)"
            cmd.Parameters.AddWithValue("@base",catalogue)
            If IsDBNull(cmd.ExecuteScalar()) Then Return False
            cn.ChangeDatabase(catalogue)
            cmd.CommandText="SELECT OBJECT_ID('dbo.Utilisateurs','U')"
            If IsDBNull(cmd.ExecuteScalar()) Then
                cmd.CommandText="SELECT IS_MEMBER('db_owner')"
                If Convert.ToInt32(cmd.ExecuteScalar())<>1 Then Throw New UnauthorizedAccessException("L’administrateur SQL doit initialiser les comptes d’accès de cette base.")
                Return False
            End If
            cmd.CommandText="SELECT TOP(1) 1 FROM dbo.Utilisateurs"
            Return cmd.ExecuteScalar() IsNot Nothing
        End Using
    End Function

    Friend Shared Function Authentifier(cs As String, nom As String, mdp As String) As Utilisateur
        Using cn As New SqlConnection(cs), cmd=cn.CreateCommand()
            cn.Open()
            cmd.CommandText="SELECT Id,NomUtilisateur,Role,MotDePasse FROM dbo.Utilisateurs WHERE NomUtilisateur=@nom AND Actif=1"
            cmd.Parameters.AddWithValue("@nom",nom.Trim())
            Using r=cmd.ExecuteReader()
                If Not r.Read() OrElse Not FrmEditUtilisateur.Verifier(mdp,r.GetString(3)) Then Return Nothing
                Dim role=r.GetString(2).ToLowerInvariant()
                If Not {"admin","lecteur","ecrivain"}.Contains(role) Then Return Nothing
                Return New Utilisateur With {.Id=r.GetInt32(0),.NomUtilisateur=r.GetString(1),.Role=role}
            End Using
        End Using
    End Function

    Private Sub OK_Click(sender As Object,e As EventArgs) Handles OK.Click
        Try
            Compte=Authentifier(Connexion,UsernameTextBox.Text,PasswordTextBox.Text)
            PasswordTextBox.Clear()
            If Compte Is Nothing Then
                MessageBox.Show(Me,"Identifiants incorrects ou compte désactivé.",Text)
                Return
            End If
            DialogResult=DialogResult.OK
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,Text)
        End Try
    End Sub
End Class