Imports System.Data.SqlClient

''' <summary>
''' Opérations administratives SQL Server. Ne lance aucune opération au constructeur.
''' </summary>
Public Class SqlAdminService

    Private ReadOnly _connectionFactory As IConnectionFactory

    Public Sub New(connectionFactory As IConnectionFactory)
        If connectionFactory Is Nothing Then
            Throw New ArgumentNullException(NameOf(connectionFactory))
        End If
        _connectionFactory = connectionFactory
    End Sub

    ''' <summary>
    ''' Sauvegarde une base dans un fichier .bak. L'appelant choisit le chemin et
    ''' doit disposer des autorisations SQL Server nécessaires.
    ''' </summary>
    Public Sub SauvegarderBase(dbName As String, backupPath As String)
        If String.IsNullOrWhiteSpace(dbName) Then
            Throw New ArgumentException("Le nom de la base est obligatoire.", NameOf(dbName))
        End If
        If String.IsNullOrWhiteSpace(backupPath) Then
            Throw New ArgumentException("Le chemin de sauvegarde est obligatoire.", NameOf(backupPath))
        End If

        ' Les identifiants SQL ne sont pas paramétrables : échapper les crochets.
        Dim escapedDatabaseName As String = "[" & dbName.Replace("]", "]]") & "]"
        Dim sql As String = "BACKUP DATABASE " & escapedDatabaseName &
                            " TO DISK = @backupPath WITH INIT"

        Using connection As SqlConnection = _connectionFactory.CreateConnection()
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.Parameters.Add("@backupPath", SqlDbType.NVarChar, 4000).Value = backupPath
                ' Une sauvegarde peut durer plusieurs minutes.
                command.CommandTimeout = 600
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' La restauration écrase potentiellement les données courantes.
    ''' Elle est volontairement désactivée jusqu'à définition d'un protocole
    ''' validé (sauvegarde préalable, base cible, connexions et retour arrière).
    ''' </summary>
    Public Sub RestaurerBase(dbName As String, backupPath As String)
        Throw New NotSupportedException(
            "Restauration SQL désactivée : une procédure de sécurité et une validation explicite sont nécessaires.")
    End Sub

End Class
