Imports System.Data.SqlClient

Public Class StatsCinemaRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireStatsParMois() As Dictionary(Of String, Decimal)
        Dim result As New Dictionary(Of String, Decimal)

        Dim lignes = _executor.ExecuteNamedReader(
            "selStatsParMois",
            Nothing,
            Function(r As SqlDataReader)
                Return New With {
                    .Mois = r("Mois").ToString(),
                    .CaTotal = CDec(r("CA_Total"))
                }
            End Function)

        For Each ligne In lignes
            result(ligne.Mois) = ligne.CaTotal
        Next

        Return result
    End Function

    Public Function LireTarifsValides() As Dictionary(Of String, Decimal)
        Dim result As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)

        Dim lignes = _executor.ExecuteNamedReader(
            "selTarifsValides",
            Nothing,
            Function(r As SqlDataReader)
                Return New With {
                    .NomTarif = r("NomTarif").ToString(),
                    .Montant = CDec(r("Montant"))
                }
            End Function)

        For Each ligne In lignes
            result(ligne.NomTarif) = ligne.Montant
        Next

        Return result
    End Function

    Public Function LireSeancesAvecFilm() As List(Of SeanceFilmStatRow)
        Return _executor.ExecuteNamedReader(
            Constantes.Sql.Selection.SeancesAvecFilm,
            Nothing,
            Function(r As SqlDataReader)
                Return New SeanceFilmStatRow With {
                    .IdFilm = r.GetValueOrDefault(Of Integer)("IdFilm"),
                    .TitreFilm = r.GetValueOrDefault(Of String)("TitreFilm"),
                    .DateHeureDebut = r.GetValueOrDefault(Of DateTime)("DateHeureDebut"),
                    .NbEntreesAdultes = r.GetValueOrDefault(Of Integer)("NbEntreesAdultes", 0),
                    .NbEntreesEnfants = r.GetValueOrDefault(Of Integer)("NbEntreesEnfants", 0),
                    .NbEntreesGroupeEnfants = r.GetValueOrDefault(Of Integer)("NbEntreesGroupeEnfants", 0)
                }
            End Function)
    End Function

End Class

Public Class SeanceFilmStatRow
    Public Property IdFilm As Integer
    Public Property TitreFilm As String
    Public Property DateHeureDebut As DateTime
    Public Property NbEntreesAdultes As Integer
    Public Property NbEntreesEnfants As Integer
    Public Property NbEntreesGroupeEnfants As Integer
End Class