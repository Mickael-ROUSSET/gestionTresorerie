Imports System.Globalization
Imports System.Reflection.Metadata

Module Utilitaires

    Public Function IndexSelectionne(cbBox As ComboBox, sNiveau As String) As Integer
        ' Si sNiveau est vide, retourner 0 (pas de catégorie)
        If String.IsNullOrEmpty(sNiveau) Then
            Return 0
        End If

        ' Retourner l'index de sNiveau dans cbBox.Items, ou -1 si non trouvé
        Return cbBox.Items.IndexOf(sNiveau)
    End Function

    ' Méthodes de conversion robustes
    Public Function ConvertToDate(value As Object) As Date
        If value Is Nothing OrElse IsDBNull(value) Then
            Return Date.MinValue
        End If
        Return CDate(value)
    End Function

    Public Function ConvertToDouble(value As Object) As Double
        If value Is Nothing OrElse IsDBNull(value) Then
            Return 0.0
        End If
        Return CDbl(value)
    End Function

    Public Function ConvertToBoolean(value As Object) As Boolean
        If value Is Nothing OrElse IsDBNull(value) Then
            Return False
        End If
        Return CBool(value)
    End Function
    Public Function ConvertToDecimal(value As Object) As Boolean
        If value Is Nothing OrElse IsDBNull(value) OrElse
            Not Decimal.TryParse(value, NumberStyles.Currency, CultureInfo.GetCultureInfo("fr-FR"), value) Then
            Return False
        End If
        Return CBool(value)
    End Function
End Module
