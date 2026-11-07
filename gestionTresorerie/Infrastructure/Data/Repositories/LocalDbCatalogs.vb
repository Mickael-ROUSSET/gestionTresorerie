Option Strict On

Imports System.Data
Imports System.Data.SqlClient

Friend NotInheritable Class LocalDbCatalogs
    Private Const InstanceName As String = "(LocalDB)\MSSQLLocalDB"

    Private Sub New()
    End Sub

    Friend Shared Sub EnsureTestCatalog()
        EnsureCatalog("bddAgumaaaTest", "CREATE DATABASE [bddAgumaaaTest];")
    End Sub

    Friend Shared Sub EnsureProductionCatalog()
        EnsureCatalog("bddAgumaaa", "CREATE DATABASE [bddAgumaaa];")
    End Sub

    Private Shared Sub EnsureCatalog(catalog As String, createSql As String)
        Using connection As New SqlConnection(ConnectionString("master"))
            connection.Open()
            Using command = connection.CreateCommand()
                command.CommandText = "SELECT DB_ID(@catalog);"
                command.Parameters.Add("@catalog", SqlDbType.NVarChar, 128).Value = catalog
                If command.ExecuteScalar() IsNot DBNull.Value Then Return

                command.CommandText = createSql
                command.Parameters.Clear()
                Try
                    command.ExecuteNonQuery()
                Catch ex As SqlException When ex.Number = 1801
                    ' Une autre initialisation a créé le même catalogue entre-temps.
                End Try
            End Using
        End Using
    End Sub

    Friend Shared Function ConnectionString(catalog As String) As String
        Return New SqlConnectionStringBuilder With {
            .DataSource = InstanceName, .InitialCatalog = catalog, .IntegratedSecurity = True,
            .ConnectTimeout = 30, .ApplicationName = "gestionTresorerie LocalDB"
        }.ConnectionString
    End Function
End Class
