Imports System.Security.Cryptography
Imports System.Text
Imports DocumentFormat.OpenXml.Drawing
Imports OpenCvSharp

Public Module UtilitairesHash

    ' --- Recadrage automatique de la zone utile d’une image ---
    Public Function RecadrerImageUtile(imagePath As String) As Mat
        Try
            ' --- Charge l'image ---
            Dim img As Mat = Cv2.ImRead(imagePath, ImreadModes.Color)
            If img.Empty() Then
                Logger.ERR($"Impossible de charger l'image : {imagePath}")
                Return Nothing
            End If

            ' --- Conversion en niveaux de gris ---
            Dim gray As New Mat()
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY)

            ' --- Floutage léger pour réduire le bruit ---
            Cv2.GaussianBlur(gray, gray, New OpenCvSharp.Size(5, 5), 0)

            ' --- Détection des contours ---
            Dim edges As New Mat()
            Cv2.Canny(gray, edges, 50, 150)

            ' --- Trouver les contours ---
            Dim contours As OpenCvSharp.Point()() = Nothing
            Dim hierarchy As OpenCvSharp.HierarchyIndex() = Nothing
            Cv2.FindContours(edges, contours, hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple)

            If contours Is Nothing OrElse contours.Length = 0 Then
                Logger.WARN($"Aucun contour trouvé dans : {imagePath}")
                Return img
            End If

            ' --- Détecte le plus grand contour (zone principale) ---
            Dim maxContour As OpenCvSharp.Point() = contours.
            OrderByDescending(Function(c) Cv2.ContourArea(c)).
            FirstOrDefault()

            If maxContour Is Nothing OrElse Cv2.ContourArea(maxContour) < 1000 Then
                Logger.WARN($"Zone utile trop petite dans : {imagePath}")
                Return img
            End If

            ' --- Calcule le rectangle englobant ---
            Dim rect As OpenCvSharp.Rect = Cv2.BoundingRect(maxContour)

            ' --- Ajoute une petite marge (évite de couper trop serré) ---
            Dim marge = 10
            rect.X = Math.Max(rect.X - marge, 0)
            rect.Y = Math.Max(rect.Y - marge, 0)
            rect.Width = Math.Min(rect.Width + 2 * marge, img.Width - rect.X)
            rect.Height = Math.Min(rect.Height + 2 * marge, img.Height - rect.Y)

            ' --- Recadre l'image ---
            Dim recadree As Mat = New Mat(img, rect).Clone()

            Logger.DBG($"Image recadrée : {imagePath} → {rect.Width}x{rect.Height}")
            Return recadree

        Catch ex As Exception
            Logger.ERR($"Erreur lors du recadrage de {imagePath} : {ex.Message}")
            Return Nothing
        End Try
    End Function


    ' --- Génération d’un hash perceptuel pour comparaison d’images ---
    Public Function HashImageSansMetadata(imagePath As String) As String
        Dim mat As Mat = RecadrerImageUtile(imagePath)
        If mat Is Nothing OrElse mat.Empty() Then Return String.Empty

        ' Redimensionnement standard (taille fixe pour comparaison)
        Dim resized As New Mat()
        Cv2.Resize(mat, resized, New Size(32, 32))

        ' Conversion en niveaux de gris
        Dim gray As New Mat()
        Cv2.CvtColor(resized, gray, ColorConversionCodes.BGR2GRAY)

        ' Calcul de la moyenne de luminosité
        Dim meanVal As Scalar = Cv2.Mean(gray)
        Dim mean As Double = meanVal.Val0

        ' Création du hash (chaîne binaire)
        Dim hashBuilder As New StringBuilder()
        For y As Integer = 0 To gray.Rows - 1
            For x As Integer = 0 To gray.Cols - 1
                Dim pixelValue = gray.Get(Of Byte)(y, x)
                hashBuilder.Append(If(pixelValue > mean, "1", "0"))
            Next
        Next

        ' Conversion en hexadécimal (lisible et compact)
        Return Convert.ToInt64(hashBuilder.ToString(), 2).ToString("X")
    End Function


    ' --- Comparaison entre deux images (distance de Hamming) ---
    Public Function SimilariteHashes(hash1 As String, hash2 As String) As Double
        If String.IsNullOrEmpty(hash1) OrElse String.IsNullOrEmpty(hash2) Then Return 0

        Dim bin1 = Convert.ToString(Convert.ToInt64(hash1, 16), 2).PadLeft(1024, "0"c)
        Dim bin2 = Convert.ToString(Convert.ToInt64(hash2, 16), 2).PadLeft(1024, "0"c)

        Dim diffCount As Integer = bin1.Zip(bin2, Function(a, b) If(a <> b, 1, 0)).Sum()
        Dim maxLen = Math.Max(bin1.Length, bin2.Length)
        Return 1 - (diffCount / maxLen)
    End Function


    ' --- Fonction utilitaire pour comparer deux fichiers image ---
    Public Function SontImagesSimilaires(path1 As String, path2 As String, Optional seuil As Double = 0.9) As Boolean
        Dim h1 = HashImageSansMetadata(path1)
        Dim h2 = HashImageSansMetadata(path2)
        Dim simil = SimilariteHashes(h1, h2)
        Return simil >= seuil
    End Function
    ' === Calcul d’un hash perceptuel basé sur la luminance moyenne ===
    Public Function CalculerHashPerceptuel(cheminFichier As String) As String
        Try
            If Not IO.File.Exists(cheminFichier) Then Return String.Empty

            ' --- Chargement en niveaux de gris ---
            Using src As Mat = Cv2.ImRead(cheminFichier, ImreadModes.Grayscale)
                If src.Empty() Then Return String.Empty

                ' --- Redimensionnement à 32x32 ---
                Using resized As New Mat()
                    Cv2.Resize(src, resized, New OpenCvSharp.Size(32, 32))

                    ' --- Conversion en double pour la DCT ---
                    Using floatMat As New Mat()
                        resized.ConvertTo(floatMat, MatType.CV_32F)

                        ' --- Transformée en cosinus discrète (DCT) ---
                        Using dctMat As New Mat()
                            Cv2.Dct(floatMat, dctMat)

                            ' --- On prend la sous-matrice 8x8 du coin supérieur gauche ---
                            Dim subDct = New Mat(dctMat, New Rect(0, 0, 8, 8))

                            ' --- Moyenne (en ignorant [0,0] qui est la moyenne globale) ---
                            Dim valeurs = New List(Of Double)
                            For y = 0 To subDct.Rows - 1
                                For x = 0 To subDct.Cols - 1
                                    If Not (x = 0 AndAlso y = 0) Then
                                        valeurs.Add(subDct.At(Of Single)(y, x))
                                    End If
                                Next
                            Next
                            Dim moyenne As Double = valeurs.Average()

                            ' --- Création du hash binaire ---
                            Dim bits As New StringBuilder()
                            For y = 0 To subDct.Rows - 1
                                For x = 0 To subDct.Cols - 1
                                    If subDct.At(Of Single)(y, x) > moyenne Then
                                        bits.Append("1")
                                    Else
                                        bits.Append("0")
                                    End If
                                Next
                            Next

                            ' --- Hachage SHA256 pour obtenir une signature compacte ---
                            Using sha As SHA256 = SHA256.Create()
                                Dim bytes = Encoding.UTF8.GetBytes(bits.ToString())
                                Dim hash = sha.ComputeHash(bytes)
                                Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
                            End Using
                        End Using
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur CalculerHashPerceptuel({cheminFichier}) : {ex.Message}")
            Return String.Empty
        End Try
    End Function
    ' NOUVELLE: surcharge qui prend un Mat déjà chargé / recadré
    Public Function CalculerHashPerceptuel(matInput As Mat) As String
        Try
            If matInput Is Nothing OrElse matInput.Empty() Then Return String.Empty

            ' Assure qu'on travaille en niveaux de gris et sur une copie
            Dim gray As Mat
            If matInput.Type() <> MatType.CV_8U Then
                gray = New Mat()
                Cv2.CvtColor(matInput, gray, ColorConversionCodes.BGR2GRAY)
            Else
                gray = matInput.Clone()
            End If

            ' Redimensionne à taille normale pour le pHash (ex: 32x32)
            Dim resized As New Mat()
            Cv2.Resize(gray, resized, New Size(32, 32))

            ' Convert to float and apply DCT
            Dim floatMat As New Mat()
            resized.ConvertTo(floatMat, MatType.CV_32F)

            Dim dctMat As New Mat()
            Cv2.Dct(floatMat, dctMat)

            ' Sous-matrice 8x8
            Dim subDct As Mat = New Mat(dctMat, New Rect(0, 0, 8, 8))

            ' Collecte valeurs (ignore [0,0])
            Dim valeurs As New List(Of Double)
            For y As Integer = 0 To subDct.Rows - 1
                For x As Integer = 0 To subDct.Cols - 1
                    If Not (x = 0 AndAlso y = 0) Then
                        valeurs.Add(CDbl(subDct.At(Of Single)(y, x)))
                    End If
                Next
            Next

            Dim moyenne As Double = If(valeurs.Count > 0, valeurs.Average(), 0)

            ' Build binary string (8x8 => 64 bits)
            Dim sb As New StringBuilder()
            For y As Integer = 0 To subDct.Rows - 1
                For x As Integer = 0 To subDct.Cols - 1
                    Dim v As Double = CDbl(subDct.At(Of Single)(y, x))
                    sb.Append(If(v > moyenne, "1"c, "0"c))
                Next
            Next

            ' Convert to sha256 for compact stable signature
            Using sha As SHA256 = SHA256.Create()
                Dim bytes = Encoding.UTF8.GetBytes(sb.ToString())
                Dim hash = sha.ComputeHash(bytes)
                Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur CalculerHashPerceptuel(Mat): {ex.Message}")
            Return String.Empty
        End Try
    End Function
End Module
