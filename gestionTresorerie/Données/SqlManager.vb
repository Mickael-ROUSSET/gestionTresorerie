Public Class SqlManager

    Public Shared Sub BackupDatabase(dbName As String, backupPath As String, connectionString As String)
        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim service As New SqlAdminService(factory)
        service.SauvegarderBase(dbName, backupPath)
    End Sub

End Class