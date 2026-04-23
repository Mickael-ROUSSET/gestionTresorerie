Imports System.Data.SqlClient
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Drawing

''' <summary>
''' Classe centralisée pour l'analyse des données et la génération de graphiques.
''' </summary>
<DebuggerDisplay("Gestionnaire de Statistiques")>
Public NotInheritable Class StatsCinema
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
            ' On utilise la constante SQL définie dans ton référentiel
            Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.DataBases.Cinema, "selStatsParMois")
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim mois = rdr("Mois").ToString()
                        dictMois(mois) = CDec(rdr("CA_Total"))
                    End While
                End Using
            End Using
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
        Dim dico As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.DataBases.Cinema, "selTarifsValides")
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    dico(rdr("NomTarif").ToString()) = CDec(rdr("Montant"))
                End While
            End Using
        End Using
        Return dico
    End Function

    Private Shared Sub TraiterLigneSeance(rdr As SqlDataReader, dico As Dictionary(Of Integer, StatFilm), tarifs As Dictionary(Of String, Decimal))
        Dim idFilm = rdr.GetValueOrDefault(Of Integer)("IdFilm")
        Dim stat As StatFilm = Nothing

        If Not dico.TryGetValue(idFilm, stat) Then
            stat = New StatFilm With {.IdFilm = idFilm, .Titre = rdr.GetValueOrDefault(Of String)("TitreFilm")}
            dico.Add(idFilm, stat)
        End If

        ' Le calcul du CA se fait souvent dans le constructeur de Seance
        stat.Seances.Add(New Seance(idFilm, rdr.GetValueOrDefault(Of DateTime)("DateHeureDebut"),
                         rdr.GetValueOrDefault(Of Integer)("NbEntreesAdultes", 0),
                         rdr.GetValueOrDefault(Of Integer)("NbEntreesEnfants", 0),
                         rdr.GetValueOrDefault(Of Integer)("NbEntreesGroupeEnfants", 0),
                         tarifs("Adulte"), tarifs("Enfant"), tarifs("GroupeEnfant")))
    End Sub
    ' --- MÉTHODES PUBLIQUES (ACCESSIBLES DEPUIS L'EXTÉRIEUR) ---

    ''' <summary>
    ''' Récupère les statistiques par film depuis la base de données.
    ''' </summary>
    Public Shared Function GetStatsParFilm() As List(Of StatFilm)
        Dim filmsDict As New Dictionary(Of Integer, StatFilm)
        Try
            Dim tarifs = ChargerDicoTarifsValides()
            Using cmdSeances = SqlCommandBuilder.CreateSqlCommand(Constantes.DataBases.Cinema, Constantes.Sql.Selection.SeancesAvecFilm)
                Using rdr = cmdSeances.ExecuteReader()
                    While rdr.Read()
                        TraiterLigneSeance(rdr, filmsDict, tarifs)
                    End While
                End Using
            End Using
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