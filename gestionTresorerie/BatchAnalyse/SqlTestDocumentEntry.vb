Option Strict On

Imports System.Globalization
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public NotInheritable Class SqlTestDocumentEntry
    Private Sub New()
    End Sub

    Public Shared Function Prepare(source As String, hash As String, root As String, cheque As Boolean,
                                   reference As String, tiers As String, dateText As String, amountText As String,
                                   Optional chequeEmis As Boolean? = Nothing) As DocumentArchiveProposal
        reference = reference.Trim()
        tiers = tiers.Trim()
        If reference.Length = 0 OrElse reference.Length > 60 OrElse tiers.Length = 0 OrElse tiers.Length > 80 Then
            Throw New ArgumentException("Renseignez le numéro du document (60 caractères au plus) et le tiers (80 caractères au plus).")
        End If
        If cheque AndAlso Not chequeEmis.HasValue Then
            Throw New ArgumentException("Indiquez si le chèque a été émis ou reçu.")
        End If

        Dim dateDocument As Date
        If Not Date.TryParseExact(dateText.Trim(), {"dd/MM/yyyy", "yyyy-MM-dd"}, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, dateDocument) Then
            Throw New ArgumentException("Renseignez la date au format JJ/MM/AAAA.")
        End If
        Dim montant As Decimal
        Dim normalized = Regex.Replace(amountText.Trim(), "[ \u00A0\u202F]", "").Replace(","c, "."c)
        If Not Regex.IsMatch(normalized, "\A-?\d+(?:\.\d{1,2})?\z") OrElse
           Not Decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign,
                               CultureInfo.InvariantCulture, montant) OrElse montant = 0D OrElse
           (cheque AndAlso montant < 0D) Then
            Throw New ArgumentException("Renseignez un montant en euros valide, avec deux décimales au plus.")
        End If

        Dim identity As New JObject()
        Dim transaction As New JObject From {{"devise", "EUR"}}
        Dim dateIso = dateDocument.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        If cheque Then
            identity("beneficiaire_nom") = tiers
            identity("emetteur_numero_cheque") = reference
            transaction("date_emission") = dateIso
            transaction("montant_numerique") = montant
        Else
            identity("fournisseur_nom") = tiers
            transaction("numero_facture") = reference
            transaction("date_facture") = dateIso
            transaction("montant_ttc") = montant
        End If

        Dim json As New JObject From {
            {"type_document", If(cheque, "Cheque", "Facture")},
            {"donnees_extraites", New JObject From {{"identite", identity}, {"transaction", transaction}}}
        }
        Return DocumentArchiveProposal.PrepareTest(source, hash, json.ToString(), root, If(cheque, chequeEmis, Nothing))
    End Function
End Class
