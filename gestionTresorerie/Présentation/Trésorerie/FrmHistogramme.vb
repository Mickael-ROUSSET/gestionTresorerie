
Imports System.Windows.Forms.DataVisualization.Charting
Public Class FrmHistogramme
    'Liste De couleurs
    'https://codes-sources.commentcamarche.net/source/53025-afficher-un-histogramme-personnalise 
    Private arrColor() As Color = {Color.Navy, Color.SteelBlue, Color.DarkGreen,
                                Color.SlateGray, Color.DimGray, Color.ForestGreen, Color.DarkGray}

    Friend Sub Explorer(cs As String)
        Dim tables = AdministrationBase.TablesMetier(cs)
        Dim axes As New Dictionary(Of String,String)
        If tables.Contains("Mouvements") Then
            AjouterAxe(axes,"Trésorerie par mois","CONVERT(char(7),m.dateMvt,126)","")
            AjouterAxe(axes,"Trésorerie par événement","NULLIF(m.evenement,'')","")
            If tables.Contains("Categorie") Then AjouterAxe(axes,"Trésorerie par catégorie","c.libelle"," LEFT JOIN dbo.Categorie c ON c.Id=m.categorie")
            If tables.Contains("SousCategorie") Then AjouterAxe(axes,"Trésorerie par sous-catégorie","c.libelle"," LEFT JOIN dbo.SousCategorie c ON c.Id=m.sousCategorie")
            If tables.Contains("Exercices") Then AjouterAxe(axes,"Trésorerie par exercice","CONVERT(char(10),c.dateDebut,23)"," LEFT JOIN dbo.Exercices c ON m.dateMvt BETWEEN c.dateDebut AND c.dateFin")
        End If
        If tables.Contains("Documents") Then axes.Add("Documents par type","SELECT COALESCE(categorieDoc,'Non renseigné'),COUNT_BIG(*) AS Documents FROM dbo.Documents GROUP BY categorieDoc ORDER BY 1")
        If tables.Contains("ParticipantMdN") Then axes.Add("Marché de Noël par activité","SELECT COALESCE(NULLIF(Activite,''),'Non renseigné'),COUNT_BIG(*) AS Participants FROM dbo.ParticipantMdN GROUP BY NULLIF(Activite,'') ORDER BY 1")
        If tables.Contains("Seances") Then
            Dim totaux="SUM(NbEntreesAdultes) AS Adultes,SUM(NbEntreesEnfants) AS Enfants,SUM(NbEntreesGroupeEnfants) AS Groupes"
            axes.Add("Entrées cinéma par mois","SELECT CONVERT(char(7),DateHeureDebut,126)," & totaux & " FROM dbo.Seances GROUP BY CONVERT(char(7),DateHeureDebut,126) ORDER BY 1")
            If tables.Contains("Films") Then axes.Add("Entrées cinéma par film","SELECT COALESCE(f.Titre,'Non renseigné')," & totaux & " FROM dbo.Seances s LEFT JOIN dbo.Films f ON f.IdFilm=s.IdFilm GROUP BY f.Titre ORDER BY 1")
        End If
        If axes.Count=0 Then Throw New InvalidOperationException("Aucune table d’analyse disponible.")
        Dim choix As New ComboBox With {.Dock=DockStyle.Top,.DropDownStyle=ComboBoxStyle.DropDownList}
        choix.Items.AddRange(axes.Keys.ToArray())
        ChartBilan.Dock=DockStyle.Fill
        Controls.Add(choix)
        ChartBilan.BringToFront()
        AddHandler choix.SelectedIndexChanged, Sub()
            Try
                Dim titre=choix.Text
                ChartBilan.Series.Clear()
                ChartBilan.Titles(0).Text=titre
                ChartBilan.ChartAreas(0).AxisY.Title=If(titre.StartsWith("Trésorerie"),"Montant (€)","Nombre")
                Using cn As New System.Data.SqlClient.SqlConnection(cs), cmd As New System.Data.SqlClient.SqlCommand(axes(titre),cn)
                    cn.Open()
                    Using r=cmd.ExecuteReader()
                        For i=1 To r.FieldCount-1
                            ChartBilan.Series.Add(r.GetName(i)).ChartType=SeriesChartType.Column
                        Next
                        If Not r.HasRows Then ChartBilan.Titles(0).Text=titre & " — aucune donnée"
                        While r.Read()
                            For i=1 To r.FieldCount-1
                                ChartBilan.Series(i-1).Points.AddXY(Convert.ToString(r(0)),Convert.ToDouble(r(i)))
                            Next
                        End While
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show(Me,ex.Message,Text)
            End Try
        End Sub
        choix.SelectedIndex=0
    End Sub

    Private Shared Sub AjouterAxe(axes As Dictionary(Of String,String), titre As String, valeur As String, jointure As String)
        axes.Add(titre,"SELECT COALESCE(" & valeur & ",'Non renseigné'), SUM(CASE WHEN m.sens=1 THEN ABS(m.montant) ELSE 0 END) AS Recettes,SUM(CASE WHEN m.sens=0 THEN ABS(m.montant) ELSE 0 END) AS Dépenses FROM dbo.Mouvements m" & jointure & " GROUP BY " & valeur & " ORDER BY 1")
    End Sub

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
            Dim unused = ChartBilan.Series("Series1").Points.AddXY(i, valeurs(i))
            ChartBilan.Series("Series1").Points(i).AxisLabel = legende(i) ' Assurez-vous que les étiquettes sont définies
            ChartBilan.Series("Series1").Points(i).Color = arrColor(i Mod (UBound(arrColor) + 1))
        Next

        ' Indiquer d'afficher cette série sur le ChartArea1
        ChartBilan.Series("Series1").ChartArea = "ChartArea1"

        ' Forcer le recalcul des étiquettes de l'axe X
        ChartBilan.ChartAreas(0).RecalculateAxesScale()
    End Sub
End Class
