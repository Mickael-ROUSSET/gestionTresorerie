Imports System.IO
Imports Newtonsoft.Json.Linq
Public Class Cheque
    Inherits DocumentAgumaaa
    ' --- Identité ---
    Public Property nom_banque As String
    Public Property banque_code_departement As String
    Public Property banque_ville_agence As String
    Public Property banque_telephone_agence As String
    Public Property emetteur_numero_compte As String
    Public Property numero_du_cheque As String ' Stocké en String pour garder les zéros initiaux
    Public Property destinataire As String
    Public Property beneficiaire_adresse_ligne1 As String
    Public Property beneficiaire_adresse_ligne2 As String
    Public Property beneficiaire_code_postal_ville As String

    ' --- Transaction ---
    Public Property montant_numerique As Decimal
    Public Property montant_lettres As String
    Public Property devise As String
    Public Property dateChq As String ' Correspond à date_emission
    Public Property lieu_emission As String
    Public Property date_impression_cheque As String

    ' --- Divers ---
    Public Property autres_infos As String
    Public Property id As Integer

    Public Sub New()
    End Sub

    ''' <summary>
    ''' Analyse le JSON complexe renvoyé par Gemini et mappe les champs vers les propriétés de l'objet.
    ''' </summary>
    Public Shared Function ParseJson(jsonBrut As String) As String
        ' Mapping : "Chemin dans le JSON Gemini" -> "Nom de la propriété dans la classe Cheque"
        Dim fieldMappings As New Dictionary(Of String, String) From {
        {"donnees_extraites.identite.banque_nom", "nom_banque"},
        {"donnees_extraites.identite.banque_code_departement", "banque_code_departement"},
        {"donnees_extraites.identite.banque_ville_agence", "banque_ville_agence"},
        {"donnees_extraites.identite.banque_telephone_agence", "banque_telephone_agence"},
        {"donnees_extraites.identite.emetteur_numero_compte", "emetteur_numero_compte"},
        {"donnees_extraites.identite.emetteur_numero_cheque", "numero_du_cheque"},
        {"donnees_extraites.identite.beneficiaire_nom", "destinataire"},
        {"donnees_extraites.identite.beneficiaire_adresse_ligne1", "beneficiaire_adresse_ligne1"},
        {"donnees_extraites.identite.beneficiaire_adresse_ligne2", "beneficiaire_adresse_ligne2"},
        {"donnees_extraites.identite.beneficiaire_code_postal_ville", "beneficiaire_code_postal_ville"},
        {"donnees_extraites.transaction.montant_numerique", "montant_numerique"},
        {"donnees_extraites.transaction.montant_lettres", "montant_lettres"},
        {"donnees_extraites.transaction.devise", "devise"},
        {"donnees_extraites.transaction.date_emission", "dateChq"},
        {"donnees_extraites.transaction.lieu_emission", "lieu_emission"},
        {"donnees_extraites.transaction.date_impression_cheque", "date_impression_cheque"},
        {"donnees_extraites.autres_infos", "autres_infos"}
    }

        ' Appelle l'utilitaire pour aplatir le JSON et convertir les types (décimaux, etc.)
        Return Utilitaires.ParseJson(jsonBrut, fieldMappings)
    End Function

    ' --- Méthodes d'affichage d'image (Inchangées mais optimisées) ---

    Public Shared Sub AfficherImage(idDoc As Integer, pbBox As PictureBox)
        Try
            ' Effacer l'image précédemment affichée
            pbBox.Image = Nothing
            Dim imageData As Object = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "reqImagesChq",
                                           New Dictionary(Of String, Object) From {{"@Id", idDoc}
                                            }).
                                            ExecuteScalar()
            If imageData IsNot Nothing AndAlso TypeOf imageData Is Byte() Then
                ' Convertir les données binaires en objet Image
                Dim imageBytes As Byte() = DirectCast(imageData, Byte())
                Using ms As New IO.MemoryStream(imageBytes)
                    Dim image As Image = Image.FromStream(ms)
                    ' Afficher l'image dans le PictureBox
                    pbBox.SizeMode = PictureBoxSizeMode.Zoom
                    pbBox.Image = AfficherTiersSuperieurImage(image, 0.33)
                End Using
            Else
                Logger.INFO($"Aucune image trouvée pour cet enregistrement : {idDoc}.")
            End If
            'End Using
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la récupération de l'image : {idDoc}, {ex.Message}")
        End Try
    End Sub

    Public Shared Function AfficherTiersSuperieurImage(image As Image, ratio As Double) As Image
        ' Renvoie seulement le ratio (33% par défaut) supérieur de l'image car les chèques sont scannés en A4
        Try
            Dim hauteurTiers As Integer = CInt(image.Height * ratio)
            ' Rectangle de découpe (on enlève souvent une marge à gauche pour les scans)
            Dim rect As New Rectangle(450, 0, image.Width - 450, hauteurTiers)
            Dim bmp As New Bitmap(rect.Width, rect.Height)

            Using g As Graphics = Graphics.FromImage(bmp)
                g.DrawImage(image, New Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel)
            End Using
            Return bmp
        Catch ex As Exception
            Logger.ERR("Erreur découpe image : " & ex.Message)
            Return New Bitmap(1, 1)
        End Try
    End Function

    ' --- Gestion des fichiers ---
    Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
        ' Détermination du sous-répertoire (Emis ou Reçus)
        Dim sens As String = IIf(emetteur_numero_compte.ToUpper().Contains(LectureProprietes.GetVariable("numCptAgumaaa")), "Emis", "Reçus")

        Dim sRepDestination As String = Path.Combine(
            LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineComptabilité"),
            LectureProprietes.GetVariable("repFichiersDocumentsChèques"),
            DateTime.Now.Year.ToString(),
            sens
        )

        If String.IsNullOrEmpty(sRepDestination) Then Throw New Exception("Répertoire destination invalide.")

        Dim nouveauNom As String = determineNouveauNomFichier(sNomFichier)
        Dim cheminComplet As String = Path.Combine(sRepDestination, nouveauNom)

        Utilitaires.RenommerEtDeplacerFichier(sNomFichier, cheminComplet)
        Return cheminComplet
    End Function

    Private Function determineNouveauNomFichier(sNomFichier As String) As String
        ' Le numéro est déjà dans la propriété grâce à PopulateObject
        Dim num As String = Me.numero_du_cheque

        If String.IsNullOrEmpty(num) Then
            num = "INCONNU_" & DateTime.Now.ToString("HHmmss")
        End If

        Return $"CHQ_{num}{Path.GetExtension(sNomFichier)}"
    End Function
End Class