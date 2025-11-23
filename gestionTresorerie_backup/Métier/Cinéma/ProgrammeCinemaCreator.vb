' ProgrammeCinemaCreator.vb
Imports System.IO
Imports System.Threading.Tasks
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports Drawing = DocumentFormat.OpenXml.Drawing

Public Class ProgrammeCinemaCreator
    Private ReadOnly _tmdb As TMDbService

    Public Sub New()
        Dim apiKey = LectureProprietes.GetVariable("cleApiTmdb")
        _tmdb = New TMDbService(apiKey)
    End Sub

    ' Lecture du programme à partir d'un fichier texte
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


    ' Enrichissement via TMDb
    Public Async Function EnrichirListeAsync(seances As List(Of FilmSeance)) As Task
        Dim sem As New Threading.SemaphoreSlim(4)
        Dim tasks As New List(Of Task)()

        For Each s In seances
            Await sem.WaitAsync()
            Dim t = Task.Run(Async Function()
                                 Try
                                     Dim f = Await _tmdb.GetMovieDetailsAsync((Await _tmdb.SearchMovieIdByTitleAsync(s.Titre)).Value)
                                     If f Is Nothing Then Return
                                     s.Synopsis = f.Synopsis
                                     s.DureeMinutes = f.DureeMinutes
                                     s.Genre = f.Genre
                                     s.Realisateur = f.Realisateur
                                     s.Casting = f.Casting
                                     s.UrlAffiche = f.UrlAffiche
                                 Finally
                                     sem.Release()
                                 End Try
                             End Function)
            tasks.Add(t)
        Next

        Await Task.WhenAll(tasks)
    End Function

    ' Génération Word
    Public Async Function GenererDocxAsync(seances As List(Of FilmSeance), cheminSortie As String) As Task(Of Boolean)
        Using wordDoc = WordprocessingDocument.Create(cheminSortie, DocumentFormat.OpenXml.WordprocessingDocumentType.Document)
            Dim mainPart = wordDoc.AddMainDocumentPart()
            mainPart.Document = New Document()
            Dim body = mainPart.Document.AppendChild(New Body())

            ' Page 1
            CreerPage1(mainPart, body, seances)

            ' --- Saut de page avant page 2 ---
            'body.Append(New Paragraph(New Run(New Break() With {.Type = BreakValues.Page})))

            ' Page 2 avec la liste des films et leurs affiches
            CreerPage2(mainPart, body, seances)

            mainPart.Document.Save()
        End Using
    End Function
    ''' <summary>
    ''' Crée la première page : colonnes avec tarifs, séances et logos
    ''' </summary>
    Private Sub CreerPage1(mainPart As MainDocumentPart, body As Body, seances As List(Of FilmSeance))
        ' Table pour simuler les colonnes
        Dim table = New Table()
        table.Append(New TableProperties(New TableWidth() With {.Type = TableWidthUnitValues.Pct, .Width = "5000"}))
        table.Append(New TableGrid(New GridColumn() With {.Width = "5000"}, New GridColumn() With {.Width = "5000"}))

        Dim tr = New TableRow()

        ' --- Colonne gauche : tarifs et séances ---
        Dim tcGauche = New TableCell()
        AjouterTarifsEtSeances(tcGauche, seances)
        'Pieds de page
        AjouterPiedDePagePage1(mainPart, tcGauche)
        tr.Append(tcGauche)

        ' --- Colonne droite : logos et programmation ---
        Dim tcDroite = New TableCell()
        AjouterLogosEtProgrammation(mainPart, tcDroite, seances)
        tr.Append(tcDroite)

        table.Append(tr)
        body.Append(table)
    End Sub
    Private Sub AjouterTarifsEtSeances(tc As TableCell, seances As List(Of FilmSeance))
        ' Tarifs
        tc.Append(New Paragraph(New Run(New Text("Tarifs : adulte 6€ / -16 ans : 5€")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "40"})})) ' 20pt

        ' Séances
        tc.Append(New Paragraph(New Run(New Text("Séances :")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "48"})})) ' 24pt

        ' Liste des films
        For Each s In seances
            tc.Append(New Paragraph(New Run(New Text($"{s.DateDiffusion:dd/MM} {s.HeureDiffusion:hh\:mm}")) With {
                .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})})) ' 14pt
            tc.Append(New Paragraph(New Run(New Text(s.Titre)) With {
                .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})})) ' 14pt
        Next
    End Sub
    Private Sub AjouterLogosEtProgrammation(mainPart As MainDocumentPart, tc As TableCell, seances As List(Of FilmSeance))
        ' Logos
        Dim logoAgumaaaPath = LectureProprietes.GetVariable("logoAgumaaa")
        Dim logoCinemaPath = LectureProprietes.GetVariable("logoCinema")
        AjouterImageDansCellule(mainPart, tc, logoAgumaaaPath, 1000000, 1000000)
        AjouterImageDansCellule(mainPart, tc, logoCinemaPath, 3000000, 1600000)

        ' Titre "Programmation"
        tc.Append(New Paragraph(New Run(New Text("Programmation")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "56"})})) ' 28pt

        ' Dates du 1er et dernier film
        If seances.Count > 0 Then
            Dim dateDebut = seances.Min(Function(f) f.DateDiffusion)
            Dim dateFin = seances.Max(Function(f) f.DateDiffusion)
            tc.Append(New Paragraph(New Run(New Text($"Du {dateDebut:dd MMMM} au {dateFin:dd MMMM}")) With {
                .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "40"})})) ' 20pt
        End If
    End Sub

    Private Sub AjouterImageDansCellule(mainPart As MainDocumentPart, cellule As TableCell, imagePath As String, Optional widthEmu As Long = 3500000, Optional heightEmu As Long = 2000000)
        Dim imagePart = mainPart.AddImagePart(DocumentFormat.OpenXml.Packaging.ImagePartType.Png)
        Using stream = New IO.FileStream(imagePath, IO.FileMode.Open)
            imagePart.FeedData(stream)
        End Using
        Dim drawing = CreerDrawing(mainPart.GetIdOfPart(imagePart), widthEmu, heightEmu)
        cellule.Append(New Paragraph(New Run(drawing)))
    End Sub

    Private Function CreerDrawing(imagePartId As String, widthEmu As Long, heightEmu As Long) As DocumentFormat.OpenXml.Wordprocessing.Drawing
        Dim picture = New Drawing.Pictures.Picture(
            New Drawing.Pictures.NonVisualPictureProperties(
                New Drawing.Pictures.NonVisualDrawingProperties() With {.Id = CType(0UI, UInt32), .Name = "Image"},
                New Drawing.Pictures.NonVisualPictureDrawingProperties()
            ),
            New Drawing.Pictures.BlipFill(
                New Drawing.Blip() With {.Embed = imagePartId},
                New Drawing.Stretch(New Drawing.FillRectangle())
            ),
            New Drawing.Pictures.ShapeProperties()
        )
        Dim graphicData = New Drawing.GraphicData(picture)
        graphicData.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
        Dim graphic = New Drawing.Graphic(graphicData)
        Dim inline = New Drawing.Wordprocessing.Inline(
            New Drawing.Wordprocessing.Extent() With {.Cx = widthEmu, .Cy = heightEmu},
            New Drawing.Wordprocessing.EffectExtent() With {.LeftEdge = 0L, .TopEdge = 0L, .RightEdge = 0L, .BottomEdge = 0L},
            New Drawing.Wordprocessing.DocProperties() With {.Id = CType(1UI, UInt32), .Name = "Image"},
            New Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                New Drawing.GraphicFrameLocks() With {.NoChangeAspect = True}
            ),
            graphic
        )
        Return New DocumentFormat.OpenXml.Wordprocessing.Drawing(inline)
    End Function
    Public Sub GenererProgramme(cheminFichier As String, seances As List(Of FilmSeance))
        Using wordDoc = WordprocessingDocument.Create(cheminFichier, DocumentFormat.OpenXml.WordprocessingDocumentType.Document)
            Dim mainPart = wordDoc.AddMainDocumentPart()
            mainPart.Document = New Document()
            Dim body = mainPart.Document.AppendChild(New Body())

            ' Mettre la page en paysage avec colonnes
            Dim sectionProps = New SectionProperties(
                New PageSize() With {.Width = 16838, .Height = 11906, .Orient = PageOrientationValues.Landscape},
                New Columns() With {.Space = "708"}
            )
            body.Append(sectionProps)

            ' --- Création de la page 1 ---
            CreerPage1(mainPart, body, seances)

            mainPart.Document.Save()
        End Using
    End Sub
    ''' <summary>
    ''' Ajoute une liste de films avec date/heure, titre et affiche dans une cellule de tableau
    ''' </summary>
    Private Sub AjouterFilmsDansCellule(mainPart As MainDocumentPart, cellule As TableCell, films As List(Of FilmSeance))
        For Each s In films
            ' Date et heure 14pt
            cellule.Append(New Paragraph(New Run(New Text($"{s.DateDiffusion:dd/MM} {s.HeureDiffusion:hh\:mm}")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})})) ' 14pt

            ' Titre 14pt
            cellule.Append(New Paragraph(New Run(New Text(s.Titre)) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})}))

            ' Affiche si existe
            If Not String.IsNullOrEmpty(s.UrlAffiche) AndAlso IO.File.Exists(s.UrlAffiche) Then
                AjouterImageDansCellule(mainPart, cellule, s.UrlAffiche, 2000000, 3000000) ' largeur et hauteur EMU (ajuster selon besoin)
            End If
        Next
    End Sub
    ''' <summary>
    ''' Crée la page 2 avec la liste des films et leurs affiches sur 2 colonnes
    ''' </summary>
    Private Sub CreerPage2(mainPart As MainDocumentPart, body As Body, seances As List(Of FilmSeance))
        If seances.Count = 0 Then Return

        ' Saut de page pour séparer de la page 1
        body.Append(New Paragraph(New Run(New Break() With {.Type = BreakValues.Page})))

        ' Section paysage et colonnes avec espace de 2 cm (en twentieths of a point = 567 Twips par cm)
        Dim sectionProps = New SectionProperties(
                New PageSize() With {.Width = 16838, .Height = 11906, .Orient = PageOrientationValues.Landscape},
                New Columns() With {.Space = (2 * 567).ToString()} ' 2 cm
                )
        body.Append(sectionProps)

        ' Table à 2 colonnes pour la mise en page
        Dim table = New Table()
        table.Append(New TableProperties(New TableWidth() With {.Type = TableWidthUnitValues.Pct, .Width = "5000"}))
        table.Append(New TableGrid(New GridColumn() With {.Width = "5000"}, New GridColumn() With {.Width = "5000"}))

        Dim tr = New TableRow()
        Dim tcLeft = New TableCell()
        Dim tcRight = New TableCell()

        ' Séparer la liste des films en deux moitiés
        Dim mid As Integer = CInt(System.Math.Ceiling(seances.Count / 2.0))
        Dim leftList = seances.Take(mid).ToList()
        Dim rightList = seances.Skip(mid).ToList()

        ' Ajouter les films dans chaque colonne
        AjouterFilmsDetailDansCellule(mainPart, tcLeft, leftList)
        AjouterFilmsDetailDansCellule(mainPart, tcRight, rightList)

        tr.Append(tcLeft)
        tr.Append(tcRight)
        table.Append(tr)
        body.Append(table)
    End Sub


    Private Sub AjouterFilmsDetailDansCellule(mainPart As MainDocumentPart, cellule As TableCell, films As List(Of FilmSeance))
        For Each s In films
            ' --- Affiche ---
            If Not String.IsNullOrEmpty(s.UrlAffiche) Then
                AjouterImageDepuisUrl(mainPart, cellule, s.UrlAffiche, 2000000, 3000000)
            ElseIf Not String.IsNullOrEmpty(s.UrlAffiche) AndAlso IO.File.Exists(s.UrlAffiche) Then
                AjouterImageDansCellule(mainPart, cellule, s.UrlAffiche, 2000000, 3000000)
            End If

            ' --- Titre en gras ---
            cellule.Append(New Paragraph(New Run(New Text(s.Titre)) With {
            .RunProperties = New RunProperties(
                New RunFonts() With {.Ascii = "Arial"},
                New FontSize() With {.Val = "32"}, ' 16pt
                New Bold()
            )
        }))

            ' --- Détails ---
            ' Genre
            cellule.Append(New Paragraph(New Run(New Text($"Genre : {s.Genre}")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})
        }))

            ' Réalisateur
            cellule.Append(New Paragraph(New Run(New Text($"Réalisateur : {s.Realisateur}")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})
        }))

            ' Acteurs
            Dim acteursTexte As String = If(s.Casting IsNot Nothing, String.Join(", ", s.Casting), "")
            cellule.Append(New Paragraph(New Run(New Text($"Acteurs : {acteursTexte}")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})
        }))

            '    ' Pays
            '    Dim paysTexte As String = If(s.Pays IsNot Nothing, String.Join(", ", s.Pays), "")
            '    cellule.Append(New Paragraph(New Run(New Text($"Pays : {paysTexte}")) With {
            '    .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})
            '}))

            ' Durée
            cellule.Append(New Paragraph(New Run(New Text($"Durée : {s.DureeMinutes}")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})
        }))

            ' Synopsis
            cellule.Append(New Paragraph(New Run(New Text($"Synopsis : {s.Synopsis}")) With {
            .RunProperties = New RunProperties(New RunFonts() With {.Ascii = "Arial"}, New FontSize() With {.Val = "28"})
        }))

            ' Espace après chaque film
            cellule.Append(New Paragraph(New Run(New Text(" "))))
        Next
    End Sub

    Private Sub AjouterImageDepuisUrl(mainPart As MainDocumentPart, cellule As TableCell, url As String, Optional widthEmu As Long = 2000000, Optional heightEmu As Long = 3000000)
        Try
            ' Récupérer l'image depuis Internet
            Dim bytes() As Byte
            Using client As New Net.Http.HttpClient()
                bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult()
            End Using

            ' Ajouter l'image à mainPart
            Dim imagePart = mainPart.AddImagePart(DocumentFormat.OpenXml.Packaging.ImagePartType.Jpeg)
            Using stream As New IO.MemoryStream(bytes)
                imagePart.FeedData(stream)
            End Using

            ' Créer le Drawing et l'ajouter
            Dim drawing = CreerDrawing(mainPart.GetIdOfPart(imagePart), widthEmu, heightEmu)
            cellule.Append(New Paragraph(New Run(drawing)))

        Catch ex As Exception
            Logger.ERR($"Impossible de charger l'image depuis l'URL '{url}' : {ex.Message}")
        End Try
    End Sub


    Private Sub AddImageToBody(imagePartId As String, body As Body)
        ' Créer la Picture
        Dim picture = New Drawing.Pictures.Picture(
        New Drawing.Pictures.NonVisualPictureProperties(
            New Drawing.Pictures.NonVisualDrawingProperties() With {.Id = CType(0UI, UInt32), .Name = "Image"},
            New Drawing.Pictures.NonVisualPictureDrawingProperties()
        ),
        New Drawing.Pictures.BlipFill(
            New Drawing.Blip() With {.Embed = imagePartId},
            New Drawing.Stretch(New Drawing.FillRectangle())
        ),
        New Drawing.Pictures.ShapeProperties()
    )

        ' Créer GraphicData et assigner l'Uri
        Dim graphicData = New Drawing.GraphicData(picture)
        graphicData.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"

        ' Créer Graphic
        Dim graphic = New Drawing.Graphic(graphicData)

        ' Créer Inline
        Dim inline = New Drawing.Wordprocessing.Inline(
        New Drawing.Wordprocessing.Extent() With {.Cx = 3500000L, .Cy = 2000000L},
        New Drawing.Wordprocessing.EffectExtent() With {.LeftEdge = 0L, .TopEdge = 0L, .RightEdge = 0L, .BottomEdge = 0L},
        New Drawing.Wordprocessing.DocProperties() With {.Id = CType(1UI, UInt32), .Name = "Picture"},
        New Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
            New Drawing.GraphicFrameLocks() With {.NoChangeAspect = True}
        ),
        graphic
    )

        ' ⚠️ Ici utiliser DocumentFormat.OpenXml.Wordprocessing.Drawing
        Dim element = New DocumentFormat.OpenXml.Wordprocessing.Drawing(inline)

        ' Ajouter au paragraphe
        Dim para = New Paragraph(New Run(element))
        body.Append(para)
    End Sub
    ''' <summary>
    ''' Ajoute le pied de page en bas de la colonne gauche de la page 1
    ''' </summary>
    Private Sub AjouterPiedDePagePage1(mainPart As MainDocumentPart, tc As TableCell)

        ' --- Espacement avant (pour pousser vers le bas)
        tc.Append(New Paragraph(New Run(New Text(" "))))

        ' =============================
        ' 1) Ligne : Ne pas jeter...
        ' =============================
        Dim p1 As New Paragraph(
        New Run(
            New Text("Ne pas jeter sur la voie publique")
        )
    )
        p1.ParagraphProperties = New ParagraphProperties(
        New Justification() With {.Val = JustificationValues.Left},
        New SpacingBetweenLines() With {.Before = "200", .After = "100"}
    )
        tc.Append(p1)

        ' =============================
        ' 2) Lignes des sites
        ' =============================
        Dim p2 As New Paragraph(
        New Run(
            New Text("agumaaa.jimdofree.com    cinevasion43.wix.com/site")
        )
    )
        p2.ParagraphProperties = New ParagraphProperties(
        New Justification() With {.Val = JustificationValues.Left},
        New SpacingBetweenLines() With {.Before = "0", .After = "200"}
    )
        tc.Append(p2)

        ' =============================
        ' 3) Logo Cinevasion
        ' =============================
        Dim urlLogo = LectureProprietes.GetVariable("logoCinevasion")
        If String.IsNullOrEmpty(urlLogo) Then Exit Sub

        Try
            Dim data As Byte()

            ' Téléchargement de l’image
            Using client As New Net.Http.HttpClient()
                data = client.GetByteArrayAsync(urlLogo).Result
            End Using

            ' Création de la part image
            Dim imagePart = mainPart.AddImagePart(ImagePartType.Png)
            Using s As New IO.MemoryStream(data)
                imagePart.FeedData(s)
            End Using

            ' Construction du paragraphe contenant l'image
            Dim imagePara = CreateImageParagraph(mainPart.GetIdOfPart(imagePart), 1600000, 500000)
            tc.Append(imagePara)

        Catch ex As Exception
            Logger.ERR($"Erreur chargement logoCinevasion : {ex.Message}")
        End Try

    End Sub

End Class
