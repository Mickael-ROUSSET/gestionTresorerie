Public Class Utilisateur
    Public Property Id As Integer
    Public Property NomUtilisateur As String
    Public Property Role As String

    ' Méthode utilitaire
    Public Function EstAdmin() As Boolean
        Return Role.Equals("admin", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function EstEcrivain() As Boolean
        Return Role.Equals("ecrivain", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function EstLecteur() As Boolean
        Return Role.Equals("lecteur", StringComparison.OrdinalIgnoreCase)
    End Function
End Class
