Imports System.Collections.ObjectModel
Imports System.Configuration.Assemblies
Imports System.Data.SqlClient

' Classe permettant de se connecter à une base de donnée SQLSERVER 

Public Class connexionDB
    Implements IDisposable
    Public _maConnexion As SqlConnection
    Public _maCommandeSql As New SqlCommand()
    Public result As String
    Public reader As SqlDataReader
    Private Shared _instance As connexionDB

    ' Méthode publique pour accéder à l'instance unique
    Public Shared Function GetInstance() As connexionDB
        If _instance Is Nothing Then
            _instance = New connexionDB()
        End If
        Return _instance
    End Function
    Public Function getConnexion() As SqlConnection
        'Crée la connexion si elle n'existe pas, sinon renvoie celle existante 
        Dim lectureProprietes As New lectureProprietes()

        creeConnexion(lectureProprietes.connexionString)
        Return _maConnexion
    End Function
    Public Sub creeConnexion(connexionString As String)
        Try
            ' Crée la connexion si elle n'existe pas, sinon renvoie celle existante
            If _maConnexion Is Nothing Then
                _maConnexion = New SqlConnection(connexionString)
                _maConnexion.Open()
                Logger.GetInstance.INFO("Connexion à la base : " & connexionString)
            End If
        Catch ex As SqlException
            ' Gère les erreurs de connexion SQL
            MsgBox("creeConnexion : erreur SQL : " & ex.Message)
            Logger.GetInstance.ERR(ex.Message)
        Catch ex As Exception
            ' Gère toutes les autres erreurs
            MsgBox("Erreur : " & ex.Message)
            Logger.GetInstance.ERR(ex.Message)
        End Try
    End Sub

    Public Function getRequete(indiceRequete As Integer) As String
        Return My.Settings.Requetes.Item(indiceRequete)
    End Function
    Public Sub setRequete(sRequete As String)
        My.Settings.Requetes.Add(sRequete)
        Logger.GetInstance.INFO("Requête : " & sRequete & " ajoutée")
    End Sub
    Private Sub SuprimeConnexion()
        'Close the reader and the database connection. 
        _maConnexion.Close()
        Logger.GetInstance.INFO("Connexion : " & _maConnexion.ConnectionString & " fermée")
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
