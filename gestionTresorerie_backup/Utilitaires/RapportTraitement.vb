Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class RapportTraitement
    ' Enum pour les types de documents
    Public Enum TypeDocument
        Cheque
        FormulaireAdhesion
        QuestionnaireMedical
    End Enum

    ' Structure interne pour stocker les compteurs par type de document
    Private Structure CompteurType
        Public Succes As Integer
        Public Echecs As Integer
        Public Avertissements As Integer
    End Structure

    ' Variables statiques pour le rapport
    Private Shared _compteurs As New Dictionary(Of TypeDocument, CompteurType)
    Private Shared _debutTraitement As DateTime
    Private Shared _finTraitement As DateTime?
    Private Shared _messageGlobal As New List(Of String)

    ' Méthode interne pour écrire dans le fichier de log
    Public Shared Sub WriteToLog(message As String, level As String)
        Try
            Dim logMessage As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}"
            File.AppendAllText(LectureProprietes.GetVariable("fichierCRBatch"), logMessage & vbCrLf)
        Catch ex As Exception
            ' En cas d'erreur d'écriture, écrire dans la console comme secours
            Logger.ERR($"Erreur d'écriture dans le fichier de log '{LectureProprietes.GetVariable("fichierCRBatch")}' : {ex.Message}")
            Logger.ERR($"Message non écrit : [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}")
        End Try
    End Sub

    ' Démarrer le rapport
    Public Shared Sub DemarrerRapport()
        _compteurs.Clear()
        For Each typeDoc As TypeDocument In [Enum].GetValues(GetType(TypeDocument))
            _compteurs(typeDoc) = New CompteurType With {
                .Succes = 0,
                .Echecs = 0,
                .Avertissements = 0
            }
        Next
        _debutTraitement = DateTime.Now
        _finTraitement = Nothing
        _messageGlobal.Clear()
        WriteToLog("Rapport de traitement démarré à : " & _debutTraitement.ToString("yyyy-MM-dd HH:mm:ss"), "INFO")
    End Sub

    ' Mettre à jour le rapport pour un type de document
    Public Shared Sub MettreAJour(typeDoc As TypeDocument, succes As Boolean, Optional message As String = "")
        If _debutTraitement = DateTime.MinValue Then
            WriteToLog("Rapport non démarré. Appelez DemarrerRapport() d'abord.", "WARN")
            Return
        End If

        Dim compteur As CompteurType = _compteurs(typeDoc)
        If succes Then
            compteur.Succes += 1
        Else
            compteur.Echecs += 1
        End If

        If Not String.IsNullOrEmpty(message) Then
            _messageGlobal.Add($"{typeDoc.ToString()} - {message}")
        End If

        _compteurs(typeDoc) = compteur
    End Sub

    ' Incrémenter les avertissements pour un type de document
    Public Shared Sub AjouterAvertissement(typeDoc As TypeDocument, Optional message As String = "")
        If _debutTraitement = DateTime.MinValue Then
            WriteToLog("Rapport non démarré. Appelez DemarrerRapport() d'abord.", "WARN")
            Return
        End If

        Dim compteur As CompteurType = _compteurs(typeDoc)
        compteur.Avertissements += 1

        If Not String.IsNullOrEmpty(message) Then
            _messageGlobal.Add($"{typeDoc.ToString()} - Avertissement : {message}")
        End If

        _compteurs(typeDoc) = compteur
    End Sub

    ' Générer le rapport au format texte
    Public Shared Function GenererRapport() As String
        If _debutTraitement = DateTime.MinValue Then
            WriteToLog("Rapport non démarré. Appelez DemarrerRapport() d'abord.", "WARN")
            Return ""
        End If

        _finTraitement = DateTime.Now
        Dim dureeTraitement As TimeSpan = _finTraitement.Value - _debutTraitement
        Dim totalSucces As Integer = 0
        Dim totalEchecs As Integer = 0
        Dim totalAvertissements As Integer = 0

        ' Construire le rapport
        Dim rapport As New System.Text.StringBuilder()
        Dim unused18 = rapport.AppendLine("=== COMPTE-RENDU DE TRAITEMENT ===")
        Dim unused17 = rapport.AppendLine($"Date de début : {_debutTraitement.ToString("yyyy-MM-dd HH:mm:ss")}")
        Dim unused16 = rapport.AppendLine($"Date de fin : {_finTraitement.Value.ToString("yyyy-MM-dd HH:mm:ss")}")
        Dim unused15 = rapport.AppendLine($"Durée totale : {dureeTraitement.ToString("hh\:mm\:ss")}")
        Dim unused14 = rapport.AppendLine()

        ' Détails par type de document
        For Each kvp As KeyValuePair(Of TypeDocument, CompteurType) In _compteurs
            totalSucces += kvp.Value.Succes
            totalEchecs += kvp.Value.Echecs
            totalAvertissements += kvp.Value.Avertissements

            Dim unused13 = rapport.AppendLine($"Type de document : {kvp.Key.ToString()}")
            Dim unused12 = rapport.AppendLine($"  - Succès : {kvp.Value.Succes}")
            Dim unused11 = rapport.AppendLine($"  - Échecs : {kvp.Value.Echecs}")
            Dim unused10 = rapport.AppendLine($"  - Avertissements : {kvp.Value.Avertissements}")
            Dim unused9 = rapport.AppendLine()
        Next

        ' Totaux généraux
        Dim unused8 = rapport.AppendLine("=== TOTAUX GÉNÉRAUX ===")
        Dim unused7 = rapport.AppendLine($"Succès totaux : {totalSucces}")
        Dim unused6 = rapport.AppendLine($"Échecs totaux : {totalEchecs}")
        Dim unused5 = rapport.AppendLine($"Avertissements totaux : {totalAvertissements}")
        Dim unused4 = rapport.AppendLine($"Documents traités : {totalSucces + totalEchecs}")
        Dim unused3 = rapport.AppendLine()

        ' Messages globaux
        If _messageGlobal.Count > 0 Then
            Dim unused2 = rapport.AppendLine("=== MESSAGES DÉTAILLÉS ===")
            For Each msg As String In _messageGlobal
                Dim unused1 = rapport.AppendLine(msg)
            Next
        End If

        Dim unused = rapport.AppendLine("=== FIN DU RAPPORT ===")
        WriteToLog("Rapport de traitement terminé.", "INFO")
        Return rapport.ToString()
    End Function

    ' Générer le rapport au format JSON
    Public Shared Function GenererRapportJson() As String
        If _debutTraitement = DateTime.MinValue Then
            WriteToLog("Rapport non démarré. Appelez DemarrerRapport() d'abord.", "WARN")
            Return "{}"
        End If

        _finTraitement = DateTime.Now
        Dim dureeTraitement As TimeSpan = _finTraitement.Value - _debutTraitement
        Dim totalSucces As Integer = 0
        Dim totalEchecs As Integer = 0
        Dim totalAvertissements As Integer = 0

        ' Construire l'objet JSON
        Dim rapportJson As New JObject()
        rapportJson("dateDebut") = _debutTraitement.ToString("yyyy-MM-dd HH:mm:ss")
        rapportJson("dateFin") = _finTraitement.Value.ToString("yyyy-MM-dd HH:mm:ss")
        rapportJson("dureeTotale") = dureeTraitement.ToString("hh\:mm\:ss")

        ' Détails par type de document
        Dim detailsJson As New JObject()
        For Each kvp As KeyValuePair(Of TypeDocument, CompteurType) In _compteurs
            totalSucces += kvp.Value.Succes
            totalEchecs += kvp.Value.Echecs
            totalAvertissements += kvp.Value.Avertissements

            Dim detail As New JObject()
            detail("succes") = kvp.Value.Succes
            detail("echecs") = kvp.Value.Echecs
            detail("avertissements") = kvp.Value.Avertissements
            detailsJson(kvp.Key.ToString()) = detail
        Next
        rapportJson("detailsParType") = detailsJson

        ' Totaux généraux
        Dim totaux As New JObject()
        totaux("succesTotaux") = totalSucces
        totaux("echecsTotaux") = totalEchecs
        totaux("avertissementsTotaux") = totalAvertissements
        totaux("documentsTraites") = totalSucces + totalEchecs
        rapportJson("totaux") = totaux

        ' Messages détaillés
        Dim messagesJson As New JArray()
        For Each msg As String In _messageGlobal
            messagesJson.Add(msg)
        Next
        rapportJson("messages") = messagesJson

        ' Convertir en JSON avec indentation
        Dim settings As New JsonSerializerSettings With {
            .Formatting = Formatting.Indented
        }
        Dim jsonString As String = JsonConvert.SerializeObject(rapportJson, settings)
        WriteToLog("Rapport JSON de traitement généré.", "INFO")
        Return jsonString
    End Function

    ' Envoyer le rapport par e-mail
    Public Shared Sub EnvoyerRapportParMail(toAddress As String, fromAddress As String, subject As String, smtpServer As String, smtpPort As Integer, smtpUsername As String, smtpPassword As String, Optional includeJsonAttachment As Boolean = True)
        If _debutTraitement = DateTime.MinValue Then
            WriteToLog("Rapport non démarré. Appelez DemarrerRapport() d'abord.", "WARN")
            Return
        End If

        Try
            ' Générer les rapports
            Dim rapportTexte As String = GenererRapport()
            Dim rapportJson As String = GenererRapportJson()

            ' Configurer le client SMTP
            Using client As New SmtpClient(smtpServer, smtpPort)
                client.EnableSsl = True
                client.Credentials = New NetworkCredential(smtpUsername, smtpPassword)

                ' Créer le message e-mail
                Using mail As New MailMessage()
                    mail.From = New MailAddress(fromAddress)
                    mail.To.Add(toAddress)
                    mail.Subject = subject
                    mail.Body = rapportTexte
                    mail.IsBodyHtml = False

                    ' Ajouter le rapport JSON comme pièce jointe si demandé
                    If includeJsonAttachment Then
                        Dim jsonAttachment As New Attachment(New MemoryStream(System.Text.Encoding.UTF8.GetBytes(rapportJson)), "RapportTraitement.json", "application/json")
                        mail.Attachments.Add(jsonAttachment)
                    End If

                    ' Envoyer l'e-mail
                    client.Send(mail)
                    WriteToLog($"Rapport envoyé par e-mail à {toAddress} avec succès.", "INFO")
                End Using
            End Using
        Catch ex As Exception
            WriteToLog($"Erreur lors de l'envoi du rapport par e-mail : {ex.Message}", "ERROR")
        End Try
    End Sub

    ' Méthode pour réinitialiser
    Public Shared Sub Reinitialiser()
        _debutTraitement = DateTime.MinValue
        _finTraitement = Nothing
        _compteurs.Clear()
        _messageGlobal.Clear()
        WriteToLog("Rapport réinitialisé.", "INFO")
    End Sub
End Class