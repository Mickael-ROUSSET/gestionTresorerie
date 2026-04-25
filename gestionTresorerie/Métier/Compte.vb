Imports System.Security.Cryptography
Imports System.Text
Public Class Compte
    Public Property Id As Integer
    Public Property Login As String
    Private ReadOnly motDePasse As String
    Public Property TypeAcces As String
    Public Sub New(login As String, motDePasse As String, typeAcces As String)
        Me.Login = login
        Me.motDePasse = HashPasswordWithSalt(motDePasse)
        Me.TypeAcces = typeAcces
    End Sub
    Private Shared Function CreateRepository() As CompteRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).
                    GetConnexion(Constantes.DataBases.Agumaaa).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New CompteRepository(executor)
    End Function
    Private Shared Function HashPasswordWithSalt(password As String, Optional sel As String = "") As String
        ' Vérifier si le mot de passe ou le sel est nul ou vide
        If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(sel) Then
            Throw New ArgumentException("Le mot de passe et le sel ne peuvent pas être nuls ou vides.")
        End If

        ' Combiner le mot de passe et le sel
        ' TODO : pour simplifier, sel unique et en paramètre de l'appli
        If sel = String.Empty Then
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
            Dim unused = builder.Append(b.ToString("x2"))
        Next

        ' Retourner la chaîne hexadécimale
        Return builder.ToString()
    End Function
    Public Sub AjouterCompte()
        CreateRepository().Inserer(Me)
    End Sub

    Public Sub ReinitialiserMotDePasse()
        CreateRepository().MettreAJour(Me)
    End Sub

    Public Sub SupprimerCompte()
        CreateRepository().Supprimer(Me.Id)
    End Sub
End Class