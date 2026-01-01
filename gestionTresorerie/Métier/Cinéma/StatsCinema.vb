Imports System.Data.SqlClient
Imports System.Windows.Forms.DataVisualization.Charting

Public Class StatsCinema

    ' --- Récupère les stats par film depuis la base ---
    Public Shared Function GetStatsParFilm() As List(Of StatFilm)
        Dim filmsDict As New Dictionary(Of Integer, StatFilm)

        Try
            ' 1. Chargement des tarifs (Modularisé)
            Dim tarifs = ChargerDicoTarifsValides()

            ' 2. Traitement des séances
            Using cmdSeances = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selSeancesAvecFilm")
                Using rdr = cmdSeances.ExecuteReader()
                    While rdr.Read()
                        ' Extraction propre des données
                        Dim idFilm = rdr.GetValueOrDefault(Of Integer)("IdFilm")
                        Dim titre = rdr.GetValueOrDefault(Of String)("TitreFilm")

                        ' Gestion de l'instance StatFilm (Utilisation de TryGetValue pour éviter CA1854 ici aussi)
                        Dim stat As StatFilm = Nothing
                        If Not filmsDict.TryGetValue(idFilm, stat) Then
                            stat = New StatFilm With {.IdFilm = idFilm, .Titre = titre}
                            filmsDict.Add(idFilm, stat)
                        End If

                        ' Création de la séance avec calcul CA (Utilisation des utilitaires de tarifs)
                        Dim seance As New Seance(
                        idFilm:=idFilm,
                        dateHeureDebut:=rdr.GetValueOrDefault(Of DateTime)("DateHeureDebut"),
                        nbAdultes:=rdr.GetValueOrDefault(Of Integer)("NbEntreesAdultes", 0),
                        nbEnfants:=rdr.GetValueOrDefault(Of Integer)("NbEntreesEnfants", 0),
                        nbGroupeEnfants:=rdr.GetValueOrDefault(Of Integer)("NbEntreesGroupeEnfants", 0),
                        tarifAdulte:=GetTarifOrDefault(tarifs, "Adulte"),
                        tarifEnfant:=GetTarifOrDefault(tarifs, "Enfant"),
                        tarifGroupeEnfant:=GetTarifOrDefault(tarifs, "GroupeEnfant")
                    )

                        stat.Seances.Add(seance)
                    End While
                End Using
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur GetStatsParFilm : {ex.Message}")
        End Try

        Return filmsDict.Values.ToList()
    End Function

    ' Fonction d'aide pour isoler la requête des tarifs
    Private Shared Function ChargerDicoTarifsValides() As Dictionary(Of String, Decimal)
        Dim dico As New Dictionary(Of String, Decimal)
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selTarifsValides")
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    dico(rdr("NomTarif").ToString()) = CDec(rdr("Montant"))
                End While
            End Using
        End Using
        Return dico
    End Function


    ' --- Génère un graphique de type barre par film ---
    Public Shared Function GenererGraphiqueCAParFilm(stats As List(Of StatFilm)) As Chart
        Dim chart As New Chart()
        chart.Width = 800
        chart.Height = 600

        Dim area As New ChartArea("CA")
        chart.ChartAreas.Add(area)

        Dim series As New Series("Chiffre d'affaires")
        series.ChartType = SeriesChartType.Column
        series.IsValueShownAsLabel = True ' affiche les valeurs sur les colonnes
        chart.Series.Add(series)

        ' Assure l'unicité X en utilisant IdFilm si nécessaire
        series.XValueType = ChartValueType.String
        series.YValueType = ChartValueType.Double
        For Each s In stats
            Dim p = series.Points.AddXY(s.IdFilm, CDbl(s.CA_Total))
            series.Points(p).AxisLabel = s.Titre  ' label visible sous la colonne
        Next
        ' Palette de couleurs automatiques (cycle si nécessaire)
        Dim palette() As Color = {Color.Blue, Color.Orange, Color.Green, Color.Purple, Color.Red, Color.Cyan, Color.Magenta}
        For index = 0 To series.Points.Count - 1
            series.Points(index).Color = palette(index Mod palette.Length)
        Next
        chart.ChartAreas(0).AxisX.Interval = 1
        chart.Titles.Add("Chiffre d'affaires par film")
        chart.ChartAreas(0).AxisX.Title = "Film"
        chart.ChartAreas(0).AxisX.IsLabelAutoFit = True
        chart.ChartAreas(0).AxisY.Title = "Montant (€)"
        chart.Legends.Add(New Legend("Légende"))

        Return chart
    End Function


    ' --- Génère un graphique par type de public (Adulte, Enfant, Groupe) ---
    Public Shared Function GenererGraphiqueCAParPublic(stats As List(Of StatFilm)) As Chart
        Dim chart As New Chart()
        chart.Width = 800
        chart.Height = 600

        Dim area As New ChartArea("CA_Public")
        chart.ChartAreas.Add(area)

        Dim series As New Series("CA Public")
        series.ChartType = SeriesChartType.Column
        series.IsValueShownAsLabel = True
        series.XValueType = ChartValueType.String
        series.YValueType = ChartValueType.Double
        series.Points.Clear()
        chart.Series.Add(series)

        ' Sommes
        Dim totalAdultes = stats.Sum(Function(s) s.CA_Adultes)
        Dim totalEnfants = stats.Sum(Function(s) s.CA_Enfants)
        Dim totalGroupe = stats.Sum(Function(s) s.CA_GroupeEnfants)

        ' Ajout des points 
        Dim p = series.Points.AddXY(1, CDbl(totalAdultes))
        series.Points(p).AxisLabel = "Adultes"  ' label visible sous la colonne
        Dim q = series.Points.AddXY(2, CDbl(totalEnfants))
        series.Points(q).AxisLabel = "Enfants"  ' label visible sous la colonne
        Dim r = series.Points.AddXY(3, CDbl(totalGroupe))
        series.Points(r).AxisLabel = "Groupe enfants"  ' label visible sous la colonne 

        ' Palette de couleurs automatiques (cycle si nécessaire)
        Dim palette() As Color = {Color.Blue, Color.Orange, Color.Green, Color.Purple, Color.Red, Color.Cyan, Color.Magenta}
        For index = 0 To series.Points.Count - 1
            series.Points(index).Color = palette(index Mod palette.Length)
        Next

        ' Fixe l'axe X pour qu'il soit discret et affiche chaque label
        chart.ChartAreas(0).AxisX.Interval = 1
        chart.ChartAreas(0).AxisX.MajorGrid.Enabled = False
        chart.ChartAreas(0).AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray

        ' Titres et légende
        chart.Titles.Add("Chiffre d'affaires par type de public")
        chart.ChartAreas(0).AxisX.Title = "Public"
        chart.ChartAreas(0).AxisY.Title = "Montant (€)"
        chart.Legends.Add(New Legend("Légende"))

        Return chart
    End Function

    ' --- Génère un graphique par période ---
    Public Shared Function GenererGraphiqueCAParMois() As Chart
        Dim chart As New Chart()
        chart.Width = 800
        chart.Height = 600

        Dim area As New ChartArea("CA_Mois")
        chart.ChartAreas.Add(area)

        Dim series As New Series("CA Mensuel")
        series.ChartType = SeriesChartType.Column
        series.IsValueShownAsLabel = True
        chart.Series.Add(series)

        Try
            ' Regrouper par mois pour sommer le CA total
            Dim dictMois As New Dictionary(Of String, Decimal)

            Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selStatsParMois")
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim mois = rdr("Mois").ToString() ' Format "2025-11" ou "Nov 2025"
                        Dim montant = CDec(rdr("CA_Total"))

                        If dictMois.ContainsKey(mois) Then
                            dictMois(mois) += montant
                        Else
                            dictMois(mois) = montant
                        End If
                    End While
                End Using
            End Using

            ' Ajouter les points triés par mois
            Dim index As Integer
            For Each kvp In dictMois.OrderBy(Function(d) d.Key)
                series.Points.AddXY(CDbl(kvp.Key), kvp.Value)
            Next
            ' Palette de couleurs automatiques (cycle si nécessaire)
            Dim palette() As Color = {Color.Blue, Color.Orange, Color.Green, Color.Purple, Color.Red, Color.Cyan, Color.Magenta}
            For index = 0 To series.Points.Count - 1
                series.Points(index).Color = palette(index Mod palette.Length)
            Next
        Catch ex As Exception
            Logger.ERR($"Erreur GenererGraphiqueCAParMois : {ex.Message}")
        End Try

        chart.Titles.Add("Chiffre d'affaires par mois")
        chart.ChartAreas(0).AxisX.Title = "Mois"
        'chart.ChartAreas(0).AxisX.LabelStyle.Angle = -45
        chart.ChartAreas(0).AxisY.Title = "Montant (€)"
        chart.Legends.Add(New Legend("Légende"))

        Return chart
    End Function
End Class
