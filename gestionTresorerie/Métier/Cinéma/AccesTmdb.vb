Option Strict On
Imports System.Runtime.InteropServices
Imports System.Text

Friend Module AccesTmdb
    <StructLayout(LayoutKind.Sequential,CharSet:=CharSet.Unicode)>
    Private Structure Credential
        Public Flags, Kind As UInteger
        Public TargetName, Comment As IntPtr
        Public LastWritten As System.Runtime.InteropServices.ComTypes.FILETIME
        Public CredentialBlobSize As UInteger
        Public CredentialBlob As IntPtr
        Public Persist, AttributeCount As UInteger
        Public Attributes, TargetAlias, UserName As IntPtr
    End Structure

    <DllImport("advapi32.dll",CharSet:=CharSet.Unicode,EntryPoint:="CredReadW",ExactSpelling:=True)>
    Private Function CredRead(target As String,kind As UInteger,flags As UInteger,ByRef result As IntPtr) As Boolean
    End Function

    <DllImport("advapi32.dll")>
    Private Sub CredFree(result As IntPtr)
    End Sub

    Private Function LireJetonCinema() As String
        For Each cible In {"CinemaAGUMAAA","tmdb@CinemaAGUMAAA"}
            Dim p As IntPtr
            If Not CredRead(cible,1UI,0UI,p) Then Continue For
            Try
                Dim c=Marshal.PtrToStructure(Of Credential)(p)
                If Marshal.PtrToStringUni(c.UserName)<>"tmdb" OrElse c.CredentialBlobSize=0 Then Continue For
                Dim octets(CInt(c.CredentialBlobSize)-1) As Byte
                Marshal.Copy(c.CredentialBlob,octets,0,octets.Length)
                Dim enc=If(octets.Contains(CByte(0)),Encoding.Unicode,Encoding.UTF8)
                Return enc.GetString(octets).Trim(ChrW(0),ChrW(&HFEFF)," "c)
            Finally
                CredFree(p)
            End Try
        Next
        Return Nothing
    End Function

    Friend Function LireEnvironnement(lireVariable As Func(Of String,EnvironmentVariableTarget,String), ByRef source As String) As String
        Dim systeme=lireVariable("cleApiTmdb",EnvironmentVariableTarget.Machine)
        If Not String.IsNullOrWhiteSpace(systeme) Then
            source="cleApiTmdb (Système Windows)"
            Return systeme.Trim()
        End If
        For Each nom In {"TMDB_READ_ACCESS_TOKEN","TMDB_API_KEY","cleApiTmdb","jetonApiTmdb"}
            For Each cible In {EnvironmentVariableTarget.User,EnvironmentVariableTarget.Machine,EnvironmentVariableTarget.Process}
                Dim valeur=lireVariable(nom,cible)
                If Not String.IsNullOrWhiteSpace(valeur) Then
                    source=nom & " (" & cible.ToString() & ")"
                    Return valeur.Trim()
                End If
            Next
        Next
        Return Nothing
    End Function

    Friend Function Lire(ByRef source As String) As String
        Dim cle=LireEnvironnement(AddressOf Environment.GetEnvironmentVariable,source)
        If cle IsNot Nothing Then Return cle
        For Each entree In {("CinemaAGUMAAA",LireJetonCinema()),("Configuration AGUMAAA",GlobalSettings.GCleTmdb),("Paramètres jetonApiTmdb",My.Settings.jetonApiTmdb),("Paramètres cleApiTmdb",My.Settings.cleApiTmdb)}
            If Not String.IsNullOrWhiteSpace(entree.Item2) Then
                source=entree.Item1
                Return entree.Item2.Trim()
            End If
        Next
        Throw New InvalidOperationException("Accès TMDb absent. Définissez cleApiTmdb dans les variables système Windows.")
    End Function
End Module