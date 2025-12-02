Imports Newtonsoft.Json

Public Class AppelIReseau
    Public Shared Async Sub TestIReseau()
        ' Initialisation auth
        IReseauApi.SetAuth()
        'IReseauApi.SetAuth("uswXFN4VPYfYmrTmNbLY16D3fxFunwjm", "PHPSESSID=82hnsl71742fu7ck1dj2hqsjvt")

        ' URL API
        Dim url As String = "https://i-reseau.ffepgv.fr/api/internal/users-current-context?page=1&itemsPerPage=50"

        'Récupère la liste des inscrits au cours de gymnastique
        Dim users As List(Of UserContext) = Await IReseauApi.GetAllUsersAsync()

        ' Affiche les résultats
        For Each utilisateur In users
            Logger.INFO($"{utilisateur.licenceId } - {utilisateur.FirstName} {utilisateur.LastName}")
            Dim licences = Await GetLicencesAsync(utilisateur)

            For Each item In licences
                Logger.INFO($"Licence : {item.LicenceId}")

                ' Télécharge document principal
                Dim licencePdf = Await DownloadLicenceDocumentAsync(item, utilisateur)

                ' Télécharge reçu fiscal
                Dim recuPdf = Await DownloadRecuFiscalAsync(item, utilisateur)
            Next
        Next
    End Sub

    Public Shared Async Function DownloadLicenceDocumentAsync(item As LicenceDetail, user As UserContext) As Task(Of String)

        If String.IsNullOrEmpty(item.DownloadIdentifier) Then
            Logger.WARN($"DownloadIdentifier absent pour licence {item.LicenceId}")
            Return Nothing
        End If

        Dim url = $"https://i-reseau.ffepgv.fr/adhesions-association/telechargement-{item.DownloadIdentifier}?_oc=uxD3nVjHWLR7&_bck=L3V0aWxpc2F0ZXVycy9saXN0ZS1jb250ZXh0ZS1hY3R1ZWw%3D"

        Dim fileName = $"{user.LastName}_{user.FirstName}_Licence_{item.LicenceId}.pdf".Replace(" ", "_")
        Dim filePath = IO.Path.Combine(LectureProprietes.GetVariable("cheminLicence"), fileName)

        Return Await DownloadPdfAsync(url, filePath)
    End Function
    Public Shared Async Function DownloadRecuFiscalAsync(item As LicenceDetail, user As UserContext) As Task(Of String)

        If String.IsNullOrEmpty(item.TaxDeductionDownloadIdentifier) Then
            Logger.WARN($"TaxDeductionDownloadIdentifier absent pour licence {item.LicenceId}")
            Return Nothing
        End If

        Dim url = $"https://i-reseau.ffepgv.fr/adhesions-association/voir-recu-fiscal-{item.TaxDeductionDownloadIdentifier}?type=print&_oc=uxD3nVjHWLR7&_bck=L3V0aWxpc2F0ZXVycy9saXN0ZS1jb250ZXh0ZS1hY3R1ZWw%3D"

        Dim fileName = $"{user.LastName}_{user.FirstName}_RecuFiscal_{item.LicenceId}.pdf".Replace(" ", "_")
        Dim filePath = IO.Path.Combine(LectureProprietes.GetVariable("cheminRecuFiscal"), fileName)

        Return Await DownloadPdfAsync(url, filePath)
    End Function
End Class
