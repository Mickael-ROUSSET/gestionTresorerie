Imports System.Data.SqlClient

Public Class CategorieRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LibelleParId(id As Integer) As String
        Dim result As Object =
            _executor.ExecuteNamedScalar(Of Object)(
                "reqLibCat",
                New List(Of SqlParameter) From {
                    New SqlParameter("@Id", id)
                })

        If result Is Nothing OrElse result Is DBNull.Value Then
            Return String.Empty
        End If

        Return result.ToString()
    End Function

End Class