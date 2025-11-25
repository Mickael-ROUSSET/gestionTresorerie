Imports System.Data.SqlClient
Imports System.IO
Imports DocumentFormat.OpenXml.Office2010.Excel
' Classe concrète implémentant ITypeDoc pour pouvoir instancier des objets
Public Class TypeDocImpl
    Inherits BaseDataRow ' Héritage de BaseDataRow pour compatibilité avec les utilitaires de sélection
    Implements ITypeDoc

    Private _contenuBase64 As String
    Private _jsonMetaDonnées As String

    ' Implémentation des propriétés de l'interface
    Public Property Prompt As String Implements ITypeDoc.Prompt
    ' Implémentation des propriétés de l'interface
    Public Property Libellé As String Implements ITypeDoc.Libellé
    Public Property GabaritRepertoire As String Implements ITypeDoc.GabaritRepertoire

    Public Property GabaritNomFichier As String Implements ITypeDoc.GabaritNomFichier

    Public Property ClasseTypeDoc As String Implements ITypeDoc.ClasseTypeDoc
    Public Sub New()

    End Sub
    Public Overrides Sub LoadFromReader(reader As SqlDataReader)

    End Sub

    Public Shared Function EncodeImageToBase64(filePath As String) As String
        ' Lire le fichier image en tant que tableau d'octets
        Dim imageBytes As Byte() = File.ReadAllBytes(filePath)
        ' Convertir le tableau d'octets en une chaîne base64
        Return Convert.ToBase64String(imageBytes)
    End Function
    Public Function RemplacerAnnees(sRepertoire As String) As String
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

    ' Constructeur avec paramètres pour initialisation depuis la base de données
    Public Sub New(prompt As String, gabaritRepertoire As String, gabaritNomFichier As String, classe As String)
        Me.Prompt = prompt
        Me.GabaritRepertoire = gabaritRepertoire
        Me.GabaritNomFichier = gabaritNomFichier
        ClasseTypeDoc = classe
    End Sub

    Public Property JsonMetaDonnées As String Implements ITypeDoc.JsonMetaDonnées
        Get
            JsonMetaDonnées = _jsonMetaDonnées
        End Get
        Set(value As String)
            _jsonMetaDonnées = value
        End Set
    End Property

    Public Property ContenuBase64 As String Implements ITypeDoc.ContenuBase64
        Get
            ContenuBase64 = _contenuBase64
        End Get
        Set(value As String)
            _contenuBase64 = value
        End Set
    End Property
End Class
