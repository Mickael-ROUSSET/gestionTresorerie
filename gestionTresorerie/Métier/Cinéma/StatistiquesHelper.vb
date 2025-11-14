Public Class StatFilm
    Public Property IdFilm As Integer
    Public Property Titre As String
    Public Property NbSeances As Integer
    Public Property TotalAdultes As Integer
    Public Property TotalEnfants As Integer
    Public Property TotalGroupeEnfants As Integer
    Public Property CA_Adultes As Decimal
    Public Property CA_Enfants As Decimal
    Public Property CA_GroupeEnfants As Decimal
    Public Property CA_Total As Decimal
End Class

Public Class StatistiquesHelper

    Public Shared Function GetStatsParFilm() As List(Of StatFilm)
        Dim result As New List(Of StatFilm)
        Using cmd = SqlCommandBuilder.CreateSqlCommand("SELECT * FROM vStatsParFilm")
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    result.Add(New StatFilm With {
                        .IdFilm = CInt(rdr("IdFilm")),
                        .Titre = rdr("Titre").ToString(),
                        .NbSeances = CInt(rdr("NbSeances")),
                        .TotalAdultes = CInt(rdr("TotalAdultes")),
                        .TotalEnfants = CInt(rdr("TotalEnfants")),
                        .TotalGroupeEnfants = CInt(rdr("TotalGroupeEnfants")),
                        .CA_Adultes = CDec(rdr("CA_Adultes")),
                        .CA_Enfants = CDec(rdr("CA_Enfants")),
                        .CA_GroupeEnfants = CDec(rdr("CA_GroupeEnfants")),
                        .CA_Total = CDec(rdr("CA_Total"))
                    })
                End While
            End Using
        End Using
        Return result
    End Function

End Class
