Imports System.IO

Public Class AttestationNonParticipationMdN
    Inherits DocumentAgumaaa
    Private _nom As String
    Private _prenom As String
    Private _jsonMetaDonnées As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String)
        _nom = nom
        _prenom = prenom
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

    Public Overrides Function RenommerFichier(sChemin As String, Optional sNouveauNom As String = "") As String
        Dim sRepDestination As String
        sRepDestination = LectureProprietes.GetVariable("repRacineAgumaaa") _
            & LectureProprietes.GetVariable("repRacineDocuments") _
            & LectureProprietes.GetVariable("repRacineMdN") _
            & "\" & DateTime.Now.Year.ToString _
            & LectureProprietes.GetVariable("repAttestationsNonParticipationAutresMarches")
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