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

        Dim hash = ComputeSha256(tempPath)
        Dim originalName = Path.GetFileNameWithoutExtension(desiredPath)
        Dim extension = Path.GetExtension(desiredPath)

        ' Ne supprimer que notre propre fichier temporaire, jamais un fichier existant.
        For Each existingPath In Directory.EnumerateFiles(folder)
            If String.Equals(Path.GetFullPath(existingPath), Path.GetFullPath(tempPath), StringComparison.OrdinalIgnoreCase) Then Continue For
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
                ' Concurrent creation: if identical, do not duplicate; otherwise retry a new filename.
                If ComputeSha256(destination) = hash Then
                    File.Delete(tempPath)
                    Return Nothing
                End If
            End Try
        Next
        Throw New IOException("Impossible de trouver un nom disponible pour la piece jointe.")
    End Function

    Public Shared Function ComputeSha256(path As String) As String
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using sha = SHA256.Create()
                Return Convert.ToHexString(sha.ComputeHash(stream))
            End Using
        End Using
    End Function
End Class
