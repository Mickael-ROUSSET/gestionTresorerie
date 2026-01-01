Imports System.Data.SqlClient

Public Class Tiers
    Inherits BaseDataRow

    Public Property Id As Integer
    Public Property RaisonSociale As String
    Public Property Nom As String
    Public Property Prenom As String
    Public Property CategorieDefaut As Integer
    Public Property SousCategorieDefaut As Integer
    ' Liste des coordonnées associées
    Public Property DateNaissance As Date
    Public Property LieuNaissance As String

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
            Me.Id = id
            Nom = sNom
            Prenom = sPrenom
            CategorieDefaut = sCategorie
            SousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Sub New(id As Integer, sRaisonSociale As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sRaisonSociale IsNot Nothing Then
            Me.Id = id
            RaisonSociale = sRaisonSociale
            CategorieDefaut = sCategorie
            SousCategorieDefaut = sSousCategorie
        End If
    End Sub

    Public Overrides Sub LoadFromReader(reader As SqlDataReader)
        'Id = If(reader("Id") Is DBNull.Value, 0, CInt(reader("Id")))
        Nom = reader("Nom").ToString()
        Prenom = reader("Prenom").ToString()
        RaisonSociale = reader("RaisonSociale").ToString()
        CategorieDefaut = If(reader("categorieDefaut") Is DBNull.Value, 0, CInt(reader("categorieDefaut")))
        SousCategorieDefaut = If(reader("sousCategorieDefaut") Is DBNull.Value, 0, CInt(reader("sousCategorieDefaut")))
    End Sub
    Public Shared Function getCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la catégorie et la sous catégorie d'un tiers  
        Dim iCategorie As Integer
        Dim monReader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "reqCategoriesDefautMouvements",
                             New Dictionary(Of String, Object) From {{"@Id", idTiers}}
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
        Dim monReader As SqlDataReader = SqlCommandBuilder.
            CreateSqlCommand(Constantes.bddAgumaaa, "reqSousCategoriesDefautMouvements",
                             New Dictionary(Of String, Object) From {{"@Id", idTiers}}
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

    'Public Shared Function ExtraireTiers() As List(Of (nom As String, prenom As String, raisonSociale As String))
    '    Dim ListeTiers As List(Of (nom As String, prenom As String, raisonSociale As String))
    '    Try
    '        Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "reqIdentiteTiers"
    '                         ).ExecuteReader()
    '            ' Parcourir les résultats et ajouter chaque enregistrement à la liste
    '            While reader.Read()
    '                Dim nom As String = reader("nom").ToString()
    '                Dim prenom As String = reader("prenom").ToString()
    '                Dim raisonSociale As String = reader("raisonSociale").ToString()

    '                ' Ajouter les données à la liste
    '                ListeTiers.Add((nom, prenom, raisonSociale))
    '            End While
    '        End Using
    '        'End Using
    '    Catch ex As Exception
    '        Logger.ERR("Erreur lors de l'extraction des données de la table Tiers : " & ex.Message)
    '    End Try
    '    Return ListeTiers
    'End Function
    'Public Shared Function ConvertirListeTiersEnChaine(listeTiers As List(Of (nom As String, prenom As String, raisonSociale As String))) As String
    '    ' Utiliser un StringBuilder pour construire la chaîne de caractères efficacement
    '    Dim sb As New System.Text.StringBuilder()

    '    ' Parcourir chaque élément de la liste et ajouter une ligne pour chaque occurrence
    '    For Each tiers In listeTiers
    '        Dim unused = sb.AppendLine($"Nom: {tiers.nom}, Prenom: {tiers.prenom}, Raison Sociale: {tiers.raisonSociale}")
    '    Next

    '    ' Retourner la chaîne de caractères complète
    '    Return sb.ToString()
    'End Function
    ' --- Méthode Shared pour convertir un DataRow en Tiers ---
    'Public Shared Function FromDataRow(dr As DataRow) As Tiers
    '    If dr Is Nothing Then Return Nothing

    '    Dim id As Integer = If(dr.Table.Columns.Contains("Id") AndAlso dr("Id") IsNot DBNull.Value, Convert.ToInt32(dr("Id")), 0)
    '    Dim categorie As Integer = If(dr.Table.Columns.Contains("categorie") AndAlso dr("categorie") IsNot DBNull.Value, Convert.ToInt32(dr("categorie")), 0)
    '    Dim sousCategorie As Integer = If(dr.Table.Columns.Contains("sousCategorie") AndAlso dr("sousCategorie") IsNot DBNull.Value, Convert.ToInt32(dr("sousCategorie")), 0)

    '    ' Priorité à la Raison sociale si disponible
    '    If dr.Table.Columns.Contains("raisonSociale") AndAlso Not String.IsNullOrWhiteSpace(dr("raisonSociale").ToString()) Then
    '        Return New Tiers(id, dr("raisonSociale").ToString(), categorie, sousCategorie)
    '    Else
    '        Dim nom As String = If(dr.Table.Columns.Contains("nom"), dr("nom").ToString(), "")
    '        Dim prenom As String = If(dr.Table.Columns.Contains("prenom"), dr("prenom").ToString(), "")
    '        Return New Tiers(id, nom, prenom, categorie, sousCategorie)
    '    End If
    'End Function
    Public Overloads Shared Function FromDataRow(dr As DataRow) As Tiers
        If dr Is Nothing Then Return Nothing

        ' Récupération des IDs et Catégories (0 par défaut si NULL)
        Dim id = dr.GetSafe(Of Integer)("Id")
        Dim cat = dr.GetSafe(Of Integer)("categorie")
        Dim sCat = dr.GetSafe(Of Integer)("sousCategorie")

        ' Récupération de la raison sociale
        Dim rs = dr.GetSafe(Of String)("raisonSociale")

        If Not String.IsNullOrWhiteSpace(rs) Then
            Return New Tiers(id, rs, cat, sCat)
        Else
            ' Récupération Nom/Prénom
            Dim nom = dr.GetSafe(Of String)("nom", "")
            Dim prenom = dr.GetSafe(Of String)("prenom", "")
            Return New Tiers(id, nom, prenom, cat, sCat)
        End If
    End Function

    ' Fonction utilitaire générique pour extraire proprement les données
    Private Shared Function GetField(Of T)(dr As DataRow, columnName As String) As T
        If dr.Table.Columns.Contains(columnName) AndAlso dr(columnName) IsNot DBNull.Value Then
            Return DirectCast(Convert.ChangeType(dr(columnName), GetType(T)), T)
        End If
        Return Nothing ' Retourne 0 pour Integer, "" pour String, etc.
    End Function
    ''' <summary>
    ''' Récupère l'idTiers à partir du nom, prénom et date de naissance du user.
    ''' Utilise la requête/procédure nommée "getIdTiersByNomPrenomDate" (à créer côté base).
    ''' </summary>
    Public Shared Function GetIdTiersByUser(nom As String, prenom As String, Optional dateNaissance As Date? = Nothing) As Integer
        Try
            ' Simplification de la gestion du DBNull
            Dim param As New Dictionary(Of String, Object) From {
            {"@nom", If(String.IsNullOrWhiteSpace(nom), DBNull.Value, nom)},
            {"@prenom", If(String.IsNullOrWhiteSpace(prenom), DBNull.Value, prenom)},
            {"@dateNaissance", If(dateNaissance, DBNull.Value)}
        }

            Using Reader As SqlDataReader = SqlCommandBuilder.
            CreateSqlCommand(Constantes.bddAgumaaa, "getIdTiersByNomPrenomDate", param).
            ExecuteReader()

                If Reader.Read() Then
                    ' Utilisation de votre extension GetValueOrDefault pour plus de sécurité
                    Return Reader.GetValueOrDefault(Of Integer)(0, 0)
                End If
            End Using

            ' Retour par défaut si aucun enregistrement n'est trouvé (Règle BC42353)
            Return 0

        Catch ex As Exception
            Logger.ERR($"GetIdTiersByUser({prenom} {nom}) : {ex.Message}")
            Return 0 ' Un Integer ne peut pas être Nothing, on retourne 0
        End Try
    End Function

    ''' <summary>
    ''' Représentation textuelle du tiers.
    ''' Priorité : RaisonSociale > "Nom Prénom (DateNaissance)" > Nom Prénom.
    ''' Date au format français dd/MM/yyyy si disponible.
    ''' </summary>
    Public Overrides Function ToString() As String
        Try
            ' Si raison sociale renseignée, l'afficher directement
            If Not String.IsNullOrWhiteSpace(RaisonSociale) Then
                Return RaisonSociale
            End If

            ' Construire le nom complet
            Dim parts As New List(Of String)
            If Not String.IsNullOrWhiteSpace(Nom) Then parts.Add(Nom.Trim())
            If Not String.IsNullOrWhiteSpace(Prenom) Then parts.Add(Prenom.Trim())

            Dim fullName As String = String.Join(" ", parts).Trim()

            ' Si date de naissance valide (non nulle)
            If DateNaissance <> Date.MinValue Then
                Dim dateStr As String = DateNaissance.ToString("dd/MM/yyyy")
                If String.IsNullOrWhiteSpace(fullName) Then
                    Return dateStr
                Else
                    Return $"{fullName} ({dateStr})"
                End If
            End If

            If String.IsNullOrWhiteSpace(fullName) Then
                Return MyBase.ToString()
            End If

            Return fullName
        Catch ex As Exception
            Logger.ERR($"ToString() Tiers Id={Id} : {ex.Message}")
            Return MyBase.ToString()
        End Try
    End Function
End Class