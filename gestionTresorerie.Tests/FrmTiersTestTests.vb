Option Strict On

Imports System
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmTiersTestTests
    <TestMethod>
    Public Sub EcranTiersTestExposeLesActionsSansConnexionSqlAuDemarrage()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        Assert.IsNotNull(form.Controls.Find("RechercheTiersTest", True)(0))
                        Assert.IsNotNull(form.Controls.Find("ResultatsTiersTest", True)(0))
                        Assert.IsNotNull(form.Controls.Find("CreerTiersTest", True)(0))
                        Assert.IsNotNull(form.Controls.Find("ModifierTiersTest", True)(0))
                        Assert.IsTrue(form.Text.Contains("TEST SQL"))
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "L'interface TEST ne doit pas bloquer au démarrage.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub NouvelleSaisieEffaceLesChampsSansConnexionSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim email = DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox)
                        Dim ville = DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox)
                        nom.Text = "Tiers fictif"
                        email.Text = "test@example.invalid"
                        ville.Text = "Ville fictive"
                        Assert.AreEqual(150, email.MaxLength, "La saisie UI doit respecter la limite SQL.")
                        email.Text = "test@example.invalid"
                        DirectCast(form.Controls.Find("NouveauTiersTest", True)(0), Button).PerformClick()
                        Assert.AreEqual("", nom.Text)
                        Assert.AreEqual("", email.Text)
                        Assert.AreEqual("", ville.Text)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)))
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub CreationAvecNomVideEstRefuseeSansConnexionSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        nom.Text = "   "
                        DirectCast(form.Controls.Find("CreerTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "nom fictif")
                        Assert.IsFalse(statut.Text.Contains("connexion"), "La validation doit intervenir avant toute connexion SQL.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La validation locale ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub ModificationSansSelectionEstRefuseeSansConnexionSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        nom.Text = "Association fictive"
                        DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.IsFalse(statut.Text.Contains("SQL"), "Aucune tentative SQL sans sélection.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "Le refus local ne doit pas bloquer.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub RechercheVideEstRefuseeSansConnexionSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim recherche = DirectCast(form.Controls.Find("RechercheTiersTest", True)(0), TextBox)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim resultats = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        recherche.Text = "   "
                        DirectCast(form.Controls.Find("ChercherTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Recherche TEST refusée")
                        Assert.IsNull(resultats.DataSource, "Une recherche invalide ne doit pas afficher des résultats anciens.")
                        Assert.IsFalse(statut.Text.Contains("impossible"), "Le refus local ne doit pas tenter LocalDB.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La recherche vide ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
