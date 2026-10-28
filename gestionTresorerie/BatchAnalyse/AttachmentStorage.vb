Imports System.IO
Imports System.Security.Cryptography

''' <summary>
''' Stockage sans ecrasement ni doublon de contenu dans un dossier de destination.
''' Le fichier temporaire doit etre dans le meme dossier pour que Move reste atomique.
''' </summary>
Public NotInheritable Class AttachmentStorage
    Private Sub New()
    End Sub

    ''' <returns>Chemin final, ou Nothing si le contenu est deja present.</returns>
    Public Shared Function StoreTemporaryFile(tempPath As String, desiredPath As String) As String
        If String.IsNullOrWhiteSpace(tempPath) Then Throw New ArgumentException("Chemin temporaire manquant.", NameOf(tempPath))
        If String.IsNullOrWhiteSpace(desiredPath) Then Throw New ArgumentException("Destination manquante.", NameOf(desiredPath))
        Dim folder = Path.GetDirectoryName(desiredPath)
        If Not String.Equals(Path.GetFullPath(Path.GetDirectoryName(tempPath)), Path.GetFullPath(folder), StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException("Le fichier temporaire doit etre dans le dossier cible.", NameOf(tempPath))
        End If

        ' Verrou inter-processus sur le dossier : la comparaison SHA-256 et le
        ' deplacement doivent former une seule operation pour eviter les doublons
        ' lorsque deux imports traitent simultanement la meme piece jointe.
        Dim lockPath = Path.Combine(folder, ".gmail-storage.lock")
        Dim storageLock As FileStream = Nothing
        For attempt As Integer = 0 To 199
            Try
                storageLock = New FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
                Exit For
            Catch ex As IOException
                If attempt = 199 Then Throw New IOException("Verrou de stockage Gmail indisponible apres 5 secondes.", ex)
                System.Threading.Thread.Sleep(25)
            End Try
        Next

        Using storageLock
            Dim hash = ComputeSha256(tempPath)
            Dim originalName = Path.GetFileNameWithoutExtension(desiredPath)
            Dim extension = Path.GetExtension(desiredPath)

            ' Ne supprimer que notre propre fichier temporaire, jamais un fichier existant.
            For Each existingPath In Directory.EnumerateFiles(folder)
                If String.Equals(Path.GetFullPath(existingPath), Path.GetFullPath(tempPath), StringComparison.OrdinalIgnoreCase) Then Continue For
                If String.Equals(Path.GetFileName(existingPath), ".gmail-storage.lock", StringComparison.OrdinalIgnoreCase) Then Continue For
                If Path.GetFileName(existingPath).StartsWith(".gmail-", StringComparison.OrdinalIgnoreCase) AndAlso
                   existingPath.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) Then Continue For
                If ComputeSha256(existingPath) = hash Then
                    File.Delete(tempPath)
                    Return Nothing
                End If
            Next

            For attempt As Integer = 0 To 19
                Dim destination = If(attempt = 0, desiredPath,
                                     Path.Combine(folder, originalName & "_" & Guid.NewGuid().ToString("N") & extension))
                Try
                    File.Move(tempPath, destination)
                    Return destination
                Catch ex As IOException When File.Exists(destination)
                    ' Un fichier de meme nom preexiste : conserver les deux contenus distincts.
                    If ComputeSha256(destination) = hash Then
                        File.Delete(tempPath)
                        Return Nothing
                    End If
                End Try
            Next
            Throw New IOException("Impossible de trouver un nom disponible pour la piece jointe.")
        End Using
    End Function

    Public Shared Function ComputeSha256(path As String) As String
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using sha = SHA256.Create()
                Return Convert.ToHexString(sha.ComputeHash(stream))
            End Using
        End Using
    End Function
End Class
