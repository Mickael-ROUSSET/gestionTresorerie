Imports System.Globalization
Imports System.Reflection.Metadata
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
                Logger.WARN($"Champ {sNomChamp} absent dans {json}.")
                Return Nothing
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur lors du parsing de {json} : " & ex.Message)
            Return Nothing
        End Try
    End Function
End Class
