
Imports System.IO
Imports System.Net.Http
Imports System.Text

Public Class appelMistral

    '' URL de l'API Mistral 
    'Dim apiUrl As String = "https://api.mistral.ai/v1/chat/completions"

    ' URL de l'API Mistral pour l'extraction de texte
    ReadOnly apiUrlImage As String = "https://api.mistral.ai/extract-text"

    'Prompt système
    'TODO : à mettre dans un fichier paramètre yaml
    ReadOnly promptSysteme As String =
"Tu es une IA spécialisée dans l'analyse de documents. 
Ta tâche est d’extraire du chèque en PJ le texte des éléments 
la banque émettrice en haut de l'image
le montant numérique dans le cadre en haut à droite
le numéro du chèque en bas à gauche
la date à droite de la mention ""Le ""
l'émetteur du chèque au centre
le destinataire à droite de la mention ""à ""
écrit les éléments extraits au format json"
    ReadOnly question = "Extrais du chèque en PJ le texte des éléments : emetteur_du_cheque=la banque émettrice en haut de l'image, le montant_numerique=montant numérique dans le cadre en haut à droite, numero_du_cheque=le numéro du chèque en bas à gauche, dateChq=la date à droite de la mention \""Le \"", emetteur_du_cheque=l'émetteur du chèque au centre, le destinataire=destinataire à droite de la mention \""à \"" retourne les éléments extraits au format json"
    'Prompt demande d'analyse du chèque
    ReadOnly promptAnalyse As String = "Analyse l'image du chèque pour en extraire le texte"

    Public Shared Function litImage(chequeImagePath As String) As Cheque
        ' Clé API Mistral
        Dim valCle = New cléApiMistral
        Dim apiKey As String = valCle.getCle
        Dim apiUrl As String = "https://api.mistral.ai/v1/chat/completions"

        ' Appeler la fonction pour extraire le texte
        Dim extractedText As String = ExtractTextFromImage(apiUrl, chequeImagePath, apiKey)

        ' Afficher le texte extrait 
        Dim jsonChq As New Cheque(extractedText)
        Return jsonChq
    End Function

    Shared Function ConvertirImageEnBase64(cheminImage As String) As String
        Dim imageBytes As Byte() = File.ReadAllBytes(cheminImage)
        Return Convert.ToBase64String(imageBytes)
    End Function

    Public Shared Function EncodeImageToBase64(filePath As String) As String
        ' Lire le fichier image en tant que tableau d'octets
        Dim imageBytes As Byte() = File.ReadAllBytes(filePath)
        ' Convertir le tableau d'octets en une chaîne base64
        Return Convert.ToBase64String(imageBytes)
    End Function

    Shared Function ExtractTextFromImage(apiUrl As String, imageFilePath As String, apiKey As String) As String
        ' Chemin du fichier image
        'Dim imageFilePath As String = "C:\Users\User\Downloads\SKM_C25825030514310.pdf"

        ' Encoder l'image en base64
        Dim base64Image As String = EncodeImageToBase64(imageFilePath)
        Dim image_url As String = "data:image/jpeg;base64," & base64Image
        Using client As New HttpClient()

            ' Lire l'image en tant que tableau d'octets 
            Dim jsonData As String = $"{{""model"": ""pixtral-12b-2409"", 
""messages"": [{{""role"": ""user"",""content"": [{{""type"": ""text"",""text"": ""Extrais du chèque en PJ le texte des éléments : emetteur_du_cheque=la banque émettrice en haut de l'image, le montant_numerique=montant numérique dans le cadre en haut à droite, numero_du_cheque=le numéro du chèque en bas à gauche, dateChq=la date à droite de la mention \""Le \"", emetteur_du_cheque=l'émetteur du chèque au centre, le destinataire=destinataire à droite de la mention \""à \"" retourne les éléments extraits au format json""}}, 
{{""type"": ""image_url"",""image_url"": ""{image_url}""}}]}}],""max_tokens"": 300}}"

            ' Créer le contenu de la requête
            Dim content As New StringContent(
                jsonData,
                Encoding.UTF8,
                "application/json"
            )
            ' Créer un contenu multipart/form-data pour l'image
            Using content
                ' Ajouter l'en-tête d'autorisation
                client.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey)

                Try
                    ' Envoyer la requête POST
                    Dim response As HttpResponseMessage = client.PostAsync(apiUrl, content).Result
                    response.EnsureSuccessStatusCode()

                    ' Lire la réponse
                    Dim responseBody As String = response.Content.ReadAsStringAsync().Result
                    Logger.GetInstance().INFO(responseBody)

                    ' Supposons que la réponse soit un JSON contenant le texte extrait
                    ' Vous pouvez utiliser JsonConvert pour désérialiser si nécessaire
                    Return responseBody

                Catch ex As HttpRequestException
                    MsgBox($"Erreur de requête : {ex.Message}")
                    Logger.GetInstance().ERR("ex.Message")
                    Return String.Empty
                End Try
            End Using
        End Using
    End Function

    Shared Function trouveTiers(apiUrl As String, questionIA As String, apiKey As String) As String

        Using client As New HttpClient()

            ' Lire l'image en tant que tableau d'octets 
            Dim jsonData As String = $"{{""model"": ""pixtral-12b-2409"", ""messages"": [{{""role"": ""user"",""content"": {{""type"": ""text"",""Content""{questionIA}""kk""}}]}}"

            ' Créer le contenu de la requête
            Dim content As New StringContent(
                jsonData,
                Encoding.UTF8,
                "application/json"
            )
            ' Créer un contenu multipart/form-data pour l'image
            Using content
                ' Ajouter l'en-tête d'autorisation
                client.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey)

                Try
                    ' Envoyer la requête POST
                    Dim response As HttpResponseMessage = client.PostAsync(apiUrl, content).Result
                    response.EnsureSuccessStatusCode()

                    ' Lire la réponse
                    Dim responseBody As String = response.Content.ReadAsStringAsync().Result
                    Logger.GetInstance().INFO(responseBody)

                    ' Supposons que la réponse soit un JSON contenant le texte extrait
                    ' Vous pouvez utiliser JsonConvert pour désérialiser si nécessaire
                    Return responseBody

                Catch ex As HttpRequestException
                    MsgBox($"Erreur de requête : {ex.Message}")
                    Logger.GetInstance().ERR("ex.Message")
                    Return String.Empty
                End Try
            End Using
        End Using
    End Function
End Class
