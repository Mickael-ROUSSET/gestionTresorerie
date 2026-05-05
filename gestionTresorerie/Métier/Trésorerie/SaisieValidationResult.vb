Public Class SaisieValidationResult

    Public Property IsValid As Boolean
    Public Property Erreurs As New Dictionary(Of String, String)

    Public Sub AjouterErreur(champ As String, message As String)
        If Not Erreurs.ContainsKey(champ) Then
            Erreurs.Add(champ, message)
        End If
        IsValid = False
    End Sub

End Class