Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
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


    ''' <summary>Piece jointe et JSON deja extrait, sans lecture de messages.</summary>
    Public NotInheritable Class PieceJson
        Public Sub New(account As String, uidValidity As UInteger, uid As UInteger,
                       attachmentIndex As Integer, jsonExtraction As String,
                       sens As PropositionMouvement.SensMouvement)
            Me.CleSource = GmailDocumentSourceKey.MakeKey(account, uidValidity, uid, attachmentIndex)
            Me.JsonExtraction = jsonExtraction
            Me.Sens = sens
        End Sub

        Public ReadOnly Property CleSource As String
        Public ReadOnly Property JsonExtraction As String
        Public ReadOnly Property Sens As PropositionMouvement.SensMouvement
    End Class

    ''' <summary>
    ''' Traite un lot de JSON deja extraits. Chaque erreur de conversion est
    ''' isolee, avec sa position d'origine, sans exposer le JSON dans le rejet.
    ''' La cle d'une piece invalide reste reservee contre les doublons.
    ''' Aucun appel Gmail, acces disque, SQL ou approbation.
    ''' </summary>
    Public Shared Function ProposerLot(pieces As IEnumerable(Of PieceJson)) As PropositionDocumentService.Resultat
        If pieces Is Nothing Then Throw New ArgumentNullException(NameOf(pieces))
        Dim entrees As New List(Of PropositionDocumentService.Entree)()
        Dim erreurs As New Dictionary(Of Integer, String)()
        Dim index As Integer = 0
        For Each piece In pieces
            If piece Is Nothing Then
                entrees.Add(Nothing)
            Else
                Dim document As DocumentAgumaaa = Nothing
                Try
                    document = Extraire(piece.JsonExtraction)
                Catch ex As Exception When TypeOf ex Is ArgumentException OrElse
                                           TypeOf ex Is JsonException OrElse
                                           TypeOf ex Is FormatException OrElse
                                           TypeOf ex Is OverflowException OrElse
                                           TypeOf ex Is InvalidCastException
                    erreurs.Add(index, "Extraction JSON invalide ou non prise en charge.")
                End Try
                entrees.Add(New PropositionDocumentService.Entree(document, piece.CleSource, piece.Sens))
            End If
            index += 1
        Next
        Dim resultat = New PropositionDocumentService().Proposer(entrees)
        If erreurs.Count = 0 Then Return resultat
        Dim rejets As New List(Of PropositionDocumentService.Rejet)()
        For Each rejet In resultat.Rejets
            Dim motif As String = Nothing
            If erreurs.TryGetValue(rejet.IndexEntree, motif) AndAlso
               String.Equals(rejet.Motif, "Document absent.", StringComparison.Ordinal) Then
                rejets.Add(New PropositionDocumentService.Rejet(rejet.IndexEntree, rejet.CleSource, motif))
            Else
                rejets.Add(rejet)
            End If
        Next
        Return New PropositionDocumentService.Resultat(
            New List(Of PropositionMouvement)(resultat.Propositions), rejets)
    End Function

    Private Shared Sub VerifierNomsProprietes(jeton As JToken)
        If TypeOf jeton Is JObject Then
            Dim noms As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each propriete As JProperty In DirectCast(jeton, JObject).Properties()
                If Not noms.Add(propriete.Name) Then
                    Throw New ArgumentException("Proprietes JSON ambigues (casse differente).")
                End If
                VerifierNomsProprietes(propriete.Value)
            Next
        ElseIf TypeOf jeton Is JArray Then
            For Each element As JToken In DirectCast(jeton, JArray)
                VerifierNomsProprietes(element)
            Next
        End If
    End Sub

    Public Shared Function Extraire(jsonBrut As String) As DocumentAgumaaa
        If String.IsNullOrWhiteSpace(jsonBrut) Then
            Throw New ArgumentException("JSON d'extraction requis.", NameOf(jsonBrut))
        End If

        Dim racine As JObject
        Try
            ' Les reponses IA avec des proprietes dupliquees sont ambigues :
            ' ne jamais choisir silencieusement la derniere valeur (montant, type...).
            racine = JObject.Parse(jsonBrut, New JsonLoadSettings With {
                .DuplicatePropertyNameHandling = Newtonsoft.Json.Linq.DuplicatePropertyNameHandling.Error})
        Catch ex As JsonException
            Throw New ArgumentException("JSON d'extraction invalide.", NameOf(jsonBrut), ex)
        End Try

        ' Json.NET peut rapprocher des noms qui ne different que par la casse.
        ' Une extraction ambigue ne doit jamais choisir implicitement une valeur.
        VerifierNomsProprietes(racine)

        Dim typeJeton = racine("type_document")
        If typeJeton Is Nothing OrElse typeJeton.Type <> JTokenType.String OrElse
           racine("donnees_extraites") Is Nothing OrElse
           racine("donnees_extraites").Type <> JTokenType.Object Then
            Throw New ArgumentException("Type ou donnees extraites manquants.", NameOf(jsonBrut))
        End If

        ' Une transaction absente ou d'un type inattendu ne constitue pas
        ' une extraction exploitable : refuser avant le mapping metier.
        Dim donnees As JObject = DirectCast(racine("donnees_extraites"), JObject)
        If donnees("transaction") Is Nothing OrElse
           donnees("transaction").Type <> JTokenType.Object Then
            Throw New ArgumentException("Transaction extraite manquante ou invalide.", NameOf(jsonBrut))
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
