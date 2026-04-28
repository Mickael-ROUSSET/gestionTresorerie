Imports System.Data.SqlClient

Public Class SqlCommandBuilder

    Private Sub New()
    End Sub

    Private Shared Function BuildExecutor(sBase As String) As ISqlExecutor
        Dim connectionString As String = ConnexionDB.GetInstance(sBase).GetConnexion(sBase).ConnectionString
        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()

        Return New SqlExecutor(factory, provider)
    End Function

    Public Shared Function ExecuteDataTable(sBase As String,
                                            nomRequete As String,
                                            Optional params As Dictionary(Of String, Object) = Nothing) As DataTable
        Dim dt As New DataTable()
        Dim sqlTextProvider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim sql As String = sqlTextProvider.GetSql(nomRequete)

        Dim connectionString As String = ConnexionDB.GetInstance(sBase).GetConnexion(sBase).ConnectionString

        Using conn As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(sql, conn)
                AddDictionaryParameters(cmd, params)

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        Return dt
    End Function

    Public Shared Function GetEntities(Of T As {BaseDataRow, New})(sBase As String,
                                                                   nomRequete As String,
                                                                   Optional params As Dictionary(Of String, Object) = Nothing) As List(Of T)
        Try
            Dim dt = ExecuteDataTable(sBase, nomRequete, params)
            Return DataRowUtils.FromDataTableGeneric(Of T)(dt)
        Catch ex As Exception
            Logger.ERR(String.Format("Erreur lors du chargement des entités {0} depuis {1} : {2}",
                                     GetType(T).Name,
                                     nomRequete,
                                     ex.Message))
            Return New List(Of T)()
        End Try
    End Function

    Private Shared Sub AddDictionaryParameters(cmd As SqlCommand, params As Dictionary(Of String, Object))
        If params Is Nothing Then
            Exit Sub
        End If

        For Each param In params
            cmd.Parameters.AddWithValue(param.Key, If(param.Value, DBNull.Value))
        Next
    End Sub

    <Obsolete("Utiliser SqlExecutor ou un Repository dédié.", False)>
    Public Shared Function CreateSqlCommand(sBase As String,
                                        queryName As String,
                                        Optional params As Dictionary(Of String, Object) = Nothing) As SqlCommand

        Dim sqlTextProvider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim sql As String = sqlTextProvider.GetSql(queryName)

        Dim connectionString As String = ConnexionDB.GetInstance(sBase).GetConnexion(sBase).ConnectionString
        Dim conn As New SqlConnection(connectionString)

        Dim cmd As New SqlCommand(sql, conn)

        If params IsNot Nothing Then
            For Each p In params
                cmd.Parameters.AddWithValue(p.Key, If(p.Value, DBNull.Value))
            Next
        End If

        Return cmd
    End Function
End Class