Public Class ConnectionStringProvider

    Public Shared Function GetConnectionString(sBase As String) As String
        If String.IsNullOrWhiteSpace(sBase) Then
            Throw New ArgumentException("Le nom logique de base est obligatoire.", NameOf(sBase))
        End If

        Dim connexion = ConnexionDB.GetInstance(sBase).GetConnexion(sBase)

        If connexion Is Nothing Then
            Throw New InvalidOperationException($"Connexion indisponible pour la base '{sBase}'.")
        End If

        If String.IsNullOrWhiteSpace(connexion.ConnectionString) Then
            Throw New InvalidOperationException($"Chaîne de connexion vide pour la base '{sBase}'.")
        End If

        Return connexion.ConnectionString
    End Function

End Class