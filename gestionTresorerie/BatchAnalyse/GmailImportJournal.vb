Imports System.IO
Imports System.Text

''' <summary>
''' Journal des messages dont toutes les pieces jointes ont ete sauvegardees.
''' Une ligne est ajoutee seulement apres succes complet; reprise idempotente.
''' </summary>
Public NotInheritable Class GmailImportJournal
    Private ReadOnly _journalPath As String
    Private ReadOnly _processed As New HashSet(Of String)(StringComparer.Ordinal)
    Private ReadOnly _cacheLock As New Object()
    Private _cachedLength As Long = -1
    Private _cachedWriteUtc As DateTime = DateTime.MinValue

    Public Sub New(journalPath As String)
        If String.IsNullOrWhiteSpace(journalPath) Then Throw New ArgumentException("Chemin de journal requis.", NameOf(journalPath))
        _journalPath = journalPath
        RefreshIfChanged()
    End Sub

    Public Shared Function MakeKey(account As String, uidValidity As UInteger, uid As UInteger) As String
        If String.IsNullOrWhiteSpace(account) Then Throw New ArgumentException("Compte Gmail manquant.", NameOf(account))
        If uidValidity = 0UI OrElse uid = 0UI Then Throw New ArgumentOutOfRangeException(NameOf(uid), "UID et UIDVALIDITY doivent etre non nuls.")
        ' Account name encoded to avoid delimiters in a single-line journal.
        Return Convert.ToHexString(Encoding.UTF8.GetBytes(account.Trim().ToLowerInvariant())) & ":" &
               uidValidity.ToString(Globalization.CultureInfo.InvariantCulture) & ":" &
               uid.ToString(Globalization.CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' Refreshes a stale snapshot when another instance updates the journal.
    ''' Unchanged files retain O(1) membership checks, including for large journals.
    ''' </summary>
    Private Sub RefreshIfChanged()
        SyncLock _cacheLock
            If Not File.Exists(_journalPath) Then
                _processed.Clear()
                _cachedLength = -1
                _cachedWriteUtc = DateTime.MinValue
                Return
            End If

            Dim info As New FileInfo(_journalPath)
            ' Capture metadata BEFORE reading: a concurrent later write must
            ' invalidate the snapshot on the next check, not be hidden by it.
            Dim currentLength = info.Length
            Dim currentWriteUtc = info.LastWriteTimeUtc
            If currentLength = _cachedLength AndAlso currentWriteUtc = _cachedWriteUtc Then Return

            Dim refreshed As New HashSet(Of String)(StringComparer.Ordinal)
            ' Writers use FileShare.None; never cache a partially written journal.
            Using stream As FileStream = OpenJournalWithRetry(_journalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Using reader As New StreamReader(stream, Encoding.UTF8, True)
                    While Not reader.EndOfStream
                        Dim entry = reader.ReadLine()
                        If entry.Length > 0 Then refreshed.Add(entry)
                    End While
                End Using
            End Using

            _processed.Clear()
            For Each entry In refreshed
                _processed.Add(entry)
            Next
            _cachedLength = currentLength
            _cachedWriteUtc = currentWriteUtc
        End SyncLock
    End Sub

    Public Function IsProcessed(key As String) As Boolean
        RefreshIfChanged()
        SyncLock _cacheLock
            Return _processed.Contains(key)
        End SyncLock
    End Function

    Private Shared Function OpenJournalWithRetry(path As String, mode As FileMode, access As FileAccess, share As FileShare) As FileStream
        ' Another process may briefly hold the journal exclusively. Retry only
        ' Windows sharing/lock violations; other I/O failures remain visible.
        For attempt As Integer = 1 To 20
            Try
                Return New FileStream(path, mode, access, share)
            Catch ex As IOException
                Dim win32Code = ex.HResult And &HFFFF
                If win32Code <> 32 AndAlso win32Code <> 33 Then Throw
                If attempt = 20 Then Throw
                Threading.Thread.Sleep(50)
            End Try
        Next
        Throw New IOException("Impossible d'ouvrir le journal.")
    End Function

    Public Sub MarkProcessed(key As String)
        If String.IsNullOrWhiteSpace(key) OrElse key.Contains(vbLf) OrElse key.Contains(vbCr) Then Throw New ArgumentException("Cle de message invalide.", NameOf(key))
        ' Always recheck the durable journal: the file may have been removed or
        ' replaced after this instance cached a key.
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(_journalPath))
        Directory.CreateDirectory(folder)
        ' Recheck on disk while holding an exclusive lock: another journal instance
        ' may have recorded the same message after this instance was constructed.
        Using stream As FileStream = OpenJournalWithRetry(_journalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            Using reader As New StreamReader(stream, Encoding.UTF8, True, 1024, True)
                While Not reader.EndOfStream
                    If String.Equals(reader.ReadLine(), key, StringComparison.Ordinal) Then
                        _processed.Add(key)
                        Return
                    End If
                End While
            End Using
            ' A truncated/imported journal may lack its final line terminator.
            ' Keep the previous key separate rather than concatenating two keys.
            Dim needsSeparator As Boolean = False
            If stream.Length > 0 Then
                stream.Seek(-1, SeekOrigin.End)
                Dim lastByte As Integer = stream.ReadByte()
                needsSeparator = (lastByte <> 10 AndAlso lastByte <> 13)
            End If
            stream.Seek(0, SeekOrigin.End)
            Using writer As New StreamWriter(stream, New UTF8Encoding(False), 1024, True)
                If needsSeparator Then writer.WriteLine()
                writer.WriteLine(key)
                writer.Flush()
            End Using
            stream.Flush(True)
        End Using
        SyncLock _cacheLock
            _processed.Add(key)
        End SyncLock
    End Sub
End Class
