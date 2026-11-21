Imports System.Data.SqlClient
Imports DocumentFormat.OpenXml.Office2010.Excel

Public Class Evenement
    Inherits BaseDataRow

    Public Property libelle As String
    Public Property DateDebut As Date
    Public Property DateFin As Date

    Friend Shared Function EnregistrerLibelle(connection As SqlConnection, nom As String) As String
        Dim valeur = If(nom, "").Trim()
        If valeur.Length = 0 OrElse valeur.Length > 50 Then
            Throw New ArgumentException("Saisissez un libellé de 1 à 50 caractères.")
        End If
        Using transaction = connection.BeginTransaction(), command = connection.CreateCommand()
            command.Transaction = transaction
            command.CommandText =
                "DECLARE @existant nvarchar(max); " &
                "SELECT TOP (1) @existant=libelle FROM dbo.Evenements WITH (TABLOCKX,HOLDLOCK) WHERE libelle=@nom ORDER BY Id; " &
                "IF @existant IS NULL INSERT dbo.Evenements(libelle) VALUES (@nom); " &
                "SELECT COALESCE(@existant,@nom);"
            command.Parameters.Add("@nom", System.Data.SqlDbType.NVarChar, 50).Value = valeur
            Dim libelleSauve = CStr(command.ExecuteScalar())
            transaction.Commit()
            Return libelleSauve
        End Using
    End Function

    Public Sub New()
        MyBase.New()
    End Sub
    Public Overrides Sub LoadFromReader(reader As SqlDataReader)

        If reader Is Nothing Then Exit Sub
        ' Libelle (nvarchar)
        If Not reader.IsDBNull(reader.GetOrdinal("Libelle")) Then
            libelle = reader.GetString(reader.GetOrdinal("Libelle"))
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
    End Sub

End Class
