Imports System.IO

Public Class BatchMailSender
    Private Shared Function CreateRepository() As BatchAnalyseRepository
        Dim factory As IConnectionFactory =
        RepositoryFactory.CreateConnectionFactory(Constantes.DataBases.Agumaaa)

        Dim provider As ISqlTextProvider = RepositoryFactory.CreateSqlTextProvider()

        Return New BatchAnalyseRepository(factory, provider)
    End Function
    Private Function ExtractDestinataires(nomRequete As String,
                                      params As Dictionary(Of String, Object)) As DataTable

        Return CreateRepository().ExtraireDataTable(nomRequete, params)
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

    Friend Function Destinataires(config As BatchMailConfig, cs As String) As DataTable
        If Not File.Exists(config.CheminTemplateCorpsEmail) Then Throw New FileNotFoundException("Modèle de courrier introuvable.", config.CheminTemplateCorpsEmail)
        For Each pj In config.CheminsPjRessource
            If Not File.Exists(pj) Then Throw New FileNotFoundException("Pièce jointe introuvable.", pj)
        Next
        Dim repo As New BatchAnalyseRepository(New AgumaaaConnectionFactory(cs), RepositoryFactory.CreateSqlTextProvider())
        If Not AssistantBatchCourrier.Requetes().Contains(config.NomRequeteSQL) Then Throw New ArgumentException("Choisissez une requête de lecture du catalogue.")
        Dim dt = repo.ExtraireDataTable(config.NomRequeteSQL, config.ParametresRequeteSQL)
        For Each col In config.RplctSujetMail.Values.Concat(config.RplctCorpsMail.Values).Concat(config.RplctPJ.Values).Append(config.ColonneEmail)
            If Not dt.Columns.Contains(col) Then Throw New ArgumentException("Colonne absente de la requête : " & col)
        Next
        Return dt
    End Function

    Public Function RunBatch(config As BatchMailConfig, Optional destinataires As DataTable = Nothing) As BatchMailReport
        Dim report As New BatchMailReport With {
            .NomBatch = config.NomBatch,
            .Description = config.Description,
            .DateDebut = Now,
            .NomRequete = config.NomRequeteSQL,
            .Parametres = config.ParametresRequeteSQL
        }

        Try
            ' Charger modèle
            Dim templateHtml As String = IO.File.ReadAllText(config.CheminTemplateCorpsEmail)

            ' Extraction destinataires
            Dim dt = If(destinataires, ExtractDestinataires(config.NomRequeteSQL, config.ParametresRequeteSQL))
            report.Total = dt.Rows.Count

            For Each dr As DataRow In dt.Rows
                Dim mailDestinataire As String = dr(config.ColonneEmail).ToString().Trim()
                If String.IsNullOrWhiteSpace(mailDestinataire) Then
                    report.FailCount += 1
                    report.Results.Add(New DestinataireResult With {.IsSuccess = False, .ErrorMessage = "Adresse électronique absente."})
                    Continue For
                End If
                Dim result As New DestinataireResult With {.Adresse = mailDestinataire}

                Try
                    ' Sujet + corps
                    Dim sujet = ApplyReplacements(config.SujetMail, dr, config.RplctSujetMail)
                    Dim corps = ApplyReplacements(templateHtml, dr, config.RplctCorpsMail)

                    Dim piecesJointes As New List(Of String)
                    piecesJointes = generePJ(config, dr)

                    ' Créer une instance du gestionnaire de mails
                    Dim mailer As New MailManager()
                    ' Liste des fichiers à joindre 
                    If mailer.EnvoyerEmail(mailDestinataire, sujet, corps, piecesJointes) Then
                        Logger.INFO($"E-mailDestinataire avec pièces jointes envoyé avec succès à {mailDestinataire}")
                    Else
                        Throw New InvalidOperationException("Envoi SMTP refusé.")
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

            Return report

        Catch ex As Exception
            Logger.ERR("BatchMailSender erreur : " & ex.Message)
            Throw
        End Try
    End Function
    Private Function generePJ(config As BatchMailConfig, dr As DataRow) As List(Of String)
        ' ------- Génération des pièces jointes -------
        Dim listePj As New List(Of String)

        For Each modelePJ In config.CheminsPjRessource
            If Not File.Exists(modelePJ) Then Throw New FileNotFoundException("Pièce jointe introuvable.", modelePJ)
            If Not String.Equals(Path.GetExtension(modelePJ), ".docx", StringComparison.OrdinalIgnoreCase) OrElse config.RplctPJ.Count = 0 Then
                listePj.Add(modelePJ)
                Continue For
            End If
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