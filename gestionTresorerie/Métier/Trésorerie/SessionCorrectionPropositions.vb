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
    Private ReadOnly _propositions As New List(Of PropositionMouvement)()
    Private ReadOnly _propositionsInitiales As New List(Of PropositionMouvement)()
    Private ReadOnly indices As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

    Public Sub New(resultat As PropositionDocumentService.Resultat)
        If resultat Is Nothing Then Throw New ArgumentNullException(NameOf(resultat))
        For Each proposition In resultat.Propositions
            If proposition Is Nothing Then Throw New ArgumentException("Proposition absente.", NameOf(resultat))
            If indices.ContainsKey(proposition.CleSource) Then
                Throw New ArgumentException("Identité source dupliquée.", NameOf(resultat))
            End If
            indices.Add(proposition.CleSource, _propositions.Count)
            _propositions.Add(proposition)
            _propositionsInitiales.Add(proposition)
        Next
    End Sub

    ''' <summary>Instantané ordonné ; les propositions elles-mêmes sont immuables.</summary>
    Public ReadOnly Property Propositions As IReadOnlyList(Of PropositionMouvement)
        Get
            Return New List(Of PropositionMouvement)(_propositions).AsReadOnly()
        End Get
    End Property

    ''' <summary>Origines immuables du lot, conservées même après correction.</summary>
    Public ReadOnly Property PropositionsInitiales As IReadOnlyList(Of PropositionMouvement)
        Get
            Return New List(Of PropositionMouvement)(_propositionsInitiales).AsReadOnly()
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
        Return _propositions(index)
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
        AppliquerResultatCorrection(correction)
        Return correction
    End Function

    ''' <summary>
    ''' Applique le résultat d'une correction déjà validée (par exemple issue de
    ''' FrmCorrectionProposition), sans approuver ni enregistrer le document.
    ''' Refuse un résultat devenu obsolète pour éviter l'écrasement silencieux.
    ''' </summary>
    Public Sub AppliquerResultatCorrection(correction As PropositionCorrection.Resultat)
        If correction Is Nothing Then Throw New ArgumentNullException(NameOf(correction))
        Dim actuelle = Selectionner(correction.Avant.CleSource)
        If Not Object.ReferenceEquals(actuelle, correction.Avant) Then
            Throw New InvalidOperationException("Correction obsolète : la proposition a déjà changé.")
        End If
        If Not String.Equals(correction.Proposition.CleSource, actuelle.CleSource, StringComparison.Ordinal) Then
            Throw New ArgumentException("L'identité du document doit rester inchangée.", NameOf(correction))
        End If
        _propositions(indices(actuelle.CleSource)) = correction.Proposition
    End Sub
End Class
