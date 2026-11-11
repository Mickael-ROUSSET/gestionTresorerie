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
        Dim kind = DetectHeading(heading, String.Join(vbLf, lines.Split(ChrW(10)).Take(48)))
        If kind = TypeDocument.Aucun Then Return Nothing

        Dim receipt = kind = TypeDocument.Recu
        Dim name = LocalPdfInvoiceAnalysis.UniqueMatch(lines, If(receipt,
            "^[ \t]*(?:[ée]metteur|organisme|b[eé]n[eé]ficiaire)[ \t]*:[ \t]*(?<v>[^\n]+)$",
            "^[ \t]*nom[ \t]+(?:et[ \t]+)?pr[eé]nom(?:s)?[ \t]*:[ \t]*(?<v>[^\n]+)$"))
        If Not receipt AndAlso name Is Nothing Then
            Dim familyName = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*nom(?:[ \t]+de[ \t]+naissance)?[ \t]*:[ \t]*(?<v>[^\n]+)$")
            Dim givenName = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
                "^[ \t]*pr[eé]nom(?:s\)?|\(s\))?[ \t]*:[ \t]*(?<v>[^\n]+)$")
            If familyName IsNot Nothing AndAlso givenName IsNot Nothing Then name = familyName & " " & givenName
        End If
        Dim documentDate = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*(?:date(?:[ \t]+(?:du[ \t]+re[çc]u|de[ \t]+signature|d['’]adh[eé]sion))?[ \t]*:|le)[ \t]*(?<v>\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})[ \t]*$")
        Dim identity As New JObject
        If name IsNot Nothing Then identity("nom_complet") = name
        If kind = TypeDocument.FormulaireAdhesion Then ExtractMembershipIdentity(lines, identity)
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

    Private Shared Sub ExtractMembershipIdentity(lines As String, identity As JObject)
        Dim familyName = ReadMembershipField(lines, "nom(?:[ \t]+de[ \t]+naissance)?")
        If familyName Is Nothing Then familyName = ReadMembershipField(lines, "nom[ \t]+d['’]usage")
        Dim firstName = ReadMembershipField(lines, "pr[eé]nom(?:s\)?|\(s\))?")
        Dim street = ReadMembershipField(lines, "adresse(?:[ \t]+postale)?")
        Dim town = ReadMembershipField(lines, "(?:ville|commune)")
        Dim postalCode = LocalPdfInvoiceAnalysis.UniqueMatch(lines, "\bcode[ \t]+postal[ \t]*[:;]?[ \t]*(?<v>\d{5})\b")
        If familyName IsNot Nothing Then identity("nom") = familyName
        If firstName IsNot Nothing Then identity("prenom") = firstName
        Dim locality = String.Join(" ", {postalCode, town}.Where(Function(part) Not String.IsNullOrWhiteSpace(part)))
        Dim address = String.Join(", ", {street, locality}.Where(Function(part) Not String.IsNullOrWhiteSpace(part)))
        If address.Length > 0 Then identity("adresse") = address
    End Sub

    Private Shared Function ReadMembershipField(lines As String, label As String) As String
        Const nextLabel As String = "nom(?:[ \t]+d['’]usage)?|pr[eé]nom(?:s\)?|\(s\))?|date[ \t]+de[ \t]+naissance|code[ \t]+postal|ville|commune|adresse|e[- ]?mail|t[eé]l[eé]phone|sexe|lieu[ \t]+de[ \t]+naissance"
        Dim value = LocalPdfInvoiceAnalysis.UniqueMatch(lines,
            "^[ \t]*" & label & "[ \t]*[:;][ \t]*(?:\n[ \t]*)?(?<v>[^\n]+?)(?=[ \t]+(?:" & nextLabel & ")[ \t]*[:;]|[ \t]*$)")
        If String.IsNullOrWhiteSpace(value) OrElse Not value.Any(Function(character) Char.IsLetter(character)) OrElse
            Regex.IsMatch(value, "^(?:" & nextLabel & ")[ \t]*[:;]", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant) Then Return Nothing
        Return value.Trim("."c, "_"c, " "c)
    End Function

    Private Shared Function DetectHeading(heading As String, lines As String) As TypeDocument
        Const options As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Multiline Or RegexOptions.CultureInvariant
        Dim membership = Regex.IsMatch(lines,
            "\b(?:bulletin|formulaire|fiche|demande)[ \t\n]+(?:d[ \t]*['’]?[ \t\n]*adh[eé]sion|de[ \t\n]+renouvellement[ \t\n]+d[ \t]*['’]?[ \t\n]*adh[eé]sion)\b", options)
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
