Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public NotInheritable Class LocalDocumentAnalysis
    Private Sub New()
    End Sub

    Friend Shared Function ReadText(source As String) As String
        Dim engine = OfflineImageOcr.FindExecutable()
        If String.Equals(Path.GetExtension(source), ".pdf", StringComparison.OrdinalIgnoreCase) Then
            Return LocalPdfInvoiceAnalysis.ReadLocalText(source, engine)
        End If
        If engine Is Nothing Then
            Throw New InvalidOperationException("Moteur OCR français introuvable. Vérifiez l’installation de gestionTresorerie.")
        End If
        Return OfflineImageOcr.Recognize(source, engine)
    End Function

    Public Shared Function Classify(content As String) As JObject
        If String.IsNullOrWhiteSpace(content) Then Return Nothing
        Dim lines = content.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim administrative = LocalAdministrativeAnalysis.Classify(lines)
        If administrative IsNot Nothing Then Return administrative
        Const options As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Multiline Or RegexOptions.CultureInvariant
        Dim cheque = Regex.IsMatch(lines,
            "^[ \t]*ch[eèé]que(?:[ \t]+(?:[ée]mis|re[çc]u|n[°º]|num[eé]ro))?[ \t]*[:.]?[ \t]*$|^[ \t]*(?:num[eé]ro[ \t]+(?:du[ \t]+)?ch[eèé]que|ch[eèé]que[ \t]+n[°º])[ \t]*[:.]?[ \t]*\d|^[ \t]*payez[ \t]+contre[ \t]+ce[ \t]+ch[eèé]que", options)
        Dim facture = Regex.IsMatch(lines,
            "^[ \t]*facture(?:[ \t]+n[°º].*)?[ \t]*[:.]?[ \t]*$|^[ \t]*(?:num[eé]ro[ \t]+(?:de[ \t]+)?facture|date[ \t]+(?:de[ \t]+)?facture)[ \t]*:", options)
        If cheque = facture Then Return Nothing
        If facture Then Return LocalPdfInvoiceAnalysis.ExtractText(lines)

        Dim identity As New JObject
        Dim transaction As New JObject
        Dim number = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:num[eé]ro[ \t]+(?:du[ \t]+)?ch[eèé]que|ch[eèé]que[ \t]+n[°º])[ \t]*[:.]?[ \t]*(?<v>\d{4,20})[ \t]*$")
        Dim beneficiary = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:b[eé]n[eé]ficiaire|[àa][ \t]+l['’]ordre[ \t]+de)[ \t]*:[ \t]*(?<v>[^\n]+)$")
        Dim dateValue = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:date(?:[ \t]+d['’][ée]mission)?[ \t]*:|le)[ \t]*(?<v>\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})[ \t]*$")
        Dim amount = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:montant(?:[ \t]+en[ \t]+euros)?[ \t]*:|€)[ \t]*(?<v>\d[\d \u00A0\u202F]*[.,]\d{2})[ \t]*(?:€|EUR)?[ \t]*$")
        If number IsNot Nothing Then identity("emetteur_numero_cheque") = number
        If beneficiary IsNot Nothing Then identity("beneficiaire_nom") = beneficiary
        Dim parsedDate As Date
        If Date.TryParseExact(dateValue, {"dd/MM/yyyy", "yyyy-MM-dd"}, CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) Then
            transaction("date_emission") = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        End If
        Dim parsedAmount As Decimal
        If amount IsNot Nothing AndAlso Decimal.TryParse(Regex.Replace(amount, "[ \u00A0\u202F]", "").Replace(","c, "."c),
            NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, parsedAmount) AndAlso parsedAmount > 0D Then
            transaction("montant_numerique") = parsedAmount
            transaction("devise") = "EUR"
        End If
        Return New JObject From {
            {"type_document", "Cheque"},
            {"donnees_extraites", New JObject From {{"identite", identity}, {"transaction", transaction}}}
        }
    End Function
End Class
