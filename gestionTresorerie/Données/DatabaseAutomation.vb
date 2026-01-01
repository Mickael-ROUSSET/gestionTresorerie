Imports System.Data.SqlClient
Imports System.IO

Public Module DatabaseAutomation
    ' À modifier selon l'instance qui fonctionne chez toi (ex: .\SQLEXPRESS)
    Private Const ServerInstance As String = "."
    Private Const DatabaseName As String = "bddAgumaaa"

    ' Chaîne de connexion vers la base Master (nécessaire pour restaurer une base active)
    Private ReadOnly MasterConnString As String = $"Server={ServerInstance};Database=master;Integrated Security=True;TrustServerCertificate=True;"

    ''' <summary>
    ''' Procédure d'exportation (Backup)
    ''' </summary>
    Public Sub ExportDatabase()
        Dim saveDialog As New SaveFileDialog With {
            .Filter = "Sauvegarde SQL (*.bak)|*.bak",
            .FileName = $"{DatabaseName}_{DateTime.Now:yyyyMMdd}.bak"
        }

        'Call attacheBase()
        If saveDialog.ShowDialog() = DialogResult.OK Then
            ' Commande SQL de sauvegarde
            Dim query As String = $"BACKUP DATABASE [{DatabaseName}] TO DISK = '{saveDialog.FileName}' WITH FORMAT, MEDIANAME = 'DbBackup', NAME = 'Full Backup of {DatabaseName}';"

            Try
                Using conn As New SqlConnection(MasterConnString)
                    Dim cmd As New SqlCommand(query, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                    MessageBox.Show("Exportation réussie dans : " & saveDialog.FileName, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Logger.INFO("Exportation réussie dans : " & saveDialog.FileName)
                End Using
            Catch ex As Exception
                MessageBox.Show("Erreur d'exportation : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Logger.ERR("Erreur d'exportation : " & ex.Message)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Procédure d'importation (Restore)
    ''' </summary>
    ''' <summary>
    ''' Importation d'un fichier .bak avec création automatique de la base
    ''' </summary>
    Public Sub ImportDatabase()
        Dim openDialog As New OpenFileDialog With {
            .Filter = "Fichier Sauvegarde SQL (*.bak)|*.bak"
        }

        'Call attacheBase()

        If openDialog.ShowDialog() = DialogResult.OK Then
            ' MOVE est nécessaire si les chemins de fichiers d'origine diffèrent de ta machine
            ' On utilise l'option 'WITH REPLACE' pour écraser si elle existe, ou créer sinon.
            Dim query As String = $"/
            IF EXISTS (SELECT name FROM master.sys.databases WHERE name = '{DatabaseName}')
            BEGIN
                ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            END
            RESTORE DATABASE [{DatabaseName}] 
            FROM DISK = '{openDialog.FileName}' 
            WITH REPLACE,
            MOVE '{DatabaseName}' TO 'C:\SQL_BASES\{DatabaseName}.mdf', 
            MOVE '{DatabaseName}_log' TO 'C:\SQL_BASES\{DatabaseName}_log.ldf';
            
            ALTER DATABASE [{DatabaseName}] SET MULTI_USER;"

            Try
                ' Vérifier que le dossier de destination existe
                If Not Directory.Exists("C:\SQL_BASES") Then Directory.CreateDirectory("C:\SQL_BASES")

                Using conn As New SqlConnection(MasterConnString)
                    conn.Open()
                    Dim cmd As New SqlCommand(query, conn) With {
                        .CommandTimeout = 0 ' Illimité pour les grosses restaurations
                        }
                    cmd.ExecuteNonQuery()
                    MessageBox.Show("La base bddAgumaaa a été importée et activée !", "Succès")
                End Using
            Catch ex As Exception
                MessageBox.Show("Échec de l'import : " & ex.Message)
            End Try
        End If
    End Sub
    Private Sub attacheBase()
        ' 1. Définition des chemins vers tes fichiers
        Dim cheminMDF As String = "C:\Users\User\SQLDATA\bddAgumaaa.mdf"
        Dim cheminLDF As String = "C:\Users\User\SQLDATA\bddAgumaaa_log.ldf"

        ' 2. Vérification si les fichiers existent sur le disque avant d'essayer
        If Not IO.File.Exists(cheminMDF) Then
            MessageBox.Show("Le fichier MDF est introuvable dans C:\Users\User\SQLDATA\", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        ' 3. Appel de la fonction que nous avons créée
        Dim monCursor = Cursors.WaitCursor ' On affiche le sablier
        Dim reussite As Boolean = DatabaseAutomation.AttachDatabaseAuto(cheminMDF, cheminLDF)
        monCursor = Cursors.Default

        ' 4. Résultat
        If reussite Then
            MessageBox.Show("La base bddAgumaaa est maintenant connectée et prête à l'usage !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub
    ''' <summary>
    ''' Tente d'attacher physiquement les fichiers de la base au serveur SQL.
    ''' </summary>
    Public Function AttachDatabaseAuto(mdfPath As String, ldfPath As String) As Boolean
        ' Connexion à Master pour effectuer l'opération système
        Using conn As New SqlConnection(MasterConnString)
            ' Script SQL pour attacher la base
            Dim query As String = $"CREATE DATABASE [{DatabaseName}] ON " &
                                  $"(FILENAME = '{mdfPath}'), " &
                                  $"(FILENAME = '{ldfPath}') FOR ATTACH;"
            Try
                conn.Open()
                Dim cmd As New SqlCommand(query, conn)
                cmd.ExecuteNonQuery()
                Return True
            Catch ex As Exception
                MessageBox.Show("Erreur d'attachement : " & ex.Message)
                Return False
            End Try
        End Using
    End Function
End Module