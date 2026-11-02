Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Controle pur avant une eventuelle approbation humaine, sans conversion ni persistance.</summary>
Public NotInheritable Class PropositionPrevalidation
    Private Sub New()
    End Sub

    Public Enum Anomalie
        PropositionAbsente
        MontantNonPositif
        DateInvalide
        CleSourceAbsente
        DocumentSourceAbsent
        ReferenceVide
        ReferenceAmbigue
        AvertissementsNonResolus
        IdentitesUtiliseesAbsentes
        IdentiteSourceReutilisee
        SensInvalide
    End Enum

    Public NotInheritable Class Resultat
        Friend Sub New(anomalies As List(Of Anomalie))
            Me.Anomalies = anomalies.AsReadOnly()
        End Sub

        Public ReadOnly Property Anomalies As IReadOnlyList(Of Anomalie)
        ''' <summary>Indique uniquement la preparation ; ne vaut jamais approbation.</summary>
        Public ReadOnly Property EstPrete As Boolean
            Get
                Return Anomalies.Count = 0
            End Get
        End Property
    End Class

    Public Shared Function Evaluer(proposition As PropositionMouvement,
                                   idDocumentSource As Integer,
                                   identitesUtilisees As IReadOnlyCollection(Of String),
                                   Optional referenceAmbigue As Boolean = True) As Resultat
        If proposition Is Nothing Then
            Return New Resultat(New List(Of Anomalie) From {Anomalie.PropositionAbsente})
        End If
        Return Evaluer(proposition.Montant, proposition.DateProposee, proposition.CleSource,
                       idDocumentSource, proposition.Reference, referenceAmbigue,
                       proposition.AvertissementsValidation, identitesUtilisees, proposition.Sens)
    End Function

    ''' <summary>
    ''' Permet aussi de controler des valeurs avant construction du modele immuable.
    ''' Nothing pour les collections signifie information absente et bloque la preparation.
    ''' Codes uniques dans l'ordre declare ci-dessous, independants de la culture et des textes recus.
    ''' Comparaison des identites identique a la conversion : Trim et OrdinalIgnoreCase.
    ''' Le document est fourni par l'appelant ; aucune verification externe de son existence.
    ''' </summary>
    Public Shared Function Evaluer(montant As Decimal, dateProposee As DateTime?, cleSource As String,
                                   idDocumentSource As Integer, reference As String, referenceAmbigue As Boolean,
                                   avertissements As IReadOnlyCollection(Of String),
                                   identitesUtilisees As IReadOnlyCollection(Of String),
                                   sens As PropositionMouvement.SensMouvement) As Resultat
        Dim anomalies As New List(Of Anomalie)()
        If montant <= 0D Then anomalies.Add(Anomalie.MontantNonPositif)
        If Not dateProposee.HasValue OrElse dateProposee.Value = DateTime.MinValue Then anomalies.Add(Anomalie.DateInvalide)
        If String.IsNullOrWhiteSpace(cleSource) Then anomalies.Add(Anomalie.CleSourceAbsente)
        If idDocumentSource <= 0 Then anomalies.Add(Anomalie.DocumentSourceAbsent)
        If String.IsNullOrWhiteSpace(reference) Then anomalies.Add(Anomalie.ReferenceVide)
        If referenceAmbigue Then anomalies.Add(Anomalie.ReferenceAmbigue)
        If avertissements Is Nothing OrElse avertissements.Count <> 0 Then anomalies.Add(Anomalie.AvertissementsNonResolus)
        If identitesUtilisees Is Nothing Then
            anomalies.Add(Anomalie.IdentitesUtiliseesAbsentes)
        ElseIf Not String.IsNullOrWhiteSpace(cleSource) Then
            For Each identite In identitesUtilisees
                If String.Equals(cleSource.Trim(), If(identite, String.Empty).Trim(), StringComparison.OrdinalIgnoreCase) Then
                    anomalies.Add(Anomalie.IdentiteSourceReutilisee)
                    Exit For
                End If
            Next
        End If
        If sens <> PropositionMouvement.SensMouvement.Debit AndAlso sens <> PropositionMouvement.SensMouvement.Credit Then
            anomalies.Add(Anomalie.SensInvalide)
        End If
        Return New Resultat(anomalies)
    End Function
End Class
