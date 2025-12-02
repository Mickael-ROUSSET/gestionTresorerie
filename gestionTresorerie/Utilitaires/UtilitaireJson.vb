Module UtilitaireJson
    Public Function CleanJson(raw As String) As String
        If raw.StartsWith(")]}'") Then
            Return raw.Substring(4)   ' retire les 4 premiers caractères
        End If
        Return raw
    End Function

End Module
