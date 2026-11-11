Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersInvalidSelectionTests
    <TestMethod>
    Public Sub LigneSansIdentifiantNeConservePasLeTiersPrecedent()
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
                        table.Rows.Add(DBNull.Value, "Ligne fictive incomplète", "invalide@example.invalid", "Ville B")
                        grid.DataSource = table
                        grid.ClearSelection()
                        grid.Rows(0).Selected = True
                        Assert.AreEqual("Association fictive A", nom.Text)
                        grid.Rows(1).Selected = True
                        Assert.AreEqual("", nom.Text)
                        Assert.AreEqual("", email.Text)
                        Assert.AreEqual("", ville.Text)
                        StringAssert.Contains(statut.Text, "invalide")
                        nom.Text = "Modification interdite"
                        DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.AreEqual("Association fictive A", CStr(table.Rows(0)("nom")))
                        Assert.AreEqual("Ligne fictive incomplète", CStr(table.Rows(1)("nom")))
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
