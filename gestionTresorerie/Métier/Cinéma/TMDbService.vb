' TMDbService.vb
Imports System.Net.Http
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq

Public Class TMDbService
    Private Shared ReadOnly _http As New HttpClient()
    Private ReadOnly _apiKey As String
    Private ReadOnly client As HttpClient

    Public Sub New(apiKey As String)
        Me.New(apiKey,_http)
    End Sub

    Friend Sub New(apiKey As String,http As HttpClient)
        _apiKey=apiKey.Trim()
        client=http
    End Sub

    Private Async Function Lire(route As String) As Task(Of JObject)
        Dim cleV3=System.Text.RegularExpressions.Regex.IsMatch(_apiKey,"^[0-9a-fA-F]{32}$")
        Dim url="https://api.themoviedb.org/3/" & route
        If cleV3 Then url &= "&api_key=" & Uri.EscapeDataString(_apiKey)
        Using requete As New HttpRequestMessage(HttpMethod.Get,url)
            If Not cleV3 Then requete.Headers.Authorization=New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",_apiKey)
            Using reponse=Await client.SendAsync(requete)
                If Not reponse.IsSuccessStatusCode Then
                    Select Case CInt(reponse.StatusCode)
                        Case 401,403
                            Throw New InvalidOperationException("TMDb refuse la clé ou le jeton. Vérifiez les identifiants TMDb dans CinemaAGUMAAA ou les variables Windows.")
                        Case 429
                            Throw New InvalidOperationException("TMDb limite temporairement les recherches. Réessayez dans un instant.")
                        Case Else
                            Throw New InvalidOperationException("TMDb indisponible (HTTP " & CInt(reponse.StatusCode) & ").")
                    End Select
                End If
                Return JObject.Parse(Await reponse.Content.ReadAsStringAsync())
            End Using
        End Using
    End Function

    Private Async Function Chercher(titre As String) As Task(Of JToken)
        Dim obj=Await Lire("search/movie?language=fr-FR&query=" & Uri.EscapeDataString(titre.Trim()))
        Dim films=TryCast(obj("results"),JArray)
        If films Is Nothing Then Throw New InvalidOperationException("Réponse de recherche TMDb incomplète.")
        Return If(films.FirstOrDefault(Function(f) String.Equals(CStr(f("title")),titre.Trim(),StringComparison.OrdinalIgnoreCase) OrElse String.Equals(CStr(f("original_title")),titre.Trim(),StringComparison.OrdinalIgnoreCase)),films.FirstOrDefault())
    End Function

    Public Async Function SearchMovieIdByTitleAsync(titre As String) As Task(Of Integer?)
        Dim resultat=Await Chercher(titre)
        If resultat Is Nothing Then Return Nothing
        Return CInt(resultat("id"))
    End Function

    Friend Async Function FicheAsync(titre As String) As Task(Of FilmSeance)
        Dim resultat=Await Chercher(titre)
        If resultat Is Nothing Then Return Nothing
        Dim film=Await GetMovieDetailsAsync(CInt(resultat("id")))
        If String.IsNullOrWhiteSpace(film.Synopsis) Then film.Synopsis=CStr(resultat("overview"))
        Return film
    End Function

    Public Async Function GetMovieDetailsAsync(idTmdb As Integer) As Task(Of FilmSeance)
            Dim d=Await Lire("movie/" & idTmdb & "?language=fr-FR&append_to_response=credits")

            Dim film As New FilmSeance With {
                .Titre = If(d("title")?.ToString(), String.Empty),
                .Pays = String.Join(", ",If(TryCast(d("production_countries"),JArray),New JArray()).Select(Function(p) p("name")?.ToString())),
                .Synopsis = If(d("overview")?.ToString(), String.Empty),
                .DureeMinutes = If(d("runtime") Is Nothing OrElse d("runtime").Type = JTokenType.Null, CType(Nothing, Integer?), CInt(d("runtime"))),
                .Genre = If(d("genres") IsNot Nothing AndAlso d("genres").Any(), d("genres")(0)("name")?.ToString(), String.Empty),
                .UrlAffiche = If(d("poster_path") IsNot Nothing, "https://image.tmdb.org/t/p/w500" & d("poster_path")?.ToString(), String.Empty)
            }

            ' Réalisateur
            Dim crew = d("credits")?("crew")
            If crew IsNot Nothing Then
                Dim director = crew.FirstOrDefault(Function(c) c("job")?.ToString() = "Director")
                If director IsNot Nothing Then film.Realisateur = director("name")?.ToString()
            End If

            ' Casting (5 premiers)
            Dim castList As New List(Of String)()
            Dim cast = d("credits")?("cast")
            If cast IsNot Nothing Then
                For Each c In cast.Take(5)
                    Dim nm = c("name")?.ToString()
                    If Not String.IsNullOrWhiteSpace(nm) Then castList.Add(nm)
                Next
            End If
            film.Casting = castList

            Return film
    End Function

    ' Télécharge l'affiche
    Public Shared Async Function DownloadPosterAsync(posterUrl As String) As Task(Of Byte())
        Try
            If String.IsNullOrWhiteSpace(posterUrl) Then Return Nothing
            Return Await _http.GetByteArrayAsync(posterUrl)
        Catch ex As Exception
            Logger.WARN($"TMDb DownloadPosterAsync : {ex.Message}")
            Return Nothing
        End Try
    End Function
End Class
