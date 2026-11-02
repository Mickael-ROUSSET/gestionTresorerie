Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

''' <summary>Session exclusivement en memoire, sans document source ni service externe.
''' Les cles restent exactes ; les collisions Trim/OrdinalIgnoreCase sont refusees
''' car elles deviendraient ambigues lors de la prevalidation.</summary>
Public NotInheritable Class PropositionSessionRevue
    Private ReadOnly courantes As New Dictionary(Of String, PropositionMouvement)(StringComparer.Ordinal)
    Private ReadOnly historique As New List(Of PropositionCorrection.Resultat)()
    Private ReadOnly examens As New Dictionary(Of String, PropositionMouvement)(StringComparer.Ordinal)

    Public Sub New(propositions As IEnumerable(Of PropositionMouvement))
        If propositions Is Nothing Then Throw New ArgumentNullException(NameOf(propositions))
        Dim identites As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each proposition In propositions
            If proposition Is Nothing Then Throw New ArgumentException("Proposition absente.", NameOf(propositions))
            If proposition.Etat <> PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine Then
                Throw New ArgumentException("Seules les propositions non approuvees sont admises.", NameOf(propositions))
            End If
            If Not identites.Add(proposition.CleSource.Trim()) Then
                Throw New ArgumentException("Identite source dupliquee ou ambigue.", NameOf(propositions))
            End If
            courantes.Add(proposition.CleSource, proposition)
        Next
        Me.Propositions = New ReadOnlyDictionary(Of String, PropositionMouvement)(courantes)
        Me.Audits = historique.AsReadOnly()
    End Sub

    Public ReadOnly Property Propositions As IReadOnlyDictionary(Of String, PropositionMouvement)
    Public ReadOnly Property Audits As IReadOnlyList(Of PropositionCorrection.Resultat)

    ''' <summary>Nombre d'examens humains explicitement confirmes dans cette session.
    ''' Ne constitue ni une prevalidation, ni une approbation, ni une ecriture.</summary>
    Public ReadOnly Property NombreExamensConfirmes As Integer
        Get
            Return examens.Count
        End Get
    End Property

    Public Function EstExamenConfirme(cleSource As String) As Boolean
        Dim courante = Selectionner(cleSource)
        Dim examinee As PropositionMouvement = Nothing
        Return examens.TryGetValue(cleSource, examinee) AndAlso
            Object.ReferenceEquals(courante, examinee)
    End Function

    ''' <summary>Trace uniquement l'examen explicite de la version courante en memoire.
    ''' Une version ancienne ou issue d'une autre session ne peut etre confirmee.</summary>
    Public Sub ConfirmerExamen(cleSource As String, propositionExaminee As PropositionMouvement)
        Dim courante = Selectionner(cleSource)
        If propositionExaminee Is Nothing OrElse Not Object.ReferenceEquals(courante, propositionExaminee) Then
            Throw New ArgumentException("Examen de la version courante obligatoire.", NameOf(propositionExaminee))
        End If
        examens(cleSource) = courante
    End Sub

    ''' <summary>Retire explicitement l'examen confirme de la version courante.
    ''' Aucun changement de proposition, d'audit ni d'enregistrement.</summary>
    Public Function AnnulerExamen(cleSource As String, propositionExaminee As PropositionMouvement) As Boolean
        Dim courante = Selectionner(cleSource)
        If propositionExaminee Is Nothing OrElse Not Object.ReferenceEquals(courante, propositionExaminee) Then
            Throw New ArgumentException("Annulation de la version courante obligatoire.", NameOf(propositionExaminee))
        End If
        Return examens.Remove(cleSource)
    End Function

    Private Function Selectionner(cleSource As String) As PropositionMouvement
        Dim proposition As PropositionMouvement = Nothing
        If cleSource Is Nothing OrElse Not courantes.TryGetValue(cleSource, proposition) Then
            Throw New ArgumentException("Selection source exacte obligatoire.", NameOf(cleSource))
        End If
        Return proposition
    End Function

    ''' <summary>Nothing signifie annulation. Un resultat de PropositionCorrection doit
    ''' porter sur l'instance courante exacte : une correction ancienne est refusee.
    ''' Meme sans changement de valeur, une correction conserve sa nouvelle instance et son audit.</summary>
    Public Sub AppliquerCorrection(cleSource As String, correction As PropositionCorrection.Resultat)
        Dim actuelle = Selectionner(cleSource)
        If correction Is Nothing Then Return
        If Not Object.ReferenceEquals(actuelle, correction.Avant) OrElse
            Not String.Equals(cleSource, correction.Proposition.CleSource, StringComparison.Ordinal) OrElse
            correction.Proposition.Etat <> PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine Then
            Throw New ArgumentException("Correction incompatible avec la selection courante.", NameOf(correction))
        End If
        courantes(cleSource) = correction.Proposition
        examens.Remove(cleSource) ' Toute correction rend caduc l'examen de la version precedente.
        historique.Add(correction)
    End Sub

    ''' <summary>Diagnostic uniquement ; ne modifie ni la proposition ni l'audit.
    ''' L'identifiant de document est une information explicite de l'appelant, jamais recherchee.</summary>
    Public Function Prevalider(cleSource As String, idDocumentSource As Integer,
                              identitesUtilisees As IReadOnlyCollection(Of String),
                              Optional referenceAmbigue As Boolean = True) As PropositionPrevalidation.Resultat
        Return PropositionPrevalidation.Evaluer(Selectionner(cleSource), idDocumentSource,
                                                identitesUtilisees, referenceAmbigue)
    End Function

    Public Sub MarquerApprouvee(cleSource As String)
        Selectionner(cleSource)
        Throw New InvalidOperationException("Une session de revue ne peut pas approuver une proposition.")
    End Sub

    Public Sub MarquerPersistee(cleSource As String)
        Selectionner(cleSource)
        Throw New InvalidOperationException("Une session de revue ne peut pas persister une proposition.")
    End Sub
End Class
