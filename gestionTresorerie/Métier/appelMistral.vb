
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports Newtonsoft.Json.Linq

Public Class AppelMistral
    Public Shared Function litImage(document As ITypeDoc) As String
        ' Appeler la fonction pour extraire le texte
        Dim extractedText As String = ExtractTextFromImage(document)

        Return extractedText
    End Function

    Public Shared Function ExtractTextFromImage(document As ITypeDoc) As String
        Using client As New HttpClient()
            ' Créer le contenu de la requête
            Dim content As New StringContent(promptJson(document), Encoding.UTF8, "application/json")
            ' Créer un contenu multipart/form-data pour l'image
            Using content
                ' Ajouter l'en-tête d'autorisation
                client.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.
                    AuthenticationHeaderValue("Bearer", LectureProprietes.GetVariable("cleApiMistral"))
                Try
                    ' Envoyer la requête POST
                    Dim response As HttpResponseMessage = client.PostAsync(LectureProprietes.GetVariable("urlMistral"), content).Result
                    Dim unused1 = response.EnsureSuccessStatusCode()

                    ' Lire la réponse
                    Dim responseBody As String = response.Content.ReadAsStringAsync().Result
                    Logger.INFO(responseBody)

                    Return responseBody

                Catch ex As HttpRequestException
                    Logger.ERR($"Erreur de requête : {ex.Message}")
                    Return String.Empty
                End Try
            End Using
        End Using
    End Function
    Private Shared Function promptJson(document As ITypeDoc) As String

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
                            .text = document.Prompt
                        },
                        New With {
                            .type = "image_url",
                            .image_url = "data:image/jpeg;base64," & document.ContenuBase64
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
                Dim unused2 = response.EnsureSuccessStatusCode()

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
                Dim unused1 = MsgBox($"Erreur de requête Mistral : {ex.Message}")
                Logger.ERR($"Erreur de requête Mistral : {ex.Message}")
                Return String.Empty
            Catch ex As Exception
                Dim unused = MsgBox($"Erreur inattendue Mistral : {ex.Message}")
                Logger.ERR($"Erreur inattendue Mistral : {ex.Message}")
                Return String.Empty
            End Try
        End Using
    End Function
End Class
