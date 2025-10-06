' Exemple de classe AdhesionGym implémentant ITypeDoc
Imports System.Data.SqlClient
Imports System.IO
Imports DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing
Imports Newtonsoft.Json.Linq
Imports Windows.Win32.System

Public Class AdhesionGym
    Implements ITypeDoc

    Private _prompt As String
    Private _gabaritRepertoire As String
    Private _gabaritNomFichier As String
    Private _nomUsage As String
    Private _email As String
    Private _telephone As String
    Private _adresse As String
    Private _ville As String
    Private _classe As String
    Private _codePostal As Integer
    Private _jsonMetaDonnées As String

    ' Champs spécifiques à AdhesionGym
    Private _nom As String
    Private _prenom As String
    Private _dateNaissance As Date

    Public Sub New(nom As String, prenom As String, nomUsage As String, dateNaissance As String, email As String, telephone As String, adresse As String, ville As String, codePostal As Integer)
        _nom = nom
        _prenom = prenom
        _nomUsage = nomUsage
        _dateNaissance = dateNaissance
        _email = email
        _telephone = telephone
        _adresse = adresse
        _ville = ville
        _codePostal = codePostal
        _prompt = "Saisir les informations d'adhésion"
        _gabaritRepertoire = "\Gabarits\Adhesion"
        _gabaritNomFichier = "Adhesion.docx"
        _classe = "AdhesionGym"
        _jsonMetaDonnées = GenerateJsonMetaDonnees()
    End Sub

    ' Générer le JSON pour les métadonnées
    Private Function GenerateJsonMetaDonnees() As String
        Dim jsonObj As New JObject()
        jsonObj("nom") = _nom
        jsonObj("prenom") = _prenom
        jsonObj("dateNaissance") = _dateNaissance.ToString("yyyy-MM-dd")
        Return jsonObj.ToString(Newtonsoft.Json.Formatting.None)
    End Function

    ' Propriétés de l'interface
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

    Public Property ClasseTypeDoc As String Implements ITypeDoc.ClasseTypeDoc
        Get
            Return _classe
        End Get
        Set(value As String)
            _classe = value
        End Set
    End Property

    Public Property JsonMetaDonnées As String Implements ITypeDoc.jsonMetaDonnées
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
    Private Function determineNouveauNom(sRepSortie As String) As String

        ' Construire le nouveau chemin complet du fichier dans le répertoire de sortie
        Dim nom As String = Utilitaires.ExtractStringFromJson(_jsonMetaDonnées, "Nom")
        Dim prenom As String = Utilitaires.ExtractStringFromJson(_jsonMetaDonnées, "Prénom")
        Return Path.Combine(
            sRepSortie,
            $"{nom}_{prenom}"
        )

    End Function
    Public Sub renommerFichier(sChemin As String) Implements ITypeDoc.renommerFichier
        'G:\Mon Drive\AGUMAAA\Documents\Manifestations récurrentes\Activités\Gym\2025-2026\Formulaires d'adhésion
        Dim anneeEnCours As Integer = DateTime.Now.Year
        Dim anneeSuivante As Integer = anneeEnCours + 1

        Dim sRepSortie As String
        sRepSortie = LectureProprietes.GetVariable("repRacineAgumaaa") &
            LectureProprietes.GetVariable("repRacineDocuments") &
            LectureProprietes.GetVariable("repFichiersGym") &
            anneeEnCours.ToString & "-" & anneeSuivante.ToString &
            LectureProprietes.GetVariable("repGymAdhésion")
    End Sub

End Class