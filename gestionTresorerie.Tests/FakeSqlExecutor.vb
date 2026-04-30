Imports System.Data.SqlClient

Public Class FakeSqlExecutor
    Implements ISqlExecutor

    Public Property LastQueryName As String
    Public Property LastParameters As IEnumerable(Of SqlParameter)
    Public Property ScalarResult As Object
    Public Property NonQueryResult As Integer = 1

    Public Function ExecuteNonQuery(sql As String, parameters As IEnumerable(Of SqlParameter)) As Integer Implements ISqlExecutor.ExecuteNonQuery
        LastQueryName = sql
        LastParameters = parameters
        Return NonQueryResult
    End Function

    Public Function ExecuteScalar(Of T)(sql As String, parameters As IEnumerable(Of SqlParameter)) As T Implements ISqlExecutor.ExecuteScalar
        LastQueryName = sql
        LastParameters = parameters
        Return CType(ScalarResult, T)
    End Function

    Public Function ExecuteReader(Of T)(sql As String, parameters As IEnumerable(Of SqlParameter), mapper As Func(Of SqlDataReader, T)) As List(Of T) Implements ISqlExecutor.ExecuteReader
        Throw New NotImplementedException()
    End Function

    Public Function ExecuteNamedNonQuery(queryName As String, parameters As IEnumerable(Of SqlParameter)) As Integer Implements ISqlExecutor.ExecuteNamedNonQuery
        LastQueryName = queryName
        LastParameters = parameters
        Return NonQueryResult
    End Function

    Public Function ExecuteNamedScalar(Of T)(queryName As String, parameters As IEnumerable(Of SqlParameter)) As T Implements ISqlExecutor.ExecuteNamedScalar
        LastQueryName = queryName
        LastParameters = parameters
        Return CType(ScalarResult, T)
    End Function

    Public Function ExecuteNamedReader(Of T)(queryName As String, parameters As IEnumerable(Of SqlParameter), mapper As Func(Of SqlDataReader, T)) As List(Of T) Implements ISqlExecutor.ExecuteNamedReader
        Throw New NotImplementedException()
    End Function
End Class