Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public NotInheritable Class LocalDocumentAnalysis
    Private Sub New()
    End Sub

    Friend Shared Function ReadText(source As String, Optional sparse As Boolean = False, Optional chequeLayout As Boolean = False) As String
        Dim engine = OfflineImageOcr.FindExecutable()
        If String.Equals(Path.GetExtension(source), ".pdf", StringComparison.OrdinalIgnoreCase) Then
            Return LocalPdfInvoiceAnalysis.ReadLocalText(source, engine, sparse, chequeLayout)
        End If
        If engine Is Nothing Then
            Throw New InvalidOperationException("Moteur OCR français introuvable. Vérifiez l’installation de gestionTresorerie.")
        End If
        Return OfflineImageOcr.Recognize(source, engine, sparse:=sparse, chequeLayout:=chequeLayout)
    End Function

    Public Shared Function Classify(content As String, Optional knownCheque As Boolean = False) As JObject
        If String.IsNullOrWhiteSpace(content) Then Return Nothing
        Dim lines = content.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim administrative = LocalAdministrativeAnalysis.Classify(lines)
        If administrative IsNot Nothing Then Return If(knownCheque, Nothing, administrative)
        Const options As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Multiline Or RegexOptions.CultureInvariant
        Const chequeReference As String = "(?:num[eé]ro[ \t]+(?:du[ \t]+)?ch[eèé]que|n[°º][ \t]+(?:du[ \t]+|de[ \t]+)?ch[eèé]que|ch[eèé]que[ \t]+(?:n[°º]|[ée]mis|re[çc]u))"
        Dim cheque = Regex.IsMatch(lines,
            "^[ \t]*ch[eèé]que(?:[ \t]+(?:[ée]mis|re[çc]u|n[°º]|num[eé]ro))?[ \t]*[:.]?[ \t]*$|^[ \t]*" &
            chequeReference & "[ \t]*[:.]?[ \t]*\d|\bpayez[ \t]+(?:contre[ \t]+)?(?:ce|par[ \t]+ce)[ \t]+ch[eèé]que\b", options)
        Dim facture = Regex.IsMatch(lines,
            "^[ \t]*facture(?:[ \t]+n[°º].*)?[ \t]*[:.]?[ \t]*$|^[ \t]*(?:num[eé]ro[ \t]+(?:de[ \t]+)?facture|date[ \t]+(?:de[ \t]+)?facture)[ \t]*:", options)
        If Not knownCheque AndAlso cheque = facture Then Return Nothing
        If facture Then Return If(knownCheque, Nothing, LocalPdfInvoiceAnalysis.ExtractText(lines))

        Dim identity As New JObject
        Dim transaction As New JObject
        Dim number = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*" & chequeReference & "[ \t]*[:.]?[ \t]*(?<v>\d{4,20})[ \t]*$")
        If number Is Nothing Then
            number = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "\bn[°ºoe]?[ \t]*(?:du|de)[ \t]*ch[eèé]que[ \t:▼¥y]*(?:\n[ \t]*)?(?<v>\d{7})(?!\d)")
        End If
        Dim beneficiary = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:b[eé]n[eé]ficiaire[ \t]*:|[àa][ \t]+l['’]ordre[ \t]+de\b[ \t]*:?)[ \t]*(?<v>[^\n]+)$")
        Dim issuer = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:[ée]metteur(?:[ \t]+du[ \t]+ch[eèé]que)?|tireur|titulaire(?:[ \t]+du[ \t]+compte)?)[ \t]*:[ \t]*(?<v>[^\n]+)$")
        If beneficiary Is Nothing Then
            beneficiary = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*(?:à|»)[ \t]+(?<v>[\p{L}][\p{L}'’ -]{2,79})[ \t]*$")
        End If
        If issuer Is Nothing Then
            issuer = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*(?<v>assoc\.[ \t]+[\p{L}][\p{L}'’ -]{2,79}?)(?:[ \t]{2,}[^\n]*)?[ \t]*$")
        End If
        Dim dateValue = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:date(?:[ \t]+d['’][ée]mission)?[ \t]*:|(?:fait[ \t]+[àa][ \t]+[^\n]*?[ \t]+)?le)[ \t]*(?<v>\d{1,2}/\d{1,2}/\d{4}|\d{4}-\d{2}-\d{2})[ \t]*$")
        Dim amount = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:(?:montant(?:[ \t]+en[ \t]+euros)?[ \t]*:|€)[ \t]*(?<v>\d[\d \u00A0\u202F]*[.,]\d{2})[ \t]*(?:€|EUR)?|(?<v>\d[\d \u00A0\u202F]*[.,]\d{2})[ \t]*(?:€|EUR))[ \t]*$")
        If amount Is Nothing AndAlso knownCheque Then
            amount = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "(?<![\d.,])(?<v>\d{1,7}[.,]\d{2})[ \t]*[=|]+[ \t]*$")
        End If
        If number IsNot Nothing Then identity("emetteur_numero_cheque") = number
        If beneficiary IsNot Nothing Then identity("beneficiaire_nom") = beneficiary
        If issuer IsNot Nothing Then identity("emetteur_du_cheque") = issuer
        Dim parsedDate As Date
        If Date.TryParseExact(dateValue, {"d/M/yyyy", "yyyy-MM-dd"}, CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) Then
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
