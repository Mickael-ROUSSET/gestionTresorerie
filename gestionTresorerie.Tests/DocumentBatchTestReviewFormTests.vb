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
                           Assert.AreEqual("OpenTestSummaryButton", form.OpenSummaryButton.Name)
                           Assert.IsTrue(form.OpenSummaryButton.Enabled)
                           Assert.IsTrue(form.ReloadSummaryButton.Enabled)
                           Assert.IsTrue(form.OpenSummaryButton.Text.Contains("TEST", StringComparison.Ordinal))
                           Assert.IsTrue(form.Text.StartsWith("TEST", StringComparison.Ordinal))
                           Assert.AreEqual("TEST - Documents a examiner : 1", form.StatusText)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
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
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-invalid-" & Guid.NewGuid().ToString("N") & ".json")
                   File.WriteAllText(path, "{}", New UTF8Encoding(False))
                   Try
                       Using form As New DocumentBatchTestReviewForm()
                       Assert.ThrowsExactly(Of FormatException)(Sub() form.LoadTestSummary(path))
                       Assert.IsNull(form.ReviewGrid.DataSource)
                       Assert.AreEqual("TEST - Aucun bilan charge", form.StatusText)
                       Assert.IsNull(form.LoadedSummaryPath)
                       Assert.IsTrue(form.OpenSummaryButton.Enabled)
                       Assert.IsFalse(form.ReloadSummaryButton.Enabled)
                       End Using
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
               End Sub)
    End Sub

    <TestMethod>
    Public Sub ActualisationRechargeLeBilanCorrigeSansChangerDeChemin()
        RunSta(Sub()
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-reload-" & Guid.NewGuid().ToString("N") & ".json")
                   Dim template = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""{0}"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""verification manuelle""}]}"
                   Try
                       File.WriteAllText(path, template.Replace("{0}", "avant.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                       Using form As New DocumentBatchTestReviewForm()
                           Assert.IsFalse(form.ReloadSummaryButton.Enabled)
                           form.LoadTestSummary(path)
                           Assert.IsTrue(form.ReloadSummaryButton.Enabled)
                           File.WriteAllText(path, template.Replace("{0}", "apres.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                           form.ReloadSummaryButton.PerformClick()
                           Assert.AreEqual("apres.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
                           Assert.AreEqual("TEST - Documents a examiner : 1", form.StatusText)
                       End Using
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
               End Sub)
    End Sub

    <TestMethod>
    Public Sub BilanCorrompuApresChargementConserveLaRevuePuisPermetLaReprise()
        RunSta(Sub()
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-recovery-" & Guid.NewGuid().ToString("N") & ".json")
                   Dim template = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""{0}"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""verification manuelle""}]}"
                   Try
                       File.WriteAllText(path, template.Replace("{0}", "initial.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                       Using form As New DocumentBatchTestReviewForm()
                           form.LoadTestSummary(path)
                           Dim originalStatus = form.StatusText
                           File.WriteAllText(path, "{", New UTF8Encoding(False))
                           Assert.ThrowsExactly(Of FormatException)(Sub() form.LoadTestSummary(path))
                           Assert.AreEqual("initial.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
                           Assert.AreEqual(originalStatus, form.StatusText)
                           Assert.IsTrue(form.ReloadSummaryButton.Enabled)
                           File.WriteAllText(path, template.Replace("{0}", "corrige.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                           form.LoadTestSummary(path)
                           Assert.AreEqual("corrige.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                       End Using
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
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
