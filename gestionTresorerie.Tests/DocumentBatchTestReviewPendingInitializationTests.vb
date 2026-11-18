Option Strict On
Option Explicit On

Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewPendingInitializationTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    <TestMethod>
    Public Sub InstantaneExposeLaFileDExamenInitialisee()
        Dim json = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""scan-test.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""controle fictif""}]}"
        Dim snapshot = DocumentBatchTestReviewSnapshot.FromSummary(DocumentBatchTestSummary.Parse(json))
        Assert.IsNotNull(snapshot.Pending)
        Assert.HasCount(1, snapshot.Pending)
        Assert.AreEqual("scan-test.pdf", snapshot.Pending(0).Name)
    End Sub

    <TestMethod>
    Public Sub InstantaneSansExamenExposeUneFileVideNonNulle()
        Dim json = "{""schema_version"":1,""total"":0,""valid"":0,""review"":0,""skipped"":0,""requires_review"":false,""documents"":[]}"
        Dim snapshot = DocumentBatchTestReviewSnapshot.FromSummary(DocumentBatchTestSummary.Parse(json))
        Assert.IsNotNull(snapshot.Pending)
        Assert.HasCount(0, snapshot.Pending)
    End Sub
End Class
