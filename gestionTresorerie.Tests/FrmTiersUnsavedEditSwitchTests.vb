Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersUnsavedEditSwitchTests
    <TestMethod>
    Public Sub ChangerDeTiersNeTransferePasLesModificationsNonEnregistrees()
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
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim table As New DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(101, "Tiers A fictif", "a@example.invalid", "Ville A")
                        table.Rows.Add(202, "Tiers B fictif", "b@example.invalid", "Ville B")
                        grid.DataSource = table
                        grid.ClearSelection()
                        grid.Rows(0).Selected = True
                        nom.Text = "Modification non enregistree A"
                        email.Text = "brouillon@example.invalid"
                        ville.Text = "Ville brouillon"

                        grid.ClearSelection()
                        grid.Rows(1).Selected = True
                        Assert.AreEqual("Tiers B fictif", nom.Text)
                        Assert.AreEqual("b@example.invalid", email.Text)
                        Assert.AreEqual("Ville B", ville.Text)
                        StringAssert.Contains(statut.Text, "202")

                        grid.ClearSelection()
                        grid.Rows(0).Selected = True
                        Assert.AreEqual("Tiers A fictif", nom.Text)
                        Assert.AreEqual("a@example.invalid", email.Text)
                        Assert.AreEqual("Ville A", ville.Text)
                        StringAssert.Contains(statut.Text, "101")
                        Assert.AreEqual("Tiers A fictif", CStr(table.Rows(0)("nom")))
                        Assert.AreEqual("a@example.invalid", CStr(table.Rows(0)("Email")))
                        Assert.AreEqual("Ville A", CStr(table.Rows(0)("Ville")))
                        Assert.AreEqual("b@example.invalid", CStr(table.Rows(1)("Email")))
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Le test ne doit pas contacter LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
