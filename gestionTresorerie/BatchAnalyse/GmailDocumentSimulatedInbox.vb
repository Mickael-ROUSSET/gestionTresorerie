Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Source de messages IMAP fictifs en memoire pour exercer le parcours
''' message -> pieces -> JSON extrait -> propositions -> revue humaine.
''' Ne se connecte a aucun compte Gmail et ne lit aucun fichier.
''' </summary>
Public NotInheritable Class GmailDocumentSimulatedInbox
    Private Sub New()
    End Sub

    Public NotInheritable Class PieceFictive
        Public Sub New(jsonExtraction As String, sens As PropositionMouvement.SensMouvement)
            Me.JsonExtraction = jsonExtraction
            Me.Sens = sens
        End Sub

        Public ReadOnly Property JsonExtraction As String
        Public ReadOnly Property Sens As PropositionMouvement.SensMouvement
    End Class

    Public NotInheritable Class MessageFictif
        Public Sub New(compte As String, uidValidity As UInteger, uid As UInteger,
                       pieces As IEnumerable(Of PieceFictive))
            If pieces Is Nothing Then Throw New ArgumentNullException(NameOf(pieces))
            Me.Compte = compte
            Me.UidValidity = uidValidity
            Me.Uid = uid
            Me.Pieces = New List(Of PieceFictive)(pieces).AsReadOnly()
        End Sub

        Public ReadOnly Property Compte As String
        Public ReadOnly Property UidValidity As UInteger
        Public ReadOnly Property Uid As UInteger
        Public ReadOnly Property Pieces As IReadOnlyList(Of PieceFictive)
    End Class

    ''' <summary>
    ''' Conserve l'indice original de chaque piece, meme absente ou invalide.
    ''' Aucun document n'est approuve ou persiste automatiquement.
    ''' </summary>
    Public Shared Function PreparerRevue(messages As IEnumerable(Of MessageFictif)) As GmailDocumentReviewWorkflow
        If messages Is Nothing Then Throw New ArgumentNullException(NameOf(messages))
        Dim pieces As New List(Of GmailDocumentExtractionAdapter.PieceJson)()
        For Each message In messages
            If message Is Nothing Then Throw New ArgumentException("Message fictif absent.", NameOf(messages))
            For index As Integer = 0 To message.Pieces.Count - 1
                Dim piece = message.Pieces(index)
                Dim json As String = If(piece Is Nothing, Nothing, piece.JsonExtraction)
                Dim sens = If(piece Is Nothing,
                              PropositionMouvement.SensMouvement.Debit,
                              piece.Sens)
                pieces.Add(New GmailDocumentExtractionAdapter.PieceJson(
                    message.Compte, message.UidValidity, message.Uid, index, json, sens))
            Next
        Next
        Return GmailDocumentReviewWorkflow.PreparerDepuisJson(pieces)
    End Function
End Class
