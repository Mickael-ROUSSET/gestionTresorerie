Public Class StatFilm
    Public Property IdFilm As Integer
    Public Property Titre As String
    Public Property Seances As New List(Of Seance)

    ' Calcul du CA total pour le film
    Public ReadOnly Property CA_Total As Decimal
        Get
            Return Seances.Sum(Function(s) s.CA_Total)
        End Get
    End Property

    Public ReadOnly Property CA_Adultes As Decimal
        Get
            Return Seances.Sum(Function(s) s.CA_Adultes)
        End Get
    End Property

    Public ReadOnly Property CA_Enfants As Decimal
        Get
            Return Seances.Sum(Function(s) s.CA_Enfants)
        End Get
    End Property

    Public ReadOnly Property CA_GroupeEnfants As Decimal
        Get
            Return Seances.Sum(Function(s) s.CA_GroupeEnfants)
        End Get
    End Property

    Public ReadOnly Property NbSeances As Integer
        Get
            Return Seances.Count
        End Get
    End Property

    Public ReadOnly Property TotalAdultes As Integer
        Get
            Return Seances.Sum(Function(s) s.NbEntreesAdultes)
        End Get
    End Property

    Public ReadOnly Property TotalEnfants As Integer
        Get
            Return Seances.Sum(Function(s) s.NbEntreesEnfants)
        End Get
    End Property

    Public ReadOnly Property TotalGroupeEnfants As Integer
        Get
            Return Seances.Sum(Function(s) s.NbEntreesGroupeEnfants)
        End Get
    End Property

    ' Méthode Clone pour filtrage
    Public Function Clone() As StatFilm
        Dim copie As New StatFilm With {
            .IdFilm = Me.IdFilm,
            .Titre = Me.Titre
        }
        copie.Seances = Me.Seances.Select(Function(s) s.Clone()).ToList()
        Return copie
    End Function
End Class
