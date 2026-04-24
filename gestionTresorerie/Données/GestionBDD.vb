Public Class GestionBDD

    Public Shared Function SauvegarderBase(dbName As String, dossier As String, connectionString As String) As String
        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim adminService As New SqlAdminService(factory)
        Dim maintenanceService As New DatabaseMaintenanceService(adminService)

        Return maintenanceService.Sauvegarder(dbName, dossier)
    End Function

End Class