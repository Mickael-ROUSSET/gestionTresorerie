Public Class Categorie
    'Public Function ExecuterRequete(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As DataTable Implements IDataService.ExecuterRequete
    '    Dim dt As New DataTable
    '    Try
    '        Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand(query, parameters)
    '        Using adpt As New SqlDataAdapter(command)
    '            adpt.Fill(dt)
    '        End Using

    '        Logger.INFO($"Requête exécutée avec succès : {query}")
    '    Catch ex As Exception
    '        Logger.ERR($"Erreur inattendue lors de l'exécution de la requête. Message: {ex.Message}")
    '        Throw
    '    End Try
    '    Return dt
    'End Function
    Public Shared Function libelleParId(Id As Integer) As String
        Dim sLib As String
        Try
            sLib = SqlCommandBuilder.
            CreateSqlCommand("reqLibCat",
                             New Dictionary(Of String, Object) From {{"@Id", Id}}
                             ).
                             ExecuteScalar

            Logger.INFO($"Requête exécutée avec succès : reqLibCat pour le param {Id} => {sLib}")
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de l'exécution de la requête. Message: {ex.Message}")
            Throw
        End Try
        Return sLib
    End Function
End Class

