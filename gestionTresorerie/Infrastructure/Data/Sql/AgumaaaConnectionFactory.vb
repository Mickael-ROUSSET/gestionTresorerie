Imports System.Data.SqlClient

Public Class AgumaaaConnectionFactory
    Implements IConnectionFactory

    Private ReadOnly _connectionString As String

    Public Sub New(connectionString As String)
        If String.IsNullOrWhiteSpace(connectionString) Then
            Throw New ArgumentException("La chaîne de connexion ne peut pas être vide.")
        End If

        _connectionString = connectionString
    End Sub

    Public Function CreateConnection() As SqlConnection Implements IConnectionFactory.CreateConnection
        Return New SqlConnection(_connectionString)
    End Function
End Class