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
End Class
