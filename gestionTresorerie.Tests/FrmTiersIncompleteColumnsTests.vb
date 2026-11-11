Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersIncompleteColumnsTests
    <TestMethod>
    Public Sub ColonnesManquantesInvalidentLaSelectionSansAccesSql()
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
                        Dim modifier = DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button)
                        Dim complete As New DataTable()
                        complete.Columns.Add("Id", GetType(Integer))
                        complete.Columns.Add("nom", GetType(String))
                        complete.Columns.Add("Email", GetType(String))
                        complete.Columns.Add("Ville", GetType(String))
                        complete.Rows.Add(123, "Tiers fictif", "test@example.invalid", "Ville TEST")
                        For Each missing As String In New String() {"Id", "nom", "Email", "Ville"}
                            grid.DataSource = complete
                            grid.ClearSelection()
                            grid.Rows(0).Selected = True
                            Assert.AreEqual("Tiers fictif", nom.Text)
                            Dim incomplete = complete.Copy()
                            incomplete.Columns.Remove(missing)
                            grid.DataSource = incomplete
                            grid.ClearSelection()
                            grid.Rows(0).Selected = True
                            Assert.AreEqual("", nom.Text, missing)
                            Assert.AreEqual("", email.Text, missing)
                            Assert.AreEqual("", ville.Text, missing)
                            StringAssert.Contains(statut.Text, "invalide")
                            nom.Text = "Modification interdite"
                            modifier.PerformClick()
                            StringAssert.Contains(statut.Text, "Sélectionnez")
                            Assert.AreEqual("Tiers fictif", CStr(complete.Rows(0)("nom")))
                        Next
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Aucun accès LocalDB ne doit être nécessaire.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
