Imports System.Runtime.InteropServices.JavaScript

Public Class Mouvements
    Private _note As String
    Private _categorie As String
    Private _sousCategorie As String
    Private _tiers As String
    Private _dateCréation As Date
    Private _dateMvt As Date
    Private _montant As Decimal
    Private _sens As String
    Private _etat As String
    Private _événement As String
    Private _type As String
    Private _modifiable As Boolean
    Private _numeroRemise As Integer

    Public Property note() As String
        Get
            Return _note
        End Get
        Set(ByVal value As String)
            _note = value
        End Set
    End Property
    Public Property categorie() As String
        Get
            Return _categorie
        End Get
        Set(ByVal value As String)
            _categorie = value
        End Set
    End Property
    Public Property sousCategorie() As String
        Get
            Return _sousCategorie
        End Get
        Set(ByVal value As String)
            _sousCategorie = value
        End Set
    End Property
    Public Property tiers() As String
        Get
            Return _tiers
        End Get
        Set(ByVal value As String)
            _tiers = value
        End Set
    End Property
    Public Property dateCréation() As Date
        Get
            Return _dateCréation
        End Get
        Set(ByVal value As Date)
            _dateCréation = value
        End Set
    End Property
    Public Property dateMvt() As Date
        Get
            Return _dateMvt
        End Get
        Set(ByVal value As Date)
            _dateMvt = value
        End Set
    End Property
    Public Property montant() As Decimal
        Get
            Return _montant
        End Get
        Set(ByVal value As Decimal)
            _montant = value
        End Set
    End Property
    Public Property sens() As String
        Get
            Return _sens
        End Get
        Set(ByVal value As String)
            _sens = value
        End Set
    End Property
    Public Property etat() As String
        Get
            Return _etat
        End Get
        Set(ByVal value As String)
            _etat = value
        End Set
    End Property
    Public Property événement() As String
        Get
            Return _événement
        End Get
        Set(ByVal value As String)
            _événement = value
        End Set
    End Property
    Public Property type() As String
        Get
            Return _type
        End Get
        Set(ByVal value As String)
            _type = value
        End Set
    End Property
    Public Property modifiable() As Boolean
        Get
            Return _modifiable
        End Get
        Set(ByVal value As Boolean)
            _modifiable = value
        End Set
    End Property
    Public Property numeroRemise() As Integer
        Get
            Return _numeroRemise
        End Get
        Set(ByVal value As Integer)
            _numeroRemise = value
        End Set
    End Property
End Class
