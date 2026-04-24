Imports System.Data.SqlClient
Imports System.Linq

Public Class SqlExecutor
    Implements ISqlExecutor

    Private ReadOnly _factory As IConnectionFactory
    Private ReadOnly _sqlTextProvider As ISqlTextProvider

    Public Sub New(factory As IConnectionFactory, Optional sqlTextProvider As ISqlTextProvider = Nothing)
        If factory Is Nothing Then
            Throw New ArgumentNullException(NameOf(factory))
        End If

        _factory = factory
        _sqlTextProvider = sqlTextProvider
    End Sub

    Public Function ExecuteNonQuery(sql As String,
                                    parameters As IEnumerable(Of SqlParameter)) As Integer Implements ISqlExecutor.ExecuteNonQuery
        Try
            Using conn = _factory.CreateConnection()
                Using cmd As New SqlCommand(sql, conn)
                    AddParameters(cmd, parameters)
                    conn.Open()
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw New DataAccessException("Erreur ExecuteNonQuery", ex)
        End Try
    End Function

    Public Function ExecuteScalar(Of T)(sql As String,
                                        parameters As IEnumerable(Of SqlParameter)) As T Implements ISqlExecutor.ExecuteScalar
        Try
            Using conn = _factory.CreateConnection()
                Using cmd As New SqlCommand(sql, conn)
                    AddParameters(cmd, parameters)
                    conn.Open()

                    Dim result = cmd.ExecuteScalar()

                    If result Is Nothing OrElse result Is DBNull.Value Then
                        Return Nothing
                    End If

                    Return CType(Convert.ChangeType(result, GetType(T)), T)
                End Using
            End Using
        Catch ex As Exception
            Throw New DataAccessException("Erreur ExecuteScalar", ex)
        End Try
    End Function

    Public Function ExecuteReader(Of T)(sql As String,
                                        parameters As IEnumerable(Of SqlParameter),
                                        mapper As Func(Of SqlDataReader, T)) As List(Of T) Implements ISqlExecutor.ExecuteReader
        Dim result As New List(Of T)

        Try
            Using conn = _factory.CreateConnection()
                Using cmd As New SqlCommand(sql, conn)
                    AddParameters(cmd, parameters)
                    conn.Open()

                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(mapper(reader))
                        End While
                    End Using
                End Using
            End Using

            Return result
        Catch ex As Exception
            Throw New DataAccessException("Erreur ExecuteReader", ex)
        End Try
    End Function

    Public Function ExecuteNamedNonQuery(queryName As String,
                                         parameters As IEnumerable(Of SqlParameter)) As Integer Implements ISqlExecutor.ExecuteNamedNonQuery
        Return ExecuteNonQuery(GetRequiredSql(queryName), parameters)
    End Function

    Public Function ExecuteNamedScalar(Of T)(queryName As String,
                                             parameters As IEnumerable(Of SqlParameter)) As T Implements ISqlExecutor.ExecuteNamedScalar
        Return ExecuteScalar(Of T)(GetRequiredSql(queryName), parameters)
    End Function

    Public Function ExecuteNamedReader(Of T)(queryName As String,
                                             parameters As IEnumerable(Of SqlParameter),
                                             mapper As Func(Of SqlDataReader, T)) As List(Of T) Implements ISqlExecutor.ExecuteNamedReader
        Return ExecuteReader(Of T)(GetRequiredSql(queryName), parameters, mapper)
    End Function

    Private Function GetRequiredSql(queryName As String) As String
        If _sqlTextProvider Is Nothing Then
            Throw New InvalidOperationException("Aucun provider SQL nommé n'a été fourni au SqlExecutor.")
        End If

        Return _sqlTextProvider.GetSql(queryName)
    End Function

    Private Shared Sub AddParameters(cmd As SqlCommand, parameters As IEnumerable(Of SqlParameter))
        If parameters Is Nothing Then
            Exit Sub
        End If

        For Each param As SqlParameter In parameters
            cmd.Parameters.Add(param)
        Next
    End Sub

End Class