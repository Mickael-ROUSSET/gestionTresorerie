Imports System.Data.SqlClient

Public Interface ISqlExecutor

    Function ExecuteNonQuery(sql As String,
                             parameters As IEnumerable(Of SqlParameter)) As Integer

    Function ExecuteScalar(Of T)(sql As String,
                                 parameters As IEnumerable(Of SqlParameter)) As T

    Function ExecuteReader(Of T)(sql As String,
                                 parameters As IEnumerable(Of SqlParameter),
                                 mapper As Func(Of SqlDataReader, T)) As List(Of T)

    Function ExecuteNamedNonQuery(queryName As String,
                                  parameters As IEnumerable(Of SqlParameter)) As Integer

    Function ExecuteNamedScalar(Of T)(queryName As String,
                                      parameters As IEnumerable(Of SqlParameter)) As T

    Function ExecuteNamedReader(Of T)(queryName As String,
                                      parameters As IEnumerable(Of SqlParameter),
                                      mapper As Func(Of SqlDataReader, T)) As List(Of T)

End Interface