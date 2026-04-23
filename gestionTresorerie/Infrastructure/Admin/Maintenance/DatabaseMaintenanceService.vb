Imports System.IO

Public Class DatabaseMaintenanceService

    Private ReadOnly _sqlAdminService As SqlAdminService

    Public Sub New(sqlAdminService As SqlAdminService)
        If sqlAdminService Is Nothing Then
            Throw New ArgumentNullException(NameOf(sqlAdminService))
        End If

        _sqlAdminService = sqlAdminService
    End Sub

    Public Function ConstruireNomBackup(dbName As String, dossier As String) As String
        If String.IsNullOrWhiteSpace(dbName) Then
            Throw New ArgumentException("Le nom de base est obligatoire.", NameOf(dbName))
        End If

        If String.IsNullOrWhiteSpace(dossier) Then
            Throw New ArgumentException("Le dossier de sauvegarde est obligatoire.", NameOf(dossier))
        End If

        If Not Directory.Exists(dossier) Then
            Directory.CreateDirectory(dossier)
        End If

        Dim nomFichier As String = dbName & "_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".bak"
        Return Path.Combine(dossier, nomFichier)
    End Function

    Public Function Sauvegarder(dbName As String, dossier As String) As String
        Dim cheminBackup As String = ConstruireNomBackup(dbName, dossier)
        _sqlAdminService.SauvegarderBase(dbName, cheminBackup)
        Return cheminBackup
    End Function

End Class