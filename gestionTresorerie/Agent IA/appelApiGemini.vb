Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json

Public Class GeminiAnalyzer

    Public Shared Async Function AnalyserDocument(apiKey As String, cheminFichier As String) As Task(Of String)
        ' 1. Construction de l'URL par concaténation simple (SANS $)  chr(58=) = 2 points
        Dim url As String = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash" & Chr(58) & "generateContent?key=" & apiKey

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
            'Dim prompt As String = LireRessourceTexte("gestionTresorerie.PromptGemini.txt")
            ' "gestionTresorerie.PromptGemini.txt" est le nom interne (compilé)
            ' "PromptGemini.txt" est le nom du fichier qui doit être dans votre dossier bin\Debug
            Dim prompt As String = LirePromptOptimise("gestionTresorerie.PromptGemini.txt", "PromptGemini.txt")

            '' 2. Construction requête : utiliser un dictionnaire au lieu d'un objet anonyme (plus stable pour la sérialisation)
            'Dim parts As New List(Of Object)()
            'parts.Add(New With {.text = prompt})
            'parts.Add(New With {.inline_data = New With {.mime_type = mimeType, .data = base64File}})

            '' On sépare l'étape pour éviter l'erreur BC30205
            'Dim premierContenu = New With {.parts = parts.ToArray()}

            '' 3. Créer le corps de la requête (Tableau de contenus)
            '' Utilisation d'un tableau d'objets classique
            'Dim tableauDeContenus() As Object = {premierContenu}

            ' 2. Préparation des composants de la requête
            Dim parts As New List(Of Object) From {
                New Dictionary(Of String, Object) From {{"text", prompt}},
                New Dictionary(Of String, Object) From {
                    {"inline_data", New Dictionary(Of String, Object) From {
                        {"mime_type", mimeType},
                        {"data", base64File}
                    }}
                }
            }

            ' 3. Configuration de la génération (Stabilité + Mode JSON)
            Dim generationConfig As New Dictionary(Of String, Object) From {
                {"temperature", 0}, 'Supprime tout hasard. L'IA devient froide et robotique
                {"topP", 0.1},      'Restreint encore plus le choix aux probabilités de tête
                {"topK", 1},        'L'IA ne choisit que le meilleur candidat. Idéal pour respecter un schéma JSON
                {"maxOutputTokens", 2048},
                {"response_mime_type", "application/json"} ' <--- Force Gemini à répondre en JSON valide
            }

            '4. Assemblage final du corps de la requête
            Dim requestBody As New Dictionary(Of String, Object) From {
                {"contents", New Object() {
                    New Dictionary(Of String, Object) From {{"parts", parts}}
                }},
                {"generationConfig", generationConfig}
            }

            Using client As New HttpClient()
                ' Timeout de 120 secondes pour éviter l'attente infinie
                client.Timeout = TimeSpan.FromMinutes(2)

                Dim jsonPayload = JsonConvert.SerializeObject(requestBody, Formatting.None)
                Dim content = New StringContent(jsonPayload, Encoding.UTF8, "application/json")

                ' Envoi et attente directement pour éviter ambiguité de parsing du compilateur VB
                Debug.WriteLine("Requête envoyée, attente...")
                Dim maReponse As HttpResponseMessage = Await client.PostAsync(url, content)
                Debug.WriteLine("Statut reçu : " & maReponse.StatusCode.ToString())
                If maReponse.StatusCode = Net.HttpStatusCode.BadRequest Then
                    ' LIRE LE MESSAGE D'ERREUR DÉTAILLÉ DE GOOGLE
                    Dim errorDetail As String = Await maReponse.Content.ReadAsStringAsync()
                    Logger.ERR("Détail Erreur 400 : " & errorDetail)
                    MsgBox("Erreur 400 : " & errorDetail) ' Affiche la raison précise (ex: "Invalid model name")
                End If

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
    Public Shared Function LirePromptOptimise(nomRessource As String, nomFichierPhysique As String) As String
        Try
            ' 1. Tentative de lecture du fichier physique (priorité absolue)
            ' On cherche dans le dossier de l'EXE (bin\Debug ou bin\Release)
            Dim cheminPhysique As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomFichierPhysique)

            If File.Exists(cheminPhysique) Then
                ' Optionnel : On peut ajouter un log pour confirmer qu'on utilise le fichier externe
                ' Debug.WriteLine("Utilisation du prompt externe : " & cheminPhysique)
                Return File.ReadAllText(cheminPhysique, System.Text.Encoding.UTF8)
            End If

            ' 2. Si le fichier physique n'existe pas, on bascule sur la ressource compilée
            ' Debug.WriteLine("Fichier externe absent, bascule sur la ressource interne.")
            Return LireRessourceTexte(nomRessource)

        Catch ex As Exception
            Logger.ERR("Impossible de lire le prompt (Physique ou Ressource) : " & ex.Message)
            Return ""
        End Try
    End Function
End Class