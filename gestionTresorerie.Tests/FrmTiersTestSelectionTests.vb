Option Strict On

Imports System
Imports System.Data
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersTestSelectionTests
    <TestMethod>
    Public Sub ChangerDeTiersSelectionneNeMelangePasLesCoordonneesFictives()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim resultats = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim email = DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox)
                        Dim ville = DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox)
                        Dim table As New DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(101, "Association École fictive", "ecole@example.invalid", "Allègre")
                        table.Rows.Add(202, "Association Marché fictive", "marche@example.invalid", "Monlet")
                        table.Rows.Add(303, "Association École fictive", "autre-ecole@example.invalid", "Céaux-d'Allègre")
                        resultats.DataSource = table

                        resultats.ClearSelection()
                        resultats.Rows(0).Selected = True
                        Assert.AreEqual("Association École fictive", nom.Text)
                        Assert.AreEqual("ecole@example.invalid", email.Text)
                        Assert.AreEqual("Allègre", ville.Text)

                        resultats.ClearSelection()
                        resultats.Rows(1).Selected = True
                        Assert.AreEqual("Association Marché fictive", nom.Text)
                        Assert.AreEqual("marche@example.invalid", email.Text)
                        Assert.AreEqual("Monlet", ville.Text)

                        resultats.ClearSelection()
                        resultats.Rows(0).Selected = True
                        Assert.AreEqual("Association École fictive", nom.Text)
                        Assert.AreEqual("ecole@example.invalid", email.Text)
                        Assert.AreEqual("Allègre", ville.Text)
                        resultats.ClearSelection()
                        resultats.Rows(2).Selected = True
                        Assert.AreEqual("Association École fictive", nom.Text)
                        Assert.AreEqual("autre-ecole@example.invalid", email.Text, "Deux tiers homonymes ne doivent pas partager leur email.")
                        Assert.AreEqual("Céaux-d'Allègre", ville.Text, "Deux tiers homonymes ne doivent pas partager leur ville.")

                        resultats.ClearSelection()
                        resultats.Rows(0).Selected = True
                        Assert.AreEqual("ecole@example.invalid", email.Text, "Le retour au premier homonyme doit restaurer ses coordonnées.")
                        Assert.AreEqual("Allègre", ville.Text)
                        Assert.AreEqual(101, Convert.ToInt32(resultats.Rows(0).Cells("Id").Value), "Le premier homonyme doit conserver son identifiant.")
                        Assert.AreEqual(303, Convert.ToInt32(resultats.Rows(2).Cells("Id").Value), "Le second homonyme doit conserver son identifiant distinct.")
                        Assert.AreNotEqual(Convert.ToInt32(resultats.Rows(0).Cells("Id").Value), Convert.ToInt32(resultats.Rows(2).Cells("Id").Value))
                        Assert.AreEqual("Association École fictive", nom.Text)
                        Assert.AreEqual(3, table.Rows.Count, "La navigation ne doit modifier aucun tiers fictif.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La navigation TEST ne doit pas attendre SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
