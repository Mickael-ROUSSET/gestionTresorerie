Option Strict On

Imports System
Imports System.Diagnostics
Imports System.IO

' OCR local facultatif : aucun téléchargement, réseau ni service payant.
' Le chemin de l'exécutable doit être explicitement fourni par l'appelant.
Public NotInheritable Class OfflineImageOcr
    Private Sub New()
    End Sub

    Public Shared Function Recognize(imagePath As String, tesseractExe As String,
                                     Optional language As String = "fra",
                                     Optional timeoutMilliseconds As Integer = 30000) As String
        If String.IsNullOrWhiteSpace(imagePath) Then Throw New ArgumentException("Image manquante.", NameOf(imagePath))
        If String.IsNullOrWhiteSpace(tesseractExe) Then Throw New ArgumentException("Moteur OCR local non configuré.", NameOf(tesseractExe))
        If timeoutMilliseconds < 1000 OrElse timeoutMilliseconds > 120000 Then
            Throw New ArgumentOutOfRangeException(NameOf(timeoutMilliseconds))
        End If
        If language <> "fra" AndAlso language <> "eng" Then Throw New ArgumentException("Langue OCR non autorisée.", NameOf(language))
        If Not Path.IsPathFullyQualified(tesseractExe) OrElse Not File.Exists(tesseractExe) Then
            Throw New FileNotFoundException("Exécutable OCR local introuvable.", tesseractExe)
        End If
        If Not DocumentFilePreview.Supports(imagePath) OrElse
           String.Equals(Path.GetExtension(imagePath), ".pdf", StringComparison.OrdinalIgnoreCase) Then
            Throw New NotSupportedException("Seules les images JPEG, PNG et TIFF sont acceptées pour cette étape.")
        End If

        ' Réutilise les contrôles de taille et de décodage existants.
        Using preview = DocumentFilePreview.ReadImage(imagePath)
            If preview.Width <= 0 OrElse preview.Height <= 0 Then Throw New InvalidDataException("Image vide.")
        End Using

        Dim start As New ProcessStartInfo(tesseractExe) With {
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = False
        }
        start.ArgumentList.Add(Path.GetFullPath(imagePath))
        start.ArgumentList.Add("stdout")
        start.ArgumentList.Add("-l")
        start.ArgumentList.Add(language)

        Using process = Process.Start(start)
            If process Is Nothing Then Throw New InvalidOperationException("Démarrage OCR impossible.")
            Try
                Dim output = process.StandardOutput.ReadToEndAsync()
                If Not process.WaitForExit(timeoutMilliseconds) Then
                    process.Kill(entireProcessTree:=True)
                    Throw New TimeoutException("Délai OCR local dépassé.")
                End If
                If process.ExitCode <> 0 Then Throw New InvalidDataException("Le moteur OCR local a échoué.")
                Return output.GetAwaiter().GetResult()
            Finally
                If Not process.HasExited Then process.Kill(entireProcessTree:=True)
            End Try
        End Using
    End Function
End Class
