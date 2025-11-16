Imports System.Windows.Forms.DataVisualization.Charting
Imports gestionTresorerie.StatsCinema

Public Class FrmStatsCinema
    Inherits Form

    ' ----- Contrôles -----
    Private WithEvents dtpDebut As New DateTimePicker With {.Format = DateTimePickerFormat.Short}
    Private WithEvents dtpFin As New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Left = 150}
    Private WithEvents btnFiltrer As New Button With {.Text = "Filtrer", .Left = 300}
    Private ChartParMois As New Chart With {.Dock = DockStyle.Top, .Height = 250}
    Private ChartParFilm As New Chart With {.Dock = DockStyle.Top, .Height = 250}
    Private ChartParPublic As New Chart With {.Dock = DockStyle.Top, .Height = 250}

    ' ----- Données -----
    Private _stats As List(Of StatFilm)

    Public Sub New()
        ' Initialisation de la fenêtre
        Me.Text = "Statistiques cinéma"
        Me.Width = 800
        Me.Height = 900

        ' Ajout des contrôles
        Me.Controls.Add(dtpDebut)
        Me.Controls.Add(dtpFin)
        Me.Controls.Add(btnFiltrer)

        ChartParMois.ChartAreas.Add(New ChartArea("CAParMois"))
        ChartParFilm.ChartAreas.Add(New ChartArea("CAParFilm"))
        ChartParPublic.ChartAreas.Add(New ChartArea("CAParPublic"))

        Me.Controls.Add(ChartParPublic)
        Me.Controls.Add(ChartParFilm)
        Me.Controls.Add(ChartParMois)
    End Sub

    ' ----- Propriété pour les stats -----
    Public Property Stats As List(Of StatFilm)
        Get
            Return _stats
        End Get
        Set(value As List(Of StatFilm))
            _stats = value
            AfficherTousGraphiques()
        End Set
    End Property

    ' ----- Bouton Filtrer -----
    Private Sub btnFiltrer_Click(sender As Object, e As EventArgs) Handles btnFiltrer.Click
        If _stats Is Nothing Then Exit Sub
        Dim dateDebut As Date = dtpDebut.Value.Date
        Dim dateFin As Date = dtpFin.Value.Date

        ' Filtrer les séances par date
        Dim statsFiltres = _stats.Select(Function(f)
                                             Dim copie = f.Clone() ' méthode Clone() à implémenter pour copier StatFilm avec ses séances
                                             copie.Seances = copie.Seances.Where(Function(s) s.DateHeureDebut.Date >= dateDebut AndAlso s.DateHeureDebut.Date <= dateFin).ToList()
                                             Return copie
                                         End Function).ToList()

        AfficherTousGraphiques(statsFiltres)
    End Sub

    ' ----- Affichage des graphiques -----
    Private Sub AfficherTousGraphiques(Optional statsAFiltrer As List(Of StatFilm) = Nothing)
        Dim statsToUse = If(statsAFiltrer, _stats)
        If statsToUse Is Nothing Then Exit Sub

        GenererGraphiqueCAParMois(statsToUse)
        GenererGraphiqueCAParFilm(statsToUse)
        GenererGraphiqueCAParPublic(statsToUse)
    End Sub

    ' ----- Graphique CA par mois -----
    Private Sub GenererGraphiqueCAParMois(stats As List(Of StatFilm))
        ChartParMois.Series.Clear()
        Dim series = New Series("CA Mensuel") With {.ChartType = SeriesChartType.Column}

        ' Calcul par mois
        Dim caParMois = stats.SelectMany(Function(f) f.Seances) _
                             .GroupBy(Function(s) New With {.Year = s.DateHeureDebut.Year, .Month = s.DateHeureDebut.Month}) _
                             .Select(Function(g) New With {
                                 .Mois = New Date(g.Key.Year, g.Key.Month, 1),
                                 .TotalCA = g.Sum(Function(s) s.CA_Total)
                             }).OrderBy(Function(x) x.Mois)

        For Each m In caParMois
            series.Points.AddXY(m.Mois.ToString("MMM yyyy"), m.TotalCA)
        Next

        ChartParMois.Series.Add(series)
        ChartParMois.ChartAreas(0).AxisX.Interval = 1
        ChartParMois.ChartAreas(0).AxisX.LabelStyle.Angle = -45
    End Sub

    ' ----- Graphique CA par film -----
    Private Sub GenererGraphiqueCAParFilm(stats As List(Of StatFilm))
        ChartParFilm.Series.Clear()
        Dim series = New Series("CA Film") With {.ChartType = SeriesChartType.Bar}

        For Each f In stats
            series.Points.AddXY(f.Titre, f.CA_Total)
        Next

        ChartParFilm.Series.Add(series)
        ChartParFilm.ChartAreas(0).AxisX.LabelStyle.Angle = -45
    End Sub

    ' ----- Graphique CA par public -----
    Private Sub GenererGraphiqueCAParPublic(stats As List(Of StatFilm))
        ChartParPublic.Series.Clear()
        Dim series = New Series("CA Public") With {.ChartType = SeriesChartType.Pie}

        Dim totalAdultes = stats.Sum(Function(f) f.CA_Adultes)
        Dim totalEnfants = stats.Sum(Function(f) f.CA_Enfants)
        Dim totalGroupeEnfants = stats.Sum(Function(f) f.CA_GroupeEnfants)

        series.Points.AddXY("Adultes", totalAdultes)
        series.Points.AddXY("Enfants", totalEnfants)
        series.Points.AddXY("Groupe Enfants", totalGroupeEnfants)

        ChartParPublic.Series.Add(series)
        ChartParPublic.Series(0).IsValueShownAsLabel = True
    End Sub

    Private Sub frmStatsCinema_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim stats = StatsCinema.GetStatsParFilm()

        ' Filtrer par période
        Dim dateDebut As Date = #2025-01-01#
        Dim dateFin As Date = #2025-12-31#
        Dim statsFiltres = stats.Select(Function(f)
                                            Dim clone = f.Clone()
                                            clone.Seances = clone.Seances.Where(Function(s) s.DateHeureDebut.Date >= dateDebut AndAlso s.DateHeureDebut.Date <= dateFin).ToList()
                                            Return clone
                                        End Function).ToList()

        ' Génération de graphiques
        Dim chartParMois = StatsCinema.GenererGraphiqueCAParMois()
        Dim chartParPublic = StatsCinema.GenererGraphiqueCAParPublic(statsFiltres)
    End Sub
End Class
