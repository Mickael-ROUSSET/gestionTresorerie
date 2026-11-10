Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersTestDescendingSelectionTests
    <TestMethod>
    Public Sub TriDecroissantConserveIdentiteEtCoordonneesDeDeuxHomonymes()
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim grid = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim email = DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox)
                        Dim ville = DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox)
                        Dim data As New DataTable()
                        data.Columns.Add("Id", GetType(Integer))
                        data.Columns.Add("nom", GetType(String))
                        data.Columns.Add("Email", GetType(String))
                        data.Columns.Add("Ville", GetType(String))
                        data.Rows.Add(101, "Association fictive", "un@example.invalid", "Allègre")
                        data.Rows.Add(303, "Association fictive", "trois@example.invalid", "Céaux-d'Allègre")
                        data.Rows.Add(202, "Autre association", "deux@example.invalid", "Monlet")
                        Dim view = data.DefaultView
                        view.Sort = "Id DESC"
                        grid.DataSource = view

                        Assert.AreEqual(303, Convert.ToInt32(grid.Rows(0).Cells("Id").Value))
                        Assert.AreEqual(101, Convert.ToInt32(grid.Rows(2).Cells("Id").Value))
                        For Each expectedId In New Integer() {303, 101, 202, 303}
                            Dim selectedRow As DataGridViewRow = Nothing
                            For Each row As DataGridViewRow In grid.Rows
                                If Not row.IsNewRow AndAlso Convert.ToInt32(row.Cells("Id").Value) = expectedId Then
                                    selectedRow = row
                                    Exit For
                                End If
                            Next
                            Assert.IsNotNull(selectedRow)
                            grid.ClearSelection()
                            selectedRow.Selected = True
                            Select Case expectedId
                                Case 101
                                    Assert.AreEqual("Association fictive", nom.Text)
                                    Assert.AreEqual("un@example.invalid", email.Text)
                                    Assert.AreEqual("Allègre", ville.Text)
                                Case 303
                                    Assert.AreEqual("Association fictive", nom.Text)
                                    Assert.AreEqual("trois@example.invalid", email.Text)
                                    Assert.AreEqual("Céaux-d'Allègre", ville.Text)
                                Case 202
                                    Assert.AreEqual("Autre association", nom.Text)
                                    Assert.AreEqual("deux@example.invalid", email.Text)
                                    Assert.AreEqual("Monlet", ville.Text)
                            End Select
                        Next
                        Assert.AreEqual(3, data.Rows.Count)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Le test WinForms fictif ne doit pas attendre SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
