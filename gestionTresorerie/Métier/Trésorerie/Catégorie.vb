Imports System.Data.SqlClient

Public Class Categorie
    Inherits BaseDataRow

    Public Property Id As Int32
    Public Property Libelle As String
    Public Property DateDebut As Date?
    Public Property DateFin As Date?
    Public Property Debit As Boolean

    Public Sub New()
        MyBase.New()
    End Sub
    ' --- Constructeur principal ---
    Public Sub New(id As Integer, libelle As String, Optional dateDebut As Date? = Nothing,
                   Optional dateFin As Date? = Nothing, Optional debit As Boolean = False)
        Me.Id = id
        Me.Libelle = libelle
        Me.DateDebut = dateDebut
        Me.DateFin = dateFin
        Me.Debit = debit
    End Sub
    Private Shared Function CreateRepository() As CategorieRepository
        Dim executor As ISqlExecutor =
    RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New CategorieRepository(executor)
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
        If Not reader.IsDBNull(reader.GetOrdinal("DateDebut")) Then
            DateDebut = reader.GetDateTime(reader.GetOrdinal("DateDebut"))
        Else
            DateDebut = Nothing
        End If

        ' DateFin (nullable)
        If Not reader.IsDBNull(reader.GetOrdinal("DateFin")) Then
            DateFin = reader.GetDateTime(reader.GetOrdinal("DateFin"))
        Else
            DateFin = Nothing
        End If

        ' Debit (bit)
        If Not reader.IsDBNull(reader.GetOrdinal("Debit")) Then
            Debit = reader.GetBoolean(reader.GetOrdinal("Debit"))
        End If

    End Sub


    ' --- Redéfinition utile pour affichage ---
    Public Overrides Function ToString() As String
        Return $"{Id} - {Libelle}" & If(Debit, " (Débit)", "")
    End Function
    Public Shared Function libelleParId(Id As Integer) As String
        Try
            Dim libelle As String = CreateRepository().LibelleParId(Id)

            Logger.INFO($"Requête exécutée avec succès : reqLibCat pour le param {Id} => {libelle}")

            Return libelle

        Catch ex As Exception
            Logger.ERR($"Erreur lors de la récupération du libellé de catégorie {Id}. Message: {ex.Message}")
            Throw
        End Try
    End Function
End Class

