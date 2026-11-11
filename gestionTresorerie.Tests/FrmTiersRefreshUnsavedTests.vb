Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersRefreshUnsavedTests
    <TestMethod>
    Public Sub RafraichirResultatsNeReutilisePasUneSaisieNonEnregistree()
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

                        Dim avant As New DataTable()
                        avant.Columns.Add("Id", GetType(Integer))
                        avant.Columns.Add("nom", GetType(String))
                        avant.Columns.Add("Email", GetType(String))
                        avant.Columns.Add("Ville", GetType(String))
                        avant.Rows.Add(101, "Tiers A fictif", "a@example.invalid", "Ville A")
                        grid.DataSource = avant
                        grid.ClearSelection()
                        grid.Rows(0).Selected = True
                        nom.Text = "Brouillon non enregistre"
                        email.Text = "brouillon@example.invalid"
                        ville.Text = "Ville brouillon"

                        ' Simule un nouveau resultat de recherche, sans acces a SQL.
                        Dim apres As New DataTable()
                        apres.Columns.Add("Id", GetType(Integer))
                        apres.Columns.Add("nom", GetType(String))
                        apres.Columns.Add("Email", GetType(String))
                        apres.Columns.Add("Ville", GetType(String))
                        apres.Rows.Add(202, "Tiers B fictif", "b@example.invalid", "Ville B")
                        grid.DataSource = apres
                        grid.ClearSelection()
                        grid.Rows(0).Selected = True

                        Assert.AreEqual("Tiers B fictif", nom.Text)
                        Assert.AreEqual("b@example.invalid", email.Text)
                        Assert.AreEqual("Ville B", ville.Text)
                        StringAssert.Contains(statut.Text, "202")
                        Assert.AreEqual("Tiers A fictif", CStr(avant.Rows(0)("nom")))
                        Assert.AreEqual("a@example.invalid", CStr(avant.Rows(0)("Email")))
                        Assert.AreEqual("Tiers B fictif", CStr(apres.Rows(0)("nom")))
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
