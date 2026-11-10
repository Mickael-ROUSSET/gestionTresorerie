Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public NotInheritable Class LocalPdfInvoiceAnalysis
    Private Sub New()
    End Sub

    Public Shared Function Extract(filePath As String, Optional tesseractExe As String = Nothing) As JObject
        Return ExtractText(ReadLocalText(filePath, tesseractExe))
    End Function

    Friend Shared Function ReadLocalText(filePath As String, tesseractExe As String, Optional sparse As Boolean = False) As String
        If sparse Then Return RecognizePages(filePath, tesseractExe, sparse:=True)
        Dim hasText As Boolean
        Dim truncated As Boolean
        Dim content = DocumentFilePreview.ReadPdfText(filePath, hasText, truncated)
        If truncated Then
            Throw New InvalidDataException("Le texte du PDF dépasse la limite d’analyse locale. Aucune donnée partielle n’est proposée.")
        End If
        If Not hasText Then
            If String.IsNullOrWhiteSpace(tesseractExe) Then
                Throw New InvalidOperationException("Ce PDF est un scan et le moteur OCR français est introuvable. Vérifiez l’installation de gestionTresorerie.")
            End If
            content = RecognizePages(filePath, tesseractExe)
        End If

        Return content
    End Function

    Friend Shared Function ExtractText(content As String) As JObject
        ' Reprend les règles conservatrices TTC/date de la PR #101.
        Dim normalized = content.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim transaction As New JObject
        Dim supplier = UniqueMatch(normalized, "^[ \t]*fournisseur[ \t]*:[ \t]*(?<v>[^\n]+)$")
        Dim number = UniqueMatch(normalized, "^[ \t]*(?:num[eé]ro[ \t]+(?:de[ \t]+)?facture|facture[ \t]+n[°º])[ \t]*:?[ \t]*(?<v>[^\n]+)$")
        Dim amount = UniqueMatch(normalized, "^[ \t]*(?:total|montant)[ \t]+ttc[ \t]*[:\-]?[ \t]*(?<v>\d[\d \u00A0\u202F]*[.,]\d{2})[ \t]*(?:EUR|€)[ \t]*$")
        Dim dateValue = UniqueMatch(normalized, "^[ \t]*date[ \t]+(?:de[ \t]+)?facture[ \t]*:[ \t]*(?<v>\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})[ \t]*$")
        Dim parsedAmount As Decimal
        If amount IsNot Nothing AndAlso Decimal.TryParse(
            Regex.Replace(amount, "[ \u00A0\u202F]", "").Replace(","c, "."c),
            NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, parsedAmount) AndAlso parsedAmount > 0D Then
            transaction("montant_ttc") = parsedAmount
            transaction("devise") = "EUR"
        End If
        Dim parsedDate As Date
        If Date.TryParseExact(dateValue, {"dd/MM/yyyy", "yyyy-MM-dd"}, CultureInfo.InvariantCulture,
                              DateTimeStyles.None, parsedDate) Then
            transaction("date_facture") = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        End If
        If number IsNot Nothing Then transaction("numero_facture") = number

        Dim identity As New JObject
        If supplier IsNot Nothing Then identity("fournisseur_nom") = supplier
        Return New JObject From {
            {"type_document", "Facture"},
            {"donnees_extraites", New JObject From {
                {"identite", identity},
                {"transaction", transaction}
            }}
        }
    End Function

    Friend Shared Function UniqueMatch(content As String, pattern As String) As String
        Dim values = Regex.Matches(content, pattern, RegexOptions.IgnoreCase Or RegexOptions.Multiline Or RegexOptions.CultureInvariant).
            Select(Function(result) result.Groups("v").Value.Trim()).
            Where(Function(value) value.Length > 0 AndAlso value.Length <= 200).
            Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToArray()
        Return If(values.Length = 1, values(0), Nothing)
    End Function

    Private Shared Function RecognizePages(filePath As String, tesseractExe As String, Optional sparse As Boolean = False) As String
        Dim pages = DocumentFilePreview.PdfPageCount(filePath)
        If pages > 3 Then Throw New InvalidDataException("L’OCR local accepte les PDF de trois pages au maximum.")
        Dim temporary = Directory.CreateTempSubdirectory("gestionTresorerie-ocr-")
        Try
            Dim content As New StringBuilder
            For index = 0 To pages - 1
                Dim imagePath = Path.Combine(temporary.FullName, $"{index}.png")
                Using page = DocumentFilePreview.RenderPdfPage(filePath, index, 3300)
                    page.Save(imagePath, Drawing.Imaging.ImageFormat.Png)
                End Using
                content.AppendLine(OfflineImageOcr.Recognize(imagePath, tesseractExe, sparse:=sparse))
            Next
            Return content.ToString()
        Finally
            temporary.Delete(recursive:=True)
        End Try
    End Function
End Class
