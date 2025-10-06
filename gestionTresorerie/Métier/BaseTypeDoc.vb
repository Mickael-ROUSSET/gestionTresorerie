' Classe de base abstraite implémentant ITypeDoc
Imports System.Data.SqlClient
Imports System.IO

Public MustInherit Class BaseTypeDoc
    Implements ITypeDoc

    ' Champs privés
    Private _prompt As String
    Private _gabaritRepertoire As String
    Private _gabaritNomFichier As String
    Private _classe As String
    Private _jsonMetaDonnées As String

    ' Constructeur protégé pour les classes dérivées
    Protected Sub New()
    End Sub

    ' Propriétés de ITypeDoc
    Public Property Prompt As String Implements ITypeDoc.Prompt
        Get
            Return _prompt
        End Get
        Set(value As String)
            _prompt = value
        End Set
    End Property

    Public Property GabaritRepertoire As String Implements ITypeDoc.GabaritRepertoire
        Get
            Return _gabaritRepertoire
        End Get
        Set(value As String)
            _gabaritRepertoire = value
        End Set
    End Property

    Public Property GabaritNomFichier As String Implements ITypeDoc.GabaritNomFichier
        Get
            Return _gabaritNomFichier
        End Get
        Set(value As String)
            _gabaritNomFichier = value
        End Set
    End Property

    Public Property jsonMetaDonnées As String Implements ITypeDoc.JsonMetaDonnées
        Get
            Return _jsonMetaDonnées
        End Get
        Set(value As String)
            _jsonMetaDonnées = value
        End Set
    End Property

    Public Property ContenuBase64 As String Implements ITypeDoc.ContenuBase64
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property ClasseTypeDoc As String Implements ITypeDoc.ClasseTypeDoc
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property


    ' Méthode abstraite pour renommerFichier (doit être implémentée par les classes dérivées)


    ' Méthode commune pour remplacer les motifs d'année
    Public Overridable Function RemplacerAnnees(sRepertoire As String) As String
        ' Obtenir l'année en cours
        Dim anneeEnCours As Integer = DateTime.Now.Year
        Dim anneeSuivante As Integer = anneeEnCours + 1

        ' Vérifier quel motif est présent et effectuer le remplacement
        If sRepertoire.Contains("{SSAA}") Then
            Return sRepertoire.Replace("{SSAA}", anneeEnCours.ToString())
        ElseIf sRepertoire.Contains("{SCO_SSAA}") Then
            Return sRepertoire.Replace("{SCO_SSAA}", $"{anneeEnCours}-{anneeSuivante}")
        End If

        ' Retourner la chaîne inchangée si aucun motif n'est trouvé
        Return sRepertoire
    End Function

    ' Méthode abstraite pour InsereEnbase (doit être implémentée par les classes dérivées) 
    'Public Sub InsereEnBase(doc As BaseTypeDoc) Implements ITypeDoc.InsereEnbase
    '    Try
    '        ' Lire l'image en tant que tableau d'octets  
    '        '<Value Profile = "(Default)" > INSERT INTO [dbo].[Documents] (
    '        'dateDoc,
    '        'contenuDoc,
    '        'cheminDoc,
    '        'categorieDoc,
    '        'sousCategorieDoc,
    '        'idMvtDoc,
    '        'metaDonnees) VALUES (@dateDoc, @contenuDoc, @cheminDoc, @categorieDoc, @sousCategorieDoc, @idMvtDoc, @metaDonnees);</Value>
    '        Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand("insertDocAgumaaa",
    '                         New Dictionary(Of String, Object) From {{"@dateDoc", Now.Date & Now.Hour},
    '                                                                 {"@contenuDoc", doc},
    '                                                                 {"@cheminDoc", """"},
    '                                                                 {"@categorieDoc", ""},
    '                                                                 {"@sousCategorieDoc", ""},
    '                                                                 {"@metaDonnees", ""}}
    '                         )
    '        command.ExecuteNonQuery()
    '        command.ExecuteNonQuery()
    '        'End Using
    '        Logger.INFO("Données insérées avec succès." & command.ToString)
    '    Catch ex As Exception
    '        Logger.ERR("Erreur lors de l'insertion des données : " & ex.Message)
    '    End Try
    'End Sub
End Class