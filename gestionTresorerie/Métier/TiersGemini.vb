Imports System.Data
Imports System.Data.SqlClient

' BaseDataRow doit être défini ailleurs dans votre projet
Public Class Tiers
    Inherits BaseDataRow

    '-------------------------------------------------------------------------
    ' 1. Propriétés (simplifiées)
    '-------------------------------------------------------------------------
    
    ' id doit être en Property avec Backing Field ou en Auto Property si mutable
    ' On le laisse ReadOnly comme demandé, mais on utilise un champ privé pour l'affecter.
    Private ReadOnly _id As Integer 
    Public ReadOnly Property Id As Integer
        Get
            Return _id
        End Get
    End Property

    Public Property RaisonSociale As String
    Public Property Nom As String
    Public Property Prenom As String
    Public Property CategorieDefaut As Integer
    Public Property SousCategorieDefaut As Integer

    ' Utilisation des Auto Properties pour les dates avec l'initialisation automatique des Setters
    Private _dateCreation As Date
    Private _dateModification As Date
    
    ' Correction : Le setter n'a plus besoin de redéfinir _dateCreation.
    ' Il suffit d'appeler le Setter si vous voulez mettre à jour.
    Public Property DateCreation As Date
        Get
            Return _dateCreation
        End Get
        Set(value As Date)
            _dateCreation = value
        End Set
    End Property
    
    ' Le setter automatique de dateModification doit être appelé lorsque l'objet est modifié.
    Public Property DateModification As Date
        Get
            Return _dateModification
        End Get
        Set(value As Date)
            _dateModification = value
        End Set
    End Property
    
    '-------------------------------------------------------------------------
    ' 2. Collection d'Adresses avec Lazy Loading
    '-------------------------------------------------------------------------

    Private _adresses As List(Of Adresse) = Nothing
    
    ' Utilise un champ privé pour stocker la DAO afin de ne pas la recréer constamment.
    Private ReadOnly _adresseDao As New AdresseDAO() 

    Public ReadOnly Property Adresses As List(Of Adresse)
        Get
            ' Si la liste est vide, on la charge
            If _adresses Is Nothing Then
                ' Lazy Loading: Charge les adresses la première fois qu'elles sont demandées
                If Me.Id > 0 Then
                     _adresses = _adresseDao.LireAdressesParTiers(Me.Id)
                Else
                     _adresses = New List(Of Adresse)() ' Liste vide si pas encore sauvegardé
                End If
               
            End If
            Return _adresses
        End Get
    End Property

    '-------------------------------------------------------------------------
    ' 3. Constructeurs (Utilisation du champ privé _id)
    '-------------------------------------------------------------------------

    Public Sub New()
        ' Initialisation des dates par défaut si l'objet est nouveau
        Me.DateCreation = Now
        Me.DateModification = Now
    End Sub
    
    ' Constructeur Personne Physique
    Public Sub New(id As Integer, sNom As String, sPrenom As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        Me.New() ' Appelle le constructeur sans argument pour initialiser les dates
        Me._id = id
        Me.Nom = sNom
        Me.Prenom = sPrenom
        Me.CategorieDefaut = sCategorie
        Me.SousCategorieDefaut = sSousCategorie
    End Sub
    
    ' Constructeur Personne Morale
    Public Sub New(id As Integer, sRaisonSociale As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        Me.New() ' Appelle le constructeur sans argument pour initialiser les dates
        Me._id = id
        Me.RaisonSociale = sRaisonSociale
        Me.CategorieDefaut = sCategorie
        Me.SousCategorieDefaut = sSousCategorie
    End Sub

    '-------------------------------------------------------------------------
    ' 4. Méthodes
    '-------------------------------------------------------------------------

    Public Overrides Function ResumeTexte() As String
        If Not String.IsNullOrEmpty(RaisonSociale) Then
            Return RaisonSociale
        Else
            Return $"{Prenom} {Nom}".Trim()
        End If
    End Function
    
    ' --- Méthode Shared pour convertir un DataRow en Tiers ---
    ' C'est le seul type de méthode statique (Shared) qui a sa place dans le modèle, 
    ' car elle est liée à la construction de l'objet à partir de données brutes.
    Public Shared Function FromDataRow(dr As DataRow) As Tiers
        If dr Is Nothing Then Return Nothing

        ' Simplification des conversions : Assurez-vous que les colonnes existent
        Dim id As Integer = If(dr.Table.Columns.Contains("id") AndAlso dr("id") IsNot DBNull.Value, Convert.ToInt32(dr("id")), 0)
        Dim categorie As Integer = If(dr.Table.Columns.Contains("categorieDefaut") AndAlso dr("categorieDefaut") IsNot DBNull.Value, Convert.ToInt32(dr("categorieDefaut")), 0)
        Dim sousCategorie As Integer = If(dr.Table.Columns.Contains("sousCategorieDefaut") AndAlso dr("sousCategorieDefaut") IsNot DBNull.Value, Convert.ToInt32(dr("sousCategorieDefaut")), 0)
        Dim dateCrea As Date = If(dr.Table.Columns.Contains("dateCreation") AndAlso dr("dateCreation") IsNot DBNull.Value, Convert.ToDateTime(dr("dateCreation")), Now)
        Dim dateModif As Date = If(dr.Table.Columns.Contains("dateModification") AndAlso dr("dateModification") IsNot DBNull.Value, Convert.ToDateTime(dr("dateModification")), Now)
        
        Dim nouveauTiers As Tiers

        If dr.Table.Columns.Contains("raisonSociale") AndAlso Not String.IsNullOrWhiteSpace(dr("raisonSociale").ToString()) Then
            nouveauTiers = New Tiers(id, dr("raisonSociale").ToString(), categorie, sousCategorie)
        Else
            Dim nom As String = If(dr.Table.Columns.Contains("nom"), dr("nom").ToString(), "")
            Dim prenom As String = If(dr.Table.Columns.Contains("prenom"), dr("prenom").ToString(), "")
            nouveauTiers = New Tiers(id, nom, prenom, categorie, sousCategorie)
        End If
        
        nouveauTiers.DateCreation = dateCrea
        nouveauTiers.DateModification = dateModif
        
        Return nouveauTiers
    End Function

End Class