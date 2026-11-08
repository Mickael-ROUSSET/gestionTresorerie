Option Strict On

Imports System.Data
Imports System.Data.SqlClient

Public NotInheritable Class SqlDocumentTestStore
    Private Const DatabaseName As String = "bddAgumaaaTest"
    Private Const ReadSql As String =
        "SELECT idDoc, dateDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc, metaDonnees FROM dbo.Documents"
    Private Const GuardSql As String =
        "IF DB_NAME() <> N'bddAgumaaaTest' THROW 51000, N'Catalogue TEST inattendu.', 1;" &
        "IF OBJECT_ID(N'dbo.Documents', N'U') IS NULL THROW 51001, N'Table Documents TEST absente.', 1;" &
        "IF EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = OBJECT_ID(N'dbo.Documents')) " &
        "THROW 51002, N'La table Documents TEST comporte des déclencheurs non autorisés.', 1;"
    Private Const MigrationGuardSql As String =
        "IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL " &
        "THROW 51006, N'Historique des migrations TEST absent.', 1;" &
        "IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version=1 " &
        "AND Description=N'Initialisation documentaire TEST') " &
        "OR EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version<>1) " &
        "THROW 51007, N'Historique des migrations TEST invalide.', 1;"

    Private Const InitializeMigrationSql As String =
        "IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NULL BEGIN " &
        "CREATE TABLE dbo.SchemaMigrations (Version int NOT NULL PRIMARY KEY, " &
        "Description nvarchar(200) NOT NULL, AppliedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()); END;" &
        "IF EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version<>1 " &
        "OR (Version=1 AND Description<>N'Initialisation documentaire TEST')) " &
        "THROW 51005, N'Migration TEST incohérente.', 1;" &
        "IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version=1) " &
        "INSERT INTO dbo.SchemaMigrations (Version, Description) " &
        "VALUES (1, N'Initialisation documentaire TEST');"

    Public Sub Initialize()
        LocalDbCatalogs.EnsureTestCatalog()

        Using connection = OpenTest(), transaction = connection.BeginTransaction()
            LocalDbSchema.Apply(connection, transaction, production:=False)
            Using migration As New SqlCommand(InitializeMigrationSql, connection, transaction)
                migration.ExecuteNonQuery()
            End Using
            Using verify As New SqlCommand(GuardSql & MigrationGuardSql & ReadSql & " WHERE 1=0;", connection, transaction)
                verify.ExecuteNonQuery()
            End Using
            transaction.Commit()
        End Using
    End Sub

    Public Sub Save(doc As DocumentAgumaaa)
        Using connection = OpenTest(), command = connection.CreateCommand()
            command.CommandText = GuardSql & MigrationGuardSql & DocumentRepository.ArchiveInsertSql
            command.Parameters.AddRange(DocumentRepository.InsertionParameters(doc).ToArray())
            command.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ReadDocuments() As DataTable
        Using connection = OpenTest(), command = connection.CreateCommand()
            command.CommandText = GuardSql & MigrationGuardSql & ReadSql & " ORDER BY idDoc DESC;"
            Using reader = command.ExecuteReader()
                Dim result As New DataTable()
                result.Load(reader)
                Return result
            End Using
        End Using
    End Function

    Private Shared Function OpenTest() As SqlConnection
        Dim value = LocalDbCatalogs.ConnectionString(DatabaseName)
        TestEnvironmentIsolation.ValidateConnectionString(value)
        Dim connection As New SqlConnection(value)
        Try
            connection.Open()
            If Not String.Equals(connection.Database, DatabaseName, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Le catalogue SQL TEST attendu n’est pas actif.")
            End If
            Return connection
        Catch
            connection.Dispose()
            Throw
        End Try
    End Function

End Class
