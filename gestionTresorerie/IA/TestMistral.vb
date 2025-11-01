Imports System.IO
Imports Newtonsoft.Json

Public Class TestMistral

    Private Shared ReadOnly stateFile As String = LectureProprietes.GetVariable("ficAgentMistral")

    Private Class MistralState
        Public Property FileId As String
        Public Property AgentId As String
    End Class

    '------------------------------------------------------------
    ' 🔹 Sauvegarder l’état localement
    '------------------------------------------------------------
    Private Shared Sub SaveState(state As MistralState)
        Dim json = JsonConvert.SerializeObject(state, Formatting.Indented)
        File.WriteAllText(stateFile, json)
    End Sub

    '------------------------------------------------------------
    ' 🔹 Charger l’état local
    '------------------------------------------------------------
    Private Shared Function LoadState() As MistralState
        If Not File.Exists(stateFile) Then Return Nothing
        Dim json = File.ReadAllText(stateFile)
        Return JsonConvert.DeserializeObject(Of MistralState)(json)
    End Function

    '------------------------------------------------------------
    ' 🔹 Fonction principale
    '------------------------------------------------------------
    Public Shared Async Sub TestAnalyse(cheminImage As String)
        Try
            Dim cheminCsv = LectureProprietes.GetVariable("ficLstTiers")
            'Dim cheminImage = LectureProprietes.GetVariable("ficExempleImage")

            ' Charger l’état précédent s’il existe
            Dim state = LoadState()
            Dim fileId As String = Nothing
            Dim agentId As String = Nothing

            If state IsNot Nothing Then
                fileId = state.FileId
                agentId = state.AgentId
                Logger.INFO("📄 Identifiants chargés :   → FileId  = {fileId}   → AgentId = {agentId}")
            End If

            ' 1️ Upload du CSV si aucun FileId connu
            If String.IsNullOrEmpty(fileId) Then
                fileId = Await MistralApi.UploadCsvAsync(cheminCsv)
                Logger.INFO("📁 Fichier CSV envoyé → " & fileId)
            End If

            ' 2️ Création de l’agent si aucun AgentId connu
            If String.IsNullOrEmpty(agentId) Then
                agentId = Await MistralApi.CreerAgentAsync(fileId)
                Logger.INFO("🤖 Agent créé → " & agentId)
            End If

            ' 3️ Sauvegarder pour les prochaines exécutions
            SaveState(New MistralState With {.FileId = fileId, .AgentId = agentId})

            ' 4️ Conversion de l’image en Base64
            Dim imageBytes = File.ReadAllBytes(cheminImage)
            Dim imageBase64 = Convert.ToBase64String(imageBytes)

            ' 5️ Analyse de l’image
            Logger.INFO("🔍 Analyse en cours...")
            Dim resultat = Await MistralApi.AnalyserImageAsync(agentId, imageBase64)
            Logger.INFO("✅ Résultat :")
            Logger.INFO(resultat)

        Catch ex As Exception
            Logger.ERR("❌ Erreur : " & ex.Message)
        End Try
    End Sub

End Class
