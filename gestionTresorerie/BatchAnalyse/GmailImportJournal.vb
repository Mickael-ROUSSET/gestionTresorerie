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
            ' Le partage en lecture seule exclut un redacteur simultane.
            Using stream As New FileStream(journalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                If stream.Length > 0 Then
                    stream.Seek(-1, SeekOrigin.End)
                    If stream.ReadByte() <> 10 Then
                        Throw New InvalidDataException("Journal Gmail incomplet : derniere ligne non terminee.")
                    End If
                    stream.Position = 0
                    Using reader As New StreamReader(stream, New UTF8Encoding(False, True))
                        Dim entry = reader.ReadLine()
                        While entry IsNot Nothing
                            If Not IsValidKey(entry) Then
                                Throw New InvalidDataException("Journal Gmail invalide : cle malformee.")
                            End If
                            _processed.Add(entry)
                            entry = reader.ReadLine()
                        End While
                    End Using
                End If
            End Using
        End If
    End Sub

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
        Return UInteger.TryParse(parts(1), Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, validity) AndAlso
               UInteger.TryParse(parts(2), Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, uid) AndAlso
               validity > 0UI AndAlso uid > 0UI
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
        If _processed.Contains(key) Then Return
        Dim folder = Path.GetDirectoryName(Path.GetFullPath(_journalPath))
        Directory.CreateDirectory(folder)
        Using stream As New FileStream(_journalPath, FileMode.Append, FileAccess.Write, FileShare.Read)
            Using writer As New StreamWriter(stream, New UTF8Encoding(False))
                writer.WriteLine(key)
                writer.Flush()
                stream.Flush(True)
            End Using
        End Using
        _processed.Add(key)
    End Sub
End Class
