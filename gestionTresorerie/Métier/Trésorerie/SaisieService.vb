Imports System.Globalization

Public Class SaisieService

    Private Shared ReadOnly CultureFr As CultureInfo =
        CultureInfo.GetCultureInfo("fr-FR")

    Public Shared Function CreerMouvement(dto As SaisieMouvementDto) As Mouvements
        If dto Is Nothing Then Throw New ArgumentNullException(NameOf(dto))

        Dim montant As Decimal = ParserMontant(dto.MontantTexte)

        Return New Mouvements With {
            .DateMvt = dto.DateMvt.Date,
            .Note = NettoyerTexte(dto.Note),
            .Montant = montant,
            .Sens = Not dto.EstDebit,
            .Categorie = dto.IdCategorie,
            .SousCategorie = dto.IdSousCategorie,
            .Tiers = dto.IdTiers,
            .idDoc = dto.IdDoc,
            .Etat = False,
            .Modifiable = True
        }
    End Function

    Public Shared Function ParserMontant(montantTexte As String) As Decimal
        If String.IsNullOrWhiteSpace(montantTexte) Then
            Throw New ArgumentException("Le montant est obligatoire.", NameOf(montantTexte))
        End If

        Dim nettoye As String =
            montantTexte.
                Replace("€", String.Empty).
                Replace(ChrW(160), String.Empty).
                Replace(" ", String.Empty).
                Trim()

        Dim montant As Decimal
        If Not Decimal.TryParse(nettoye, NumberStyles.Number, CultureFr, montant) Then
            Throw New FormatException($"Montant invalide : {montantTexte}")
        End If

        If montant < 0D Then
            montant = Math.Abs(montant)
        End If

        Return montant
    End Function

    Private Shared Function NettoyerTexte(value As String) As String
        If value Is Nothing Then Return String.Empty
        Return value.Trim()
    End Function

End Class