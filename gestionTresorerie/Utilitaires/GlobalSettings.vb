Public Class GlobalSettings
    ' Propriétés partagées (accessibles partout via GlobalSettings.NomDeLaPropriete)
    Public Shared Property GeminiKey As String
    Public Shared Property SqlConnectionString As String
    Public Shared Property GmailPassword As String
    Public Shared Property GCleTmdb As String

    ''' <summary>
    ''' Initialise les paramètres au démarrage
    ''' </summary>
    Public Shared Sub Initialize()
        Try
            Dim secrets = ConfigurationLoader.LoadSecrets()
            GeminiKey = secrets.GeminiKey
            'SqlConnectionString = secrets.SqlConn
            GmailPassword = secrets.GmailPass
            GCleTmdb = secrets.GCleTmdb

            Logger.INFO("Configuration chargée avec succès.")
        Catch ex As Exception
            Logger.ERR("Erreur fatale au chargement de la config : " & ex.Message)
            ' Optionnel : On peut forcer l'arrêt de l'appli ici si les secrets sont vitaux
            Throw
        End Try
    End Sub
End Class