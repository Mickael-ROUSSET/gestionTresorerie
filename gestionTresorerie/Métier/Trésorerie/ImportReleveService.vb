Public Class ImportReleveService

    Public Shared Function ChargerDepuisFichier(fichier As String,
                                                Optional verifierDoublons As Boolean = False) As ImportReleveResult
        If String.IsNullOrWhiteSpace(fichier) Then
            Throw New ArgumentException("Le fichier de relevé est obligatoire.", NameOf(fichier))
        End If

        Dim lignes As List(Of LigneReleve) = LitRelevé.ParserFichier(fichier)

        Return ConstruireDepuisLignes(lignes, verifierDoublons)
    End Function

    Public Shared Function ConstruireDepuisLignes(lignes As IEnumerable(Of LigneReleve),
                                                  Optional verifierDoublons As Boolean = False) As ImportReleveResult
        If lignes Is Nothing Then
            Throw New ArgumentNullException(NameOf(lignes))
        End If

        Dim result As New ImportReleveResult()

        For Each ligne As LigneReleve In lignes
            Dim mouvement As Mouvements = ReleveToMouvementMapper.Mapper(ligne)

            Dim existe As Boolean = False
            If verifierDoublons Then
                existe = Mouvements.Existe(mouvement)
            End If

            result.LignesImport.Add(New LigneImportReleve With {
                .Ligne = ligne,
                .Mouvement = mouvement,
                .ExisteDeja = existe
            })
        Next

        Return result
    End Function

End Class