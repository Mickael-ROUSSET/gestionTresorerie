Imports System.IO

Public Class Logger
    ' Niveau de log minimum (DBG, INFO, WARN, ERR)
    Private Shared logLevel As String = "INFO"

    ' Constructeur privé pour empêcher l'instanciation directe
    Private Sub New()
    End Sub

    ' Méthode pour définir le niveau de log minimum
    Public Shared Sub SetLogLevel(level As String)
        ' Vérifier que le niveau de log est valide
        Dim niveauxValides As String() = {"DBG", "INFO", "WARN", "ERR"}
        If Not niveauxValides.Contains(level) Then
            Throw New ArgumentException("Niveau de log invalide. Les niveaux valides sont : DBG, INFO, WARN, ERR.")
        End If

        ' Définir le niveau de log
        logLevel = level
    End Sub

    ' Méthode pour écrire dans le fichier de trace avec un niveau de log
    Private Shared Sub EcrireDansFichierTrace(level As String, message As String)
        ' Vérifier si le niveau de log est suffisant pour écrire dans le fichier
        Dim niveaux As String() = {"DBG", "INFO", "WARN", "ERR"}
        Dim niveauIndex As Integer = Array.IndexOf(niveaux, level)
        Dim niveauMinIndex As Integer = Array.IndexOf(niveaux, logLevel)

        If niveauIndex < niveauMinIndex Then
            Return
        End If

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
