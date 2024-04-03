
Imports System.Windows.Forms.DataVisualization.Charting
Imports DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing
Public Class frmHistogramme
    Public Sub creeChart(ByVal titre As String, ByVal valeurs() As Decimal, ByVal legende() As String)
        'https://plasserre.developpez.com/cours/chart/#LIII-C-1
        Dim i As Integer

        'Supprimer tous les points
        ChartBilan.Series("Series1").Points.Clear()

        'ChartBilan.Titles.Add(titre)
        ChartBilan.ChartAreas(0).AxisX.Title = "Catégorie"
        ChartBilan.ChartAreas(0).AxisY.Title = "Montant"
        For i = 0 To UBound(valeurs) - 1
            ChartBilan.Series("Series1").Points.AddXY((i), valeurs(i))
        Next
        'ChartBilan.Series("Series1").IsXValueIndexed = False
        'ChartBilan.ChartAreas("ChartArea1").RecalculateAxesScale()
        'On indique d'afficher ces Series sur le ChartArea1
        ChartBilan.Series("Series1").ChartArea = "ChartArea1"
        'ChartBilan.BringToFront()
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
    'Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    '    Dim marge As Integer = 20
    '    Dim cpt_Right As Integer = Histo_PourCent("% Navets / Récolte Locale 1902", arrayNavets, arrayNom, marge, 40, 300, 30, 10)
    '    cpt_Right = Histo_PourCent("% Radis / Récolte Locale 1902", arrayCornichons, arrayNom, marge, cpt_Right + marge, 300, 20)
    '    cpt_Right = Histo_PourCent("% Persil / Récolte Locale 1902", arrayPersil, arrayNom, marge, cpt_Right + marge, 200, 30) ' etc
    '    'Width = cpt_Right + marge

    'End Sub
    Public Function Histogramme(ByVal _titre As String, ByVal _valeurs() As Decimal, ByVal _legende() As String,
                                  ByVal _top As Integer, ByVal _left As Integer,
                                  ByVal _hauteur As Integer, ByVal _largeurBarre As Integer,
                                  Optional ByVal _interval As Integer = 0) As PictureBox
        'Contrôles sur le nb de valeurs
        Dim nbBarres As Integer = _valeurs.Length : If nbBarres < 0 Then Return Nothing
        If nbBarres > _legende.Length Then Return Nothing
        'Largeur de l'histogramme
        Dim ctl_Largeur As Integer = (nbBarres * _largeurBarre) + ((nbBarres - 1) * _interval)
        'Définition des marges gauche et bas
        Dim cpt_Left As Integer = IIf(_left < 10, 10, _left)
        Dim ctl_Bas As Integer = IIf(_top < 10, 10, _top) + IIf(_hauteur < 20, 20, _hauteur)
        Dim cpt_Hauteur As Integer = 0                          ' compteur
        Dim lbl_Hauteur As Integer = maFont.Size / maFont.FontFamily.GetCellAscent(0) * maFont.FontFamily.GetEmHeight(0) * 1.8
        Dim lbl_Largeur As Integer = 0
        Dim nbCouleursDisponibles As Integer = UBound(arrColor) - 1
        Dim monImage As Image

        ' ----- Barres :
        Dim maBColor, maFColor As Color
        Dim valMax As Decimal = MaxTableau(_valeurs)
        For i As Integer = 0 To nbBarres - 1
            'If _valeurs(i) < 0 Then _valeurs(i) = 0
            'cpt_Hauteur = _hauteur / 100 * _valeurs(i)
            'todo : gérer le cas MaxTableau = 0
            'cpt_Hauteur = MaxTableau(_valeurs) * _top / MaxTableau(_valeurs)
            'Dim cpt_Hauteur As Integer = MaxTableau(_valeurs)
            cpt_Hauteur = _hauteur / valMax * _valeurs(i)
            Dim ctl_Top As Integer = _top + _hauteur - cpt_Hauteur
            If _valeurs(i) <= 2 Then
                ctl_Top -= 1        ' on triche juste ce qu'il faut, c'est pour capter le ToolTip !
                cpt_Hauteur += 1
            End If
            Try
                picBRectangles = New PictureBox With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre, .Height = cpt_Hauteur, .BackColor = arrColor(i Mod nbCouleursDisponibles)}
                'Me.Show()
                'Me.TopMost = True
                'monImage = P.Image
                'monImage.Save("C:\Users\User\source\repos\gestionTresorerie\ressources\image1.jpg ", System.Drawing.Imaging.ImageFormat.Jpeg)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
            ToolTipHisto.SetToolTip(picBRectangles, " " & _legende(i) & " " & _valeurs(i) & " % ")
            Controls.Add(picBRectangles)
            ' ----- Valeurs :
            If _valeurs(i) > 0.5 * valMax Then
                ctl_Top = picBRectangles.Top
                maBColor = arrColor(i Mod nbCouleursDisponibles)
                maFColor = Color.Wheat
            ElseIf _valeurs(i) < 0.05 * valMax Then
                ctl_Top = picBRectangles.Top - 20
                maBColor = Color.Tan
                maFColor = Color.Black
            Else
                ctl_Top = picBRectangles.Top - 15
                maBColor = Color.Tan
                maFColor = Color.Black
            End If
            lblValeurs = New Label With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre, .BackColor = maBColor,
                                .ForeColor = maFColor, .Text = _valeurs(i), .TextAlign = ContentAlignment.TopCenter,
                                .FlatStyle = FlatStyle.System, .Font = maFontBold, .Height = 12}
            Controls.Add(lblValeurs)
            'L.Image.Save("C:\Users\User\source\repos\gestionTresorerie\ressources\image5.jpg")
            lblValeurs.BringToFront()
            cpt_Left += _largeurBarre + _interval
        Next
        ' ----- Barre des 50% :
        'P = New PictureBox With {.Top = _top + (_hauteur / 2), .Left = _left, .Width = ctl_Largeur, .Height = 1, .BackColor = Color.DarkMagenta}
        'monImage = P.Image
        'monImage.Save("C:\Users\User\source\repos\gestionTresorerie\ressources\image1.jpg ", System.Drawing.Imaging.ImageFormat.Jpeg)
        Controls.Add(picBRectangles)

        ' ----- Titre sous le graphe :
        lblValeurs = New Label With {.Text = _titre, .Font = maFont, .Left = _left, .Top = _top + _hauteur + 5,
                            .TextAlign = ContentAlignment.MiddleLeft, .FlatStyle = FlatStyle.System, .AutoSize = True}
        Controls.Add(lblValeurs)
        'L.Image.Save("C:\Users\User\source\repos\gestionTresorerie\ressources\image6.jpg")

        ' ----- Légende (nuancier + étiquettes ) :
        cpt_Left += 5 - _interval       ' marge de 5 pixels entre le graph et les carrés et entre les carrés et les étiquettes
        For i As Integer = nbBarres - 1 To 0 Step -1
            picBRectangles = New PictureBox With {.Top = ctl_Bas - 10, .Left = cpt_Left, .Width = 10, .Height = 10, .BackColor = arrColor(i Mod 7)}
            lblValeurs = New Label With {.Top = ctl_Bas - 12, .Left = cpt_Left + 10, .Text = _legende(i), .AutoSize = True}
            If lblValeurs.Width > lbl_Largeur Then lbl_Largeur = lblValeurs.Width
            ctl_Bas -= lbl_Hauteur - 4
            ToolTipHisto.SetToolTip(picBRectangles, " " & _valeurs(i) & " % ")
            ToolTipHisto.SetToolTip(lblValeurs, " " & _valeurs(i) & " % ")  ' et la valeur si on ne peut pas la capter (=0) dans le graphe
            Controls.Add(picBRectangles)
            Controls.Add(lblValeurs)
        Next

        ' ----- Cadre Fond :
        picBRectangles = New PictureBox With {.Top = _top, .Left = _left, .Width = ctl_Largeur, .Height = _hauteur, .BackColor = Color.Black}
        monImage = picBRectangles.Image
        Controls.Add(picBRectangles)

        Return picBRectangles
    End Function

    'TODO à sortir dans un module de copie d'écran si cela marche
    Private Declare Function BitBlt Lib "GDI32" (ByVal hDestDC As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hSrcDC As IntPtr, ByVal SrcX As Integer, ByVal SrcY As Integer, ByVal Rop As Integer) As Integer
    Private Declare Function GetForegroundWindow Lib "user32" () As IntPtr
    Private Declare Function GetWindowRect Lib "user32.dll" (ByVal hWnd As IntPtr, ByRef lpRect As Rectangle) As Integer
    Private Declare Function GetDesktopWindow Lib "user32" () As IntPtr
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
    Private Function MaxTableau(tab() As Decimal) As Decimal
        'TODO gérer les valeurs négatives
        Dim i As Integer
        Dim maxTemp As Decimal = 0
        For i = 0 To UBound(tab) - 1
            If tab(i) > maxTemp Then
                maxTemp = tab(i)
            End If
        Next
        Return maxTemp
    End Function


    'Private Sub Form1_Load(ByVal s As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    Public Sub TestHisto()
        Dim marge As Integer = 20
        Dim cpt_Right As Integer = Histo_PourCent("% Navets / Récolte Locale 1902", arrayNavets, arrayNom, marge, 40, 300, 30, 10)
        cpt_Right = Histo_PourCent("% Radis / Récolte Locale 1902", arrayCornichons, arrayNom, marge, cpt_Right + marge, 300, 20)
        cpt_Right = Histo_PourCent("% Persil / Récolte Locale 1902", arrayPersil, arrayNom, marge, cpt_Right + marge, 200, 30) ' etc
        Width = cpt_Right + marge
    End Sub

    ' ===== Histogramme à Barres Verticales pour 100 :
    Private Function Histo_PourCent(ByVal _titre As String, ByVal _valeurs() As Integer, ByVal _legende() As String,
                                  ByVal _top As Integer, ByVal _left As Integer, ByVal _hauteur As Integer, ByVal _largeurBarre As Integer,
                                  Optional ByVal _interval As Integer = 0) As Integer

        Dim nbBarres As Integer = _valeurs.Length : If nbBarres < 0 Then Return 0
        If nbBarres > _legende.Length Then Return 0
        Dim ctl_Largeur As Integer = (nbBarres * _largeurBarre) + ((nbBarres - 1) * _interval)
        Dim cpt_Left As Integer = IIf(_left < 10, 10, _left)    ' compteur
        Dim ctl_Bas As Integer = IIf(_top < 10, 10, _top) + IIf(_hauteur < 20, 20, _hauteur)
        Dim ctl_Top As Integer = 0
        Dim cpt_Hauteur As Integer = 0                          ' compteur
        Dim lbl_Hauteur As Integer = maFont.Size / maFont.FontFamily.GetCellAscent(0) * maFont.FontFamily.GetEmHeight(0) * 1.8
        Dim lbl_Largeur As Integer = 0

        ' ----- Barres :
        Dim maBColor, maFColor As Color
        For i As Integer = 0 To nbBarres - 1
            If _valeurs(i) > 100 Then _valeurs(i) = 100
            If _valeurs(i) < 0 Then _valeurs(i) = 0
            cpt_Hauteur = _hauteur / 100 * _valeurs(i)
            ctl_Top = _top + _hauteur - cpt_Hauteur
            If _valeurs(i) <= 2 Then
                ctl_Top -= 1        ' on triche juste ce qu'il faut, c'est pour capter le ToolTip !
                cpt_Hauteur += 1
            End If
            picBRectangles = New PictureBox With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre, .Height = cpt_Hauteur, .BackColor = arrColor(i)}
            'ttip.SetToolTip(P, " " & _legende(i) & " " & _valeurs(i) & " % ") 
            Controls.Add(picBRectangles)
            ' ----- Valeurs :
            If _valeurs(i) > 50 Then
                ctl_Top = picBRectangles.Top
                maBColor = arrColor(i)
                maFColor = Color.Wheat
            ElseIf _valeurs(i) < 5 Then
                ctl_Top = picBRectangles.Top - 20
                maBColor = Color.Tan
                maFColor = Color.Black
            Else
                ctl_Top = picBRectangles.Top - 15
                maBColor = Color.Tan
                maFColor = Color.Black
            End If
            lblValeurs = New Label With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre, .BackColor = maBColor,
                                .ForeColor = maFColor, .Text = _valeurs(i), .TextAlign = ContentAlignment.TopCenter,
                                .FlatStyle = FlatStyle.System, .Font = maFontBold, .Height = 12}
            Controls.Add(lblValeurs)
            lblValeurs.BringToFront()
            cpt_Left += _largeurBarre + _interval
        Next
        ' ----- Barre des 50% :
        picBRectangles = New PictureBox With {.Top = _top + (_hauteur / 2), .Left = _left, .Width = ctl_Largeur, .Height = 1, .BackColor = Color.DarkMagenta}
        Controls.Add(picBRectangles)

        ' ----- Titre sous le graphe :
        lblValeurs = New Label With {.Text = _titre, .Font = maFont, .Left = _left, .Top = _top + _hauteur + 5,
                            .TextAlign = ContentAlignment.MiddleLeft, .FlatStyle = FlatStyle.System, .AutoSize = True}
        Controls.Add(lblValeurs)

        ' ----- Légende (nuancier + étiquettes ) :
        cpt_Left += 5 - _interval       ' marge de 5 pixels entre le graph et les carrés et entre les carrés et les étiquettes
        For i As Integer = nbBarres - 1 To 0 Step -1
            picBRectangles = New PictureBox With {.Top = ctl_Bas - 10, .Left = cpt_Left, .Width = 10, .Height = 10, .BackColor = arrColor(i)}
            lblValeurs = New Label With {.Top = ctl_Bas - 12, .Left = cpt_Left + 10, .Text = _legende(i), .AutoSize = True}
            If lblValeurs.Width > lbl_Largeur Then lbl_Largeur = lblValeurs.Width
            ctl_Bas -= lbl_Hauteur - 4
            'ttip.SetToolTip(P, " " & _valeurs(i) & " % ")
            'ttip.SetToolTip(L, " " & _valeurs(i) & " % ")  ' et la valeur si on ne peut pas la capter (=0) dans le graphe
            Controls.Add(picBRectangles)
            Controls.Add(lblValeurs)
        Next

        ' ----- Cadre Fond :
        picBRectangles = New PictureBox With {.Top = _top, .Left = _left, .Width = ctl_Largeur, .Height = _hauteur, .BackColor = Color.Tan}
        Controls.Add(picBRectangles)
        picBRectangles.SendToBack()

        ' Return = la position right de l'histogramme = marge + barres + maxi étiquette légende
        Return cpt_Left + lbl_Largeur
    End Function

End Class

'Imports System.Data.SqlClient

'Public Class frmHistogramme
'    ' ===== CDC : Afficher un Histogramme % (ou plusieurs dans la même Form) 
'    '             indépendant des Données, paramétrable en position / dimensions / marges / couleurs etc.
'    '             utilisant seulement des Controles PictureBox et Labels 
'    ' ===== VERSION 1 = Histogramme(s) % via un Array 

'    Private P As PictureBox
'    Private L As Label
'    Private arrColor() As Color = {Color.Tomato, Color.DarkSeaGreen, Color.CornflowerBlue,
'                                   Color.Orchid, Color.OliveDrab, Color.SlateBlue, Color.Goldenrod}
'    Private arrayNavets() As Integer = {16, 85, 0, 76, 18, 100, 53}
'    Private arrayCornichons() As Integer = {6, 25, 36, 87, 8, 25, 35}
'    Private arrayPersil() As Integer = {76, 5, 63, 57, 2, 95, 50}
'    Private arrayNom() As String = {"Dijon", "Nantes", "Nice", "Paris", "Lyon", "Cherbourg", "Pau"}
'    Private maFont As New Font("Verdana", 9)
'    Private maFontBold As New Font("Verdana", 8, FontStyle.Bold)
'    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
'        'Dim marge As Integer = 20
'        'Dim cpt_Right As Integer = Histo_PourCent("% Navets / Récolte Locale 1902", arrayNavets, arrayNom, marge, 40, 300, 30, 10)
'        'cpt_Right = Histo_PourCent("% Radis / Récolte Locale 1902", arrayCornichons, arrayNom, marge, cpt_Right + marge, 300, 20)
'        'cpt_Right = Histo_PourCent("% Persil / Récolte Locale 1902", arrayPersil, arrayNom, marge, cpt_Right + marge, 200, 30) ' etc
'        ''Width = cpt_Right + marge

'    End Sub
'    'Public Sub afficheHisto(conn As SqlConnection)

'    'End Sub
'    ' ===== Histogramme à Barres Verticales pour 100 :
'    Public Function Histo_PourCent(ByVal _titre As String, ByVal _valeurs() As Decimal, ByVal _legende() As String,
'                                  ByVal _top As Integer, ByVal _left As Integer,
'                                  ByVal _hauteur As Integer, ByVal _largeurBarre As Integer,
'                                  Optional ByVal _interval As Integer = 0) As Integer

'        Dim nbBarres As Integer = _valeurs.Length : If nbBarres < 0 Then Return 0
'        If nbBarres > _legende.Length Then Return 0
'        Dim ctl_Largeur As Integer = (nbBarres * _largeurBarre) + ((nbBarres - 1) * _interval)
'        Dim cpt_Left As Integer = IIf(_left < 10, 10, _left)    ' compteur
'        Dim ctl_Bas As Integer = IIf(_top < 10, 10, _top) + IIf(_hauteur < 20, 20, _hauteur)
'        Dim ctl_Top As Integer = 0
'        Dim cpt_Hauteur As Integer = 0                          ' compteur
'        Dim lbl_Hauteur As Integer = maFont.Size / maFont.FontFamily.GetCellAscent(0) * maFont.FontFamily.GetEmHeight(0) * 1.8
'        Dim lbl_Largeur As Integer = 0

'        ' ----- Barres :
'        Dim maBColor, maFColor As Color
'        For i As Integer = 0 To nbBarres - 1
'            'If _valeurs(i) > 100 Then _valeurs(i) = 100
'            'If _valeurs(i) < 0 Then _valeurs(i) = 0
'            cpt_Hauteur = _hauteur / 100 * _valeurs(i)
'            ctl_Top = _top + _hauteur - cpt_Hauteur
'            If _valeurs(i) <= 2 Then
'                ctl_Top -= 1        ' on triche juste ce qu'il faut, c'est pour capter le ToolTip !
'                cpt_Hauteur += 1
'            End If
'            Try
'                'todo mod 7
'                P = New PictureBox With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre,
'                                     .Height = cpt_Hauteur, .BackColor = arrColor(i Mod 7)}
'            Catch ex As Exception
'                Console.WriteLine(ex.Message)
'            End Try
'            'ttip.SetToolTip(P, " " & _legende(i) & " " & _valeurs(i) & " % ")
'            Controls.Add(P)
'            ' ----- Valeurs :
'            If _valeurs(i) > 50 Then
'                ctl_Top = P.Top
'                maBColor = arrColor(i Mod 7)
'                maFColor = Color.Wheat
'            ElseIf _valeurs(i) < 5 Then
'                ctl_Top = P.Top - 20
'                maBColor = Color.Tan
'                maFColor = Color.Black
'            Else
'                ctl_Top = P.Top - 15
'                maBColor = Color.Tan
'                maFColor = Color.Black
'            End If
'            L = New Label With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre, .BackColor = maBColor,
'                                .ForeColor = maFColor, .Text = _valeurs(i), .TextAlign = ContentAlignment.TopCenter,
'                                .FlatStyle = FlatStyle.System, .Font = maFontBold, .Height = 12}
'            Controls.Add(L)
'            L.BringToFront()
'            cpt_Left += _largeurBarre + _interval
'        Next
'        ' ----- Barre des 50% :
'        P = New PictureBox With {.Top = _top + (_hauteur / 2), .Left = _left, .Width = ctl_Largeur,
'                                 .Height = 1, .BackColor = Color.DarkMagenta}
'        Controls.Add(P)

'        ' ----- Titre sous le graphe :
'        L = New Label With {.Text = _titre, .Font = maFont, .Left = _left, .Top = _top + _hauteur + 5,
'                            .TextAlign = ContentAlignment.MiddleLeft, .FlatStyle = FlatStyle.System, .AutoSize = True}
'        Controls.Add(L)

'        ' ----- Légende (nuancier + étiquettes ) :
'        cpt_Left += 5 - _interval       ' marge de 5 pixels entre le graph et les carrés et entre les carrés et les étiquettes
'        For i As Integer = nbBarres - 1 To 0 Step -1
'            P = New PictureBox With {.Top = ctl_Bas - 10, .Left = cpt_Left, .Width = 10, .Height = 10, .BackColor = arrColor(i Mod 7)}
'            L = New Label With {.Top = ctl_Bas - 12, .Left = cpt_Left + 10, .Text = _legende(i), .AutoSize = True}
'            If L.Width > lbl_Largeur Then lbl_Largeur = L.Width
'            ctl_Bas -= lbl_Hauteur - 4
'            'ttip.SetToolTip(P, " " & _valeurs(i) & " % ")
'            'ttip.SetToolTip(L, " " & _valeurs(i) & " % ")  ' et la valeur si on ne peut pas la capter (=0) dans le graphe
'            Controls.Add(P)
'            Controls.Add(L)
'        Next

'        ' ----- Cadre Fond :
'        P = New PictureBox With {.Top = _top, .Left = _left, .Width = ctl_Largeur, .Height = _hauteur,
'                                 .BackColor = Color.Tan}
'        Controls.Add(P)
'        P.SendToBack()

'        ' Return = la position right de l'histogramme = marge + barres + maxi étiquette légende
'        Return cpt_Left + lbl_Largeur
'    End Function
'End Class