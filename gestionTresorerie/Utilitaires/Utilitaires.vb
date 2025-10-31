Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq
Friend Class Utilitaires
    Public Shared Sub selLigneDgvParLibelle(dgv As DataGridView, libelle As String)
        ' Sélectionne la ligne dont le libellé correspond au paramètre (sur le nombre de caractères renseignés)
        If libelle.Length > 1 Then
            Dim libelleMajuscule As String = libelle.ToUpper()

            For Each row As DataGridViewRow In dgv.Rows
                If Not row.IsNewRow Then
                    For Each cellIndex As Integer In {1, 3}
                        Dim cellValue As Object = row.Cells(cellIndex).Value
                        If Not IsDBNull(cellValue) AndAlso cellValue.ToString().StartsWith(libelleMajuscule, StringComparison.CurrentCultureIgnoreCase) Then
                            dgv.Rows(row.Index).Selected = True
                            dgv.CurrentCell = dgv.SelectedRows(0).Cells(0)
                            Logger.DBG($"{libelle} trouvé dans {dgv.Name }")
                            Exit For
                        End If
                    Next
                End If
            Next
        End If
    End Sub
    Public Function IndexSelectionne(cbBox As ComboBox, sNiveau As String) As Integer
        ' Si sNiveau est vide, retourner 0 (pas de catégorie)
        If String.IsNullOrEmpty(sNiveau) Then
            Return 0
        End If

        ' Retourner l'index de sNiveau dans cbBox.Items, ou -1 si non trouvé
        Return cbBox.Items.IndexOf(sNiveau)
    End Function

    ' Méthodes de conversion robustes
    Public Shared Function ConvertToDate(value As Object) As Date
        Return If(value Is Nothing OrElse IsDBNull(value), Date.MinValue, CDate(value))
    End Function

    Public Shared Function ConvertToDouble(value As Object) As Double
        Return If(value Is Nothing OrElse IsDBNull(value), 0.0, CDbl(value))
    End Function

    Public Shared Function ConvertToBoolean(value As Object) As Boolean
        Return If(value Is Nothing OrElse IsDBNull(value), False, CBool(value))
    End Function
    Public Shared Function ConvertToDecimal(value As Object) As Decimal
        If value Is Nothing OrElse IsDBNull(value) Then
            Return 0
        End If

        Dim result As Decimal
        Return If(Decimal.TryParse(value.ToString(), NumberStyles.Currency, CultureInfo.GetCultureInfo("fr-FR"), result), result, 0)
    End Function
    Public Shared Function ChargerCriteresDepuisConfig(nomDico As String) As Dictionary(Of String, String)
        Dim dico As New Dictionary(Of String, String)()

        ' Lire la chaîne de caractères depuis app.config
        Dim dicoTypeMvtString As String = LectureProprietes.GetVariable(nomDico)

        ' Diviser la chaîne en paires clé-valeur
        Dim paires As String() = dicoTypeMvtString.Split(";"c)

        ' Ajouter chaque paire au dictionnaire
        For Each paire As String In paires
            Dim keyValue As String() = paire.Split(":"c)
            If keyValue.Length = 2 Then
                dico.Add(keyValue(0).Trim(), keyValue(1).Trim())
            End If
        Next
        Return dico
    End Function
    Public Shared Function convertStringToDecimal(montantStr As String) As Decimal
        Dim montantDecimal As Decimal

        montantStr = montantStr.Replace(".", ",")
        If String.IsNullOrEmpty(montantStr) OrElse montantStr = "0" Then
            montantDecimal = 0D ' Valeur par défaut si vide ou zéro
        Else
            Try
                ' Conversion avec Decimal.Parse (recommandé pour les formats culturels)
                montantDecimal = Decimal.Parse(montantStr, Globalization.NumberStyles.Currency Or Globalization.NumberStyles.Float, Globalization.CultureInfo.CurrentCulture)
            Catch ex As FormatException
                ' Gestion d'erreur si le format n'est pas valide (ex: "abc" au lieu de "123.45")
                Logger.ERR($"Format invalide pour montant_numerique : {montantStr}. Erreur : {ex.Message}")
                montantDecimal = 0D ' Valeur par défaut en cas d'erreur
            Catch ex As OverflowException
                Logger.ERR($"Montant trop grand pour montant_numerique : {montantStr}")
                montantDecimal = 0D
            End Try
        End If
        Return montantDecimal
    End Function
    Public Shared Function ExtractDateFromJson(json As String, sNomChamp As String) As Date?
        Try
            ' Vérifier si JsonMetaDonnées est non vide
            If String.IsNullOrEmpty(json) Then
                Logger.WARN("json est vide ou null.")
                Return Nothing
            End If

            ' Parser le JSON
            Dim jsonObj As JObject = JObject.Parse(json)

            ' Vérifier si le champ dateDocument existe
            If jsonObj(sNomChamp) IsNot Nothing Then
                Try
                    ' Convertir la valeur dateDocument en Date
                    Dim dateStr As String = jsonObj(sNomChamp).ToString()
                    Return DateTime.Parse(dateStr, Globalization.CultureInfo.InvariantCulture)
                Catch ex As FormatException
                    Logger.ERR("Format de date invalide dans dateDocument : " & ex.Message)
                    Return Nothing
                End Try
            Else
                Logger.WARN("Champ dateDocument absent dans JsonMetaDonnées.")
                Return Nothing
            End If

        Catch ex As Exception
            Logger.ERR("Erreur lors du parsing de JsonMetaDonnées : " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Shared Function ExtractStringFromJson(json As String, sNomChamp As String) As String
        Try
            ' Vérifier si le JSON est non vide
            If String.IsNullOrEmpty(json) Then
                Logger.WARN($"Le JSON est vide ou null : {json}, {sNomChamp}")
                Return ""
            End If

            ' Parser le JSON racine
            Dim jsonObj As JObject = JObject.Parse(json)

            ' Naviguer jusqu'à choices[0].message.content
            Dim choices As JArray = jsonObj("choices")
            If choices Is Nothing OrElse choices.Count = 0 Then
                Logger.WARN($"Le champ 'choices' est absent ou vide dans le JSON : {json}")
                Return ""
            End If

            Dim message As JObject = choices(0)("message")
            If message Is Nothing Then
                Logger.WARN($"Le champ 'message' est absent dans choices[0] : {json}")
                Return ""
            End If

            Dim content As String = message("content")?.ToString()
            If String.IsNullOrEmpty(content) Then
                Logger.WARN($"Le champ 'content' est vide ou absent dans message : {content}")
                Return ""
            End If

            ' Extraire le JSON imbriqué dans content
            Dim contentJson As String = ExtractJsonFromContent(content)
            If String.IsNullOrEmpty(contentJson) Then
                Logger.WARN($"Aucun JSON valide extrait du champ 'content' : {contentJson}")
                Return ""
            End If

            ' Parser le JSON imbriqué
            Dim innerJsonObj As JObject = JObject.Parse(contentJson)

            ' Vérifier si le champ demandé existe
            If innerJsonObj(sNomChamp) IsNot Nothing Then
                Return innerJsonObj(sNomChamp).ToString()
            Else
                Logger.WARN($"Champ '{sNomChamp}' absent dans le JSON imbriqué : {contentJson}")
                Return ""
            End If
        Catch ex As Exception
            Logger.ERR($"Erreur lors du parsing du JSON pour le champ '{sNomChamp}' : {ex.Message}")
            Return ""
        End Try
    End Function

    ' Fonction auxiliaire pour extraire le JSON de la chaîne content
    Private Shared Function ExtractJsonFromContent(content As String) As String
        Try
            ' Trouver le début et la fin du bloc JSON dans content
            Dim startIndex As Integer = content.IndexOf("```json") + 7
            Dim endIndex As Integer = content.LastIndexOf("```")
            If startIndex < 7 OrElse endIndex <= startIndex Then
                Logger.WARN("Aucun bloc JSON trouvé dans le contenu.")
                Return ""
            End If

            ' Extraire la sous-chaîne JSON
            Dim jsonString As String = content.Substring(startIndex, endIndex - startIndex).Trim()
            Return jsonString
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'extraction du JSON du contenu : {ex.Message}")
            Return ""
        End Try
    End Function

    Public Shared Function ExtractAndCleanJson(content As String) As String
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

    Public Shared Sub RenommerEtDeplacerFichier(sAncienNom As String, sNouveauNom As String)
        'sAncienNom et sNouveauNom contiennent le chemin et le nom du fichier
        Try
            ' Vérifier si le fichier existe
            If Not File.Exists(sAncienNom) Then
                Logger.ERR($"Le fichier {sAncienNom} n'existe pas.")
                Return
            End If

            ' Vérifier si sNouveauNom est vide
            If String.IsNullOrEmpty(sNouveauNom) Then
                Logger.ERR($"Le nouveau nom du fichier : {sNouveauNom} est vide.")
                Return
            End If

            ' Vérifier si le répertoire de sortie existe, sinon le créer 
            Dim sRepSortie As String = Path.GetDirectoryName(sNouveauNom)
            If Not Directory.Exists(sRepSortie) Then
                Directory.CreateDirectory(sRepSortie)
            End If

            ' Vérifier si un fichier avec le même nom existe déjà
            If File.Exists(sNouveauNom) Then
                Logger.INFO($"Un fichier avec le nom '{sNouveauNom}' existe déjà dans {sRepSortie}. Renommage et déplacement annulés.")
                Return
            End If

            ' Renommer et déplacer le fichier
            File.Move(sAncienNom, sNouveauNom)
            Logger.INFO($"Fichier {sAncienNom} renommé et déplacé vers {sNouveauNom}")

        Catch ex As Exception
            Logger.ERR($"Erreur lors du renommage/déplacement du fichier {sAncienNom} vers {sNouveauNom}: {ex.Message}")
        End Try
    End Sub
    Public Shared Function ParseJson(json As String, fieldMappings As Dictionary(Of String, String)) As String
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
                        Dim resultatJson As String = Utilitaires.ExtractAndCleanJson(content)
                        Dim objResultat As JObject = JObject.Parse(resultatJson)

                        ' Extraire dynamiquement les champs selon les mappages fournis
                        For Each mapping In fieldMappings
                            Dim fieldName As String = mapping.Key
                            Dim fieldType As String = mapping.Value
                            Dim fieldValue As String = objResultat.Item(fieldName)?.ToString()

                            If String.IsNullOrEmpty(fieldValue) Then
                                Logger.DBG($"Champ {fieldName} non trouvé dans le JSON")
                                Continue For
                            End If

                            Select Case fieldType.ToLower()
                                Case "string"
                                    resultat(fieldName) = fieldValue
                                Case "decimal"
                                    resultat(fieldName) = Utilitaires.convertStringToDecimal(fieldValue)
                                Case "integer"
                                    resultat(fieldName) = CInt(fieldValue)
                                Case "date"
                                    resultat(fieldName) = CDate(fieldValue).ToString("yyyy-MM-dd")
                                Case Else
                                    Logger.DBG($"Type de conversion non reconnu pour le champ {fieldName}: {fieldType}")
                                    resultat(fieldName) = fieldValue
                            End Select
                        Next

                    Case Else
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

    Public Shared Function ExtraitNuméroChèque(libelle As String) As String
        If String.IsNullOrWhiteSpace(libelle) Then
            Return Nothing
        End If

        ' 1) Cas principal : "CHEQUE EMIS #######"
        Dim patternChequeEmis As New Regex("\bCH[eé]QUE\s+EMIS\s*[:\-]?\s*(\d+)\b",
                                           RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        Dim m As Match = patternChequeEmis.Match(libelle)
        If m.Success AndAlso m.Groups.Count > 1 Then
            Return m.Groups(1).Value
        End If

        ' 2) Variante : "CHEQUE", "CHQ", "CHÈQUE" suivi du numéro
        Dim patternGeneric As New Regex("\b(?:CHEQUE|CHQ|CH[eé]QUE)\b.*?(\d{4,})",
                                        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        m = patternGeneric.Match(libelle)
        If m.Success AndAlso m.Groups.Count > 1 Then
            Return m.Groups(1).Value
        End If

        ' 3) Dernier recours : première suite de >=4 chiffres
        Dim patternDigits As New Regex("\b(\d{4,})\b")
        m = patternDigits.Match(libelle)
        If m.Success Then
            Return m.Groups(1).Value
        End If

        Return Nothing
    End Function


    Public Shared Function ExtraireJsonValide(chaine As String) As String
        Try
            ' 1. Supprimer les balises ```json ou ```
            Dim nettoyee As String = Regex.Replace(chaine, "```json|```", "", RegexOptions.IgnoreCase)

            ' 2. Trouver le bloc JSON le plus profond avec des accolades équilibrées
            Dim motif As String = "\{(?:[^{}]|(?<open>\{)|(?<-open>\}))*(?(open)(?!))\}"
            Dim correspondances = Regex.Matches(nettoyee, motif, RegexOptions.Singleline)

            For Each match As Match In correspondances
                Dim possibleJson = match.Value.Trim()

                ' 3. Valider le JSON
                Try
                    Dim jObj = JObject.Parse(possibleJson)
                    Return jObj.ToString() ' JSON valide → on le renvoie
                Catch ex As Exception
                    ' Ignorer les erreurs, continuer
                End Try
            Next

            Return Nothing ' Aucun JSON valide trouvé

        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Récupère une chaîne depuis le reader en gérant DBNull et en nettoyant
    ''' les guillemets superflus (ex: """"" -> "" -> "" -> String.Empty).
    ''' </summary>
    Public Shared Function GetSafeStringFromReader(reader As SqlDataReader, index As Integer) As String
        If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))
        If reader.IsDBNull(index) Then
            Return String.Empty
        End If

        Dim raw As String = reader.GetString(index)

        If String.IsNullOrEmpty(raw) Then
            Return String.Empty
        End If

        ' Caractère guillemet
        Dim q As Char = Chr(34)

        ' Si la chaîne est uniquement des guillemets, renvoyer vide
        If raw.Trim(q).Length = 0 Then
            Return String.Empty
        End If

        ' Retirer guillemets entourants simples (ex: "val" -> val)
        If raw.Length >= 2 AndAlso raw.StartsWith(q) AndAlso raw.EndsWith(q) Then
            raw = raw.Substring(1, raw.Length - 2)
        End If

        ' Remplacer les doubles guillemets consécutifs par un seul (ex: "" -> ")
        ' Boucle au cas où il y aurait des séquences répétées
        Dim doubleQ As String = New String(q, 2)
        While raw.Contains(doubleQ)
            raw = raw.Replace(doubleQ, q.ToString())
        End While

        ' Si après nettoyage il ne reste que des guillemets -> vide
        If raw.Trim(q).Length = 0 Then
            Return String.Empty
        End If

        Return raw
    End Function
    Public Shared Function SafeGetString(rdr As SqlDataReader, index As Integer) As String
        Return If(rdr.IsDBNull(index), String.Empty, rdr.GetString(index))
    End Function

    Public Shared Function SafeGetInt(rdr As SqlDataReader, index As Integer) As Integer
        Return If(rdr.IsDBNull(index), 0, rdr.GetInt32(index))
    End Function

    Public Shared Function SafeGetDate(rdr As SqlDataReader, index As Integer) As Date
        Return If(rdr.IsDBNull(index), Date.MinValue, rdr.GetDateTime(index))
    End Function

End Class
