Imports System.Net.Mail
Imports System.Net
Imports System.IO ' Nécessaire pour vérifier les fichiers


' Données du compte d'envoi
'Private Const SmtpServer As String = "smtp.gmail.com"
'Private Const SmtpPort As Integer = 587 ' Port standard pour TLS/STARTTLS
'Private Const SmtpUsername As String = "agumaaa43@gmail.com"

'' ATTENTION : REMPLACER "VOTRE_MOT_DE_PASSE_APP" par le mot de passe d'application généré par Google
'Private Const SmtpPassword As String = "VOTRE_MOT_DE_PASSE_APP"

''' <summary>
''' Envoie un e-mail via le serveur Gmail.
''' </summary>
''' <param name="destinataire">Coordonnees e-mail du destinataire.</param>
''' <param name="sujet">Sujet de l'e-mail.</param>
''' <param name="corps">Contenu de l'e-mail (peut être HTML).</param>
''' <returns>Vrai si l'envoi a réussi, Faux sinon.</returns> 

Public Class MailManager

    ' Données du compte d'envoi
    Private Const SmtpServer As String = "smtp.gmail.com"
    Private Const SmtpPort As Integer = 587
    Private Const SmtpUsername As String = "agumaaa43@gmail.com"

    ' ATTENTION : Utiliser le MOT DE PASSE D'APPLICATION généré par Google
    Private Const SmtpPassword As String = "VOTRE_MOT_DE_PASSE_APP"

    ''' <summary>
    ''' Envoie un e-mail via le serveur Gmail, avec support pour les pièces jointes.
    ''' </summary>
    ''' <param name="destinataire">Coordonnees e-mail du destinataire.</param>
    ''' <param name="sujet">Sujet de l'e-mail.</param>
    ''' <param name="corps">Contenu de l'e-mail.</param>
    ''' <param name="cheminsFichiers">Liste optionnelle des chemins complets vers les fichiers à joindre.</param>
    ''' <returns>Vrai si l'envoi a réussi, Faux sinon.</returns>
    Public Function EnvoyerEmail(ByVal destinataire As String, ByVal sujet As String, ByVal corps As String, Optional ByVal cheminsFichiers As List(Of String) = Nothing) As Boolean
        Try
            ' 1. Créer le message
            Using mail As New MailMessage()
                mail.From = New MailAddress(LectureProprietes.GetVariable("SmtpUsername"))
                mail.To.Add(destinataire)
                mail.Subject = sujet
                mail.Body = corps
                mail.IsBodyHtml = True

                ' 2. Gérer les pièces jointes
                If cheminsFichiers IsNot Nothing Then
                    For Each chemin As String In cheminsFichiers
                        If File.Exists(chemin) Then
                            ' Ajouter la pièce jointe à la collection Attachments
                            mail.Attachments.Add(New Attachment(chemin))
                        Else
                            ' Journaliser une erreur si le fichier est introuvable
                            Logger.ERR($"Pièce jointe introuvable et ignorée : {chemin}")
                        End If
                    Next
                End If

                ' 3. Créer et configurer le client SMTP
                Using smtp As New SmtpClient(LectureProprietes.GetVariable("SmtpServer"), LectureProprietes.GetVariable("SmtpPort"))
                    smtp.EnableSsl = True
                    smtp.UseDefaultCredentials = False
                    smtp.Credentials = New NetworkCredential(LectureProprietes.GetVariable("SmtpUsername"), LectureProprietes.GetVariable("SmtpPassword"))

                    ' 4. Envoyer le message
                    smtp.Send(mail)
                End Using
            End Using

            Return True

        Catch ex As SmtpException
            Logger.ERR($"Erreur SMTP lors de l'envoi de l'e-mail : {ex.StatusCode} - {ex.Message}")
            Return False
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de l'envoi de l'e-mail : {ex.Message}")
            Return False
        End Try
    End Function

End Class