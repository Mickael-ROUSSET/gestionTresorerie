Option Strict On
Imports Microsoft.Win32
Imports System.Security.Cryptography
Imports System.Text

Friend Module SecretGmail
    Private Const Chemin As String = "Software\AGUMAAA\GestionTresorerie\Gmail"
    Private Function Cle(compte As String) As String
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(compte.Trim().ToLowerInvariant())))
    End Function

    Friend Function Lire(compte As String) As String
        Using key = Registry.CurrentUser.OpenSubKey(Chemin)
            Dim chiffre = TryCast(key?.GetValue(Cle(compte)), Byte())
            If chiffre Is Nothing Then Return Nothing
            Dim clair = ProtectedData.Unprotect(chiffre, Nothing, DataProtectionScope.CurrentUser)
            Try
                Return Encoding.UTF8.GetString(clair)
            Finally
                CryptographicOperations.ZeroMemory(clair)
            End Try
        End Using
    End Function

    Friend Sub Enregistrer(compte As String, secret As String)
        Dim clair = Encoding.UTF8.GetBytes(secret)
        Try
            Dim chiffre = ProtectedData.Protect(clair, Nothing, DataProtectionScope.CurrentUser)
            Using key = Registry.CurrentUser.CreateSubKey(Chemin)
                key.SetValue(Cle(compte), chiffre, RegistryValueKind.Binary)
            End Using
        Finally
            CryptographicOperations.ZeroMemory(clair)
        End Try
    End Sub
End Module
