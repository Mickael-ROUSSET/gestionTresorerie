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
    Private Const PrepareTableSql As String =
        "DECLARE @lock int; " &
        "EXEC @lock = sys.sp_getapplock @Resource=N'gestionTresorerie-Documents-TEST', " &
        "@LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=10000; " &
        "IF @lock < 0 THROW 51003, N'Initialisation TEST déjà en cours.', 1;" &
        "IF DB_NAME() <> N'bddAgumaaaTest' THROW 51000, N'Catalogue TEST inattendu.', 1;" &
        "IF OBJECT_ID(N'dbo.Documents') IS NULL BEGIN " &
        "CREATE TABLE dbo.Documents (" &
        "idDoc int IDENTITY PRIMARY KEY, dateDoc datetime2 NOT NULL, contenuDoc nvarchar(max) NULL, " &
        "cheminDoc nvarchar(1024) NOT NULL, categorieDoc nvarchar(100) NULL, " &
        "sousCategorieDoc nvarchar(100) NULL, idMvtDoc int NULL, metaDonnees nvarchar(max) NULL); END;"

    Public Sub Initialize()
        LocalDbCatalogs.EnsureTestCatalog()

        Using connection = OpenTest(), transaction = connection.BeginTransaction()
            Using prepare As New SqlCommand(PrepareTableSql, connection, transaction)
                prepare.ExecuteNonQuery()
            End Using
            Using verify As New SqlCommand(GuardSql & ReadSql & " WHERE 1=0;", connection, transaction)
                verify.ExecuteNonQuery()
            End Using
            transaction.Commit()
        End Using
    End Sub

    Public Sub Save(doc As DocumentAgumaaa)
        Using connection = OpenTest(), command = connection.CreateCommand()
            command.CommandText = GuardSql & DocumentRepository.ArchiveInsertSql
            command.Parameters.AddRange(DocumentRepository.InsertionParameters(doc).ToArray())
            command.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ReadDocuments() As DataTable
        Using connection = OpenTest(), command = connection.CreateCommand()
            command.CommandText = GuardSql & ReadSql & " ORDER BY idDoc DESC;"
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
