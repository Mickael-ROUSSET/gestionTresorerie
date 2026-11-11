Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersNewEntryResetTests
    <TestMethod>
    Public Sub NouvelleSaisieEffaceSelectionEtPermetUnBrouillonDistinct()
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
                        Dim nouveau = DirectCast(form.Controls.Find("NouveauTiersTest", True)(0), Button)
                        Dim modifier = DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button)

                        Dim tiers As New DataTable()
                        tiers.Columns.Add("Id", GetType(Integer))
                        tiers.Columns.Add("nom", GetType(String))
                        tiers.Columns.Add("Email", GetType(String))
                        tiers.Columns.Add("Ville", GetType(String))
                        tiers.Rows.Add(101, "Tiers A fictif", "a@example.invalid", "Ville A")
                        grid.DataSource = tiers
                        grid.ClearSelection()
                        grid.Rows(0).Selected = True
                        Assert.AreEqual("Tiers A fictif", nom.Text)

                        nouveau.PerformClick()
                        Assert.IsNull(grid.DataSource)
                        Assert.AreEqual(0, grid.SelectedRows.Count)
                        Assert.AreEqual("", nom.Text)
                        Assert.AreEqual("", email.Text)
                        Assert.AreEqual("", ville.Text)
                        StringAssert.Contains(statut.Text, "Nouvelle saisie")

                        nom.Text = "Tiers B fictif"
                        email.Text = "b@example.invalid"
                        ville.Text = "Ville B"
                        modifier.PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.AreEqual("Tiers B fictif", nom.Text)
                        Assert.AreEqual("b@example.invalid", email.Text)
                        Assert.AreEqual("Ville B", ville.Text)
                        Assert.AreEqual("Tiers A fictif", CStr(tiers.Rows(0)("nom")))
                        Assert.AreEqual("a@example.invalid", CStr(tiers.Rows(0)("Email")))
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Aucun acces SQL n'est attendu.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
