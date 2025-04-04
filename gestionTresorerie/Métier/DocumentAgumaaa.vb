Imports System.Data.SqlClient

Public Class DocumentAgumaaa
    ' Champs privés pour stocker les valeurs
    Private _dateDoc As Date
    Private _contenuDoc As String
    Private _cheminDoc As String
    Private _categorieDoc As String
    Private _sousCategorieDoc As String
    Private _idMvtDoc As Integer ' id du mouvement associé au document
    Property IdMvtDoc As Integer
        Get
            Return _idMvtDoc
        End Get
        Set(value As Integer)
            _idMvtDoc = value
        End Set
    End Property

    ' Propriété pour dateDoc
    Public Property DateDoc As Date
        Get
            Return _dateDoc
        End Get
        Set(value As Date)
            _dateDoc = value
        End Set
    End Property

    ' Propriété pour contenuDoc
    Public Property ContenuDoc As String
        Get
            Return _contenuDoc
        End Get
        Set(value As String)
            _contenuDoc = value
        End Set
    End Property

    ' Propriété pour cheminDoc
    Public Property CheminDoc As String
        Get
            Return _cheminDoc
        End Get
        Set(value As String)
            _cheminDoc = value
        End Set
    End Property

    ' Propriété pour categorieDoc
    Public Property CategorieDoc As String
        Get
            Return _categorieDoc
        End Get
        Set(value As String)
            _categorieDoc = value
        End Set
    End Property

    ' Propriété pour sousCategorieDoc
    Public Property SousCategorieDoc As String
        Get
            Return _sousCategorieDoc
        End Get
        Set(value As String)
            _sousCategorieDoc = value
        End Set
    End Property

    Public Sub InsererDocument(dateDoc As Date, contenuDoc As String, cheminDoc As String, categorieDoc As String, sousCategorieDoc As String, idMvtDoc As Integer)
        'Using connection As New SqlConnection("Votre_Chaine_De_Connexion")
        Using connexionDB.GetInstance.getConnexion
            Try
                Dim query As String = "INSERT INTO [dbo].[Documents] (dateDoc, contenuDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc) VALUES (@dateDoc, @contenuDoc, @cheminDoc, @categorieDoc, @sousCategorieDoc, @idMvtDoc);"

                Using command As New SqlCommand(query, connexionDB.GetInstance.getConnexion)
                    command.Parameters.AddWithValue("@dateDoc", dateDoc)
                    command.Parameters.AddWithValue("@contenuDoc", contenuDoc)
                    command.Parameters.AddWithValue("@cheminDoc", cheminDoc)
                    command.Parameters.AddWithValue("@categorieDoc", categorieDoc)
                    command.Parameters.AddWithValue("@sousCategorieDoc", sousCategorieDoc)
                    command.Parameters.AddWithValue("@idMvtDoc", idMvtDoc)

                    command.ExecuteNonQuery()
                End Using

                Console.WriteLine("Document inséré avec succès.")
            Catch ex As Exception
                Console.WriteLine("Erreur lors de l'insertion du document : " & ex.Message)
            End Try
        End Using
    End Sub
    Public Function LireDocuments() As DataTable
        Dim table As New DataTable()

        'Using connection As New SqlConnection("Votre_Chaine_De_Connexion")
        Using connexionDB.GetInstance.getConnexion
            Try
                Dim query As String = "SELECT idDoc, dateDoc, contenuDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc FROM [dbo].[Documents];"

                Using command As New SqlCommand(query, connexionDB.GetInstance.getConnexion)
                    Using adapter As New SqlDataAdapter(command)
                        adapter.Fill(table)
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Erreur lors de la lecture des documents : " & ex.Message)
            End Try
        End Using

        Return table
    End Function
    Public Sub MettreAJourDocument(idDoc As Integer, dateDoc As Date, contenuDoc As String, cheminDoc As String, categorieDoc As String, sousCategorieDoc As String, idMvtDoc As Integer)
        'Using connection As New SqlConnection("Votre_Chaine_De_Connexion")
        Using connexionDB.GetInstance.getConnexion
            Try
                Dim query As String = "UPDATE [dbo].[Documents] SET dateDoc = @dateDoc, contenuDoc = @contenuDoc, cheminDoc = @cheminDoc, categorieDoc = @categorieDoc, sousCategorieDoc = @sousCategorieDoc, idMvtDoc=@idMvtDoc WHERE idDoc = @idDoc;"

                Using command As New SqlCommand(query, connexionDB.GetInstance.getConnexion)
                    command.Parameters.AddWithValue("@idDoc", idDoc)
                    command.Parameters.AddWithValue("@dateDoc", dateDoc)
                    command.Parameters.AddWithValue("@contenuDoc", contenuDoc)
                    command.Parameters.AddWithValue("@cheminDoc", cheminDoc)
                    command.Parameters.AddWithValue("@categorieDoc", categorieDoc)
                    command.Parameters.AddWithValue("@sousCategorieDoc", sousCategorieDoc)
                    command.Parameters.AddWithValue("@idMvtDoc", idMvtDoc)

                    command.ExecuteNonQuery()
                End Using

                Console.WriteLine("Document mis à jour avec succès.")
            Catch ex As Exception
                Console.WriteLine("Erreur lors de la mise à jour du document : " & ex.Message)
            End Try
        End Using
    End Sub
    Public Sub SupprimerDocument(idDoc As Integer)
        'Using connection As New SqlConnection("Votre_Chaine_De_Connexion")
        Using connexionDB.GetInstance.getConnexion
            Try
                'connection.Open()
                Dim query As String = "DELETE FROM [dbo].[Documents] WHERE idDoc = @idDoc;"

                Using command As New SqlCommand(query, connexionDB.GetInstance.getConnexion)
                    command.Parameters.AddWithValue("@idDoc", idDoc)

                    command.ExecuteNonQuery()
                End Using

                Console.WriteLine("Document supprimé avec succès.")
            Catch ex As Exception
                Console.WriteLine("Erreur lors de la suppression du document : " & ex.Message)
            End Try
        End Using
    End Sub

End Class
