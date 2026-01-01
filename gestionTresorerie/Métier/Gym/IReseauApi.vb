Imports System.Net.Http
Imports System.Net.Http.Headers
Imports Newtonsoft.Json

Public Module IReseauApi

    Private ReadOnly client As HttpClient

    Sub New()
        client = New HttpClient()

        ' En-têtes communs
        client.DefaultRequestHeaders.Accept.Clear()
        client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        client.DefaultRequestHeaders.Add("cache-control", "max-age=0, must-revalidate, privat")
        client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br, zstd")
        client.DefaultRequestHeaders.Add("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6")
    End Sub

    ''' <summary>
    ''' Initialise l’authentification Cookie
    ''' </summary>
    Public Sub SetAuth(Optional cookie As String = Nothing)

        ' --- Demande du cookie à l'utilisateur si absent ---
        If String.IsNullOrWhiteSpace(cookie) Then
            cookie = InputBox("Veuillez saisir le cookie nécessaire à l'authentification :", "Cookie requis")
        End If

        ' --- Si l'utilisateur n'a rien saisi, on stoppe proprement ---
        If String.IsNullOrWhiteSpace(cookie) Then
            Throw New InvalidOperationException("Aucun cookie fourni : authentification impossible.")
        End If

        ' --- Ajout du cookie ---
        If client.DefaultRequestHeaders.Contains("Cookie") Then
            client.DefaultRequestHeaders.Remove("Cookie")
        End If

        client.DefaultRequestHeaders.Add("Cookie", cookie)
    End Sub


    ''' <summary>
    ''' Requête GET générique
    ''' </summary>
    Public Async Function GetAsync(Of T)(url As String) As Task(Of T)
        Dim response = Await client.GetAsync(url)
        response.EnsureSuccessStatusCode()

        Dim json As String = Await response.Content.ReadAsStringAsync()
        Return JsonConvert.DeserializeObject(Of T)(json)
    End Function


    ''' <summary>
    ''' Requête POST générique avec multipart/form-data
    ''' </summary>
    Public Async Function PostAsync(Of T)(url As String, formData As Dictionary(Of String, String)) As Task(Of T)
        Using content As New MultipartFormDataContent()
            For Each kvp In formData
                content.Add(New StringContent(kvp.Value), kvp.Key)
            Next
            Dim response = Await client.PostAsync(url, content)
            response.EnsureSuccessStatusCode()
            Dim json As String = Await response.Content.ReadAsStringAsync()
            Return JsonConvert.DeserializeObject(Of T)(json)
        End Using
    End Function
    Public Async Function GetAllUsersAsync() As Task(Of List(Of UserContext))
        Dim allUsers As New List(Of UserContext)()
        Dim page As Integer = 1
        Dim itemsPerPage As Integer = 50
        'organizationsContexts.accessRightsTemplate.ref=nonea
        Dim filtreUser As String = "organizationsContexts.accessRightsTemplate.ref"
        Dim valeurFiltre As String = "nonea"
        Dim hasMore As Boolean = True

        While hasMore
            ' Construire l'URL avec pagination
            Dim url As String = $"https://i-reseau.ffepgv.fr/api/internal/users-current-context?page={page}&itemsPerPage={itemsPerPage}&{filtreUser}={valeurFiltre}"

            ' Récupérer la page courante 
            Dim monUserPage As UserPage = Await GetAsync(Of UserPage)(url)

            Dim users As List(Of UserContext) = monUserPage.Items

            ' Ajouter les utilisateurs à la liste complète
            allUsers.AddRange(users)

            ' Vérifier si on a reçu moins d'éléments que demandé → dernière page
            If allUsers.Count < itemsPerPage Then
                hasMore = False
            Else
                page += 1
            End If
        End While

        Return allUsers
    End Function

    Public Async Function GetLicencesForUserAsync(uref As String) As Task(Of List(Of LicenceDetail))
        Dim url = $"https://i-reseau.ffepgv.fr/api/internal/association-members-licences-all-for-user?uref={uref}&order[licence.season.title]=DESC"

        Dim raw As String = Await client.GetStringAsync(url)
        Dim json As String = UtilitaireJson.CleanJson(raw)

        Return JsonConvert.DeserializeObject(Of List(Of LicenceDetail))(json)
    End Function
    Public Async Function GetLicencesAsync(user As UserContext) As Task(Of List(Of LicenceDetail))
        ' 1. Validation des entrées (Fail-fast)
        If user Is Nothing OrElse String.IsNullOrWhiteSpace(user.Ref) Then
            Logger.WARN("GetLicencesAsync: UserContext ou Référence utilisateur invalide.")
            Return New List(Of LicenceDetail)()
        End If

        Try
            ' 2. Appel API direct (évite l'instanciation inutile d'une liste intermédiaire)
            ' .ConfigureAwait(False) est recommandé pour les tâches de bibliothèque/backend
            Dim licences = Await IReseauApi.GetLicencesForUserAsync(user.Ref).ConfigureAwait(False)

            ' 3. Retourne le résultat ou une liste vide si l'API renvoie Nothing
            Return If(licences, New List(Of LicenceDetail)())

        Catch ex As Exception
            ' 4. Gestion centralisée des erreurs
            Logger.ERR($"Erreur lors de la récupération des licences pour {user.Ref} : {ex.Message}")

            ' On retourne une liste vide pour éviter de faire planter l'UI avec un Nothing
            Return New List(Of LicenceDetail)()
        End Try
    End Function
    Public Async Function DownloadPdfAsync(url As String, outputFile As String) As Task(Of String)

        Try
            Dim bytes = Await client.GetByteArrayAsync(url)

            Dim folder = IO.Path.GetDirectoryName(outputFile)
            IO.Directory.CreateDirectory(folder)

            Await IO.File.WriteAllBytesAsync(outputFile, bytes)

            Logger.INFO($"Téléchargé : {outputFile}")
            Return outputFile

        Catch ex As Exception
            Logger.ERR($"Erreur téléchargement {url} : {ex.Message}")
            Return Nothing
        End Try

    End Function
End Module
