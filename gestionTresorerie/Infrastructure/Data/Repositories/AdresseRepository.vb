Imports System.Data.SqlClient

Public Class AdresseRepository
    Implements IAdresseRepository

    Private ReadOnly _sqlExecutor As ISqlExecutor

    Public Sub New(sqlExecutor As ISqlExecutor)
        If sqlExecutor Is Nothing Then
            Throw New ArgumentNullException(NameOf(sqlExecutor))
        End If

        _sqlExecutor = sqlExecutor
    End Sub

    Public Function CreerAdresse(adresse As Coordonnees) As Integer Implements IAdresseRepository.CreerAdresse
        If adresse Is Nothing Then
            Throw New ArgumentNullException(NameOf(adresse))
        End If

        Dim sql As String =
            "INSERT INTO [dbo].[Coordonnées] " &
            "([IdTiers], [Rue1], [Rue2], [CodePostal], [Ville], [Pays], [EstPrincipale]) " &
            "VALUES (@IdTiers, @Rue1, @Rue2, @CodePostal, @Ville, @Pays, @EstPrincipale); " &
            "SELECT CAST(SCOPE_IDENTITY() AS INT);"

        Dim parameters As SqlParameter() = {
            New SqlParameter("@IdTiers", adresse.IdTiers),
            New SqlParameter("@Rue1", ToDbValue(adresse.Rue1)),
            New SqlParameter("@Rue2", ToDbValue(adresse.Rue2)),
            New SqlParameter("@CodePostal", ToDbValue(adresse.CodePostal)),
            New SqlParameter("@Ville", ToDbValue(adresse.Ville)),
            New SqlParameter("@Pays", ToDbValue(adresse.Pays)),
            New SqlParameter("@EstPrincipale", GetEstPrincipaleValue(adresse))
        }

        Dim newId As Integer = _sqlExecutor.ExecuteScalar(Of Integer)(sql, parameters)
        adresse.Id = newId
        Return newId
    End Function

    Public Function LireAdressesParTiers(idTiers As Integer) As List(Of Coordonnees) Implements IAdresseRepository.LireAdressesParTiers
        Dim sql As String =
            "SELECT * " &
            "FROM [dbo].[Coordonnées] " &
            "WHERE IdTiers = @IdTiers " &
            "ORDER BY EstPrincipale DESC, Id ASC"

        Dim parameters As SqlParameter() = {
            New SqlParameter("@IdTiers", idTiers)
        }

        Return _sqlExecutor.ExecuteReader(
            sql,
            parameters,
            Function(reader As SqlDataReader) MapCoordonnees(reader)
        )
    End Function

    Public Function MettreAJourAdresse(adresse As Coordonnees) As Boolean Implements IAdresseRepository.MettreAJourAdresse
        If adresse Is Nothing Then
            Throw New ArgumentNullException(NameOf(adresse))
        End If

        Dim sql As String =
            "UPDATE [dbo].[Coordonnées] SET " &
            "Rue1 = @Rue1, " &
            "Rue2 = @Rue2, " &
            "CodePostal = @CodePostal, " &
            "Ville = @Ville, " &
            "Pays = @Pays, " &
            "EstPrincipale = @EstPrincipale " &
            "WHERE Id = @Id AND IdTiers = @IdTiers"

        Dim parameters As SqlParameter() = {
            New SqlParameter("@Id", adresse.Id),
            New SqlParameter("@IdTiers", adresse.IdTiers),
            New SqlParameter("@Rue1", ToDbValue(adresse.Rue1)),
            New SqlParameter("@Rue2", ToDbValue(adresse.Rue2)),
            New SqlParameter("@CodePostal", ToDbValue(adresse.CodePostal)),
            New SqlParameter("@Ville", ToDbValue(adresse.Ville)),
            New SqlParameter("@Pays", ToDbValue(adresse.Pays)),
            New SqlParameter("@EstPrincipale", GetEstPrincipaleValue(adresse))
        }

        Return _sqlExecutor.ExecuteNonQuery(sql, parameters) > 0
    End Function

    Public Function SupprimerAdresse(idAdresse As Integer) As Boolean Implements IAdresseRepository.SupprimerAdresse
        Dim sql As String =
            "DELETE FROM [dbo].[Coordonnées] WHERE Id = @Id"

        Dim parameters As SqlParameter() = {
            New SqlParameter("@Id", idAdresse)
        }

        Return _sqlExecutor.ExecuteNonQuery(sql, parameters) > 0
    End Function

    Private Shared Function MapCoordonnees(reader As SqlDataReader) As Coordonnees
        Dim adresse As New Coordonnees()

        adresse.Id = reader.GetInt32(reader.GetOrdinal("Id"))
        adresse.IdTiers = reader.GetInt32(reader.GetOrdinal("IdTiers"))
        adresse.Rue1 = GetNullableString(reader, "Rue1")
        adresse.Rue2 = GetNullableString(reader, "Rue2")
        adresse.CodePostal = GetNullableString(reader, "CodePostal")
        adresse.Ville = GetNullableString(reader, "Ville")
        adresse.Pays = GetNullableString(reader, "Pays")

        Return adresse
    End Function

    Private Shared Function GetNullableString(reader As SqlDataReader, columnName As String) As String
        Dim ordinal As Integer = reader.GetOrdinal(columnName)

        If reader.IsDBNull(ordinal) Then
            Return Nothing
        End If

        Return reader.GetString(ordinal)
    End Function

    Private Shared Function ToDbValue(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then
            Return DBNull.Value
        End If

        Return value.Trim()
    End Function

    Private Shared Function GetEstPrincipaleValue(adresse As Coordonnees) As Object
        Dim prop = GetType(Coordonnees).GetProperty("EstPrincipale")

        If prop Is Nothing Then
            Return DBNull.Value
        End If

        Dim value = prop.GetValue(adresse, Nothing)

        If value Is Nothing Then
            Return DBNull.Value
        End If

        Return value
    End Function
End Class