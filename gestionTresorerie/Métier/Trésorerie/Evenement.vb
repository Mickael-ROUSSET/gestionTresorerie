Imports System.Data.SqlClient
Imports DocumentFormat.OpenXml.Office2010.Excel

Public Class Evenement
    Inherits BaseDataRow

    Public Property libelle As String
    Public Property DateDebut As Date
    Public Property DateFin As Date

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
