Imports System.Data.SqlClient

' Classe permettant de se connecter à une base de donnée SQLSERVER 

Public Class connexionDB
    Implements IDisposable
    Public _maConnexion As SqlConnection
    Public _maCommandeSql As New SqlCommand()
    Private Shared _instance As connexionDB
    Private disposed As Boolean = False ' Pour détecter les appels redondants

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

    ' Destructeur
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' Implémentation de IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposed Then
            If disposing Then
                ' Libérer les ressources managées ici.
                If _maConnexion IsNot Nothing Then
                    If _maConnexion.State = ConnectionState.Open Then
                        _maConnexion.Close()
                    End If
                    _maConnexion.Dispose()
                    _maConnexion = Nothing
                End If
                If _maCommandeSql IsNot Nothing Then
                    _maCommandeSql.Dispose()
                    _maCommandeSql = Nothing
                End If
                ' Libérer d'autres ressources managées ici si nécessaire.
            End If

            ' Libérer les ressources non managées ici (si vous en avez).
            disposed = True
        End If
    End Sub
End Class
