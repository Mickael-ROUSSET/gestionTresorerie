Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Orchestration hors ligne des pieces Gmail deja analysees : propositions,
''' diagnostics et session de corrections humaines. Aucun acces Gmail ou SQL.
''' </summary>
Public NotInheritable Class GmailDocumentReviewWorkflow
    Private Sub New(resultat As PropositionDocumentService.Resultat,
                    sessionRevue As PropositionSessionRevue)
        Me.ResultatInitial = resultat
        Me.SessionRevue = sessionRevue
    End Sub

    Public ReadOnly Property ResultatInitial As PropositionDocumentService.Resultat
    Public ReadOnly Property SessionRevue As PropositionSessionRevue

    ''' <summary>
    ''' L'appelant fournit les documents deja extraits et leur sens explicite.
    ''' Aucune extraction ni lecture de message n'est lancee ici.
    ''' </summary>
    Public Shared Function Preparer(pieces As IEnumerable(Of GmailDocumentPropositionAdapter.PieceAnalysee)) As GmailDocumentReviewWorkflow
        If pieces Is Nothing Then Throw New ArgumentNullException(NameOf(pieces))
        Dim resultat = New GmailDocumentPropositionAdapter().Proposer(pieces)
        Return New GmailDocumentReviewWorkflow(resultat, New PropositionSessionRevue(resultat.Propositions))
    End Function

    ''' <summary>
    ''' Cree la fenetre sur le thread UI, sans l'afficher. Lors d'une reouverture,
    ''' les corrections de la session remplacent les propositions initiales,
    ''' dans le meme ordre ; les rejets restent visibles. Aucune approbation.
    ''' </summary>

    ''' <summary>
    ''' Revue en memoire de JSON fictifs deja extraits, rejets individuels inclus.
    ''' Aucune validation ni persistance automatique.
    ''' </summary>
    Public Shared Function PreparerDepuisJson(pieces As IEnumerable(Of GmailDocumentExtractionAdapter.PieceJson)) As GmailDocumentReviewWorkflow
        Dim resultat = GmailDocumentExtractionAdapter.ProposerLot(pieces)
        Return New GmailDocumentReviewWorkflow(resultat, New PropositionSessionRevue(resultat.Propositions))
    End Function

    Public Function CreerFenetre() As FrmRevuePropositions
        Dim courantes As New List(Of PropositionMouvement)()
        For Each originale In ResultatInitial.Propositions
            Dim courante As PropositionMouvement = Nothing
            If Not SessionRevue.Propositions.TryGetValue(originale.CleSource, courante) Then
                Throw New InvalidOperationException("Proposition absente de la session.")
            End If
            courantes.Add(courante)
        Next
        Dim rejets As New List(Of PropositionDocumentService.Rejet)(ResultatInitial.Rejets)
        Dim instantane As New PropositionDocumentService.Resultat(courantes, rejets)
        Return FrmPrincipale.CreerFenetreRevuePropositions(instantane, SessionRevue)
    End Function
End Class
