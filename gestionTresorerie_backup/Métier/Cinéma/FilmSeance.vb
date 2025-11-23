' FilmSeance.vb
Public Class FilmSeance
    Public Property Titre As String
    Public Property DateDiffusion As Date
    Public Property HeureDiffusion As TimeSpan

    ' Infos enrichies TMDb
    Public Property Synopsis As String
    Public Property DureeMinutes As Integer?
    Public Property Genre As String
    Public Property Realisateur As String
    Public Property Casting As List(Of String)
    Public Property UrlAffiche As String
End Class
