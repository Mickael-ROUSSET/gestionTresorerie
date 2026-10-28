Option Strict On
Option Explicit On

Imports System
Imports System.Globalization
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' Transforme une extraction JSON deja obtenue en proposition en memoire.
''' Aucun acces Gmail, aucune ecriture SQL et aucune approbation implicite.
''' Le sens est obligatoirement fourni par l'appelant : il n'est jamais deduit.
''' </summary>
Public NotInheritable Class PropositionMouvementMapper
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Contrat minimal hors ligne : date (ISO yyyy-MM-dd), montant (nombre JSON
    ''' positif), libelle (texte) et reference (texte facultatif).
    ''' Les formats ambigus sont refuses pour permettre une correction humaine.
    ''' </summary>
    Public Shared Function DepuisExtraction(cleSource As String,
                                            typeDocument As String,
                                            extraction As JObject,
                                            sens As PropositionMouvement.SensMouvement) As PropositionMouvement
        If extraction Is Nothing Then Throw New ArgumentNullException(NameOf(extraction))

        Dim dateToken = extraction("date")
        If dateToken Is Nothing OrElse dateToken.Type <> JTokenType.String Then
            Throw New FormatException("Date documentaire absente ou non textuelle.")
        End If
        Dim dateProposee As DateTime
        If Not DateTime.TryParseExact(dateToken.Value(Of String)(), "yyyy-MM-dd",
                                      CultureInfo.InvariantCulture, DateTimeStyles.None, dateProposee) Then
            Throw New FormatException("Date documentaire attendue au format yyyy-MM-dd.")
        End If

        Dim montantToken = extraction("montant")
        If montantToken Is Nothing OrElse
           (montantToken.Type <> JTokenType.Integer AndAlso montantToken.Type <> JTokenType.Float) Then
            Throw New FormatException("Montant documentaire attendu sous forme de nombre JSON.")
        End If
        Dim montant As Decimal
        If Not Decimal.TryParse(montantToken.ToString(Formatting.None),
                                NumberStyles.AllowLeadingSign Or NumberStyles.AllowDecimalPoint,
                                CultureInfo.InvariantCulture, montant) Then
            Throw New FormatException("Montant documentaire non representable en decimal.")
        End If

        Dim libelleToken = extraction("libelle")
        If libelleToken Is Nothing OrElse libelleToken.Type <> JTokenType.String Then
            Throw New FormatException("Libelle documentaire absent ou non textuel.")
        End If

        Dim reference As String = Nothing
        Dim referenceToken = extraction("reference")
        If referenceToken IsNot Nothing AndAlso referenceToken.Type <> JTokenType.Null Then
            If referenceToken.Type <> JTokenType.String Then
                Throw New FormatException("Reference documentaire attendue sous forme de texte.")
            End If
            reference = referenceToken.Value(Of String)()
        End If

        Return New PropositionMouvement(cleSource, typeDocument, dateProposee, montant,
                                        sens, libelleToken.Value(Of String)(), reference)
    End Function
End Class
