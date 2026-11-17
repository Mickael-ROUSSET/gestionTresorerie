Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentBatchTestReviewGridBindingTests
    Private Const HashValue As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"

    <TestMethod>
    Public Sub ChargeUnBilanFictifDansLaGrilleEnLectureSeule()
        RunSta(
            Sub()
                Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "grid-test-" & Guid.NewGuid().ToString("N") & ".json")
                Dim json = "{""schema_version"":1,""total"":1,""valid"":0,""review"":1,""skipped"":0,""requires_review"":true,""documents"":[{""name"":""scan-fictif.pdf"",""hash"":""" & HashValue & """,""status"":""review"",""reason"":""type incertain""}]}"
                Try
                    File.WriteAllText(path, json, New UTF8Encoding(False))
                    Dim original = File.ReadAllBytes(path)
                    Using grid As New DataGridView()
                        DocumentBatchTestReviewGridBinding.LoadFromFile(grid, path)
                        Assert.IsTrue(grid.ReadOnly)
                        Assert.IsFalse(grid.AllowUserToAddRows)
                        Assert.IsFalse(grid.AllowUserToDeleteRows)
                        Assert.AreEqual(1, grid.Rows.Count)
                        Assert.AreEqual("scan-fictif.pdf", grid.Rows(0).Cells("Nom").Value)
                        Assert.AreEqual("type incertain", grid.Rows(0).Cells("Motif").Value)
                    End Using
                    CollectionAssert.AreEqual(original, File.ReadAllBytes(path))
                Finally
                    If File.Exists(path) Then File.Delete(path)
                End Try
            End Sub)
    End Sub

    <TestMethod>
    Public Sub RefuseUnBilanInvalideSansRemplacerLaGrille()
        RunSta(
            Sub()
                Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "grid-test-" & Guid.NewGuid().ToString("N") & ".json")
                Try
                    File.WriteAllText(path, "{}", New UTF8Encoding(False))
                    Using grid As New DataGridView()
                        Assert.ThrowsExactly(Of FormatException)(
                            Sub() DocumentBatchTestReviewGridBinding.LoadFromFile(grid, path))
                        Assert.IsNull(grid.DataSource)
                    End Using
                Finally
                    If File.Exists(path) Then File.Delete(path)
                End Try
            End Sub)
    End Sub

    Private Shared Sub RunSta(action As Action)
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    action()
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        worker.Join()
        If failure IsNot Nothing Then System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw()
    End Sub
End Class
