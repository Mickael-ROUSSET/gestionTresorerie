Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Friend Module PublicationCinema
    Friend Const Adresse As String="https://programme-cinema-allegre.mickael-rousset.chatgpt.site/"
    Private Const Ressource As String="https://api.github.com/repos/Mickael-ROUSSET/cinema-agumaaa/contents/programme.json"
    Private Const Branche As String="programme-public"

    Friend Function Programme(films As IEnumerable(Of FilmSeance)) As String
        Dim seances As New JArray()
        For Each film In films.OrderBy(Function(f) f.DateDiffusion+f.HeureDiffusion)
            Dim seance As New JObject From {{"titre",film.Titre},{"date",film.DateDiffusion.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)},{"heure",film.HeureDiffusion.ToString("hh\:mm")},{"genre",film.Genre},{"synopsis",film.Synopsis}}
            If film.UrlAffiche IsNot Nothing AndAlso film.UrlAffiche.StartsWith("https://image.tmdb.org/",StringComparison.OrdinalIgnoreCase) Then seance("affiche")=film.UrlAffiche
            seances.Add(seance)
        Next
        If seances.Count=0 Then Throw New ArgumentException("Le programme est vide.")
        Return New JObject From {{"version",1},{"seances",seances}}.ToString(Formatting.Indented)
    End Function

    Friend Async Function Publier(films As List(Of FilmSeance),owner As IWin32Window) As Task
        Dim contenu=Programme(films)
        Dim apercu=String.Join(vbCrLf,films.Select(Function(f) f.DateDiffusion.ToString("dd/MM/yyyy") & " à " & f.HeureDiffusion.ToString("hh\:mm") & " — " & f.Titre))
        If MessageBox.Show(owner,"Remplacer le programme public à " & Adresse & " par ces séances ?" & vbCrLf & vbCrLf & apercu,"Publication cinéma",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)<>DialogResult.Yes Then Return
        Dim fichier=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"gestionTresorerie","cinema-publication.bin")
        Dim jeton As String=Nothing
        If File.Exists(fichier) Then
            Try
                jeton=Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(fichier),Nothing,DataProtectionScope.CurrentUser))
            Catch ex As CryptographicException
                jeton=Nothing
            End Try
        End If
        Dim nouveau=String.IsNullOrWhiteSpace(jeton)
        If nouveau Then
            Using fenetre As New Form With {.Text="Accès à la publication cinéma",.Width=580,.Height=185,.StartPosition=FormStartPosition.CenterParent},
                  champ As New TextBox With {.UseSystemPasswordChar=True,.Dock=DockStyle.Top},
                  valider As New Button With {.Text="Publier",.DialogResult=DialogResult.OK,.Dock=DockStyle.Bottom}
                fenetre.Controls.Add(champ)
                fenetre.Controls.Add(New Label With {.Dock=DockStyle.Top,.Height=54,.Text="Jeton GitHub limité au dépôt cinema-agumaaa, permission Contents en lecture et écriture. Il sera conservé chiffré pour votre compte Windows."})
                fenetre.Controls.Add(valider)
                fenetre.AcceptButton=valider
                If fenetre.ShowDialog(owner)<>DialogResult.OK Then Return
                jeton=champ.Text.Trim()
                If jeton="" Then Throw New ArgumentException("Le jeton GitHub est vide.")
            End Using
        End If
        Using client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AGUMAAA-Cinema/1.0")
            client.DefaultRequestHeaders.Authorization=New AuthenticationHeaderValue("Bearer",jeton)
            Using actuel=Await client.GetAsync(Ressource & "?ref=" & Branche)
                Verifier(actuel,fichier)
                Dim meta=JObject.Parse(Await actuel.Content.ReadAsStringAsync())
                Dim payload As New JObject From {{"message","Mettre à jour les séances du cinéma d’Allègre"},{"branch",Branche},{"sha",meta("sha").ToString()},{"content",Convert.ToBase64String(Encoding.UTF8.GetBytes(contenu))}}
                Using corps As New StringContent(payload.ToString(),Encoding.UTF8,"application/json"), reponse=Await client.PutAsync(Ressource,corps)
                    Verifier(reponse,fichier)
                End Using
            End Using
        End Using
        If nouveau Then
            Directory.CreateDirectory(Path.GetDirectoryName(fichier))
            File.WriteAllBytes(fichier,ProtectedData.Protect(Encoding.UTF8.GetBytes(jeton),Nothing,DataProtectionScope.CurrentUser))
        End If
        MessageBox.Show(owner,"Programme publié. Adresse du public :" & vbCrLf & Adresse,"Cinéma")
    End Function

    Private Sub Verifier(reponse As HttpResponseMessage,fichier As String)
        Select Case CInt(reponse.StatusCode)
            Case 401,403,404
                If File.Exists(fichier) Then File.Delete(fichier)
                Throw New InvalidOperationException("Publication refusée. Vérifiez l’accès GitHub au dépôt cinema-agumaaa et son droit Contents en écriture, puis réessayez.")
            Case 409
                Throw New InvalidOperationException("Le programme a changé pendant la publication. Vérifiez la version publique avant de réessayer.")
        End Select
        reponse.EnsureSuccessStatusCode()
    End Sub
End Module