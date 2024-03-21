Imports System.Data.SqlClient

Public Class frmHistogramme
    ' ===== CDC : Afficher un Histogramme % (ou plusieurs dans la même Form) 
    '             indépendant des Données, paramétrable en position / dimensions / marges / couleurs etc.
    '             utilisant seulement des Controles PictureBox et Labels 
    ' ===== VERSION 1 = Histogramme(s) % via un Array 

    Private P As PictureBox
    Private L As Label
    Private arrColor() As Color = {Color.Tomato, Color.DarkSeaGreen, Color.CornflowerBlue,
                                   Color.Orchid, Color.OliveDrab, Color.SlateBlue, Color.Goldenrod}
    'Private arrayNavets() As Integer = {16, 85, 0, 76, 18, 100, 53}
    'Private arrayCornichons() As Integer = {6, 25, 36, 87, 8, 25, 35}
    'Private arrayPersil() As Integer = {76, 5, 63, 57, 2, 95, 50}
    'Private arrayNom() As String = {"Dijon", "Nantes", "Nice", "Paris", "Lyon", "Cherbourg", "Pau"}
    Private maFont As New Font("Verdana", 9)
    Private maFontBold As New Font("Verdana", 8, FontStyle.Bold)
    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Dim marge As Integer = 20
        'Dim cpt_Right As Integer = Histo_PourCent("% Navets / Récolte Locale 1902", arrayNavets, arrayNom, marge, 40, 300, 30, 10)
        'cpt_Right = Histo_PourCent("% Radis / Récolte Locale 1902", arrayCornichons, arrayNom, marge, cpt_Right + marge, 300, 20)
        'cpt_Right = Histo_PourCent("% Persil / Récolte Locale 1902", arrayPersil, arrayNom, marge, cpt_Right + marge, 200, 30) ' etc
        ''Width = cpt_Right + marge

    End Sub
    'Public Sub afficheHisto(conn As SqlConnection)

    'End Sub
    ' ===== Histogramme à Barres Verticales pour 100 :
    Public Function Histogramme(ByVal _titre As String, ByVal _valeurs() As Decimal, ByVal _legende() As String,
                                  ByVal _top As Integer, ByVal _left As Integer,
                                  ByVal _hauteur As Integer, ByVal _largeurBarre As Integer,
                                  Optional ByVal _interval As Integer = 0) As Integer
        'Contrôles sur le nb de valeurs
        Dim nbBarres As Integer = _valeurs.Length : If nbBarres < 0 Then Return 0
        If nbBarres > _legende.Length Then Return 0
        'Largeur de l'histogramme
        Dim ctl_Largeur As Integer = (nbBarres * _largeurBarre) + ((nbBarres - 1) * _interval)
        'Définition des marges gauche et bas
        Dim cpt_Left As Integer = IIf(_left < 10, 10, _left)
        Dim ctl_Bas As Integer = IIf(_top < 10, 10, _top) + IIf(_hauteur < 20, 20, _hauteur)
        Dim lbl_Hauteur As Integer = maFont.Size / maFont.FontFamily.GetCellAscent(0) * maFont.FontFamily.GetEmHeight(0) * 1.8
        Dim lbl_Largeur As Integer = 0
        Dim nbCouleursDisponibles As Integer = UBound(arrColor) - 1

        ' ----- Barres :
        Dim maBColor, maFColor As Color
        For i As Integer = 0 To nbBarres - 1
            'If _valeurs(i) > 100 Then _valeurs(i) = 100
            'If _valeurs(i) < 0 Then _valeurs(i) = 0
            'cpt_Hauteur = _hauteur / 100 * _valeurs(i)
            'todo : gérer le cas MaxTableau = 0
            'cpt_Hauteur = MaxTableau(_valeurs) * _top / MaxTableau(_valeurs)
            Dim cpt_Hauteur As Integer = MaxTableau(_valeurs)
            Dim ctl_Top As Integer = _top + _hauteur - cpt_Hauteur
            If _valeurs(i) <= 2 Then
                ctl_Top -= 1        ' on triche juste ce qu'il faut, c'est pour capter le ToolTip !
                cpt_Hauteur += 1
            End If
            Try
                P = New PictureBox With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre,
                                     .Height = cpt_Hauteur, .BackColor = arrColor(i Mod nbCouleursDisponibles)}
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
            'ttip.SetToolTip(P, " " & _legende(i) & " " & _valeurs(i) & " % ")
            Controls.Add(P)
            ' ----- Valeurs :
            If _valeurs(i) > 50 Then
                ctl_Top = P.Top
                maBColor = arrColor(i Mod nbCouleursDisponibles)
                maFColor = Color.Wheat
            ElseIf _valeurs(i) < 5 Then
                ctl_Top = P.Top - 20
                maBColor = Color.Tan
                maFColor = Color.Black
            Else
                ctl_Top = P.Top - 15
                maBColor = Color.Tan
                maFColor = Color.Black
            End If
            L = New Label With {.Left = cpt_Left, .Top = ctl_Top, .Width = _largeurBarre, .BackColor = maBColor,
                                .ForeColor = maFColor, .Text = _valeurs(i), .TextAlign = ContentAlignment.TopCenter,
                                .FlatStyle = FlatStyle.System, .Font = maFontBold, .Height = 12}
            Controls.Add(L)
            L.BringToFront()
            cpt_Left += _largeurBarre + _interval
        Next
        ' ----- Barre des 50% :
        P = New PictureBox With {.Top = _top + (_hauteur / 2), .Left = _left, .Width = ctl_Largeur,
                                 .Height = 1, .BackColor = Color.DarkMagenta}
        Controls.Add(P)

        ' ----- Titre sous le graphe :
        L = New Label With {.Text = _titre, .Font = maFont, .Left = _left, .Top = _top + _hauteur + 5,
                            .TextAlign = ContentAlignment.MiddleLeft, .FlatStyle = FlatStyle.System, .AutoSize = True}
        Controls.Add(L)

        ' ----- Légende (nuancier + étiquettes ) :
        cpt_Left += 5 - _interval       ' marge de 5 pixels entre le graph et les carrés et entre les carrés et les étiquettes
        For i As Integer = nbBarres - 1 To 0 Step -1
            P = New PictureBox With {.Top = ctl_Bas - 10, .Left = cpt_Left, .Width = 10, .Height = 10, .BackColor = arrColor(i Mod 7)}
            L = New Label With {.Top = ctl_Bas - 12, .Left = cpt_Left + 10, .Text = _legende(i), .AutoSize = True}
            If L.Width > lbl_Largeur Then lbl_Largeur = L.Width
            ctl_Bas -= lbl_Hauteur - 4
            'ttip.SetToolTip(P, " " & _valeurs(i) & " % ")
            'ttip.SetToolTip(L, " " & _valeurs(i) & " % ")  ' et la valeur si on ne peut pas la capter (=0) dans le graphe
            Controls.Add(P)
            Controls.Add(L)
        Next

        ' ----- Cadre Fond :
        P = New PictureBox With {.Top = _top, .Left = _left, .Width = ctl_Largeur, .Height = _hauteur,
                                 .BackColor = Color.Tan}
        Controls.Add(P)
        P.SendToBack()

        ' Return = la position right de l'histogramme = marge + barres + maxi étiquette légende
        Return cpt_Left + lbl_Largeur
    End Function
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