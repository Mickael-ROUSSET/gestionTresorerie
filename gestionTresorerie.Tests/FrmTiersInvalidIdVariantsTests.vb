Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersInvalidIdVariantsTests
    <TestMethod>
    Public Sub IdentifiantsNonNumeriquesOuNonPositifsNePermettentPasDeModifierUnTiers()
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
                        Dim table As New DataTable()
                        table.Columns.Add("Id", GetType(String))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add("123", "Tiers fictif valide", "valide@example.invalid", "Ville A")
                        table.Rows.Add("0", "Identifiant zéro", "zero@example.invalid", "Ville B")
                        table.Rows.Add("-7", "Identifiant négatif", "negatif@example.invalid", "Ville C")
                        table.Rows.Add("abc", "Identifiant non numérique", "texte@example.invalid", "Ville D")
                        grid.DataSource = table
                        For index As Integer = 1 To 3
                            grid.ClearSelection()
                            grid.Rows(0).Selected = True
                            Assert.AreEqual("Tiers fictif valide", nom.Text)
                            grid.ClearSelection()
                            grid.Rows(index).Selected = True
                            Assert.AreEqual("", nom.Text)
                            Assert.AreEqual("", email.Text)
                            Assert.AreEqual("", ville.Text)
                            StringAssert.Contains(statut.Text, "invalide")
                            nom.Text = "Tentative de modification"
                            modifier.PerformClick()
                            StringAssert.Contains(statut.Text, "Sélectionnez")
                            Assert.AreEqual("Tiers fictif valide", CStr(table.Rows(0)("nom")))
                            Assert.AreEqual("Identifiant zéro", CStr(table.Rows(1)("nom")))
                            Assert.AreEqual("Identifiant négatif", CStr(table.Rows(2)("nom")))
                            Assert.AreEqual("Identifiant non numérique", CStr(table.Rows(3)("nom")))
                        Next
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
