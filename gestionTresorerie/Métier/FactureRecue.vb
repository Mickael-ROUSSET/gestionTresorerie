Imports System.IO

Public Class FactureRecue
    Inherits DocumentAgumaaa
    Private _nomSociete As String
    Private _reference As String
    Private _dateFacture As Date
    Private _montant As Decimal
    Private _jsonMetaDonnées As String

    ' Constructeur par défaut
    Public Sub New()
    End Sub

    ' Constructeur avec paramètres
    Public Sub New(nomSociete As String, reference As String, dateFacture As Date, montant As Decimal)
        _nomSociete = nomSociete
        _reference = reference
        _dateFacture = dateFacture
        _montant = montant
    End Sub

    ' Propriétés
    Public Property NomSociete As String
        Get
            Return _nomSociete
        End Get
        Set(value As String)
            _nomSociete = value
        End Set
    End Property

    Public Property Reference As String
        Get
            Return _reference
        End Get
        Set(value As String)
            _reference = value
        End Set
    End Property

    Public Property dateFacture As Date
        Get
            Return _dateFacture
        End Get
        Set(value As Date)
            _dateFacture = value
        End Set
    End Property

    Public Property Montant As Decimal
        Get
            Return _montant
        End Get
        Set(value As Decimal)
            _montant = value
        End Set
    End Property

    Public Overrides Sub RenommerFichier(sChemin As String, Optional sNouveauNom As String = "")
        Dim sRepDestination As String
        sRepDestination = LectureProprietes.GetVariable("repRacineAgumaaa") _
            & LectureProprietes.GetVariable("repRacineComptabilité") _
            & LectureProprietes.GetVariable("repFichiersDocumentsFactures") _
            & "\" & DateTime.Now.Year.ToString _
            & LectureProprietes.GetVariable("repFichiersFacturesEmises")
        'TODO trouver comment orienter vers "Emises" ou "recues"
        '& LectureProprietes.GetVariable("repFichiersFacturesRecues")
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