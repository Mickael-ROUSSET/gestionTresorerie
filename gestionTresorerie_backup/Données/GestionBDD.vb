Imports System.Data.SqlClient
Imports System.IO
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Drive.v3
Imports Google.Apis.Services
Imports Google.Apis.Upload
Imports Google.Apis.Util.Store
Imports System.Threading

Public Class GestionBDD

    Private Const NomBase As String = "bddAgumaaa"
    Private Const NomServeur As String = ".\SQLEXPRESS"
    Public Shared Property DossierSauvegarde As String = "D:\Sauvegardes"
    Private Shared ReadOnly CredentialsPath As String = "credentials.json" ' fichier API Google
    Private Shared ReadOnly TokenPath As String = "token.json"

    '------------------------------------------------------------
    ' 🟢 SAUVEGARDE AVEC ROTATION + COPIE GOOGLE DRIVE
    '------------------------------------------------------------
    Public Shared Sub SauvegarderBase(Optional nbMaxSauvegardes As Integer = 7)
        Try
            If Not Directory.Exists(DossierSauvegarde) Then
                Directory.CreateDirectory(DossierSauvegarde)
            End If

            Dim nomFichier As String = $"{NomBase}_{DateTime.Now:yyyy-MM-dd_HHmmss}.bak"
            Dim cheminSauvegarde As String = Path.Combine(DossierSauvegarde, nomFichier)

            Using con As New SqlConnection($"Server={NomServeur};Database=master;Integrated Security=True;")
                con.Open()

                Dim sql As String =
                    $"BACKUP DATABASE [{NomBase}] TO DISK = N'{cheminSauvegarde.Replace("'", "''")}' " &
                    "WITH NOFORMAT, INIT, NAME = N'Sauvegarde automatique', SKIP, NOREWIND, NOUNLOAD, STATS = 5"

                Using cmd As New SqlCommand(sql, con)
                    cmd.CommandTimeout = 0
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            NettoyerAnciennesSauvegardes(nbMaxSauvegardes)

            ' ✅ Copie vers Google Drive
            Task.Run(Sub() EnvoyerSurGoogleDrive(cheminSauvegarde))

            MessageBox.Show($"Sauvegarde réussie :{Environment.NewLine}{cheminSauvegarde}",
                            "Sauvegarde terminée", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Erreur lors de la sauvegarde : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '------------------------------------------------------------
    ' 🔁 ROTATION AUTOMATIQUE
    '------------------------------------------------------------
    Private Shared Sub NettoyerAnciennesSauvegardes(nbMaxSauvegardes As Integer)
        Dim fichiers = Directory.GetFiles(DossierSauvegarde, $"{NomBase}_*.bak").
                       OrderByDescending(Function(f) File.GetCreationTime(f)).ToList()

        If fichiers.Count > nbMaxSauvegardes Then
            For Each f In fichiers.Skip(nbMaxSauvegardes)
                Try
                    File.Delete(f)
                Catch
                End Try
            Next
        End If
    End Sub

    '------------------------------------------------------------
    ' 🔵 RESTAURATION
    '------------------------------------------------------------
    Public Shared Sub RestaurerBase(cheminSauvegarde As String)
        Try
            If Not File.Exists(cheminSauvegarde) Then
                MessageBox.Show("Le fichier de sauvegarde est introuvable : " & cheminSauvegarde,
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            Dim rep = MessageBox.Show($"⚠️ Cette opération va écraser la base '{NomBase}'.{Environment.NewLine}" &
                                      "Voulez-vous continuer ?", "Confirmation",
                                      MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If rep = DialogResult.No Then Exit Sub

            Using con As New SqlConnection($"Server={NomServeur};Database=master;Integrated Security=True;")
                con.Open()

                Dim killSql As String =
                    $"DECLARE @sql NVARCHAR(MAX) = N''; " &
                    $"SELECT @sql += 'KILL ' + CONVERT(VARCHAR(5), session_id) + ';' " &
                    $"FROM sys.dm_exec_sessions WHERE database_id = DB_ID('{NomBase}'); EXEC(@sql);"
                Using cmdKill As New SqlCommand(killSql, con)
                    cmdKill.ExecuteNonQuery()
                End Using

                Dim sqlRestore As String =
                    $"RESTORE DATABASE [{NomBase}] FROM DISK = N'{cheminSauvegarde.Replace("'", "''")}' " &
                    "WITH REPLACE, RECOVERY, STATS = 5"

                Using cmd As New SqlCommand(sqlRestore, con)
                    cmd.CommandTimeout = 0
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Restauration terminée avec succès !", "Succès",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Erreur lors de la restauration : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '------------------------------------------------------------
    ' ☁️ COPIE AUTOMATIQUE SUR GOOGLE DRIVE
    '------------------------------------------------------------
    Private Shared Sub EnvoyerSurGoogleDrive(cheminSauvegarde As String)
        Try
            If Not File.Exists(CredentialsPath) Then
                MessageBox.Show("Fichier credentials.json manquant pour Google Drive.",
                                "Erreur Drive", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If

            Dim credential As UserCredential
            Using stream = New FileStream(CredentialsPath, FileMode.Open, FileAccess.Read)
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    New String() {DriveService.Scope.DriveFile},
                    "user",
                    CancellationToken.None,
                    New FileDataStore(TokenPath, True)
                ).Result
            End Using

            ' Création du service Drive
            Dim service = New DriveService(New BaseClientService.Initializer() With {
                .HttpClientInitializer = credential,
                .ApplicationName = "Sauvegarde bddAgumaaa"
            })

            ' Création du fichier distant
            Dim fichierMetadata = New Google.Apis.Drive.v3.Data.File() With {
                .Name = Path.GetFileName(cheminSauvegarde)
            }

            Using stream = New FileStream(cheminSauvegarde, FileMode.Open)
                Dim requete = service.Files.Create(fichierMetadata, stream, "application/octet-stream")
                requete.Fields = "id"
                requete.Upload()
            End Using

            ' Optionnel : message de confirmation
            Console.WriteLine("Sauvegarde envoyée sur Google Drive : " & Path.GetFileName(cheminSauvegarde))

        Catch ex As Exception
            Console.WriteLine("Erreur Drive : " & ex.Message)
        End Try
    End Sub

End Class
