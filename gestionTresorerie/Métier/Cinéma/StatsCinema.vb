Imports System.Data.SqlClient
Imports System.Windows.Forms.DataVisualization.Charting

Public Class StatsCinema

    '' Structure pour stocker les stats par film
    'Public Class StatFilm
    '    Public Property IdFilm As Integer
    '    Public Property Titre As String
    '    Public Property NbSeances As Integer
    '    Public Property TotalAdultes As Integer
    '    Public Property TotalEnfants As Integer
    '    Public Property TotalGroupeEnfants As Integer
    '    Public Property CA_Adultes As Decimal
    '    Public Property CA_Enfants As Decimal
    '    Public Property CA_GroupeEnfants As Decimal
    '    Public ReadOnly Property CA_Total As Decimal
    '        Get
    '            Return CA_Adultes + CA_Enfants + CA_GroupeEnfants
    '        End Get
    '    End Property
    '    Public Property Seances As List(Of Seance)
    'End Class

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
                            nbGroupeEnfants:=nbGroupe
                        )
                        'tarifAdulte:=If(tarifs.ContainsKey("Adulte"), tarifs("Adulte"), 0D),
                        'tarifEnfant:=If(tarifs.ContainsKey("Enfant"), tarifs("Enfant"), 0D),
                        'tarifGroupeEnfant:=If(tarifs.ContainsKey("GroupeEnfant"), tarifs("GroupeEnfant"), 0D)

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
        chart.Series.Add(series)

        For Each s In stats
            series.Points.AddXY(s.Titre, s.CA_Total)
        Next

        ' Optionnel : titres, axes, légendes
        chart.Titles.Add("Chiffre d'affaires par film")
        chart.ChartAreas(0).AxisX.Title = "Film"
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
        chart.Series.Add(series)

        ' On somme pour chaque type
        Dim totalAdultes = stats.Sum(Function(s) s.CA_Adultes)
        Dim totalEnfants = stats.Sum(Function(s) s.CA_Enfants)
        Dim totalGroupe = stats.Sum(Function(s) s.CA_GroupeEnfants)

        series.Points.AddXY("Adultes", totalAdultes)
        series.Points.AddXY("Enfants", totalEnfants)
        series.Points.AddXY("Groupe Enfants", totalGroupe)

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
        series.ChartType = SeriesChartType.Line
        chart.Series.Add(series)

        Try
            Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selStatsParMois")
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim mois = rdr("Mois").ToString()
                        Dim montant = CDec(rdr("CA_Total"))
                        series.Points.AddXY(mois, montant)
                    End While
                End Using
            End Using
        Catch ex As Exception
            Logger.ERR($"Erreur GenererGraphiqueCAParMois : {ex.Message}")
        End Try

        chart.Titles.Add("Chiffre d'affaires par mois")
        chart.ChartAreas(0).AxisX.Title = "Mois"
        chart.ChartAreas(0).AxisY.Title = "Montant (€)"
        chart.Legends.Add(New Legend("Légende"))

        Return chart
    End Function

End Class
