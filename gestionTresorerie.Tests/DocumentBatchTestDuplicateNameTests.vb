Option Strict On
Option Explicit On

Imports System
Imports System.Text.Json
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestDuplicateNameTests
    Private Const HashA As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
    Private Const HashB As String = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"

    Private Shared Function SummaryFor(firstName As String, secondName As String) As String
        Dim first = JsonSerializer.Serialize(firstName)
        Dim second = JsonSerializer.Serialize(secondName)
        Return "{""schema_version"":1,""total"":2,""valid"":1,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[" &
               "{""name"":" & first & ",""hash"":""" & HashA & """,""status"":""valid"",""reason"":""simule""}," &
               "{""name"":" & second & ",""hash"":""" & HashB & """,""status"":""review"",""reason"":""simule""}]}"
    End Function

    <TestMethod>
    Public Sub RefuseLeMemeNomDeScanDansUnLotFictif()
        Assert.ThrowsExactly(Of FormatException)(
            Sub() DocumentBatchTestSummary.Parse(SummaryFor("scan-test.pdf", "scan-test.pdf")))
    End Sub

    <TestMethod>
    Public Sub RefuseLaCollisionDeCasseSousWindows()
        Assert.ThrowsExactly(Of FormatException)(
            Sub() DocumentBatchTestSummary.Parse(SummaryFor("Scan-Test.PDF", "scan-test.pdf")))
    End Sub

    <TestMethod>
    Public Sub AccepteDeuxScansFictifsDistincts()
        Dim summary = DocumentBatchTestSummary.Parse(SummaryFor("scan-001.pdf", "scan-002.pdf"))
        Assert.AreEqual(2, summary.Total)
        Assert.AreEqual(1, DocumentBatchTestReviewQueue.FromSummary(summary).Count)
        Assert.AreEqual("scan-002.pdf", DocumentBatchTestReviewQueue.FromSummary(summary)(0).Name)
    End Sub
End Class
