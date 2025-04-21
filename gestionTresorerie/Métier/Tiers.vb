Imports System.Data.SqlClient

Public Class Tiers

    ReadOnly _id As Integer
    Dim _raisonSociale As String
    Dim _nom As String
    Dim _prénom As String
    Dim _categorieDefaut As Integer
    Dim _sousCategorieDefaut As Integer
    Dim _dateCreation As Date
    Dim _dateModification As Date

    Public Sub New(id As Integer, sNom As String, sPrenom As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sNom IsNot Nothing Then
            _id = id
            _nom = sNom
            _prénom = sPrenom
            _categorieDefaut = sCategorie
            _sousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Sub New(id As Integer, sRaisonSociale As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sRaisonSociale IsNot Nothing Then
            _id = id
            _raisonSociale = sRaisonSociale
            _categorieDefaut = sCategorie
            _sousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Shared Function getCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la catégorie et la sous catégorie d'un tiers 
        Dim maCmd As SqlCommand
        Dim monReader As SqlDataReader
        Dim iCategorie As Integer

        maCmd = New SqlCommand
        With maCmd
            .Connection = connexionDB.GetInstance.getConnexion
            .CommandText = "SELECT categorieDefaut FROM Tiers where id = '" & idTiers & "';"
        End With
        monReader = maCmd.ExecuteReader()
        Do While monReader.Read()
            Try
                iCategorie = monReader.GetInt32(0)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Loop
        'TODO : gérer les réponses multiples éventuelles
        monReader.Close()
        Return iCategorie
    End Function
    Public Shared Function getSousCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la sous catégorie d'un tiers 
        Dim maCmd As SqlCommand
        Dim monReader As SqlDataReader
        Dim iSousCategorie As Integer

        maCmd = New SqlCommand
        With maCmd
            .Connection = connexionDB.GetInstance.getConnexion
            .CommandText = "SELECT sousCategorieDefaut FROM Tiers where id = '" & idTiers & "';"
        End With
        monReader = maCmd.ExecuteReader()
        Do While monReader.Read()
            Try
                iSousCategorie = monReader.GetInt32(0)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Loop
        'TODO : gérer les réponses multiples éventuelles
        monReader.Close()
        Return iSousCategorie
    End Function

    Public Shared Function ExtraireTiers() As List(Of (nom As String, prenom As String, raisonSociale As String))
        Dim sqlConnexion As SqlConnection = Nothing
        Dim ListeTiers As List(Of (nom As String, prenom As String, raisonSociale As String))
        Try
            ' Obtenir la connexion SQL
            sqlConnexion = connexionDB.GetInstance.getConnexion

            ' Ouvrir la connexion
            If sqlConnexion.State <> ConnectionState.Open Then
                sqlConnexion.Open()
            End If

            ' Requête SQL pour extraire les données
            Dim query As String = "SELECT [nom], [prenom], [raisonSociale] FROM [dbo].Tiers"

            Using command As New SqlCommand(query, sqlConnexion)
                ' Exécuter la requête et récupérer les résultats
                Using reader As SqlDataReader = command.ExecuteReader()
                    ' Parcourir les résultats et ajouter chaque enregistrement à la liste
                    While reader.Read()
                        Dim nom As String = reader("nom").ToString()
                        Dim prenom As String = reader("prenom").ToString()
                        Dim raisonSociale As String = reader("raisonSociale").ToString()

                        ' Ajouter les données à la liste
                        ListeTiers.Add((nom, prenom, raisonSociale))
                    End While
                End Using
            End Using
        Catch ex As Exception
            Logger.ERR("Erreur lors de l'extraction des données de la table Tiers : " & ex.Message)
        Finally
            ' Fermer la connexion si elle est ouverte
            If sqlConnexion IsNot Nothing AndAlso sqlConnexion.State = ConnectionState.Open Then
                sqlConnexion.Close()
            End If
        End Try
        Return ListeTiers
    End Function
    Public Shared Function ConvertirListeTiersEnChaine(listeTiers As List(Of (nom As String, prenom As String, raisonSociale As String))) As String
        ' Utiliser un StringBuilder pour construire la chaîne de caractères efficacement
        Dim sb As New System.Text.StringBuilder()

        ' Parcourir chaque élément de la liste et ajouter une ligne pour chaque occurrence
        For Each tiers In listeTiers
            sb.AppendLine($"Nom: {tiers.nom}, Prénom: {tiers.prenom}, Raison Sociale: {tiers.raisonSociale}")
        Next

        ' Retourner la chaîne de caractères complète
        Return sb.ToString()
    End Function

    Public ReadOnly Property id As Integer
        Get
            Return _id
        End Get
    End Property
    Public Property RaisonSociale As String
        Get
            Return _raisonSociale
        End Get
        Set(value As String)
            _raisonSociale = value
        End Set
    End Property

    Public Property Nom As String
        Get
            Return _nom
        End Get
        Set(value As String)
            _nom = value
        End Set
    End Property

    Public Property Prénom As String
        Get
            Return _prénom
        End Get
        Set(value As String)
            _prénom = value
        End Set
    End Property

    Public Property CategorieDefaut As Integer
        Get
            Return _categorieDefaut
        End Get
        Set(value As Integer)
            _categorieDefaut = value
        End Set
    End Property

    Public Property SousCategorieDefaut As Integer
        Get
            Return _sousCategorieDefaut
        End Get
        Set(value As Integer)
            _sousCategorieDefaut = value
        End Set
    End Property

    Public Property dateCreation As Date
        Get
            Return _dateCreation
        End Get
        Set()
            _dateCreation = Now
        End Set
    End Property
    Public Property dateModification As Date
        Get
            Return _dateModification
        End Get
        Set()
            _dateModification = Now
        End Set
    End Property

End Class
