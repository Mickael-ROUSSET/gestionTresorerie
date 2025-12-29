' FilmManager.vb
Imports System.Collections.Generic

Public Class FilmManager
    Private Shared ReadOnly _tmdbService As TMDbService

    Shared Sub New()
        'Dim apiKey = LectureProprietes.GetVariable("cleApiTmdb")
        '_tmdbService = New TMDbService(apiKey)
        _tmdbService = New TMDbService(GlobalSettings.GCleTmdb)
    End Sub

    ' Recherche d'un film via TMDb
    Public Shared Async Function GetFilmFromTMDb(titre As String) As Task(Of Filmseance)
        If String.IsNullOrWhiteSpace(titre) Then Return Nothing
        Dim id = Await _tmdbService.SearchMovieIdByTitleAsync(titre)
        If Not id.HasValue Then Return Nothing
        Return Await _tmdbService.GetMovieDetailsAsync(id.Value)
    End Function

    ' Insertion / vérification doublon
    Public Shared Sub InsererOuMettreAJourFilmEnBase(film As FilmSeance)
        If film Is Nothing Then Exit Sub
        Try
            Dim paramCheck As New Dictionary(Of String, Object) From {{"@Titre", film.Titre}}
            Using cmdCheck = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selFilmByTitre", paramCheck)
                Using rdr = cmdCheck.ExecuteReader()
                    If rdr.HasRows Then
                        Logger.INFO($"Film '{film.Titre}' existe déjà en base.")
                        Return
                    End If
                End Using
            End Using

            Dim paramInsert As New Dictionary(Of String, Object) From {
                {"@Titre", film.Titre},
                {"@DureeMinutes", If(film.DureeMinutes, film.DureeMinutes, 0)},
                {"@Genre", If(String.IsNullOrWhiteSpace(film.Genre), DBNull.Value, CType(film.Genre, Object))},
                {"@Realisateur", If(String.IsNullOrWhiteSpace(film.Realisateur), DBNull.Value, CType(film.Realisateur, Object))},
                {"@DateSortie", DBNull.Value},
                {"@Synopsis", If(String.IsNullOrWhiteSpace(film.Synopsis), DBNull.Value, CType(film.Synopsis, Object))},
                {"@AfficheUrl", If(String.IsNullOrWhiteSpace(film.UrlAffiche), DBNull.Value, CType(film.UrlAffiche, Object))}
            }

            Using cmdInsert = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "insertFilm", paramInsert)
                cmdInsert.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Logger.ERR($"InsererOuMettreAJourFilmEnBase : {ex.Message}")
        End Try
    End Sub
End Class
