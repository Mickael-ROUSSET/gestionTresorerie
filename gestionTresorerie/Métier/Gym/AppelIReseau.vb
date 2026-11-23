Imports System.Data.SqlClient
Imports System.IO

Public Class AppelIReseau
    Private Shared Function CreateRepository() As GymRepository
        Dim executor As ISqlExecutor = RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New GymRepository(executor)
    End Function
    Public Shared Async Function Executer(licences As Boolean, recus As Boolean) As Task
        IReseauApi.SetAuth()
        Dim users=Await IReseauApi.GetAllUsersAsync()
        If Not licences AndAlso Not recus Then
            Using liste As New Form With {.Text="Inscrits à la gymnastique",.Width=700,.Height=500},
                  grille As New DataGridView With {.Dock=DockStyle.Fill,.ReadOnly=True,.DataSource=users.Select(Function(u) New With {.Nom=u.LastName,.Prenom=u.FirstName,.Licence=u.licenceId}).ToList()}
                liste.Controls.Add(grille)
                liste.ShowDialog()
            End Using
            Return
        End If
        Dim nombre As Integer
        For Each inscrit In users
            For Each item In Await GetLicencesAsync(inscrit)
                If licences AndAlso Not String.IsNullOrEmpty(Await DownloadLicenceDocumentAsync(item,inscrit)) Then nombre+=1
                If recus AndAlso Not String.IsNullOrEmpty(Await DownloadRecuFiscalAsync(item,inscrit)) Then nombre+=1
            Next
        Next
        MessageBox.Show(nombre.ToString() & " document(s) téléchargé(s).","Gymnastique")
    End Function

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
            Dim rowsAffected As Integer = CreateRepository().MettreAJourDateMajLicence(idTiers, Date.Now)

            Logger.INFO($"Date de mise à jour licence mise à jour pour IdTiers={idTiers}, lignes={rowsAffected}")
            If rowsAffected = 0 Then
                    Logger.WARN($"Aucune ligne mise à jour pour LicenceId={licenceId} dans AdherantsGym.")
                End If

            Return rowsAffected > 0
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
