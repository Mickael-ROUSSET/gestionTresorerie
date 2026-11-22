Option Strict On
Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.Text
Imports Microsoft.Win32.SafeHandles

Friend Module VerificationSession
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure Invite
        Public Taille As Integer
        Public Fenetre As IntPtr
        Public Message As String
        Public Titre As String
        Public Banniere As IntPtr
    End Structure

    <DllImport("credui.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)>
    Private Function CredUIPromptForCredentialsW(ByRef info As Invite, cible As String, reserve As IntPtr,
        erreur As Integer, utilisateur As StringBuilder, tailleUtilisateur As Integer, secret As IntPtr,
        tailleSecret As Integer, ByRef enregistrer As Integer, options As Integer) As Integer
    End Function

    <DllImport("advapi32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True, SetLastError:=True)>
    Private Function LogonUserW(utilisateur As String, domaine As String, secret As IntPtr,
        typeConnexion As Integer, fournisseur As Integer, ByRef jeton As SafeAccessTokenHandle) As Boolean
    End Function

    Friend Function Confirmer(parent As IntPtr) As Boolean
        Using identite = WindowsIdentity.GetCurrent()
            Dim nom = identite.Name
            Dim parties = nom.Split(New Char() {"\"c}, 2)
            Dim domaine As String = If(parties.Length = 2, parties(0), Nothing)
            Dim utilisateur = parties.Last()
            Dim saisie As New StringBuilder(nom, 514)
            Dim info As New Invite With {.Taille = Marshal.SizeOf(Of Invite)(), .Fenetre = parent,
                .Titre = "AGUMAAA — Vérification Windows", .Message = "Confirmez le mot de passe de votre session Windows pour afficher le secret Gmail."}
            Const capacite As Integer = 256
            Dim tampon = Marshal.AllocHGlobal(capacite * 2)
            Dim zeros(capacite * 2 - 1) As Byte
            Try
                Marshal.Copy(zeros, 0, tampon, zeros.Length)
                Dim enregistrer As Integer
                ' DO_NOT_PERSIST, ALWAYS_SHOW_UI, GENERIC_CREDENTIALS, KEEP_USERNAME.
                Const options As Integer = &H2 Or &H80 Or &H40000 Or &H100000
                If CredUIPromptForCredentialsW(info, "AGUMAAA/Consultation", IntPtr.Zero, 0, saisie,
                    saisie.Capacity, tampon, capacite, enregistrer, options) <> 0 Then Return False
                If Marshal.ReadInt16(tampon) = 0 Then Return False
                Dim jeton As SafeAccessTokenHandle = Nothing
                If Not LogonUserW(utilisateur, domaine, tampon, 2, 0, jeton) Then Return False
                Using jeton
                    Using sessionVerifiee As New WindowsIdentity(jeton.DangerousGetHandle())
                        Return identite.User IsNot Nothing AndAlso identite.User.Equals(sessionVerifiee.User)
                    End Using
                End Using
            Finally
                Marshal.Copy(zeros, 0, tampon, zeros.Length)
                Marshal.FreeHGlobal(tampon)
            End Try
        End Using
    End Function
End Module
