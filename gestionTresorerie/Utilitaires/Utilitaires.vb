Imports System.Globalization
Imports System.Reflection.Metadata

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
End Class
