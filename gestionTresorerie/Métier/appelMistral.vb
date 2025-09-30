
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json.Linq
Imports System.Text.Json

Public Class AppelMistral
    Public Shared Function litImage(chequeImagePath As String, sPrompt As String) As Cheque
        ' Appeler la fonction pour extraire le texte
        Dim extractedText As String = ExtractTextFromImage(chequeImagePath, sPrompt)

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

    Shared Function ExtractTextFromImage(imageFilePath As String, sPrompt As String) As String
        ' Encoder l'image en base64
        Dim base64Image As String = EncodeImageToBase64(imageFilePath)
        Dim imageUrl As String = "data:image/jpeg;base64," & base64Image
        Using client As New HttpClient()
            '            Dim jsonData As String = $"{{""model"": ""pixtral-12b-2409"", 
            '""messages"": [{{""role"": ""user"",""content"": [{{""type"": ""text"",""text"": ""Extrais du chèque en PJ le texte des éléments : emetteur_du_cheque=la banque émettrice en haut de l'image, le montant_numerique=montant numérique dans le cadre en haut à droite, numero_du_cheque=le numéro du chèque en bas à gauche, dateChq=la date à droite de la mention \""Le \"", emetteur_du_cheque=l'émetteur du chèque au centre, le destinataire=destinataire à droite de la mention \""à \"" retourne les éléments extraits au format json""}}, 
            '{{""type"": ""image_url"",""image_url"": ""{image_url}""}}]}}],""max_tokens"": 300}}"
            ''Dim jsonData As String = promptJson(imageUrl)

            ' Créer le contenu de la requête
            Dim content As New StringContent(promptJson(imageUrl, sPrompt), Encoding.UTF8, "application/json")
            ' Créer un contenu multipart/form-data pour l'image
            Using content
                ' Ajouter l'en-tête d'autorisation
                client.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.
                    AuthenticationHeaderValue("Bearer", LectureProprietes.GetVariable("cleApiMistral"))
                Try
                    ' Envoyer la requête POST
                    Dim response As HttpResponseMessage = client.PostAsync(LectureProprietes.GetVariable("urlMistral"), content).Result
                    response.EnsureSuccessStatusCode()

                    ' Lire la réponse
                    Dim responseBody As String = response.Content.ReadAsStringAsync().Result
                    Logger.INFO(responseBody)

                    Return responseBody

                Catch ex As HttpRequestException
                    MsgBox($"Erreur de requête : {ex.Message}")
                    Logger.ERR("ex.Message")
                    Return String.Empty
                End Try
            End Using
        End Using
    End Function
    Private Shared Function promptJson(imageUrl As String, sPrompt As String) As String

        ' Créer un objet anonyme pour structurer les données
        '.model = "pixtral-12b-2409",
        Dim data = New With {
        .model = LectureProprietes.GetVariable("modeleMistral"),
            .messages = New Object() {
                New With {
                    .role = "user",
                    .content = New Object() {
                        New With {
                            .type = "text",
                            .text = sPrompt
                        },
                        New With {
                            .type = "image_url",
                            .image_url = imageUrl
                        }
                    }
                }
            },
            .max_tokens = 300
        }

        ' Sérialiser l'objet en JSON
        Dim jsonData As String = JsonSerializer.Serialize(data)
        Return jsonData
    End Function
    Public Shared Function questionMistral(questionIA As String) As String
        Using client As New HttpClient()
            ' Créer le contenu JSON de la requête
            Dim sModeleMistral As String = LectureProprietes.GetVariable("modeleMistral")
            Dim jsonData As String = $"{{""model"": ""{sModeleMistral}"", ""messages"": [{{""role"": ""user"",""content"": ""{questionIA}""}}]}}"

            ' Créer le contenu de la requête
            Dim content As New StringContent(jsonData, Encoding.UTF8, "application/json")

            ' Ajouter l'en-tête d'autorisation 
            client.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LectureProprietes.GetVariable("cleApiMistral"))

            Try
                ' Envoyer la requête POST de manière synchrone 
                Dim response As HttpResponseMessage = client.PostAsync(LectureProprietes.GetVariable("urlMistral"), content).Result
                response.EnsureSuccessStatusCode()

                ' Lire la réponse de manière synchrone
                Dim responseBody As String = response.Content.ReadAsStringAsync().Result
                Logger.INFO(responseBody)

                ' Désérialiser la réponse JSON
                Dim jsonResponse As JObject = JObject.Parse(responseBody)

                ' Extraire le contenu de la balise "content"
                Dim contentValue As String = jsonResponse("choices")(0)("message")("content").ToString()

                ' Retourner le contenu extrait
                Return contentValue

            Catch ex As HttpRequestException
                MsgBox($"Erreur de requête Mistral : {ex.Message}")
                Logger.ERR($"Erreur de requête Mistral : {ex.Message}")
                Return String.Empty
            Catch ex As Exception
                MsgBox($"Erreur inattendue Mistral : {ex.Message}")
                Logger.ERR($"Erreur inattendue Mistral : {ex.Message}")
                Return String.Empty
            End Try
        End Using
    End Function
End Class
