Imports System.Data.SqlClient

Public MustInherit Class DocumentAgumaaa
    'Identifiant interne
    Public Property IdDoc As Integer
    'Identifiant du mouvement associé
    Public Property IdMvtDoc As Integer
    ' Propriété pour dateDoc
    Public Property DateDoc As Date
    ' Propriété pour cheminDoc
    Public Property CheminDoc As String
    ' Propriété pour categorieDoc
    Public Property CategorieDoc As String
    ' Propriété pour sousCategorieDoc
    Public Property SousCategorieDoc As String
    ' Propriété pour les méta donnees JSON
    Public Property metaDonnees As String
    ' Propriété pour la date de modification
    Public Property dateModif As String
    ' Propriété pour le contenu du document (base64)
    Private _contenuDoc As String

    Public Sub New()
    End Sub
    Public Sub New(idDoc As Integer,
                   dateDoc As Date,
                   contenuDoc As String,
                   cheminDoc As String,
                   categorieDoc As String,
                   sousCategorieDoc As String,
                   idMvtDoc As Integer,
                   metaDonnees As String,
                   dateModif As String)
        Me.IdDoc = idDoc
        Me.IdMvtDoc = idMvtDoc
        Me.DateDoc = dateDoc
        Me.ContenuDoc = contenuDoc
        Me.CheminDoc = cheminDoc
        Me.CategorieDoc = categorieDoc
        Me.SousCategorieDoc = sousCategorieDoc
        Me.metaDonnees = metaDonnees
        Me.dateModif = dateModif
    End Sub
    Private Shared Function CreateRepository() As DocumentRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).
                    GetConnexion(Constantes.DataBases.Agumaaa).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New DocumentRepository(executor, factory, provider)
    End Function
    Public Property ContenuDoc As String
        Get
            Return _contenuDoc
        End Get
        Set(value As String)
            If Not String.IsNullOrEmpty(value) Then
                Try
                    ' Vérifier si la chaîne est un Base64 valide
                    Dim unused = Convert.FromBase64String(value)
                Catch ex As FormatException
                    Logger.ERR($"ContenuDoc n'est pas une chaîne Base64 valide : {ex.Message}")
                    Throw New ArgumentException("La valeur de ContenuDoc doit être une chaîne Base64 valide.")
                End Try
            End If
            _contenuDoc = value
        End Set
    End Property
    Public Shared Sub InsererDocument(doc As DocumentAgumaaa)
        Try
            CreateRepository().Inserer(doc)
            Logger.INFO($"Document {doc.IdMvtDoc} inséré avec succès.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'insertion du document : {ex.Message}")
            Throw
        End Try
    End Sub
    Public Shared Function LireDocuments() As DataTable
        Try
            Return CreateRepository().LireTous()
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la lecture des documents : {ex.Message}")
            Return New DataTable()
        End Try
    End Function
    Public Shared Sub MettreAJourDocument(idDoc As Integer,
                                      dateDoc As Date,
                                      contenuDoc As String,
                                      cheminDoc As String,
                                      categorieDoc As String,
                                      sousCategorieDoc As String,
                                      idMvtDoc As Integer)
        Try
            CreateRepository().MettreAJour(idDoc, dateDoc, contenuDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc)
            Logger.INFO($"Document {idDoc} mis à jour avec succès.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la mise à jour du document : {ex.Message}")
            Throw
        End Try
    End Sub
    Public Shared Sub SupprimerDocument(idDoc As Integer)
        Try
            CreateRepository().Supprimer(idDoc)
            Logger.INFO($"Document {idDoc} supprimé avec succès.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la suppression du document : {ex.Message}")
            Throw
        End Try
    End Sub
    Public MustOverride Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
    ''' <summary>
    ''' Retourne un résumé textuel propre du document
    ''' </summary>
    Public Overrides Function ToString() As String
        Try
            ' On essaie de récupérer le type et une info clé pour le log
            Dim typeInfo As String = If(Not String.IsNullOrEmpty(CategorieDoc), CategorieDoc, "Document Inconnu")
            Dim dateInfo As String = DateDoc.ToString("dd/MM/yyyy HH:mm")

            ' Optionnel : Si tu veux extraire une info du JSON pour le ToString
            ' Dim montant = ExtraireMontantDuJson(metaDonnees) 

            Return $"[{typeInfo}] traité le {dateInfo}"
        Catch
            Return "DocumentAgumaaa (Erreur de lecture des propriétés)"
        End Try
    End Function
End Class