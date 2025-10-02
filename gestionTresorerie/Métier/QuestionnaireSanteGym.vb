Public Class QuestionnaireSanteGym
    Implements ITypeDoc
    Private _nom As String
    Private _prenom As String
    Private _nomUsage As String
    Private _dateNaissance As Date

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String, nomUsage As String, dateNaissance As Date)
        _nom = nom
        _prenom = prenom
        _nomUsage = nomUsage
        _dateNaissance = dateNaissance
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

    Public Property Prompt As String Implements ITypeDoc.Prompt
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property GabaritRepertoire As String Implements ITypeDoc.GabaritRepertoire
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property GabaritNomFichier As String Implements ITypeDoc.GabaritNomFichier
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property classe As String Implements ITypeDoc.classe
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property
End Class