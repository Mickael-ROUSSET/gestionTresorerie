Option Strict On

Imports System.Globalization
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq
Imports gestionTresorerie.Agumaaa

Friend NotInheritable Class LocalAdministrativeAnalysis
    Private Sub New()
    End Sub

    Friend Shared Function Classify(lines As String) As JObject
        ' Rechercher les titres en haut de la page pour ignorer les listes de pièces à joindre.
        Dim heading = String.Join(vbLf, lines.Split(ChrW(10)).Where(Function(line) Not String.IsNullOrWhiteSpace(line)).Take(8))
        Dim kind = DetectHeading(heading)
        If kind = TypeDocument.Aucun Then Return Nothing

        Dim receipt = kind = TypeDocument.Recu
        Dim name = LocalPdfInvoiceAnalysis.UniqueMatch(lines, If(receipt,
            "^[ \t]*(?:[ée]metteur|organisme|b[eé]n[eé]ficiaire)[ \t]*:[ \t]*(?<v>[^\n]+)$",
            "^[ \t]*nom[ \t]+(?:et[ \t]+)?pr[eé]nom(?:s)?[ \t]*:[ \t]*(?<v>[^\n]+)$"))
        If Not receipt AndAlso name Is Nothing Then
            Dim familyName = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*nom(?:[ \t]+de[ \t]+naissance)?[ \t]*:[ \t]*(?<v>[^\n]+)$")
            Dim givenName = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*pr[eé]nom(?:s|\(s\))?[ \t]*:[ \t]*(?<v>[^\n]+)$")
            If familyName IsNot Nothing AndAlso givenName IsNot Nothing Then name = familyName & " " & givenName
        End If
        Dim documentDate = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:date(?:[ \t]+(?:du[ \t]+re[çc]u|de[ \t]+signature|d['’]adh[eé]sion))?[ \t]*:|le)[ \t]*(?<v>\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})[ \t]*$")
        Dim identity As New JObject
        If name IsNot Nothing Then identity("nom_complet") = name
        Dim fields As New JObject
        Dim parsedDate As Date
        If Date.TryParseExact(documentDate, {"dd/MM/yyyy", "yyyy-MM-dd"}, CultureInfo.InvariantCulture,
                              DateTimeStyles.None, parsedDate) Then
            fields("date_document") = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        End If
        If receipt Then
            Dim number = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*(?:num[eé]ro[ \t]+(?:du[ \t]+)?re[çc]u|re[çc]u(?:[ \t]+fiscal)?[ \t]+n[°º])[ \t]*:?[ \t]*(?<v>[^\n]+)$")
            If number IsNot Nothing Then fields("reference") = number
            Dim amount = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*(?:montant(?:[ \t]+re[çc]u)?|total)[ \t]*:[ \t]*(?<v>\d[\d \u00A0\u202F]*[.,]\d{2})[ \t]*(?:€|EUR)?[ \t]*$")
            If amount IsNot Nothing Then fields("montant_numerique") = amount
        End If
        Return New JObject From {
            {"type_document", kind.ToString()},
            {"donnees_extraites", New JObject From {{"identite", identity}, {"transaction", fields}}}
        }
    End Function

    Private Shared Function DetectHeading(heading As String) As TypeDocument
        Const options As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Multiline Or RegexOptions.CultureInvariant
        Dim membership = Regex.IsMatch(heading,
            "^[ \t]*(?:bulletin|formulaire|fiche|demande)[ \t]+(?:d['’][ \t]*adh[eé]sion|de[ \t]+renouvellement[ \t]+d['’][ \t]*adh[eé]sion)\b", options)
        Dim medical = Regex.IsMatch(heading,
            "^[ \t]*questionnaire[ \t]+(?:m[eé]dical|(?:de[ \t]+)?sant[eé])\b[^\n]*$|^[ \t]*QS[- ]SPORT\b", options)
        Dim receipt = Regex.IsMatch(heading,
            "^[ \t]*re[çc]u(?:[ \t]+(?:fiscal(?:[ \t]+n[°º].*)?|de[ \t]+(?:paiement|versement)|n[°º].*))?[ \t]*[:.]?[ \t]*$", options)
        If If(membership, 1, 0) + If(medical, 1, 0) + If(receipt, 1, 0) <> 1 Then Return TypeDocument.Aucun
        If membership Then Return TypeDocument.FormulaireAdhesion
        If medical Then Return TypeDocument.QuestionnaireMedical
        Return TypeDocument.Recu
    End Function
End Class
