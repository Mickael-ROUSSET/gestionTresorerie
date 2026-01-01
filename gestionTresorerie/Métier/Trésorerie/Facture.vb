Imports System.IO

Public Class Facture
    Inherits DocumentAgumaaa
    'Private _jsonMetaDonnées As String
    ' --- Propriétés Identité ---
    Public Property fournisseur_nom As String
    Public Property fournisseur_siret As String
    Public Property fournisseur_iban As String
    Public Property adherent_nom As String
    Public Property adherent_numero As String
    Public Property adherent_ville As String
    ' --- Propriétés Transaction ---
    Public Property montant_ht As Decimal
    Public Property montant_tva As Decimal
    Public Property montant_ttc As Decimal
    Public Property devise As String
    Public Property date_facture As String
    Public Property date_echeance As String
    Public Property numero_facture As String
    ' --- Autres ---
    Public Property autres_infos As String

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
    ''' <summary>
    ''' Mappe le JSON structuré de Gemini vers les propriétés de la classe Facture
    ''' </summary>
    Public Shared Function ParseJson(jsonBrut As String) As String
        ' Mapping : "Chemin dans le JSON de l'IA" -> "Nom de la propriété VB"
        Dim fieldMappings As New Dictionary(Of String, String) From {
            {"donnees_extraites.identite.fournisseur_nom", "fournisseur_nom"},
            {"donnees_extraites.identite.fournisseur_siret", "fournisseur_siret"},
            {"donnees_extraites.identite.fournisseur_iban", "fournisseur_iban"},
            {"donnees_extraites.identite.adherent_nom", "adherent_nom"},
            {"donnees_extraites.identite.adherent_numero", "adherent_numero"},
            {"donnees_extraites.identite.adherent_ville", "adherent_ville"},
            {"donnees_extraites.transaction.montant_ht", "montant_ht"},
            {"donnees_extraites.transaction.montant_tva", "montant_tva"},
            {"donnees_extraites.transaction.montant_ttc", "montant_ttc"},
            {"donnees_extraites.transaction.devise", "devise"},
            {"donnees_extraites.transaction.date_facture", "date_facture"},
            {"donnees_extraites.transaction.date_echeance", "date_echeance"},
            {"donnees_extraites.transaction.numero_facture", "numero_facture"},
            {"donnees_extraites.autres_infos", "autres_infos"}
        }

        ' Utilise l'utilitaire partagé pour aplatir et convertir les données numériques
        Return Utilitaires.ParseJson(jsonBrut, fieldMappings)
    End Function
    ''' <summary>
    ''' Logique de renommage spécifique aux factures
    ''' </summary>
    Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
        ' Organisation par année et par fournisseur
        Dim annee As String = If(Not String.IsNullOrEmpty(Me.date_facture), Me.date_facture.Substring(0, 4), DateTime.Now.Year.ToString())
        Dim nomFournisseur As String = UtilitairesIdentite.NettoyerNomFichier(Me.fournisseur_nom)
        Dim sens As String = IIf(nomFournisseur.Contains("AGUMAAA", StringComparison.CurrentCultureIgnoreCase), LectureProprietes.GetVariable("repFichiersFacturesEmises"), LectureProprietes.GetVariable("repFichiersFacturesRecues"))

        Dim sRepDestination As String = Path.Combine(
            LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineComptabilité"),
            LectureProprietes.GetVariable("repFichiersDocumentsFactures"),
            sens,
            annee,
            nomFournisseur
        )

        ' Créer le répertoire s'il n'existe pas
        If Not Directory.Exists(sRepDestination) Then Directory.CreateDirectory(sRepDestination)

        Dim nouveauNom As String = $"FAC_{Me.numero_facture}_{nomFournisseur}{Path.GetExtension(sNomFichier)}"
        Dim cheminComplet As String = Path.Combine(sRepDestination, nouveauNom)

        Utilitaires.RenommerEtDeplacerFichier(sNomFichier, cheminComplet)
        Return cheminComplet
    End Function
End Class