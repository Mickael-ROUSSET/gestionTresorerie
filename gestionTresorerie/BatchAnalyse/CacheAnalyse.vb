Option Strict On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json.Linq

Friend Module CacheAnalyse
    Friend Function Dossier(environnement As String, moteur As String) As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"gestionTresorerie",environnement,"Analyses",moteur)
    End Function

    Friend Async Function Recuperer(dossier As String, empreinte As String, analyser As Func(Of Task(Of JObject)), verifier As Func(Of String,JObject)) As Task(Of JObject)
        If empreinte.Length<>64 OrElse Not empreinte.All(Function(c) Uri.IsHexDigit(c)) Then Throw New ArgumentException("Empreinte de document invalide.")
        Dim fichier=Path.Combine(dossier,empreinte.ToUpperInvariant() & ".bin")
        If File.Exists(fichier) Then
            Try
                Dim clair=ProtectedData.Unprotect(Await File.ReadAllBytesAsync(fichier),Nothing,DataProtectionScope.CurrentUser)
                Return verifier(Encoding.UTF8.GetString(clair))
            Catch ex As Exception When TypeOf ex Is CryptographicException OrElse TypeOf ex Is Newtonsoft.Json.JsonException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is FileNotFoundException
                ' Une entrée périmée ou illisible sera remplacée à la fin de la lecture.
            End Try
        End If
        Dim resultat=Await analyser()
        If resultat Is Nothing Then Throw New InvalidDataException("Aucune analyse disponible.")
        Dim json=resultat.ToString(Newtonsoft.Json.Formatting.None)
        verifier(json)
        Directory.CreateDirectory(dossier)
        Dim temporaire=fichier & "." & Guid.NewGuid().ToString("N")
        Try
            Dim chiffre=ProtectedData.Protect(Encoding.UTF8.GetBytes(json),Nothing,DataProtectionScope.CurrentUser)
            Await File.WriteAllBytesAsync(temporaire,chiffre)
            File.Move(temporaire,fichier,True)
        Finally
            If File.Exists(temporaire) Then File.Delete(temporaire)
        End Try
        Return resultat
    End Function
End Module