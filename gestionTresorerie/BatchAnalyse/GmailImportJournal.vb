Imports System.IO
Imports System.Text

''' <summary>
''' Journal des messages dont toutes les pieces jointes ont ete sauvegardees.
''' Une ligne est ajoutee seulement apres succes complet; reprise idempotente.
''' </summary>
Public NotInheritable Class GmailImportJournal
    Private ReadOnly _journalPath As String
    Private ReadOnly _processed As New HashSet(Of String)(StringComparer.Ordinal)

    Public Sub New(journalPath As String)
        If String.IsNullOrWhiteSpace(journalPath) Then Throw New ArgumentException("Chemin de journal requis.", NameOf(journalPath))
        _journalPath = journalPath
        If File.Exists(journalPath) Then
            ' Ne pas accepter un journal en cours d'ecriture ou tronque.
            Using stream As New FileStream(journalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                _processed.UnionWith(ReadEntries(stream))
            End Using
        End If
    End Sub

    Private Shared Function ReadEntries(stream As FileStream) As HashSet(Of String)
        Dim entries As New HashSet(Of String)(StringComparer.Ordinal)
        If stream.Length = 0 Then Return entries
        stream.Seek(-1, SeekOrigin.End)
        If stream.ReadByte() <> 10 Then
            Throw New InvalidDataException("Journal Gmail incomplet : derniere ligne non terminee.")
        End If
        stream.Position = 0
        Using reader As New StreamReader(stream, New UTF8Encoding(False, True), True, 1024, True)
            Dim entry = reader.ReadLine()
            While entry IsNot Nothing
                ' Les anciens journaux peuvent contenir des lignes vides.
                ' Les ignorer sans accepter de cles malformees ni de ligne tronquee.
                If entry.Length > 0 Then
                    If Not IsValidKey(entry) Then
                        Throw New InvalidDataException("Journal Gmail invalide : cle malformee.")
                    End If
                    entries.Add(entry)
                End If
                entry = reader.ReadLine()
            End While
        End Using
        Return entries
    End Function

    Public ReadOnly Property HasEntries As Boolean
        Get
            Return _processed.Count > 0
        End Get
    End Property

    Private Shared Function IsValidKey(key As String) As Boolean
        If String.IsNullOrEmpty(key) Then Return False
        Dim parts = key.Split(":"c)
        If parts.Length <> 3 OrElse parts(0).Length = 0 OrElse parts(0).Length Mod 2 <> 0 Then Return False
        For Each c As Char In parts(0)
            If Not ((c >= "0"c AndAlso c <= "9"c) OrElse (c >= "A"c AndAlso c <= "F"c)) Then Return False
        Next
        Dim validity As UInteger
        Dim uid As UInteger
        If Not UInteger.TryParse(parts(1), Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, validity) OrElse
           Not UInteger.TryParse(parts(2), Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, uid) OrElse
           validity = 0UI OrElse uid = 0UI Then Return False

        ' Une cle doit contenir un compte UTF-8 valide, pas seulement des octets hexadecimaux.
        ' Garder la compatibilite avec les cles historiques deja enregistrees.
        Try
            Dim account = New UTF8Encoding(False, True).GetString(Convert.FromHexString(parts(0)))
            Return Not String.IsNullOrWhiteSpace(account) AndAlso account.IndexOf(ChrW(0)) < 0
        Catch ex As DecoderFallbackException
            Return False
        End Try
    End Function

    Public Shared Function MakeKey(account As String, uidValidity As UInteger, uid As UInteger) As String
        If String.IsNullOrWhiteSpace(account) Then Throw New ArgumentException("Compte Gmail manquant.", NameOf(account))
        If uidValidity = 0UI OrElse uid = 0UI Then Throw New ArgumentOutOfRangeException(NameOf(uid), "UID et UIDVALIDITY doivent etre non nuls.")
        ' Account name encoded to avoid delimiters in a single-line journal.
        Return Convert.ToHexString(Encoding.UTF8.GetBytes(account.Trim().ToLowerInvariant())) & ":" &
               uidValidity.ToString(Globalization.CultureInfo.InvariantCulture) & ":" &
               uid.ToString(Globalization.CultureInfo.InvariantCulture)
    End Function

    Public Function IsProcessed(key As String) As Boolean
        Return _processed.Contains(key)
    End Function

    Public Sub MarkProcessed(key As String)
        If Not IsValidKey(key) Then Throw New ArgumentException("Cle de message invalide.", NameOf(key))
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(_journalPath))
        Directory.CreateDirectory(folder)
        ' Relire sous verrou exclusif : une autre instance a pu ecrire depuis New().
        ' Ne jamais ajouter une ligne a un journal devenu incomplet ou corrompu.
        Using stream As New FileStream(_journalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            Dim diskEntries = ReadEntries(stream)
            If Not diskEntries.Contains(key) Then
                stream.Seek(0, SeekOrigin.End)
                Using writer As New StreamWriter(stream, New UTF8Encoding(False), 1024, True)
                    writer.WriteLine(key)
                    writer.Flush()
                    stream.Flush(True)
                End Using
                diskEntries.Add(key)
            End If
            _processed.Clear()
            _processed.UnionWith(diskEntries)
        End Using
    End Sub
End Class
