Imports System.IO

Public Class FactureRecue
    Inherits DocumentAgumaaa

    Private _jsonMetaDonnées As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nomSociete As String, reference As String, dateFacture As Date, montant As Decimal)
        Me.NomSociete = nomSociete
        Me.Reference = reference
        Me.dateFacture = dateFacture
        Me.Montant = montant
    End Sub

    ' Propriétés
    Public Property NomSociete As String

    Public Property Reference As String

    Public Property dateFacture As Date

    Public Property Montant As Decimal

    Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
        Dim sRepDestination As String
        sRepDestination = LectureProprietes.GetVariable("repRacineAgumaaa") _
            & LectureProprietes.GetVariable("repRacineComptabilité") _
            & LectureProprietes.GetVariable("repFichiersDocumentsFactures") _
            & "\" & DateTime.Now.Year.ToString _
            & LectureProprietes.GetVariable("repFichiersFacturesEmises")
        'TODO trouver comment orienter vers "Emises" ou "recues"
        '& LectureProprietes.GetVariable("repFichiersFacturesRecues")
        Utilitaires.RenommerEtDeplacerFichier(sNomFichier, determineNouveauNom(sRepDestination))
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