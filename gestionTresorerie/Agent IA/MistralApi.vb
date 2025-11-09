Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.IO
Imports Newtonsoft.Json

Public Class MistralApi

    Private Shared ReadOnly apiKey As String = "sk-xxxxxx" ' ← remplace par ta clé Mistral
    Private Shared ReadOnly baseUrl As String = "https://api.mistral.ai/v1"

    '------------------------------------------------------------
    ' 📤 UPLOAD DU FICHIER CSV DANS LA LIBRARY MISTRAL
    '------------------------------------------------------------
    Public Shared Async Function UploadCsvAsync(fichierLocal As String) As Task(Of String)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.Authorization =
                New AuthenticationHeaderValue("Bearer", apiKey)

            Using form As New MultipartFormDataContent()
                Dim fileBytes = File.ReadAllBytes(fichierLocal)
                Dim fileContent As New ByteArrayContent(fileBytes)
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv")

                form.Add(fileContent, "file", Path.GetFileName(fichierLocal))
                form.Add(New StringContent("library"), "purpose")

                'N'existe pas chez Mistral
                Dim response = Await client.PostAsync($"{baseUrl}/files", form)
                Dim result = Await response.Content.ReadAsStringAsync()

                If Not response.IsSuccessStatusCode Then
                    Throw New Exception($"Erreur upload : {response.StatusCode} → {result}")
                End If

                Dim json = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(result)
                Return json("id").ToString()
            End Using
        End Using
    End Function
    '------------------------------------------------------------
    ' 🤖 CRÉATION D’UN AGENT AVEC LE FICHIER EN BIBLIOTHÈQUE
    '------------------------------------------------------------
    Public Shared Async Function CreerAgentAsync(fichierId As String) As Task(Of String)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.Authorization =
                New AuthenticationHeaderValue("Bearer", apiKey)

            Dim payload = New With {
                .name = "AgentAnalyseImage",
                .instructions = "Analyse les images encodées en base64 et utilise lstTiers.csv pour identifier des correspondances avec les tiers connus.",
                .model = "mistral-large-latest",
                .library = New String() {fichierId}
            }

            Dim json = JsonConvert.SerializeObject(payload)
            Dim content = New StringContent(json, Encoding.UTF8, "application/json")

            Dim response = Await client.PostAsync($"{baseUrl}/agents", content)
            Dim result = Await response.Content.ReadAsStringAsync()

            If Not response.IsSuccessStatusCode Then
                Throw New Exception($"Erreur création agent : {response.StatusCode} → {result}")
            End If

            Dim data = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(result)
            Return data("id").ToString()
        End Using
    End Function

    '------------------------------------------------------------
    ' 🧠 ANALYSE D’UNE IMAGE BASE64 PAR L’AGENT
    '------------------------------------------------------------
    Public Shared Async Function AnalyserImageAsync(agentId As String, imageBase64 As String) As Task(Of String)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.Authorization =
                New AuthenticationHeaderValue("Bearer", apiKey)

            Dim payload = New With {
                .agent_id = agentId,
                .input = New Object() {
                    New With {
                        .role = "user",
                        .content = New Object() {
                            New With {.type = "text", .text = "Détecte le type de document dans l'image avec la mention caractéristique puis extrais les informations pertinentes : chèque caractérisé par la mention 'Payez contre ce chèque', formulaire d'adhésion dont le titre en haut est 'Bulletin d'adhésion', questionnaire de santé dont le titre est 'Questionnaire de santé EPGV', facture reçue par l'AGUMAAA, ordonnance qui mentionne un docteur. Indique les informations pertinentes selon le type de document : nom d'usage, prénoms, date de naissance, email, numéro de téléphone, numéro de chèque, adresse, ville et code postal, nom de naissance dans un json, prescripteur, nombre de chèque, montant de chacun et autres selon la cas. Formatte la sortie en json avec un champ qui indique le type de document détecté et trouve les correspondances avec lstTiers.csv"},
                            New With {.type = "image", .data = imageBase64}
                        }
                    }
                }
            }

            Dim json = JsonConvert.SerializeObject(payload)
            Dim content = New StringContent(json, Encoding.UTF8, "application/json")

            Dim response = Await client.PostAsync($"{baseUrl}/agents/complete", content)
            Dim result = Await response.Content.ReadAsStringAsync()

            If Not response.IsSuccessStatusCode Then
                Throw New Exception($"Erreur analyse : {response.StatusCode} → {result}")
            End If

            Dim data = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(result)
            Return data("output_text").ToString()
        End Using
    End Function
    '------------------------------------------------------------
    ' 🕵️‍♂️ Vérifie si un agent Mistral existe encore côté serveur
    '------------------------------------------------------------
    Public Shared Async Function AgentExisteAsync(agentId As String) As Task(Of Boolean)
        If String.IsNullOrEmpty(agentId) Then Return False

        Using client As New HttpClient()
            client.DefaultRequestHeaders.Authorization =
            New AuthenticationHeaderValue("Bearer", apiKey)

            Dim response = Await client.GetAsync($"{baseUrl}/agents/{agentId}")

            ' Si l’API retourne 404 ou autre → l’agent n’existe plus
            If Not response.IsSuccessStatusCode Then
                Return False
            End If

            Return True
        End Using
    End Function

End Class
