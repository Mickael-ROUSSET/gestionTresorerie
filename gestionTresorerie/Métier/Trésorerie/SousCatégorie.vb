Imports System.Data.SqlClient

Public Class SousCategorie
    Inherits BaseDataRow

    Public Property Id As Int32
    Public Property Libelle As String
    Public Property DateDebut As Date?
    Public Property DateFin As Date?
    Public Property idCategorie As Integer?

    Public Sub New()
        MyBase.New()
    End Sub
    ' --- Constructeur principal ---
    Public Sub New(id As Integer, libelle As String, Optional dateDebut As Date? = Nothing,
                   Optional dateFin As Date? = Nothing, Optional idCategorie As Integer? = 0)
        Me.Id = id
        Me.Libelle = libelle
        Me.DateDebut = dateDebut
        Me.DateFin = dateFin
        Me.idCategorie = idCategorie
    End Sub
    Private Shared Function CreateRepository() As SousCategorieRepository
        Dim executor As ISqlExecutor =
    RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New SousCategorieRepository(executor)
    End Function
    Public Shared Function LireToutes() As List(Of SousCategorie)
        Try
            Return CreateRepository().LireToutes()
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la lecture des sous-catégories : {ex.Message}")
            Throw
        End Try
    End Function

    Public Shared Function LireParCategorie(idCategorie As Integer) As List(Of SousCategorie)
        Try
            Return CreateRepository().LireParCategorie(idCategorie)
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la lecture des sous-catégories de la catégorie {idCategorie} : {ex.Message}")
            Throw
        End Try
    End Function
    Public Overrides Sub LoadFromReader(reader As SqlDataReader)

        If reader Is Nothing Then Exit Sub

        ' Id (int)
        If Not reader.IsDBNull(reader.GetOrdinal("Id")) Then
            Id = reader.GetInt32(reader.GetOrdinal("Id"))
        End If

        ' Libelle (nvarchar)
        If Not reader.IsDBNull(reader.GetOrdinal("Libelle")) Then
            Libelle = reader.GetString(reader.GetOrdinal("Libelle"))
        End If

        ' DateDebut (nullable)
        If HasColumn(reader, "DateDebut") AndAlso Not reader.IsDBNull(reader.GetOrdinal("DateDebut")) Then
            DateDebut = reader.GetDateTime(reader.GetOrdinal("DateDebut"))
        Else
            DateDebut = Nothing
        End If

        ' DateFin (nullable) 
        If HasColumn(reader, "DateFin") AndAlso Not reader.IsDBNull(reader.GetOrdinal("DateFin")) Then
            DateFin = reader.GetDateTime(reader.GetOrdinal("DateFin"))
        Else
            DateFin = Nothing
        End If

        'Id Catégorie
        If HasColumn(reader, "idCategorie") AndAlso Not reader.IsDBNull(reader.GetOrdinal("idCategorie")) Then
            idCategorie = reader.GetInt32(reader.GetOrdinal("idCategorie"))
        Else
            idCategorie = Nothing
        End If
    End Sub
    Private Shared Function HasColumn(reader As SqlDataReader, columnName As String) As Boolean
        For i As Integer = 0 To reader.FieldCount - 1
            If String.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next

        Return False
    End Function
End Class
