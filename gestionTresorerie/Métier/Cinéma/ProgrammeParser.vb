Public Class ProgrammeParser
    Public Shared Function ParseLigne(ligne As String) As FilmSeance
        Dim sep() As Char = {";"c, "|"c}
        Dim tokens = ligne.Split(sep, StringSplitOptions.RemoveEmptyEntries).Select(Function(s) s.Trim()).ToArray()

        If tokens.Length < 3 Then Return Nothing

        Try
            Return New FilmSeance With {
                .Titre = tokens(0),
                .DateDiffusion = Date.Parse(tokens(1)),
                .HeureDiffusion = TimeSpan.Parse(tokens(2).Replace("h", ":"))
            }
        Catch ex As Exception
            Logger.ERR($"Erreur parsing ligne : {ligne}")
            Return Nothing
        End Try
    End Function
End Class