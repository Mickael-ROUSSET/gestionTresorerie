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

    Public Shared Function PdfPageCount(filePath As String) As Integer
        Using stream = OpenRead(filePath)
            Return PDFtoImage.Conversion.GetPageCount(stream, leaveOpen:=True)
        End Using
    End Function

    Public Shared Function RenderPdfPage(filePath As String, pageIndex As Integer) As Bitmap
        Return RenderPdfPage(filePath, pageIndex, 1800)
    End Function

    Friend Shared Function RenderPdfPage(filePath As String, pageIndex As Integer, maxDimension As Integer) As Bitmap
        Using stream = OpenRead(filePath), png As New MemoryStream()
            Dim page As New System.Index(pageIndex)
            Dim size = PDFtoImage.Conversion.GetPageSize(stream, page, leaveOpen:=True)
            If Not Single.IsFinite(size.Width) OrElse Not Single.IsFinite(size.Height) OrElse
               size.Width <= 0 OrElse size.Height <= 0 Then
                Throw New InvalidDataException("Dimensions de page PDF invalides.")
            End If

            Dim scale = CDbl(maxDimension) / Math.Max(size.Width, size.Height)
            Dim options As New PDFtoImage.RenderOptions(
                Width:=Math.Max(1, CInt(size.Width * scale)),
                Height:=Math.Max(1, CInt(size.Height * scale)))
            stream.Position = 0
            PDFtoImage.Conversion.SavePng(png, stream, page, leaveOpen:=True, options:=options)
            png.Position = 0
            Using rendered = Image.FromStream(png)
                Return New Bitmap(rendered)
            End Using
        End Using
    End Function

    Public Shared Function ReadPdfText(filePath As String) As String
        Dim hasText As Boolean
        Dim truncated As Boolean
        Return ReadPdfText(filePath, hasText, truncated)
    End Function

    Friend Shared Function ReadPdfText(filePath As String, ByRef hasText As Boolean, ByRef truncated As Boolean) As String
        hasText = False
        truncated = False
        Using stream = OpenRead(filePath), pdf = PdfDocument.Open(stream)
            Dim result As New StringBuilder
            Dim pages = Math.Min(pdf.NumberOfPages, MaxPdfPages)
            truncated = pages < pdf.NumberOfPages
            For pageNumber = 1 To pages
                Dim pageText = ContentOrderTextExtractor.GetText(pdf.GetPage(pageNumber))
                If String.IsNullOrWhiteSpace(pageText) Then Continue For

                hasText = True
                result.AppendLine($"Page {pageNumber}")
                Dim remaining = MaxTextLength - result.Length
                If pageText.Length >= remaining Then
                    result.Append(pageText, 0, remaining)
                    truncated = True
                    result.AppendLine().AppendLine("[Aperçu du texte limité.]")
                    Return result.ToString()
                End If
                result.AppendLine(pageText).AppendLine()
                If result.Length >= MaxTextLength - 30 Then
                    truncated = True
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
