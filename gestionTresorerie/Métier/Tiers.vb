Imports System.Data.SqlClient

Public Class Tiers
    Inherits BaseDataRow

    Public ReadOnly Property id As Integer
    Public Property RaisonSociale As String

    Public Property Nom As String

    Public Property Prenom As String

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
    Private _dateCreation As Date
    Private _dateModification As Date

    Public Sub New()
        'MyBase.New()
    End Sub
    Public Sub New(id As Integer, sNom As String, sPrenom As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sNom IsNot Nothing Then
            Me.id = id
            Nom = sNom
            Prenom = sPrenom
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
        Dim iCategorie As Integer
        Dim monReader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqCategoriesDefautMouvements",
                             New Dictionary(Of String, Object) From {{"@id", idTiers}}
                             ).ExecuteReader()
        Do While monReader.Read()
            Try
                iCategorie = monReader.GetInt32(0)
            Catch ex As Exception
                Dim unused = MsgBox(ex.Message)
            End Try
        Loop
        'TODO : gérer les réponses multiples éventuelles
        monReader.Close()
        Return iCategorie
    End Function
    Public Shared Function getSousCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la sous catégorie d'un tiers  
        Dim iSousCategorie As Integer
        Dim monReader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqSousCategoriesDefautMouvements",
                             New Dictionary(Of String, Object) From {{"@id", idTiers}}
                             ).ExecuteReader()
        Do While monReader.Read()
            Try
                iSousCategorie = monReader.GetInt32(0)
            Catch ex As Exception
                Dim unused = MsgBox(ex.Message)
            End Try
        Loop
        'TODO : gérer les réponses multiples éventuelles
        monReader.Close()
        Return iSousCategorie
    End Function

    Public Shared Function ExtraireTiers() As List(Of (nom As String, prenom As String, raisonSociale As String))
        Dim ListeTiers As List(Of (nom As String, prenom As String, raisonSociale As String))
        Try
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
            Dim unused = sb.AppendLine($"Nom: {tiers.nom}, Prenom: {tiers.prenom}, Raison Sociale: {tiers.raisonSociale}")
        Next

        ' Retourner la chaîne de caractères complète
        Return sb.ToString()
    End Function
    ' --- Méthode Shared pour convertir un DataRow en Tiers ---
    Public Shared Function FromDataRow(dr As DataRow) As Tiers
        If dr Is Nothing Then Return Nothing

        Dim id As Integer = If(dr.Table.Columns.Contains("id") AndAlso dr("id") IsNot DBNull.Value, Convert.ToInt32(dr("id")), 0)
        Dim categorie As Integer = If(dr.Table.Columns.Contains("categorie") AndAlso dr("categorie") IsNot DBNull.Value, Convert.ToInt32(dr("categorie")), 0)
        Dim sousCategorie As Integer = If(dr.Table.Columns.Contains("sousCategorie") AndAlso dr("sousCategorie") IsNot DBNull.Value, Convert.ToInt32(dr("sousCategorie")), 0)

        ' Priorité à la Raison sociale si disponible
        If dr.Table.Columns.Contains("raisonSociale") AndAlso Not String.IsNullOrWhiteSpace(dr("raisonSociale").ToString()) Then
            Return New Tiers(id, dr("raisonSociale").ToString(), categorie, sousCategorie)
        Else
            Dim nom As String = If(dr.Table.Columns.Contains("nom"), dr("nom").ToString(), "")
            Dim prenom As String = If(dr.Table.Columns.Contains("prenom"), dr("prenom").ToString(), "")
            Return New Tiers(id, nom, prenom, categorie, sousCategorie)
        End If
    End Function

    Public Overrides Function ResumeTexte() As String
        If Not String.IsNullOrEmpty(RaisonSociale) Then
            Return RaisonSociale
        Else
            Return $"{Prenom} {Nom}".Trim()
        End If
    End Function
End Class
