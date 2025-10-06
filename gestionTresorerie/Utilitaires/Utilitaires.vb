Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Reflection.Metadata
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Class Utilitaires
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
        If value Is Nothing OrElse IsDBNull(value) Then
            Return Date.MinValue
        End If
        Return CDate(value)
    End Function

    Public Shared Function ConvertToDouble(value As Object) As Double
        If value Is Nothing OrElse IsDBNull(value) Then
            Return 0.0
        End If
        Return CDbl(value)
    End Function

    Public Shared Function ConvertToBoolean(value As Object) As Boolean
        If value Is Nothing OrElse IsDBNull(value) Then
            Return False
        End If
        Return CBool(value)
    End Function
    Public Shared Function ConvertToDecimal(value As Object) As Decimal
        If value Is Nothing OrElse IsDBNull(value) Then
            Return 0
        End If

        Dim result As Decimal
        If Decimal.TryParse(value.ToString(), NumberStyles.Currency, CultureInfo.GetCultureInfo("fr-FR"), result) Then
            Return result
        End If

        Return 0
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
            ' Vérifier si JsonMetaDonnées est non vide
            If String.IsNullOrEmpty(json) Then
                Logger.WARN("json est vide ou null.")
                Return Nothing
            End If

            ' Parser le JSON
            Dim jsonObj As JObject = JObject.Parse(json)

            ' Vérifier si le champ dateDocument existe
            If jsonObj(sNomChamp) IsNot Nothing Then
                Return jsonObj(sNomChamp).ToString()
            Else
                '                [json]
                'id: "620d30ce6fd64005bd8d7b3ef6175453"
                'created: 1759784402
                'model: "pixtral-12b-2409"
                'usage
                'prompt_tokens: 3131
                'total_tokens: 3303
                'completion_tokens: 172
                'Object :  "chat.completion"
                'choices
                '[0]
                'index: 0
                'finish_reason: "stop"
                'Message
                'role: "assistant"
                'tool_calls:     null
                'content: "Voici les éléments extraits du chèque au format JSON :

                '```json
                '{
                '  "emetteur_du_cheque": "BNP PARIBAS",
                '  "montant_numerique": "135.00",
                '  "numero_du_cheque": "00000251182",
                '  "dateChq": "04/09/2015",
                '  "emetteur_du_cheque": "MADAME ANNIE LABARE",
                '  "le_destinataire": "AGUMAAA"
                '}
                Logger.WARN($"Champ {sNomChamp} absent dans {json}.")
                Return Nothing
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur lors du parsing de {json} : " & ex.Message)
            Return Nothing
        End Try
    End Function
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

    Public Shared Sub RenommerEtDeplacerFichier(sAncienNom As String, sNouveauNom As String)
        Try
            ' Vérifier si le fichier existe
            If Not File.Exists(sAncienNom) Then
                Logger.ERR($"Le fichier {sAncienNom} n'existe pas.")
                Return
            End If

            ' Vérifier si sNouveauNom est vide
            If String.IsNullOrEmpty(sNouveauNom) Then
                Logger.ERR("Le nouveau nom du fichier est vide.")
                Return
            End If

            ' Vérifier si le répertoire de sortie existe, sinon le créer
            Dim sRepSortie As String = Path.GetFullPath(sNouveauNom)
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
End Class
