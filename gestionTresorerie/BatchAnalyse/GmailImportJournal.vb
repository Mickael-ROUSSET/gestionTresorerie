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
            For Each entry In File.ReadLines(journalPath)
                If entry.Length > 0 Then _processed.Add(entry)
            Next
        End If
    End Sub

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
        If String.IsNullOrWhiteSpace(key) OrElse key.Contains(vbLf) OrElse key.Contains(vbCr) Then Throw New ArgumentException("Cle de message invalide.", NameOf(key))
        If _processed.Contains(key) Then Return
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(_journalPath))
        Directory.CreateDirectory(folder)
        ' Recheck on disk while holding an exclusive lock: another journal instance
        ' may have recorded the same message after this instance was constructed.
        Using stream As New FileStream(_journalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            Using reader As New StreamReader(stream, Encoding.UTF8, True, 1024, True)
                While Not reader.EndOfStream
                    If String.Equals(reader.ReadLine(), key, StringComparison.Ordinal) Then
                        _processed.Add(key)
                        Return
                    End If
                End While
            End Using
            stream.Seek(0, SeekOrigin.End)
            Using writer As New StreamWriter(stream, New UTF8Encoding(False), 1024, True)
                writer.WriteLine(key)
                writer.Flush()
            End Using
            stream.Flush(True)
        End Using
        _processed.Add(key)
    End Sub
End Class
