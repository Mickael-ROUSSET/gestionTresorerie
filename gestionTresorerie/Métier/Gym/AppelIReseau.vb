Imports System.Data.SqlClient
Imports System.IO

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
            Logger.INFO($"{utilisateur.licenceId} - {utilisateur.FirstName} {utilisateur.LastName}")
            Dim licences = Await GetLicencesAsync(utilisateur)

            For Each item In licences
                Logger.INFO($"Licence : {item.LicenceId}")

                ' Télécharge document principal
                Dim licencePdf = Await DownloadLicenceDocumentAsync(item, utilisateur)

                ' Télécharge reçu fiscal
                Dim recuPdf = Await DownloadRecuFiscalAsync(item, utilisateur)
            Next
        Next
        MsgBox($"Traitement terminé. {users.Count} utilisateurs traités")
    End Sub

    Public Shared Async Function DownloadLicenceDocumentAsync(item As LicenceDetail, user As UserContext) As Task(Of String)

        If String.IsNullOrEmpty(item.DownloadIdentifier) Then
            Logger.WARN($"DownloadIdentifier absent pour licence {item.LicenceId}")
            Return Nothing
        End If

        Dim url = $"https://i-reseau.ffepgv.fr/adhesions-association/telechargement-{item.DownloadIdentifier}?_oc=uxD3nVjHWLR7&_bck=L3V0aWxpc2F0ZXVycy9saXN0ZS1jb250ZXh0ZS1hY3R1ZWw%3D"

        Dim fileName = $"{user.LastName}_{user.FirstName}_Licence_{item.LicenceId}.pdf".Replace(" ", "_")
        Dim filePath = IO.Path.Combine(repDocGym("Licences"), fileName)

        ' Télécharger le PDF puis mettre à jour la date en base si succès
        Dim result = Await DownloadPdfAsync(url, filePath)

        If Not String.IsNullOrEmpty(result) Then
            ' Mettre à jour la date de mise à jour de la licence dans la table AdherantsGym (on suppose que la colonne existe)
            MettreAJourDateLicenceAsync(user, item.LicenceId, DateTime.Now)
        End If

        Return result
    End Function
    Public Shared Async Function DownloadRecuFiscalAsync(item As LicenceDetail, user As UserContext) As Task(Of String)

        If String.IsNullOrEmpty(item.TaxDeductionDownloadIdentifier) Then
            Logger.WARN($"TaxDeductionDownloadIdentifier absent pour licence {item.LicenceId}")
            Return Nothing
        End If

        Dim url = $"https://i-reseau.ffepgv.fr/adhesions-association/voir-recu-fiscal-{item.TaxDeductionDownloadIdentifier}?type=print&_oc=uxD3nVjHWLR7&_bck=L3V0aWxpc2F0ZXVycy9saXN0ZS1jb250ZXh0ZS1hY3R1ZWw%3D"

        Dim fileName = $"{user.LastName}_{user.FirstName}_RecuFiscal_{item.LicenceId}.pdf".Replace(" ", "_")
        Dim filePath = IO.Path.Combine(repDocGym("RecusFiscaux"), fileName)

        Return Await DownloadPdfAsync(url, filePath)
    End Function

    Private Shared Function MettreAJourDateLicenceAsync(user As UserContext, licenceId As Integer, dateMaj As DateTime)
        Try
            'Récupération de l'idTiers à partir du nom, prénom et date de naissance
            Dim idTiers As Integer? = Tiers.GetIdTiersByUser(user.FirstName, user.LastName, user.BirthDate)
            If Not idTiers.HasValue Then
                Logger.WARN($"Impossible de récupérer IdTiers pour la licence {licenceId}. La date de mise à jour ne sera pas effectuée.")
                Return False
            End If
            ' Mise à jour
            Dim param As New Dictionary(Of String, Object) From {
            {"@dateMaj", dateMaj},
            {"@licenceId", licenceId},
            {"@idTiers", idTiers.Value}
        }
            Using cmdUpdate = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "updDateMajLicence", param)
                Dim rowsAffected = cmdUpdate.ExecuteNonQuery()
                If rowsAffected = 0 Then
                    Logger.WARN($"Aucune ligne mise à jour pour LicenceId={licenceId} dans AdherantsGym.")
                End If

                Return rowsAffected > 0
            End Using
        Catch ex As Exception
            Logger.ERR($"MettreAJourDateLicenceAsync({licenceId}) : {ex.Message}")
            Return False
        End Try
    End Function

    Private Shared Function repDocGym(sTypeDoc As String) As String
        'G:\Mon Drive\AGUMAAA\Documents\Manifestations récurrentes\Activités\Gym\2024-2025\Licences
        Dim sRepDestination As String
        Dim anneeEnCours As Integer = DateTime.Now.Year
        Dim anneeSuivante As Integer = anneeEnCours + 1

        sRepDestination = Path.Combine(LectureProprietes.GetVariable("repRacineAgumaaa"),
                                LectureProprietes.GetVariable("repRacineDocuments"),
                                LectureProprietes.GetVariable("repFichiersGym"),
                                anneeEnCours.ToString & "-" & anneeSuivante.ToString, sTypeDoc
                                )
        Return sRepDestination
    End Function
End Class
