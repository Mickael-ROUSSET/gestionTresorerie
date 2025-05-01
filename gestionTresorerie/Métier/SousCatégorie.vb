Imports System.Data
Imports System.Data.SqlClient

Public Class SousCategorie
    Implements IDataService

    Public Function ExecuterRequete(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As DataTable Implements IDataService.ExecuterRequete
        Dim dt As New DataTable
        Try
            Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand(query, parameters)
            Using adpt As New SqlDataAdapter(command)
                adpt.Fill(dt)
            End Using

            Logger.INFO($"Requête exécutée avec succès : {query}")
        Catch ex As SqlException
            Logger.ERR($"Erreur SQL lors de l'exécution de la requête. Message: {ex.Message}")
            Throw
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de l'exécution de la requête. Message: {ex.Message}")
            Throw
        End Try
        Return dt
    End Function
End Class
