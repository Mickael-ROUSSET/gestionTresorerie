Imports System.IO
Imports Newtonsoft.Json

Public Class TestMistral

    Private Shared ReadOnly stateFile As String = "mistral_state.json"

    Private Class MistralState
        Public Property FileId As String
        Public Property AgentId As String
        Public Property CsvLastWriteTime As Date
    End Class

    Private Shared Sub SaveState(state As MistralState)
        Dim json = JsonConvert.SerializeObject(state, Formatting.Indented)
        File.WriteAllText(stateFile, json)
    End Sub

    Private Shared Function LoadState() As MistralState
        If Not File.Exists(stateFile) Then Return Nothing
        Dim json = File.ReadAllText(stateFile)
        Return JsonConvert.DeserializeObject(Of MistralState)(json)
    End Function

    Public Shared Sub ResetState()
        Try
            If File.Exists(stateFile) Then
                File.Delete(stateFile)
                Logger.INFO("🗑️ Fichier d’état supprimé (reset local effectué).")
            End If
        Catch ex As Exception
            Logger.ERR("Erreur lors du reset de l’état : " & ex.Message)
        End Try
    End Sub

    '------------------------------------------------------------
    ' 🤖 Création ou rechargement intelligent d’un agent Mistral
    '------------------------------------------------------------
    Public Shared Async Function CreeAgent(Optional forceRecreation As Boolean = False) As Task(Of String)
        Try
            Dim cheminCsv = LectureProprietes.GetVariable("ficLstTiers")
            Dim csvLastWrite = File.GetLastWriteTimeUtc(cheminCsv)

            Dim state As MistralState = Nothing
            Dim fileId As String = Nothing
            Dim agentId As String = Nothing

            ' 1️⃣ Charger l’état précédent si on ne force pas
            If Not forceRecreation Then
                state = LoadState()
                If state IsNot Nothing Then
                    fileId = state.FileId
                    agentId = state.AgentId
                    Logger.INFO($"📄 Identifiants chargés : → FileId = {fileId} → AgentId = {agentId}")

                    ' 🔍 Vérifie si le CSV a été modifié
                    If state.CsvLastWriteTime <> csvLastWrite Then
                        Logger.INFO("⚠️ Le fichier CSV a été modifié → recréation de l’agent nécessaire.")
                        forceRecreation = True
                    Else
                        ' 🔍 Vérifie si l’agent existe toujours côté serveur
                        Dim existe = Await MistralApi.AgentExisteAsync(agentId)
                        If Not existe Then
                            Logger.INFO("⚠️ L’agent n’existe plus sur le serveur → recréation nécessaire.")
                            forceRecreation = True
                        End If
                    End If
                End If
            End If

            ' 2️⃣ Si recréation forcée → supprimer l’état local
            If forceRecreation Then
                If File.Exists(stateFile) Then
                    File.Delete(stateFile)
                    Logger.INFO("♻️ Recréation forcée : fichier d’état supprimé.")
                End If
                fileId = Nothing
                agentId = Nothing
            End If

            ' 3️⃣ Upload du CSV si nécessaire
            If String.IsNullOrEmpty(fileId) Then
                fileId = Await MistralApi.UploadCsvAsync(cheminCsv)
                Logger.INFO("📁 Fichier CSV envoyé → " & fileId)
            End If

            ' 4️⃣ Création de l’agent si nécessaire
            If String.IsNullOrEmpty(agentId) Then
                agentId = Await MistralApi.CreerAgentAsync(fileId)
                Logger.INFO("🤖 Agent créé → " & agentId)
            End If

            ' 5️⃣ Sauvegarde mise à jour avec date du CSV
            SaveState(New MistralState With {
                .FileId = fileId,
                .AgentId = agentId,
                .CsvLastWriteTime = csvLastWrite
            })

            ' ✅ Retourne l’identifiant de l’agent
            Return agentId

        Catch ex As Exception
            Logger.ERR("❌ Erreur dans CreeAgent : " & ex.Message)
            Return Nothing
        End Try
    End Function

End Class
