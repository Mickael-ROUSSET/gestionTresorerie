Public Class AdhesionGym
    Private _nom As String
    Private _prenom As String
    Private _nomUsage As String
    Private _dateNaissance As Date
    Private _email As String
    Private _telephone As String
    Private _adresse As String
    Private _ville As String
    Private _codePostal As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String, nomUsage As String, dateNaissance As Date, email As String, telephone As String, adresse As String, ville As String, codePostal As String)
        _nom = nom
        _prenom = prenom
        _nomUsage = nomUsage
        _dateNaissance = dateNaissance
        _email = email
        _telephone = telephone
        _adresse = adresse
        _ville = ville
        _codePostal = codePostal
    End Sub

    ' Propriétés
    Public Property Nom As String
        Get
            Return _nom
        End Get
        Set(value As String)
            _nom = value
        End Set
    End Property

    Public Property Prenom As String
        Get
            Return _prenom
        End Get
        Set(value As String)
            _prenom = value
        End Set
    End Property

    Public Property NomUsage As String
        Get
            Return _nomUsage
        End Get
        Set(value As String)
            _nomUsage = value
        End Set
    End Property

    Public Property DateNaissance As Date
        Get
            Return _dateNaissance
        End Get
        Set(value As Date)
            _dateNaissance = value
        End Set
    End Property

    Public Property Email As String
        Get
            Return _email
        End Get
        Set(value As String)
            _email = value
        End Set
    End Property

    Public Property Telephone As String
        Get
            Return _telephone
        End Get
        Set(value As String)
            _telephone = value
        End Set
    End Property

    Public Property Adresse As String
        Get
            Return _adresse
        End Get
        Set(value As String)
            _adresse = value
        End Set
    End Property

    Public Property Ville As String
        Get
            Return _ville
        End Get
        Set(value As String)
            _ville = value
        End Set
    End Property

    Public Property CodePostal As String
        Get
            Return _codePostal
        End Get
        Set(value As String)
            _codePostal = value
        End Set
    End Property
End Class