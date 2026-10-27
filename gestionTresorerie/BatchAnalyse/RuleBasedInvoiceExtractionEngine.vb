Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' Regles locales et conservatrices sur le texte fourni par un moteur interchangeable.
''' Aucun OCR, reseau, Gemini, SQL ou enregistrement comptable.
''' Les champs produits restent exclusivement des propositions a verifier.
''' </summary>
Public NotInheritable Class RuleBasedInvoiceExtractionEngine
    Implements IDocumentExtractionEngine

    Private ReadOnly _inner As IDocumentExtractionEngine

    Public Sub New(inner As IDocumentExtractionEngine)
        If inner Is Nothing Then Throw New ArgumentNullException(NameOf(inner))
        _inner = inner
    End Sub

    Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
        Get
            Return "regles-facture-local/" & _inner.EngineName
        End Get
    End Property

    Public Async Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
        Dim rawJson = Await _inner.ExtractAsync(documentPath).ConfigureAwait(False)
        Dim extracted = JObject.Parse(rawJson)
        Dim rawText = If(extracted("texte") Is Nothing, "", extracted("texte").ToString())
        ' PdfPig utilise les fins de ligne Windows CRLF ; les expressions ^/$ doivent rester portables.
        Dim normalizedText = rawText.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        ' Une extraction partielle peut masquer un second total contradictoire.
        ' Ne jamais proposer de montant ou date depuis un PDF tronque.
        If String.Equals(If(extracted("statut_extraction") Is Nothing, "", extracted("statut_extraction").ToString()),
                         "Texte_tronque_a_verifier", StringComparison.Ordinal) Then
            extracted("champs_proposes") = New JObject()
            extracted("statut_champs") = "Extraction_incomplete_a_verifier"
            Return extracted.ToString(Formatting.None)
        End If
        Dim proposed As New JObject()
        Dim amounts As New HashSet(Of Decimal)()
        Dim dates As New HashSet(Of String)(StringComparer.Ordinal)

        ' Ne retenir que des totaux explicitement etiquetes TTC et libelles en euros.
        For Each match As Match In Regex.Matches(normalizedText,
            "(?im)^[ \t]*(?:total|montant)[ \t]+ttc[ \t]*[:\-]?[ \t]*(?<amount>\d[\d \u00A0]*[.,]\d{2})[ \t]*(?:EUR|€)[ \t]*$")
            Dim valueText = match.Groups("amount").Value.Replace(" ", "").Replace(ChrW(&HA0).ToString(), "").Replace(",", ".")
            Dim value As Decimal
            If Decimal.TryParse(valueText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, value) AndAlso value > 0D Then
                amounts.Add(value)
            End If
        Next

        ' Ne pas deduire une date depuis une ligne non etiquetee.
        For Each match As Match In Regex.Matches(normalizedText,
            "(?im)^[ \t]*date[ \t]+(?:de[ \t]+)?facture[ \t]*:[ \t]*(?<date>\d{2}/\d{2}/\d{4})[ \t]*$")
            Dim value As DateTime
            If DateTime.TryParseExact(match.Groups("date").Value, "dd/MM/yyyy",
                                      CultureInfo.InvariantCulture, DateTimeStyles.None, value) Then
                dates.Add(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            End If
        Next

        If amounts.Count = 1 Then proposed("montant_ttc") = JToken.FromObject(amounts.Single())
        If dates.Count = 1 Then proposed("date_document") = dates.Single()

        extracted("champs_proposes") = proposed
        extracted("statut_champs") = If(amounts.Count > 1 OrElse dates.Count > 1,
                                        "Ambigu_a_verifier",
                                        If(proposed.HasValues, "Champs_a_verifier", "Aucune_donnee_fiable"))
        Return extracted.ToString(Formatting.None)
    End Function
End Class
