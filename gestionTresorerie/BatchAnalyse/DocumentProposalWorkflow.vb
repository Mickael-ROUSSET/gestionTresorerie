Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports UglyToad.PdfPig

''' <summary>Un moteur produit des donnees brutes a verifier, sans ecriture comptable.</summary>
Public Interface IDocumentExtractionEngine
    ReadOnly Property EngineName As String
    Function ExtractAsync(documentPath As String) As Task(Of String)
End Interface

''' <summary>
''' Lecture locale des PDF textuels. Les images et PDF scannes restent a examiner
''' manuellement : aucun appel a un service externe et aucune reconnaissance inventee.
''' </summary>
Public NotInheritable Class LocalPdfTextExtractionEngine
    Implements IDocumentExtractionEngine

    Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
        Get
            Return "pdf-text-local"
        End Get
    End Property

    Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
        If Not File.Exists(documentPath) Then Throw New FileNotFoundException("Document introuvable.", documentPath)
        Dim extension = Path.GetExtension(documentPath).ToLowerInvariant()
        Dim extractedText As String = String.Empty

        Select Case extension
            Case ".pdf"
                Dim text As New StringBuilder()
                Using pdf = PdfDocument.Open(documentPath)
                    For Each page In pdf.GetPages()
                        If text.Length >= 16000 Then Exit For
                        text.AppendLine(page.Text)
                    Next
                End Using
                extractedText = If(text.Length > 16000, text.ToString(0, 16000), text.ToString())
            Case ".jpg", ".jpeg", ".png"
                ' Pas d'OCR implicite : l'operateur choisira un moteur adapte.
            Case Else
                Throw New NotSupportedException("Type de document non pris en charge.")
        End Select

        Dim status = If(String.IsNullOrWhiteSpace(extractedText), "OCR_ou_saisie_manuelle_requis", "Texte_a_verifier")
        Dim result = JsonConvert.SerializeObject(New With {
            .type_document = "A_verifier",
            .statut_extraction = status,
            .texte = extractedText
        })
        Return Task.FromResult(result)
    End Function
End Class

''' <summary>
''' File locale de propositions. Une proposition n'est jamais une operation validee.
''' Aucune methode de cette classe n'ecrit en SQL ou ne renomme le document source.
''' </summary>
Public NotInheritable Class DocumentProposalStore
    Private ReadOnly _folder As String

    Public Sub New(folder As String)
        If String.IsNullOrWhiteSpace(folder) Then Throw New ArgumentException("Dossier requis.", NameOf(folder))
        _folder = Path.GetFullPath(folder)
    End Sub

    ''' <returns>True si une nouvelle proposition est creee; False si le contenu existe deja.</returns>
    Public Function SavePending(sourcePath As String, engineName As String, extractedJson As String,
                                Optional expectedSha256 As String = Nothing) As Boolean
        If Not File.Exists(sourcePath) Then Throw New FileNotFoundException("Document introuvable.", sourcePath)
        If String.IsNullOrWhiteSpace(engineName) Then Throw New ArgumentException("Moteur requis.", NameOf(engineName))
        If String.IsNullOrWhiteSpace(extractedJson) Then Throw New ArgumentException("Extraction vide.", NameOf(extractedJson))

        ' Rejeter toute reponse non JSON avant de creer un fichier.
        Dim extraction = JObject.Parse(extractedJson)
        Dim sha256 = AttachmentStorage.ComputeSha256(sourcePath)
        ' Ne jamais associer une extraction ancienne a un document modifie entre-temps.
        If expectedSha256 IsNot Nothing AndAlso
           Not String.Equals(sha256, expectedSha256, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException("Document source modifie pendant l'extraction : proposition refusee.")
        End If
        Dim proposal = New JObject From {
            {"schema_version", 1},
            {"statut", "EnAttenteValidation"},
            {"source", Path.GetFullPath(sourcePath)},
            {"source_sha256", sha256},
            {"moteur", engineName},
            {"cree_utc", DateTime.UtcNow.ToString("O", Globalization.CultureInfo.InvariantCulture)},
            {"extraction", extraction}
        }

        Directory.CreateDirectory(_folder)
        Dim destination = Path.Combine(_folder, sha256 & ".json")
        ' Ecrire hors du nom public : une revue ne doit jamais voir un JSON partiel.
        ' Le renommage dans le meme dossier publie la proposition complete atomiquement.
        Dim temporary = Path.Combine(_folder, ".proposal-tmp-" & Guid.NewGuid().ToString("N"))
        Try
            Using stream As New FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                Using writer As New StreamWriter(stream, New UTF8Encoding(False))
                    writer.Write(proposal.ToString(Formatting.Indented))
                    writer.Flush()
                    stream.Flush(True)
                End Using
            End Using
            ' File.Move ne remplace jamais une proposition deja publiee.
            File.Move(temporary, destination)
            Return True
        Catch ex As IOException When File.Exists(destination)
            ' Collision ou import concurrent du meme contenu : ne pas ecraser.
            Return False
        Finally
            If File.Exists(temporary) Then File.Delete(temporary)
        End Try
    End Function
End Class

''' <summary>Coordonne extraction et mise en attente, sans approbation automatique.</summary>
Public NotInheritable Class DocumentProposalWorkflow
    Private ReadOnly _engine As IDocumentExtractionEngine
    Private ReadOnly _store As DocumentProposalStore

    Public Sub New(engine As IDocumentExtractionEngine, store As DocumentProposalStore)
        If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
        If store Is Nothing Then Throw New ArgumentNullException(NameOf(store))
        _engine = engine
        _store = store
    End Sub

    Public Async Function ProposeAsync(sourcePath As String) As Task(Of Boolean)
        Dim sourceSha256 = AttachmentStorage.ComputeSha256(sourcePath)
        Dim json = Await _engine.ExtractAsync(sourcePath).ConfigureAwait(False)
        Return _store.SavePending(sourcePath, _engine.EngineName, json, sourceSha256)
    End Function
End Class
