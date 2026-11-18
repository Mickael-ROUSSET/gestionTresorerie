Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Proposition en mémoire uniquement, sans écriture ni approbation automatique.
''' La clé source identifie le document ; son unicité est assurée par l'appelant.
''' </summary>
Public NotInheritable Class PropositionMouvement
    Public Enum SensMouvement
        Debit = 0
        Credit = 1
    End Enum

    Public Enum EtatProposition
        EnAttenteDeRevueHumaine = 0
    End Enum

    Public Sub New(cleSource As String,
                   typeDocument As String,
                   dateProposee As DateTime?,
                   montant As Decimal,
                   sens As SensMouvement,
                   libelle As String,
                   Optional reference As String = Nothing,
                   Optional avertissementsValidation As IEnumerable(Of String) = Nothing)
        If String.IsNullOrWhiteSpace(cleSource) Then
            Throw New ArgumentException("Une identité source est obligatoire.", NameOf(cleSource))
        End If
        If String.IsNullOrWhiteSpace(typeDocument) Then
            Throw New ArgumentException("Le type de document est obligatoire.", NameOf(typeDocument))
        End If
        If Not dateProposee.HasValue OrElse dateProposee.Value = DateTime.MinValue Then
            Throw New ArgumentException("Une date proposée est obligatoire.", NameOf(dateProposee))
        End If
        If montant <= 0D Then
            Throw New ArgumentOutOfRangeException(NameOf(montant), "Le montant doit être strictement positif.")
        End If
        If sens <> SensMouvement.Debit AndAlso sens <> SensMouvement.Credit Then
            Throw New ArgumentOutOfRangeException(NameOf(sens))
        End If
        If String.IsNullOrWhiteSpace(libelle) Then
            Throw New ArgumentException("Le libellé est obligatoire.", NameOf(libelle))
        End If

        Me.CleSource = cleSource
        Me.TypeDocument = typeDocument
        Me.DateProposee = dateProposee.Value
        Me.Montant = montant
        Me.Sens = sens
        Me.Libelle = libelle
        Me.Reference = reference
        Dim avertissements As New List(Of String)()
        If avertissementsValidation IsNot Nothing Then
            avertissements.AddRange(avertissementsValidation)
        End If
        Me.AvertissementsValidation = avertissements.AsReadOnly()
    End Sub

    Public ReadOnly Property CleSource As String
    Public ReadOnly Property TypeDocument As String
    Public ReadOnly Property DateProposee As DateTime
    Public ReadOnly Property Montant As Decimal
    Public ReadOnly Property Sens As SensMouvement
    Public ReadOnly Property Libelle As String
    Public ReadOnly Property Reference As String
    Public ReadOnly Property AvertissementsValidation As IReadOnlyList(Of String)

    Public ReadOnly Property Etat As EtatProposition
        Get
            Return EtatProposition.EnAttenteDeRevueHumaine
        End Get
    End Property
End Class
