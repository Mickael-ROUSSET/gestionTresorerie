Public Class ImportReleveResult

    Public Property Lignes As List(Of LigneReleve)
    Public Property Mouvements As List(Of Mouvements)
    Public Property NbLignes As Integer
    Public Property NbMouvements As Integer

    Public Sub New()
        Lignes = New List(Of LigneReleve)()
        Mouvements = New List(Of Mouvements)()
    End Sub

End Class