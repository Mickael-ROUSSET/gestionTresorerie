Imports System.Net.Http
Imports Newtonsoft.Json.Linq

Public Class FilmTMDb
    Public Shared Function GetFilmInfos(titre As String, apiKey As String) As FilmDetail
        Try
            Using client As New HttpClient()
                Dim url = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={Uri.EscapeDataString(titre)}&language=fr-FR"
                Dim response = client.GetStringAsync(url).Result
                Dim json = JObject.Parse(response)

                Dim result = json("results")?.FirstOrDefault()
                If result Is Nothing Then Return Nothing

                Dim film As New FilmDetail With {
                    .Titre = result("title")?.ToString(),
                    .Synopsis = result("overview")?.ToString(),
                    .DateSortie = If(String.IsNullOrEmpty(result("release_date")?.ToString()), Nothing, CDate(result("release_date")?.ToString())),
                    .AfficheUrl = If(String.IsNullOrEmpty(result("poster_path")?.ToString()), Nothing, $"https://image.tmdb.org/t/p/w500{result("poster_path")}")
                }

                ' Pour obtenir durée, genre, réalisateur, on doit appeler le détail
                Dim movieId = result("Id").ToString()
                Dim urlDetail = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={apiKey}&language=fr-FR&append_to_response=credits"
                Dim detailResponse = client.GetStringAsync(urlDetail).Result
                Dim detailJson = JObject.Parse(detailResponse)

                film.DureeMinutes = If(detailJson("runtime")?.Type = JTokenType.Null, Nothing, CInt(detailJson("runtime")))
                film.Genre = If(detailJson("genres")?.Any(), String.Join(", ", detailJson("genres").Select(Function(g) g("name").ToString())), "")
                ' Réalisateur principal
                Dim crew = detailJson("credits")?("crew")
                Dim reals = crew?.Where(Function(c) c("job")?.ToString() = "Director")
                film.Realisateur = If(reals?.Any(), reals.First()("name").ToString(), "")

                Return film
            End Using
        Catch ex As Exception
            Logger.ERR($"TMDb erreur pour '{titre}' : {ex.Message}")
            Return Nothing
        End Try
    End Function
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
