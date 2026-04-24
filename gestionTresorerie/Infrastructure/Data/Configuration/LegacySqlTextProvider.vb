Public Class LegacySqlTextProvider
    Implements ISqlTextProvider

    Public Function GetSql(queryName As String) As String Implements ISqlTextProvider.GetSql
        If String.IsNullOrWhiteSpace(queryName) Then
            Throw New ArgumentException("Le nom de requête ne peut pas être vide.", NameOf(queryName))
        End If

        Dim sql As String = LectureProprietes.GetVariable(queryName)

        If String.IsNullOrWhiteSpace(sql) Then
            Throw New InvalidOperationException(
                String.Format("Aucune requête SQL trouvée pour la clé '{0}'.", queryName))
        End If

        Return sql
    End Function
End Class