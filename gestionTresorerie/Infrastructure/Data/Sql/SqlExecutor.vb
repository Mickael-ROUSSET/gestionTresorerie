Imports System.Data.SqlClient

Public Class SqlExecutor
    Implements ISqlExecutor

    Private ReadOnly _factory As IConnectionFactory

    Public Sub New(factory As IConnectionFactory)
        _factory = factory
    End Sub

    Public Function ExecuteNonQuery(sql As String,
                                   parameters As IEnumerable(Of SqlParameter)) As Integer Implements ISqlExecutor.ExecuteNonQuery
        Try
            Using conn = _factory.CreateConnection()
                Using cmd As New SqlCommand(sql, conn)
                    If parameters IsNot Nothing Then
                        cmd.Parameters.AddRange(parameters.ToArray())
                    End If

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
                    If parameters IsNot Nothing Then
                        cmd.Parameters.AddRange(parameters.ToArray())
                    End If

                    conn.Open()
                    Dim result = cmd.ExecuteScalar()

                    If result Is Nothing OrElse result Is DBNull.Value Then
                        Return Nothing
                    End If

                    Return CType(result, T)
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
                    If parameters IsNot Nothing Then
                        cmd.Parameters.AddRange(parameters.ToArray())
                    End If

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

End Class