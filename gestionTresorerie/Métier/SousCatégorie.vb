Imports System.Data
Imports System.Data.SqlClient

Public Class SousCategorie
    Implements IDataService

    Public Function ExecuterRequete(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As DataTable Implements IDataService.ExecuterRequete
        Dim dt As New DataTable
        Try
            Dim conn As SqlConnection = ConnexionDB.GetInstance.getConnexion
            If conn.State <> ConnectionState.Open Then
                conn.Open()
            End If

            Using command As New SqlCommand(query, conn)
                If parameters IsNot Nothing Then
                    For Each param In parameters
                        command.Parameters.AddWithValue(param.Key, param.Value)
                    Next
                End If

                Using adpt As New SqlDataAdapter(command)
                    adpt.Fill(dt)
                End Using
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
