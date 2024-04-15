Imports System.Collections.ObjectModel
Imports System.Data.SqlClient

' Classe permettant de se connecter à une base de donnée SQLSERVER 

Public Class connexionDB
    Implements IDisposable
    Public maConnexion As SqlConnection
    Public maCommandeSql As New SqlCommand()
    Public result As String
    Public reader As SqlDataReader
    'Public sRequete As String
    'Private collRequetes As Collection(Of String)

    Public Sub New()
        getConnexion()
    End Sub
    Public Function getConnexion() As SqlConnection
        'Crée la connexion si elle n'existe pas, sinon renvoie celle existante
        If maConnexion Is Nothing Then
            maConnexion = New SqlConnection(My.Settings.DBsource)
        End If
        Return maConnexion
        'maConnexion.Open()
    End Function

    Public Function getRequete(indiceRequete As Integer) As String
        'Return collRequetes(indiceRequete)
        Return My.Settings.Requetes.Item(indiceRequete)
    End Function
    Public Sub setRequete(sRequete As String)
        'collRequetes.Add(collRequetes(indiceRequete))
        My.Settings.Requetes.Add(sRequete)
    End Sub
    Private Sub SuprimeConnexion()
        'Close the reader and the database connection. 
        maConnexion.Close()
    End Sub
    'Public Sub creeConnexion()
    '    Me.maCommandeSql.Connection = maConnexion
    '    'Me.maCommandeSql.CommandText = Me.sRequete
    '    Try
    '        Me.maConnexion.Open()
    '        Me.result = maCommandeSql.ExecuteScalar().ToString
    '    Catch ex As Exception
    '        MsgBox("Erreur : " & ex.Message)
    '    Finally
    '        Me.maCommandeSql.Dispose()
    '        Me.maConnexion.Close()
    '        Me.maConnexion.Dispose()
    '    End Try
    'End Sub

    Public Function ConnexionExecuteReader() As SqlDataReader
        Me.maCommandeSql.Connection = maConnexion
        'Me.maCommandeSql.CommandText = Me.sRequete
        Try
            Me.maConnexion.Open()
            Me.reader = maCommandeSql.ExecuteReader(CommandBehavior.CloseConnection)
        Catch ex As Exception
        Finally
        End Try
        Return reader
    End Function

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Try
            'Me.sRequete = Nothing
            Me.maConnexion.Close()
            Me.maConnexion.Dispose()
            Me.maCommandeSql.Dispose()
            Me.reader.Close()
            Me.result = Nothing
        Catch ex As Exception
        End Try
    End Sub
End Class
