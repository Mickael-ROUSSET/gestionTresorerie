Imports System.IO
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports A = DocumentFormat.OpenXml.Drawing
Imports BottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder
Imports DW = DocumentFormat.OpenXml.Drawing.Wordprocessing
Imports LeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder
Imports PIC = DocumentFormat.OpenXml.Drawing.Pictures
Imports RightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder
Imports Run = DocumentFormat.OpenXml.Wordprocessing.Run
Imports Table = DocumentFormat.OpenXml.Wordprocessing.Table
Imports Text = DocumentFormat.OpenXml.Wordprocessing.Text
Imports TopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder


Module creeOpenXml
    'https://learn.microsoft.com/fr-fr/office/open-xml/word/overview


    Public Sub creeDoc(ByVal filepath As String)
        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-create-a-word-processing-document-by-providing-a-file-name?tabs=vb-0%2Ccs-1%2Ccs-2%2Cvb
        ' Create a document by supplying the filepath.
        Using wordDocument As WordprocessingDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document)

            ' Add a main document part. 
            Dim mainPart As MainDocumentPart = wordDocument.AddMainDocumentPart()

            ' Create the document structure and add some text.
            mainPart.Document = New Document()
            Dim body As Body = mainPart.Document.AppendChild(New Body())
            Dim para As Paragraph = body.AppendChild(New Paragraph())
            Dim run As Run = para.AppendChild(New Run())
            run.AppendChild(New Text("Create text in body - CreateWordprocessingDocument"))
        End Using
    End Sub
    ' Take the data from a two-dimensional array and build a table at the 
    ' end of the supplied document.
    Public Sub ajouteTableau(document As WordprocessingDocument, ByVal data(,) As String)
        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-add-tables-to-word-processing-documents?tabs=cs-0%2Cvb-1%2Ccs-2%2Ccs-3%2Ccs-4%2Ccs-5%2Ccs-6%2Ccs-7%2Ccs-8%2Cvb

        Dim doc = document.MainDocumentPart.Document

        Dim table As New Table()

        Dim props As New TableProperties(New TableBorders(
            New TopBorder With {
                .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                .Size = 12},
            New BottomBorder With {
                .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                .Size = 12},
            New LeftBorder With {
                .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                .Size = 12},
            New RightBorder With {
                .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                .Size = 12},
            New InsideHorizontalBorder With {
                .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                .Size = 12},
            New InsideVerticalBorder With {
                .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                .Size = 12}))
        table.AppendChild(Of TableProperties)(props)

        For i = 0 To UBound(data, 1)
            Dim tr As New TableRow
            For j = 0 To UBound(data, 2)
                Dim tc As New TableCell
                tc.Append(New Paragraph(New Run(New Text(data(i, j)))))

                ' Assume you want columns that are automatically sized.
                tc.Append(New TableCellProperties(
                    New TableCellWidth With {.Type = TableWidthUnitValues.Auto}))

                tr.Append(tc)
            Next
            table.Append(tr)
        Next
        doc.Body.Append(table)
        doc.Save()
    End Sub
    Public Sub ajouteTableau(ByVal fileName As String, ByVal data(,) As String)
        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-add-tables-to-word-processing-documents?tabs=cs-0%2Cvb-1%2Ccs-2%2Ccs-3%2Ccs-4%2Ccs-5%2Ccs-6%2Ccs-7%2Ccs-8%2Cvb
        Using document = WordprocessingDocument.Open(fileName, True)

            Dim doc = document.MainDocumentPart.Document

            Dim table As New Table()

            Dim props As New TableProperties(New TableBorders(
                New TopBorder With {
                    .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                    .Size = 12},
                New BottomBorder With {
                    .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                    .Size = 12},
                New LeftBorder With {
                    .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                    .Size = 12},
                New RightBorder With {
                    .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                    .Size = 12},
                New InsideHorizontalBorder With {
                    .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                    .Size = 12},
                New InsideVerticalBorder With {
                    .Val = New EnumValue(Of BorderValues)(BorderValues.Single),
                    .Size = 12}))
            table.AppendChild(Of TableProperties)(props)

            For i = 0 To UBound(data, 1)
                Dim tr As New TableRow
                For j = 0 To UBound(data, 2)
                    Dim tc As New TableCell
                    tc.Append(New Paragraph(New Run(New Text(data(i, j)))))
                    ' Assume you want columns that are automatically sized.
                    tc.Append(New TableCellProperties(New TableCellWidth With {.Type = TableWidthUnitValues.Auto}))
                    tr.Append(tc)
                Next
                table.Append(tr)
            Next
            doc.Body.Append(table)
            doc.Save()
        End Using
    End Sub
    ' Create a new character style with the specified style id, style name and aliases and add 
    ' it to the specified style definitions part.
    Public Sub CreateAndAddCharacterStyle(ByVal styleDefinitionsPart As StyleDefinitionsPart,
        ByVal styleid As String, ByVal stylename As String, Optional ByVal aliases As String = "")
        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-create-and-add-a-character-style-to-a-word-processing-document?tabs=cs-0%2Ccs-1%2Ccs-2%2Ccs-3%2Ccs-4%2Ccs-5%2Ccs-6%2Cvb
        ' Get access to the root element of the styles part.
        Dim styles As Styles = styleDefinitionsPart.Styles
        If styles Is Nothing Then
            styleDefinitionsPart.Styles = New Styles()
            styleDefinitionsPart.Styles.Save()
        End If

        ' Create a new character style and specify some of the attributes.
        Dim style As New Style() With {
            .Type = StyleValues.Character,
            .StyleId = styleid,
            .CustomStyle = True}

        ' Create and add the child elements (properties of the style).
        Dim aliases1 As New Aliases() With {.Val = aliases}
        Dim styleName1 As New StyleName() With {.Val = stylename}
        Dim linkedStyle1 As New LinkedStyle() With {.Val = "OverdueAmountPara"}
        If aliases <> "" Then
            style.Append(aliases1)
        End If
        style.Append(styleName1)
        style.Append(linkedStyle1)

        ' Create the StyleRunProperties object and specify some of the run properties.
        Dim styleRunProperties1 As New StyleRunProperties()
        Dim bold1 As New Bold()
        Dim color1 As New Color() With {.ThemeColor = ThemeColorValues.Accent3}
        'Dim color1 As New Color() With {.ThemeColor = ThemeColorValues.Accent3}
        Dim font1 As New RunFonts() With {.Ascii = "Tahoma"}
        Dim italic1 As New Italic()
        ' Specify a 24 point size.
        Dim fontSize1 As New FontSize() With {.Val = "72"}
        styleRunProperties1.Append(font1)
        styleRunProperties1.Append(fontSize1)
        styleRunProperties1.Append(color1)
        'styleRunProperties1.Append(bold1)
        'styleRunProperties1.Append(italic1)

        ' Add the run properties to the style.
        style.Append(styleRunProperties1)

        ' Add the style to the styles part.
        styles.Append(style)
    End Sub


    ' Add a StylesDefinitionsPart to the document.  Returns a reference to it.
    '2005
    Public Function AddStylesPartToPackage(document As WordprocessingDocument) As StyleDefinitionsPart
        Dim part As StyleDefinitionsPart
        part = document.MainDocumentPart.AddNewPart(Of StyleDefinitionsPart)()
        Dim root As New Styles()
        root.Save(part)
        Return part
    End Function
    ' Add a StylesDefinitionsPart to the document.  Returns a reference to it.
    Public Function AddStylesPartToPackage(ByVal fileName As String) As StyleDefinitionsPart
        Dim document As WordprocessingDocument = WordprocessingDocument.Open(fileName, True)
        Dim part As StyleDefinitionsPart
        part = document.MainDocumentPart.AddNewPart(Of StyleDefinitionsPart)()
        Dim root As New Styles()
        root.Save(part)
        Return part
    End Function
    ' Add a StylesDefinitionsPart to the document.  Returns a reference to it.
    'Public Function AddStylesPartToPackage(ByVal doc As WordprocessingDocument) As StyleDefinitionsPart
    '    Dim part As StyleDefinitionsPart
    '    part = doc.MainDocumentPart.AddNewPart(Of StyleDefinitionsPart)()
    '    Dim root As New Styles()
    '    root.Save(part)
    '    Return part
    'End Function
    Public Sub ajouteImage(ByVal document As String, ByVal fileName As String)
        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-insert-a-picture-into-a-word-processing-document?tabs=cs-0%2Ccs-1%2Ccs-2%2Cvb-3%2Cvb 
        Using wordprocessingDocument As WordprocessingDocument = WordprocessingDocument.Open(document, True)
            Dim mainPart As MainDocumentPart = wordprocessingDocument.MainDocumentPart

            Dim imagePart As ImagePart = mainPart.AddImagePart(ImagePartType.Jpeg)

            Using stream As New FileStream(fileName, FileMode.Open)
                imagePart.FeedData(stream)
            End Using

            AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart))
        End Using
    End Sub
    Public Sub ajouteImage(document As WordprocessingDocument, ByVal fileName As String)
        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-insert-a-picture-into-a-word-processing-document?tabs=cs-0%2Ccs-1%2Ccs-2%2Cvb-3%2Cvb  
        Dim mainPart As MainDocumentPart = document.MainDocumentPart

        Dim imagePart As ImagePart = mainPart.AddImagePart(ImagePartType.Jpeg)

        Using stream As New FileStream(fileName, FileMode.Open)
            imagePart.FeedData(stream)
        End Using

        AddImageToBody(document, mainPart.GetIdOfPart(imagePart))
    End Sub

    Private Sub AddImageToBody(ByVal wordDoc As WordprocessingDocument, ByVal relationshipId As String)
        ' Define the reference of the image.
        Dim element = New Drawing(
                              New DW.Inline(
                          New DW.Extent() With {.Cx = 5990000L, .Cy = 5792000L},
                          New DW.EffectExtent() With {.LeftEdge = 0L, .TopEdge = 0L, .RightEdge = 0L, .BottomEdge = 0L},
                          New DW.DocProperties() With {.Id = CType(1UI, UInt32Value), .Name = "Picture1"},
                          New DW.NonVisualGraphicFrameDrawingProperties(
                              New A.GraphicFrameLocks() With {.NoChangeAspect = True}
                              ),
                          New A.Graphic(New A.GraphicData(
                                        New PIC.Picture(
                                            New PIC.NonVisualPictureProperties(
                                                New PIC.NonVisualDrawingProperties() With {.Id = 0UI, .Name = "Koala.jpg"},
                                                New PIC.NonVisualPictureDrawingProperties()
                                                ),
                                            New PIC.BlipFill(
                                                New A.Blip(
                                                    New A.BlipExtensionList(
                                                        New A.BlipExtension() With {.Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"})
                                                    ) With {.Embed = relationshipId, .CompressionState = A.BlipCompressionValues.Print},
                                                New A.Stretch(
                                                    New A.FillRectangle()
                                                    )
                                                ),
                                            New PIC.ShapeProperties(
                                                New A.Transform2D(
                                                    New A.Offset() With {.X = 0L, .Y = 0L},
                                                    New A.Extents() With {.Cx = 5990000L, .Cy = 5792000L}),
                                                New A.PresetGeometry(
                                                    New A.AdjustValueList()
                                                    ) With {.Preset = A.ShapeTypeValues.Rectangle}
                                                )
                                            )
                                        ) With {.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"}
                                    )
                                ) With {.DistanceFromTop = 0UI,
                                        .DistanceFromBottom = 0UI,
                                        .DistanceFromLeft = 0UI,
                                        .DistanceFromRight = 0UI}
                            )

        ' Append the reference to body, the element should be in a Run.
        wordDoc.MainDocumentPart.Document.Body.AppendChild(New Paragraph(New Run(element)))
    End Sub
    '2005'
    Public Function ajouteParagraphe(ByVal document As WordprocessingDocument, ByVal txt As String) As Paragraph
        ' Open a WordprocessingDocument for editing using the filepath. 
        ' Assign a reference to the existing document body.
        Dim body As Body = document.MainDocumentPart.Document.Body

        ' Add a paragraph with some text.            
        Dim para As Paragraph = body.AppendChild(New Paragraph())
        Dim run As Run = para.AppendChild(New Run())
        run.AppendChild(New Text(txt))
        Return para
    End Function
    Public Function ajouteParagraphe(ByVal filepath As String, ByVal txt As String) As Paragraph
        ' Open a WordprocessingDocument for editing using the filepath.
        Using wordprocessingDocument As WordprocessingDocument = WordprocessingDocument.Open(filepath, True)
            ' Assign a reference to the existing document body.
            Dim body As Body = wordprocessingDocument.MainDocumentPart.Document.Body

            ' Add a paragraph with some text.            
            Dim para As Paragraph = body.AppendChild(New Paragraph())
            Dim run As Run = para.AppendChild(New Run())
            run.AppendChild(New Text(txt))
            Return para
        End Using
    End Function
    'Public Function ajouteParagraphe(ByVal doc As WordprocessingDocument, ByVal txt As String) As Paragraph
    '    ' Open a WordprocessingDocument for editing using the filepath.
    '    'Using doc
    '    ' Assign a reference to the existing document body.
    '    Dim body As Body = doc.MainDocumentPart.Document.Body

    '        ' Add a paragraph with some text.            
    '        Dim para As Paragraph = body.AppendChild(New Paragraph())
    '        Dim run As Run = para.AppendChild(New Run())
    '        run.AppendChild(New Text(txt))
    '        Return para
    '    'End Using
    'End Function

    ' Apply a style to a paragraph.
    Public Sub ApplyStyleToParagraph(ByVal filepath As String, ByVal styleid As String, ByVal stylename As String, ByVal p As Paragraph)

        Using wordprocessingDocument As WordprocessingDocument = WordprocessingDocument.Open(filepath, True)
            ' If the paragraph has no ParagraphProperties object, create one.
            If p.Elements(Of ParagraphProperties)().Count() = 0 Then
                p.PrependChild(Of ParagraphProperties)(New ParagraphProperties)
            End If

            ' Get the paragraph properties element of the paragraph.
            Dim pPr As ParagraphProperties = p.Elements(Of ParagraphProperties)().First()

            ' Get the Styles part for this document.
            Dim part As StyleDefinitionsPart = wordprocessingDocument.MainDocumentPart.StyleDefinitionsPart

            ' If the Styles part does not exist, add it and then add the style.
            If part Is Nothing Then
                part = AddStylesPartToPackage(filepath)
                AddNewStyle(part, styleid, stylename)
            Else
                ' If the style is not in the document, add it.
                If Not IsStyleIdInDocument(wordprocessingDocument, styleid) Then
                    ' No match on styleid, so let's try style name.
                    Dim styleidFromName As String =
                    GetStyleIdFromStyleName(wordprocessingDocument, stylename)
                    If styleidFromName Is Nothing Then
                        AddNewStyle(part, styleid, stylename)
                    Else
                        styleid = styleidFromName
                    End If
                End If
            End If

            ' Set the style of the paragraph.
            pPr.ParagraphStyleId = New ParagraphStyleId With {.Val = styleid}
        End Using
    End Sub
    '' Apply a style to a paragraph.
    Public Sub ApplyStyleToParagraph(ByVal doc As WordprocessingDocument, ByVal styleid As String, ByVal stylename As String, ByVal p As Paragraph)

        ' If the paragraph has no ParagraphProperties object, create one.
        If p.Elements(Of ParagraphProperties)().Count() = 0 Then
            p.PrependChild(Of ParagraphProperties)(New ParagraphProperties)
        End If

        ' Get the paragraph properties element of the paragraph.
        Dim pPr As ParagraphProperties = p.Elements(Of ParagraphProperties)().First()

        ' Get the Styles part for this document.
        Dim part As StyleDefinitionsPart = doc.MainDocumentPart.StyleDefinitionsPart

        ' If the Styles part does not exist, add it and then add the style.
        If part Is Nothing Then
            part = AddStylesPartToPackage(doc)
            AddNewStyle(part, styleid, stylename)
        Else
            ' If the style is not in the document, add it.
            If Not IsStyleIdInDocument(doc, styleid) Then
                ' No match on styleid, so let's try style name.
                Dim styleidFromName As String =
                    GetStyleIdFromStyleName(doc, stylename)
                If styleidFromName Is Nothing Then
                    AddNewStyle(part, styleid, stylename)
                Else
                    styleid = styleidFromName
                End If
            End If
        End If

        ' Set the style of the paragraph.
        pPr.ParagraphStyleId = New ParagraphStyleId With {.Val = styleid}
    End Sub

    ' Return true if the style id is in the document, false otherwise.
    Public Function IsStyleIdInDocument(ByVal doc As WordprocessingDocument, ByVal styleid As String) As Boolean
        ' Get access to the Styles element for this document.
        Dim s As Styles = doc.MainDocumentPart.StyleDefinitionsPart.Styles

        ' Check that there are styles and how many.
        Dim n As Integer = s.Elements(Of Style)().Count()
        If n = 0 Then
            Return False
        End If

        ' Look for a match on styleid.
        Dim style As Style = s.Elements(Of Style)().Where(Function(st) (st.StyleId = styleid) AndAlso (st.Type.Value = StyleValues.Paragraph)).FirstOrDefault()
        If style Is Nothing Then
            Return False
        End If

        Return True
    End Function

    ' Return styleid that matches the styleName, or null when there's no match.
    Public Function GetStyleIdFromStyleName(ByVal doc As WordprocessingDocument, ByVal styleName As String) As String
        Dim stylePart As StyleDefinitionsPart = doc.MainDocumentPart.StyleDefinitionsPart
        Dim styleId As String = stylePart.Styles.Descendants(Of StyleName)().
            Where(Function(s) s.Val.Value.Equals(styleName) AndAlso ((CType(s.Parent, Style)).Type.Value = StyleValues.Paragraph)).
            Select(Function(n) (CType(n.Parent, Style)).StyleId).
            FirstOrDefault()
        Return styleId
    End Function

    ' Create a new style with the specified styleid and stylename and add it to the specified style definitions part.
    Public Sub AddNewStyle(ByVal styleDefinitionsPart As StyleDefinitionsPart, ByVal styleid As String, ByVal stylename As String)
        ' Get access to the root element of the styles part.
        Dim styles As Styles = styleDefinitionsPart.Styles

        ' Create a new paragraph style and specify some of the properties.
        Dim style As New Style With {.Type = StyleValues.Paragraph, .StyleId = styleid, .CustomStyle = True}
        Dim styleName1 As New StyleName With {.Val = stylename}
        Dim basedOn1 As New BasedOn With {.Val = "Normal"}
        Dim nextParagraphStyle1 As New NextParagraphStyle With {.Val = "Normal"}
        style.Append(styleName1)
        style.Append(basedOn1)
        'TODO : remettre ?
        style.Append(nextParagraphStyle1)

        ' Create the StyleRunProperties object and specify some of the run properties.
        Dim styleRunProperties1 As New StyleRunProperties
        Dim bold1 As New Bold
        Dim color1 As New Color With {.ThemeColor = ThemeColorValues.Accent2}
        Dim font1 As New RunFonts With {.Ascii = "Lucida Console"}
        Dim italic1 As New Italic
        ' Specify a 12 point size.
        Dim fontSize1 As New FontSize With {.Val = "48"}
        styleRunProperties1.Append(bold1)
        styleRunProperties1.Append(color1)
        styleRunProperties1.Append(font1)
        styleRunProperties1.Append(fontSize1)
        styleRunProperties1.Append(italic1)

        ' Add the run properties to the style.
        style.Append(styleRunProperties1)

        ' Add the style to the styles part.
        styles.Append(style)
    End Sub

    Public Sub CreateAndAddParagraphStyle(ByVal styleDefinitionsPart As StyleDefinitionsPart,
        ByVal styleid As String, ByVal stylename As String, Optional ByVal aliases As String = "")

        ' Access the root element of the styles part.
        Dim styles As Styles = styleDefinitionsPart.Styles
        If styles Is Nothing Then
            styleDefinitionsPart.Styles = New Styles()
            styleDefinitionsPart.Styles.Save()
        End If

        ' Create a new paragraph style element and specify some of the attributes.
        Dim style As New Style() With {
         .Type = StyleValues.Paragraph,
         .StyleId = styleid,
         .CustomStyle = True,
         .[Default] = False}

        ' Create and add the child elements (properties of the style)
        Dim aliases1 As New Aliases() With {.Val = aliases}
        Dim autoredefine1 As New AutoRedefine() With {.Val = OnOffOnlyValues.Off}
        Dim basedon1 As New BasedOn() With {.Val = "Normal"}
        Dim linkedStyle1 As New LinkedStyle() With {.Val = "OverdueAmountChar"}
        Dim locked1 As New Locked() With {.Val = OnOffOnlyValues.Off}
        Dim primarystyle1 As New PrimaryStyle() With {.Val = OnOffOnlyValues.[On]}
        Dim stylehidden1 As New StyleHidden() With {.Val = OnOffOnlyValues.Off}
        Dim semihidden1 As New SemiHidden() With {.Val = OnOffOnlyValues.Off}
        Dim styleName1 As New StyleName() With {.Val = stylename}
        Dim nextParagraphStyle1 As New NextParagraphStyle() With {
         .Val = "Normal"}
        Dim uipriority1 As New UIPriority() With {.Val = 1}
        Dim unhidewhenused1 As New UnhideWhenUsed() With {
         .Val = OnOffOnlyValues.[On]}
        If aliases <> "" Then
            style.Append(aliases1)
        End If
        style.Append(autoredefine1)
        style.Append(basedon1)
        style.Append(linkedStyle1)
        style.Append(locked1)
        style.Append(primarystyle1)
        style.Append(stylehidden1)
        style.Append(semihidden1)
        style.Append(styleName1)
        style.Append(nextParagraphStyle1)
        style.Append(uipriority1)
        style.Append(unhidewhenused1)

        ' Create the StyleRunProperties object and specify some of the run properties.
        Dim styleRunProperties1 As New StyleRunProperties()
        Dim bold1 As New Bold()
        Dim color1 As New Color() With {
         .ThemeColor = ThemeColorValues.Accent2}
        Dim font1 As New RunFonts() With {
         .Ascii = "Lucida Console"}
        Dim italic1 As New Italic()
        ' Specify a 12 point size.
        Dim fontSize1 As New FontSize() With {
         .Val = "24"}
        styleRunProperties1.Append(bold1)
        styleRunProperties1.Append(color1)
        styleRunProperties1.Append(font1)
        styleRunProperties1.Append(fontSize1)
        styleRunProperties1.Append(italic1)

        ' Add the run properties to the style.
        style.Append(styleRunProperties1)

        ' Add the style to the styles part.
        styles.Append(style)
    End Sub
    Public Sub AddSectionBreakToTheDocument(ByVal doc As WordprocessingDocument)
        Dim myMainPart As MainDocumentPart = doc.MainDocumentPart
        Dim paragraphSectionBreak As Paragraph = New Paragraph()
        Dim paragraphSectionBreakProperties As ParagraphProperties = New ParagraphProperties()
        Dim SectionBreakProperties As SectionProperties = New SectionProperties()
        'Dim SectionBreakType As SectionType = New SectionType() { .Val() = SectionMarkValues.NextPage}
        Dim SectionBreakType As New SectionType
        SectionBreakType.Val = SectionMarkValues.NextPage
        SectionBreakProperties.Append(SectionBreakType)
        paragraphSectionBreakProperties.Append(SectionBreakProperties)
        paragraphSectionBreak.Append(paragraphSectionBreakProperties)
        myMainPart.Document.Body.InsertAfter(paragraphSectionBreak, myMainPart.Document.Body.LastChild)
        myMainPart.Document.Save()
    End Sub
    ' Add a StylesDefinitionsPart to the document. Returns a reference to it.
    'Public Function AddStylesPartToPackage(ByVal doc As WordprocessingDocument) _
    '    As StyleDefinitionsPart
    '    Dim part As StyleDefinitionsPart
    '    part = doc.MainDocumentPart.AddNewPart(Of StyleDefinitionsPart)()
    '    Dim root As New Styles()
    '    root.Save(part)
    '    Return part
    'End Function
End Module