Imports System.Net.Mail
Imports System.Net
Imports System.IO

''' <summary>
''' Gère l'expédition de courriels de manière asynchrone.
''' </summary>
Public NotInheritable Class MailManager

    ''' <summary>
    ''' Envoie un e-mail avec gestion optionnelle de pièces jointes.
    ''' </summary>
    ''' <returns>True si l'envoi est confirmé, sinon False.</returns>
    Public Async Function EnvoyerEmailAsync(destinataire As String, sujet As String, corps As String, Optional cheminsFichiers As List(Of String) = Nothing) As Task(Of Boolean)
        Try
            Using mail = PreparerMessage(destinataire, sujet, corps, cheminsFichiers)
                Return Await EnvoyerViaSmtpAsync(mail)
            End Using
        Catch ex As Exception
            Logger.ERR($"Echec de la procédure d'envoi : {ex.Message}")
            Return False
        End Try
    End Function
    Public Function EnvoyerEmail(destinataire As String,
                             sujet As String,
                             corps As String,
                             Optional cheminsFichiers As List(Of String) = Nothing) As Boolean

        Return EnvoyerEmailAsync(destinataire, sujet, corps, cheminsFichiers).
        GetAwaiter().
        GetResult()
    End Function

    ''' <summary>
    ''' Construit l'objet MailMessage et attache les fichiers valides.
    ''' </summary>
    Private Function PreparerMessage(destinataire As String, sujet As String, corps As String, chemins As List(Of String)) As MailMessage
        Dim mail As New MailMessage()

        mail.From = New MailAddress(LectureProprietes.GetVariable("SmtpUsername"))
        mail.To.Add(destinataire)
        mail.Subject = sujet
        mail.Body = corps
        mail.IsBodyHtml = True

        ' Ajout des pièces jointes avec vérification d'existence
        If chemins IsNot Nothing Then
            For Each chemin In chemins
                If File.Exists(chemin) Then
                    mail.Attachments.Add(New Attachment(chemin))
                Else
                    Logger.ERR($"Fichier manquant ignoré : {chemin}")
                End If
            Next
        End If

        Return mail
    End Function

    ''' <summary>
    ''' Configure le client SMTP et procède à l'envoi asynchrone.
    ''' </summary>
    Private Async Function EnvoyerViaSmtpAsync(mail As MailMessage) As Task(Of Boolean)
        ' Récupération des paramètres via ton utilitaire de propriétés
        Dim host = LectureProprietes.GetVariable("SmtpServer")
        Dim port = CInt(LectureProprietes.GetVariable("SmtpPort"))
        Dim user = LectureProprietes.GetVariable("SmtpUsername")
        Dim pass = LectureProprietes.GetVariable("SmtpPassword")

        Using smtp As New SmtpClient(host, port)
            smtp.EnableSsl = True
            smtp.UseDefaultCredentials = False
            smtp.Credentials = New NetworkCredential(user, pass)

            ' Envoi asynchrone pour ne pas geler l'application
            Await smtp.SendMailAsync(mail)
            Return True
        End Using
    End Function

End Class