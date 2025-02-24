
Imports System.Windows.Forms.DataVisualization.Charting
Public Class frmHistogramme
    Public Sub creeChart(ByVal titre As String, ByVal valeurs() As Decimal, ByVal legende() As String)
        'https://plasserre.developpez.com/cours/chart/#LIII-C-1
        Dim i As Integer

        'Supprimer tous les points
        ChartBilan.Series("Series1").Points.Clear()

        'ChartBilan.Titles.Add(titre)
        ChartBilan.ChartAreas(0).AxisX.Title = "Catégorie"
        ChartBilan.ChartAreas(0).AxisY.Title = "Montant"
        ' Set the Series style 
        ChartBilan.ChartAreas(0).BorderColor = Color.Violet
        'ChartBilan.Series.Style.ShadowInterior = New Syncfusion.Drawing.BrushInfo(Color.White)
        'ChartBilan.Series.Style.ShadowOffset = New Size(3, 3)
        ChartBilan.Palette = ChartColorPalette.Fire
        ChartBilan.Series("Series1").MarkerStyle = MarkerStyle.Diamond
        ' Met un  Gradient sur la couleur du fond
        ChartBilan.BackGradientStyle = GradientStyle.DiagonalRight
        'Le LightStyle modifie la lumière : la couleur des côtés et du dessus des colonnes change  
        ChartBilan.ChartAreas("ChartArea1").Area3DStyle.LightStyle = LightStyle.Realistic
        ChartBilan.Series("Series1").BorderColor = Color.Bisque
        For i = 0 To UBound(valeurs) - 1
            'Ne marche pas pour mettre le libellé
            'sLegende = Str(i) & "_" & Strings.Replace(legende(i), " ", "")
            'ChartBilan.Series("Series1").Points.AddXY(sLegende, valeurs(i)
            ChartBilan.Series("Series1").Points.AddXY((i), valeurs(i))
            ChartBilan.Series("Series1").Points(i).Color = arrColor(i Mod (UBound(arrColor) - 1))
        Next
        'On indique d'afficher ces Series sur le ChartArea1
        ChartBilan.Series("Series1").ChartArea = "ChartArea1"
    End Sub

    'https://codes-sources.commentcamarche.net/source/53025-afficher-un-histogramme-personnalise

    ' ===== CDC : Afficher un Histogramme % (ou plusieurs dans la même Form) 
    '             indépendant des Données, paramétrable en position / dimensions / marges / couleurs etc.
    '             utilisant seulement des Controles PictureBox et Labels 
    ' ===== VERSION 1 = Histogramme(s) % via un Array 

    Private picBRectangles As PictureBox
    Private lblValeurs As Label
    Private arrColor() As Color = {Color.Tomato, Color.DarkSeaGreen, Color.CornflowerBlue,
                                   Color.Orchid, Color.OliveDrab, Color.SlateBlue, Color.Goldenrod}
    'Pour test
    Private arrayNavets() As Integer = {16, 85, 0, 76, 18, 100, 53}
    Private arrayCornichons() As Integer = {6, 25, 36, 87, 8, 25, 35}
    Private arrayPersil() As Integer = {76, 5, 63, 57, 2, 95, 50}
    Private arrayNom() As String = {"Dijon", "Nantes", "Nice", "Paris", "Lyon", "Cherbourg", "Pau"}
    'Pour test
    Private maFont As New Font("Verdana", 7)
    Private maFontBold As New Font("Verdana", 7, FontStyle.Bold)


    'TODO à sortir dans un module de copie d'écran si cela marche
    'Private Declare Function BitBlt Lib "GDI32" (ByVal hDestDC As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hSrcDC As IntPtr, ByVal SrcX As Integer, ByVal SrcY As Integer, ByVal Rop As Integer) As Integer
    'Private Declare Function GetForegroundWindow Lib "user32" () As IntPtr
    'Private Declare Function GetWindowRect Lib "user32.dll" (ByVal hWnd As IntPtr, ByRef lpRect As Rectangle) As Integer
    'Private Declare Function GetDesktopWindow Lib "user32" () As IntPtr
    'Capture tout l'écran
    'Public Shared Function ShotScreen() As Bitmap
    '    Try
    '        Dim DesktopRect As Rectangle = Screen.GetBounds(New Point(0, 0)) 'obtient la taille du bureau sous forme de rectangle dans DesktopRect
    '        Return ShotScreenPart(DesktopRect.Width, DesktopRect.Height) 'appele la fonction ShotScreenPart avec les dimensions du bureau. 
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '    End Try
    'End Function
    'Capture la fenetre active
    'Public Function ShotActiveWin() As Bitmap
    '    Dim WinRect As Rectangle
    '    Try
    '        If GetWindowRect(GetForegroundWindow, WinRect) Then 'obtient la taille et la position de la fenetre active sous forme de rectangle (WinRect)
    '            Return ShotScreenPart(WinRect.Size.Width - WinRect.Left, WinRect.Size.Height - WinRect.Top, WinRect.Left, WinRect.Top)  'appele la fonction ShotLoc avec les dimensions et la position de la fenetre. 
    '        End If
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '    End Try
    'End Function
    'Capture une partie de l'ecran, defini par les deux variable width et height (dimensions du rectangle), et des valeur optionels X et Y (base du rectangle)
    'Public Shared Function ShotScreenPart(ByVal nwidth As Integer, ByVal nheight As Integer, Optional ByVal x As Integer = 0, Optional ByVal y As Integer = 0) As Bitmap
    '    Dim resultBmp As New Bitmap(nwidth, nheight) 'crée l'objet bitmap cible
    '    Dim SrcGraph As Graphics = Graphics.FromHwnd(GetDesktopWindow) 'crée l'objet "graphics" SelGraph a partir du handdle du bureau
    '    Dim BmpGraph As Graphics = Graphics.FromImage(resultBmp) 'crée un objet graphics à partir du bitmap
    '    Dim bmpDC As IntPtr = BmpGraph.GetHdc() 'obtient le device context du bitmap
    '    Dim hDC As IntPtr = SrcGraph.GetHdc() 'obtient le device context du bureau
    '    BitBlt(bmpDC, 0, 0, nwidth, nheight, hDC, x, y, &HCC0020) '"bit-block transfer" : copie chaque bits affichés dans le device context hDC dans le device context du bitmap 
    '    SrcGraph.ReleaseHdc(hDC) 'relache le device context du bureau
    '    BmpGraph.ReleaseHdc(bmpDC) 'relache le device context du bitmap
    '    SrcGraph.Dispose()
    '    BmpGraph.Dispose() 'libere toutes les ressources crées par l'objet (useless?)
    '    Return resultBmp
    'End Function
    'Private Function MaxTableau(tab() As Decimal) As Decimal
    '    'TODO gérer les valeurs négatives
    '    Dim i As Integer
    '    Dim maxTemp As Decimal = 0
    '    For i = 0 To UBound(tab) - 1
    '        If tab(i) > maxTemp Then
    '            maxTemp = tab(i)
    '        End If
    '    Next
    '    Return maxTemp
    'End Function

End Class
