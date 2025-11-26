Imports System.Data.SqlClient

Public Class AdresseDAO

    Private ReadOnly ConnectionStringConstant As String = Constantes.bddAgumaaa

    '-------------------------------------------------------------------------
    ' Méthode d'aide pour l'exécution simple (Update/Delete)
    '-------------------------------------------------------------------------
    Private Function ExecuterCommande(ByVal sqlQuery As String, ByVal parameters As SqlParameter()) As Integer
        Dim lignesAffectees As Integer = 0

        Using command As SqlCommand = SqlCommandBuilder.CreateSqlCommand(ConnectionStringConstant, sqlQuery)

            If parameters IsNot Nothing Then
                For Each p As SqlParameter In parameters
                    command.Parameters.Add(p)
                Next
            End If

            Try
                command.Connection.Open()
                lignesAffectees = command.ExecuteNonQuery()
            Catch ex As Exception
                Throw New Exception("Erreur lors de l'exécution de la commande SQL.", ex)
            Finally
                If command.Connection IsNot Nothing AndAlso command.Connection.State = Data.ConnectionState.Open Then
                    command.Connection.Close()
                End If
            End Try
        End Using
        Return lignesAffectees
    End Function

    '-------------------------------------------------------------------------
    ' 1. CRÉER (Create)
    '-------------------------------------------------------------------------
    Public Function CreerAdresse(ByVal adresse As Coordonnees) As Integer
        Dim sql As String = "INSERT INTO [dbo].[Coordonnées] " &
                            "([IdTiers], [Rue1], [Rue2], [CodePostal], [NomCommune], [Pays], [EstPrincipale]) " &
                            "VALUES (@IdTiers, @Rue1, @Rue2, @CodePostal, @NomCommune, @Pays, @EstPrincipale); " &
                            "SELECT CAST(SCOPE_IDENTITY() AS INT);"

        Dim parameters() As SqlParameter = {
            New SqlParameter("@IdTiers", adresse.IdTiers),
            New SqlParameter("@Rue1", If(String.IsNullOrEmpty(adresse.Rue1), CObj(DBNull.Value), adresse.Rue1)),
            New SqlParameter("@Rue2", If(String.IsNullOrEmpty(adresse.Rue2), CObj(DBNull.Value), adresse.Rue2)),
            New SqlParameter("@CodePostal", If(String.IsNullOrEmpty(adresse.CodePostal), CObj(DBNull.Value), adresse.CodePostal)),
            New SqlParameter("@NomCommune", If(String.IsNullOrEmpty(adresse.NomCommune), CObj(DBNull.Value), adresse.NomCommune)),
            New SqlParameter("@Pays", If(String.IsNullOrEmpty(adresse.Pays), CObj(DBNull.Value), adresse.Pays))
        }

        Dim newId As Integer = 0
        Using command As SqlCommand = SqlCommandBuilder.CreateSqlCommand(ConnectionStringConstant, sql)
            If parameters IsNot Nothing Then
                For Each p As SqlParameter In parameters
                    command.Parameters.Add(p)
                Next
            End If

            Try
                command.Connection.Open()
                ' ExecuteScalar pour récupérer l'ID généré
                newId = Convert.ToInt32(command.ExecuteScalar())
                adresse.Id = newId
            Catch ex As Exception
                Throw New Exception("Erreur lors de l'insertion de l'adresse.", ex)
            Finally
                If command.Connection IsNot Nothing AndAlso command.Connection.State = Data.ConnectionState.Open Then
                    command.Connection.Close()
                End If
            End Try
        End Using

        Return newId
    End Function

    '-------------------------------------------------------------------------
    ' 2. LIRE (Read)
    '-------------------------------------------------------------------------
    Public Function LireAdressesParTiers(ByVal idTiers As Integer) As List(Of Coordonnees)
        Dim adresses As New List(Of Coordonnees)
        Dim sql As String = "SELECT * FROM [dbo].[Coordonnées] WHERE IdTiers = @IdTiers ORDER BY EstPrincipale DESC, Id ASC"

        Dim parameters() As SqlParameter = {New SqlParameter("@IdTiers", idTiers)}

        Using command As SqlCommand = SqlCommandBuilder.CreateSqlCommand(ConnectionStringConstant, sql)
            For Each p As SqlParameter In parameters
                command.Parameters.Add(p)
            Next

            Try
                command.Connection.Open()

                Using reader As SqlDataReader = command.ExecuteReader()
                    While reader.Read()
                        ' Mapping des données
                        Dim adresse As New Coordonnees With {
                            .Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            .IdTiers = reader.GetInt32(reader.GetOrdinal("IdTiers")),
                            .Rue1 = If(reader.IsDBNull(reader.GetOrdinal("Rue1")), Nothing, reader.GetString(reader.GetOrdinal("Rue1"))),
                            .Rue2 = If(reader.IsDBNull(reader.GetOrdinal("Rue2")), Nothing, reader.GetString(reader.GetOrdinal("Rue2"))),
                            .CodePostal = If(reader.IsDBNull(reader.GetOrdinal("CodePostal")), Nothing, reader.GetString(reader.GetOrdinal("CodePostal"))),
                            .NomCommune = If(reader.IsDBNull(reader.GetOrdinal("NomCommune")), Nothing, reader.GetString(reader.GetOrdinal("NomCommune"))),
                            .Pays = If(reader.IsDBNull(reader.GetOrdinal("Pays")), Nothing, reader.GetString(reader.GetOrdinal("Pays")))
                        }
                        adresses.Add(adresse)
                    End While
                End Using
            Catch ex As Exception
                Throw New Exception("Erreur lors de la lecture des adresses.", ex)
            Finally
                If command.Connection IsNot Nothing AndAlso command.Connection.State = Data.ConnectionState.Open Then
                    command.Connection.Close()
                End If
            End Try
        End Using
        Return adresses
    End Function

    '-------------------------------------------------------------------------
    ' 3. METTRE À JOUR (Update)
    '-------------------------------------------------------------------------
    Public Function MettreAJourAdresse(ByVal adresse As Coordonnees) As Boolean
        Dim sql As String = "UPDATE [dbo].[Coordonnées] SET " &
                            "Rue1 = @Rue1, Rue2 = @Rue2, " &
                            "CodePostal = @CodePostal, NomCommune = @NomCommune, Pays = @Pays, " &
                            "EstPrincipale = @EstPrincipale " &
                            "WHERE Id = @Id AND IdTiers = @IdTiers"

        Dim parameters() As SqlParameter = {
            New SqlParameter("@Id", adresse.Id),
            New SqlParameter("@IdTiers", adresse.IdTiers),
            New SqlParameter("@Rue1", If(String.IsNullOrEmpty(adresse.Rue1), CObj(DBNull.Value), adresse.Rue1)),
            New SqlParameter("@Rue2", If(String.IsNullOrEmpty(adresse.Rue2), CObj(DBNull.Value), adresse.Rue2)),
            New SqlParameter("@CodePostal", If(String.IsNullOrEmpty(adresse.CodePostal), CObj(DBNull.Value), adresse.CodePostal)),
            New SqlParameter("@NomCommune", If(String.IsNullOrEmpty(adresse.NomCommune), CObj(DBNull.Value), adresse.NomCommune)),
            New SqlParameter("@Pays", If(String.IsNullOrEmpty(adresse.Pays), CObj(DBNull.Value), adresse.Pays))
        }

        Return ExecuterCommande(sql, parameters) > 0
    End Function

    '-------------------------------------------------------------------------
    ' 4. SUPPRIMER (Delete)
    '-------------------------------------------------------------------------
    Public Function SupprimerAdresse(ByVal idAdresse As Integer) As Boolean
        Dim sql As String = "DELETE FROM [dbo].[Coordonnées] WHERE Id = @Id"

        Dim parameters() As SqlParameter = {New SqlParameter("@Id", idAdresse)}

        Return ExecuterCommande(sql, parameters) > 0
    End Function

End Class