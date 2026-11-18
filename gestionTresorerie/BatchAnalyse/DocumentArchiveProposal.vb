Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports gestionTresorerie.Agumaaa
Imports Newtonsoft.Json.Linq

Public NotInheritable Class DocumentArchiveProposal
    Private ReadOnly _source As String
    Private ReadOnly _root As String
    Private ReadOnly _hash As String
    Private ReadOnly _document As DocumentAgumaaa

    Private Sub New(source As String, root As String, hash As String, document As DocumentAgumaaa, summary As String)
        _source = source
        _root = root
        _hash = hash
        _document = document
        Me.Summary = summary
    End Sub

    Public ReadOnly Property Summary As String

    Public ReadOnly Property Destination As String
        Get
            Return _document.CheminDoc
        End Get
    End Property

    Public Shared Function Prepare(source As String, hash As String, json As String, root As String,
                                   setting As Func(Of String, String)) As DocumentArchiveProposal
        Return PrepareCore(source, hash, json, root, setting, Nothing)
    End Function

    Friend Shared Function PrepareTest(source As String, hash As String, json As String, root As String,
                                       chequeEmis As Boolean?, Optional archiveOnDrive As Boolean = False) As DocumentArchiveProposal
        Return PrepareCore(source, hash, json, root, AddressOf OfflineTestWorkspace.DocumentArchiveTestSetting, chequeEmis, archiveOnDrive)
    End Function

    Private Shared Function PrepareCore(source As String, hash As String, json As String, root As String,
                                        setting As Func(Of String, String), chequeEmisTest As Boolean?, Optional archiveOnDrive As Boolean = False) As DocumentArchiveProposal
        If Not DocumentFilePreview.Supports(source) Then Throw New ArgumentException("Format de fichier non pris en charge.")
        If Not Path.IsPathFullyQualified(root) OrElse Not Directory.Exists(root) Then
            Throw New IOException("Le dossier d’archivage est inaccessible. Vérifiez le lecteur et le chemin : " & root)
        End If
        Dim document = GmailDocumentExtractionAdapter.Extraire(json)
        Dim facture = TryCast(document, Facture)
        Dim dateDocument As Date
        Dim reference As String
        Dim montant As Decimal
        Dim devise As String
        Dim tiers As String
        Dim issuer As String = Nothing
        Dim sens As String
        Dim dossier As String
        Dim extension = Path.GetExtension(source)
        Dim nom As String
        If facture IsNot Nothing Then
            dateDocument = ReadDate(facture.date_facture)
            reference = facture.numero_facture
            montant = facture.montant_ttc
            devise = facture.devise
            tiers = facture.fournisseur_nom
            If String.IsNullOrWhiteSpace(tiers) Then Throw New ArgumentException("Fournisseur manquant.")
            Dim fournisseur = SafeSegment(tiers)
            sens = If(fournisseur.Contains("AGUMAAA", StringComparison.OrdinalIgnoreCase), "Émise", "Reçue")
            dossier = Path.Combine(setting("repFichiersDocumentsFactures"),
                setting(If(sens = "Émise", "repFichiersFacturesEmises", "repFichiersFacturesRecues")),
                dateDocument.ToString("yyyy", CultureInfo.InvariantCulture), fournisseur)
            nom = $"FAC_{SafeReference(reference)}_{fournisseur}{extension}"
            document.CategorieDoc = "Facture"
            document.metaDonnees = Facture.ParseJson(json)
        Else
            Dim cheque = DirectCast(document, Cheque)
            dateDocument = ReadDate(cheque.dateChq)
            reference = cheque.numero_du_cheque
            montant = cheque.montant_numerique
            devise = cheque.devise
            tiers = cheque.destinataire
            issuer = cheque.emetteur_du_cheque
            If chequeEmisTest.HasValue Then
                sens = If(chequeEmisTest.Value, "Emis", "Reçus")
                document.SousCategorieDoc = sens
            Else
                Dim compte = setting("numCptAgumaaa")
                If String.IsNullOrWhiteSpace(compte) OrElse String.IsNullOrWhiteSpace(cheque.emetteur_numero_compte) Then
                    Throw New ArgumentException("Compte émetteur ou compte AGUMAAA manquant. Le sens du chèque doit être connu.")
                End If
                sens = If(cheque.emetteur_numero_compte.Contains(compte, StringComparison.OrdinalIgnoreCase), "Emis", "Reçus")
            End If
            dossier = Path.Combine(setting("repFichiersDocumentsChèques"),
                dateDocument.ToString("yyyy", CultureInfo.InvariantCulture), sens)
            nom = $"CHQ_{SafeReference(reference)}{extension}"
            document.CategorieDoc = "Cheque"
            document.metaDonnees = Cheque.ParseJson(json)
        End If
        If montant = 0D OrElse String.IsNullOrWhiteSpace(devise) Then Throw New ArgumentException("Montant ou devise manquant.")
        document.DateDoc = dateDocument

        Dim summaryText = $"{document.CategorieDoc} — {sens}{vbCrLf}" &
            $"Référence : {reference}{vbCrLf}Date : {document.DateDoc:dd/MM/yyyy}{vbCrLf}" &
            $"Montant : {montant:N2} {devise}{vbCrLf}Tiers : {tiers}"
        If Not String.IsNullOrWhiteSpace(issuer) Then summaryText &= vbCrLf & "Émetteur : " & issuer
        Return BuildProposal(source, root, hash, document, If(archiveOnDrive, "TEST", setting("repRacineComptabilité")), dossier, nom, summaryText)
    End Function

    Public Shared Function PrepareAdministrativeTest(source As String, hash As String, root As String, kind As TypeDocument,
                                                     party As String, dateText As String, reference As String,
                                                     amountText As String, Optional firstName As String = "",
                                                     Optional address As String = "", Optional archiveOnDrive As Boolean = False) As DocumentArchiveProposal
        If Not DocumentFilePreview.Supports(source) Then Throw New ArgumentException("Format de fichier non pris en charge.")
        If Not Path.IsPathFullyQualified(root) OrElse Not Directory.Exists(root) Then
            Throw New IOException("Le dossier d’archivage TEST est inaccessible. Vérifiez le lecteur et le chemin : " & root)
        End If
        Dim folder As String
        Dim filenamePrefix As String
        Dim label As String
        Select Case kind
            Case TypeDocument.FormulaireAdhesion
                folder = "Adhesions"
                filenamePrefix = "ADH"
                label = "Bulletin d’adhésion"
            Case TypeDocument.QuestionnaireMedical
                folder = "QuestionnairesMedicaux"
                filenamePrefix = "QS"
                label = "Questionnaire médical"
            Case TypeDocument.Recu
                folder = "Recus"
                filenamePrefix = "RECU"
                label = "Reçu"
            Case Else
                Throw New ArgumentException("Type de document administratif invalide.", NameOf(kind))
        End Select

        party = party.Trim()
        If party.Length = 0 OrElse party.Length > 80 Then Throw New ArgumentException("Renseignez le nom indiqué sur le document.")
        Dim documentDate = ReadDate(dateText.Trim())
        If documentDate.Year < 1753 Then Throw New ArgumentException("La date du document est trop ancienne pour SQL TEST.")
        If Not Regex.IsMatch(hash, "\A[0-9A-F]{64}\z", RegexOptions.CultureInvariant) Then
            Throw New ArgumentException("Empreinte du document invalide.", NameOf(hash))
        End If
        Dim fields As New JObject From {{"nom_complet", party}, {"date_document", documentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}}
        Dim summary = label & vbCrLf & $"Nom : {party}{vbCrLf}Date : {documentDate:dd/MM/yyyy}"
        If kind = TypeDocument.FormulaireAdhesion Then
            firstName = firstName.Trim()
            address = address.Trim()
            If firstName.Length > 80 OrElse address.Length > 240 Then Throw New ArgumentException("Prénom ou adresse trop longue.")
            fields("nom") = party
            If firstName.Length > 0 Then
                fields("prenom") = firstName
                fields("nom_complet") = party & " " & firstName
                summary &= vbCrLf & "Prénom : " & firstName
            End If
            If address.Length > 0 Then
                fields("adresse") = address
                summary &= vbCrLf & "Adresse : " & address
            End If
        End If
        If kind = TypeDocument.Recu Then
            reference = reference.Trim()
            If reference.Length > 60 Then Throw New ArgumentException("Le numéro du reçu dépasse 60 caractères.")
            If reference.Length > 0 Then
                fields("reference") = reference
                summary &= vbCrLf & "Référence : " & reference
            End If
            If Not String.IsNullOrWhiteSpace(amountText) Then
                Dim amount As Decimal
                Dim normalized = Regex.Replace(amountText.Trim(), "[ \u00A0\u202F]", "").Replace(","c, "."c)
                If Not Regex.IsMatch(normalized, "\A\d+(?:\.\d{1,2})?\z") OrElse Not Decimal.TryParse(normalized,
                    NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, amount) OrElse amount <= 0D Then
                    Throw New ArgumentException("Renseignez un montant de reçu valide en euros.")
                End If
                fields("montant_numerique") = amount
                fields("devise") = "EUR"
                summary &= vbCrLf & $"Montant : {amount:N2} EUR"
            End If
        End If

        Dim document As New DocumentAgumaaaImpl(0, documentDate, Nothing, Nothing, kind.ToString(), Nothing, 0,
            fields.ToString(Newtonsoft.Json.Formatting.None), Date.Now)
        ' Un nom neutre évite d’exposer l’identité d’un adhérent dans les noms de fichiers.
        Dim fileName = filenamePrefix & "_" & documentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) &
            "_" & hash.Substring(0, 16) & Path.GetExtension(source)
        Return BuildProposal(source, root, hash, document, If(archiveOnDrive, "TEST", OfflineTestWorkspace.DocumentArchiveTestSetting("repRacineComptabilité")),
            Path.Combine(folder, documentDate.ToString("yyyy", CultureInfo.InvariantCulture)), fileName, summary)
    End Function

    Private Shared Function BuildProposal(source As String, root As String, hash As String, document As DocumentAgumaaa,
                                          relativeRoot As String, folder As String, fileName As String,
                                          summary As String) As DocumentArchiveProposal
        Dim prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root))
        If Not Path.EndsInDirectorySeparator(prefix) Then prefix &= Path.DirectorySeparatorChar
        Dim archive = Path.GetFullPath(Path.Combine(prefix, relativeRoot, folder, fileName))
        If Not archive.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException("Le répertoire de classement sort de la racine configurée.")
        End If
        document.CheminDoc = archive
        Return New DocumentArchiveProposal(source, root, hash, document, summary)
    End Function

    Public Shared Function HashSource(source As String) As String
        Using input = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read)
            If input.Length = 0 OrElse input.Length > 25L * 1024 * 1024 Then
                Throw New IOException("La taille du document doit être comprise entre 1 octet et 25 Mio.")
            End If
            Return Convert.ToHexString(SHA256.HashData(input))
        End Using
    End Function

    Public Sub Confirm(saveDocument As Action(Of DocumentAgumaaa))
        If saveDocument Is Nothing Then Throw New ArgumentNullException(NameOf(saveDocument))
        If Not Directory.Exists(_root) Then Throw New IOException("La racine d’archivage configurée est inaccessible.")
        RefuseReparsePoints(_root, Destination)
        Using input = File.Open(_source, FileMode.Open, FileAccess.Read, FileShare.Read)
            ' La source peut avoir grandi depuis la préparation : conserver la limite de 25 Mio.
            If input.Length = 0 OrElse input.Length > 25L * 1024 * 1024 Then
                Throw New IOException("La taille du document doit être comprise entre 1 octet et 25 Mio.")
            End If
            If Not String.Equals(Convert.ToHexString(SHA256.HashData(input)), _hash, StringComparison.Ordinal) Then
                Throw New IOException("Le document a changé depuis son analyse. Relancez l’analyse.")
            End If
            input.Position = 0
            Dim archiveDirectory = Path.GetDirectoryName(Destination)
            Directory.CreateDirectory(archiveDirectory)
            RefuseReparsePoints(_root, Destination)
            If Not File.Exists(Destination) Then
                Dim temporary = Path.Combine(archiveDirectory, "." & Guid.NewGuid().ToString("N") & ".tmp")
                Try
                    Using output = File.Open(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                        input.CopyTo(output)
                        output.Flush(flushToDisk:=True)
                    End Using
                    Try
                        File.Move(temporary, Destination)
                    Catch ex As IOException When File.Exists(Destination)
                        ' Une autre instance a publié l’archive. Vérifier son contenu avant l’écriture SQL.
                    End Try
                Finally
                    File.Delete(temporary)
                End Try
            End If
            RefuseReparsePoints(_root, Destination)
            Using archived = File.Open(Destination, FileMode.Open, FileAccess.Read, FileShare.Read)
                If Not String.Equals(Convert.ToHexString(SHA256.HashData(archived)), _hash, StringComparison.Ordinal) Then
                    Throw New IOException("Un autre document porte déjà ce nom dans l’archive. Aucune donnée n’a été écrasée.")
                End If
                saveDocument(_document)
            End Using
        End Using
    End Sub

    ' Refuser les jonctions et liens symboliques : une destination lexicalement dans TEST
    ' ne doit pas pouvoir pointer vers un emplacement externe.
    Private Shared Sub RefuseReparsePoints(root As String, destination As String)
        Dim basePath = Path.GetFullPath(root)
        Dim targetPath = Path.GetFullPath(destination)
        Dim relative = Path.GetRelativePath(basePath, targetPath)
        If Path.IsPathRooted(relative) OrElse relative = ".." OrElse
           relative.StartsWith(".." & Path.DirectorySeparatorChar, StringComparison.Ordinal) Then
            Throw New IOException("La destination sort de la racine autorisée.")
        End If
        Dim current = basePath
        If (File.GetAttributes(current) And FileAttributes.ReparsePoint) <> 0 Then
            Throw New IOException("Racine d’archivage liée à un autre emplacement.")
        End If
        For Each segment In relative.Split(Path.DirectorySeparatorChar)
            current = Path.Combine(current, segment)
            If File.Exists(current) OrElse Directory.Exists(current) Then
                If (File.GetAttributes(current) And FileAttributes.ReparsePoint) <> 0 Then
                    Throw New IOException("Lien ou jonction interdit dans la destination d’archivage.")
                End If
            End If
        Next
    End Sub

    Private Shared Function ReadDate(value As String) As Date
        Dim parsed As Date
        If Not Date.TryParseExact(value, {"yyyy-MM-dd", "dd/MM/yyyy"}, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, parsed) Then
            Throw New ArgumentException("Date du document manquante ou invalide.")
        End If
        Return parsed
    End Function

    Private Shared Function SafeReference(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Throw New ArgumentException("Référence de classement manquante.")
        Dim invalid = Path.GetInvalidFileNameChars()
        Return New String(value.Trim().Select(Function(character) If(invalid.Contains(character), "_"c, character)).ToArray())
    End Function

    Private Shared Function SafeSegment(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Throw New ArgumentException("Référence de classement manquante.")
        Dim segment = UtilitairesIdentite.NettoyerNomFichier(value)
        If String.IsNullOrWhiteSpace(segment) Then Throw New ArgumentException("Référence de classement invalide.")
        Return segment
    End Function
End Class
