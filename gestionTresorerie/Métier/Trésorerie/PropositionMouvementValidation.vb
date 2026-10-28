Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Validation et conversion en mémoire uniquement, sans approbation de la proposition.</summary>
Public NotInheritable Class PropositionMouvementValidation
    Private Sub New()
    End Sub

    ''' <summary>
    ''' L'appelant confirme la référence et fournit le document source et les identités déjà utilisées.
    ''' L'ensemble n'est pas modifié : l'appelant doit enregistrer CleSource après conversion et
    ''' sérialiser les conversions pour empêcher la réutilisation concurrente d'une identité.
    ''' Aucun contrôle d'existence du document dans une ressource externe n'est effectué.
    ''' </summary>
    Public Shared Function Convertir(proposition As PropositionMouvement,
                                     idDocumentSource As Integer,
                                     identitesUtilisees As IReadOnlyCollection(Of String),
                                     Optional approbationHumaine As Boolean = False,
                                     Optional referenceAmbigue As Boolean = True) As Mouvements
        If proposition Is Nothing Then Throw New ArgumentNullException(NameOf(proposition))
        If identitesUtilisees Is Nothing Then Throw New ArgumentNullException(NameOf(identitesUtilisees))
        If Not approbationHumaine Then
            Throw New InvalidOperationException("Une approbation humaine explicite est obligatoire.")
        End If
        If idDocumentSource <= 0 OrElse String.IsNullOrWhiteSpace(proposition.CleSource) OrElse
           String.IsNullOrWhiteSpace(proposition.TypeDocument) Then
            Throw New ArgumentException("Un document source identifié est obligatoire.", NameOf(idDocumentSource))
        End If
        If proposition.Montant <= 0D Then
            Throw New ArgumentOutOfRangeException(NameOf(proposition), "Le montant doit être strictement positif.")
        End If
        If proposition.AvertissementsValidation.Count <> 0 Then
            Throw New InvalidOperationException("Les avertissements doivent être résolus avant conversion.")
        End If
        If referenceAmbigue OrElse String.IsNullOrWhiteSpace(proposition.Reference) Then
            Throw New InvalidOperationException("Une référence non ambiguë doit être confirmée explicitement.")
        End If
        For Each identite In identitesUtilisees
            If String.Equals(proposition.CleSource.Trim(), If(identite, String.Empty).Trim(),
                             StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Cette identité de proposition a déjà été utilisée.")
            End If
        Next

        Return New Mouvements With {
            .DateMvt = proposition.DateProposee,
            .Montant = proposition.Montant,
            .Sens = proposition.Sens = PropositionMouvement.SensMouvement.Credit,
            .Note = proposition.Libelle & vbLf & "Source : " & proposition.CleSource,
            .reference = proposition.Reference,
            .typeReference = proposition.TypeDocument,
            .idDoc = idDocumentSource,
            .TypeMouvement = proposition.TypeDocument,
            .Etat = False,
            .Modifiable = True
        }
    End Function
End Class
