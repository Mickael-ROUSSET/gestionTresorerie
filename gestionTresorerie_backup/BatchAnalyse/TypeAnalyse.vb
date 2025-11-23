Public Class TypeAnalyse
    Public Property RepertoireSortie As String
    Public Property Type As String
    Public Property Prompt As String

    Public Sub New(repertoireSortie As String, type As String, prompt As String)
        Me.RepertoireSortie = repertoireSortie
        Me.Type = type
        Me.Prompt = prompt
    End Sub
End Class
