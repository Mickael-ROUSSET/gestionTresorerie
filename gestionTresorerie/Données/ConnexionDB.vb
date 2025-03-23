Imports System.Collections.ObjectModel
Imports System.Data.SqlClient

' Classe permettant de se connecter à une base de donnée SQLSERVER 

Public Class connexionDB
    Implements IDisposable
    Public _maConnexion As SqlConnection
    Public _maCommandeSql As New SqlCommand()
    Public result As String
    Public reader As SqlDataReader
    'Public sRequete As String
    'Private collRequetes As Collection(Of String)

    Public Sub New()
        creeConnexion()
    End Sub
    Public Function getConnexion() As SqlConnection
        'Crée la connexion si elle n'existe pas, sinon renvoie celle existante 
        Return _maConnexion
    End Function
    Public Sub creeConnexion()
        Try
            ' Crée la connexion si elle n'existe pas, sinon renvoie celle existante
            If _maConnexion Is Nothing Then
                _maConnexion = New SqlConnection(My.Settings.DBsource)
                _maConnexion.Open()
            End If
        Catch ex As SqlException
            ' Gère les erreurs de connexion SQL
            MsgBox("creeConnexion : erreur SQL : " & ex.Message)
        Catch ex As Exception
            ' Gère toutes les autres erreurs
            MsgBox("Erreur : " & ex.Message)
        End Try
    End Sub


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
        _maConnexion.Close()
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
        Me._maCommandeSql.Connection = _maConnexion
        'Me.maCommandeSql.CommandText = Me.sRequete
        Try
            Me._maConnexion.Open()
            Me.reader = _maCommandeSql.ExecuteReader(CommandBehavior.CloseConnection)
        Catch ex As Exception
        Finally
        End Try
        Return reader
    End Function

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Try
            'Me.sRequete = Nothing
            Me._maConnexion.Close()
            Me._maConnexion.Dispose()
            Me._maCommandeSql.Dispose()
            Me.reader.Close()
            Me.result = Nothing
        Catch ex As Exception
        End Try
    End Sub
End Class
