Imports System.IO

Public Class QuestionnaireSanteGym
    Inherits DocumentAgumaaa
    Private _jsonMetaDonnées As String
    Private _nom As String
    Private _prenom As String
    Private _nomUsage As String
    Private _dateNaissance As Date

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String, nomUsage As String, dateNaissance As Date)
        _nom = nom
        _prenom = prenom
        _nomUsage = nomUsage
        _dateNaissance = dateNaissance
    End Sub

    ' Propriétés
    Public Property Nom As String
        Get
            Return _nom
        End Get
        Set(value As String)
            _nom = value
        End Set
    End Property

    Public Property Prenom As String
        Get
            Return _prenom
        End Get
        Set(value As String)
            _prenom = value
        End Set
    End Property

    Public Property NomUsage As String
        Get
            Return _nomUsage
        End Get
        Set(value As String)
            _nomUsage = value
        End Set
    End Property

    Public Property DateNaissance As Date
        Get
            Return _dateNaissance
        End Get
        Set(value As Date)
            _dateNaissance = value
        End Set
    End Property
    Public Overrides Sub RenommerFichier(sChemin As String, Optional sNouveauNom As String = "")
        'G:\Mon Drive\AGUMAAA\Documents\Manifestations récurrentes\Activités\Gym\2025-2026\QuestionnairesSanté
        Dim anneeEnCours As Integer = DateTime.Now.Year
        Dim anneeSuivante As Integer = anneeEnCours + 1

        Dim sRepDestination As String
        sRepDestination = LectureProprietes.GetVariable("repRacineAgumaaa") &
            LectureProprietes.GetVariable("repRacineDocuments") &
            LectureProprietes.GetVariable("repFichiersGym") &
            anneeEnCours.ToString & "-" & anneeSuivante.ToString &
            LectureProprietes.GetVariable("repGymQuestionnaire")
        Utilitaires.RenommerEtDeplacerFichier(sChemin, determineNouveauNom(sRepDestination))
    End Sub
    Private Function determineNouveauNom(sRepSortie As String) As String

        ' Construire le nouveau chemin complet du fichier dans le répertoire de sortie
        Dim sNom As String = Utilitaires.ExtractStringFromJson(_jsonMetaDonnées, "nom")
        Dim sPrenom As String = Utilitaires.ExtractStringFromJson(_jsonMetaDonnées, "prenom")
        Return Path.Combine(
            sRepSortie,
            $"{sNom}_{sPrenom}"
        )
    End Function
End Class