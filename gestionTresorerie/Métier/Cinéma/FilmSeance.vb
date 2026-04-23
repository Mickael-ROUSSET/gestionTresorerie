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
    ''' <summary>
    ''' Transfère les données d'un objet de détails vers cette instance.
    ''' </summary>
    Public Sub RemplirDetails(details As Object)
        If details Is Nothing Then Return

        Me.Synopsis = details.Synopsis
        Me.DureeMinutes = details.DureeMinutes.ToString()
        Me.Genre = details.Genre
        Me.Realisateur = details.Realisateur
        Me.Casting = details.Casting
        Me.UrlAffiche = details.UrlAffiche
    End Sub
End Class
