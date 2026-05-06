Public Class MouvementBuildService

    Public Shared Function ConstruireDepuisSaisie(dto As SaisieMouvementDto,
                                                  options As MouvementBuildOptions) As Mouvements
        Dim validation = SaisieValidationService.Valider(
            dto,
            options.TiersSelectionne,
            options.CategorieSelectionnee,
            options.SousCategorieSelectionnee,
            options.TypeMvtSelectionne)

        If Not validation.IsValid Then
            Throw New ArgumentException(String.Join(vbCrLf, validation.Erreurs.Values))
        End If

        Dim mouvement As Mouvements = SaisieService.CreerMouvement(dto)

        AppliquerMetadonnees(mouvement, options)

        Return mouvement
    End Function

    Public Shared Function ConstruireDepuisReleve(ligne As LigneReleve,
                                                  regles As IEnumerable(Of RegleAffectationReleve)) As Mouvements
        Dim mouvement As Mouvements = ReleveToMouvementMapper.Mapper(ligne)

        AffectationReleveService.AppliquerAffectation(mouvement, regles)

        Return mouvement
    End Function

    Private Shared Sub AppliquerMetadonnees(mouvement As Mouvements,
                                            options As MouvementBuildOptions)
        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))
        If options Is Nothing Then Exit Sub

        mouvement.Evenement = Nettoyer(options.EvenementLibelle)
        mouvement.TypeMouvement = Nettoyer(options.TypeMvtLibelle)
        mouvement.Etat = options.Rapproche
        mouvement.Modifiable = options.Modifiable
        mouvement.NumeroRemise = Nettoyer(options.NumeroRemise)
        mouvement.reference = Nettoyer(options.Reference)
        mouvement.typeReference = Nettoyer(options.TypeReference)
    End Sub

    Private Shared Function Nettoyer(value As String) As String
        If value Is Nothing Then Return String.Empty
        Return value.Trim()
    End Function

End Class