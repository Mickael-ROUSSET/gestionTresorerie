Imports System.Data.SqlClient
Imports System.Windows.Forms.DataVisualization.Charting

Public Class StatsCinema

    ' --- Récupère les stats par film depuis la base ---
    Public Shared Function GetStatsParFilm() As List(Of StatFilm)
        Dim result As New List(Of StatFilm)

        Try
            ' 1. Récupérer tous les tarifs valides
            Dim tarifs As New Dictionary(Of String, Decimal)
            Using cmdTarifs = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selTarifsValides")
                Using rdr = cmdTarifs.ExecuteReader()
                    While rdr.Read()
                        Dim nom As String = rdr("NomTarif").ToString()
                        Dim montant As Decimal = CDec(rdr("Montant"))
                        tarifs(nom) = montant
                    End While
                End Using
            End Using

            ' 2. Récupérer toutes les séances avec les infos de film
            Using cmdSeances = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selSeancesAvecFilm")
                Using rdr = cmdSeances.ExecuteReader()
                    Dim filmsDict As New Dictionary(Of Integer, StatFilm)

                    While rdr.Read()
                        Dim idFilm As Integer = CInt(rdr("IdFilm"))
                        Dim titre As String = rdr("TitreFilm").ToString()

                        ' Crée le StatFilm si inexistant
                        If Not filmsDict.ContainsKey(idFilm) Then
                            filmsDict(idFilm) = New StatFilm With {.IdFilm = idFilm, .Titre = titre}
                        End If

                        ' Nombre d'entrées (valeurs par défaut si NULL)
                        Dim nbAdultes As Integer = If(IsDBNull(rdr("NbEntreesAdultes")), 0, CInt(rdr("NbEntreesAdultes")))
                        Dim nbEnfants As Integer = If(IsDBNull(rdr("NbEntreesEnfants")), 0, CInt(rdr("NbEntreesEnfants")))
                        Dim nbGroupe As Integer = If(IsDBNull(rdr("NbEntreesGroupeEnfants")), 0, CInt(rdr("NbEntreesGroupeEnfants")))

                        ' Crée une séance avec calcul automatique du CA
                        Dim seance As New Seance(
                            idFilm:=idFilm,
                            dateHeureDebut:=CDate(rdr("DateHeureDebut")),
                            nbAdultes:=nbAdultes,
                            nbEnfants:=nbEnfants,
                            nbGroupeEnfants:=nbGroupe,
                            tarifAdulte:=If(tarifs.ContainsKey("Adulte"), tarifs("Adulte"), 0D),
                            tarifEnfant:=If(tarifs.ContainsKey("Enfant"), tarifs("Enfant"), 0D),
                            tarifGroupeEnfant:=If(tarifs.ContainsKey("GroupeEnfant"), tarifs("GroupeEnfant"), 0D)
                        )

                        ' Ajoute la séance au film correspondant
                        filmsDict(idFilm).Seances.Add(seance)
                    End While

                    ' Ajoute tous les StatFilm à la liste de résultat
                    result.AddRange(filmsDict.Values)
                End Using
            End Using

        Catch ex As Exception
            Logger.ERR($"Erreur GetStatsParFilm : {ex.Message}")
        End Try

        Return result
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
