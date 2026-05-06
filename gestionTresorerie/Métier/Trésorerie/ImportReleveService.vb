Public Class ImportReleveService

    Public Shared Function ChargerDepuisFichier(fichier As String) As ImportReleveResult
        If String.IsNullOrWhiteSpace(fichier) Then
            Throw New ArgumentException("Le fichier de relevé est obligatoire.", NameOf(fichier))
        End If

        Dim lignes As List(Of LigneReleve) = LitRelevé.ParserFichier(fichier)
        Return ConstruireDepuisLignes(lignes)
    End Function

    Public Shared Function ConstruireDepuisLignes(lignes As IEnumerable(Of LigneReleve)) As ImportReleveResult
        If lignes Is Nothing Then
            Throw New ArgumentNullException(NameOf(lignes))
        End If

        Dim result As New ImportReleveResult()

        result.Lignes = lignes.ToList()
        result.Mouvements =
            result.Lignes.
            Select(Function(l) ReleveToMouvementMapper.Mapper(l)).
            ToList()

        result.NbLignes = result.Lignes.Count
        result.NbMouvements = result.Mouvements.Count

        Return result
    End Function

End Class