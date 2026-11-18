Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestSafeFilenameTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    Private Shared Function SummaryFor(name As String) As String
        Dim encoded = System.Text.Json.JsonSerializer.Serialize(name)
        Return "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":" & encoded &
               ",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""simule""}]}"
    End Function

    <TestMethod>
    Public Sub RefuseLesNomsDangereuxAvantLaFileExamen()
        For Each name In New String() {"../scan.pdf", "..\scan.pdf", "C:\scan.pdf", "CON.pdf", "NUL.jpg", "COM1.png", "LPT9.pdf", "scan?.pdf", "scan.pdf.", "scan.pdf ", ".", "..", "scan" & ChrW(10) & ".pdf"}
            Assert.ThrowsExactly(Of FormatException)(Sub() DocumentBatchTestSummary.Parse(SummaryFor(name)), name)
        Next
    End Sub

    <TestMethod>
    Public Sub AccepteLesNomsFictifsSimples()
        For Each name In New String() {"facture-fictive.pdf", "cheque_001.png", "recu.test.jpeg"}
            Dim summary = DocumentBatchTestSummary.Parse(SummaryFor(name))
            Assert.AreEqual(name, DocumentBatchTestReviewQueue.FromSummary(summary)(0).Name)
        Next
    End Sub
End Class
