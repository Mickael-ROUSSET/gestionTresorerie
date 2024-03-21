Public Class ClsTiers

    Dim _raisonSociale As String
    Dim _nom As String
    Dim _prénom As String
    Dim _categorieDefaut As String
    Dim _sousCategorieDefaut As String
    Public Property RaisonSociale As String
        Get
            Return _raisonSociale
        End Get
        Set(value As String)
            _raisonSociale = value
        End Set
    End Property

    Public Property Nom As String
        Get
            Return _nom
        End Get
        Set(value As String)
            _nom = value
        End Set
    End Property

    Public Property Prénom As String
        Get
            Return _prénom
        End Get
        Set(value As String)
            _prénom = value
        End Set
    End Property

    Public Property CategorieDefaut As String
        Get
            Return _categorieDefaut
        End Get
        Set(value As String)
            _categorieDefaut = value
        End Set
    End Property

    Public Property SousCategorieDefaut As String
        Get
            Return _sousCategorieDefaut
        End Get
        Set(value As String)
            _sousCategorieDefaut = value
        End Set
    End Property
End Class
