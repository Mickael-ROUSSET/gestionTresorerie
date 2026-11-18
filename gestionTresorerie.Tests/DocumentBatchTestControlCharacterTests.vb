Option Strict On
Option Explicit On

Imports System
Imports System.Text.Json
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestControlCharacterTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    Private Shared Function SummaryFor(name As String) As String
        Return "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{" &
               """name"":" & JsonSerializer.Serialize(name) & ",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""simule""}]}"
    End Function

    <TestMethod>
    Public Sub RefuseLeCaractereDelDansLeNomDuScan()
        Assert.ThrowsExactly(Of FormatException)(
            Sub() DocumentBatchTestSummary.Parse(SummaryFor("scan" & ChrW(127) & ".pdf")))
    End Sub

    <TestMethod>
    Public Sub RefuseUnControleUnicodeDansLeNomDuScan()
        Assert.ThrowsExactly(Of FormatException)(
            Sub() DocumentBatchTestSummary.Parse(SummaryFor("scan" & ChrW(&H85) & ".pdf")))
    End Sub

    <TestMethod>
    Public Sub AccepteUnNomUnicodeSansCaractereDeControle()
        Dim summary = DocumentBatchTestSummary.Parse(SummaryFor("recu-école.pdf"))
        Assert.AreEqual(1, summary.Review)
        Assert.AreEqual("recu-école.pdf", DocumentBatchTestReviewQueue.FromSummary(summary)(0).Name)
    End Sub
End Class
