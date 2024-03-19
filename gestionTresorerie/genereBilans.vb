
Imports System.Xml

Imports System.IO
Imports System.IO.Packaging
Imports DocumentFormat.OpenXml.Presentation
Module genereBilans
    'Utilisation du NameSpace WordprocessingML
    Dim WordprocessingML As String = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
    'https://badger.developpez.com/tutoriels/dotnet/creer-fichier-word-openxml/#LIX
    Public Sub creeBilans()

        ' Création du WordML
        Dim xmlStartPart = New XmlDocument()
        Dim tagDocument As XmlElement
        tagDocument = xmlStartPart.CreateElement("w:document", WordprocessingML)
        xmlStartPart.AppendChild(tagDocument)
        Dim tagBody As XmlElement
        tagBody = xmlStartPart.CreateElement("w:body", WordprocessingML)
        tagDocument.AppendChild(tagBody)
        Dim tagParagraph As XmlElement
        tagParagraph = xmlStartPart.CreateElement("w:p", WordprocessingML)
        tagBody.AppendChild(tagParagraph)
        Dim tagRun As XmlElement
        tagRun = xmlStartPart.CreateElement("w:r", WordprocessingML)
        tagParagraph.AppendChild(tagRun)
        Dim tagText As XmlElement
        tagText = xmlStartPart.CreateElement("w:t", WordprocessingML)
        tagRun.AppendChild(tagText)

        ' Insertion du texte
        Dim nodeText As XmlNode
        nodeText = xmlStartPart.CreateNode(XmlNodeType.Text, "w:t", WordprocessingML)
        nodeText.Value = "Salut !"
        tagText.AppendChild(nodeText)

        ' Création d'un nouveau package
        Dim pkgOutputDoc As Package = Nothing
        pkgOutputDoc = Package.Open("monFichier.docx", FileMode.Create, FileAccess.ReadWrite)

        ' Création d'une part
        Dim Uri As Uri
        Uri = New Uri("/word/document.xml", UriKind.Relative)
        Dim partDocumentXML As PackagePart
        partDocumentXML = pkgOutputDoc.CreatePart(Uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")

        Dim streamStartPart As StreamWriter
        streamStartPart = New StreamWriter(partDocumentXML.GetStream(FileMode.Create, FileAccess.Write))
        xmlStartPart.Save(streamStartPart)
        streamStartPart.Close()

        ' Création de la RelationShip
        pkgOutputDoc.CreateRelationship(Uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId1")
        pkgOutputDoc.Flush()

        ' Fermeture du package
        pkgOutputDoc.Close()
    End Sub
    Private Sub creeStyle(sNomStyle As String, sAlignement As String, colCouleur As Color)
        ' Création du style
        Dim xmlStylePart As XmlDocument
        xmlStylePart = New XmlDocument()

        Dim tagStyles As XmlElement
        tagStyles = xmlStylePart.CreateElement("w:styles", WordprocessingML)
        xmlStylePart.AppendChild(tagStyles)
        Dim tagStyle As XmlElement
        tagStyle = xmlStylePart.CreateElement("w:style", WordprocessingML)
        tagStyles.AppendChild(tagStyle)

        '/****attributs du style****/
        'type de style
        Dim attributType As XmlAttribute
        attributType = xmlStylePart.CreateAttribute("w:type", WordprocessingML)
        attributType.InnerText = "paragraph"
        tagStyle.SetAttributeNode(attributType)
        'identifiant du style
        Dim attributId As XmlAttribute
        attributId = xmlStylePart.CreateAttribute("w:styleId", WordprocessingML)
        attributId.InnerText = sNomStyle
        tagStyle.SetAttributeNode(attributId)

        'noeud Name
        Dim tagName As XmlElement
        tagName = xmlStylePart.CreateElement("w:name", WordprocessingML)
        tagStyle.AppendChild(tagName)
        Dim attributName As XmlAttribute
        attributName = xmlStylePart.CreateAttribute("w:val", WordprocessingML)
        attributName.InnerText = sNomStyle
        tagName.SetAttributeNode(attributName)

        'noeud pPr
        Dim tagpPr As XmlElement
        tagpPr = xmlStylePart.CreateElement("w:pPr", WordprocessingML)
        tagStyle.AppendChild(tagpPr)
        'propriété alignement
        Dim tagAlignement As XmlElement
        tagAlignement = xmlStylePart.CreateElement("w:jc", WordprocessingML)
        'attribut du noeud w:jc
        Dim attributJc As XmlAttribute
        attributJc = xmlStylePart.CreateAttribute("w:val", WordprocessingML)
        'attributJc.InnerText = "center"
        attributJc.InnerText = sAlignement
        tagAlignement.SetAttributeNode(attributJc)
        tagpPr.AppendChild(tagAlignement)

        'noeud rPr
        Dim tagrPr As XmlElement
        tagrPr = xmlStylePart.CreateElement("w:rPr", WordprocessingML)
        tagStyle.AppendChild(tagrPr)
        'propriété couleur
        Dim tagCouleur As XmlElement
        tagCouleur = xmlStylePart.CreateElement("w:color", WordprocessingML)
        'attribut de l'élément w:color
        Dim valeurCouleur As XmlAttribute
        valeurCouleur = xmlStylePart.CreateAttribute("w:val", WordprocessingML)
        'valeurCouleur.InnerText = "FF0000"
        valeurCouleur.InnerText = colCouleur.ToString
        tagCouleur.SetAttributeNode(valeurCouleur)
        tagrPr.AppendChild(tagCouleur)
    End Sub
End Module





'Imports System.IO
'Imports System.Net.Mime.MediaTypeNames
'Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Tab
'Imports DocumentFormat.OpenXml
'Imports DocumentFormat.OpenXml.Drawing
'Imports DocumentFormat.OpenXml.Packaging
'Imports DocumentFormat.OpenXml.Spreadsheet
'Imports DocumentFormat.OpenXml.Wordprocessing

'Module genereBilans
'    Dim path = "c:\temp\test.docx"

'    Public Sub genereOpenXml()
'        Using ms As New MemoryStream
'            Using doc As WordprocessingDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, True)
'                Dim docBody As New DocumentFormat.OpenXml.Wordprocessing.Body()
'                Dim mainPart As MainDocumentPart = doc.AddMainDocumentPart
'                mainPart.Document = New Document()
'                doc.MainDocumentPart.Document.Append(docBody)
'                'doc.AddMainDocumentPart.Document = New Document()

'                Dim imagePart As ImagePart = mainPart.AddImagePart(ImagePartType.Jpeg)
'                Using stream As New FileStream("D:\Logo\Icons\logo_c1.png", FileMode.Open)
'                    imagePart.FeedData(stream)
'                End Using

'                AddImageToBody(doc, mainPart.GetIdOfPart(imagePart), True)
'                AddParagraphe(doc, 22, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris sit amet dui ante. Sed eget purus in nisi tincidunt vulputate vel at turpis. Nullam non pharetra quam, non malesuada nunc. In hac habitasse platea dictumst. Praesent suscipit nibh non leo vulputate consectetur. Aliquam est nibh, pretium vel arcu sit amet, scelerisque facilisis velit. Pellentesque orci odio, iaculis non eros id, convallis mattis nibh. Aenean vehicula lacus vel pellentesque iaculis. Curabitur nec semper orci, a sagittis tellus. Duis mollis placerat elit vel rutrum. Proin mollis libero sed ex pretium, a ornare nibh malesuada. Maecenas ultrices purus vel nisl cursus, consectetur vestibulum massa dignissim. Nam imperdiet mattis mi scelerisque dictum. Phasellus non ullamcorper ligula, eleifend porta tortor. Vestibulum odio dolor, vehicula non suscipit non, euismod vitae augue.
'Maecenas viverra leo neque, interdum maximus dui luctus id. Quisque commodo, lorem sed ullamcorper dignissim, neque nibh venenatis leo, sed imperdiet risus libero eu risus. Nulla ultrices, justo in eleifend bibendum, sem nulla egestas augue, sit amet consequat nulla dui sit amet dui. Quisque feugiat libero eu ex dignissim pellentesque. Curabitur aliquam ex eu consectetur congue. Suspendisse venenatis neque vel odio scelerisque, at mattis magna venenatis. Nulla facilisi. Fusce pulvinar consectetur viverra. Duis eu eros vitae arcu viverra dapibus. Aliquam porttitor elit vel elit dignissim, ut blandit tortor molestie.")
'                AddPage(doc)

'                If IO.File.Exists(path) Then
'                    IO.File.Delete(path)
'                End If

'                doc.Save()
'            End Using
'        End Using
'    End Sub

'    Private Sub AddParagraphe(ByVal wordDoc As WordprocessingDocument, ByVal fontSize As Integer, ByVal txt As String)
'        Dim runFont As New Wordprocessing.RunFonts With {.Ascii = "Comic Sans MS"}
'        Dim runProp As New DocumentFormat.OpenXml.Drawing.RunProperties
'        Dim runPara As New DocumentFormat.OpenXml.Spreadsheet.Run
'        Dim para As New DocumentFormat.OpenXml.Wordprocessing.Paragraph

'        runProp.Append(runFont)
'        runProp.FontSize = New DocumentFormat.OpenXml.Spreadsheet.FontSize() With {.Val = fontSize}

'        runPara.Append(runProp)
'        runPara.Append(New DocumentFormat.OpenXml.Drawing.Text(txt))
'        para.Append(runPara)

'        wordDoc.MainDocumentPart.Document.AppendChild(para)
'    End Sub

'    Private Sub AddImageToBody(ByVal wordDoc As WordprocessingDocument, ByVal relationshipId As String, ByVal Optional center As Boolean = False)
'        Dim img = New BitmapImage(New Uri("D:\Logo\Icons\logo_c1.png", UriKind.RelativeOrAbsolute))
'        Dim iWidth As Integer = CInt(System.Math.Round(img.Width * 9525))
'        Dim iHeight As Integer = CInt(System.Math.Round(img.Height * 9525))

'        ' Define the reference of the image.
'        Dim element = New DocumentFormat.OpenXml.Wordprocessing.Drawing(
'                              New DW.Inline(
'                          New DW.Extent() With {.Cx = iWidth, .Cy = iHeight},
'                          New DW.EffectExtent() With {.LeftEdge = 0L, .TopEdge = 0L, .RightEdge = 0L, .BottomEdge = 0L},
'                          New DW.DocProperties() With {.Id = CType(1UI, UInt32Value), .Name = "Picture1"},
'                          New DW.NonVisualGraphicFrameDrawingProperties(
'                              New A.GraphicFrameLocks() With {.NoChangeAspect = True}
'                              ),
'                          New A.Graphic(New A.GraphicData(
'                                        New PIC.Picture(
'                                            New PIC.NonVisualPictureProperties(
'                                                New PIC.NonVisualDrawingProperties() With {.Id = 0UI, .Name = "Koala.jpg"},
'                                                New PIC.NonVisualPictureDrawingProperties()
'                                                ),
'                                            New PIC.BlipFill(
'                                                New A.Blip(
'                                                    New A.BlipExtensionList(
'                                                        New A.BlipExtension() With {.Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"})
'                                                    ) With {.Embed = relationshipId, .CompressionState = A.BlipCompressionValues.Print},
'                                                New A.Stretch(
'                                                    New A.FillRectangle()
'                                                    )
'                                                ),
'                                            New PIC.ShapeProperties(
'                                                New A.Transform2D(
'                                                    New A.Offset() With {.X = 0L, .Y = 0L},
'                                                    New A.Extents() With {.Cx = iWidth, .Cy = iHeight}),
'                                                New A.PresetGeometry(
'                                                    New A.AdjustValueList()
'                                                    ) With {.Preset = A.ShapeTypeValues.Rectangle}
'                                                )
'                                            )
'                                        ) With {.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"}
'                                    )
'                                ) With {.DistanceFromTop = 0UI,
'                                        .DistanceFromBottom = 0UI,
'                                        .DistanceFromLeft = 0UI,
'                                        .DistanceFromRight = 0UI})

'        Dim para As New DocumentFormat.OpenXml.Drawing.Paragraph
'        Dim justification As New Justification With {.Val = JustificationValues.Center}
'        Dim paraProp As New DocumentFormat.OpenXml.Drawing.ParagraphProperties()
'        Dim run As New DocumentFormat.OpenXml.Drawing.Run()

'        If center Then
'            paraProp.Append(justification)
'            para.Append(paraProp)
'        End If

'        run.Append(element)
'        para.Append(run)
'        ' Append the reference to body, the element should be in a Run.
'        wordDoc.MainDocumentPart.Document.Body.AppendChild(para) 'New Paragraph(New Run(element)))
'    End Sub

'    Private Sub AddPage(ByVal wordDoc As WordprocessingDocument)
'        wordDoc.MainDocumentPart.Document.Body.Append(New DocumentFormat.OpenXml.Drawing.Paragraph(New DocumentFormat.OpenXml.Drawing.Run(New DocumentFormat.OpenXml.Drawing.Break() With {.Type = BreakValues.Page})))
'    End Sub
'End Module
