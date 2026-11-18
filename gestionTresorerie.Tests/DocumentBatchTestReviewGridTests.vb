Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewGridTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    Private Shared Function Report() As String
        Return "{""schema_version"":1,""total"":2,""valid"":1,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[" &
               "{""name"":""facture-fictive.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""type incertain""}," &
               "{""name"":""recu-fictif.pdf"",""hash"":""" & HashValue & """,""status"":""valid"",""reason"":""simulation""}]}"
    End Function

    <TestMethod>
    Public Sub AfficheSeulementLesDocumentsAExaminerSansEmpreinte()
        Dim snapshot = DocumentBatchTestReviewSnapshot.FromSummary(DocumentBatchTestSummary.Parse(Report()))
        Dim table = DocumentBatchTestReviewGrid.CreateTable(snapshot)

        Assert.AreEqual(2, table.Columns.Count)
        Assert.AreEqual("Nom", table.Columns(0).ColumnName)
        Assert.AreEqual("Motif", table.Columns(1).ColumnName)
        Assert.AreEqual(1, table.Rows.Count)
        Assert.AreEqual("facture-fictive.pdf", table.Rows(0)("Nom"))
        Assert.AreEqual("type incertain", table.Rows(0)("Motif"))
        Assert.IsFalse(table.Columns.Contains("Hash"))
    End Sub

    <TestMethod>
    Public Sub RefuseUnInstantaneAbsent()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub() DocumentBatchTestReviewGrid.CreateTable(Nothing))
    End Sub

    <TestMethod>
    Public Sub BilanSansExamenProduitUneGrilleVide()
        Dim json = "{""schema_version"":1,""total"":0,""valid"":0,""review"":0,""skipped"":0,""requires_review"":false,""documents"":[]}"
        Dim snapshot = DocumentBatchTestReviewSnapshot.FromSummary(DocumentBatchTestSummary.Parse(json))
        Dim table = DocumentBatchTestReviewGrid.CreateTable(snapshot)
        Assert.AreEqual(0, table.Rows.Count)
        Assert.AreEqual(2, table.Columns.Count)
    End Sub
End Class
