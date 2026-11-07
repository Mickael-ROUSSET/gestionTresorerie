Option Strict On

Imports System.Drawing
Imports System.IO
Imports System.Text
Imports UglyToad.PdfPig
Imports UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor

Public NotInheritable Class DocumentFilePreview
    Private Const MaxBytes As Long = 25L * 1024 * 1024
    Private Const MaxPdfPages As Integer = 10
    Private Const MaxTextLength As Integer = 50000
    Private Const MaxImagePixels As Long = 25000000

    Public Shared Function Supports(filePath As String) As Boolean
        Select Case Path.GetExtension(filePath).ToLowerInvariant()
            Case ".pdf", ".jpg", ".jpeg", ".png", ".tif", ".tiff"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Shared Function ReadPdfText(filePath As String) As String
        Using stream = OpenRead(filePath), pdf = PdfDocument.Open(stream)
            Dim result As New StringBuilder
            Dim pages = Math.Min(pdf.NumberOfPages, MaxPdfPages)
            For pageNumber = 1 To pages
                Dim pageText = ContentOrderTextExtractor.GetText(pdf.GetPage(pageNumber))
                If String.IsNullOrWhiteSpace(pageText) Then Continue For

                result.AppendLine($"Page {pageNumber}")
                Dim remaining = MaxTextLength - result.Length
                If pageText.Length >= remaining Then
                    result.Append(pageText, 0, remaining)
                    result.AppendLine().AppendLine("[Aperçu du texte limité.]")
                    Return result.ToString()
                End If
                result.AppendLine(pageText).AppendLine()
                If result.Length >= MaxTextLength - 30 Then
                    result.AppendLine("[Aperçu du texte limité.]")
                    Return result.ToString()
                End If
            Next

            If result.Length = 0 Then result.AppendLine("Aucun texte détecté dans les pages examinées. Ce PDF peut nécessiter une reconnaissance optique (OCR).")
            If pages < pdf.NumberOfPages Then result.AppendLine($"Aperçu limité aux {pages} premières pages.")
            Return result.ToString().TrimEnd()
        End Using
    End Function

    Public Shared Function ReadImage(filePath As String) As Bitmap
        Using stream = OpenRead(filePath), source = Image.FromStream(stream)
            If CLng(source.Width) * source.Height > MaxImagePixels Then
                Throw New InvalidDataException("L’image dépasse la taille maximale de l’aperçu (25 mégapixels).")
            End If
            ' Une copie permet de fermer le flux et de libérer le fichier d'origine.
            Return New Bitmap(source)
        End Using
    End Function

    Private Shared Function OpenRead(filePath As String) As FileStream
        Dim stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        If stream.Length <= MaxBytes Then Return stream
        stream.Dispose()
        Throw New InvalidDataException("Le fichier dépasse la taille maximale de l’aperçu (25 Mio).")
    End Function
End Class
