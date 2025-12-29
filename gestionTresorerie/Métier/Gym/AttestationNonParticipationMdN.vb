Imports System.IO

Public Class AttestationNonParticipationMdN
    Inherits DocumentAgumaaa

    Private _jsonMetaDonnées As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nom As String, prenom As String)
        Me.Nom = nom
        Me.Prenom = prenom
    End Sub

    ' Propriétés
    Public Property Nom As String

    Public Property Prenom As String

    Public Overrides Function RenommerFichier(sChemin As String, Optional sNouveauNom As String = "") As String
        Dim sRepDestination As String
        sRepDestination = path.combine(LectureProprietes.GetVariable("repRacineAgumaaa") , 
										LectureProprietes.GetVariable("repRacineDocuments") , 
										LectureProprietes.GetVariable("repRacineMdN") , 
										DateTime.Now.Year.ToString , 
										LectureProprietes.GetVariable("repAttestationsNonParticipationAutresMarches")
										)
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