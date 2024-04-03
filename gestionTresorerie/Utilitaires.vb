Module Utilitaires

    Public Function IndexSelectionne(cbBox As ComboBox, sNiveau As String) As Integer
        Dim iIndex As Integer

        'Pas de catégorie
        iIndex = 0
        If Not sNiveau.Equals("") Then
            iIndex = cbBox.Items.IndexOf(sNiveau)
        End If
        Return iIndex
    End Function
End Module
