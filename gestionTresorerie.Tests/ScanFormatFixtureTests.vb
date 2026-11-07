Option Strict On

Imports System
Imports System.Drawing
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ScanFormatFixtureTests
    <TestMethod>
    Public Sub ApercuJpegEtTiffFictifsSansVerrouillerLesOriginaux()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            For Each format In New Imaging.ImageFormat() {Imaging.ImageFormat.Jpeg, Imaging.ImageFormat.Tiff}
                Dim extension = If(format.Guid = Imaging.ImageFormat.Jpeg.Guid, ".jpg", ".tiff")
                Dim chemin = Path.Combine(root, "scan-fictif" & extension)
                Using image As New Bitmap(9, 6)
                    image.Save(chemin, format)
                End Using
                Assert.IsTrue(DocumentFilePreview.Supports(chemin))
                Using apercu = DocumentFilePreview.ReadImage(chemin)
                    Assert.AreEqual(9, apercu.Width)
                    Assert.AreEqual(6, apercu.Height)
                    File.Delete(chemin)
                    Assert.IsFalse(File.Exists(chemin))
                End Using
            Next
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ApercuPngFictifEtExtensionInsensibleALaCasse()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim chemin = Path.Combine(root, "SCAN-FICTIF.PNG")
            Using image As New Bitmap(11, 8)
                image.SetPixel(0, 0, Color.Red)
                image.Save(chemin, Imaging.ImageFormat.Png)
            End Using
            Assert.IsTrue(DocumentFilePreview.Supports(chemin))
            Using apercu = DocumentFilePreview.ReadImage(chemin)
                Assert.AreEqual(11, apercu.Width)
                Assert.AreEqual(8, apercu.Height)
                Assert.AreEqual(Color.Red.ToArgb(), apercu.GetPixel(0, 0).ToArgb())
            End Using
            File.Delete(chemin)
            Assert.IsFalse(File.Exists(chemin))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ExtensionsJpegEtTifCourtesAvecImagesFictives()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            For Each extension In New String() {".JPEG", ".TIF"}
                Dim chemin = Path.Combine(root, "scan-fictif" & extension)
                Dim format = If(extension = ".JPEG", Imaging.ImageFormat.Jpeg, Imaging.ImageFormat.Tiff)
                Using image As New Bitmap(7, 5)
                    image.Save(chemin, format)
                End Using
                Assert.IsTrue(DocumentFilePreview.Supports(chemin))
                Using apercu = DocumentFilePreview.ReadImage(chemin)
                    Assert.AreEqual(7, apercu.Width)
                    Assert.AreEqual(5, apercu.Height)
                End Using
                File.Delete(chemin)
                Assert.IsFalse(File.Exists(chemin))
            Next
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub FichierTronqueOuTropVolumineuxEstRejeteSansModification()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim tronque = Path.Combine(root, "scan.png")
            Dim octets = New Byte() {&H89, &H50, &H4E, &H47}
            File.WriteAllBytes(tronque, octets)
            Dim erreur As Exception = Nothing
            Try
                Using apercu = DocumentFilePreview.ReadImage(tronque)
                End Using
            Catch ex As ArgumentException
                erreur = ex
            Catch ex As OutOfMemoryException
                ' GDI+ signale parfois une image invalide par OutOfMemoryException.
                erreur = ex
            End Try
            Assert.IsNotNull(erreur, "Un fichier PNG tronqué ne doit pas être accepté.")
            CollectionAssert.AreEqual(octets, File.ReadAllBytes(tronque))

            Dim volumineux = Path.Combine(root, "scan.jpg")
            Using flux As New FileStream(volumineux, FileMode.CreateNew, FileAccess.Write)
                flux.SetLength(25L * 1024 * 1024 + 1)
            End Using
            Assert.ThrowsException(Of InvalidDataException)(Sub() DocumentFilePreview.ReadImage(volumineux))
            Assert.AreEqual(25L * 1024 * 1024 + 1, New FileInfo(volumineux).Length)
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub
    <TestMethod>
    Public Sub ApercuTiffMultipageFictifAfficheLaPremierePageSansVerrouillage()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim chemin = Path.Combine(root, "scan-multipage-fictif.tiff")
            Dim codec = Array.Find(Imaging.ImageCodecInfo.GetImageEncoders(),
                                   Function(item) item.FormatID = Imaging.ImageFormat.Tiff.Guid)
            Assert.IsNotNull(codec, "Le codec TIFF Windows doit être disponible.")
            Dim saveFlag = Imaging.Encoder.SaveFlag
            Using first As New Bitmap(13, 9), second As New Bitmap(5, 4)
                first.SetPixel(0, 0, Color.Red)
                second.SetPixel(0, 0, Color.Blue)
                Using parameters As New Imaging.EncoderParameters(1)
                    parameters.Param(0) = New Imaging.EncoderParameter(saveFlag, CLng(Imaging.EncoderValue.MultiFrame))
                    first.Save(chemin, codec, parameters)
                    parameters.Param(0) = New Imaging.EncoderParameter(saveFlag, CLng(Imaging.EncoderValue.FrameDimensionPage))
                    first.SaveAdd(second, parameters)
                    parameters.Param(0) = New Imaging.EncoderParameter(saveFlag, CLng(Imaging.EncoderValue.Flush))
                    first.SaveAdd(parameters)
                End Using
            End Using
            Using source = Image.FromFile(chemin)
                Assert.AreEqual(2, source.GetFrameCount(Imaging.FrameDimension.Page),
                                "La fixture doit réellement contenir deux pages.")
            End Using
            Using apercu = DocumentFilePreview.ReadImage(chemin)
                Assert.AreEqual(13, apercu.Width)
                Assert.AreEqual(9, apercu.Height)
                Assert.AreEqual(Color.Red.ToArgb(), apercu.GetPixel(0, 0).ToArgb())
                File.Delete(chemin)
                Assert.IsFalse(File.Exists(chemin), "Le TIFF source ne doit pas rester verrouillé.")
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

End Class
