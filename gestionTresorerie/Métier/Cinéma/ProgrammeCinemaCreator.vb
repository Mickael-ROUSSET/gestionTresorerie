Imports System.IO
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class ProgrammeCinemaCreator
    Private ReadOnly _tmdb As TMDbService

    Public Sub New()
        _tmdb = New TMDbService(LectureProprietes.GetVariable("cleApiTmdb"))
    End Sub

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
    Public Async Function GenererDocxAsync(seances As List(Of FilmSeance), cheminSortie As String) As Task(Of Boolean)
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
                    New PageSize() With {.Width = 16838, .Height = 11906, .Orient = PageOrientationValues.Landscape}
                )
                                          body.Append(sectionProps)

                                          ' 3. Génération des pages (Appel des méthodes que nous avons créées)
                                          CreerPage1(mainPart, body, seances)

                                          ' Saut de page propre via le helper
                                          WordHelper.AjouterSautDePage(body)

                                          CreerPage2(mainPart, body, seances)

                                          ' 4. Finalisation
                                          mainPart.Document.Save()
                                      End Using
                                      Return True
                                  End Function)

        Catch ex As Exception
            Logger.ERR($"Erreur fatale GenererDocxAsync : {ex.Message}")
            Return False
        End Try
    End Function
    Private Async Function EnrichirListeAsync(seances As List(Of FilmSeance)) As Task
        ' Utilisation d'un vrai parallélisme contrôlé
        Dim tasks = seances.Select(Async Function(s)
                                       Dim id = Await _tmdb.SearchMovieIdByTitleAsync(s.Titre)
                                       If id.HasValue Then
                                           Dim details = Await _tmdb.GetMovieDetailsAsync(id.Value)
                                           s.RemplirDetails(details) ' Extension ou méthode interne à FilmSeance
                                       End If
                                   End Function)
        Await Task.WhenAll(tasks)
    End Function
    Private Shared Sub AjouterContenuTarifs(tc As TableCell)
        ' Utilisation du helper pour un titre de section
        WordHelper.AjouterParagrapheStyle(tc, "Tarifs : adulte 6€ / -16 ans : 5€", "40", True, "1F4E79")

        ' Ajout d'un espace vide (1 cm)
        tc.Append(New Paragraph(New Run(New Text(""))))

        ' Sous-titre Séances
        WordHelper.AjouterParagrapheStyle(tc, "Séances :", "48", True, "1F4E79")
    End Sub
    ' ... Les méthodes CreerPage1 et CreerPage2 utilisent maintenant WordHelper ...
    Private Shared Sub CreerPage1(mainPart As MainDocumentPart, body As Body, seances As List(Of FilmSeance))
        ' Création d'un tableau invisible pour simuler les deux colonnes
        Dim table = New Table()
        Dim grid = New TableGrid(
            New GridColumn() With {.Width = "5000"},
            New GridColumn() With {.Width = "5000"}
        )
        table.Append(grid)

        Dim row = New TableRow()

        ' --- Colonne Gauche (Tarifs et Dates) ---
        Dim cellGauche = New TableCell()
        AjouterContenuTarifs(cellGauche)
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
        Dim row = New TableRow()

        For Each col In colonnes
            Dim cell = New TableCell()
            ' On applique la marge via le helper pour rester propre
            WordHelper.AppliquerMargesCellule(cell, 567)

            ' On remplit la cellule
            For Each film In col
                AjouterBlocFilmDetaille(mainPart, cell, film)
            Next

            row.Append(cell)
        Next

        table.Append(row)
        body.Append(table)
    End Sub
    Private Shared Sub AjouterEnteteProgrammation(mainPart As MainDocumentPart, tc As TableCell, seances As List(Of FilmSeance))
        ' 1. Logo Principal
        Dim pathLogo = LectureProprietes.GetVariable("logoAgumaaa")
        If IO.File.Exists(pathLogo) Then
            WordHelper.AjouterImage(mainPart, tc, pathLogo, 2400000, 1200000)
        End If

        ' 2. Titre "Programmation"
        WordHelper.AjouterParagrapheStyle(tc, "Programmation", "56", True, "1F4E79")

        ' 3. Dates de validité
        If seances.Any() Then
            Dim debut = seances.Min(Function(f) f.DateDiffusion)
            Dim fin = seances.Max(Function(f) f.DateDiffusion)
            WordHelper.AjouterParagrapheStyle(tc, $"Du {debut:dd MMMM} au {fin:dd MMMM}", "40", False)
        End If
    End Sub
    Private Shared Sub AjouterListeSeancesPage1(tc As TableCell, seances As List(Of FilmSeance))
        For Each s In seances
            ' Date et Heure
            Dim dateTexte = $"{s.DateDiffusion:dddd dd MMMM} {s.HeureDiffusion:hh\:mm}"
            WordHelper.AjouterParagrapheStyle(tc, dateTexte, "28", True)

            ' Titre du film
            WordHelper.AjouterParagrapheStyle(tc, s.Titre, "28", True, "1F4E79")

            ' Petit espace entre les films
            tc.Append(New Paragraph(New Run(New Text(""))))
        Next
    End Sub
    Private Shared Sub AjouterBlocFilmDetaille(mainPart As MainDocumentPart, cell As TableCell, film As FilmSeance)
        ' 1. Affiche
        If Not String.IsNullOrEmpty(film.UrlAffiche) Then
            WordHelper.AjouterImage(mainPart, cell, film.UrlAffiche, 1800000, 2500000)
        End If

        ' 2. Titre (Utilise le helper de style créé précédemment)
        WordHelper.AjouterParagrapheStyle(cell, film.Titre, "32", True, "2E74B5")

        ' 3. Infos techniques
        Dim infos = $"Genre: {film.Genre} | Durée: {film.DureeMinutes} min"
        WordHelper.AjouterParagrapheStyle(cell, infos, "24", True)

        ' 4. Synopsis
        WordHelper.AjouterParagrapheStyle(cell, film.Synopsis, "20", False)

        ' 5. Espacement
        cell.Append(New Paragraph(New Run(New Text(""))))
    End Sub

End Class