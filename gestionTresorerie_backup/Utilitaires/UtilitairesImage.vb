'Imports Emgu.CV
'Imports Emgu.CV.Structure
'Imports Emgu.CV.CvEnum
'Imports System.Drawing

'Public Class UtilitairesImage

'    ''' <summary>
'    ''' Rogne automatiquement une image scannée pour ne garder que la zone utile (contours détectés).
'    ''' Gère le fond gris, les bords, les ombres légères et redimensionne la sortie.
'    ''' </summary>
'    Public Shared Function RognerZoneUtileAvancee(imagePath As String, Optional tailleStandard As Integer = 256) As Bitmap
'        ' Charger l’image d’origine
'        Dim src As New Mat(imagePath, ImreadModes.Color)

'        ' Convertir en niveaux de gris
'        Dim gray As New Mat()
'        CvInvoke.CvtColor(src, gray, ColorConversion.Bgr2Gray)

'        ' Flouter légèrement pour éliminer le bruit
'        CvInvoke.GaussianBlur(gray, gray, New Size(5, 5), 0)

'        ' Binarisation adaptative (gère les fonds non uniformes)
'        Dim thresh As New Mat()
'        CvInvoke.AdaptiveThreshold(gray, thresh, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 15, 10)

'        ' Trouver les contours
'        Dim contours As New VectorOfVectorOfPoint()
'        CvInvoke.FindContours(thresh, contours, Nothing, RetrType.External, ChainApproxMethod.ChainApproxSimple)

'        If contours.Size = 0 Then
'            ' Aucun contour trouvé → on retourne l’image originale
'            Return src.ToBitmap()
'        End If

'        ' Trouver le plus grand contour (zone utile)
'        Dim maxArea As Double = 0
'        Dim bestRect As Rectangle = Rectangle.Empty

'        For i = 0 To contours.Size - 1
'            Dim rect = CvInvoke.BoundingRectangle(contours(i))
'            Dim area = rect.Width * rect.Height
'            If area > maxArea Then
'                maxArea = area
'                bestRect = rect
'            End If
'        Next

'        ' Rogner la zone utile
'        Dim cropped = New Mat(src, bestRect)

'        ' Redimensionner pour comparaison uniforme
'        Dim resized As New Mat()
'        CvInvoke.Resize(cropped, resized, New Size(tailleStandard, tailleStandard))

'        Return resized.ToBitmap()
'    End Function

'End Class
