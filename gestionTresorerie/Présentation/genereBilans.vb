
''https://badger.developpez.com/tutoriels/dotnet/creer-fichier-word-openxml/
'Imports System.Xml

'Imports System.IO
'Imports System.IO.Packaging
'Imports DocumentFormat.OpenXml.Presentation
'Imports DocumentFormat.OpenXml.Drawing.Diagrams
'Imports DocumentFormat.OpenXml.Office2019.Drawing.SVG
'Imports DocumentFormat.OpenXml.Spreadsheet
'Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
'Imports DocumentFormat.OpenXml.Math
'Imports DocumentFormat.OpenXml.Packaging
'Imports DocumentFormat.OpenXml.Wordprocessing
'Imports DocumentFormat.OpenXml
'Imports BottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder
'Imports LeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder
'Imports RightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder
'Imports Run = DocumentFormat.OpenXml.Wordprocessing.Run
'Imports Table = DocumentFormat.OpenXml.Wordprocessing.Table
'Imports Text = DocumentFormat.OpenXml.Wordprocessing.Text
'Imports TopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder
'Module genereBilans
'    Dim xmlStartPart As XmlDocument
'    Dim tagBody As XmlElement
'    Dim tagDrawing As XmlElement
'    Dim pkgOutputDoc As Package
'    Dim uri As Uri
'    Dim partDocumentXML As PackagePart
'    Dim streamStartPart As StreamWriter
'    Dim iOrdRelation As Integer = 1

'    'Utilisation du NameSpace WordprocessingML
'    ReadOnly WordprocessingML As String = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
'    'https://badger.developpez.com/tutoriels/dotnet/creer-fichier-word-openxml/#LIX
'    'https://marketplace.visualstudio.com/items?itemName=mikeebowen.OOXMLSnippets
'    Public Sub CreeBilans()

'        ' Création du WordML
'        Dim xmlStartPart = New XmlDocument()
'        Dim tagDocument As XmlElement
'        tagDocument = xmlStartPart.CreateElement("w:document", WordprocessingML)
'        xmlStartPart.AppendChild(tagDocument)
'        Dim tagBody As XmlElement
'        tagBody = xmlStartPart.CreateElement("w:body", WordprocessingML)
'        tagDocument.AppendChild(tagBody)

'        Call CreeStyle("monStyle", "center")
'        'TODO : passer les titres et les tableaux de valeurs
'        Call AjouteParagraphe(xmlStartPart, tagBody, "glop 11")
'        Call AjouteParagraphe(xmlStartPart, tagBody, "glop 12")
'        Call AjouteParagraphe(xmlStartPart, tagBody, "glop 13")

'        ' Création d'un nouveau package
'        Dim pkgOutputDoc As Package = Package.Open("monFichier.docx", FileMode.Create, FileAccess.ReadWrite)

'        ' Création d'une part
'        Dim Uri As Uri
'        Uri = New Uri("/word/document.xml", UriKind.Relative)
'        Dim partDocumentXML As PackagePart
'        partDocumentXML = pkgOutputDoc.CreatePart(Uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")
'        'Call AjouteImage(xmlStartPart, pkgOutputDoc, partDocumentXML, "C:\Users\User\Downloads\Orion_HST-631f.jpeg", Uri)

'        Dim streamStartPart As StreamWriter
'        streamStartPart = New StreamWriter(partDocumentXML.GetStream(FileMode.Create, FileAccess.Write))
'        xmlStartPart.Save(streamStartPart)
'        streamStartPart.Close()

'        ' Création de la RelationShip
'        pkgOutputDoc.CreateRelationship(Uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId2")
'        pkgOutputDoc.Flush()

'        ' Fermeture du package
'        pkgOutputDoc.Close()
'    End Sub
'    Private Sub AjouteParagraphe(xmlStartPart As XmlDocument, tagBody As XmlElement, sTexteParagraphe As String)
'        Dim tagParagraph As XmlElement
'        tagParagraph = xmlStartPart.CreateElement("w:p", WordprocessingML)
'        tagBody.AppendChild(tagParagraph)
'        Dim tagRun As XmlElement
'        tagRun = xmlStartPart.CreateElement("w:r", WordprocessingML)
'        tagParagraph.AppendChild(tagRun)
'        Dim tagText As XmlElement
'        tagText = xmlStartPart.CreateElement("w:t", WordprocessingML)
'        tagRun.AppendChild(tagText)

'        ' Insertion du texte
'        Dim nodeText As XmlNode
'        nodeText = xmlStartPart.CreateNode(XmlNodeType.Text, "w:t", WordprocessingML)
'        nodeText.Value = sTexteParagraphe
'        tagText.AppendChild(nodeText)
'    End Sub
'    Private Sub CreeStyle(sNomStyle As String, sAlignement As String)
'        ' Création du style
'        Dim xmlStylePart As XmlDocument
'        xmlStylePart = New XmlDocument()

'        Dim tagStyles As XmlElement
'        tagStyles = xmlStylePart.CreateElement("w:styles", WordprocessingML)
'        xmlStylePart.AppendChild(tagStyles)
'        Dim tagStyle As XmlElement
'        tagStyle = xmlStylePart.CreateElement("w:style", WordprocessingML)
'        tagStyles.AppendChild(tagStyle)

'        '/****attributs du style****/
'        'type de style
'        Dim attributType As XmlAttribute
'        attributType = xmlStylePart.CreateAttribute("w:type", WordprocessingML)
'        attributType.InnerText = "paragraph"
'        tagStyle.SetAttributeNode(attributType)
'        'identifiant du style
'        Dim attributId As XmlAttribute
'        attributId = xmlStylePart.CreateAttribute("w:styleId", WordprocessingML)
'        attributId.InnerText = sNomStyle
'        tagStyle.SetAttributeNode(attributId)

'        'noeud Name
'        Dim tagName As XmlElement
'        tagName = xmlStylePart.CreateElement("w:name", WordprocessingML)
'        tagStyle.AppendChild(tagName)
'        Dim attributName As XmlAttribute
'        attributName = xmlStylePart.CreateAttribute("w:val", WordprocessingML)
'        attributName.InnerText = sNomStyle
'        tagName.SetAttributeNode(attributName)

'        'noeud pPr
'        Dim tagpPr As XmlElement
'        tagpPr = xmlStylePart.CreateElement("w:pPr", WordprocessingML)
'        tagStyle.AppendChild(tagpPr)
'        'propriété alignement
'        Dim tagAlignement As XmlElement
'        tagAlignement = xmlStylePart.CreateElement("w:jc", WordprocessingML)
'        'attribut du noeud w:jc
'        Dim attributJc As XmlAttribute
'        attributJc = xmlStylePart.CreateAttribute("w:val", WordprocessingML)
'        'attributJc.InnerText = "center"
'        attributJc.InnerText = sAlignement
'        tagAlignement.SetAttributeNode(attributJc)
'        tagpPr.AppendChild(tagAlignement)

'        'noeud rPr
'        Dim tagrPr As XmlElement
'        tagrPr = xmlStylePart.CreateElement("w:rPr", WordprocessingML)
'        tagStyle.AppendChild(tagrPr)
'        'propriété couleur
'        Dim tagCouleur As XmlElement
'        tagCouleur = xmlStylePart.CreateElement("w:color", WordprocessingML)
'        'attribut de l'élément w:color
'        Dim valeurCouleur As XmlAttribute
'        valeurCouleur = xmlStylePart.CreateAttribute("w:val", WordprocessingML)
'        valeurCouleur.InnerText = "FFFF00"
'        'valeurCouleur.InnerText = colCouleur.ToString
'        'tagCouleur.SetAttributeNode(valeurCouleur)
'        tagrPr.AppendChild(tagCouleur)
'    End Sub
'    'Private Sub AjouteImage(xmlStartPart As XmlDocument, pkgOutputDoc As Package, partDocumentXML As PackagePart, sNomImage As String, uriDoc As Uri)
'    Public Sub AjouteImage()
'        Dim WordprocessingML As String = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
'        Dim RelationShips As String = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
'        Dim Drawing As String = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
'        Dim DrawingML As String = "http://schemas.openxmlformats.org/drawingml/2006/main"
'        Dim Pic As String = "http://schemas.openxmlformats.org/drawingml/2006/picture"

'        ' Création du WordML
'        Dim xmlStartPart As XmlDocument = New XmlDocument()

'        Dim tagDrawing As XmlElement = xmlStartPart.CreateElement("w:drawing", WordprocessingML)
'        Dim tagInline As XmlElement = xmlStartPart.CreateElement("wp:inline", Drawing)
'        tagDrawing.AppendChild(tagInline)

'        'Element extent
'        Dim tagExtent As XmlElement = xmlStartPart.CreateElement("wp:extent", Drawing)
'        tagInline.AppendChild(tagExtent)
'        'attributs de extent
'        Dim attributcx As XmlAttribute = xmlStartPart.CreateAttribute("cx")
'        attributcx.InnerText = "1300000"
'        tagExtent.SetAttributeNode(attributcx)
'        Dim attributcy As XmlAttribute = xmlStartPart.CreateAttribute("cy")
'        attributcy.InnerText = "1300000"
'        tagExtent.SetAttributeNode(attributcy)

'        'Element docPr
'        Dim tagdocPr As XmlElement = xmlStartPart.CreateElement("wp:docPr", Drawing)
'        tagInline.AppendChild(tagdocPr)
'        'attributs de docPr
'        Dim attributname As XmlAttribute = xmlStartPart.CreateAttribute("name")
'        attributname.InnerText = "openxml.png"
'        tagdocPr.SetAttributeNode(attributname)
'        Dim attributid As XmlAttribute = xmlStartPart.CreateAttribute("id")
'        attributid.InnerText = "1"
'        tagdocPr.SetAttributeNode(attributid)

'        'Element graphic
'        Dim taggraphic As XmlElement = xmlStartPart.CreateElement("a:graphic", DrawingML)
'        tagInline.AppendChild(taggraphic)
'        'Element graphicData
'        Dim taggraphicData As XmlElement = xmlStartPart.CreateElement("a:graphicData", DrawingML)
'        taggraphic.AppendChild(taggraphicData)
'        'attributs de graphicData
'        Dim attributuri As XmlAttribute = xmlStartPart.CreateAttribute("uri")
'        attributuri.InnerText = "http://schemas.openxmlformats.org/drawingml/2006/picture"
'        taggraphicData.SetAttributeNode(attributuri)

'        'Element pic
'        Dim tagpic As XmlElement = xmlStartPart.CreateElement("pic:pic", Pic)
'        taggraphicData.AppendChild(tagpic)

'        'Element nvPicPr
'        Dim tagnvPicPr As XmlElement = xmlStartPart.CreateElement("pic:nvPicPr", Pic)
'        tagpic.AppendChild(tagnvPicPr)
'        'Element cNvPr
'        Dim tagcNvPr As XmlElement = xmlStartPart.CreateElement("pic:cNvPr", Pic)
'        tagnvPicPr.AppendChild(tagcNvPr)
'        'attributs de cNvPr
'        Dim attributnamecNvPr As XmlAttribute = xmlStartPart.CreateAttribute("name")
'        attributnamecNvPr.InnerText = "openxml.jpg"
'        tagcNvPr.SetAttributeNode(attributnamecNvPr)
'        Dim attributidcNvPr As XmlAttribute = xmlStartPart.CreateAttribute("id")
'        attributidcNvPr.InnerText = "2"
'        tagcNvPr.SetAttributeNode(attributidcNvPr)
'        'Element cNvPicPr
'        Dim tagcNvPicPr As XmlElement = xmlStartPart.CreateElement("pic:cNvPicPr", Pic)
'        tagnvPicPr.AppendChild(tagcNvPicPr)

'        'Element blipFill
'        Dim tagblipFill As XmlElement = xmlStartPart.CreateElement("pic:blipFill", Pic)
'        tagpic.AppendChild(tagblipFill)
'        'Element blip
'        Dim tagblip As XmlElement = xmlStartPart.CreateElement("a:blip", DrawingML)
'        tagblipFill.AppendChild(tagblip)
'        'attribut de blip
'        Dim attributembed As XmlAttribute = xmlStartPart.CreateAttribute("r:embed", RelationShips)
'        attributembed.InnerText = "rId1"
'        tagblip.SetAttributeNode(attributembed)
'        'Element stretch
'        Dim tagstretch As XmlElement = xmlStartPart.CreateElement("a:stretch", DrawingML)
'        tagblipFill.AppendChild(tagstretch)
'        'Element fillRect
'        Dim tagfillRect As XmlElement = xmlStartPart.CreateElement("a:fillRect", DrawingML)
'        tagstretch.AppendChild(tagfillRect)

'        'Element spPr
'        Dim tagspPr As XmlElement = xmlStartPart.CreateElement("pic:spPr", Pic)
'        tagpic.AppendChild(tagspPr)
'        'Element xfrm
'        Dim tagxfrm As XmlElement = xmlStartPart.CreateElement("a:xfrm", DrawingML)
'        tagspPr.AppendChild(tagxfrm)

'        'Element off
'        Dim tagoff As XmlElement = xmlStartPart.CreateElement("a:off", DrawingML)
'        tagxfrm.AppendChild(tagoff)
'        'attributs de off
'        Dim attributx As XmlAttribute = xmlStartPart.CreateAttribute("x")
'        attributx.InnerText = "0"
'        tagoff.SetAttributeNode(attributx)
'        Dim attributy As XmlAttribute = xmlStartPart.CreateAttribute("y")
'        attributy.InnerText = "0"
'        tagoff.SetAttributeNode(attributy)
'        'Element ext
'        Dim tagext As XmlElement = xmlStartPart.CreateElement("a:ext", DrawingML)
'        tagxfrm.AppendChild(tagext)
'        'attributs de ext
'        Dim attributcxext As XmlAttribute = xmlStartPart.CreateAttribute("cx")
'        attributcxext.InnerText = "1300000"
'        tagext.SetAttributeNode(attributcxext)
'        Dim attributcyext As XmlAttribute = xmlStartPart.CreateAttribute("cy")
'        attributcyext.InnerText = "1300000"
'        tagext.SetAttributeNode(attributcyext)

'        'Element prstGeom
'        Dim tagprstGeom As XmlElement = xmlStartPart.CreateElement("a:prstGeom", DrawingML)
'        tagspPr.AppendChild(tagprstGeom)
'        'attributs de prstGeom
'        Dim attributprst As XmlAttribute = xmlStartPart.CreateAttribute("prst")
'        attributprst.InnerText = "rect"
'        tagprstGeom.SetAttributeNode(attributprst)

'        'Une fois l'arborescence créée, il nous faut l'intégrer dans un élément run au sein d'un XmlDocument.
'        'On crée ici un XmlDocument qui contiendra deux paragraphes : un pour du texte et un pour l'image.
'        'L'insertion du XmlElement précédemment créé se fait à la dernière ligne :
'        Dim tagDocument As XmlElement = xmlStartPart.CreateElement("w:document", WordprocessingML)
'        xmlStartPart.AppendChild(tagDocument)
'        Dim tagBody As XmlElement = xmlStartPart.CreateElement("w:body", WordprocessingML)
'        tagDocument.AppendChild(tagBody)
'        Dim tagParagraph As XmlElement = xmlStartPart.CreateElement("w:p", WordprocessingML)
'        tagBody.AppendChild(tagParagraph)
'        'création du premier run
'        Dim tagRun As XmlElement = xmlStartPart.CreateElement("w:r", WordprocessingML)
'        tagParagraph.AppendChild(tagRun)
'        'un peu de texte
'        Dim tagText As XmlElement = xmlStartPart.CreateElement("w:t", WordprocessingML)
'        tagRun.AppendChild(tagText)
'        tagText.InnerText = "Voici une image :"
'        tagRun.AppendChild(tagText)

'        'Nouveau paragraphe avec une image
'        Dim tagParagraph2 As XmlElement = xmlStartPart.CreateElement("w:p", WordprocessingML)
'        tagBody.AppendChild(tagParagraph2)
'        'création du 2e run
'        Dim tagRun2 As XmlElement = xmlStartPart.CreateElement("w:r", WordprocessingML)
'        tagParagraph2.AppendChild(tagRun2)

'        'insertion de l'image dans le run
'        tagRun2.AppendChild(tagDrawing)

'        '-------------
'        ' Création d'un nouveau package
'        Dim pkgOutputDoc As Package = Nothing
'        pkgOutputDoc = Package.Open("monFichier.docx", FileMode.Create, FileAccess.ReadWrite)
'        ' Sauvegarde du XmlDocument dans le un fichier document.xml dans le package
'        Dim uri = New Uri("/word/document.xml", UriKind.Relative)
'        Dim partDocumentXML As PackagePart = pkgOutputDoc.CreatePart(uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")
'        'pkgOutputDoc.CreatePart(uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")
'        Dim streamStartPart As New StreamWriter(partDocumentXML.GetStream(FileMode.Create, FileAccess.Write))
'        xmlStartPart.Save(streamStartPart)
'        streamStartPart.Close()

'        ' Création de la RelationShip 
'        pkgOutputDoc.CreateRelationship(uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId1")
'        '-------------------

'        'sauvegarde de l'image dans le package
'        uri = New Uri("/word/media/openxml.png", UriKind.Relative)
'        Dim partImage As PackagePart = pkgOutputDoc.CreatePart(uri, "image/png")
'        Dim targetStream As Stream = partImage.GetStream()

'        Dim sourceStream As New FileStream("C:\Users\User\Downloads\Orion_HST-631f.jpeg", FileMode.Open, FileAccess.Read)

'        Dim buffer() As Byte
'        ReDim buffer(1024)
'        Dim nrBytesWritten As Integer = sourceStream.Read(buffer, 0, 1024)
'        While (nrBytesWritten > 0)
'            targetStream.Write(buffer, 0, nrBytesWritten)
'            nrBytesWritten = sourceStream.Read(buffer, 0, 1024)
'        End While

'        ' Création de la RelationShip entre l'image et la part principale
'        partDocumentXML.CreateRelationship(uri, TargetMode.Internal, "http:'schemas.openxmlformats.org/officeDocument/2006/relationships/image", "rId1")

'        'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image est le type de la relation qui correspond au type de relation pour les images. Nous spécifions ici un identifiant de relation rId1 car c'est la seule relation que nous déclarons dans le fichier document.xml.rels. S'il existe déjà d'autres relations (pour les styles par exemple), il vaudra vérifier que l'identifiant est bien unique.
'        'Enfin, on ferme le package 
'        pkgOutputDoc.Flush()
'        pkgOutputDoc.Close()
'    End Sub
'    'Public Sub GenereBilanStructureDeb(sNomFichierBilan As String)
'    '    Dim Drawing As String = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
'    '    xmlStartPart = New XmlDocument()

'    '    tagDrawing = xmlStartPart.CreateElement("w:drawing", WordprocessingML)
'    '    Dim tagInline As XmlElement = xmlStartPart.CreateElement("wp:inline", Drawing)
'    '    tagDrawing.AppendChild(tagInline)
'    '    xmlStartPart = CreeArborescence(xmlStartPart, tagInline)
'    '    Dim tagDocument As XmlElement = xmlStartPart.CreateElement("w:document", WordprocessingML)
'    '    xmlStartPart.AppendChild(tagDocument)
'    '    tagBody = xmlStartPart.CreateElement("w:body", WordprocessingML)
'    '    tagDocument.AppendChild(tagBody)
'    '    pkgOutputDoc = Nothing
'    '    pkgOutputDoc = Package.Open(sNomFichierBilan, FileMode.Create, FileAccess.ReadWrite)
'    'End Sub
'    'Public Sub GenereBilanStructureMilieu(sTexteParagraphe As String, sNomImage As String)
'    '    '--------------------------------------------------------
'    '    Call AjouteParagraphe(xmlStartPart, tagBody, sTexteParagraphe)
'    '    Call AjouteParagraphe(xmlStartPart, tagBody, "")
'    '    Call AjouteParagrapheImage(xmlStartPart, tagBody, tagDrawing)

'    '    ' Création d'un nouveau package  
'    '    uri = New Uri("/word/document.xml", UriKind.Relative)
'    '    If partDocumentXML Is Nothing Then
'    '        partDocumentXML = pkgOutputDoc.CreatePart(uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")
'    '    End If
'    '    streamStartPart = New StreamWriter(partDocumentXML.GetStream(FileMode.Create, FileAccess.Write))
'    '    xmlStartPart.Save(streamStartPart)
'    '    streamStartPart.Close()

'    '    Call SauveImage(pkgOutputDoc, sNomImage, partDocumentXML)
'    '    ' Création de la RelationShip 
'    '    'pkgOutputDoc.CreateRelationship(uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId" & iOrdRelation)
'    '    iOrdRelation += 1
'    'End Sub
'    'Public Sub GenereBilanStructureFin()
'    '-------------------------------------------------------------------------------
'    ' Création de la RelationShip 
'    'pkgOutputDoc.CreateRelationship(uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId" & iOrdRelation)
'    'iOrdRelation += 1
'    Call FermePackage(pkgOutputDoc)
'    End Sub

'    'Marche 20240330
'    'Public Sub GenereBilanStructure()
'    '    Dim Drawing As String = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
'    '    'Dim xmlStartPart As New XmlDocument
'    '    ' Création du WordML
'    '    Dim xmlStartPart As XmlDocument = New XmlDocument()

'    '    Dim tagDrawing As XmlElement = xmlStartPart.CreateElement("w:drawing", WordprocessingML)
'    '    Dim tagInline As XmlElement = xmlStartPart.CreateElement("wp:inline", Drawing)
'    '    tagDrawing.AppendChild(tagInline)
'    '    xmlStartPart = CreeArborescence(xmlStartPart, tagInline)
'    '    Dim tagDocument As XmlElement = xmlStartPart.CreateElement("w:document", WordprocessingML)
'    '    xmlStartPart.AppendChild(tagDocument)
'    '    Dim tagBody As XmlElement = xmlStartPart.CreateElement("w:body", WordprocessingML)
'    '    tagDocument.AppendChild(tagBody)
'    '    '--------------------------------------------------------
'    '    Call AjouteParagraphe(xmlStartPart, tagBody, "glop b")
'    '    Call AjouteParagrapheImage(xmlStartPart, tagBody, tagDrawing)

'    '    ' Création d'un nouveau package
'    '    Dim pkgOutputDoc As Package = Nothing
'    '    pkgOutputDoc = Package.Open("monFichier.docx", FileMode.Create, FileAccess.ReadWrite)
'    '    ' Sauvegarde du XmlDocument dans le un fichier document.xml dans le package
'    '    Dim uri = New Uri("/word/document.xml", UriKind.Relative)
'    '    Dim partDocumentXML As PackagePart = pkgOutputDoc.CreatePart(uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")
'    '    'pkgOutputDoc.CreatePart(uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")
'    '    Dim streamStartPart As New StreamWriter(partDocumentXML.GetStream(FileMode.Create, FileAccess.Write))
'    '    xmlStartPart.Save(streamStartPart)
'    '    streamStartPart.Close()

'    '    Call SauveImage(pkgOutputDoc, "C:\Users\User\Downloads\Orion_HST-631f.jpeg", partDocumentXML)
'    '    '-------------------------------------------------------------------------------
'    '    ' Création de la RelationShip 
'    '    pkgOutputDoc.CreateRelationship(uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId1")
'    '    Call FermePackage(pkgOutputDoc)
'    'End Sub
'    Private Sub AjouteParagrapheImage(xmlStartPart As XmlDocument, tagBody As XmlElement, tagDrawing As XmlElement)
'        'Nouveau paragraphe avec une image
'        Dim tagParagraph2 As XmlElement = xmlStartPart.CreateElement("w:p", WordprocessingML)
'        tagBody.AppendChild(tagParagraph2)
'        'création du 2e run
'        Dim tagRun2 As XmlElement = xmlStartPart.CreateElement("w:r", WordprocessingML)
'        tagParagraph2.AppendChild(tagRun2)

'        'insertion de l'image dans le run
'        tagRun2.AppendChild(tagDrawing)
'    End Sub
'    Private Sub SauveImage(pkgOutputDoc As Package, sImage As String, partDocumentXML As PackagePart)
'        'sauvegarde de l'image dans le package
'        'Dim uri = New Uri("/word/media/openxml.png", UriKind.Relative)
'        Dim uri = New Uri("/word/media/" & Mid(sImage, InStrRev(sImage, "\") + 1), UriKind.Relative)
'        'Dim partImage As PackagePart = pkgOutputDoc.CreatePart(uri, "image/jpeg")
'        Dim partImage As PackagePart = pkgOutputDoc.CreatePart(uri, System.Net.Mime.MediaTypeNames.Image.Jpeg)
'        Dim targetStream As Stream = partImage.GetStream()

'        Dim sourceStream As New FileStream(sImage, FileMode.Open, FileAccess.Read)

'        Dim buffer() As Byte
'        ReDim buffer(1024)
'        Dim nrBytesWritten As Integer = sourceStream.Read(buffer, 0, 1024)
'        While (nrBytesWritten > 0)
'            targetStream.Write(buffer, 0, nrBytesWritten)
'            nrBytesWritten = sourceStream.Read(buffer, 0, 1024)
'        End While
'        ' Création de la RelationShip entre l'image et la part principale
'        iOrdRelation += 1
'        partDocumentXML.CreateRelationship(uri, TargetMode.Internal, "http:'schemas.openxmlformats.org/officeDocument/2006/relationships/image", "rId" & iOrdRelation)
'    End Sub
'    Private Sub FermePackage(pkgOutputDoc As Package)
'        Enfin, on ferme le package 
'        pkgOutputDoc.Flush()
'        pkgOutputDoc.Close()
'    End Sub
'    Private Sub AjouteTableau()
'        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-insert-a-table-into-a-word-processing-document?tabs=cs-0%2Ccs-1%2Cvb-2%2Cvb-3%2Cvb-4%2Cvb
'    End Sub
'    Private Function CreeArborescence(xmlStartPart As XmlDocument, tagInline As XmlElement) As XmlDocument
'        Dim WordprocessingML As String = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
'        Dim RelationShips As String = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
'        Dim Drawing As String = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
'        Dim DrawingML As String = "http://schemas.openxmlformats.org/drawingml/2006/main"
'        Dim Pic As String = "http://schemas.openxmlformats.org/drawingml/2006/picture"

'        '' Création du WordML
'        'Dim xmlStartPart As XmlDocument = New XmlDocument()

'        'Dim tagDrawing As XmlElement = xmlStartPart.CreateElement("w:drawing", WordprocessingML)
'        'Dim tagInline As XmlElement = xmlStartPart.CreateElement("wp:inline", Drawing)
'        'tagDrawing.AppendChild(tagInline)

'        'Element extent
'        Dim tagExtent As XmlElement = xmlStartPart.CreateElement("wp:extent", Drawing)
'        tagInline.AppendChild(tagExtent)
'        'attributs de extent
'        Dim attributcx As XmlAttribute = xmlStartPart.CreateAttribute("cx")
'        attributcx.InnerText = "1300000"
'        tagExtent.SetAttributeNode(attributcx)
'        Dim attributcy As XmlAttribute = xmlStartPart.CreateAttribute("cy")
'        attributcy.InnerText = "1300000"
'        tagExtent.SetAttributeNode(attributcy)

'        'Element docPr
'        Dim tagdocPr As XmlElement = xmlStartPart.CreateElement("wp:docPr", Drawing)
'        tagInline.AppendChild(tagdocPr)
'        'attributs de docPr
'        Dim attributname As XmlAttribute = xmlStartPart.CreateAttribute("name")
'        'attributname.InnerText = "openxml.png"
'        attributname.InnerText = "orion.jpeg"
'        tagdocPr.SetAttributeNode(attributname)
'        Dim attributid As XmlAttribute = xmlStartPart.CreateAttribute("id")
'        attributid.InnerText = "1"
'        tagdocPr.SetAttributeNode(attributid)

'        'Element graphic
'        Dim taggraphic As XmlElement = xmlStartPart.CreateElement("a:graphic", DrawingML)
'        tagInline.AppendChild(taggraphic)
'        'Element graphicData
'        Dim taggraphicData As XmlElement = xmlStartPart.CreateElement("a:graphicData", DrawingML)
'        taggraphic.AppendChild(taggraphicData)
'        'attributs de graphicData
'        Dim attributuri As XmlAttribute = xmlStartPart.CreateAttribute("uri")
'        attributuri.InnerText = "http://schemas.openxmlformats.org/drawingml/2006/picture"
'        taggraphicData.SetAttributeNode(attributuri)

'        'Element pic
'        Dim tagpic As XmlElement = xmlStartPart.CreateElement("pic:pic", Pic)
'        taggraphicData.AppendChild(tagpic)

'        'Element nvPicPr
'        Dim tagnvPicPr As XmlElement = xmlStartPart.CreateElement("pic:nvPicPr", Pic)
'        tagpic.AppendChild(tagnvPicPr)
'        'Element cNvPr
'        Dim tagcNvPr As XmlElement = xmlStartPart.CreateElement("pic:cNvPr", Pic)
'        tagnvPicPr.AppendChild(tagcNvPr)
'        'attributs de cNvPr
'        Dim attributnamecNvPr As XmlAttribute = xmlStartPart.CreateAttribute("name")
'        attributnamecNvPr.InnerText = "openxml.jpg"
'        tagcNvPr.SetAttributeNode(attributnamecNvPr)
'        Dim attributidcNvPr As XmlAttribute = xmlStartPart.CreateAttribute("id")
'        attributidcNvPr.InnerText = "2"
'        tagcNvPr.SetAttributeNode(attributidcNvPr)
'        'Element cNvPicPr
'        Dim tagcNvPicPr As XmlElement = xmlStartPart.CreateElement("pic:cNvPicPr", Pic)
'        tagnvPicPr.AppendChild(tagcNvPicPr)

'        'Element blipFill
'        Dim tagblipFill As XmlElement = xmlStartPart.CreateElement("pic:blipFill", Pic)
'        tagpic.AppendChild(tagblipFill)
'        'Element blip
'        Dim tagblip As XmlElement = xmlStartPart.CreateElement("a:blip", DrawingML)
'        tagblipFill.AppendChild(tagblip)
'        'attribut de blip
'        Dim attributembed As XmlAttribute = xmlStartPart.CreateAttribute("r:embed", RelationShips)
'        attributembed.InnerText = "rId1"
'        tagblip.SetAttributeNode(attributembed)
'        'Element stretch
'        Dim tagstretch As XmlElement = xmlStartPart.CreateElement("a:stretch", DrawingML)
'        tagblipFill.AppendChild(tagstretch)
'        'Element fillRect
'        Dim tagfillRect As XmlElement = xmlStartPart.CreateElement("a:fillRect", DrawingML)
'        tagstretch.AppendChild(tagfillRect)

'        'Element spPr
'        Dim tagspPr As XmlElement = xmlStartPart.CreateElement("pic:spPr", Pic)
'        tagpic.AppendChild(tagspPr)
'        'Element xfrm
'        Dim tagxfrm As XmlElement = xmlStartPart.CreateElement("a:xfrm", DrawingML)
'        tagspPr.AppendChild(tagxfrm)

'        'Element off
'        Dim tagoff As XmlElement = xmlStartPart.CreateElement("a:off", DrawingML)
'        tagxfrm.AppendChild(tagoff)
'        'attributs de off
'        Dim attributx As XmlAttribute = xmlStartPart.CreateAttribute("x")
'        attributx.InnerText = "0"
'        tagoff.SetAttributeNode(attributx)
'        Dim attributy As XmlAttribute = xmlStartPart.CreateAttribute("y")
'        attributy.InnerText = "0"
'        tagoff.SetAttributeNode(attributy)
'        'Element ext
'        Dim tagext As XmlElement = xmlStartPart.CreateElement("a:ext", DrawingML)
'        tagxfrm.AppendChild(tagext)
'        'attributs de ext
'        Dim attributcxext As XmlAttribute = xmlStartPart.CreateAttribute("cx")
'        attributcxext.InnerText = "1300000"
'        tagext.SetAttributeNode(attributcxext)
'        Dim attributcyext As XmlAttribute = xmlStartPart.CreateAttribute("cy")
'        attributcyext.InnerText = "1300000"
'        tagext.SetAttributeNode(attributcyext)

'        'Element prstGeom
'        Dim tagprstGeom As XmlElement = xmlStartPart.CreateElement("a:prstGeom", DrawingML)
'        tagspPr.AppendChild(tagprstGeom)
'        'attributs de prstGeom
'        Dim attributprst As XmlAttribute = xmlStartPart.CreateAttribute("prst")
'        attributprst.InnerText = "rect"
'        tagprstGeom.SetAttributeNode(attributprst)

'        Return xmlStartPart
'    End Function
'    Public Sub AjouteTableau(ByVal fileName As String)
'        'https://learn.microsoft.com/fr-fr/office/open-xml/word/how-to-insert-a-table-into-a-word-processing-document?tabs=cs-0%2Ccs-1%2Cvb-2%2Cvb-3%2Cvb-4%2Cvb
'        ' Insert a table into a word processing document. 
'        ' Use the file name and path passed in as an argument 
'        ' to open an existing Word 2007 document.

'        Using doc As WordprocessingDocument = WordprocessingDocument.Open(fileName, True)
'            ' Create an empty table.
'            Dim table As New Table()

'            ' Create a TableProperties object and specify its border information.
'            Dim tblProp As New TableProperties(New TableBorders(
'            New TopBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
'            New BottomBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
'            New LeftBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
'            New RightBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
'            New InsideHorizontalBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
'            New InsideVerticalBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24}))
'            ' Append the TableProperties object to the empty table.
'            table.AppendChild(Of TableProperties)(tblProp)

'            ' Create a row.
'            Dim tr As New TableRow()

'            ' Create a cell.
'            Dim tc1 As New TableCell()

'            ' Specify the width property of the table cell.
'            tc1.Append(New TableCellProperties(New TableCellWidth()))

'            ' Specify the table cell content.
'            tc1.Append(New Wordprocessing.Paragraph(New Run(New Text("some text"))))

'            ' Append the table cell to the table row.
'            tr.Append(tc1)

'            ' Create a second table cell by copying the OuterXml value of the first table cell.
'            Dim tc2 As New TableCell(tc1.OuterXml)

'            ' Append the table cell to the table row.
'            tr.Append(tc2)

'            ' Append the table row to the table.
'            table.Append(tr)

'            ' Append the table to the document.
'            doc.MainDocumentPart.Document.Body.Append(table)
'        End Using
'    End Sub
'End Module



''Imports DocumentFormat.OpenXml
''Imports DocumentFormat.OpenXml.Packaging
''Imports DocumentFormat.OpenXml.Wordprocessing
''Imports BottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder
''Imports LeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder
''Imports RightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder
''Imports Run = DocumentFormat.OpenXml.Wordprocessing.Run
''Imports Table = DocumentFormat.OpenXml.Wordprocessing.Table
''Imports Text = DocumentFormat.OpenXml.Wordprocessing.Text
''Imports TopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder

''Module MyModule

''    Sub Main(args As String())
''    End Sub


''    ' Insert a table into a word processing document.
''    Public Sub CreateTable(ByVal fileName As String)
''        ' Use the file name and path passed in as an argument 
''        ' to open an existing Word 2007 document.

''        Using doc As WordprocessingDocument = WordprocessingDocument.Open(fileName, True)
''            ' Create an empty table.
''            Dim table As New Table()

''            ' Create a TableProperties object and specify its border information.
''            Dim tblProp As New TableProperties(New TableBorders(
''            New TopBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
''            New BottomBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
''            New LeftBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
''            New RightBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
''            New InsideHorizontalBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24},
''            New InsideVerticalBorder() With {.Val = New EnumValue(Of BorderValues)(BorderValues.Dashed), .Size = 24}))
''            ' Append the TableProperties object to the empty table.
''            table.AppendChild(Of TableProperties)(tblProp)

''            ' Create a row.
''            Dim tr As New TableRow()

''            ' Create a cell.
''            Dim tc1 As New TableCell()

''            ' Specify the width property of the table cell.
''            tc1.Append(New TableCellProperties(New TableCellWidth()))

''            ' Specify the table cell content.
''            tc1.Append(New Paragraph(New Run(New Text("some text"))))

''            ' Append the table cell to the table row.
''            tr.Append(tc1)

''            ' Create a second table cell by copying the OuterXml value of the first table cell.
''            Dim tc2 As New TableCell(tc1.OuterXml)

''            ' Append the table cell to the table row.
''            tr.Append(tc2)

''            ' Append the table row to the table.
''            table.Append(tr)

''            ' Append the table to the document.
''            doc.MainDocumentPart.Document.Body.Append(table)
''        End Using
''    End Sub
''End Module


''Imports System.IO
''Imports System.Net.Mime.MediaTypeNames
''Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Tab
''Imports DocumentFormat.OpenXml
''Imports DocumentFormat.OpenXml.Drawing
''Imports DocumentFormat.OpenXml.Packaging
''Imports DocumentFormat.OpenXml.Spreadsheet
''Imports DocumentFormat.OpenXml.Wordprocessing

''Module genereBilans
''    Dim path = "c:\temp\test.docx"

''    Public Sub genereOpenXml()
''        Using ms As New MemoryStream
''            Using doc As WordprocessingDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, True)
''                Dim docBody As New DocumentFormat.OpenXml.Wordprocessing.Body()
''                Dim mainPart As MainDocumentPart = doc.AddMainDocumentPart
''                mainPart.Document = New Document()
''                doc.MainDocumentPart.Document.Append(docBody)
''                'doc.AddMainDocumentPart.Document = New Document()

''                Dim imagePart As ImagePart = mainPart.AddImagePart(ImagePartType.Jpeg)
''                Using stream As New FileStream("D:\Logo\Icons\logo_c1.png", FileMode.Open)
''                    imagePart.FeedData(stream)
''                End Using

''                AddImageToBody(doc, mainPart.GetIdOfPart(imagePart), True)
''                AddParagraphe(doc, 22, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris sit amet dui ante. Sed eget purus in nisi tincidunt vulputate vel at turpis. Nullam non pharetra quam, non malesuada nunc. In hac habitasse platea dictumst. Praesent suscipit nibh non leo vulputate consectetur. Aliquam est nibh, pretium vel arcu sit amet, scelerisque facilisis velit. Pellentesque orci odio, iaculis non eros id, convallis mattis nibh. Aenean vehicula lacus vel pellentesque iaculis. Curabitur nec semper orci, a sagittis tellus. Duis mollis placerat elit vel rutrum. Proin mollis libero sed ex pretium, a ornare nibh malesuada. Maecenas ultrices purus vel nisl cursus, consectetur vestibulum massa dignissim. Nam imperdiet mattis mi scelerisque dictum. Phasellus non ullamcorper ligula, eleifend porta tortor. Vestibulum odio dolor, vehicula non suscipit non, euismod vitae augue.
''Maecenas viverra leo neque, interdum maximus dui luctus id. Quisque commodo, lorem sed ullamcorper dignissim, neque nibh venenatis leo, sed imperdiet risus libero eu risus. Nulla ultrices, justo in eleifend bibendum, sem nulla egestas augue, sit amet consequat nulla dui sit amet dui. Quisque feugiat libero eu ex dignissim pellentesque. Curabitur aliquam ex eu consectetur congue. Suspendisse venenatis neque vel odio scelerisque, at mattis magna venenatis. Nulla facilisi. Fusce pulvinar consectetur viverra. Duis eu eros vitae arcu viverra dapibus. Aliquam porttitor elit vel elit dignissim, ut blandit tortor molestie.")
''                AddPage(doc)

''                If IO.File.Exists(path) Then
''                    IO.File.Delete(path)
''                End If

''                doc.Save()
''            End Using
''        End Using
''    End Sub

''    Private Sub AddParagraphe(ByVal wordDoc As WordprocessingDocument, ByVal fontSize As Integer, ByVal txt As String)
''        Dim runFont As New Wordprocessing.RunFonts With {.Ascii = "Comic Sans MS"}
''        Dim runProp As New DocumentFormat.OpenXml.Drawing.RunProperties
''        Dim runPara As New DocumentFormat.OpenXml.Spreadsheet.Run
''        Dim para As New DocumentFormat.OpenXml.Wordprocessing.Paragraph

''        runProp.Append(runFont)
''        runProp.FontSize = New DocumentFormat.OpenXml.Spreadsheet.FontSize() With {.Val = fontSize}

''        runPara.Append(runProp)
''        runPara.Append(New DocumentFormat.OpenXml.Drawing.Text(txt))
''        para.Append(runPara)

''        wordDoc.MainDocumentPart.Document.AppendChild(para)
''    End Sub

''    Private Sub AddImageToBody(ByVal wordDoc As WordprocessingDocument, ByVal relationshipId As String, ByVal Optional center As Boolean = False)
''        Dim img = New BitmapImage(New Uri("D:\Logo\Icons\logo_c1.png", UriKind.RelativeOrAbsolute))
''        Dim iWidth As Integer = CInt(System.Math.Round(img.Width * 9525))
''        Dim iHeight As Integer = CInt(System.Math.Round(img.Height * 9525))

''        ' Define the reference of the image.
''        Dim element = New DocumentFormat.OpenXml.Wordprocessing.Drawing(
''                              New DW.Inline(
''                          New DW.Extent() With {.Cx = iWidth, .Cy = iHeight},
''                          New DW.EffectExtent() With {.LeftEdge = 0L, .TopEdge = 0L, .RightEdge = 0L, .BottomEdge = 0L},
''                          New DW.DocProperties() With {.Id = CType(1UI, UInt32Value), .Name = "Picture1"},
''                          New DW.NonVisualGraphicFrameDrawingProperties(
''                              New A.GraphicFrameLocks() With {.NoChangeAspect = True}
''                              ),
''                          New A.Graphic(New A.GraphicData(
''                                        New PIC.Picture(
''                                            New PIC.NonVisualPictureProperties(
''                                                New PIC.NonVisualDrawingProperties() With {.Id = 0UI, .Name = "Koala.jpg"},
''                                                New PIC.NonVisualPictureDrawingProperties()
''                                                ),
''                                            New PIC.BlipFill(
''                                                New A.Blip(
''                                                    New A.BlipExtensionList(
''                                                        New A.BlipExtension() With {.Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"})
''                                                    ) With {.Embed = relationshipId, .CompressionState = A.BlipCompressionValues.Print},
''                                                New A.Stretch(
''                                                    New A.FillRectangle()
''                                                    )
''                                                ),
''                                            New PIC.ShapeProperties(
''                                                New A.Transform2D(
''                                                    New A.Offset() With {.X = 0L, .Y = 0L},
''                                                    New A.Extents() With {.Cx = iWidth, .Cy = iHeight}),
''                                                New A.PresetGeometry(
''                                                    New A.AdjustValueList()
''                                                    ) With {.Preset = A.ShapeTypeValues.Rectangle}
''                                                )
''                                            )
''                                        ) With {.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"}
''                                    )
''                                ) With {.DistanceFromTop = 0UI,
''                                        .DistanceFromBottom = 0UI,
''                                        .DistanceFromLeft = 0UI,
''                                        .DistanceFromRight = 0UI})

''        Dim para As New DocumentFormat.OpenXml.Drawing.Paragraph
''        Dim justification As New Justification With {.Val = JustificationValues.Center}
''        Dim paraProp As New DocumentFormat.OpenXml.Drawing.ParagraphProperties()
''        Dim run As New DocumentFormat.OpenXml.Drawing.Run()

''        If center Then
''            paraProp.Append(justification)
''            para.Append(paraProp)
''        End If

''        run.Append(element)
''        para.Append(run)
''        ' Append the reference to body, the element should be in a Run.
''        wordDoc.MainDocumentPart.Document.Body.AppendChild(para) 'New Paragraph(New Run(element)))
''    End Sub

''    Private Sub AddPage(ByVal wordDoc As WordprocessingDocument)
''        wordDoc.MainDocumentPart.Document.Body.Append(New DocumentFormat.OpenXml.Drawing.Paragraph(New DocumentFormat.OpenXml.Drawing.Run(New DocumentFormat.OpenXml.Drawing.Break() With {.Type = BreakValues.Page})))
''    End Sub
''End Module
