Option Strict On

Imports System.Data.SqlClient
Imports System.IO

Friend NotInheritable Class LocalDbSchema
    Private Sub New()
    End Sub

    Friend Shared Sub Apply(connection As SqlConnection, transaction As SqlTransaction, production As Boolean)
        Dim expected = If(production, "bddAgumaaa", "bddAgumaaaTest")
        If Not String.Equals(connection.Database, expected, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Catalogue inattendu lors de l’initialisation du schéma.")
        End If

        Using command = connection.CreateCommand()
            command.Transaction = transaction
            command.CommandText =
                "DECLARE @lock int; " &
                "EXEC @lock = sys.sp_getapplock @Resource=N'gestionTresorerie-Schema', " &
                "@LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=10000; " &
                "IF @lock < 0 THROW 51003, N'Initialisation du schéma déjà en cours.', 1;"
            command.ExecuteNonQuery()

            If production Then
                ' Une base de production déjà structurée est hors du périmètre de l’installateur.
                command.CommandText =
                    "SELECT TOP (1) 1 FROM sys.objects WHERE is_ms_shipped = 0;"
                If command.ExecuteScalar() IsNot Nothing Then Return
            End If

            Using stream = GetType(LocalDbSchema).Assembly.GetManifestResourceStream("gestionTresorerie.Schema.001.sql")
                If stream Is Nothing Then Throw New InvalidOperationException("Schéma SQL embarqué introuvable.")
                Using reader As New StreamReader(stream)
                    command.CommandText = reader.ReadToEnd()
                End Using
            End Using
            command.ExecuteNonQuery()
            If Not production Then
                command.CommandText = "IF COL_LENGTH('dbo.Tiers','nomUsage') IS NULL ALTER TABLE dbo.Tiers ADD nomUsage nvarchar(50) NULL;" &
                                      "IF COL_LENGTH('dbo.Tiers','genre') IS NULL ALTER TABLE dbo.Tiers ADD genre nvarchar(30) NULL;"
                command.ExecuteNonQuery()
                command.CommandText = "IF OBJECT_ID('dbo.ClassementDocumentTest','U') IS NULL CREATE TABLE dbo.ClassementDocumentTest(idDoc int NOT NULL PRIMARY KEY,nature nvarchar(100) NULL,evenement nvarchar(100) NULL);"
                command.ExecuteNonQuery()
            End If
        End Using
    End Sub
End Class
