Imports System.IO

Public Class QuestionnaireSanteGym
    Inherits DocumentAgumaaa
    Private _jsonMetaDonnées As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String, nomUsage As String, dateNaissance As Date)
        Me.Nom = nom
        Me.Prenom = prenom
        Me.NomUsage = nomUsage
        Me.DateNaissance = dateNaissance
    End Sub

    ' Propriétés
    Public Property Nom As String

    Public Property Prenom As String

    Public Property NomUsage As String

    Public Property DateNaissance As Date
    Public Overrides Function RenommerFichier(sChemin As String, Optional sNouveauNom As String = "") As String
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
        Return sRepDestination
    End Function
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