Imports System.Data.SqlClient
Imports System.Reflection.Metadata

Public Class Tiers
    Dim _dateCreation As Date
    Dim _dateModification As Date

    Public Sub New(id As Integer, sNom As String, sPrenom As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sNom IsNot Nothing Then
            Me.id = id
            Nom = sNom
            Prénom = sPrenom
            CategorieDefaut = sCategorie
            SousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Sub New(id As Integer, sRaisonSociale As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sRaisonSociale IsNot Nothing Then
            Me.id = id
            RaisonSociale = sRaisonSociale
            CategorieDefaut = sCategorie
            SousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Shared Function getCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la catégorie et la sous catégorie d'un tiers 
        'Dim maCmd As SqlCommand
        'Dim monReader As SqlDataReader
        Dim iCategorie As Integer

        'maCmd = New SqlCommand
        'With maCmd
        '    .Connection = ConnexionDB.GetInstance.getConnexion
        '    .CommandText = "SELECT categorieDefaut FROM Tiers where id = '" & idTiers & "';"
        'End With
        'monReader = maCmd.ExecuteReader()
        Dim monReader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqCategoriesDefautMouvements",
                             New Dictionary(Of String, Object) From {{"@id", idTiers}}
                             ).ExecuteReader()
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
        'Dim maCmd As SqlCommand
        'Dim monReader As SqlDataReader
        Dim iSousCategorie As Integer

        'maCmd = New SqlCommand
        'With maCmd
        '    .Connection = ConnexionDB.GetInstance.getConnexion
        '    .CommandText = "SELECT sousCategorieDefaut FROM Tiers where id = '" & idTiers & "';"
        'End With
        'monReader = maCmd.ExecuteReader()
        Dim monReader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqSousCategoriesDefautMouvements",
                             New Dictionary(Of String, Object) From {{"@id", idTiers}}
                             ).ExecuteReader()
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
        'Dim sqlConnexion As SqlConnection = Nothing
        Dim ListeTiers As List(Of (nom As String, prenom As String, raisonSociale As String))
        Try
            ' Obtenir la connexion SQL
            'sqlConnexion = ConnexionDB.GetInstance.getConnexion

            '' Ouvrir la connexion
            'If sqlConnexion.State <> ConnectionState.Open Then
            '    sqlConnexion.Open()
            'End If

            '' Requête SQL pour extraire les données
            'Dim query As String = "SELECT [nom], [prenom], [raisonSociale] FROM [dbo].Tiers"

            'Using command As New SqlCommand(query, sqlConnexion)
            ' Exécuter la requête et récupérer les résultats
            'Using reader As SqlDataReader = command.ExecuteReader()
            Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqIdentiteTiers"
                             ).ExecuteReader()
                ' Parcourir les résultats et ajouter chaque enregistrement à la liste
                While reader.Read()
                    Dim nom As String = reader("nom").ToString()
                    Dim prenom As String = reader("prenom").ToString()
                    Dim raisonSociale As String = reader("raisonSociale").ToString()

                    ' Ajouter les données à la liste
                    ListeTiers.Add((nom, prenom, raisonSociale))
                End While
            End Using
            'End Using
        Catch ex As Exception
            Logger.ERR("Erreur lors de l'extraction des données de la table Tiers : " & ex.Message)
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
    Public Property RaisonSociale As String

    Public Property Nom As String

    Public Property Prénom As String

    Public Property CategorieDefaut As Integer

    Public Property SousCategorieDefaut As Integer

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
