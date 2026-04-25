Imports System.Data.SqlClient

Public Class ChequeRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then
            Throw New ArgumentNullException(NameOf(executor))
        End If

        _executor = executor
    End Sub

    Public Function LireImageCheque(idDoc As Integer) As Byte()
        Dim result As Object =
            _executor.ExecuteNamedScalar(Of Object)(
                "reqImagesChq",
                New List(Of SqlParameter) From {
                    New SqlParameter("@Id", idDoc)
                })

        If result Is Nothing OrElse result Is DBNull.Value Then
            Return Nothing
        End If

        If TypeOf result Is Byte() Then
            Return DirectCast(result, Byte())
        End If

        Return Nothing
    End Function

End Class