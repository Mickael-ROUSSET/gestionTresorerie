Public Class Adresse
    ' Propriétés (correspondant aux colonnes de la table [Adresses])
    Public Property Id As Integer
    Public Property IdTiers As Integer
    Public Property TypeAdresse As String
    Public Property Rue1 As String
    Public Property Rue2 As String
    Public Property CodePostal As String
    Public Property Ville As String
    Public Property Pays As String
    Public Property EstPrincipale As Boolean

    Public Sub New()
    End Sub

End Class