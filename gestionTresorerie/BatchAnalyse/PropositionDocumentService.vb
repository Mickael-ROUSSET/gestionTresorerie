Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Orchestration en memoire uniquement ; aucun document n'est modifie ni approuve.</summary>
Public NotInheritable Class PropositionDocumentService
    Public NotInheritable Class Entree
        Public Sub New(document As DocumentAgumaaa, cleSource As String,
                       sens As PropositionMouvement.SensMouvement)
            Me.Document = document
            Me.CleSource = cleSource
            Me.Sens = sens
        End Sub

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

    Public NotInheritable Class Resultat
        Friend Sub New(propositions As List(Of PropositionMouvement), rejets As List(Of Rejet))
            Me.Propositions = propositions.AsReadOnly()
            Me.Rejets = rejets.AsReadOnly()
        End Sub

        Public ReadOnly Property Propositions As IReadOnlyList(Of PropositionMouvement)
        Public ReadOnly Property Rejets As IReadOnlyList(Of Rejet)
    End Class

    ''' <summary>
    ''' Conserve l'ordre d'entree. Les cles sont comparees apres Trim et sans tenir compte de la casse,
    ''' comme lors du controle des identites deja utilisees a la conversion.
    ''' La premiere occurrence reserve la cle, meme invalide ; les suivantes sont rejetees.
    ''' Aucun etat n'est conserve entre appels : un meme lot retourne les memes valeurs.
    ''' Une erreur de validation est isolee par document ; une erreur d'enumeration reste visible.
    ''' </summary>
    Public Function Proposer(entrees As IEnumerable(Of Entree)) As Resultat
        If entrees Is Nothing Then Throw New ArgumentNullException(NameOf(entrees))
        Dim propositions As New List(Of PropositionMouvement)()
        Dim rejets As New List(Of Rejet)()
        Dim cles As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim index As Integer = 0
        For Each entree In entrees
            Dim cle = If(entree Is Nothing, Nothing, entree.CleSource)
            Try
                If entree Is Nothing Then Throw New ArgumentException("Entree absente.")
                If String.IsNullOrWhiteSpace(cle) Then Throw New ArgumentException("Cle source obligatoire.")
                If Not cles.Add(cle.Trim()) Then Throw New ArgumentException("Cle source dupliquee dans le lot.")
                If entree.Document Is Nothing Then Throw New ArgumentException("Document absent.")
                If TypeOf entree.Document Is Facture Then
                    propositions.Add(PropositionMouvementMapper.Mapper(
                        DirectCast(entree.Document, Facture), cle, entree.Sens))
                ElseIf TypeOf entree.Document Is Cheque Then
                    propositions.Add(PropositionMouvementMapper.Mapper(
                        DirectCast(entree.Document, Cheque), cle, entree.Sens))
                Else
                    Throw New ArgumentException("Type de document non pris en charge.")
                End If
            Catch ex As ArgumentException
                ' Messages locaux ou validations du mapper, sans journalisation du document.
                rejets.Add(New Rejet(index, cle, ex.Message))
            End Try
            index += 1
        Next
        Return New Resultat(propositions, rejets)
    End Function
End Class
