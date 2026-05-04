Public Class AffectationReleveService

    Public Shared Sub AppliquerAffectation(mouvement As Mouvements,
                                           regles As IEnumerable(Of RegleAffectationReleve))

        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))
        If regles Is Nothing Then Exit Sub

        Dim libelle As String = If(mouvement.Note, String.Empty).ToUpperInvariant()

        Dim regle =
            regles.
            Where(Function(r) r IsNot Nothing AndAlso r.Actif).
            Where(Function(r) String.IsNullOrWhiteSpace(r.MotCle) = False).
            Where(Function(r) libelle.Contains(r.MotCle.ToUpperInvariant())).
            Where(Function(r) Not r.Sens.HasValue OrElse r.Sens.Value = mouvement.Sens).
            OrderBy(Function(r) r.Priorite).
            FirstOrDefault()

        If regle Is Nothing Then Exit Sub

        If regle.IdTiers.HasValue Then mouvement.Tiers = regle.IdTiers.Value
        If regle.IdCategorie.HasValue Then mouvement.Categorie = regle.IdCategorie.Value
        If regle.IdSousCategorie.HasValue Then mouvement.SousCategorie = regle.IdSousCategorie.Value
    End Sub

End Class