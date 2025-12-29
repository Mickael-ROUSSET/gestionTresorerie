Imports System.Data.SqlClient

Public Class _ServiceExport

    Public Shared Sub ExecuterExportSQL(cheminDossier As String)
        ' Utilisation de ta classe de connexion existante
        Dim db = ConnexionDB.GetInstance(Constantes.bddAgumaaa)

        Using conn As SqlConnection = db.GetConnexion(Constantes.bddAgumaaa)
            ' S'assurer que la connexion est ouverte
            If conn.State = ConnectionState.Closed Then conn.Open()

            Using cmd As New SqlCommand("dbo.usp_ExporterBaseVersFichier", conn)
                cmd.CommandType = CommandType.StoredProcedure

                ' Ajout du paramètre du chemin de destination
                ' Attention : SQL Server doit avoir les droits d'écriture sur ce dossier
                cmd.Parameters.AddWithValue("@CheminDossier", cheminDossier)

                ' TRÈS IMPORTANT : On augmente le timeout à 5 minutes (300 secondes)
                ' car l'exportation de données et l'écriture fichier peuvent prendre du temps
                cmd.CommandTimeout = 300

                Try
                    cmd.ExecuteNonQuery()
                    MessageBox.Show("L'exportation a été générée avec succès dans : " & cheminDossier,
                                    "Export terminé", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Logger.ERR("L'exportation a été générée avec succès dans : " & cheminDossier)
                Catch ex As SqlException
                    ' Gestion spécifique des erreurs SQL (droits xp_cmdshell, etc.)
                    MessageBox.Show("Erreur SQL lors de l'export : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Logger.ERR("Erreur SQL lors de l'export : " & ex.Message)
                Catch ex As Exception
                    MessageBox.Show("Erreur générale : " & ex.Message)
                    Logger.ERR("Erreur générale : " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

End Class