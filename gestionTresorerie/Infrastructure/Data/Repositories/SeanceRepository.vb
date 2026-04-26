Imports System.Data.SqlClient

Public Class SeanceRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireParFilm(idFilm As Integer) As List(Of Seance)
        Return _executor.ExecuteNamedReader(
            "selSeanceIdFilm",
            New List(Of SqlParameter) From {
                New SqlParameter("@IdFilm", idFilm)
            },
            Function(r As SqlDataReader)
                Return New Seance With {
                    .IdSeance = Convert.ToInt32(r("IdSeance")),
                    .IdFilm = Convert.ToInt32(r("IdFilm")),
                    .DateHeureDebut = Convert.ToDateTime(r("DateHeureDebut")),
                    .TarifBase = Convert.ToDecimal(r("TarifBase")),
                    .Langue = If(IsDBNull(r("Langue")), Nothing, r("Langue").ToString()),
                    .Format = If(IsDBNull(r("Format")), Nothing, r("Format").ToString())
                }
            End Function)
    End Function

    Public Function Inserer(seance As Seance) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "insertSeance",
            New List(Of SqlParameter) From {
                New SqlParameter("@IdFilm", seance.IdFilm),
                New SqlParameter("@DateHeureDebut", seance.DateHeureDebut),
                New SqlParameter("@TarifBase", seance.TarifBase),
                New SqlParameter("@Langue", If(seance.Langue, DBNull.Value)),
                New SqlParameter("@Format", If(seance.Format, DBNull.Value)),
                New SqlParameter("@NbEntreesAdultes", seance.NbEntreesAdultes),
                New SqlParameter("@NbEntreesEnfants", seance.NbEntreesEnfants),
                New SqlParameter("@NbEntreesGroupeEnfants", seance.NbEntreesGroupeEnfants)
            })
    End Function

    Public Function Supprimer(idSeance As Integer) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "delSeance",
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", idSeance)
            })
    End Function

End Class