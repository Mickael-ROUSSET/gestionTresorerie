Public Class AppelIReseau
    Public Shared Async Sub TestIReseau()
        ' Initialisation auth
        IReseauApi.SetAuth()
        'IReseauApi.SetAuth("uswXFN4VPYfYmrTmNbLY16D3fxFunwjm", "PHPSESSID=82hnsl71742fu7ck1dj2hqsjvt")

        ' URL API
        Dim url As String = "https://i-reseau.ffepgv.fr/api/internal/users-current-context?page=1&itemsPerPage=50"

        ' GET
        'Dim users As List(Of UserContext) = Await IReseauApi.GetAsync(Of List(Of UserContext))(url)
        Dim users As List(Of UserContext) = Await IReseauApi.GetAllUsersAsync()

        ' Affiche les résultats
        For Each u In users
            Console.WriteLine($"{u.licenceId } - {u.FirstName} {u.LastName}")
        Next
    End Sub
End Class
