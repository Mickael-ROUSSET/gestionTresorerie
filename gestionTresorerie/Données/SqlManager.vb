' Module de gestion de sauvegarde SQL
' Nécessite la référence : Microsoft.SqlServer.Smo
''' # Documentation Fonctionnelle - Module SqlManager

''' ## Description
''' Ce Module fournit des outils pour la maintenance programmée de la base de données SQL Server `bddAgumaaa`.

''' ## Fonctions principales
''' - **BackupDatabase** : Génère un fichier de sauvegarde physique (.bak) à partir d'une base de données active.

''' ## Paramètres de configuration
''' | Paramètre | Type | Description |
''' | :--- | :--- | :--- |
''' | connectionString | String | Doit pointer vers une instance SQL Server avec les droits SysAdmin ou Backup Operator. |
''' | dbName | String | Le nom de la base de données cible (bddAgumaaa). |
''' | backupPath | String | Le chemin d'accès local ou réseau où le fichier sera écrit. |

Imports System.Data.SqlClient

Public Module SqlManager

    ''' <summary>
    ''' Effectue une sauvegarde complète de la base de données.
    ''' </summary>
    ''' <param name="connectionString">Chaîne de connexion au serveur master</param>
    ''' <param name="dbName">Nom de la base (ex: bddAgumaaa)</param>
    ''' <param name="backupPath">Chemin complet du fichier .bak</param>
    Public Sub BackupDatabase(connectionString As String, dbName As String, backupPath As String)
        Dim query As String = $"BACKUP DATABASE [{dbName}] TO DISK = '{backupPath}'"

        Using conn As New SqlConnection(connectionString)
            Dim cmd As New SqlCommand(query, conn)
            Try
                conn.Open()
                cmd.ExecuteNonQuery()
                Console.WriteLine("Sauvegarde réussie !")
            Catch ex As Exception
                Console.WriteLine("Erreur lors de la sauvegarde : " & ex.Message)
            End Try
        End Using
    End Sub

End Module