Option Strict On

Imports System
Imports System.Drawing
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OfflineImageOcrTests
    <TestMethod>
    Public Sub RefuseUnMoteurAbsentSansModifierLeScanFictif()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim scan = Path.Combine(root, "scan-fictif.png")
            Using image As New Bitmap(12, 8)
                image.Save(scan, Imaging.ImageFormat.Png)
            End Using
            Dim avant = File.ReadAllBytes(scan)
            Assert.ThrowsException(Of FileNotFoundException)(
                Sub() OfflineImageOcr.Recognize(scan, Path.Combine(root, "moteur-absent.exe")))
            CollectionAssert.AreEqual(avant, File.ReadAllBytes(scan))
            File.Delete(scan)
            Assert.IsFalse(File.Exists(scan))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub RefuseParametresDangereuxSansLancerDeProcessus()
        Assert.ThrowsException(Of ArgumentException)(
            Sub() OfflineImageOcr.Recognize("scan.png", ""))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() OfflineImageOcr.Recognize("scan.png", "moteur.exe", timeoutMilliseconds:=0))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() OfflineImageOcr.Recognize("scan.png", "moteur.exe", language:="fra;cmd"))
    End Sub
End Class
