'Public Class WordImageHelper

'    ''' <summary>
'    ''' Ajoute une image à un Body de document Word.
'    ''' </summary>
'    ''' <param name="body">Le Body du document Word</param>
'    ''' <param name="imagePartId">L'ID de l'image (ImagePart)</param>
'    ''' <param name="widthEmu">Largeur en EMUs (1 cm ≈ 360000 EMU)</param>
'    ''' <param name="heightEmu">Hauteur en EMUs</param>
'    Public Shared Sub AddImageToBody(body As Body, imagePartId As String, Optional widthEmu As Long = 3500000, Optional heightEmu As Long = 2000000)
'        ' Créer la Picture
'        Dim picture = New Drawing.Pictures.Picture(
'            New Drawing.Pictures.NonVisualPictureProperties(
'                New Drawing.Pictures.NonVisualDrawingProperties() With {.Id = CType(0UI, UInt32), .Name = "Image"},
'                New Drawing.Pictures.NonVisualPictureDrawingProperties()
'            ),
'            New Drawing.Pictures.BlipFill(
'                New Drawing.Blip() With {.Embed = imagePartId},
'                New Drawing.Stretch(New Drawing.FillRectangle())
'            ),
'            New Drawing.Pictures.ShapeProperties()
'        )

'        ' Créer GraphicData et assigner l'Uri
'        Dim graphicData = New Drawing.GraphicData(picture)
'        graphicData.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"

'        ' Créer Graphic
'        Dim graphic = New Drawing.Graphic(graphicData)

'        ' Créer Inline
'        Dim inline = New Drawing.Wordprocessing.Inline(
'            New Drawing.Wordprocessing.Extent() With {.Cx = widthEmu, .Cy = heightEmu},
'            New Drawing.Wordprocessing.EffectExtent() With {.LeftEdge = 0L, .TopEdge = 0L, .RightEdge = 0L, .BottomEdge = 0L},
'            New Drawing.Wordprocessing.DocProperties() With {.Id = CType(1UI, UInt32), .Name = "Picture"},
'            New Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
'                New Drawing.GraphicFrameLocks() With {.NoChangeAspect = True}
'            ),
'            graphic
'        )

'        ' Créer le Drawing Wordprocessing
'        Dim element = New DocumentFormat.OpenXml.Wordprocessing.Drawing(inline)

'        ' Ajouter au paragraphe et au body
'        Dim para = New Paragraph(New Run(element))
'        body.Append(para)
'    End Sub

'End Class
