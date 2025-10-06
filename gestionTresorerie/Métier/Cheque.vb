Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions
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

    Public Shared Function ParseJson(json As String) As String
        Try
            ' Parse le JSON d'entrée
            Dim objJson As JObject = JObject.Parse(json)
            Dim choix As JArray = objJson("choices")
            Dim referenceMessage As IList(Of JToken) = choix(0).Children().ToList()

            ' Créer un objet pour stocker les résultats
            Dim resultat As New JObject()

            For Each item As JProperty In referenceMessage
                item.CreateReader()

                Select Case item.Name
                    Case "message"
                        Dim message As String = item.Value.ToString()
                        Dim objMsg As JObject = JObject.Parse(message)
                        Dim content As String = objMsg("content").ToString()
                        Dim resultatJson As String = ExtractAndCleanJson(content)
                        Dim objResultat As JObject = JObject.Parse(resultatJson)

                        ' Remplir l'objet JSON avec les champs demandés
                        resultat("montant_numerique") = Utilitaires.convertStringToDecimal(objResultat.Item(NameOf(montant_numerique)).ToString())
                        resultat("numero_du_cheque") = CInt(objResultat.Item(NameOf(numero_du_cheque)).ToString())
                        resultat("dateChq") = CDate(objResultat.Item(NameOf(dateChq)).ToString()).ToString("yyyy-MM-dd")
                        resultat("emetteur_du_cheque") = objResultat.Item(NameOf(emetteur_du_cheque)).ToString()
                        resultat("destinataire") = objResultat.Item("le_destinataire").ToString()

                    Case Else
                        ' Logger l'information
                        Logger.DBG("Propriété non reconnue : " & item.Name)
                End Select
            Next

            ' Retourner le JSON sérialisé
            Return resultat.ToString(Newtonsoft.Json.Formatting.None)
        Catch ex As Exception
            Logger.ERR("Erreur lors du parsing JSON : " & ex.Message)
            Return "{}" ' Retourner un JSON vide en cas d'erreur
        End Try
    End Function
    'Sub ParseJson(json As String)
    '    ' Parse the JSON string
    '    'Dim jsonObject As JObject = JObject.Parse(json)

    '    Dim objJson = JObject.Parse(json)
    '    Dim choix = objJson("choices")
    '    Dim referenceMessage = choix(0).Children().ToList


    '    For Each item As JProperty In referenceMessage
    '        item.CreateReader()

    '        Select Case item.Name
    '            Case "message"
    '                Dim message As String = item.Value.ToString()
    '                Dim objMsg = JObject.Parse(message)
    '                Dim content As String = objMsg("content").ToString
    '                Dim resultatJson As String = ExtractAndCleanJson(content)
    '                Dim objResultat = JObject.Parse(resultatJson)
    '                With objResultat 
    '                    '_id = CInt(.Item("id").ToString)
    '                    _montant_numerique = Utilitaires.convertStringToDecimal(objResultat.Item(NameOf(montant_numerique)).ToString)
    '                    _numero_du_cheque = CInt(.Item(NameOf(numero_du_cheque)).ToString)
    '                    _dateChq = CDate(.Item(NameOf(dateChq)).ToString)
    '                    _emetteur_du_cheque = .Item(NameOf(emetteur_du_cheque)).ToString
    '                    _destinataire = .Item("le_destinataire").ToString
    '                End With
    '            Case Else
    '                ' Logger l'information
    '                Logger.DBG("Propriété non reconnue : " & item.Name)
    '        End Select
    '    Next
    'End Sub
    Public Sub New(id As Integer, montant_numerique As String, numero_du_cheque As Integer, dateChq As Date, emetteur_du_cheque As String, destinataire As String)
        _id = id
        _montant_numerique = montant_numerique
        _numero_du_cheque = numero_du_cheque
        _dateChq = dateChq
        _emetteur_du_cheque = emetteur_du_cheque
        _destinataire = destinataire
    End Sub
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
    Property dateChq() As String
        Get
            Return _dateChq
        End Get
        Set(ByVal value As String)
            _dateChq = value
        End Set
    End Property
    Property id() As Integer
        Get
            Return _id
        End Get
        Set(ByVal value As Integer)
            _id = value
        End Set
    End Property

    Shared Function ExtractAndCleanJson(content As String) As String
        ' Use regex to extract text between the first '{' and the last '}'
        Dim match As Match = Regex.Match(content, "\{(.*?)\}", RegexOptions.Singleline)

        If match.Success Then
            ' Get the matched value and remove '\n' and '\'
            Dim jsonText As String = match.Value
            jsonText = jsonText.Replace("\n", "").Replace("\", "")
            Return jsonText
        Else
            Return String.Empty
        End If
    End Function
    Public Shared Sub AfficherImage(idCheque As Integer, pbBox As PictureBox)
        Try
            ' Effacer l'image précédemment affichée
            pbBox.Image = Nothing
            Dim imageData As Object = SqlCommandBuilder.CreateSqlCommand("reqImagesChq",
                                           New Dictionary(Of String, Object) From {{"@id", idCheque}
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
                Logger.INFO("Aucune image trouvée pour cet enregistrement.")
            End If
            'End Using
        Catch ex As Exception
            Logger.ERR("Erreur lors de la récupération de l'image : " & ex.Message)
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

    Public Overloads Sub RenommerFichier(sChemin As String, Optional sNouveauNom As String = "")
        Dim sRepDestination As String
        sRepDestination = LectureProprietes.GetVariable("repRacineAgumaaa") _
            & LectureProprietes.GetVariable("repRacineComptabilité") _
            & LectureProprietes.GetVariable("repFichiersDocumentsChèques") _
            & "\" & DateTime.Now.Year.ToString _
            & "\" & IIf(_emetteur_du_cheque = "AGUMAAA", "Emis", "Reçus")
        MyBase.RenommerFichier(sChemin, determineNouveauNom(sRepDestination))
    End Sub
    Private Function determineNouveauNom(sRepSortie As String) As String

        ' Construire le nouveau chemin complet du fichier dans le répertoire de sortie
        Dim numeroChq As String = Utilitaires.ExtractStringFromJson(_jsonMetaDonnées, "numero_du_cheque")
        Return Path.Combine(
            sRepSortie,
            $"CHQ_{numeroChq}"
        )

    End Function

End Class
