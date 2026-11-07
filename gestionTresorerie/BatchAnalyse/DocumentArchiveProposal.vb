Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography

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
        If Not DocumentFilePreview.Supports(source) Then Throw New ArgumentException("Format de fichier non pris en charge.")
        If Not Path.IsPathFullyQualified(root) OrElse Not Directory.Exists(root) Then
            Throw New IOException("La racine d’archivage configurée est inaccessible.")
        End If
        Dim document = GmailDocumentExtractionAdapter.Extraire(json)
        Dim facture = TryCast(document, Facture)
        Dim dateDocument As Date
        Dim reference As String
        Dim montant As Decimal
        Dim devise As String
        Dim tiers As String
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
            nom = $"FAC_{SafeSegment(reference)}_{fournisseur}{extension}"
            document.CategorieDoc = "Facture"
            document.metaDonnees = Facture.ParseJson(json)
        Else
            Dim cheque = DirectCast(document, Cheque)
            dateDocument = ReadDate(cheque.dateChq)
            reference = cheque.numero_du_cheque
            montant = cheque.montant_numerique
            devise = cheque.devise
            tiers = cheque.destinataire
            Dim compte = setting("numCptAgumaaa")
            If String.IsNullOrWhiteSpace(compte) OrElse String.IsNullOrWhiteSpace(cheque.emetteur_numero_compte) Then
                Throw New ArgumentException("Compte émetteur ou compte AGUMAAA manquant. Le sens du chèque doit être connu.")
            End If
            sens = If(cheque.emetteur_numero_compte.Contains(compte, StringComparison.OrdinalIgnoreCase), "Emis", "Reçus")
            dossier = Path.Combine(setting("repFichiersDocumentsChèques"),
                dateDocument.ToString("yyyy", CultureInfo.InvariantCulture), sens)
            nom = $"CHQ_{SafeSegment(reference)}{extension}"
            document.CategorieDoc = "Cheque"
            document.metaDonnees = Cheque.ParseJson(json)
        End If
        If montant = 0D OrElse String.IsNullOrWhiteSpace(devise) Then Throw New ArgumentException("Montant ou devise manquant.")
        document.DateDoc = dateDocument

        Dim prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root))
        If Not Path.EndsInDirectorySeparator(prefix) Then prefix &= Path.DirectorySeparatorChar
        Dim archive = Path.GetFullPath(Path.Combine(prefix, setting("repRacineComptabilité"), dossier, nom))
        If Not archive.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException("Le répertoire de classement sort de la racine configurée.")
        End If
        document.CheminDoc = archive
        Dim resume = $"{document.CategorieDoc} — {sens}{vbCrLf}" &
            $"Référence : {reference}{vbCrLf}Date : {document.DateDoc:dd/MM/yyyy}{vbCrLf}" &
            $"Montant : {montant:N2} {devise}{vbCrLf}Tiers : {tiers}"
        Return New DocumentArchiveProposal(source, root, hash, document, resume)
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
        Using input = File.Open(_source, FileMode.Open, FileAccess.Read, FileShare.Read)
            If Not String.Equals(Convert.ToHexString(SHA256.HashData(input)), _hash, StringComparison.Ordinal) Then
                Throw New IOException("Le document a changé depuis son analyse. Relancez l’analyse.")
            End If
            input.Position = 0
            Dim directory = Path.GetDirectoryName(Destination)
            Directory.CreateDirectory(directory)
            If Not File.Exists(Destination) Then
                Dim temporary = Path.Combine(directory, "." & Guid.NewGuid().ToString("N") & ".tmp")
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
            Using archived = File.Open(Destination, FileMode.Open, FileAccess.Read, FileShare.Read)
                If Not String.Equals(Convert.ToHexString(SHA256.HashData(archived)), _hash, StringComparison.Ordinal) Then
                    Throw New IOException("Un autre document porte déjà ce nom dans l’archive. Aucune donnée n’a été écrasée.")
                End If
                saveDocument(_document)
            End Using
        End Using
    End Sub

    Private Shared Function ReadDate(value As String) As Date
        Dim parsed As Date
        If Not Date.TryParseExact(value, {"yyyy-MM-dd", "dd/MM/yyyy"}, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, parsed) Then
            Throw New ArgumentException("Date du document manquante ou invalide.")
        End If
        Return parsed
    End Function

    Private Shared Function SafeSegment(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Throw New ArgumentException("Référence de classement manquante.")
        Dim segment = UtilitairesIdentite.NettoyerNomFichier(value)
        If String.IsNullOrWhiteSpace(segment) Then Throw New ArgumentException("Référence de classement invalide.")
        Return segment
    End Function
End Class
