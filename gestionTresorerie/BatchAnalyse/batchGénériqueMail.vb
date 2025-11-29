Imports System.IO
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Public Class BatchMailSender

    Private Function ExtractDestinataires(nomRequete As String,
                                          params As Dictionary(Of String, Object)) As DataTable

        Dim dt As New DataTable()
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, nomRequete, params)
            Using rdr = cmd.ExecuteReader()
                dt.Load(rdr)
            End Using
        End Using
        Return dt
    End Function

    Private Shared Function ApplyReplacements(template As String, dr As DataRow, mapping As Dictionary(Of String, String)) As String

        Dim result = template
        For Each kvp In mapping
            Dim placeholder = kvp.Key
            Dim col = kvp.Value

            Dim value As String = ""
            If dr.Table.Columns.Contains(col) AndAlso dr(col) IsNot DBNull.Value Then
                value = dr(col).ToString()
            End If

            result = result.Replace(placeholder, value)
        Next

        Return result
    End Function

    Public Function RunBatch(config As BatchMailConfig) As BatchMailReport
        Dim report As New BatchMailReport With {
            .DateDebut = Now,
            .NomRequete = config.NomRequeteSQL,
            .Parametres = config.ParametresRequeteSQL
        }

        Try
            ' Charger modèle
            Dim templateHtml As String = IO.File.ReadAllText(config.CheminTemplateCorpsEmail)

            ' Extraction destinataires
            Dim dt = ExtractDestinataires(config.NomRequeteSQL, config.ParametresRequeteSQL)
            report.Total = dt.Rows.Count

            For Each dr As DataRow In dt.Rows
                Dim mailDestinataire As String = dr(config.ColonneEmail).ToString().Trim()
                If String.IsNullOrWhiteSpace(mailDestinataire) Then
                    Logger.WARN("Aucun email pour : ")
                    Continue For
                End If
                Dim result As New DestinataireResult With {.Adresse = mailDestinataire}

                Try
                    ' Sujet + corps
                    Dim sujet = ApplyReplacements(config.SujetMail, dr, config.RplctSujetMail)
                    Dim corps = ApplyReplacements(templateHtml, dr, config.RplctSujetMail)

                    Dim piecesJointes As New List(Of String)
                    piecesJointes = generePJ(config, dr)

                    ' Créer une instance du gestionnaire de mails
                    Dim mailer As New MailManager()
                    ' Liste des fichiers à joindre 
                    If mailer.EnvoyerEmail(mailDestinataire, sujet, corps, piecesJointes) Then
                        Logger.INFO($"E-mailDestinataire avec pièces jointes envoyé avec succès à {mailDestinataire}")
                    Else
                        Logger.INFO($"Échec de l'envoi de l'e-mailDestinataire à {mailDestinataire}.")
                    End If
                    result.IsSuccess = True
                    report.SuccessCount += 1

                Catch ex As Exception
                    result.IsSuccess = False
                    result.ErrorMessage = ex.Message
                    report.FailCount += 1
                End Try

                report.Results.Add(result)
            Next

            report.DateFin = Now

            ' Sauvegarde rapport
            If Not String.IsNullOrEmpty(config.CheminRapportTrt) Then
                IO.File.WriteAllText(config.CheminRapportTrt & ".txt", report.ToText())
                IO.File.WriteAllText(config.CheminRapportTrt & ".json", report.ToJson())
            End If
            envoieReport(report.ToString, config)
            MsgBox($"BatchMailSender terminé pour {config.NomRequeteSQL}")
            Return report

        Catch ex As Exception
            Logger.ERR("BatchMailSender erreur : " & ex.Message)
            Return report
        End Try
    End Function
    Private Sub envoieReport(sRapport As String, config As BatchMailConfig)

        Dim mailer As New MailManager()
        Dim lstFic As List(Of String) = New List(Of String)
        lstFic.Add(config.CheminRapportTrt & ".txt")
        ' Liste des fichiers à joindre 
        mailer.EnvoyerEmail("mickael_rousset@hotmail.com", "Rapport de batch générique", sRapport, lstFic)
    End Sub
    Private Function generePJ(config As BatchMailConfig, dr As DataRow) As List(Of String)
        ' ------- Génération des pièces jointes -------
        Dim listePj As New List(Of String)

        For Each modelePJ In config.CheminsPjRessource
            If Not File.Exists(modelePJ) Then Continue For ' Construire le dictionnaire réel à partir du DataRow
            Dim dicoRemplacementsPJ As New Dictionary(Of String, String)

            For Each kvp In config.RplctPJ
                Dim placeholder As String = kvp.Key
                Dim nomColonne As String = kvp.Value

                If dr.Table.Columns.Contains(nomColonne) Then
                    Dim valeur As String = If(dr(nomColonne)?.ToString(), "")
                    dicoRemplacementsPJ.Add(placeholder, valeur)
                Else
                    Logger.WARN($"Colonne inconnue dans la PJ : {nomColonne}")
                    dicoRemplacementsPJ.Add(placeholder, "") ' Éviter l'erreur et laisser vide
                End If
            Next

            ' Applique les remplacements sur une copie du modèle PJ
            Dim pjGeneree As String = UtilitaireDocx.RemplaceTexteDocx(modelePJ, dicoRemplacementsPJ, dr)

            listePj.Add(pjGeneree)
        Next
        Return listePj
    End Function
End Class
