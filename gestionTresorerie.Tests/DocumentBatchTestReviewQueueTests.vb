Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewQueueTests
    Private Const HashA As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
    Private Const HashB As String = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
    Private Const HashC As String = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"

    Private Shared Function Row(name As String, hash As String, status As String) As String
        Return "{""name"":""" & name & """,""hash"":""" & hash & """,""status"":""" & status & """,""reason"":""simule""}"
    End Function

    Private Shared Function Fixture() As String
        Return "{""schema_version"":1,""total"":3,""valid"":1,""review"":1,""skipped"":1,""requires_review"":true,""documents"":[" &
               Row("facture-fictive.pdf", HashA, "valid") & "," &
               Row("cheque-fictif.png", HashB, "review") & "," &
               Row("recu-fictif.pdf", HashC, "skipped") & "]}"
    End Function

    <TestMethod>
    Public Sub SeulsLesDocumentsAExaminerSontProposes_DansOrdreDuLot()
        Dim summary = DocumentBatchTestSummary.Parse(Fixture())
        Dim pending = DocumentBatchTestReviewQueue.FromSummary(summary)
        Assert.AreEqual(1, pending.Count)
        Assert.AreEqual("cheque-fictif.png", pending(0).Name)
        Assert.AreEqual(HashB, pending(0).Hash)
        Assert.AreEqual("review", pending(0).Status)
        Assert.AreEqual(3, summary.Documents.Count)
    End Sub

    <TestMethod>
    Public Sub RepriseSansExamen_RetourneUneFileVide()
        Dim json = "{""schema_version"":1,""total"":1,""valid"":0,""review"":0,""skipped"":1,""requires_review"":false,""documents"":[" &
                   Row("deja-traite.pdf", HashA, "skipped") & "]}"
        Assert.AreEqual(0, DocumentBatchTestReviewQueue.FromSummary(DocumentBatchTestSummary.Parse(json)).Count)
    End Sub

    <TestMethod>
    Public Sub LectureDepuisFichierFictif_NeModifiePasLeBilan()
        Dim file = Path.Combine(Path.GetTempPath(), "gt-review-" & Guid.NewGuid().ToString("N") & ".json")
        Try
            Dim original = Encoding.UTF8.GetBytes(Fixture())
            File.WriteAllBytes(file, original)
            Dim pending = DocumentBatchTestReviewQueue.FromFile(file)
            Assert.AreEqual(1, pending.Count)
            CollectionAssert.AreEqual(original, File.ReadAllBytes(file))
        Finally
            If File.Exists(file) Then File.Delete(file)
        End Try
    End Sub

    <TestMethod>
    Public Sub BilanInvalide_EstRefuseAvantProjection()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() DocumentBatchTestReviewQueue.FromSummary(Nothing))
        Dim broken = Fixture().Replace("""review"":1", """review"":2")
        Assert.ThrowsExactly(Of FormatException)(Sub() DocumentBatchTestReviewQueue.FromSummary(DocumentBatchTestSummary.Parse(broken)))
    End Sub
End Class
