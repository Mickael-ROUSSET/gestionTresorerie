Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Correction pure : aucune approbation, persistance ou ressource externe.</summary>
Public NotInheritable Class PropositionCorrection
    Private Sub New()
    End Sub

    Public Enum Champ
        DateProposee
        Montant
        Sens
        Libelle
        Reference
    End Enum

    ''' <summary>
    ''' L'utilisateur atteste la resolution de ce texte exact pour ce champ.
    ''' Les avertissements libres n'ont pas de correspondance automatique avec les champs :
    ''' l'appelant doit recueillir cette association explicitement, jamais la deduire.
    ''' </summary>
    Public NotInheritable Class ConfirmationAvertissement
        Public Sub New(champ As Champ, avertissement As String)
            If Not [Enum].IsDefined(GetType(Champ), champ) Then Throw New ArgumentOutOfRangeException(NameOf(champ))
            If String.IsNullOrWhiteSpace(avertissement) Then Throw New ArgumentException("Avertissement obligatoire.", NameOf(avertissement))
            Me.Champ = champ
            Me.Avertissement = avertissement
        End Sub

        Public ReadOnly Property Champ As Champ
        Public ReadOnly Property Avertissement As String
    End Class

    ''' <summary>
    ''' Audit en memoire : instantanes immuables avant/apres des cinq champs et
    ''' confirmations explicites. Une correction, meme identique, ne vaut pas approbation.
    ''' </summary>
    Public NotInheritable Class Resultat
        Friend Sub New(avant As PropositionMouvement, apres As PropositionMouvement,
                       confirmations As List(Of ConfirmationAvertissement))
            Me.Avant = avant
            Me.Proposition = apres
            Me.Confirmations = confirmations.AsReadOnly()
            Dim modifies As New List(Of Champ)()
            If avant.DateProposee <> apres.DateProposee Then modifies.Add(Champ.DateProposee)
            If avant.Montant <> apres.Montant Then modifies.Add(Champ.Montant)
            If avant.Sens <> apres.Sens Then modifies.Add(Champ.Sens)
            If Not String.Equals(avant.Libelle, apres.Libelle, StringComparison.Ordinal) Then modifies.Add(Champ.Libelle)
            If Not String.Equals(avant.Reference, apres.Reference, StringComparison.Ordinal) Then modifies.Add(Champ.Reference)
            Me.ChampsModifies = modifies.AsReadOnly()
        End Sub

        Public ReadOnly Property Avant As PropositionMouvement
        Public ReadOnly Property Proposition As PropositionMouvement
        Public ReadOnly Property Confirmations As IReadOnlyList(Of ConfirmationAvertissement)
        Public ReadOnly Property ChampsModifies As IReadOnlyList(Of Champ)
    End Class

    ''' <summary>
    ''' Tous les champs corrigibles sont fournis explicitement. Nothing efface la reference.
    ''' Aucune normalisation d'identite, de reference ou de texte d'avertissement.
    ''' Les avertissements non confirmes restent dans leur ordre initial, doublons compris.
    ''' </summary>
    Public Shared Function Corriger(proposition As PropositionMouvement,
                                    dateProposee As DateTime?, montant As Decimal,
                                    sens As PropositionMouvement.SensMouvement,
                                    libelle As String, reference As String,
                                    Optional confirmations As IEnumerable(Of ConfirmationAvertissement) = Nothing) As Resultat
        If proposition Is Nothing Then Throw New ArgumentNullException(NameOf(proposition))
        Dim audit As New List(Of ConfirmationAvertissement)()
        If confirmations IsNot Nothing Then
            For Each confirmation In confirmations
                If confirmation Is Nothing Then Throw New ArgumentException("Confirmation absente.", NameOf(confirmations))
                Dim trouve As Boolean = False
                For Each avertissement In proposition.AvertissementsValidation
                    If String.Equals(avertissement, confirmation.Avertissement, StringComparison.Ordinal) Then trouve = True
                Next
                If Not trouve Then Throw New ArgumentException("Avertissement inconnu de la proposition.", NameOf(confirmations))
                audit.Add(confirmation)
            Next
        End If

        Dim restants As New List(Of String)()
        For Each avertissement In proposition.AvertissementsValidation
            Dim confirme As Boolean = False
            For Each confirmation In audit
                If String.Equals(avertissement, confirmation.Avertissement, StringComparison.Ordinal) Then confirme = True
            Next
            If Not confirme Then restants.Add(avertissement)
        Next
        ' Le constructeur centralise la validation des valeurs et copie les avertissements.
        Dim nouvelle As New PropositionMouvement(proposition.CleSource, proposition.TypeDocument,
                                                dateProposee, montant, sens, libelle, reference, restants)
        Return New Resultat(proposition, nouvelle, audit)
    End Function
End Class
