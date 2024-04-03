Imports System.Runtime.InteropServices.JavaScript
Imports DocumentFormat.OpenXml.Wordprocessing
Imports System.Text.RegularExpressions
Imports System
Imports DocumentFormat.OpenXml.Office.Word
Imports DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing
Imports System.Data.SqlClient
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
    Private _numeroRemise As String
    Public Sub New(ByVal note As String, ByVal categorie As String, ByVal sousCategorie As String, ByVal tiers As String, ByVal dateMvt As Date, ByVal montant As String, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As String)
        ' Set the property value.
        If VerifParam(note, categorie, sousCategorie, tiers, dateMvt, montant, sens, etat, événement, type, modifiable, numeroRemise) Then
            Me.Note = note
            Me.Categorie = categorie
            Me.SousCategorie = sousCategorie
            Me.Tiers = tiers
            Me.DateMvt = dateMvt
            Me.DateCréation = Date.Now
            Me.Montant = montant
            Me.Sens = sens
            Me.Etat = etat
            Me.Événement = événement
            Me.Type = type
            Me.Modifiable = modifiable
            Me.NumeroRemise = numeroRemise
        End If
    End Sub
    Public Shared Function existe(ByVal dateMvt As Date, ByVal montant As String, ByVal sens As String) As Boolean
        ' Vérifie si le mouvement existe déjà
        Dim bExiste As Boolean = False
        Dim monMvt As SqlCommand
        Dim myReader As SqlDataReader
        Dim sDate As String

        sDate = dateMvt.Year.ToString & "-" & dateMvt.Month.ToString & "-" & dateMvt.Day.ToString
        monMvt = New SqlCommand("SELECT count(*) FROM Mouvements where dateMvt = '" & sDate & "' and montant = '" & montant & "' and sens = '" & sens & "';", FrmPrincipale.myConn)
        myReader = monMvt.ExecuteReader()
        Do While myReader.Read()
            bExiste = (myReader.GetInt32(0) > 0)
        Loop
        myReader.Close()
        Return bExiste
    End Function
    Public Shared Function VerifParam(note As String, categorie As String, sousCategorie As String, tiers As String, dateMvt As Date, montant As String, sens As String, etat As String, événement As String, type As String, modifiable As Boolean, numeroRemise As String) As Boolean
        Dim bToutEstLa As Boolean = False
        If categorie <> "" And sousCategorie <> "" And tiers <> "" And IsDate(dateMvt) And sens <> "" And etat <> "" And type <> "" Then
            bToutEstLa = True
        End If
        Return bToutEstLa
    End Function
    Public Property DateCréation() As Date
        'https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/regular-expressions
        Get
            Return _dateCréation
        End Get
        Private Set(ByVal value As Date)
            _dateCréation = value
        End Set
    End Property
    Public Property Note() As String
        'https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/regular-expressions
        Get
            Return IIf(_note > "", _note, "Null")
        End Get
        Set(ByVal value As String)
            '_note = Trim(value)
            'Dim pattern As String = "(Mr\\.? |Mrs\\.? |Miss |Ms\\.? )"
            'Dim names() As String = {"Mr. Henry Hunt", "Ms. Sara Samuels", "Abraham Adams", "Ms. Nicole Norris"}
            'Suppression des doubles quotes
            _note = Regex.Replace(value, "(.*)""(.*)", String.Empty)
        End Set
    End Property
    Public Property Categorie() As String
        Get
            Return _categorie
        End Get
        Set(ByVal value As String)
            _categorie = Trim(value)
        End Set
    End Property
    Public Property SousCategorie() As String
        Get
            Return _sousCategorie
        End Get
        Set(ByVal value As String)
            _sousCategorie = Trim(value)
        End Set
    End Property
    Public Property Tiers() As String
        Get
            Return Split(_tiers, vbTab)(0)
        End Get
        Set(ByVal value As String)
            _tiers = Trim(value)
        End Set
    End Property
    Public Property DateMvt() As Date
        Get
            Return _dateMvt
        End Get
        Set(ByVal value As Date)
            _dateMvt = value
        End Set
    End Property
    Public Property Montant() As String
        Get
            Return CDec(_montant)
        End Get
        Set(ByVal value As String)
            If Decimal.TryParse(value, vbNull) Then
                _montant = value
            Else
                _montant = "0"
            End If
        End Set
    End Property
    Public Property Sens() As String
        Get
            Return _sens
        End Get
        Set(ByVal value As String)
            _sens = Trim(value)
        End Set
    End Property
    Public Property Etat() As String
        Get
            Return _etat
        End Get
        Set(ByVal value As String)
            _etat = value
        End Set
    End Property
    Public Property Événement() As String
        Get
            Return _événement
        End Get
        Set(ByVal value As String)
            _événement = Trim(value)
        End Set
    End Property
    Public Property Type() As String
        Get
            Return _type
        End Get
        Set(ByVal value As String)
            _type = Trim(value)
        End Set
    End Property
    Public Property Modifiable() As Boolean
        Get
            Return _modifiable
        End Get
        Set(ByVal value As Boolean)
            _modifiable = value
        End Set
    End Property
    Public Property NumeroRemise() As String
        Get
            Return CInt(_numeroRemise)
        End Get
        Set(ByVal value As String)
            Dim s As String
            s = Trim(Strings.Replace(value, """", ""))
            'Dim myRegex As Regex = New Regex("(.*)""(.*)")
            'If Integer.TryParse(Trim(myRegex.Replace(value, "$1$2")), vbNull) Then
            If Integer.TryParse(Trim(Strings.Replace(value, """", "")), vbNull) Then
                _numeroRemise = Trim(Strings.Replace(value, """", ""))
            Else
                _numeroRemise = "0"
            End If
        End Set
    End Property
End Class
