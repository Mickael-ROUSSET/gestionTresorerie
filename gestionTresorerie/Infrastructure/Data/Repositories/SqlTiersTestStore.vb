Option Strict On

Imports System
Imports System.Data
Imports System.Data.SqlClient

''' <summary>Écrit uniquement dans bddAgumaaaTest. Aucun appel au dépôt historique PROD.</summary>
Public NotInheritable Class SqlTiersTestStore
    Private Const Catalog As String = "bddAgumaaaTest"

    Private Shared Function OpenTest() As SqlConnection
        Dim cs = LocalDbCatalogs.ConnectionString(Catalog)
        TestEnvironmentIsolation.ValidateConnectionString(cs)
        Dim connection As New SqlConnection(cs)
        Try
            connection.Open()
            If Not String.Equals(connection.Database, Catalog, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Catalogue TEST inattendu.")
            End If
            Return connection
        Catch
            connection.Dispose()
            Throw
        End Try
    End Function

    Public Sub Initialize()
        Dim documents As New SqlDocumentTestStore()
        documents.Initialize()
    End Sub

    ''' <summary>Crée atomiquement un tiers fictif et ses coordonnées, ou ne crée rien.</summary>
    Public Function CreateTiers(nom As String, email As String, ville As String) As Integer
        If String.IsNullOrWhiteSpace(nom) OrElse nom.Trim().Length > 50 Then
            Throw New ArgumentException("Nom TEST obligatoire (50 caractères maximum).", NameOf(nom))
        End If
        If Not Coordonnees.ValiderEmail(email) Then
            Throw New ArgumentException("Adresse électronique TEST invalide.", NameOf(email))
        End If
        If ville IsNot Nothing AndAlso ville.Trim().Length > 50 Then
            Throw New ArgumentException("Ville TEST trop longue.", NameOf(ville))
        End If

        Using connection = OpenTest(), transaction = connection.BeginTransaction()
            Try
                Using command As New SqlCommand(
                    "IF DB_NAME()<>N'bddAgumaaaTest' THROW 51000,N'Catalogue TEST inattendu.',1;" &
                    "IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version=1 AND Description=N'Initialisation documentaire TEST') " &
                    "OR EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version<>1) THROW 51007,N'Migrations TEST invalides.',1;" &
                    "INSERT dbo.Tiers(nom,dateCreation,dateModification) VALUES(@nom,GETDATE(),GETDATE());" &
                    "SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction)
                    command.Parameters.Add("@nom", SqlDbType.NVarChar, 50).Value = nom.Trim()
                    Dim id = CInt(command.ExecuteScalar())
                    Using address As New SqlCommand(
                        "INSERT dbo.Coordonnees(IdTiers,Email,Ville) VALUES(@id,@email,@ville);", connection, transaction)
                        address.Parameters.Add("@id", SqlDbType.Int).Value = id
                        address.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value =
                            If(String.IsNullOrWhiteSpace(email), CType(DBNull.Value, Object), email.Trim())
                        address.Parameters.Add("@ville", SqlDbType.NVarChar, 50).Value =
                            If(String.IsNullOrWhiteSpace(ville), CType(DBNull.Value, Object), ville.Trim())
                        address.ExecuteNonQuery()
                    End Using
                    transaction.Commit()
                    Return id
                End Using
            Catch
                transaction.Rollback()
                Throw
            End Try
        End Using
    End Function

    ''' <summary>Corrige atomiquement le nom et les coordonnées d'un tiers TEST existant.</summary>
    Public Sub UpdateTiers(id As Integer, nom As String, email As String, ville As String)
        If id <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(id))
        If String.IsNullOrWhiteSpace(nom) OrElse nom.Trim().Length > 50 Then
            Throw New ArgumentException("Nom TEST obligatoire (50 caractères maximum).", NameOf(nom))
        End If
        If Not Coordonnees.ValiderEmail(email) Then
            Throw New ArgumentException("Adresse électronique TEST invalide.", NameOf(email))
        End If
        If ville IsNot Nothing AndAlso ville.Trim().Length > 50 Then
            Throw New ArgumentException("Ville TEST trop longue.", NameOf(ville))
        End If

        Using connection = OpenTest(), transaction = connection.BeginTransaction()
            Try
                Using command As New SqlCommand(
                    "IF DB_NAME()<>N'bddAgumaaaTest' THROW 51000,N'Catalogue TEST inattendu.',1;" &
                    "IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version=1 AND Description=N'Initialisation documentaire TEST') " &
                    "OR EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version<>1) THROW 51007,N'Migrations TEST invalides.',1;" &
                    "UPDATE dbo.Tiers SET nom=@nom,dateModification=GETDATE() WHERE Id=@id;" &
                    "IF @@ROWCOUNT<>1 THROW 51008,N'Tiers TEST introuvable.',1;" &
                    "UPDATE dbo.Coordonnees SET Email=@email,Ville=@ville WHERE IdTiers=@id;" &
                    "IF @@ROWCOUNT<>1 THROW 51009,N'Coordonnees TEST absentes ou multiples.',1;", connection, transaction)
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id
                    command.Parameters.Add("@nom", SqlDbType.NVarChar, 50).Value = nom.Trim()
                    command.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value =
                        If(String.IsNullOrWhiteSpace(email), CType(DBNull.Value, Object), email.Trim())
                    command.Parameters.Add("@ville", SqlDbType.NVarChar, 50).Value =
                        If(String.IsNullOrWhiteSpace(ville), CType(DBNull.Value, Object), ville.Trim())
                    command.ExecuteNonQuery()
                End Using
                transaction.Commit()
            Catch
                transaction.Rollback()
                Throw
            End Try
        End Using
    End Sub

    ''' <summary>Recherche des tiers fictifs par nom, sans exposer le catalogue de production.</summary>
    Public Function SearchTiers(nomPartiel As String) As DataTable
        If String.IsNullOrWhiteSpace(nomPartiel) Then Throw New ArgumentException("Recherche TEST obligatoire.", NameOf(nomPartiel))
        If nomPartiel.Trim().Length > 50 Then Throw New ArgumentException("Recherche TEST trop longue.", NameOf(nomPartiel))
        Using connection = OpenTest(), command = connection.CreateCommand()
            command.CommandText =
                "IF DB_NAME()<>N'bddAgumaaaTest' THROW 51000,N'Catalogue TEST inattendu.',1;" &
                "IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version=1 AND Description=N'Initialisation documentaire TEST') " &
                "OR EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version<>1) THROW 51007,N'Migrations TEST invalides.',1;" &
                "SELECT TOP (100) t.Id,t.nom,c.Email,c.Ville FROM dbo.Tiers t " &
                "LEFT JOIN dbo.Coordonnees c ON c.IdTiers=t.Id " &
                "WHERE t.nom LIKE @pattern ESCAPE N'~' ORDER BY t.nom,t.Id,c.Id;"
            Dim escaped = nomPartiel.Trim().Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "[[]")
            command.Parameters.Add("@pattern", SqlDbType.NVarChar, 110).Value = "%" & escaped & "%"
            Using reader = command.ExecuteReader()
                Dim result As New DataTable()
                result.Load(reader)
                Return result
            End Using
        End Using
    End Function

    Public Function ReadTiers(id As Integer) As DataTable
        If id <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(id))
        Using connection = OpenTest(), command = connection.CreateCommand()
            command.CommandText =
                "IF DB_NAME()<>N'bddAgumaaaTest' THROW 51000,N'Catalogue TEST inattendu.',1;" &
                "IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version=1 AND Description=N'Initialisation documentaire TEST') " &
                "OR EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version<>1) THROW 51007,N'Migrations TEST invalides.',1;" &
                "SELECT t.Id,t.nom,c.Email,c.Ville FROM dbo.Tiers t " &
                "LEFT JOIN dbo.Coordonnees c ON c.IdTiers=t.Id WHERE t.Id=@id ORDER BY c.Id;"
            command.Parameters.Add("@id", SqlDbType.Int).Value = id
            Using reader = command.ExecuteReader()
                Dim result As New DataTable()
                result.Load(reader)
                Return result
            End Using
        End Using
    End Function
End Class
