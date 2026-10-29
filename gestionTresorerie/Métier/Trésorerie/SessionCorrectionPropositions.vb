Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Session de correction strictement en mémoire pour un lot de propositions.
''' Une correction ne vaut jamais approbation et ne déclenche aucune écriture.
''' Les identités des documents restent inchangées.
''' </summary>
Public NotInheritable Class SessionCorrectionPropositions
    Private ReadOnly propositions As New List(Of PropositionMouvement)()
    Private ReadOnly indices As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

    Public Sub New(resultat As PropositionDocumentService.Resultat)
        If resultat Is Nothing Then Throw New ArgumentNullException(NameOf(resultat))
        For Each proposition In resultat.Propositions
            If proposition Is Nothing Then Throw New ArgumentException("Proposition absente.", NameOf(resultat))
            If indices.ContainsKey(proposition.CleSource) Then
                Throw New ArgumentException("Identité source dupliquée.", NameOf(resultat))
            End If
            indices.Add(proposition.CleSource, propositions.Count)
            propositions.Add(proposition)
        Next
    End Sub

    ''' <summary>Instantané ordonné ; les propositions elles-mêmes sont immuables.</summary>
    Public ReadOnly Property Propositions As IReadOnlyList(Of PropositionMouvement)
        Get
            Return New List(Of PropositionMouvement)(propositions).AsReadOnly()
        End Get
    End Property

    ''' <summary>Recherche exacte ; jamais de sélection implicite par numéro de ligne.</summary>
    Public Function Selectionner(cleSource As String) As PropositionMouvement
        If String.IsNullOrWhiteSpace(cleSource) Then
            Throw New ArgumentException("Identité source obligatoire.", NameOf(cleSource))
        End If
        Dim index As Integer
        If Not indices.TryGetValue(cleSource, index) Then
            Throw New KeyNotFoundException("Proposition introuvable.")
        End If
        Return propositions(index)
    End Function

    ''' <summary>
    ''' Remplace uniquement la proposition sélectionnée après validation métier.
    ''' En cas d'erreur, la session reste inchangée. Aucune confirmation automatique.
    ''' </summary>
    Public Function Corriger(cleSource As String, dateProposee As DateTime?, montant As Decimal,
                            sens As PropositionMouvement.SensMouvement,
                            libelle As String, reference As String) As PropositionCorrection.Resultat
        Dim source = Selectionner(cleSource)
        Dim correction = PropositionCorrection.Corriger(source, dateProposee, montant, sens, libelle, reference)
        propositions(indices(cleSource)) = correction.Proposition
        Return correction
    End Function
End Class
