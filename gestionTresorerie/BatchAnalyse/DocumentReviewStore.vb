Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' Enregistre une decision humaine explicite, sans insertion comptable.
''' Les recus de revue sont immuables et distincts des propositions d'origine.
''' </summary>
Public NotInheritable Class DocumentReviewStore
    Public Enum ReviewDecision
        ApprouvePourSaisie
        Rejete
    End Enum

    Private ReadOnly _folder As String

    Public Sub New(proposalsFolder As String)
        If String.IsNullOrWhiteSpace(proposalsFolder) Then Throw New ArgumentException("Dossier requis.", NameOf(proposalsFolder))
        _folder = Path.GetFullPath(proposalsFolder)
    End Sub

    ''' <summary>Retourne les empreintes des propositions sans decision enregistree.</summary>
    Public Function ListPending() As IReadOnlyList(Of String)
        Dim result As New List(Of String)()
        If Not Directory.Exists(_folder) Then Return result

        For Each filePath In Directory.EnumerateFiles(_folder, "*.json", SearchOption.TopDirectoryOnly)
            Dim sha = Path.GetFileNameWithoutExtension(filePath)
            If Not IsSha256(sha) Then Continue For
            Dim receiptPath = ReviewPath(sha)
            If File.Exists(receiptPath) Then
                ValidateReviewReceipt(receiptPath, sha)
            Else
                result.Add(sha)
            End If
        Next
        result.Sort(StringComparer.Ordinal)
        Return result
    End Function

    ''' <summary>
    ''' Retourne les propositions encore en attente pour une future interface de revue.
    ''' Chaque element passe par les memes controles que ReadPendingProposal :
    ''' un fichier corrompu interrompt la liste plutot que d'etre affiche comme fiable.
    ''' Lecture seule, sans approbation et sans ecriture comptable.
    ''' </summary>
    Public Function ListPendingProposals() As IReadOnlyList(Of JObject)
        Dim proposals As New List(Of JObject)()
        For Each sha In ListPending()
            proposals.Add(ReadPendingProposal(sha))
        Next
        Return proposals
    End Function

    ''' <summary>
    ''' Charge une proposition encore en attente pour sa presentation en revue humaine.
    ''' Lecture seule : aucune approbation, aucun recu et aucune ecriture comptable.
    ''' RecordReview reverifie l'empreinte du document avant toute approbation.
    ''' </summary>
    Public Function ReadPendingProposal(sourceSha256 As String) As JObject
        If Not IsSha256(sourceSha256) Then Throw New ArgumentException("Empreinte SHA-256 invalide.", NameOf(sourceSha256))
        Dim sha = sourceSha256.ToUpperInvariant()
        Dim pendingPath = Path.Combine(_folder, sha & ".json")
        If Not File.Exists(pendingPath) Then Throw New FileNotFoundException("Proposition introuvable.", pendingPath)

        Dim proposal = LoadPendingProposal(pendingPath, sha)
        If File.Exists(ReviewPath(sha)) Then
            ValidateReviewReceipt(ReviewPath(sha), sha)
            Throw New InvalidOperationException("Cette proposition a deja fait l'objet d'une revue.")
        End If
        Return proposal
    End Function

    ''' <summary>
    ''' Une approbation autorise seulement une future saisie manuelle/controlee.
    ''' Elle ne cree aucune operation SQL. False indique une decision deja presente.
    ''' </summary>
    Public Function RecordReview(sourceSha256 As String,
                                 reviewer As String,
                                 decision As ReviewDecision,
                                 correctedData As JObject) As Boolean
        If Not IsSha256(sourceSha256) Then Throw New ArgumentException("Empreinte SHA-256 invalide.", NameOf(sourceSha256))
        If String.IsNullOrWhiteSpace(reviewer) Then Throw New ArgumentException("Identite du validateur requise.", NameOf(reviewer))
        If decision <> ReviewDecision.ApprouvePourSaisie AndAlso decision <> ReviewDecision.Rejete Then
            Throw New ArgumentOutOfRangeException(NameOf(decision))
        End If
        If decision = ReviewDecision.ApprouvePourSaisie AndAlso
           (correctedData Is Nothing OrElse Not correctedData.HasValues) Then
            Throw New ArgumentException("Les donnees verifiees sont requises pour approuver.", NameOf(correctedData))
        End If

        Dim sha = sourceSha256.ToUpperInvariant()
        ' Une decision deja publiee reste definitive : un nouvel appel ne doit pas
        ' revalider un document source supprime ou modifie depuis la revue.
        If File.Exists(ReviewPath(sha)) Then
            ValidateReviewReceipt(ReviewPath(sha), sha)
            Return False
        End If

        Dim pendingPath = Path.Combine(_folder, sha & ".json")
        If Not File.Exists(pendingPath) Then Throw New FileNotFoundException("Proposition introuvable.", pendingPath)

        Dim proposal = LoadPendingProposal(pendingPath, sha)

        ' Une approbation doit correspondre au document exact qui a ete extrait.
        ' Une proposition obsolete peut etre rejetee, mais jamais approuvee.
        If decision = ReviewDecision.ApprouvePourSaisie Then
            Dim sourcePath = If(proposal("source") Is Nothing, "", proposal("source").ToString())
            If String.IsNullOrWhiteSpace(sourcePath) OrElse Not File.Exists(sourcePath) Then
                Throw New InvalidDataException("Document source introuvable : approbation impossible.")
            End If
            If Not String.Equals(AttachmentStorage.ComputeSha256(sourcePath), sha, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException("Document source modifie depuis l'extraction : approbation impossible.")
            End If
        End If

        Dim receipt As New JObject From {
            {"schema_version", 1},
            {"source_sha256", sha},
            {"decision", decision.ToString()},
            {"validateur", reviewer.Trim()},
            {"revue_utc", DateTime.UtcNow.ToString("O", Globalization.CultureInfo.InvariantCulture)}
        }
        If correctedData IsNot Nothing Then receipt.Add("donnees_corrigees", correctedData.DeepClone())

        Dim destination = ReviewPath(sha)
        Dim temporary = Path.Combine(_folder, ".review-tmp-" & Guid.NewGuid().ToString("N"))
        Try
            Using stream As New FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                Using writer As New StreamWriter(stream, New UTF8Encoding(False))
                    writer.Write(receipt.ToString(Formatting.Indented))
                    writer.Flush()
                    stream.Flush(True)
                End Using
            End Using
            ' Le renommage dans le meme dossier publie un recu complet d'un seul coup.
            ' File.Move sans ecrasement garantit une seule decision par empreinte.
            File.Move(temporary, destination)
            Return True
        Catch ex As IOException When File.Exists(destination)
            ValidateReviewReceipt(destination, sha)
            Return False
        Finally
            If File.Exists(temporary) Then File.Delete(temporary)
        End Try
    End Function

    ''' <summary>
    ''' Convertit les JSON tronques ou malformes en erreur documentaire explicite.
    ''' La meme validation est appliquee a la lecture et a la decision humaine.
    ''' </summary>
    Private Shared Function LoadPendingProposal(pendingPath As String, sha As String) As JObject
        Dim proposal As JObject
        Try
            proposal = JObject.Parse(File.ReadAllText(pendingPath, Encoding.UTF8))
        Catch ex As JsonException
            Throw New InvalidDataException("JSON de proposition invalide.", ex)
        End Try
        ValidatePendingProposal(proposal, sha)
        Return proposal
    End Function

    ''' <summary>
    ''' Validation partagee : une proposition alteree ne doit etre ni presentee
    ''' comme valide, ni approuvee directement en contournant l'interface.
    ''' </summary>
    Private Shared Sub ValidatePendingProposal(proposal As JObject, sha As String)
        Dim recordedHash = If(proposal("source_sha256") Is Nothing, "", proposal("source_sha256").ToString())
        Dim recordedStatus = If(proposal("statut") Is Nothing, "", proposal("statut").ToString())
        Dim schemaVersion = proposal("schema_version")
        Dim extraction = proposal("extraction")
        If Not String.Equals(recordedHash, sha, StringComparison.Ordinal) OrElse
           Not String.Equals(recordedStatus, "EnAttenteValidation", StringComparison.Ordinal) OrElse
           schemaVersion Is Nothing OrElse schemaVersion.Type <> JTokenType.Integer OrElse
           Not String.Equals(schemaVersion.ToString(), "1", StringComparison.Ordinal) OrElse
           extraction Is Nothing OrElse extraction.Type <> JTokenType.Object Then
            Throw New InvalidDataException("Schema ou contenu de proposition invalide.")
        End If
    End Sub

    ''' <summary>
    ''' Un recu present mais altere ne doit pas faire disparaitre silencieusement
    ''' une proposition du catalogue ni etre traite comme une decision fiable.
    ''' </summary>
    Private Shared Sub ValidateReviewReceipt(receiptPath As String, sha As String)
        Dim receipt As JObject
        Try
            receipt = JObject.Parse(File.ReadAllText(receiptPath, Encoding.UTF8))
        Catch ex As JsonException
            Throw New InvalidDataException("Recu de revue invalide : JSON corrompu.", ex)
        End Try
        Dim version = receipt("schema_version")
        Dim recordedSha = If(receipt("source_sha256") Is Nothing, "", receipt("source_sha256").ToString())
        Dim decision = If(receipt("decision") Is Nothing, "", receipt("decision").ToString())
        Dim reviewer = If(receipt("validateur") Is Nothing, "", receipt("validateur").ToString())
        If version Is Nothing OrElse version.Type <> JTokenType.Integer OrElse
           Not String.Equals(version.ToString(), "1", StringComparison.Ordinal) OrElse
           Not String.Equals(recordedSha, sha, StringComparison.Ordinal) OrElse
           (Not String.Equals(decision, ReviewDecision.Rejete.ToString(), StringComparison.Ordinal) AndAlso
            Not String.Equals(decision, ReviewDecision.ApprouvePourSaisie.ToString(), StringComparison.Ordinal)) OrElse
           String.IsNullOrWhiteSpace(reviewer) Then
            Throw New InvalidDataException("Recu de revue invalide : schema ou decision incoherente.")
        End If
    End Sub

    Private Function ReviewPath(sha As String) As String
        Return Path.Combine(_folder, sha.ToUpperInvariant() & ".review.json")
    End Function

    Private Shared Function IsSha256(value As String) As Boolean
        Return value IsNot Nothing AndAlso Regex.IsMatch(value, "\A[0-9a-fA-F]{64}\z")
    End Function
End Class
