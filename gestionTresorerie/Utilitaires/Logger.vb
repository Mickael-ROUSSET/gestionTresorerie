Imports System.IO
Imports System.Configuration

Public Class Logger
    ' Instance unique de la classe
    Private Shared _instance As Logger

    ' Constructeur privé pour empêcher l'instanciation directe
    Private Sub New()
    End Sub

    ' Méthode publique pour accéder à l'instance unique
    Public Shared Function GetInstance() As Logger
        If _instance Is Nothing Then
            _instance = New Logger()
        End If
        Return _instance
    End Function

    ' Méthode pour écrire dans le fichier de trace avec un niveau de log
    Public Sub EcrireDansFichierTrace(level As String, message As String)
        Try
            ' Lire le chemin du fichier de trace à partir de app.config
            Dim cheminFichierTrace As String = lectureProprietes.GetVariable("fichierLog")

            If String.IsNullOrEmpty(cheminFichierTrace) Then
                Throw New Exception("Le chemin du fichier de trace n'est pas configuré dans app.config.")
            End If

            ' Obtenir l'heure actuelle
            Dim heureActuelle As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

            ' Obtenir le nom de la fonction appelante
            Dim stackTrace As New System.Diagnostics.StackTrace()
            'Dim sTmp As String
            'For i = 0 To stackTrace.FrameCount - 1
            '    sTmp += "i : " & i & " Méthode : " & stackTrace.GetFrame(i).GetMethod().Name & vbCrLf
            'Next
            'MsgBox("Liste méthodes : " & vbCrLf & sTmp)
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
    Public Sub DBG(message As String)
        EcrireDansFichierTrace("DBG", message)
    End Sub

    Public Sub INFO(message As String)
        EcrireDansFichierTrace("INFO", message)
    End Sub

    Public Sub WARN(message As String)
        EcrireDansFichierTrace("WARN", message)
    End Sub

    Public Sub ERR(message As String)
        EcrireDansFichierTrace("ERR", message)
    End Sub
End Class
