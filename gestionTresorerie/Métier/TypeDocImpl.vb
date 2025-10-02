' Classe concrète implémentant ITypeDoc pour pouvoir instancier des objets
Public Class TypeDocImpl
    Implements ITypeDoc

    ' Champs privés
    Private _prompt As String
    Private _gabaritRepertoire As String
    Private _gabaritNomFichier As String
    Private _classe As String

    ' Constructeur avec paramètres pour initialisation depuis la base de données
    Public Sub New(prompt As String, gabaritRepertoire As String, gabaritNomFichier As String, classe As String)
        _prompt = prompt
        _gabaritRepertoire = gabaritRepertoire
        _gabaritNomFichier = gabaritNomFichier
        _classe = classe
    End Sub


    ' Implémentation des propriétés de l'interface
    Public Property Prompt As String Implements ITypeDoc.Prompt
        Get
            Return _prompt
        End Get
        Set(value As String)
            _prompt = value
        End Set
    End Property

    Public Property GabaritRepertoire As String Implements ITypeDoc.GabaritRepertoire
        Get
            Return _gabaritRepertoire
        End Get
        Set(value As String)
            _gabaritRepertoire = value
        End Set
    End Property

    Public Property GabaritNomFichier As String Implements ITypeDoc.GabaritNomFichier
        Get
            Return _gabaritNomFichier
        End Get
        Set(value As String)
            _gabaritNomFichier = value
        End Set
    End Property

    Public Property classe As String Implements ITypeDoc.classe
        Get
            Return _classe
        End Get
        Set(value As String)
            _classe = value
        End Set
    End Property
End Class
