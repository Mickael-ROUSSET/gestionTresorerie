
Imports System.Windows.Forms.DataVisualization.Charting
Public Class FrmHistogramme
    'Liste De couleurs
    'https://codes-sources.commentcamarche.net/source/53025-afficher-un-histogramme-personnalise 
    Private arrColor() As Color = {Color.Navy, Color.SteelBlue, Color.DarkGreen,
                                Color.SlateGray, Color.DimGray, Color.ForestGreen, Color.DarkGray}

    Public Sub creeChart(ByVal titre As String, ByVal valeurs() As Decimal, ByVal legende() As String)
        ' https://plasserre.developpez.com/cours/chart/#LIII-C-1
        Dim i As Integer

        ' Supprimer tous les points
        ChartBilan.Series("Series1").Points.Clear()

        ' Définir les titres des axes
        ChartBilan.ChartAreas(0).AxisX.Title = "Catégorie"
        ChartBilan.ChartAreas(0).AxisY.Title = "Montant"

        ' Définir le style de la série
        ChartBilan.Palette = ChartColorPalette.Fire
        ChartBilan.Series("Series1").MarkerStyle = MarkerStyle.Diamond

        ' Appliquer un dégradé sur la couleur du fond
        ChartBilan.BackGradientStyle = GradientStyle.DiagonalRight

        ' Modifier la lumière : la couleur des côtés et du dessus des colonnes change
        ChartBilan.ChartAreas("ChartArea1").Area3DStyle.LightStyle = LightStyle.Realistic

        ' Définir la couleur de la bordure de la série
        ChartBilan.Series("Series1").BorderColor = Color.Bisque

        ' Vérifier et définir le type de graphique
        If ChartBilan.Series("Series1").ChartType <> SeriesChartType.Column Then
            ChartBilan.Series("Series1").ChartType = SeriesChartType.Column
        End If

        ' Configurer l'axe X pour afficher des catégories
        ChartBilan.ChartAreas(0).AxisX.IsMarginVisible = True
        ChartBilan.ChartAreas(0).AxisX.Interval = 1
        ChartBilan.ChartAreas(0).AxisX.LabelStyle.Interval = 1
        ChartBilan.ChartAreas(0).AxisX.MajorGrid.Enabled = True

        ' Ajouter les points avec les légendes 
        For i = 0 To UBound(valeurs)
            'ChartBilan.Series("Series1").Points.AddXY(legende(i), valeurs(i))
            ChartBilan.Series("Series1").Points.AddXY((i), valeurs(i))
            ChartBilan.Series("Series1").Points(i).AxisLabel = legende(i) ' Assurez-vous que les étiquettes sont définies
            ChartBilan.Series("Series1").Points(i).Color = arrColor(i Mod (UBound(arrColor) + 1))
        Next

        ' Indiquer d'afficher cette série sur le ChartArea1
        ChartBilan.Series("Series1").ChartArea = "ChartArea1"

        ' Forcer le recalcul des étiquettes de l'axe X
        ChartBilan.ChartAreas(0).RecalculateAxesScale()
    End Sub
End Class
