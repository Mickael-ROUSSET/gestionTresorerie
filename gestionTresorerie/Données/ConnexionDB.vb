Imports System.Data.SqlClient
Imports System.IO

''' <summary>
''' Classe singleton thread-safe pour gérer les connexions SQL Server.
''' </summary>
Public Class ConnexionDB
    Implements IDisposable

    Private _connexionBddAgumaaa As SqlConnection
    Private _connexionCinema As SqlConnection
    Private _connexionMdN As SqlConnection
    Private disposedValue As Boolean

    ' Locks pour le thread-safe
    Private Shared ReadOnly _lockBddAgumaaa As New Object()
    Private Shared ReadOnly _lockCinema As New Object()
    Private Shared ReadOnly _lockMdN As New Object()

    ' Singleton par base
    Private Shared _instanceBddAgumaaa As ConnexionDB
    Private Shared _instanceCinema As ConnexionDB
    Private Shared _instanceMdN As ConnexionDB

    ' Accès aux instances singleton
    Public Shared Function GetInstance(sBase As String) As ConnexionDB
        Select Case sBase
            Case Constantes.bddAgumaaa
                If _instanceBddAgumaaa Is Nothing Then
                    SyncLock _lockBddAgumaaa
                        If _instanceBddAgumaaa Is Nothing Then
                            _instanceBddAgumaaa = New ConnexionDB()
                        End If
                    End SyncLock
                End If
                Return _instanceBddAgumaaa
            Case Constantes.cinemaDB
                If _instanceCinema Is Nothing Then
                    SyncLock _lockCinema
                        If _instanceCinema Is Nothing Then
                            _instanceCinema = New ConnexionDB()
                        End If
                    End SyncLock
                End If
                Return _instanceCinema
            Case Constantes.MarcheDeNoelDB
                If _instanceMdN Is Nothing Then
                    SyncLock _lockMdN
                        If _instanceMdN Is Nothing Then
                            _instanceMdN = New ConnexionDB()
                        End If
                    End SyncLock
                End If
                Return _instanceMdN
            Case Else
                Throw New ArgumentException($"Base inconnue : {sBase}")
        End Select
    End Function

    ''' <summary>
    ''' Renvoie la connexion SQL pour la base demandée
    ''' </summary>
    Public Function GetConnexion(sBase As String) As SqlConnection
        Try
            ' Vérifie l'accès au dossier racine
            If Not Directory.Exists(LectureProprietes.GetVariable("repRacineAgumaaa")) Then
                Throw New IOException("Le stockage Google Drive n'est pas accessible.")
            End If

            Select Case sBase
                Case Constantes.bddAgumaaa
                    SyncLock _lockBddAgumaaa
                        If _connexionBddAgumaaa Is Nothing OrElse _connexionBddAgumaaa.State = ConnectionState.Closed Then
                            _connexionBddAgumaaa = CreeConnexion(LectureProprietes.connexionString(sBase))
                        End If
                        Return _connexionBddAgumaaa
                    End SyncLock
                Case Constantes.cinemaDB
                    SyncLock _lockCinema
                        If _connexionCinema Is Nothing OrElse _connexionCinema.State = ConnectionState.Closed Then
                            _connexionCinema = CreeConnexion(LectureProprietes.connexionString(sBase))
                        End If
                        Return _connexionCinema
                    End SyncLock
                Case Constantes.MarcheDeNoelDB
                    SyncLock _lockMdN
                        If _connexionMdN Is Nothing OrElse _connexionMdN.State = ConnectionState.Closed Then
                            _connexionMdN = CreeConnexion(LectureProprietes.connexionString(sBase))
                        End If
                        Return _connexionMdN
                    End SyncLock
                Case Else
                    Throw New ArgumentException($"Base inconnue : {sBase}")
            End Select

        Catch ex As SqlException
            Logger.ERR($"Erreur SQL : {ex.Message}")
            MessageBox.Show("Impossible de se connecter à la base de données.", "Erreur SQL",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        Catch ex As IOException
            Logger.ERR($"Erreur d'accès Drive : {ex.Message}")
            MessageBox.Show("Le stockage Google Drive semble inactif ou inaccessible.", "Erreur Drive",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return Nothing
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue : {ex.Message}")
            MessageBox.Show("Une erreur inattendue est survenue : " & ex.Message, "Erreur",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Crée et ouvre une connexion SQL
    ''' </summary>
    Private Function CreeConnexion(connexionString As String) As SqlConnection
        Dim cn As New SqlConnection(connexionString)
        cn.Open()
        Logger.INFO($"Connexion ouverte : {connexionString}")
        Return cn
    End Function

    ''' <summary>
    ''' Ferme la connexion pour la base spécifiée
    ''' </summary>
    Public Sub FermerConnexion(sBase As String)
        Select Case sBase
            Case Constantes.bddAgumaaa
                SyncLock _lockBddAgumaaa
                    FermerConnexion(_connexionBddAgumaaa)
                    _connexionBddAgumaaa = Nothing
                End SyncLock
            Case Constantes.cinemaDB
                SyncLock _lockCinema
                    FermerConnexion(_connexionCinema)
                    _connexionCinema = Nothing
                End SyncLock
            Case Constantes.MarcheDeNoelDB
                SyncLock _lockMdN
                    FermerConnexion(_connexionMdN)
                    _connexionMdN = Nothing
                End SyncLock
        End Select
    End Sub

    Private Sub FermerConnexion(ByRef cn As SqlConnection)
        If cn IsNot Nothing Then
            Try
                If cn.State = ConnectionState.Open Then cn.Close()
                cn.Dispose()
                Logger.INFO("Connexion fermée.")
            Catch ex As Exception
                Logger.ERR($"Erreur à la fermeture de connexion : {ex.Message}")
            End Try
        End If
    End Sub

#Region "IDisposable Support"
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                FermerConnexion(_connexionBddAgumaaa)
                FermerConnexion(_connexionCinema)
            End If
            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
#End Region

End Class
