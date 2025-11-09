Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports UglyToad.PdfPig

Module UtilitairesHash

    ' --- Hash MD5 classique pour tout type de fichier ---
    Public Function CalculerHashMD5(cheminFichier As String) As String
        Using md5 As MD5 = MD5.Create()
            Using stream As FileStream = File.OpenRead(cheminFichier)
                Dim hashBytes() As Byte = md5.ComputeHash(stream)
                Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Calcule un hash d’image indépendant des métadonnées et du format (basé sur les pixels bruts).
    ''' </summary>
    Public Function HashImageSansMetadata(imagePath As String) As String
        Try
            Using img As Image = Image.FromFile(imagePath)
                ' --- Normaliser : convertir en Bitmap 32bpp sans métadonnées ---
                Using bmp As New Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb)
                    Using g As Graphics = Graphics.FromImage(bmp)
                        g.DrawImage(img, New Rectangle(0, 0, img.Width, img.Height))
                    End Using

                    ' --- Extraire les données de pixels ---
                    Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
                    Dim bmpData As BitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat)
                    Dim bytes(bmpData.Stride * bmp.Height - 1) As Byte
                    Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length)
                    bmp.UnlockBits(bmpData)

                    ' --- Calculer le hash des pixels ---
                    Using md5 As MD5 = MD5.Create()
                        Dim hashBytes As Byte() = md5.ComputeHash(bytes)
                        Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur dans HashImageSansMetadata({imagePath}) : {ex.Message}")
            Return String.Empty
        End Try
    End Function

    ' --- Hash pour PDF basé sur le texte uniquement (PdfPig) ---
    Public Function HashPdfTexte(cheminPdf As String) As String
        Dim texte As New StringBuilder()

        Try
            Using document = PdfDocument.Open(cheminPdf)
                For Each page In document.GetPages()
                    texte.Append(page.Text)
                Next
            End Using

            Using md5 As MD5 = MD5.Create()
                Dim hashBytes() As Byte = md5.ComputeHash(Encoding.UTF8.GetBytes(texte.ToString()))
                Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur lors du hash PDF : {cheminPdf} → {ex.Message}")
            Return String.Empty
        End Try
    End Function

    ' --- Hash intelligent selon le type de fichier ---
    Public Function CalculerHashIntelligent(cheminFichier As String) As String
        Select Case Path.GetExtension(cheminFichier).ToLower()
            Case ".jpg", ".jpeg", ".png", ".bmp", ".gif"
                Return HashImageSansMetadata(cheminFichier)
            Case ".pdf"
                Return HashPdfTexte(cheminFichier)
            Case Else
                Return CalculerHashMD5(cheminFichier)
        End Select
    End Function
    ''' <summary>
    ''' Calcule un hash perceptuel (pHash simplifié) d'une image.
    ''' Résiste aux changements de format, compression, redimensionnement léger.
    ''' </summary>
    ''' <param name="imagePath">Chemin de l'image à analyser</param>
    ''' <returns>Chaîne hexadécimale représentant le hash</returns>
    Public Function CalculerHashPerceptuel(imagePath As String) As String
        Try
            Const SIZE As Integer = 8 ' Taille réduite (8x8 pixels)
            Using img As Image = Image.FromFile(imagePath)
                ' --- Étape 1 : redimensionner et convertir en niveaux de gris ---
                Using smallBmp As New Bitmap(SIZE, SIZE)
                    Using g As Graphics = Graphics.FromImage(smallBmp)
                        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                        g.DrawImage(img, 0, 0, SIZE, SIZE)
                    End Using

                    ' --- Étape 2 : extraire la luminance moyenne ---
                    Dim grayValues(SIZE * SIZE - 1) As Double
                    Dim sum As Double = 0
                    For y As Integer = 0 To SIZE - 1
                        For x As Integer = 0 To SIZE - 1
                            Dim c As Color = smallBmp.GetPixel(x, y)
                            Dim gray = (c.R * 0.299 + c.G * 0.587 + c.B * 0.114)
                            grayValues(y * SIZE + x) = gray
                            sum += gray
                        Next
                    Next
                    Dim avgGray As Double = sum / (SIZE * SIZE)

                    ' --- Étape 3 : construire le hash binaire ---
                    Dim sb As New StringBuilder()
                    For Each gray In grayValues
                        sb.Append(If(gray >= avgGray, "1", "0"))
                    Next

                    ' --- Étape 4 : convertir en hexadécimal pour stockage ---
                    Dim binaryStr As String = sb.ToString()
                    Dim hashBytes(binaryStr.Length \ 8 - 1) As Byte
                    For i As Integer = 0 To hashBytes.Length - 1
                        hashBytes(i) = Convert.ToByte(binaryStr.Substring(i * 8, 8), 2)
                    Next
                    Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
                End Using
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur dans PerceptualHash({imagePath}) : {ex.Message}")
            Return String.Empty
        End Try
    End Function


    ''' <summary>
    ''' Compare deux hashs perceptuels et renvoie une distance de similarité (0 = identique, 64 = très différent).
    ''' </summary>
    Public Function HammingDistance(hash1 As String, hash2 As String) As Integer
        If String.IsNullOrEmpty(hash1) OrElse String.IsNullOrEmpty(hash2) Then Return Integer.MaxValue
        If hash1.Length <> hash2.Length Then Return Integer.MaxValue

        Dim distance As Integer = 0
        For i As Integer = 0 To hash1.Length - 1
            If hash1(i) <> hash2(i) Then distance += 1
        Next
        Return distance
    End Function

End Module
