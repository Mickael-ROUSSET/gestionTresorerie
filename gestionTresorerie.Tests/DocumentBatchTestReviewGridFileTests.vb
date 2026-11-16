Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewGridFileTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    <TestMethod>
    Public Sub ChargeUnLotFictifSansModifierLeFichier()
        Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "revue-test-" & Guid.NewGuid().ToString("N") & ".json")
        Dim json = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""facture-fictive.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""type incertain""}]}"
        Try
            File.WriteAllText(path, json, New UTF8Encoding(False))
            Dim before = File.ReadAllBytes(path)
            Dim table = DocumentBatchTestReviewGrid.CreateTableFromFile(path)
            CollectionAssert.AreEqual(before, File.ReadAllBytes(path))
            Assert.AreEqual(1, table.Rows.Count)
            Assert.AreEqual("facture-fictive.pdf", table.Rows(0)("Nom"))
            Assert.AreEqual("type incertain", table.Rows(0)("Motif"))
            Assert.AreEqual(2, table.Columns.Count)
        Finally
            If File.Exists(path) Then File.Delete(path)
        End Try
    End Sub

    <TestMethod>
    Public Sub RefuseUnBilanFictifInvalide()
        Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "revue-test-" & Guid.NewGuid().ToString("N") & ".json")
        Try
            File.WriteAllText(path, "{}", New UTF8Encoding(False))
            Assert.ThrowsExactly(Of FormatException)(
                Sub() DocumentBatchTestReviewGrid.CreateTableFromFile(path))
        Finally
            If File.Exists(path) Then File.Delete(path)
        End Try
    End Sub
End Class
