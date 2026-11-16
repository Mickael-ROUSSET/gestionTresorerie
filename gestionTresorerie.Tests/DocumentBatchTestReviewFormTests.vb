Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports System.Threading
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewFormTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    <TestMethod>
    Public Sub AfficheUnDocumentFictifSansModifierLeBilan()
        RunSta(Sub()
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-form-" & Guid.NewGuid().ToString("N") & ".json")
                   Dim json = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""exemple.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""verification manuelle""}]}"
                   Try
                       File.WriteAllText(path, json, New UTF8Encoding(False))
                       Dim before = File.ReadAllBytes(path)
                       Using form As New DocumentBatchTestReviewForm()
                           form.LoadTestSummary(path)
                           Assert.AreEqual(1, form.ReviewGrid.Rows.Count)
                           Assert.AreEqual("exemple.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.IsTrue(form.ReviewGrid.ReadOnly)
                           Assert.IsTrue(form.Text.StartsWith("TEST", StringComparison.Ordinal))
                       End Using
                       CollectionAssert.AreEqual(before, File.ReadAllBytes(path))
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
               End Sub)
    End Sub

    <TestMethod>
    Public Sub BilanInvalideNeRemplacePasLaGrille()
        RunSta(Sub()
                   Using form As New DocumentBatchTestReviewForm()
                       Assert.ThrowsExactly(Of ArgumentNullException)(Sub() form.LoadTestSummary(Nothing))
                       Assert.IsNull(form.ReviewGrid.DataSource)
                   End Using
               End Sub)
    End Sub

    Private Shared Sub RunSta(action As Action)
        Dim failure As Exception = Nothing
        Dim worker As New Thread(Sub()
                                     Try
                                         action()
                                     Catch ex As Exception
                                         failure = ex
                                     End Try
                                 End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        worker.Join()
        If failure IsNot Nothing Then Throw New AssertFailedException("Erreur du test STA", failure)
    End Sub
End Class
