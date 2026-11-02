Option Strict On
Option Explicit On

Imports System
Imports System.Globalization

''' <summary>Mapping en mémoire uniquement, sans accès aux ressources externes.</summary>
Public NotInheritable Class PropositionMouvementMapper
    Private Sub New()
    End Sub

    ''' <summary>
    ''' L'appelant fournit une clé unique par document et le sens proposé.
    ''' Seule date_facture est utilisée ; aucune échéance ou date historique de secours.
    ''' </summary>
    Public Shared Function Mapper(document As Facture, cleSource As String,
                                  sens As PropositionMouvement.SensMouvement,
                                  Optional sensConfirme As Boolean = False) As PropositionMouvement
        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        ValiderDevise(document.devise)
        Return New PropositionMouvement(cleSource, "FACTURE", LireDate(document.date_facture),
                                       document.montant_ttc, sens, "Facture", document.numero_facture,
                                       Avertissements(sensConfirme))
    End Function

    ''' <summary>
    ''' L'unicité de cleSource est garantie par l'appelant, jamais déduite du numéro de chèque.
    ''' Aucun champ d'émetteur ne détermine le sens.
    ''' </summary>
    Public Shared Function Mapper(document As Cheque, cleSource As String,
                                  sens As PropositionMouvement.SensMouvement,
                                  Optional sensConfirme As Boolean = False) As PropositionMouvement
        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        ValiderDevise(document.devise)
        Return New PropositionMouvement(cleSource, "CHEQUE", LireDate(document.dateChq),
                                       document.montant_numerique, sens, "Chèque", document.numero_du_cheque,
                                       Avertissements(sensConfirme))
    End Function

    Private Shared Sub ValiderDevise(devise As String)
        If Not String.Equals(devise, "EUR", StringComparison.Ordinal) Then
            Throw New ArgumentException("La devise doit être explicitement EUR.", NameOf(devise))
        End If
    End Sub

    Private Shared Function LireDate(valeur As String) As DateTime
        Dim resultat As DateTime
        ' Formats invariants année-mois-jour uniquement, sans heure ni normalisation.
        If Not DateTime.TryParseExact(valeur, New String() {"yyyy-MM-dd", "yyyyMMdd"},
                                      CultureInfo.InvariantCulture, DateTimeStyles.None, resultat) OrElse
           resultat = DateTime.MinValue Then
            Throw New ArgumentException("Date obligatoire au format yyyy-MM-dd ou yyyyMMdd.", NameOf(valeur))
        End If
        Return resultat
    End Function

    Private Shared Function Avertissements(sensConfirme As Boolean) As String()
        If sensConfirme Then Return Array.Empty(Of String)()
        Return New String() {"Sens débit/crédit incertain : confirmation humaine nécessaire."}
    End Function
End Class
