Imports System.Data.SqlClient
Imports System.IO

''' <summary>
''' Classe singleton thread-safe pour gérer les connexions SQL Server.
''' </summary>
Public Class ConnexionDB
    Implements IDisposable

    Private _connexionBddAgumaaa As SqlConnection
    Private disposedValue As Boolean

    Private Shared ReadOnly _lockBddAgumaaa As New Object()
    Private Shared _instanceBddAgumaaa As ConnexionDB

    Public Shared Function GetInstance(sBase As String) As ConnexionDB
        If String.IsNullOrWhiteSpace(sBase) Then
            Throw New ArgumentException("Le nom logique de base est obligatoire.", NameOf(sBase))
        End If

        Select Case sBase
            Case Constantes.DataBases.Agumaaa
                If _instanceBddAgumaaa Is Nothing Then
                    SyncLock _lockBddAgumaaa
                        If _instanceBddAgumaaa Is Nothing Then
                            _instanceBddAgumaaa = New ConnexionDB()
                        End If
                    End SyncLock
                End If

                Return _instanceBddAgumaaa

            Case Else
                Throw New ArgumentException($"Base inconnue : {sBase}", NameOf(sBase))
        End Select
    End Function

    Public Function GetConnexion(sBase As String) As SqlConnection
        If String.IsNullOrWhiteSpace(sBase) Then
            Throw New ArgumentException("Le nom logique de base est obligatoire.", NameOf(sBase))
        End If

        Try
            VerifierStockageAccessible()

            Select Case sBase
                Case Constantes.DataBases.Agumaaa
                    SyncLock _lockBddAgumaaa
                        If _connexionBddAgumaaa Is Nothing OrElse
                           _connexionBddAgumaaa.State = ConnectionState.Closed OrElse
                           _connexionBddAgumaaa.State = ConnectionState.Broken Then

                            Dim cs As String = LectureProprietes.connexionString(sBase)

                            If String.IsNullOrWhiteSpace(cs) Then
                                Throw New InvalidOperationException($"Chaîne de connexion vide pour la base '{sBase}'.")
                            End If

                            _connexionBddAgumaaa = CreeConnexion(cs)
                        End If

                        Return _connexionBddAgumaaa
                    End SyncLock

                Case Else
                    Throw New ArgumentException($"Base inconnue : {sBase}", NameOf(sBase))
            End Select

        Catch ex As SqlException
            Logger.ERR($"Erreur SQL lors de la connexion à '{sBase}' : {ex.Message}")
            Throw New InvalidOperationException($"Impossible de se connecter à la base SQL '{sBase}'.", ex)

        Catch ex As IOException
            Logger.ERR($"Erreur d'accès au stockage lors de la connexion à '{sBase}' : {ex.Message}")
            Throw New InvalidOperationException("Le stockage Google Drive est inactif ou inaccessible.", ex)

        Catch ex As Exception
            Logger.ERR($"Erreur inattendue ConnexionDB.GetConnexion('{sBase}') : {ex.Message}")
            Throw
        End Try
    End Function

    Private Shared Sub VerifierStockageAccessible()
        Dim repRacine As String = LectureProprietes.GetVariable("repRacineAgumaaa")

        If String.IsNullOrWhiteSpace(repRacine) Then
            Throw New IOException("Le paramètre 'repRacineAgumaaa' est vide ou absent.")
        End If

        If Not Directory.Exists(repRacine) Then
            Throw New IOException($"Le stockage Google Drive n'est pas accessible : {repRacine}")
        End If
    End Sub

    Private Function CreeConnexion(connexionString As String) As SqlConnection
        If String.IsNullOrWhiteSpace(connexionString) Then
            Throw New ArgumentException("La chaîne de connexion ne peut pas être vide.", NameOf(connexionString))
        End If

        Dim cn As New SqlConnection(connexionString)
        cn.Open()

        Logger.INFO("Connexion SQL ouverte.")
        Return cn
    End Function

    Public Sub FermerConnexion(sBase As String)
        If String.IsNullOrWhiteSpace(sBase) Then
            Throw New ArgumentException("Le nom logique de base est obligatoire.", NameOf(sBase))
        End If

        Select Case sBase
            Case Constantes.DataBases.Agumaaa
                SyncLock _lockBddAgumaaa
                    FermerConnexion(_connexionBddAgumaaa)
                    _connexionBddAgumaaa = Nothing
                End SyncLock

            Case Else
                Throw New ArgumentException($"Base inconnue : {sBase}", NameOf(sBase))
        End Select
    End Sub

    Private Sub FermerConnexion(ByRef cn As SqlConnection)
        If cn Is Nothing Then
            Exit Sub
        End If

        Try
            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If

            cn.Dispose()
            Logger.INFO("Connexion SQL fermée.")

        Catch ex As Exception
            Logger.ERR($"Erreur à la fermeture de connexion : {ex.Message}")
        Finally
            cn = Nothing
        End Try
    End Sub

#Region "IDisposable Support"

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                FermerConnexion(_connexionBddAgumaaa)
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