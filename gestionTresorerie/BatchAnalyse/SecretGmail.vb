Option Strict On
Imports Microsoft.Win32
Imports System.Security.Cryptography
Imports System.Text

Friend Module SecretGmail
    Private Const Chemin As String = "Software\AGUMAAA\GestionTresorerie\Gmail"
    Private Function Cle(compte As String) As String
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(compte.Trim().ToLowerInvariant())))
    End Function

    Friend Function DernierCompte(defaut As String) As String
        Using key = Registry.CurrentUser.OpenSubKey(Chemin)
            Return If(TryCast(key?.GetValue("DernierCompte"), String), defaut)
        End Using
    End Function

    Friend Function EstMemorise(compte As String) As Boolean
        Using key = Registry.CurrentUser.OpenSubKey(Chemin)
            Return TypeOf key?.GetValue(Cle(compte)) Is Byte()
        End Using
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
                key.SetValue("DernierCompte", compte.Trim(), RegistryValueKind.String)
                key.Flush()
            End Using
        Finally
            CryptographicOperations.ZeroMemory(clair)
        End Try
    End Sub
End Module
