Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersTestSortedSelectionTests
    <TestMethod>
    Public Sub TriDesResultatsNeMelangePasLesCoordonneesDeTiersHomonymes()
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
                        data.Rows.Add(301, "Association fictive", "premier@example.invalid", "Allègre")
                        data.Rows.Add(102, "Association fictive", "second@example.invalid", "Monlet")
                        data.Rows.Add(203, "Autre association", "autre@example.invalid", "Vernassal")
                        Dim view = data.DefaultView
                        view.Sort = "Id ASC"
                        grid.DataSource = view

                        For Each expectedId In New Integer() {102, 301, 203, 102}
                            Dim target As DataGridViewRow = Nothing
                            For Each row As DataGridViewRow In grid.Rows
                                If Not row.IsNewRow AndAlso Convert.ToInt32(row.Cells("Id").Value) = expectedId Then
                                    target = row
                                    Exit For
                                End If
                            Next
                            Assert.IsNotNull(target, "Le tiers fictif doit rester accessible après le tri.")
                            grid.ClearSelection()
                            target.Selected = True
                            Select Case expectedId
                                Case 102
                                    Assert.AreEqual("Association fictive", nom.Text)
                                    Assert.AreEqual("second@example.invalid", email.Text)
                                    Assert.AreEqual("Monlet", ville.Text)
                                Case 301
                                    Assert.AreEqual("Association fictive", nom.Text)
                                    Assert.AreEqual("premier@example.invalid", email.Text)
                                    Assert.AreEqual("Allègre", ville.Text)
                                Case 203
                                    Assert.AreEqual("Autre association", nom.Text)
                                    Assert.AreEqual("autre@example.invalid", email.Text)
                                    Assert.AreEqual("Vernassal", ville.Text)
                            End Select
                        Next
                        Assert.AreEqual(3, data.Rows.Count, "Le tri ne doit créer ni modifier de tiers.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Le test d'interface fictive ne doit pas attendre SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
