Imports System.Data.SqlClient

' Classe permettant de se connecter à une base de donnée SQLSERVER 

Public Class ConnexionDB
    Implements IDisposable
    Public _maConnexion As SqlConnection
    Public _maCommandeSql As New SqlCommand()
    Private Shared _instance As ConnexionDB
    Private disposed As Boolean = False ' Pour détecter les appels redondants

    ' Méthode publique pour accéder à l'instance unique
    Public Shared Function GetInstance() As ConnexionDB
        If _instance Is Nothing Then
            _instance = New ConnexionDB()
        End If
        Return _instance
    End Function
    Public Function getConnexion() As SqlConnection
        'Crée la connexion si elle n'existe pas, sinon renvoie celle existante 

        creeConnexion(LectureProprietes.connexionString)
        Return _maConnexion
    End Function
    Public Sub creeConnexion(connexionString As String)
        Dim connexionOuverte As Boolean
        Try
            ' Crée la connexion si elle n'existe pas, sinon renvoie celle existante
            If _maConnexion Is Nothing Then
                _maConnexion = New SqlConnection(connexionString)
                _maConnexion.Open()
                connexionOuverte = True
                Logger.INFO("Connexion à la base : " & connexionString)
            End If
        Catch ex As SqlException
            ' Gère les erreurs de connexion SQL
            Dim unused1 = MsgBox("creeConnexion : erreur SQL : " & ex.Message)
            Logger.ERR(ex.Message)
            End
        Catch ex As Exception
            ' Gère toutes les autres erreurs
            Dim unused = MsgBox("Erreur : " & ex.Message)
            Logger.ERR(ex.Message)
            End
        End Try
    End Sub


    Public Function getRequete(indiceRequete As Integer) As String
        Return My.Settings.Requetes.Item(indiceRequete)
    End Function
    Public Sub setRequete(sRequete As String)
        Dim unused = My.Settings.Requetes.Add(sRequete)
        Logger.INFO("Requête : " & sRequete & " ajoutée")
    End Sub
    Private Sub SuprimeConnexion()
        'Close the reader and the database connection. 
        _maConnexion.Close()
        Logger.INFO("Connexion : " & _maConnexion.ConnectionString & " fermée")
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
