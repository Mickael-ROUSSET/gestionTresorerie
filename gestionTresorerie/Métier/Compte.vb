Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class Compte
    Public Property Id As Integer
    Public Property Login As String
    Private motDePasse As String
    Public Property TypeAcces As String

    Public Sub New(login As String, motDePasse As String, typeAcces As String)
        Me.Login = login
        Me.motDePasse = HashPasswordWithSalt(motDePasse)
        Me.TypeAcces = typeAcces
    End Sub
    Private Shared Function HashPasswordWithSalt(password As String, Optional sel As String = Constantes.Vide) As String
        ' Vérifier si le mot de passe ou le sel est nul ou vide
        If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(sel) Then
            Throw New ArgumentException("Le mot de passe et le sel ne peuvent pas être nuls ou vides.")
        End If

        ' Combiner le mot de passe et le sel
        ' TODO : pour simplifier, sel unique et en paramètre de l'appli
        If sel = Constantes.Vide Then
            sel = LectureProprietes.GetVariable("selAleatoire")
        End If
        Dim passwordWithSalt As String = password & sel

        ' Convertir en tableau de bytes
        Dim passwordBytes As Byte() = Encoding.UTF8.GetBytes(passwordWithSalt)

        ' Hacher les données
        Dim hashBytes As Byte() = SHA256.HashData(passwordBytes)

        ' Convertir en chaîne hexadécimale
        Dim builder As New StringBuilder(hashBytes.Length * 2)
        For Each b As Byte In hashBytes
            builder.Append(b.ToString("x2"))
        Next

        ' Retourner la chaîne hexadécimale
        Return builder.ToString()
    End Function



    Public Sub AjouterCompte()
        Dim connection As SqlConnection = ConnexionDB.GetInstance.getConnexion()
        If connection.State <> ConnectionState.Open Then
            connection.Open()
        End If

        Dim query As String = "INSERT INTO [dbo].[Comptes] ([login], [motDePasse], [typeAcces]) VALUES (@login, @motDePasse, @typeAcces)"
        Using command As New SqlCommand(query, connection)
            command.Parameters.AddWithValue("@login", Me.Login)
            command.Parameters.AddWithValue("@motDePasse", Me.motDePasse)
            command.Parameters.AddWithValue("@typeAcces", Me.TypeAcces)
            command.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub ReinitialiserMotDePasse(nouveauMotDePasse As String)
        Me.motDePasse = HashPasswordWithSalt(nouveauMotDePasse)
        Dim connection As SqlConnection = ConnexionDB.GetInstance.getConnexion()
        If connection.State <> ConnectionState.Open Then
            connection.Open()
        End If

        Dim query As String = "UPDATE [dbo].[Comptes] SET [motDePasse] = @motDePasse WHERE [login] = @login"
        Using command As New SqlCommand(query, connection)
            command.Parameters.AddWithValue("@motDePasse", Me.motDePasse)
            command.Parameters.AddWithValue("@login", Me.Login)
            command.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub SupprimerCompte()
        Dim connection As SqlConnection = ConnexionDB.GetInstance.getConnexion()
        If connection.State <> ConnectionState.Open Then
            connection.Open()
        End If

        Dim query As String = "DELETE FROM [dbo].[Comptes] WHERE [login] = @login"
        Using command As New SqlCommand(query, connection)
            command.Parameters.AddWithValue("@login", Me.Login)
            command.ExecuteNonQuery()
        End Using
    End Sub
End Class
'Sub Main()
'    ' Ajouter un nouveau compte
'    Dim nouveauCompte As New Compte("utilisateur1", "motdepasse123", "admin")
'    nouveauCompte.AjouterCompte()
'    Console.WriteLine("Compte ajouté avec succès.")

'    ' Réinitialiser le mot de passe du compte
'    Dim compteExistant As New Compte("utilisateur1", "", "") ' Le mot de passe et le type d'accès ne sont pas nécessaires ici
'    compteExistant.ReinitialiserMotDePasse("nouveaumotdepasse")
'    Console.WriteLine("Mot de passe réinitialisé avec succès.")

'    ' Supprimer le compte
'    Dim compteASupprimer As New Compte("utilisateur1", "", "") ' Le mot de passe et le type d'accès ne sont pas nécessaires ici
'    compteASupprimer.SupprimerCompte()
'    Console.WriteLine("Compte supprimé avec succès.")
'End Sub

