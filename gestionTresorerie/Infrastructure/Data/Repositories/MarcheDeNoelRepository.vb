Imports System.Data.SqlClient

Public Class MarcheDeNoelRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function InsererParticipant(parametres As Dictionary(Of String, Object)) As Integer
        Return _executor.ExecuteNamedNonQuery("insertParticipantMdN", ConvertParameters(parametres))
    End Function

    Public Function ChargerParticipants() As List(Of Participant)
        Return _executor.ExecuteNamedReader(
            "selParticipant",
            Nothing,
            Function(reader As SqlDataReader)
                Return New Participant(reader)
            End Function)
    End Function

    Private Shared Function ConvertParameters(parametres As Dictionary(Of String, Object)) As List(Of SqlParameter)
        Dim result As New List(Of SqlParameter)

        If parametres Is Nothing Then
            Return result
        End If

        For Each kvp In parametres
            result.Add(New SqlParameter(kvp.Key, If(kvp.Value, DBNull.Value)))
        Next

        Return result
    End Function

End Class