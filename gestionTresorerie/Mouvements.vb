Imports System.Runtime.InteropServices.JavaScript
Imports DocumentFormat.OpenXml.Wordprocessing
Imports System.Text.RegularExpressions
Imports System
Imports DocumentFormat.OpenXml.Office.Word
Public Class Mouvements
    Private _note As String
    Private _categorie As String
    Private _sousCategorie As String
    Private _tiers As String
    'Private _dateCréation As Date
    Private _dateMvt As Date
    Private _montant As Decimal
    Private _sens As String
    Private _etat As String
    Private _événement As String
    Private _type As String
    Private _modifiable As Boolean
    Private _numeroRemise As Integer
    Public Sub New(ByVal note As String, ByVal categorie As String, ByVal sousCategorie As String, ByVal tiers As String, ByVal dateMvt As Date, ByVal montant As Decimal, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As Integer)
        ' Set the property value.
        Me._note = note
    End Sub
    Public Function verifParam(ByVal note As String, ByVal categorie As String, ByVal sousCategorie As String, ByVal tiers As String, dateMvt As Date, ByVal montant As Decimal, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As Integer) As Boolean
        Dim bToutEstLa As Boolean = False
        If categorie <> "" And sousCategorie <> "" And tiers <> "" And IsDate(dateMvt) And sens <> "" And etat <> "" And type <> "" Then
            bToutEstLa = True
        End If
        Return bToutEstLa
    End Function
    Public Property note() As String
        'https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/regular-expressions
        Get
            Return _note
        End Get
        Set(ByVal value As String)
            '_note = Trim(value)
            'Dim pattern As String = "(Mr\\.? |Mrs\\.? |Miss |Ms\\.? )"
            'Dim names() As String = {"Mr. Henry Hunt", "Ms. Sara Samuels", "Abraham Adams", "Ms. Nicole Norris"}
            'Suppression des doubles quotes
            _note = Regex.Replace(value, "(.*)""(.*)", String.Empty)
        End Set
    End Property
    Public Property categorie() As String
        Get
            Return _categorie
        End Get
        Set(ByVal value As String)
            _categorie = Trim(value)
        End Set
    End Property
    Public Property sousCategorie() As String
        Get
            Return _sousCategorie
        End Get
        Set(ByVal value As String)
            _sousCategorie = Trim(value)
        End Set
    End Property
    Public Property tiers() As String
        Get
            Return _tiers
        End Get
        Set(ByVal value As String)
            _tiers = Trim(value)
        End Set
    End Property
    'Private Property dateCréation() As Date
    '    Get
    '        Return _dateCréation
    '    End Get
    '    Set(ByVal value As Date)
    '        _dateCréation = value
    '    End Set
    'End Property
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
            _sens = Trim(value)
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
            _événement = Trim(value)
        End Set
    End Property
    Public Property type() As String
        Get
            Return _type
        End Get
        Set(ByVal value As String)
            _type = Trim(value)
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
            If IsNumeric(value) Then
                _numeroRemise = CInt(value)
            Else
                _numeroRemise = 0
            End If
        End Set
    End Property
End Class
