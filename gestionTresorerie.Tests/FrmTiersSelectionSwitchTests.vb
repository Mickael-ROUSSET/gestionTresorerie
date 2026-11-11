Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersSelectionSwitchTests
    <TestMethod>
    Public Sub ChangerDeTiersRechargeLesCoordonneesDuBonIdentifiantSansSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim grid = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim email = DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox)
                        Dim ville = DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim data As New DataTable()
                        data.Columns.Add("Id", GetType(Integer))
                        data.Columns.Add("nom", GetType(String))
                        data.Columns.Add("Email", GetType(String))
                        data.Columns.Add("Ville", GetType(String))
                        data.Rows.Add(123, "Association fictive A", "a@example.invalid", "Ville A")
                        data.Rows.Add(456, "Association fictive B", "b@example.invalid", "Ville B")
                        grid.DataSource = data
                        grid.ClearSelection()

                        grid.Rows(0).Selected = True
                        Assert.AreEqual("Association fictive A", nom.Text)
                        Assert.AreEqual("a@example.invalid", email.Text)
                        Assert.AreEqual("Ville A", ville.Text)
                        StringAssert.Contains(statut.Text, "123")

                        grid.ClearSelection()
                        grid.Rows(1).Selected = True
                        Assert.AreEqual("Association fictive B", nom.Text)
                        Assert.AreEqual("b@example.invalid", email.Text)
                        Assert.AreEqual("Ville B", ville.Text)
                        StringAssert.Contains(statut.Text, "456")

                        grid.ClearSelection()
                        grid.Rows(0).Selected = True
                        Assert.AreEqual("Association fictive A", nom.Text)
                        Assert.AreEqual("a@example.invalid", email.Text)
                        Assert.AreEqual("Ville A", ville.Text)
                        StringAssert.Contains(statut.Text, "123")
                        Assert.AreEqual(2, data.Rows.Count)
                        Assert.AreEqual("b@example.invalid", CStr(data.Rows(1)("Email")))
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La sélection de tiers fictifs ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
