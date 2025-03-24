Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class Cheque
    Public _dateChq As String
    Public _numero_du_cheque As Integer
    Public _emetteur As String
    Public _emetteur_du_cheque As String
    Public _montant_numerique As Decimal
    Public _destinataire As String

    Public Sub New()
    End Sub
    Public Sub New(json As String)
        Call ParseJson(json)
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

    Sub ParseJson(json As String)
        ' Parse the JSON string
        Dim jsonObject As JObject = JObject.Parse(json)

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
                        '_emetteur = .Item("emetteur").ToString
                        _montant_numerique = .Item("montant_numerique").ToString
                        _numero_du_cheque = CInt(.Item("numero_du_cheque").ToString)
                        _dateChq = CDate(.Item("dateChq").ToString)
                        _emetteur_du_cheque = .Item("emetteur_du_cheque").ToString
                        _destinataire = .Item("le_destinataire").ToString
                    End With
                    Exit Select
                Case Else
                    Exit Select
            End Select
        Next
    End Sub
    Function ExtractAndCleanJson(content As String) As String
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
        Using FrmPrincipale.maConn
            Try
                Dim query As String = "INSERT INTO [dbo].Cheque ([numero], [date], [emetteur], [montant], [banque], [destinataire], [imageChq]) VALUES (@numero, @date, @emetteur, @montant, @banque, @destinataire, @imageChq)"

                Using command As New SqlCommand(query, FrmPrincipale.maConn)
                    command.Parameters.AddWithValue("@numero", _numero_du_cheque)
                    command.Parameters.AddWithValue("@date", Convert.ToDateTime(_dateChq))
                    command.Parameters.AddWithValue("@emetteur", _emetteur_du_cheque)
                    command.Parameters.AddWithValue("@montant", _montant_numerique)
                    command.Parameters.AddWithValue("@banque", "CA43")
                    command.Parameters.AddWithValue("@destinataire", _destinataire)

                    ' Lire l'image en tant que tableau d'octets 
                    Dim imageBytes As Byte() = File.ReadAllBytes(cheminChq)

                    ' Ajouter le paramètre pour l'image
                    command.Parameters.AddWithValue("@imageChq", imageBytes)

                    command.ExecuteNonQuery()
                End Using

                Console.WriteLine("Données insérées avec succès.")
            Catch ex As Exception
                Console.WriteLine("Erreur lors de l'insertion des données : " & ex.Message)
            End Try
        End Using
    End Sub
    Public Sub AfficherImage(idCheque As Integer, pbBox As PictureBox)
        Using FrmPrincipale.maConn
            Try
                ' Requête SQL pour récupérer l'image
                Dim query As String = "SELECT [imageChq] FROM [dbo].Cheque WHERE [id] = @id"

                Using command As New SqlCommand(query, FrmPrincipale.maConn)
                    command.Parameters.AddWithValue("@id", idCheque)

                    ' Exécuter la requête et récupérer les données binaires de l'image
                    Dim imageData As Object = command.ExecuteScalar()

                    If imageData IsNot Nothing AndAlso TypeOf imageData Is Byte() Then
                        ' Convertir les données binaires en objet Image
                        Dim imageBytes As Byte() = DirectCast(imageData, Byte())
                        Using ms As New IO.MemoryStream(imageBytes)
                            Dim image As Image = Image.FromStream(ms)
                            ' Afficher l'image dans le PictureBox
                            pbBox.Image = image
                        End Using
                    Else
                        MessageBox.Show("Aucune image trouvée pour cet enregistrement.")
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Erreur lors de la récupération de l'image : " & ex.Message)
            End Try
        End Using
    End Sub


End Class
