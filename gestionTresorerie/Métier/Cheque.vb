Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq
Public Class Cheque
    Public _dateChq As String
    Public _numero_du_cheque As Integer
    Public _emetteur As String
    Public _emetteur_du_cheque As String
    Public _montant_numerique As Decimal
    Public _destinataire As String
    Public _id As Integer

    Public Sub New()
    End Sub
    Public Sub New(json As String)
        Call ParseJson(json)
    End Sub
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

    Sub ParseJson(json As String)
        ' Parse the JSON string
        'Dim jsonObject As JObject = JObject.Parse(json)

        Dim objJson = JObject.Parse(json)
        Dim choix = objJson("choices")
        Dim referenceMessage = choix(0).Children().ToList


        For Each item As JProperty In referenceMessage
            item.CreateReader()

            Select Case item.Name
                Case "message"
                    Dim message As String = item.Value.ToString()
                    Dim objMsg = JObject.Parse(message)
                    Dim content As String = objMsg("content").ToString
                    Dim resultatJson As String = ExtractAndCleanJson(content)
                    Dim objResultat = JObject.Parse(resultatJson)
                    With objResultat
                        'TODO : je ne l'ai pas encore
                        '_id = CInt(.Item("id").ToString)
                        _montant_numerique = .Item("montant_numerique").ToString
                        _numero_du_cheque = CInt(.Item("numero_du_cheque").ToString)
                        _dateChq = CDate(.Item("dateChq").ToString)
                        _emetteur_du_cheque = .Item("emetteur_du_cheque").ToString
                        _destinataire = .Item("le_destinataire").ToString
                    End With
                Case Else
                    ' Logger l'information
                    Logger.GetInstance().INFO("Propriété non reconnue : " & item.Name)
            End Select
        Next
    End Sub

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
    Public Sub InsereEnBase(cheminChq As String)

        Try
            Dim query As String = "INSERT INTO [dbo].Cheque ([numero], [date], [emetteur], [montant], [banque], [destinataire], [imageChq]) VALUES (@numero, @date, @emetteur, @montant, @banque, @destinataire, @imageChq)"

            Using command As New SqlCommand(query, connexionDB.GetInstance.getConnexion)
                With command.Parameters
                    .AddWithValue("@numero", _numero_du_cheque)
                    .AddWithValue("@date", Convert.ToDateTime(_dateChq))
                    .AddWithValue("@emetteur", _emetteur_du_cheque)
                    .AddWithValue("@montant", _montant_numerique)
                    .AddWithValue("@banque", "CA43")
                    .AddWithValue("@destinataire", _destinataire)

                    ' Lire l'image en tant que tableau d'octets 
                    Dim imageBytes As Byte() = File.ReadAllBytes(cheminChq)

                    ' Ajouter le paramètre pour l'image
                    .AddWithValue("@imageChq", imageBytes)
                End With
                command.ExecuteNonQuery()
            End Using
            Logger.GetInstance.INFO("Données insérées avec succès." & Command.ToString)
        Catch ex As Exception
            Logger.GetInstance.ERR("Erreur lors de l'insertion des données : " & ex.Message)
        End Try
    End Sub
    Public Sub AfficherImage(idCheque As Integer, pbBox As PictureBox)
        Dim sqlConnexion As SqlConnection = Nothing

        Try
            ' Effacer l'image précédemment affichée
            pbBox.Image = Nothing

            ' Obtenir la connexion SQL
            sqlConnexion = connexionDB.GetInstance.getConnexion

            ' Ouvrir la connexion
            If sqlConnexion.State <> ConnectionState.Open Then
                sqlConnexion.Open()
            End If

            ' Requête SQL pour récupérer l'image
            Dim query As String = "SELECT [imageChq] FROM [dbo].Cheque WHERE [id] = @id"

            Using command As New SqlCommand(query, sqlConnexion)
                command.Parameters.AddWithValue("@id", idCheque)

                ' Exécuter la requête et récupérer les données binaires de l'image
                Dim imageData As Object = command.ExecuteScalar()

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
                    Logger.GetInstance().INFO("Aucune image trouvée pour cet enregistrement.")
                End If
            End Using
        Catch ex As Exception
            Logger.GetInstance().ERR("Erreur lors de la récupération de l'image : " & ex.Message)
        Finally
            ' Fermer la connexion si elle est ouverte
            If sqlConnexion IsNot Nothing AndAlso sqlConnexion.State = ConnectionState.Open Then
                sqlConnexion.Close()
            End If
        End Try
    End Sub

    Public Function AfficherTiersSuperieurImage(image As Image, ratio As Double) As Image
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
            Logger.GetInstance().INFO("Tiers supérieur de l'image affiché avec succès.")

            ' Retourner l'image du tiers supérieur
            Return tiersSuperieurImage

        Catch ex As Exception
            ' Logger l'erreur
            Logger.GetInstance().ERR("Erreur lors de l'extraction du tiers supérieur de l'image : " & ex.Message)
            ' Retourner une image vide en cas d'erreur
            Return New Bitmap(1, 1)
        End Try
    End Function

End Class
