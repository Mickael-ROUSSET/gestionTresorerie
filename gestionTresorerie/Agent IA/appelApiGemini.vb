Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports ZXing.Client

Public Class GeminiAnalyzer

    Public Shared Async Function AnalyserDocument(apiKey As String, cheminFichier As String) As Task(Of String)
        ' 1. Construction de l'URL par concaténation simple (SANS $)  chr(58=) = 2 points
        Dim url As String = "https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash" & Chr(58) & "generateContent?key=" & apiKey

        Try
            ' 1. Vérification fichier
            If Not File.Exists(cheminFichier) Then
                Logger.ERR($"Fichier introuvable pour AnalyserDocument : {cheminFichier}")
                Return Nothing
            End If

            Dim bytes = File.ReadAllBytes(cheminFichier)
            Dim base64File = Convert.ToBase64String(bytes)
            Dim extension = Path.GetExtension(cheminFichier).ToLower()
            Dim mimeType = If(extension = ".pdf", "application/pdf", "image/jpeg")
            ' Prompt unique qui demande l'identification ET l'extraction
            Dim prompt As String = LireRessourceTexte("gestionTresorerie.PromptGemini.txt")

            ' 2. Construction requête : utiliser un dictionnaire au lieu d'un objet anonyme (plus stable pour la sérialisation)
            Dim parts As New List(Of Object)()
            parts.Add(New With {.text = prompt})
            parts.Add(New With {.inline_data = New With {.mime_type = mimeType, .data = base64File}})

            ' On sépare l'étape pour éviter l'erreur BC30205
            Dim premierContenu = New With {.parts = parts.ToArray()}

            ' 3. Créer le corps de la requête (Tableau de contenus)
            ' Utilisation d'un tableau d'objets classique
            Dim tableauDeContenus() As Object = {premierContenu}

            Dim requestBody = New With {.contents = tableauDeContenus}

            Using client As New HttpClient()
                ' Timeout de 120 secondes pour éviter l'attente infinie
                client.Timeout = TimeSpan.FromMinutes(2)

                Dim jsonPayload = JsonConvert.SerializeObject(requestBody, Formatting.None)
                Dim content = New StringContent(jsonPayload, Encoding.UTF8, "application/json")

                ' Envoi et attente directement pour éviter ambiguité de parsing du compilateur VB
                Debug.WriteLine("Requête envoyée, attente...")
                Dim maReponse As HttpResponseMessage = Await client.PostAsync(url, content)
                Debug.WriteLine("Statut reçu : " & maReponse.StatusCode.ToString())

                ' Lire le contenu de la réponse
                Dim jsonFinal As String = Await maReponse.Content.ReadAsStringAsync()

                ' Vérifier succès HTTP
                If maReponse.IsSuccessStatusCode Then
                    If String.IsNullOrEmpty(jsonFinal) Then
                        Logger.ERR("API Gemini a renvoyé une réponse vide.")
                        Return Nothing
                    End If

                    Return NettoyerReponseGemini(jsonFinal)
                Else
                    Logger.ERR($"API Gemini HTTP {maReponse.StatusCode} : {jsonFinal}")
                    Return Nothing
                End If
            End Using

        Catch ex As Exception
            Logger.ERR($"Exception dans AnalyserDocument : {ex.Message}")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Nettoie la réponse pour ne garder que le JSON pur (Version robuste)
    ''' </summary>
    Private Shared Function NettoyerReponseGemini(rawResponse As String) As String
        Try
            Dim dynamicRes = JsonConvert.DeserializeObject(Of Object)(rawResponse)
            Dim textResult As String = dynamicRes("candidates")(0)("content")("parts")(0)("text").ToString()
            ' Nettoyage des balises Markdown ```json ... ```
            Return textResult.Replace("```json", "").Replace("```", "").Trim()
        Catch ex As Exception
            Logger.ERR($"erreur: Echec du nettoyage JSON : {ex.Message}")
            Return "{""erreur"": ""Echec du nettoyage JSON""}"
        End Try
    End Function

    Private Shared Function LireRessourceTexte(nom As String) As String
        Dim asm = GetType(GeminiAnalyzer).Assembly

        Using stream = asm.GetManifestResourceStream(nom)
            If stream Is Nothing Then
                Logger.ERR($"Ressource intégrée introuvable : {nom}")
                Return String.Empty
            End If
            Using reader As New StreamReader(stream, Encoding.UTF8)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function
End Class