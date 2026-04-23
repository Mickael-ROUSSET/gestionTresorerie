Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports Drawing = DocumentFormat.OpenXml.Drawing

Public NotInheritable Class WordHelper
    ''' <summary>
    ''' Ajoute une image (locale ou URL) dans un conteneur Word avec centrage.
    ''' </summary>
    Public Shared Sub AjouterImage(mainPart As MainDocumentPart, container As OpenXmlCompositeElement, pathOrUrl As String, widthEmu As Long, heightEmu As Long)
        Try
            Dim bytes() As Byte = RecupererBytesImage(pathOrUrl)
            If bytes Is Nothing Then Return

            Dim imagePart = mainPart.AddImagePart(ImagePartType.Jpeg)
            Using ms As New IO.MemoryStream(bytes)
                imagePart.FeedData(ms)
            End Using

            Dim drawing = CreerDrawing(mainPart.GetIdOfPart(imagePart), widthEmu, heightEmu)

            ' Paragraphe avec justification centrée
            Dim p = New Paragraph(
                New ParagraphProperties(New Justification() With {.Val = JustificationValues.Center}),
                New Run(drawing)
            )
            container.Append(p)
        Catch ex As Exception
            Logger.ERR($"Erreur image WordHelper : {ex.Message}")
        End Try
    End Sub

    Private Shared Function RecupererBytesImage(source As String) As Byte()
        If source.StartsWith("http") Then
            Using client As New Net.Http.HttpClient()
                Return client.GetByteArrayAsync(source).GetAwaiter().GetResult()
            End Using
        ElseIf IO.File.Exists(source) Then
            Return IO.File.ReadAllBytes(source)
        End If
        Return Nothing
    End Function

    Private Shared Function CreerDrawing(id As String, w As Long, h As Long) As DocumentFormat.OpenXml.Wordprocessing.Drawing
        ' Construction de la structure XML complexe requise par Word
        Dim pic = New Drawing.Pictures.Picture(
            New Drawing.Pictures.NonVisualPictureProperties(
                New Drawing.Pictures.NonVisualDrawingProperties() With {.Id = 0UI, .Name = "Img"},
                New Drawing.Pictures.NonVisualPictureDrawingProperties()
            ),
            New Drawing.BlipFill(
                New Drawing.Blip() With {.Embed = id},
                New Drawing.Stretch(New Drawing.FillRectangle())
            ),
            New Drawing.Pictures.ShapeProperties()
        )

        Return New DocumentFormat.OpenXml.Wordprocessing.Drawing(
            New Drawing.Wordprocessing.Inline(
                New Drawing.Wordprocessing.Extent() With {.Cx = w, .Cy = h},
                New Drawing.Wordprocessing.DocProperties() With {.Id = 1UI, .Name = "Picture"},
                New Drawing.Graphic(New Drawing.GraphicData(pic) With {.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"})
            )
        )
    End Function

    Public Shared Sub AppliquerMargesCellule(cell As TableCell, dxaValue As Integer)
        cell.TableCellProperties = New TableCellProperties(
            New TableCellMargin(
                New RightMargin() With {.Width = dxaValue.ToString(), .Type = TableWidthUnitValues.Dxa}
            )
        )
    End Sub
    ''' <summary>
    ''' Crée et ajoute un paragraphe stylisé à un conteneur (Cellule ou Corps).
    ''' </summary>
    ''' <param name="container">L'élément parent (TableCell ou Body).</param>
    ''' <param name="texte">Le texte à afficher.</param>
    ''' <param name="fontSize">La taille (en demi-points, ex: "28" pour 14pt).</param>
    ''' <param name="bold">True pour mettre en gras.</param>
    ''' <param name="colorHex">Couleur au format Hexadécimal (ex: "1F4E79").</param>
    Public Shared Sub AjouterParagrapheStyle(container As OpenXmlCompositeElement, texte As String, fontSize As String, Optional bold As Boolean = False, Optional colorHex As String = "000000")
        ' 1. Création du texte et de la zone de style (Run)
        Dim runText As New Text(texte)
        Dim run As New Run(runText)

        ' 2. Définition des propriétés de style
        Dim rp As New RunProperties()
        rp.Append(New RunFonts() With {.Ascii = "Arial"})
        rp.Append(New FontSize() With {.Val = fontSize})

        If bold Then
            rp.Append(New Bold())
        End If

        rp.Append(New Color() With {.Val = colorHex})

        ' 3. Assignation du style au texte
        run.RunProperties = rp

        ' 4. Ajout dans un nouveau paragraphe
        Dim p As New Paragraph(run)
        container.Append(p)
    End Sub
    ''' <summary>
    ''' Insère un saut de page propre dans le corps du document Word.
    ''' </summary>
    Public Shared Sub AjouterSautDePage(body As DocumentFormat.OpenXml.Wordprocessing.Body)
        ' On crée un paragraphe contenant un break de type "Page"
        Dim breakNode = New DocumentFormat.OpenXml.Wordprocessing.Break() With {
            .Type = DocumentFormat.OpenXml.Wordprocessing.BreakValues.Page
        }

        Dim run = New DocumentFormat.OpenXml.Wordprocessing.Run(breakNode)
        Dim para = New DocumentFormat.OpenXml.Wordprocessing.Paragraph(run)

        body.Append(para)
    End Sub
End Class