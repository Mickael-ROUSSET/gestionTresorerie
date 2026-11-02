Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Instantane de presentation en memoire, sans approbation ni effet externe.</summary>
Public NotInheritable Class PropositionRevue
    Public Enum StatutRevue
        AwaitingHumanReview = 0
    End Enum

    Public NotInheritable Class Ligne
        Friend Sub New(proposition As PropositionMouvement)
            CleSource = proposition.CleSource
            TypeDocument = proposition.TypeDocument
            DateProposee = proposition.DateProposee
            Montant = proposition.Montant
            Sens = proposition.Sens
            Libelle = proposition.Libelle
            Reference = proposition.Reference
            AvertissementsValidation = New List(Of String)(proposition.AvertissementsValidation).AsReadOnly()
        End Sub

        Public ReadOnly Property CleSource As String
        Public ReadOnly Property TypeDocument As String
        Public ReadOnly Property DateProposee As DateTime
        Public ReadOnly Property Montant As Decimal
        Public ReadOnly Property Sens As PropositionMouvement.SensMouvement
        Public ReadOnly Property Libelle As String
        Public ReadOnly Property Reference As String
        Public ReadOnly Property AvertissementsValidation As IReadOnlyList(Of String)
        Public ReadOnly Property Statut As StatutRevue
            Get
                Return StatutRevue.AwaitingHumanReview
            End Get
        End Property
    End Class

    ''' <summary>Diagnostic uniquement : aucun document ni octet source n'est conserve.</summary>
    Public NotInheritable Class DiagnosticRejet
        Friend Sub New(rejet As PropositionDocumentService.Rejet)
            IndexEntree = rejet.IndexEntree
            CleSource = rejet.CleSource
            Motif = rejet.Motif
        End Sub

        Public ReadOnly Property IndexEntree As Integer
        Public ReadOnly Property CleSource As String
        Public ReadOnly Property Motif As String
    End Class

    ''' <summary>
    ''' Conserve l'ordre des propositions et celui des rejets du resultat.
    ''' Les rejets restent separes : ils n'ont pas de montant ou de date valide.
    ''' Aucune reference au resultat ou aux documents d'origine n'est retenue.
    ''' </summary>
    Public Sub New(resultat As PropositionDocumentService.Resultat)
        If resultat Is Nothing Then Throw New ArgumentNullException(NameOf(resultat))
        Dim lignes As New List(Of Ligne)()
        For Each proposition In resultat.Propositions
            lignes.Add(New Ligne(proposition))
        Next
        Dim rejets As New List(Of DiagnosticRejet)()
        For Each rejet In resultat.Rejets
            rejets.Add(New DiagnosticRejet(rejet))
        Next
        Me.Lignes = lignes.AsReadOnly()
        Me.Rejets = rejets.AsReadOnly()
    End Sub

    Public ReadOnly Property Lignes As IReadOnlyList(Of Ligne)
    Public ReadOnly Property Rejets As IReadOnlyList(Of DiagnosticRejet)
End Class
