'Imports System.Data.SqlClient

'Public Class TiersRepository

'    Private ReadOnly _connectionString As String

'    Public Sub New(connStr As String)
'        _connectionString = connStr
'    End Sub


'    ' ---------------------------------------------------------
'    ' Récupération d’un tiers par son ID
'    ' ---------------------------------------------------------
'    Public Function GetById(Id As Integer) As Tiers

'        Dim sql = "SELECT * FROM Tiers WHERE Id=@Id"

'        Using conn As New SqlConnection(_connectionString),
'              cmd As New SqlCommand(sql, conn)

'            cmd.Parameters.AddWithValue("@Id", Id)
'            conn.Open()

'            Using reader = cmd.ExecuteReader()
'                If reader.Read() Then

'                    Dim t As New Tiers With {
'                        .Id = reader("Id"),
'                        .RaisonSociale = If(reader("RaisonSociale") Is DBNull.Value, Nothing, reader("RaisonSociale").ToString()),
'                        .Nom = If(reader("Nom") Is DBNull.Value, Nothing, reader("Nom").ToString()),
'                        .Prenom = If(reader("Prenom") Is DBNull.Value, Nothing, reader("Prenom").ToString()),
'                        .CategorieDefaut = reader("Categorie"),
'                        .SousCategorieDefaut = reader("SousCategorie")
'                    }

'                    ' Récupération des coordonnées associées
'                    t.Coordonnees = GetCoordonneesByTiersId(t.Id)

'                    Return t

'                Else
'                    Return Nothing
'                End If
'            End Using

'        End Using

'    End Function



'    ' ---------------------------------------------------------
'    ' Récupérer les coordonnées associées
'    ' ---------------------------------------------------------
'    Public Function GetCoordonneesByTiersId(idTiers As Integer) As List(Of Coordonnees)

'        Dim liste As New List(Of Coordonnees)

'        Dim sql = "SELECT * FROM Coordonnees WHERE IdTiers=@Id"

'        Using conn As New SqlConnection(_connectionString),
'              cmd As New SqlCommand(sql, conn)

'            cmd.Parameters.AddWithValue("@Id", idTiers)
'            conn.Open()

'            Using reader = cmd.ExecuteReader()

'                While reader.Read()
'                    liste.Add(New Coordonnees With {
'                        .Id = reader("Id"),
'                        .IdTiers = idTiers,
'                        .TypeAdresse = reader("TypeAdresse").ToString(),
'                        .Rue1 = reader("Rue1").ToString(),
'                        .Rue2 = reader("Rue2").ToString(),
'                        .CodePostal = reader("CodePostal").ToString(),
'                        .NomCommune = reader("NomCommune").ToString(),
'                        .Pays = reader("Pays").ToString(),
'                        .EstPrincipale = CBool(reader("EstPrincipale")),
'                        .Email = If(reader("Email") Is DBNull.Value, Nothing, reader("Email").ToString()),
'                        .Telephone = If(reader("Telephone") Is DBNull.Value, Nothing, reader("Telephone").ToString())
'                    })
'                End While

'            End Using
'        End Using

'        Return liste

'    End Function

'End Class
