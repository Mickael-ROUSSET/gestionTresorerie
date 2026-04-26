Imports System.Data.SqlClient

Public Class FilmRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireTous() As List(Of Film)
        Return _executor.ExecuteNamedReader(
            "selTousFilms",
            Nothing,
            Function(r As SqlDataReader)
                Return New Film With {
                    .IdFilm = Convert.ToInt32(r("IdFilm")),
                    .Titre = r("Titre").ToString(),
                    .DureeMinutes = Convert.ToInt32(r("DureeMinutes")),
                    .Genre = If(IsDBNull(r("Genre")), Nothing, r("Genre").ToString()),
                    .Realisateur = If(IsDBNull(r("Realisateur")), Nothing, r("Realisateur").ToString()),
                    .DateSortie = If(IsDBNull(r("DateSortie")), CType(Nothing, Date?), Convert.ToDateTime(r("DateSortie"))),
                    .Synopsis = If(IsDBNull(r("Synopsis")), Nothing, r("Synopsis").ToString()),
                    .AgeMinimum = If(IsDBNull(r("AgeMinimum")), CType(Nothing, Integer?), Convert.ToInt32(r("AgeMinimum"))),
                    .AfficheUrl = If(IsDBNull(r("AfficheUrl")), Nothing, r("AfficheUrl").ToString())
                }
            End Function)
    End Function

    Public Function Inserer(film As Film) As Integer
        Return _executor.ExecuteNamedNonQuery("insertFilm", BuildParams(film, includeAgeMinimum:=True))
    End Function

    Public Function InsererFilmSeance(film As FilmSeance) As Integer
        Dim p As New List(Of SqlParameter) From {
            New SqlParameter("@Titre", film.Titre),
            New SqlParameter("@DureeMinutes", film.DureeMinutes),
            New SqlParameter("@Genre", If(String.IsNullOrWhiteSpace(film.Genre), DBNull.Value, CType(film.Genre, Object))),
            New SqlParameter("@Realisateur", If(String.IsNullOrWhiteSpace(film.Realisateur), DBNull.Value, CType(film.Realisateur, Object))),
            New SqlParameter("@DateSortie", DBNull.Value),
            New SqlParameter("@Synopsis", If(String.IsNullOrWhiteSpace(film.Synopsis), DBNull.Value, CType(film.Synopsis, Object))),
            New SqlParameter("@AfficheUrl", If(String.IsNullOrWhiteSpace(film.UrlAffiche), DBNull.Value, CType(film.UrlAffiche, Object)))
        }

        Return _executor.ExecuteNamedNonQuery("insertFilm", p)
    End Function

    Public Function Supprimer(idFilm As Integer) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "delFilm",
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", idFilm)
            })
    End Function

    Public Function LireTitreParId(idFilm As Integer) As String
        Dim result As Object =
            _executor.ExecuteNamedScalar(Of Object)(
                "selTitreFilmParId",
                New List(Of SqlParameter) From {
                    New SqlParameter("@IdFilm", idFilm)
                })

        If result Is Nothing OrElse result Is DBNull.Value Then
            Return "Titre inconnu"
        End If

        Return result.ToString()
    End Function

    Public Function ExisteParTitre(titre As String) As Boolean
        Dim result As Object =
            _executor.ExecuteNamedScalar(Of Object)(
                "selFilmByTitre",
                New List(Of SqlParameter) From {
                    New SqlParameter("@Titre", titre)
                })

        Return result IsNot Nothing AndAlso result IsNot DBNull.Value
    End Function

    Private Shared Function BuildParams(film As Film, includeAgeMinimum As Boolean) As List(Of SqlParameter)
        Dim p As New List(Of SqlParameter) From {
            New SqlParameter("@Titre", film.Titre),
            New SqlParameter("@DureeMinutes", film.DureeMinutes),
            New SqlParameter("@Genre", If(String.IsNullOrWhiteSpace(film.Genre), DBNull.Value, CType(film.Genre, Object))),
            New SqlParameter("@Realisateur", If(String.IsNullOrWhiteSpace(film.Realisateur), DBNull.Value, CType(film.Realisateur, Object))),
            New SqlParameter("@DateSortie", If(film.DateSortie.HasValue, CType(film.DateSortie.Value, Object), DBNull.Value)),
            New SqlParameter("@Synopsis", If(String.IsNullOrWhiteSpace(film.Synopsis), DBNull.Value, CType(film.Synopsis, Object))),
            New SqlParameter("@AfficheUrl", If(String.IsNullOrWhiteSpace(film.AfficheUrl), DBNull.Value, CType(film.AfficheUrl, Object)))
        }

        If includeAgeMinimum Then
            p.Add(New SqlParameter("@AgeMinimum", If(film.AgeMinimum.HasValue, CType(film.AgeMinimum.Value, Object), DBNull.Value)))
        End If

        Return p
    End Function

End Class