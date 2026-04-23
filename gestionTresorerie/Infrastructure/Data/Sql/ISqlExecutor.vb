Imports System.Data.SqlClient

Public Interface ISqlExecutor

    Function ExecuteNonQuery(sql As String,
                             parameters As IEnumerable(Of SqlParameter)) As Integer

    Function ExecuteScalar(Of T)(sql As String,
                                 parameters As IEnumerable(Of SqlParameter)) As T

    Function ExecuteReader(Of T)(sql As String,
                                 parameters As IEnumerable(Of SqlParameter),
                                 mapper As Func(Of SqlDataReader, T)) As List(Of T)

End Interface