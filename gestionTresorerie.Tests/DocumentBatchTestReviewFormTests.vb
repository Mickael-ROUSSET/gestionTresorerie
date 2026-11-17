Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Linq
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
                           Assert.AreEqual("TEST - Revue documentaire - " & System.IO.Path.GetFileName(path), form.Text)
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
                           Dim originalTitle = form.Text
                           File.WriteAllText(path, "{", New UTF8Encoding(False))
                           Assert.ThrowsExactly(Of FormatException)(Sub() form.LoadTestSummary(path))
                           Assert.AreEqual("initial.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
                           Assert.AreEqual(originalStatus, form.StatusText)
                           Assert.AreEqual(originalTitle, form.Text)
                           Assert.IsTrue(form.ReloadSummaryButton.Enabled)
                           File.WriteAllText(path, template.Replace("{0}", "corrige.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                           form.LoadTestSummary(path)
                           Assert.AreEqual("corrige.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(originalTitle, form.Text)
                       End Using
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
               End Sub)
    End Sub

    <TestMethod>
    Public Sub BilanSupprimeApresChargementConserveLaRevueEtPermetLaReprise()
        RunSta(Sub()
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-missing-" & Guid.NewGuid().ToString("N") & ".json")
                   Dim template = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""{0}"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""verification manuelle""}]}"
                   Try
                       File.WriteAllText(path, template.Replace("{0}", "avant.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                       Using form As New DocumentBatchTestReviewForm()
                           form.LoadTestSummary(path)
                           Dim originalTitle = form.Text
                           Dim originalStatus = form.StatusText
                           File.Delete(path)
                           Assert.ThrowsExactly(Of FileNotFoundException)(Sub() form.LoadTestSummary(path))
                           Assert.AreEqual("avant.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
                           Assert.AreEqual(originalTitle, form.Text)
                           Assert.AreEqual(originalStatus, form.StatusText)
                           Assert.IsTrue(form.ReloadSummaryButton.Enabled)
                           File.WriteAllText(path, template.Replace("{0}", "retabli.pdf", StringComparison.Ordinal), New UTF8Encoding(False))
                           form.LoadTestSummary(path)
                           Assert.AreEqual("retabli.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(originalTitle, form.Text)
                       End Using
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
               End Sub)
    End Sub


    <TestMethod>
    Public Sub BilanAvecNomsDupliquesNeRemplacePasUneRevueValidee()
        RunSta(Sub()
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-duplicate-" & Guid.NewGuid().ToString("N") & ".json")
                   Dim document = "{""name"":""exemple.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""verification manuelle""}"
                   Dim prefix = "{""schema_version"":1,""total"":"
                   Dim suffix = ",""valid"":0,""review"":"
                   Dim ending = ",""skipped"":0,""requires_review"":true,""documents"":["
                   Try
                       File.WriteAllText(path, prefix & "1" & suffix & "1" & ending & document & "]}", New UTF8Encoding(False))
                       Using form As New DocumentBatchTestReviewForm()
                           form.LoadTestSummary(path)
                           Dim previousStatus = form.StatusText
                           Dim previousTitle = form.Text
                           File.WriteAllText(path, prefix & "2" & suffix & "2" & ending & document & "," & document & "]}", New UTF8Encoding(False))
                           Assert.ThrowsExactly(Of FormatException)(Sub() form.LoadTestSummary(path))
                           Assert.AreEqual(1, form.ReviewGrid.Rows.Count)
                           Assert.AreEqual("exemple.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual(previousStatus, form.StatusText)
                           Assert.AreEqual(previousTitle, form.Text)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
                           File.WriteAllText(path, prefix & "1" & suffix & "1" & ending & document.Replace("exemple.pdf", "corrige.pdf", StringComparison.Ordinal) & "]}", New UTF8Encoding(False))
                           form.LoadTestSummary(path)
                           Assert.AreEqual("corrige.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                       End Using
                   Finally
                       If File.Exists(path) Then File.Delete(path)
                   End Try
               End Sub)
    End Sub

    <TestMethod>
    Public Sub RevueDocumentaireAccessibleSansInitialisationSqlTest()
        RunSta(Sub()
                   Dim formType = GetType(DocumentBatchTestReviewForm).Assembly.GetType("gestionTresorerie.FrmSqlTestDocuments", throwOnError:=True)
                   Using form = DirectCast(Activator.CreateInstance(formType, System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.NonPublic, Nothing, New Object() {Nothing}, Nothing), System.Windows.Forms.Form)
                       Dim buttonProperty = formType.GetProperty("ReviewSummaryButton", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.NonPublic)
                       Assert.IsNotNull(buttonProperty)
                       Dim button = DirectCast(buttonProperty.GetValue(form), System.Windows.Forms.Button)
                       Assert.AreEqual("ReviewTestSummaryButton", button.Name)
                       Assert.IsTrue(button.Enabled)
                       Assert.IsTrue(button.Text.Contains("TEST", StringComparison.Ordinal))
                       Assert.IsTrue(form.Controls.OfType(Of FlowLayoutPanel)().
                           Any(Function(panel) panel.Controls.Contains(button)))
                   End Using
               End Sub)
    End Sub

    <TestMethod>
    Public Sub BilanMixteFictifAfficheSeulementLesDocumentsAExaminer()
        RunSta(Sub()
                   Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "review-mixed-" & Guid.NewGuid().ToString("N") & ".json")
                   Dim otherHash = New String("b"c, 64)
                   Dim json = "{""schema_version"":1,""total"":2,""valid"":1,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[" &
                       "{""name"":""facture.pdf"",""hash"":""" & HashValue & """,""status"":""valid"",""reason"":""""}," &
                       "{""name"":""cheque.pdf"",""hash"":""" & otherHash & """,""status"":""review"",""reason"":""montant illisible""}]}"
                   Try
                       File.WriteAllText(path, json, New UTF8Encoding(False))
                       Using form As New DocumentBatchTestReviewForm()
                           form.LoadTestSummary(path)
                           Assert.AreEqual(1, form.ReviewGrid.Rows.Count)
                           Assert.AreEqual("cheque.pdf", form.ReviewGrid.Rows(0).Cells("Nom").Value)
                           Assert.AreEqual("TEST - Documents a examiner : 1", form.StatusText)
                           Assert.AreEqual(path, form.LoadedSummaryPath)
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
        worker.IsBackground = True
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        If Not worker.Join(TimeSpan.FromSeconds(30)) Then
            Assert.Fail("Le test WinForms STA a depasse 30 secondes (blocage possible d'une boite de dialogue).")
        End If
        If failure IsNot Nothing Then Throw New AssertFailedException("Erreur du test STA", failure)
    End Sub
End Class
