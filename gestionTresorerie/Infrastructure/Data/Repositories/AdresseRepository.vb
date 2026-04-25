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
        Return Inserer(adresse)
    End Function

    Public Function LireAdressesParTiers(idTiers As Integer) As List(Of Coordonnees) Implements IAdresseRepository.LireAdressesParTiers
        Return LireParTiers(idTiers)
    End Function

    Public Function MettreAJourAdresse(adresse As Coordonnees) As Boolean Implements IAdresseRepository.MettreAJourAdresse
        Return MettreAJour(adresse) > 0
    End Function

    Public Function SupprimerAdresse(idAdresse As Integer) As Boolean Implements IAdresseRepository.SupprimerAdresse
        Return Supprimer(idAdresse) > 0
    End Function

    Public Function LireParTiers(idTiers As Integer) As List(Of Coordonnees)
        Return _sqlExecutor.ExecuteNamedReader(
            "selCoordonneesByIdTiers",
            New List(Of SqlParameter) From {
                New SqlParameter("@IdTiers", idTiers)
            },
            Function(reader As SqlDataReader)
                Return MapCoordonnees(reader)
            End Function)
    End Function

    Public Function ExistePourTiers(idTiers As Integer) As Boolean
        Dim result As Object =
            _sqlExecutor.ExecuteNamedScalar(Of Object)(
                "existeCoordonnee",
                New List(Of SqlParameter) From {
                    New SqlParameter("@IdTiers", idTiers)
                })

        Return result IsNot Nothing AndAlso result IsNot DBNull.Value
    End Function

    Public Function Inserer(coord As Coordonnees) As Integer
        If coord Is Nothing Then Throw New ArgumentNullException(NameOf(coord))

        Dim id As Integer =
            _sqlExecutor.ExecuteNamedScalar(Of Integer)(
                "insertCoordonnees",
                BuildParameters(coord, includeId:=False))

        coord.Id = id
        Return id
    End Function

    Public Function MettreAJour(coord As Coordonnees) As Integer
        If coord Is Nothing Then Throw New ArgumentNullException(NameOf(coord))

        Return _sqlExecutor.ExecuteNamedNonQuery(
            "updateCoordonnees",
            BuildParameters(coord, includeId:=True))
    End Function

    Public Function InsererOuMettreAJour(coord As Coordonnees) As Integer
        If coord Is Nothing Then Throw New ArgumentNullException(NameOf(coord))

        If coord.Id > 0 OrElse ExistePourTiers(coord.IdTiers) Then
            Return MettreAJour(coord)
        End If

        Inserer(coord)
        Return 1
    End Function

    Public Function Supprimer(idAdresse As Integer) As Integer
        Return _sqlExecutor.ExecuteNonQuery(
            "DELETE FROM Coordonnees WHERE Id = @Id",
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", idAdresse)
            })
    End Function

    Public Function LireVillesParCodePostal(codePostal As String) As List(Of String)
        Return _sqlExecutor.ExecuteNamedReader(
            "selVillesParCP",
            New List(Of SqlParameter) From {
                New SqlParameter("@CodePostal", codePostal)
            },
            Function(reader As SqlDataReader)
                If reader.IsDBNull(0) Then
                    Return String.Empty
                End If

                Return reader.GetString(0)
            End Function)
    End Function

    Private Shared Function BuildParameters(coord As Coordonnees, includeId As Boolean) As List(Of SqlParameter)
        Dim parameters As New List(Of SqlParameter)

        If includeId Then
            parameters.Add(New SqlParameter("@Id", coord.Id))
        End If

        parameters.Add(New SqlParameter("@IdTiers", coord.IdTiers))
        parameters.Add(New SqlParameter("@Rue1", ToDbValue(coord.Rue1)))
        parameters.Add(New SqlParameter("@Rue2", ToDbValue(coord.Rue2)))
        parameters.Add(New SqlParameter("@CodePostal", ToDbValue(coord.CodePostal)))
        parameters.Add(New SqlParameter("@Ville", ToDbValue(coord.Ville)))
        parameters.Add(New SqlParameter("@Pays", ToDbValue(coord.Pays)))
        parameters.Add(New SqlParameter("@Email", ToDbValue(coord.Email)))
        parameters.Add(New SqlParameter("@Telephone", ToDbValue(coord.Telephone)))

        Return parameters
    End Function

    Private Shared Function MapCoordonnees(reader As SqlDataReader) As Coordonnees
        Dim coord As New Coordonnees()

        coord.Id = GetInt32(reader, "Id")
        coord.IdTiers = GetInt32(reader, "IdTiers")
        coord.Rue1 = GetNullableString(reader, "Rue1")
        coord.Rue2 = GetNullableString(reader, "Rue2")
        coord.CodePostal = GetNullableString(reader, "CodePostal")
        coord.Ville = GetNullableString(reader, "Ville")
        coord.Pays = GetNullableString(reader, "Pays")
        coord.Email = GetNullableString(reader, "Email")
        coord.Telephone = GetNullableString(reader, "Telephone")

        Return coord
    End Function

    Private Shared Function GetInt32(reader As SqlDataReader, columnName As String) As Integer
        Dim ordinal As Integer = reader.GetOrdinal(columnName)

        If reader.IsDBNull(ordinal) Then
            Return 0
        End If

        Return Convert.ToInt32(reader.GetValue(ordinal))
    End Function

    Private Shared Function GetNullableString(reader As SqlDataReader, columnName As String) As String
        Dim ordinal As Integer = reader.GetOrdinal(columnName)

        If reader.IsDBNull(ordinal) Then
            Return Nothing
        End If

        Return reader.GetValue(ordinal).ToString()
    End Function

    Private Shared Function ToDbValue(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then
            Return DBNull.Value
        End If

        Return value.Trim()
    End Function

End Class