' FilmManager.vb
Imports System.Collections.Generic

Public Class FilmManager
    Private Shared ReadOnly _tmdbService As TMDbService

    Shared Sub New()
        'Dim apiKey = LectureProprietes.GetVariable("cleApiTmdb")
        '_tmdbService = New TMDbService(apiKey)
        _tmdbService = New TMDbService(GlobalSettings.GCleTmdb)
    End Sub

    Private Shared Function CreateRepository() As FilmRepository
        Dim executor As ISqlExecutor =
    RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New FilmRepository(executor)
    End Function

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
            Dim repo As FilmRepository = CreateRepository()

            If repo.ExisteParTitre(film.Titre) Then
                Logger.INFO($"Film '{film.Titre}' existe déjà en base.")
                Return
            End If

            repo.InsererFilmSeance(film)

        Catch ex As Exception
            Logger.ERR($"InsererOuMettreAJourFilmEnBase : {ex.Message}")
        End Try
    End Sub
End Class
