Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewSnapshotTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    Private Shared Function Report() As String
        Return "{""schema_version"":1,""total"":2,""valid"":1,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[" &
               "{""name"":""facture-test.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""type incertain""}," &
               "{""name"":""recu-test.pdf"",""hash"":""" & HashValue & """,""status"":""valid"",""reason"":""simulation""}]}"
    End Function

    <TestMethod>
    Public Sub ExposeLesCompteursEtUniquementLesDocumentsAExaminer()
        Dim snapshot = DocumentBatchTestReviewSnapshot.FromSummary(DocumentBatchTestSummary.Parse(Report()))
        Assert.AreEqual(2, snapshot.Total)
        Assert.AreEqual(1, snapshot.Valid)
        Assert.AreEqual(1, snapshot.Review)
        Assert.AreEqual(0, snapshot.Skipped)
        Assert.IsTrue(snapshot.RequiresReview)
        Assert.AreEqual(1, snapshot.Pending.Count)
        Assert.AreEqual("facture-test.pdf", snapshot.Pending(0).Name)
    End Sub

    <TestMethod>
    Public Sub RefuseUnBilanAbsent()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() DocumentBatchTestReviewSnapshot.FromSummary(Nothing))
    End Sub

    <TestMethod>
    Public Sub ChargeUnFichierFictifSansLeModifier()
        Dim filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".json")
        Try
            Dim bytes = New UTF8Encoding(False).GetBytes(Report())
            System.IO.File.WriteAllBytes(filePath, bytes)
            Dim snapshot = DocumentBatchTestReviewSnapshot.FromFile(filePath)
            Assert.AreEqual(1, snapshot.Pending.Count)
            CollectionAssert.AreEqual(bytes, System.IO.File.ReadAllBytes(filePath))
        Finally
            If System.IO.File.Exists(filePath) Then System.IO.File.Delete(filePath)
        End Try
    End Sub
End Class
