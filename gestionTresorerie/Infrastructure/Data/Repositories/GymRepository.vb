Imports System.Data.SqlClient

Public Class GymRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function MettreAJourDateMajLicence(idTiers As Integer, dateMaj As Date) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "updDateMajLicence",
            New List(Of SqlParameter) From {
                New SqlParameter("@IdTiers", idTiers),
                New SqlParameter("@DateMajLicence", dateMaj)
            })
    End Function

End Class