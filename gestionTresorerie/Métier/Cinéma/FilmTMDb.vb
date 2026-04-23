Imports System.Net.Http
Imports Newtonsoft.Json.Linq

Public Class FilmTMDb
    '    Public Shared Function GetFilmInfos(titre As String, apiKey As String) As FilmDetail
    '        Try
    '            Using client As New HttpClient()
    '                Dim url = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={Uri.EscapeDataString(titre)}&language=fr-FR"
    '                Dim response = client.GetStringAsync(url).Result
    '                Dim json = JObject.Parse(response)

    '                Dim result = json("results")?.FirstOrDefault()
    '                If result Is Nothing Then Return Nothing

    '                Dim film As New FilmDetail With {
    '                    .Titre = result("title")?.ToString(),
    '                    .Synopsis = result("overview")?.ToString(),
    '                    .DateSortie = If(String.IsNullOrEmpty(result("release_date")?.ToString()), Nothing, CDate(result("release_date")?.ToString())),
    '                    .AfficheUrl = If(String.IsNullOrEmpty(result("poster_path")?.ToString()), Nothing, $"https://image.tmdb.org/t/p/w500{result("poster_path")}")
    '                }

    '                ' Pour obtenir durée, genre, réalisateur, on doit appeler le détail
    '                Dim movieId = result("Id").ToString()
    '                Dim urlDetail = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={apiKey}&language=fr-FR&append_to_response=credits"
    '                Dim detailResponse = client.GetStringAsync(urlDetail).Result
    '                Dim detailJson = JObject.Parse(detailResponse)

    '                film.DureeMinutes = If(detailJson("runtime")?.Type = JTokenType.Null, Nothing, CInt(detailJson("runtime")))
    '                film.Genre = If(detailJson("genres")?.Any(), String.Join(", ", detailJson("genres").Select(Function(g) g("name").ToString())), "")
    '                ' Réalisateur principal
    '                Dim crew = detailJson("credits")?("crew")
    '                Dim reals = crew?.Where(Function(c) c("job")?.ToString() = "Director")
    '                film.Realisateur = If(reals?.Any(), reals.First()("name").ToString(), "")

    '                Return film
    '            End Using
    '        Catch ex As Exception
    '            Logger.ERR($"TMDb erreur pour '{titre}' : {ex.Message}")
    '            Return Nothing
    '        End Try
    '    End Function
    Public Class TmdbService
        Private Shared ReadOnly Client As New HttpClient() ' HttpClient doit être statique pour éviter l'épuisement de sockets

        Public Shared Async Function GetFilmInfosAsync(titre As String, apiKey As String) As Task(Of FilmDetail)
            Try
                ' 1. Recherche initiale
                Dim searchJson = Await FetchJsonAsync(BuildSearchUrl(titre, apiKey))
                Dim firstResult = searchJson("results")?.FirstOrDefault()
                If firstResult Is Nothing Then Return Nothing

                ' 2. Récupération des détails via l'ID
                Dim movieId = firstResult("id").ToString()
                Dim detailJson = Await FetchJsonAsync(BuildDetailUrl(movieId, apiKey))

                ' 3. Mapping (Transformation du JSON vers l'Objet)
                Return MapJsonToFilmDetail(firstResult, detailJson)

            Catch ex As Exception
                Logger.ERR($"TMDb erreur pour '{titre}' : {ex.Message}")
                Return Nothing
            End Try
        End Function

        ' --- MÉTHODES PRIVÉES (Réduction de la complexité) ---

        Private Shared Function BuildSearchUrl(titre As String, key As String) As String
            Return $"https://api.themoviedb.org/3/search/movie?api_key={key}&query={Uri.EscapeDataString(titre)}&language=fr-FR"
        End Function

        Private Shared Function BuildDetailUrl(id As String, key As String) As String
            Return $"https://api.themoviedb.org/3/movie/{id}?api_key={key}&language=fr-FR&append_to_response=credits"
        End Function

        Private Shared Async Function FetchJsonAsync(url As String) As Task(Of JObject)
            Dim response = Await Client.GetStringAsync(url).ConfigureAwait(False)
            Return JObject.Parse(response)
        End Function

        Private Shared Function MapJsonToFilmDetail(searchResult As JToken, detailJson As JToken) As FilmDetail
            ' Utilisation de l'initialiseur d'objet (IDE0017)
            Dim film As New FilmDetail With {
                .Titre = searchResult("title")?.ToString(),
                .Synopsis = searchResult("overview")?.ToString(),
                .DateSortie = ParseDate(searchResult("release_date")?.ToString()),
                .AfficheUrl = BuildPosterUrl(searchResult("poster_path")?.ToString()),
                .DureeMinutes = ParseRuntime(detailJson("runtime")),
                .Genre = ExtractGenres(detailJson("genres")),
                .Realisateur = ExtractDirector(detailJson("credits")?("crew"))
            }
            Return film
        End Function
        ' Convertit la chaîne de date de l'API en objet Date? (Nullable)
        Private Shared Function ParseDate(dateString As String) As Date?
            Dim dt As Date
            If Date.TryParse(dateString, dt) Then
                Return dt
            End If
            Return Nothing
        End Function

        ' Construit l'URL complète de l'affiche
        Private Shared Function BuildPosterUrl(path As String) As String
            If String.IsNullOrWhiteSpace(path) Then Return Nothing
            Return $"https://image.tmdb.org/t/p/w500{path}"
        End Function

        ' Extrait la durée en gérant le cas où elle est absente
        Private Shared Function ParseRuntime(runtimeToken As JToken) As Integer
            If runtimeToken Is Nothing OrElse runtimeToken.Type = JTokenType.Null Then
                Return 0
            End If
            Return runtimeToken.Value(Of Integer)
        End Function

        ' Concatène les noms des genres
        Private Shared Function ExtractGenres(genresToken As JToken) As String
            If genresToken Is Nothing OrElse Not genresToken.Any() Then Return ""
            Return String.Join(", ", genresToken.Select(Function(g) g("name")?.ToString()))
        End Function

        ' Trouve le réalisateur dans l'équipe technique
        Private Shared Function ExtractDirector(crewToken As JToken) As String
            If crewToken Is Nothing Then Return ""
            Dim director = crewToken.FirstOrDefault(Function(c) c("job")?.ToString() = "Director")
            Return If(director?("name")?.ToString(), "")
        End Function
    End Class
End Class

' Classe de données pour Film
Public Class FilmDetail
    Public Property Titre As String
    Public Property DureeMinutes As Integer?
    Public Property Genre As String
    Public Property Realisateur As String
    Public Property DateSortie As Date?
    Public Property Synopsis As String
    Public Property AfficheUrl As String
End Class
