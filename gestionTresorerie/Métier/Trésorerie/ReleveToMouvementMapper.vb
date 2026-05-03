Public Class ReleveToMouvementMapper

    Public Shared Function Mapper(ligne As LigneReleve) As Mouvements
        If ligne Is Nothing Then Throw New ArgumentNullException(NameOf(ligne))

        Dim montant As Decimal
        Dim sens As Boolean

        If ligne.MontantCredit.HasValue Then
            montant = ligne.MontantCredit.Value
            sens = True
        ElseIf ligne.MontantDebit.HasValue Then
            montant = ligne.MontantDebit.Value
            sens = False
        Else
            Throw New InvalidOperationException("La ligne de relevé ne contient ni débit ni crédit.")
        End If

        Return New Mouvements With {
            .DateMvt = ligne.DateOperation,
            .Note = ligne.Libelle,
            .Montant = montant,
            .Sens = sens,
            .Etat = False,
            .Modifiable = True,
            .TypeMouvement = "RELEVE"
        }
    End Function

    Public Shared Function MapperListe(lignes As IEnumerable(Of LigneReleve)) As List(Of Mouvements)
        If lignes Is Nothing Then Throw New ArgumentNullException(NameOf(lignes))

        Return lignes.Select(Function(l) Mapper(l)).ToList()
    End Function

End Class