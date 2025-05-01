
Imports System.Windows.Forms.DataVisualization.Charting
Public Class FrmHistogramme
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
        For i = 0 To UBound(valeurs)
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

End Class
