Public Class FactureRecue
    Private _nomSociete As String
    Private _reference As String
    Private _dateFacture As Date
    Private _montant As Decimal

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nomSociete As String, reference As String, dateFacture As Date, montant As Decimal)
        _nomSociete = nomSociete
        _reference = reference
        _dateFacture = dateFacture
        _montant = montant
    End Sub

    ' Propriétés
    Public Property NomSociete As String
        Get
            Return _nomSociete
        End Get
        Set(value As String)
            _nomSociete = value
        End Set
    End Property

    Public Property Reference As String
        Get
            Return _reference
        End Get
        Set(value As String)
            _reference = value
        End Set
    End Property

    Public Property dateFacture As Date
        Get
            Return _dateFacture
        End Get
        Set(value As Date)
            _dateFacture = value
        End Set
    End Property

    Public Property Montant As Decimal
        Get
            Return _montant
        End Get
        Set(value As Decimal)
            _montant = value
        End Set
    End Property
End Class