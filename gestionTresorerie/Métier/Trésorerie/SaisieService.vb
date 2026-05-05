Imports System.Globalization

Public Class SaisieService

    Private Shared ReadOnly CultureFr As CultureInfo =
        CultureInfo.GetCultureInfo("fr-FR")
    Public Shared Sub Valider(dto As SaisieMouvementDto)
        If dto Is Nothing Then
            Throw New ArgumentNullException(NameOf(dto))
        End If

        If dto.DateMvt = DateTime.MinValue Then
            Throw New ArgumentException("La date du mouvement est obligatoire.", NameOf(dto.DateMvt))
        End If

        If String.IsNullOrWhiteSpace(dto.MontantTexte) Then
            Throw New ArgumentException("Le montant est obligatoire.", NameOf(dto.MontantTexte))
        End If

        Dim montant As Decimal = ParserMontant(dto.MontantTexte)

        If montant <= 0D Then
            Throw New ArgumentException("Le montant doit être strictement positif.", NameOf(dto.MontantTexte))
        End If
    End Sub
    Public Shared Function CreerMouvement(dto As SaisieMouvementDto) As Mouvements
        Valider(dto)

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