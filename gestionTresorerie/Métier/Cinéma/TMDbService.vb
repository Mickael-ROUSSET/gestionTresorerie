' TMDbService.vb
Imports System.Net.Http
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq

Public Class TMDbService
    Private Shared ReadOnly _http As New HttpClient()
    Private ReadOnly _apiKey As String

    Public Sub New(apiKey As String)
        _apiKey = apiKey
    End Sub

    Public Async Function SearchMovieIdByTitleAsync(titre As String) As Task(Of Integer?)
        Try
            Dim url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(titre)}&language=fr-FR"
            Dim json = Await _http.GetStringAsync(url)
            Dim obj = JObject.Parse(json)
            Dim first = obj("results")?.FirstOrDefault()
            If first Is Nothing Then Return Nothing
            Return CInt(first("id"))
        Catch ex As Exception
            Logger.ERR($"TMDb SearchMovieIdByTitleAsync('{titre}') : {ex.Message}")
            Return Nothing
        End Try
    End Function

    Public Async Function GetMovieDetailsAsync(idTmdb As Integer) As Task(Of FilmSeance)
        Try
            Dim url = $"https://api.themoviedb.org/3/movie/{idTmdb}?api_key={_apiKey}&language=fr-FR&append_to_response=credits"
            Dim json = Await _http.GetStringAsync(url)
            Dim d = JObject.Parse(json)

            Dim film As New FilmSeance With {
                .Titre = If(d("title")?.ToString(), String.Empty),
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
        Catch ex As Exception
            Logger.ERR($"TMDb GetMovieDetailsAsync({idTmdb}) : {ex.Message}")
            Return Nothing
        End Try
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
