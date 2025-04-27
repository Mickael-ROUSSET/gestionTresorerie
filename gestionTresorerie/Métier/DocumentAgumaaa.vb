Imports System.Data.SqlClient

Public Class DocumentAgumaaa
    Property IdMvtDoc As Integer

    ' Propriété pour dateDoc
    Public Property DateDoc As Date

    ' Propriété pour contenuDoc
    Public Property ContenuDoc As String

    ' Propriété pour cheminDoc
    Public Property CheminDoc As String

    ' Propriété pour categorieDoc
    Public Property CategorieDoc As String

    ' Propriété pour sousCategorieDoc
    Public Property SousCategorieDoc As String

    Public Shared Sub InsererDocument(dateDoc As Date, contenuDoc As String, cheminDoc As String, categorieDoc As String, sousCategorieDoc As String, idMvtDoc As Integer)
        Try
            'Dim query As String = "INSERT INTO [dbo].[Documents] (dateDoc, contenuDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc) VALUES (@dateDoc, @contenuDoc, @cheminDoc, @categorieDoc, @sousCategorieDoc, @idMvtDoc);"

            'Using command As New SqlCommand(query, ConnexionDB.GetInstance.getConnexion)
            '    command.Parameters.AddWithValue("@dateDoc", dateDoc)
            '    command.Parameters.AddWithValue("@contenuDoc", contenuDoc)
            '    command.Parameters.AddWithValue("@cheminDoc", cheminDoc)
            '    command.Parameters.AddWithValue("@categorieDoc", categorieDoc)
            '    command.Parameters.AddWithValue("@sousCategorieDoc", sousCategorieDoc)
            '    command.Parameters.AddWithValue("@idMvtDoc", idMvtDoc)

            '    command.ExecuteNonQuery()
            'End Using
            SqlCommandBuilder.CreateSqlCommand("insertDocAgumaaa",
                             New Dictionary(Of String, Object) From {{"@dateDoc", dateDoc},
                                                                     {"@contenuDoc", contenuDoc},
                                                                     {"@cheminDoc", cheminDoc},
                                                                     {"@categorieDoc", categorieDoc},
                                                                     {"@sousCategorieDoc", sousCategorieDoc},
                                                                     {"@idMvtDoc", idMvtDoc}}
                             ).ExecuteNonQuery()
            Console.WriteLine("Document inséré avec succès.")
        Catch ex As Exception
            Console.WriteLine("Erreur lors de l'insertion du document : " & ex.Message)
        End Try
    End Sub
    Public Shared Function LireDocuments() As DataTable
        Dim table As New DataTable()
        Try
            'Dim query As String = "SELECT idDoc, dateDoc, contenuDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc FROM [dbo].[Documents];"
            'Using command As New SqlCommand(query, ConnexionDB.GetInstance.getConnexion)
            Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand("reqDocs")

            Using adapter As New SqlDataAdapter(command)
                adapter.Fill(table)
            End Using
            'End Using
        Catch ex As Exception
            Console.WriteLine("Erreur lors de la lecture des documents : " & ex.Message)
        End Try

        Return table
    End Function
    Public Shared Sub MettreAJourDocument(idDoc As Integer, dateDoc As Date, contenuDoc As String, cheminDoc As String, categorieDoc As String, sousCategorieDoc As String, idMvtDoc As Integer)
        Try
            'Dim query As String = "UPDATE [dbo].[Documents] SET dateDoc = @dateDoc, contenuDoc = @contenuDoc, cheminDoc = @cheminDoc, categorieDoc = @categorieDoc, sousCategorieDoc = @sousCategorieDoc, idMvtDoc=@idMvtDoc WHERE idDoc = @idDoc;"

            'Using command As New SqlCommand(query, ConnexionDB.GetInstance.getConnexion)
            '    command.Parameters.AddWithValue("@idDoc", idDoc)
            '    command.Parameters.AddWithValue("@dateDoc", dateDoc)
            '    command.Parameters.AddWithValue("@contenuDoc", contenuDoc)
            '    command.Parameters.AddWithValue("@cheminDoc", cheminDoc)
            '    command.Parameters.AddWithValue("@categorieDoc", categorieDoc)
            '    command.Parameters.AddWithValue("@sousCategorieDoc", sousCategorieDoc)
            '    command.Parameters.AddWithValue("@idMvtDoc", idMvtDoc)

            '    command.ExecuteNonQuery()
            'End Using

            Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand("updDocs",
                             New Dictionary(Of String, Object) From {{"@idDoc", idDoc},
                                                                     {"@dateDoc", dateDoc},
                                                                     {"@contenuDoc", contenuDoc},
                                                                     {"@cheminDoc", cheminDoc},
                                                                     {"@categorieDoc", categorieDoc},
                                                                     {"@sousCategorieDoc", sousCategorieDoc},
                                                                     {"@idMvtDoc", idMvtDoc}}
                             )
            command.ExecuteNonQuery()
            Console.WriteLine("Document mis à jour avec succès.")
        Catch ex As Exception
            Console.WriteLine("Erreur lors de la mise à jour du document : " & ex.Message)
        End Try
    End Sub
    Public Shared Sub SupprimerDocument(idDoc As Integer)
        Try
            ''connection.Open()
            'Dim query As String = "DELETE FROM [dbo].[Documents] WHERE idDoc = @idDoc;"

            'Using command As New SqlCommand(query, ConnexionDB.GetInstance.getConnexion)
            '    command.Parameters.AddWithValue("@idDoc", idDoc)

            '    command.ExecuteNonQuery()
            'End Using
            Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand("delDocs",
                             New Dictionary(Of String, Object) From {{"@idDoc", idDoc}}
                             )
            command.ExecuteNonQuery()
            Console.WriteLine("Document supprimé avec succès.")
        Catch ex As Exception
            Console.WriteLine("Erreur lors de la suppression du document : " & ex.Message)
        End Try
    End Sub

End Class
