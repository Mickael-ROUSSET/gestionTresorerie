Public Class AttestationNonParticipationMdN
    Implements ITypeDoc
    Private _nom As String
    Private _prenom As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String)
        _nom = nom
        _prenom = prenom
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

    Public Property ClasseTypeDoc As String Implements ITypeDoc.ClasseTypeDoc
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property ContenuBase64 As String Implements ITypeDoc.ContenuBase64
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property JsonMetaDonnées As String Implements ITypeDoc.JsonMetaDonnées
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Function ReperoireDestination() As String Implements ITypeDoc.ReperoireDestination
        Throw New NotImplementedException()
    End Function

    Public Function renommerFichier(sChemin As String) As String Implements ITypeDoc.renommerFichier
        Throw New NotImplementedException()
    End Function
End Class