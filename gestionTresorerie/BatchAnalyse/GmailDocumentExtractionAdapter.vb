Option Strict On
Option Explicit On

Imports System
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' Convertit une reponse JSON d'extraction deja obtenue en document metier
''' en memoire. Ne lit aucun message, fichier ou service et n'enregistre rien.
''' Seuls Facture et Cheque sont autorises (aucune instanciation par reflexion).
''' </summary>
Public NotInheritable Class GmailDocumentExtractionAdapter
    Private Sub New()
    End Sub

    Public Shared Function Extraire(jsonBrut As String) As DocumentAgumaaa
        If String.IsNullOrWhiteSpace(jsonBrut) Then
            Throw New ArgumentException("JSON d'extraction requis.", NameOf(jsonBrut))
        End If

        Dim racine As JObject
        Try
            racine = JObject.Parse(jsonBrut)
        Catch ex As JsonException
            Throw New ArgumentException("JSON d'extraction invalide.", NameOf(jsonBrut), ex)
        End Try

        Dim typeJeton = racine("type_document")
        If typeJeton Is Nothing OrElse typeJeton.Type <> JTokenType.String OrElse
           racine("donnees_extraites") Is Nothing OrElse
           racine("donnees_extraites").Type <> JTokenType.Object Then
            Throw New ArgumentException("Type ou donnees extraites manquants.", NameOf(jsonBrut))
        End If

        Dim document As DocumentAgumaaa
        Select Case typeJeton.Value(Of String)()
            Case "Facture"
                document = JsonConvert.DeserializeObject(Of Facture)(Facture.ParseJson(jsonBrut))
            Case "Cheque"
                document = JsonConvert.DeserializeObject(Of Cheque)(Cheque.ParseJson(jsonBrut))
            Case Else
                Throw New ArgumentException("Type de document non pris en charge.", NameOf(jsonBrut))
        End Select
        If document Is Nothing Then
            Throw New ArgumentException("Impossible de convertir le document extrait.", NameOf(jsonBrut))
        End If
        Return document
    End Function
End Class
