Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Orchestration en memoire uniquement ; aucun document n'est modifie ni approuve.</summary>
Public NotInheritable Class PropositionDocumentService
    Public NotInheritable Class Entree
        Public Sub New(document As DocumentAgumaaa, cleSource As String,
                       sens As PropositionMouvement.SensMouvement,
                       Optional provenance As String = Nothing)
            Me.Provenance = provenance
            Me.Document = document
            Me.CleSource = cleSource
            Me.Sens = sens
        End Sub

        Friend Property ErreurCorrespondance As String
        Public ReadOnly Property Provenance As String
        Public ReadOnly Property Document As DocumentAgumaaa
        Public ReadOnly Property CleSource As String
        Public ReadOnly Property Sens As PropositionMouvement.SensMouvement
    End Class

    Public NotInheritable Class Rejet
        Public Sub New(indexEntree As Integer, cleSource As String, motif As String)
            Me.IndexEntree = indexEntree
            Me.CleSource = cleSource
            Me.Motif = motif
        End Sub

        Public ReadOnly Property IndexEntree As Integer
        Public ReadOnly Property CleSource As String
        Public ReadOnly Property Motif As String
    End Class

    ''' <summary>Instantane de provenance, sans reference au document analyse.</summary>
    Public NotInheritable Class SourceProposition
        Friend Sub New(index As Integer, entree As Entree)
            IndexEntree = index
            CleSource = entree.CleSource
            IdDoc = entree.Document.IdDoc
            CheminDoc = entree.Document.CheminDoc
            Provenance = entree.Provenance
        End Sub

        Public ReadOnly Property IndexEntree As Integer
        Public ReadOnly Property CleSource As String
        Public ReadOnly Property IdDoc As Integer
        Public ReadOnly Property CheminDoc As String
        Public ReadOnly Property Provenance As String
    End Class

    Public NotInheritable Class Resultat
        Friend Sub New(propositions As List(Of PropositionMouvement), rejets As List(Of Rejet), sources As List(Of SourceProposition))
            Me.Propositions = propositions.AsReadOnly()
            Me.Rejets = rejets.AsReadOnly()
            Me.Sources = sources.AsReadOnly()
        End Sub

        Public ReadOnly Property Sources As IReadOnlyList(Of SourceProposition)
        Public ReadOnly Property Propositions As IReadOnlyList(Of PropositionMouvement)
        Public ReadOnly Property Rejets As IReadOnlyList(Of Rejet)
    End Class

    ''' <summary>
    ''' Conserve l'ordre d'entree. Les cles sont comparees exactement (Ordinal), sans normalisation.
    ''' La premiere occurrence reserve la cle, meme invalide ; les suivantes sont rejetees.
    ''' Les objets sont compares par reference ; les IdDoc non nuls doivent aussi etre uniques.
    ''' Zero signifie non attribue : aucune identite n'est deduite du chemin ou de la reference.
    ''' Sources suit l'ordre des propositions et conserve leur provenance exacte en instantane.
    ''' Aucun etat n'est conserve entre appels : un meme lot retourne les memes valeurs.
    ''' Une erreur de validation est isolee par document ; une erreur d'enumeration reste visible.
    ''' </summary>
    Public Function Proposer(entrees As IEnumerable(Of Entree)) As Resultat
        If entrees Is Nothing Then Throw New ArgumentNullException(NameOf(entrees))
        Dim propositions As New List(Of PropositionMouvement)()
        Dim rejets As New List(Of Rejet)()
        Dim cles As New HashSet(Of String)(StringComparer.Ordinal)
        Dim documents As New HashSet(Of DocumentAgumaaa)(ReferenceEqualityComparer.Instance)
        Dim identifiants As New HashSet(Of Integer)()
        Dim sources As New List(Of SourceProposition)()
        Dim index As Integer = 0
        For Each entree In entrees
            Dim cle = If(entree Is Nothing, Nothing, entree.CleSource)
            Try
                If entree Is Nothing Then Throw New ArgumentException("Entree absente.")
                If entree.ErreurCorrespondance IsNot Nothing Then Throw New ArgumentException(entree.ErreurCorrespondance)
                If String.IsNullOrWhiteSpace(cle) Then Throw New ArgumentException("Cle source obligatoire.")
                If Not cles.Add(cle) Then Throw New ArgumentException("Cle source dupliquee dans le lot.")
                If entree.Document Is Nothing Then Throw New ArgumentException("Document absent.")
                If Not documents.Add(entree.Document) Then Throw New ArgumentException("Document deja present dans le lot sous une autre cle source.")
                If entree.Document.IdDoc <> 0 AndAlso Not identifiants.Add(entree.Document.IdDoc) Then
                    Throw New ArgumentException("Identifiant document duplique dans le lot.")
                End If
                If TypeOf entree.Document Is Facture Then
                    propositions.Add(PropositionMouvementMapper.Mapper(
                        DirectCast(entree.Document, Facture), cle, entree.Sens))
                ElseIf TypeOf entree.Document Is Cheque Then
                    propositions.Add(PropositionMouvementMapper.Mapper(
                        DirectCast(entree.Document, Cheque), cle, entree.Sens))
                Else
                    Throw New ArgumentException("Type de document non pris en charge.")
                End If
                sources.Add(New SourceProposition(index, entree))
            Catch ex As ArgumentException
                ' Messages locaux ou validations du mapper, sans journalisation du document.
                rejets.Add(New Rejet(index, cle, ex.Message))
            End Try
            index += 1
        Next
        Return New Resultat(propositions, rejets, sources)
    End Function
End Class
