Public Class ConnectionStringProvider

    Public Shared Function GetConnectionString(sBase As String) As String
        If String.IsNullOrWhiteSpace(sBase) Then
            Throw New ArgumentException("Le nom logique de base est obligatoire.", NameOf(sBase))
        End If

        Dim cs As String = LectureProprietes.connexionString(sBase)

        If String.IsNullOrWhiteSpace(cs) Then
            Throw New InvalidOperationException($"Chaîne de connexion vide pour la base '{sBase}'.")
        End If

        Return cs
    End Function

End Class