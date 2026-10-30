Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Pont hors ligne entre les identites IMAP des pieces jointes deja analysees
''' et la revue de propositions. Aucun acces Gmail, fichier ou base de donnees.
''' </summary>
Public NotInheritable Class GmailDocumentPropositionAdapter
    Public NotInheritable Class PieceAnalysee
        Public Sub New(account As String, uidValidity As UInteger, uid As UInteger,
                       attachmentIndex As Integer, document As DocumentAgumaaa,
                       sens As PropositionMouvement.SensMouvement)
            ' L'identite provient toujours du message et de la position d'origine,
            ' jamais du nom de fichier (potentiellement identique entre deux pieces).
            Me.CleSource = GmailDocumentSourceKey.MakeKey(account, uidValidity, uid, attachmentIndex)
            Me.Document = document
            Me.Sens = sens
        End Sub

        Public ReadOnly Property CleSource As String
        Public ReadOnly Property Document As DocumentAgumaaa
        Public ReadOnly Property Sens As PropositionMouvement.SensMouvement
    End Class

    ''' <summary>
    ''' Les documents sont deja extraits par l'appelant. Le sens reste explicite.
    ''' Les doublons, documents absents et erreurs metier sont conserves comme rejets
    ''' par PropositionDocumentService, avec leur index d'origine dans le lot.
    ''' </summary>
    Public Function Proposer(pieces As IEnumerable(Of PieceAnalysee)) As PropositionDocumentService.Resultat
        Return New PropositionAnalyseAdapter().Proposer(ConstruireEntrees(pieces))
    End Function

    ''' <summary>Produit seulement un instantane en attente de validation humaine.</summary>
    Public Function PreparerRevue(pieces As IEnumerable(Of PieceAnalysee)) As PropositionRevue
        Return New PropositionAnalyseAdapter().PreparerRevue(ConstruireEntrees(pieces))
    End Function

    Private Shared Function ConstruireEntrees(pieces As IEnumerable(Of PieceAnalysee)) As List(Of PropositionDocumentService.Entree)
        If pieces Is Nothing Then Throw New ArgumentNullException(NameOf(pieces))
        Dim entrees As New List(Of PropositionDocumentService.Entree)()
        For Each piece In pieces
            If piece Is Nothing Then
                entrees.Add(Nothing)
            Else
                entrees.Add(New PropositionDocumentService.Entree(piece.Document, piece.CleSource, piece.Sens))
            End If
        Next
        Return entrees
    End Function
End Class
