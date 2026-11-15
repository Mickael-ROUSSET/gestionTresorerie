Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestSummaryTests
    Private Const Hash As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    Private Shared Function Row(status As String) As String
        Return "{""name"":""scan.pdf"",""hash"":""" & Hash & """,""status"":""" & status & """,""reason"":""fictitious""}"
    End Function

    Private Shared Function Summary(total As Integer, valid As Integer, review As Integer,
                                    skipped As Integer, requiresReview As String, rows As String) As String
        Return "{""schema_version"":1,""total"":" & total & ",""valid"":" & valid &
               ",""review"":" & review & ",""skipped"":" & skipped &
               ",""requires_review"":" & requiresReview & ",""documents"":[" & rows & "]}"
    End Function

    <TestMethod>
    Public Sub LotFictifAvecExamen_EstLuSansPersistance()
        Dim result = DocumentBatchTestSummary.Parse(Summary(1, 0, 1, 0, "true", Row("review")))
        Assert.AreEqual(1, result.Total)
        Assert.IsTrue(result.RequiresReview)
        Assert.AreEqual("scan.pdf", result.Documents(0).Name)
        Assert.AreEqual("review", result.Documents(0).Status)
    End Sub

    <TestMethod>
    Public Sub RepriseFictiveDejaValidee_IndiqueSkipped()
        Dim result = DocumentBatchTestSummary.Parse(Summary(1, 0, 0, 1, "false", Row("skipped")))
        Assert.IsFalse(result.RequiresReview)
        Assert.AreEqual(1, result.Skipped)
    End Sub

    <TestMethod>
    Public Sub BilansIncoherentsOuInconnus_SontRefuses()
        Dim invalid = {
            Summary(1, 1, 0, 0, "true", Row("valid")),
            Summary(2, 1, 0, 0, "false", Row("valid")),
            Summary(1, 0, 1, 0, "false", Row("review")),
            Summary(1, 1, 0, 0, "false", Row("unknown")),
            Summary(1, 1, 0, 0, "false", Row("review")),
            "{""schema_version"":2,""total"":0,""valid"":0,""review"":0,""skipped"":0,""requires_review"":false,""documents"":[]}",
            "{broken"
        }
        For Each sample In invalid
            Assert.ThrowsExactly(Of FormatException)(Sub() DocumentBatchTestSummary.Parse(sample))
        Next
    End Sub
End Class
