Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestSummaryFileReaderTests
    Private Const Hash As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    Private Shared Function Fixture() As String
        Return "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""scan-fictif.pdf"",""hash"":""" & Hash & """,""status"":""review"",""reason"":""analyse simulee""}]}"
    End Function

    <TestMethod>
    Public Sub LectureBilanFictif_EstSansModificationDuFichier()
        Dim folder = Path.Combine(Path.GetTempPath(), "gt-summary-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Dim file = Path.Combine(folder, "summary.json")
        Try
            Dim original = Encoding.UTF8.GetBytes(Fixture())
            File.WriteAllBytes(file, original)
            Dim summary = DocumentBatchTestSummaryFileReader.Read(file)
            Assert.AreEqual(1, summary.Total)
            Assert.IsTrue(summary.RequiresReview)
            Assert.AreEqual("scan-fictif.pdf", summary.Documents(0).Name)
            CollectionAssert.AreEqual(original, File.ReadAllBytes(file))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub BilanSurdimensionne_EstRefuse()
        Dim file = Path.Combine(Path.GetTempPath(), "gt-summary-" & Guid.NewGuid().ToString("N") & ".json")
        Try
            File.WriteAllBytes(file, New Byte(1024 * 1024) {})
            Assert.ThrowsExactly(Of FormatException)(Sub() DocumentBatchTestSummaryFileReader.Read(file))
        Finally
            If File.Exists(file) Then File.Delete(file)
        End Try
    End Sub

    <TestMethod>
    Public Sub EncodageUtf8Invalide_EstRefuse()
        Dim file = Path.Combine(Path.GetTempPath(), "gt-summary-" & Guid.NewGuid().ToString("N") & ".json")
        Try
            File.WriteAllBytes(file, New Byte() {&HC3, &H28})
            Assert.ThrowsExactly(Of DecoderFallbackException)(Sub() DocumentBatchTestSummaryFileReader.Read(file))
        Finally
            If File.Exists(file) Then File.Delete(file)
        End Try
    End Sub
End Class
