Imports System.IO
Imports Newtonsoft.Json.Linq
Public Class Cheque
    Inherits DocumentAgumaaa
    Public _dateChq As String
    Public _numero_du_cheque As Integer
    Public _emetteur As String
    Public _emetteur_du_cheque As String
    Public _montant_numerique As Decimal
    Public _destinataire As String
    Public _id As Integer
    Private _jsonMetaDonnées As String

    Public Sub New()
    End Sub
    Public Sub New(id As Integer, montant_numerique As String, numero_du_cheque As Integer, dateChq As Date, emetteur_du_cheque As String, destinataire As String)
        _id = id
        _montant_numerique = montant_numerique
        _numero_du_cheque = numero_du_cheque
        _dateChq = dateChq
        _emetteur_du_cheque = emetteur_du_cheque
        _destinataire = destinataire
    End Sub
    Public Shared Function ParseJson(json As String) As String
        Dim sJsonCheque As String
        Dim fieldMappings As New Dictionary(Of String, String) From {
                                                                        {"montant_numerique", "decimal"},
                                                                        {"numero_du_cheque", "integer"},
                                                                        {"dateChq", "date"},
                                                                        {"emetteur_du_cheque", "string"},
                                                                        {"le_destinataire", "string"}
                                                                    }
        Return sJsonCheque = Utilitaires.ParseJson(json, fieldMappings)
    End Function
    Public Property destinataire() As String
        Get
            Return _destinataire
        End Get
        Set(ByVal value As String)
            _destinataire = value
        End Set
    End Property
    Public Property montant_numerique() As Decimal
        Get
            Return _montant_numerique
        End Get
        Set(ByVal value As Decimal)
            _montant_numerique = value
        End Set
    End Property
    Public Property emetteur_du_cheque() As String
        Get
            Return _emetteur_du_cheque
        End Get
        Set(ByVal value As String)
            _emetteur_du_cheque = value
        End Set
    End Property
    Public Property emetteur() As String
        Get
            Return _emetteur
        End Get
        Set(ByVal value As String)
            _emetteur = value
        End Set
    End Property
    Public Property numero_du_cheque() As Integer
        Get
            Return _numero_du_cheque
        End Get
        Set(ByVal value As Integer)
            _numero_du_cheque = value
        End Set
    End Property

    Public Property dateChq() As String
        Get
            Return _dateChq
        End Get
        Set(ByVal value As String)
            _dateChq = value
        End Set
    End Property

    Public Property id() As Integer
        Get
            Return _id
        End Get
        Set(ByVal value As Integer)
            _id = value
        End Set
    End Property

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
            ' Calculer la hauteur du tiers supérieur
            Dim tiersSuperieurHauteur As Integer = CInt(image.Height * ratio)

            ' Créer un rectangle pour définir la zone à découper
            Dim rectangleTiersSuperieur As New Rectangle(450, 0, image.Width - 450, tiersSuperieurHauteur)

            ' Créer une nouvelle image pour le tiers supérieur
            Dim tiersSuperieurImage As New Bitmap(rectangleTiersSuperieur.Width, rectangleTiersSuperieur.Height)

            ' Dessiner la partie supérieure de l'image originale sur la nouvelle image
            Using g As Graphics = Graphics.FromImage(tiersSuperieurImage)
                g.DrawImage(image, New Rectangle(0, 0, rectangleTiersSuperieur.Width, rectangleTiersSuperieur.Height), rectangleTiersSuperieur, GraphicsUnit.Pixel)
            End Using

            ' Logger l'information
            Logger.INFO("Tiers supérieur de l'image affiché avec succès.")

            ' Retourner l'image du tiers supérieur
            Return tiersSuperieurImage

        Catch ex As Exception
            ' Logger l'erreur
            Logger.ERR("Erreur lors de l'extraction du tiers supérieur de l'image : " & ex.Message)
            ' Retourner une image vide en cas d'erreur
            Return New Bitmap(1, 1)
        End Try
    End Function

    Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
        Dim sRepDestination As String
        sRepDestination = LectureProprietes.GetVariable("repRacineAgumaaa") _
            & LectureProprietes.GetVariable("repRacineComptabilité") _
            & LectureProprietes.GetVariable("repFichiersDocumentsChèques") _
            & "\" & DateTime.Now.Year.ToString _
            & "\" & IIf(_emetteur_du_cheque = "AGUMAAA", "Emis", "Reçus")
        ' Vérifier si le répertoire de sortie est valide
        If String.IsNullOrEmpty(sRepDestination) Then
            Logger.ERR($"Le répertoire de sortie {sRepDestination} est vide ou null.")
            Throw New ArgumentException("Le répertoire de sortie ne peut pas être vide ou null.", NameOf(sRepDestination))
        End If
        Utilitaires.RenommerEtDeplacerFichier(sNomFichier, sRepDestination & "\" & determineNouveauNomFichier(sNomFichier))
        'Retourne le nouveau nom du fichier avec le chemin
        Return sRepDestination & "\" & determineNouveauNomFichier(sNomFichier)
    End Function
    Private Function determineNouveauNomFichier(sNomFichier As String) As String
        Try

            ' Vérifier si metaDonnees est valide
            If String.IsNullOrEmpty(metaDonnees) Then
                Logger.ERR("_jsonMetaDonnées est vide ou null.")
                Throw New InvalidOperationException("Les métadonnées JSON sont vides ou null.")
            End If

            ' Valider que metaDonnees est un JSON valide
            Dim jsonMeta As JObject
            Try
                jsonMeta = JObject.Parse(metaDonnees)
            Catch ex As Exception
                Logger.ERR($"Erreur lors du parsing de metaDonnees : {ex.Message}")
                Throw New InvalidOperationException("Les métadonnées JSON ne sont pas valides.", ex)
            End Try

            ' Extraire numero_du_cheque
            Dim numeroChq As String = Utilitaires.ExtractStringFromJson(metaDonnees, "numero_du_cheque")
            If String.IsNullOrEmpty(numeroChq) Then
                Logger.ERR("Le champ 'numero_du_cheque' est vide ou non trouvé dans metaDonnees.")
                Throw New InvalidOperationException("Le numéro du chèque est vide ou non trouvé dans les métadonnées.")
            End If

            ' Construire le nouveau chemin complet
            'Dim sNouveauNomFichier As String = $"CHQ_{numeroChq}{Path.GetExtension(sNomFichier)}"
            'Dim nouveauNom As String = Path.Combine(Path.GetFullPath(sNomFichier), sNouveauNomFichier)
            Dim nouveauNom As String = $"CHQ_{numeroChq}{Path.GetExtension(sNomFichier)}"

            Logger.INFO($"Nouveau nom déterminé : {nouveauNom}")
            Return nouveauNom
        Catch ex As Exception
            Logger.ERR($"Erreur dans determineNouveauNomFichier : {ex.Message}")
            Throw ' Relever l'exception pour permettre à l'appelant de gérer l'erreur
        End Try
    End Function
End Class