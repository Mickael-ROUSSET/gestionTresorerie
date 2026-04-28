Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Drawing

''' <summary>
''' Classe centralisée pour l'analyse des données et la génération de graphiques.
''' </summary>
<DebuggerDisplay("Gestionnaire de Statistiques")>
Public NotInheritable Class StatsCinema
    Private Shared Function CreateRepository() As StatsCinemaRepository
        Dim executor As ISqlExecutor =
    RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New StatsCinemaRepository(executor)
    End Function

    ''' <summary>
    ''' Génère le graphique du Chiffre d'Affaires réparti par type de public.
    ''' </summary>
    Public Shared Function GenererGraphiqueCAParPublic(stats As List(Of StatFilm)) As Chart
        Dim donnees = New List(Of Object) From {
        New With {Key .Label = "Adultes", Key .Valeur = CDbl(stats.Sum(Function(s) s.CA_Adultes))},
        New With {Key .Label = "Enfants", Key .Valeur = CDbl(stats.Sum(Function(s) s.CA_Enfants))},
        New With {Key .Label = "Groupes", Key .Valeur = CDbl(stats.Sum(Function(s) s.CA_GroupeEnfants))}
    }

        Return ConstruireGraphiqueDeBase("CA par type de public", "Public", donnees)
    End Function

    ''' <summary>
    ''' Génère le graphique du Chiffre d'Affaires par période mensuelle.
    ''' </summary>
    Public Shared Function GenererGraphiqueCAParMois() As Chart
        Dim dictMois As New Dictionary(Of String, Decimal)

        Try
            dictMois = CreateRepository().LireStatsParMois()
        Catch ex As Exception
            Logger.ERR($"Erreur récupération mois : {ex.Message}")
        End Try

        Dim donnees = dictMois.OrderBy(Function(kvp) kvp.Key).
                       Select(Function(kvp) New With {Key .Label = kvp.Key, Key .Valeur = CDbl(kvp.Value)}).
                       ToList()

        Return ConstruireGraphiqueDeBase("Chiffre d'affaires par mois", "Période", donnees)
    End Function

    ' --- MOTEUR DE RENDU PRIVÉ (CENTRALISATION DU DESIGN) ---

    Private Shared Function ConstruireGraphiqueDeBase(titre As String, titreAxeX As String, donnees As IEnumerable(Of Object)) As Chart
        Dim chart As New Chart() With {.Width = 800, .Height = 600}

        Dim area As New ChartArea("MainArea")
        area.AxisX.Title = titreAxeX
        area.AxisX.Interval = 1
        area.AxisY.Title = "Montant (€)"
        area.AxisY.MajorGrid.LineColor = Color.LightGray
        chart.ChartAreas.Add(area)

        Dim series As New Series("CA") With {
        .ChartType = SeriesChartType.Column,
        .IsValueShownAsLabel = True,
        .XValueType = ChartValueType.String
    }

        Dim palette() As Color = {Color.Blue, Color.Orange, Color.Green, Color.Purple, Color.Red, Color.Cyan, Color.Magenta}
        Dim i As Integer = 0

        For Each d In donnees
            Dim pIndex = series.Points.AddXY(d.Label, d.Valeur)
            series.Points(pIndex).Color = palette(i Mod palette.Length)
            i += 1
        Next

        chart.Series.Add(series)
        chart.Titles.Add(titre)
        chart.Legends.Add(New Legend("Légende"))
        Return chart
    End Function

    ' --- UTILITAIRES DE DONNÉES PRIVÉS ---

    Private Shared Function ChargerDicoTarifsValides() As Dictionary(Of String, Decimal)
        Return CreateRepository().LireTarifsValides()
    End Function

    Private Shared Sub TraiterLigneSeance(row As SeanceFilmStatRow,
                                      dico As Dictionary(Of Integer, StatFilm),
                                      tarifs As Dictionary(Of String, Decimal))
        Dim idFilm = row.IdFilm
        Dim stat As StatFilm = Nothing

        If Not dico.TryGetValue(idFilm, stat) Then
            stat = New StatFilm With {.IdFilm = idFilm, .Titre = row.TitreFilm}
            dico.Add(idFilm, stat)
        End If

        stat.Seances.Add(New Seance(idFilm,
                                row.DateHeureDebut,
                                row.NbEntreesAdultes,
                                row.NbEntreesEnfants,
                                row.NbEntreesGroupeEnfants,
                                tarifs("Adulte"),
                                tarifs("Enfant"),
                                tarifs("GroupeEnfant")))
    End Sub
    ' --- MÉTHODES PUBLIQUES (ACCESSIBLES DEPUIS L'EXTÉRIEUR) ---

    ''' <summary>
    ''' Récupère les statistiques par film depuis la base de données.
    ''' </summary>
    Public Shared Function GetStatsParFilm() As List(Of StatFilm)
        Dim filmsDict As New Dictionary(Of Integer, StatFilm)

        Try
            Dim tarifs = ChargerDicoTarifsValides()
            Dim lignes = CreateRepository().LireSeancesAvecFilm()

            For Each ligne As SeanceFilmStatRow In lignes
                TraiterLigneSeance(ligne, filmsDict, tarifs)
            Next

        Catch ex As Exception
            Logger.ERR($"Erreur GetStatsParFilm : {ex.Message}")
        End Try

        Return filmsDict.Values.ToList()
    End Function

    ''' <summary>
    ''' Génère le graphique du Chiffre d'Affaires par film.
    ''' </summary>
    Public Shared Function GenererGraphiqueCAParFilm(stats As List(Of StatFilm)) As Chart
        ' Préparation des données simplifiée pour le moteur de rendu
        Dim donnees = stats.Select(Function(s) New With {
            Key .Label = s.Titre,
            Key .Valeur = CDbl(s.CA_Total)
        }).ToList()

        Return ConstruireGraphiqueDeBase("Chiffre d'affaires par film", "Films", donnees)
    End Function
End Class