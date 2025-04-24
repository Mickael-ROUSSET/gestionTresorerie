Imports System.IO

Public Class Logger
    ' Constructeur privé pour empêcher l'instanciation directe
    Private Sub New()
    End Sub

    ' Méthode pour écrire dans le fichier de trace avec un niveau de log
    Public Shared Sub EcrireDansFichierTrace(level As String, message As String)
        Try
            ' Lire le chemin du fichier de trace à partir de app.config
            Dim cheminFichierTrace As String = LectureProprietes.GetCheminEtVariable("fichierLog")

            If String.IsNullOrEmpty(cheminFichierTrace) Then
                Throw New Exception("Le chemin du fichier de trace n'est pas configuré dans app.config.")
            End If

            ' Obtenir l'heure actuelle
            Dim heureActuelle As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

            ' Obtenir le nom de la fonction appelante
            Dim stackTrace As New System.Diagnostics.StackTrace()
            Dim nomFonctionAppelante As String = stackTrace.GetFrame(2).GetMethod().Name

            ' Construire le message complet
            Dim messageComplet As String = $"{heureActuelle} - [{level}] - {nomFonctionAppelante} : {message}"

            ' Écrire dans le fichier de trace
            Using writer As New StreamWriter(cheminFichierTrace, True)
                writer.WriteLine(messageComplet)
            End Using

            'Console.WriteLine("Trace écrite avec succès.")
        Catch ex As Exception
            Console.WriteLine("Erreur lors de l'écriture dans le fichier de trace : " & ex.Message)
        End Try
    End Sub

    ' Méthodes pour les différents niveaux de log
    Public Shared Sub DBG(message As String)
        EcrireDansFichierTrace("DBG", message)
    End Sub

    Public Shared Sub INFO(message As String)
        EcrireDansFichierTrace("INFO", message)
    End Sub

    Public Shared Sub WARN(message As String)
        EcrireDansFichierTrace("WARN", message)
    End Sub

    Public Shared Sub ERR(message As String)
        EcrireDansFichierTrace("ERR", message)
    End Sub
End Class
