Public Class SaisieValidationService

    Public Shared Function Valider(dto As SaisieMouvementDto,
                                   tiersSelectionne As Boolean,
                                   categorieSelectionnee As Boolean,
                                   sousCategorieSelectionnee As Boolean,
                                   typeMvtSelectionne As Boolean) As SaisieValidationResult

        Dim result As New SaisieValidationResult With {.IsValid = True}

        If dto Is Nothing Then
            result.AjouterErreur("Formulaire", "Les données de saisie sont absentes.")
            Return result
        End If

        If String.IsNullOrWhiteSpace(dto.MontantTexte) Then
            result.AjouterErreur("Montant", "Le montant est obligatoire.")
        Else
            Try
                SaisieService.ParserMontant(dto.MontantTexte)
            Catch ex As Exception
                result.AjouterErreur("Montant", ex.Message)
            End Try
        End If

        If Not tiersSelectionne Then
            result.AjouterErreur("Tiers", "Veuillez sélectionner un tiers.")
        End If

        If Not categorieSelectionnee Then
            result.AjouterErreur("Categorie", "Veuillez sélectionner une catégorie.")
        End If

        If Not sousCategorieSelectionnee Then
            result.AjouterErreur("SousCategorie", "Veuillez sélectionner une sous-catégorie.")
        End If

        If Not typeMvtSelectionne Then
            result.AjouterErreur("TypeMvt", "Veuillez sélectionner un type de mouvement.")
        End If

        Return result
    End Function

End Class