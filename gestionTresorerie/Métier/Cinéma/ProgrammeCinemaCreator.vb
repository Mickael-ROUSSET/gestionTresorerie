Imports System.IO
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class ProgrammeCinemaCreator
    Private Shared ReadOnly France As New Globalization.CultureInfo("fr-FR")
    Private Shared Function ServiceTmdb() As TMDbService
        Dim cle=Environment.GetEnvironmentVariable("TMDB_API_KEY")
        If String.IsNullOrWhiteSpace(cle) Then cle=LectureProprietes.GetVariable("cleApiTmdb")
        Return If(String.IsNullOrWhiteSpace(cle),Nothing,New TMDbService(cle))
    End Function

    Public Async Function GenererDepuisFichierAsync(cheminFichier As String) As Task
        If Not File.Exists(cheminFichier) Then Return

        ' 1. Chargement et Parsing
        Dim seances = File.ReadAllLines(cheminFichier) _
                          .Select(AddressOf ProgrammeParser.ParseLigne) _
                          .Where(Function(f) f IsNot Nothing) _
                          .ToList()

        ' 2. Enrichissement
        Await EnrichirListeAsync(seances)

        ' 3. Génération Docx
        Dim sortie = LectureProprietes.GetVariable("cheminProgramme")
        Utilitaires.supprimeSiExiste(sortie)
        Await GenererDocxAsync(seances, sortie)
    End Function
    ''' <summary>
    ''' Point d'entrée public pour générer le document Word.
    ''' </summary>
    Public Async Function GenererDocxAsync(seances As List(Of FilmSeance), cheminSortie As String, Optional tarifs As String = "Adulte 6 € / moins de 16 ans 5 €") As Task(Of Boolean)
        Try
            ' On utilise Task.Run pour ne pas figer l'interface pendant la création du XML lourd
            Return Await Task.Run(Function()
                                      Using wordDoc = WordprocessingDocument.Create(cheminSortie, WordprocessingDocumentType.Document)
                                          ' 1. Initialisation
                                          Dim mainPart = wordDoc.AddMainDocumentPart()
                                          mainPart.Document = New Document()
                                          Dim body = mainPart.Document.AppendChild(New Body())

                                          ' 2. Configuration de la page (Paysage A4)
                                          Dim sectionProps = New SectionProperties(
                    New PageSize() With {.Width = 16838, .Height = 11906, .Orient = PageOrientationValues.Landscape},
                    New PageMargin With {.Top=540,.Bottom=540,.Left=650UI,.Right=650UI}
                )


                                          ' 3. Génération des pages (Appel des méthodes que nous avons créées)
                                          CreerPage1(mainPart, body, seances,tarifs)

                                          ' Saut de page propre via le helper
                                          WordHelper.AjouterSautDePage(body)

                                          CreerPage2(mainPart, body, seances)

                                          ' 4. Finalisation
                                          body.Append(sectionProps)
                                          mainPart.Document.Save()
                                      End Using
                                      Return True
                                  End Function)

        Catch ex As Exception
            Logger.ERR($"Erreur fatale GenererDocxAsync : {ex.Message}")
            Return False
        End Try
    End Function
    Friend Async Function EnrichirListeAsync(seances As List(Of FilmSeance)) As Task
        Dim _tmdb=ServiceTmdb()
        If _tmdb Is Nothing Then Return
        Dim tasks = seances.Select(Async Function(s)
                                       Dim id = Await _tmdb.SearchMovieIdByTitleAsync(s.Titre)
                                       If id.HasValue Then
                                           Dim details = Await _tmdb.GetMovieDetailsAsync(id.Value)
                                           s.RemplirDetails(details) ' Extension ou méthode interne à FilmSeance
                                       End If
                                   End Function)
        Await Task.WhenAll(tasks)
    End Function
    Private Shared Sub AjouterContenuTarifs(tc As TableCell, tarifs As String)
        ' Utilisation du helper pour un titre de section
        WordHelper.AjouterParagrapheStyle(tc, "Tarifs : " & tarifs, "34", True, "CE1414")

        ' Ajout d'un espace vide (1 cm)
        tc.Append(New Paragraph(New Run(New Text(""))))

        ' Sous-titre Séances
        WordHelper.AjouterParagrapheStyle(tc, "Séances :", "48", True, "087731")
    End Sub
    ' ... Les méthodes CreerPage1 et CreerPage2 utilisent maintenant WordHelper ...
    Private Shared Sub CreerPage1(mainPart As MainDocumentPart, body As Body, seances As List(Of FilmSeance),tarifs As String)
        ' Création d'un tableau invisible pour simuler les deux colonnes
        Dim table = New Table()
        Dim grid = New TableGrid(
            New GridColumn() With {.Width = "7000"},
            New GridColumn() With {.Width = "5000"}
        )
        table.Append(grid)

        Dim row = New TableRow()

        ' --- Colonne Gauche (Tarifs et Dates) ---
        Dim cellGauche = New TableCell()
        AjouterContenuTarifs(cellGauche,tarifs)
        AjouterListeSeancesPage1(cellGauche, seances)
        row.Append(cellGauche)

        ' --- Colonne Droite (Logos et Titre) ---
        Dim cellDroite = New TableCell()
        AjouterEnteteProgrammation(mainPart, cellDroite, seances)
        row.Append(cellDroite)

        table.Append(row)
        body.Append(table)
    End Sub
    Private Shared Sub CreerPage2(mainPart As MainDocumentPart, body As Body, seances As List(Of FilmSeance))
        If Not seances.Any() Then Return

        ' Séparation propre de la liste en 2 (Complexité 1)
        Dim milieu = CInt(System.Math.Ceiling(seances.Count / 2.0))
        Dim colonnes = {seances.Take(milieu).ToList(), seances.Skip(milieu).ToList()}

        ' Création de la table de rendu
        Dim table = New Table()
        table.Append(New TableGrid(New GridColumn With {.Width="7300"},New GridColumn With {.Width="7300"}))
        Dim row = New TableRow()

        For Each col In colonnes
            Dim cell = New TableCell()
            ' On applique la marge via le helper pour rester propre
            WordHelper.AppliquerMargesCellule(cell, 567)

            ' On remplit la cellule
            For Each film In col
                AjouterBlocFilmDetaille(mainPart, cell, film)
            Next

            If Not col.Any() Then cell.Append(New Paragraph())
            row.Append(cell)
        Next

        table.Append(row)
        body.Append(table)
    End Sub
    Private Shared Sub AjouterEnteteProgrammation(mainPart As MainDocumentPart, tc As TableCell, seances As List(Of FilmSeance))
        ' 1. Logo Principal
        For Each logo In {"logoCinema","logoAgumaaa","logoCinevasion"}
            Dim fichier=Path.Combine(AppContext.BaseDirectory,"Resources",logo & ".png")
            If File.Exists(fichier) Then WordHelper.AjouterImage(mainPart,tc,fichier,1900000,850000)
        Next
        WordHelper.AjouterParagrapheStyle(tc,"Cinéma d’Allègre","38",True,"087731")

        ' 2. Titre "Programmation"
        WordHelper.AjouterParagrapheStyle(tc, "Programmation", "56", True, "087731")

        ' 3. Dates de validité
        If seances.Any() Then
            Dim debut = seances.Min(Function(f) f.DateDiffusion)
            Dim fin = seances.Max(Function(f) f.DateDiffusion)
            WordHelper.AjouterParagrapheStyle(tc, "Du " & debut.ToString("dd MMMM",France) & " au " & fin.ToString("dd MMMM yyyy",France), "40", False)
        End If
        WordHelper.AjouterParagrapheStyle(tc,"agumaaa.jimdofree.com   •   cinevasion43.wix.com/site","20",False)
    End Sub
    Private Shared Sub AjouterListeSeancesPage1(tc As TableCell, seances As List(Of FilmSeance))
        For Each s In seances
            ' Date et Heure
            Dim dateTexte = s.DateDiffusion.ToString("dddd dd MMMM",France) & " " & s.HeureDiffusion.ToString("hh\hmm")
            WordHelper.AjouterParagrapheStyle(tc, dateTexte, "28", True)

            ' Titre du film
            WordHelper.AjouterParagrapheStyle(tc, s.Titre, "28", True, "087731")

            ' Petit espace entre les films
            tc.Append(New Paragraph(New Run(New Text(""))))
        Next
    End Sub
    Private Shared Sub AjouterBlocFilmDetaille(mainPart As MainDocumentPart, cell As TableCell, film As FilmSeance)
        ' 1. Affiche
        If Not String.IsNullOrEmpty(film.UrlAffiche) Then
            WordHelper.AjouterImage(mainPart, cell, film.UrlAffiche, 800000, 1150000)
        End If

        ' 2. Titre (Utilise le helper de style créé précédemment)
        WordHelper.AjouterParagrapheStyle(cell, film.Titre, "32", True, "087731")

        ' 3. Infos techniques
        Dim infos As New List(Of String)
        If Not String.IsNullOrWhiteSpace(film.Genre) Then infos.Add(film.Genre)
        If Not String.IsNullOrWhiteSpace(film.Pays) Then infos.Add(film.Pays)
        If film.DureeMinutes.HasValue Then infos.Add(film.DureeMinutes.Value.ToString() & " min")
        If infos.Count>0 Then WordHelper.AjouterParagrapheStyle(cell,String.Join("  •  ",infos),"21",True)
        If Not String.IsNullOrWhiteSpace(film.Realisateur) Then WordHelper.AjouterParagrapheStyle(cell,"Réalisation : " & film.Realisateur,"20")
        If film.Casting IsNot Nothing AndAlso film.Casting.Count>0 Then WordHelper.AjouterParagrapheStyle(cell,"Avec " & String.Join(", ",film.Casting),"20")
        If Not String.IsNullOrWhiteSpace(film.Synopsis) Then WordHelper.AjouterParagrapheStyle(cell,film.Synopsis,"20")

        ' 5. Espacement
        cell.Append(New Paragraph(New Run(New Text(""))))
    End Sub
    Public Async Function ChargerProgramme(cheminFichier As String) As Task
        Dim liste As New List(Of FilmSeance)()
        If Not File.Exists(cheminFichier) Then Exit Function

        For Each ligne In File.ReadAllLines(cheminFichier)
            Dim f = ParseLigne(ligne)
            If f IsNot Nothing Then liste.Add(f)
        Next
        Await EnrichirListeAsync(liste)
        Dim sNomProgramme As String = LectureProprietes.GetVariable("cheminProgramme")
        Utilitaires.supprimeSiExiste(sNomProgramme)
        Await GenererDocxAsync(liste, sNomProgramme)
    End Function
    Private Function ParseLigne(ligne As String) As FilmSeance
        Dim sep() As Char = {";"c, "|"c}
        Dim tokens = ligne.Split(sep, StringSplitOptions.RemoveEmptyEntries).Select(Function(s) s.Trim()).ToArray()
        If tokens.Length < 3 Then
            Logger.ERR($"Ligne ignorée (moins de 3 tokens) : '{ligne}'")
            Return Nothing
        End If

        Dim titre = tokens(0)
        Dim datePart = tokens(1)
        Dim timePart = tokens(2)

        Dim d As Date
        If Not Date.TryParse(datePart, d) Then
            Logger.ERR($"Erreur parsing date : '{datePart}' dans la ligne '{ligne}'")
            Return Nothing
        End If

        Dim ts As TimeSpan
        Dim normalizedTime = timePart.Replace("h", ":")
        If Not TimeSpan.TryParse(normalizedTime, ts) Then
            Logger.ERR($"Erreur parsing heure : '{timePart}' (converti en '{normalizedTime}') dans la ligne '{ligne}'")
            Return Nothing
        End If

        Return New FilmSeance With {.Titre = titre, .DateDiffusion = d, .HeureDiffusion = ts}
    End Function
End Class