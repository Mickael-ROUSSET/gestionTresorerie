Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersGridDeselectionTests
    <TestMethod>
    Public Sub DeselectionDeLaGrilleEffaceLaFicheEtInterditLaModificationSansSql()
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
                        table.Rows.Add(123, "Association fictive A", "a@example.invalid", "Ville A")
                        table.Rows.Add(456, "Association fictive B", "b@example.invalid", "Ville B")
                        grid.DataSource = table
                        grid.ClearSelection()
                        grid.Rows(1).Selected = True
                        Assert.AreEqual("Association fictive B", nom.Text)
                        StringAssert.Contains(statut.Text, "456")

                        grid.ClearSelection()
                        Assert.AreEqual(0, grid.SelectedRows.Count)
                        Assert.AreEqual("", nom.Text)
                        Assert.AreEqual("", email.Text)
                        Assert.AreEqual("", ville.Text)
                        nom.Text = "Modification fictive interdite"
                        DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.AreEqual("Association fictive B", CStr(table.Rows(1)("nom")))
                        Assert.AreEqual("b@example.invalid", CStr(table.Rows(1)("Email")))
                        Assert.AreEqual(2, table.Rows.Count)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "La désélection ne doit pas accéder à LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
