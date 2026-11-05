Option Strict On
Option Explicit On

Imports System
Imports System.Data.SqlClient
Imports System.IO

' Validation sans création de dossier, lecture de fichier ni ouverture SQL.
Public NotInheritable Class TestEnvironmentIsolation
    Private Sub New()
    End Sub

    Public Shared Function GetInstallationDirectory(productionPath As String,
                                                    testPath As String) As String
        Dim production = NormalizeDirectory(productionPath)
        Dim testing = NormalizeDirectory(testPath)
        If testing.StartsWith("\\", StringComparison.Ordinal) Then
            Throw New InvalidOperationException("Le dossier TEST doit être local.")
        End If
        If IsWithin(testing, production) OrElse IsWithin(production, testing) Then
            Throw New InvalidOperationException(
                "Les dossiers TEST et PROD doivent être distincts et sans imbrication.")
        End If
        Return testing & Path.DirectorySeparatorChar
    End Function

    Public Shared Function GetFilePath(productionPath As String, testPath As String,
                                       relativePath As String) As String
        Dim root = GetInstallationDirectory(productionPath, testPath)
        If String.IsNullOrWhiteSpace(relativePath) OrElse
           Path.IsPathRooted(relativePath) OrElse relativePath.Contains(":") Then
            Throw New InvalidOperationException("Un chemin relatif au dossier TEST est obligatoire.")
        End If
        Dim result = Path.GetFullPath(Path.Combine(root, relativePath))
        If Not result.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Le fichier doit rester dans le dossier TEST.")
        End If
        Return result
    End Function

    Public Shared Sub ValidateConnectionString(connectionString As String)
        Dim configuration As SqlConnectionStringBuilder
        Try
            configuration = New SqlConnectionStringBuilder(connectionString)
        Catch ex As ArgumentException
            Throw New InvalidOperationException("Configuration SQL TEST invalide.")
        End Try

        If Not String.Equals(configuration.DataSource, "(LocalDB)\MSSQLLocalDB",
                             StringComparison.OrdinalIgnoreCase) OrElse
           Not String.Equals(configuration.InitialCatalog, "bddAgumaaaTest",
                             StringComparison.OrdinalIgnoreCase) OrElse
           Not configuration.IntegratedSecurity OrElse
           Not String.IsNullOrEmpty(configuration.AttachDBFilename) OrElse
           Not String.IsNullOrEmpty(configuration.UserID) OrElse
           Not String.IsNullOrEmpty(configuration.Password) Then
            Throw New InvalidOperationException(
                "TEST exige la base locale bddAgumaaaTest avec authentification Windows, sans attachement MDF.")
        End If
    End Sub

    Private Shared Function NormalizeDirectory(value As String) As String
        If String.IsNullOrWhiteSpace(value) OrElse Not Path.IsPathFullyQualified(value) Then
            Throw New InvalidOperationException("Les dossiers TEST et PROD doivent être des chemins absolus.")
        End If
        Return Path.TrimEndingDirectorySeparator(Path.GetFullPath(value))
    End Function

    Private Shared Function IsWithin(candidate As String, root As String) As Boolean
        Return String.Equals(candidate, root, StringComparison.OrdinalIgnoreCase) OrElse
               candidate.StartsWith(Path.TrimEndingDirectorySeparator(root) &
                                    Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
    End Function
End Class
