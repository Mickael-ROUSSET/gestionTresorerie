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

    <TestMethod>
    Public Sub CreationAvecTiersSelectionneEstRefuseeSansConnexionSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim resultats = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim table As New System.Data.DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(123, "Tiers fictif sélectionné", "fictif@example.invalid", "Ville fictive")
                        resultats.DataSource = table
                        resultats.Rows(0).Selected = True
                        Assert.IsTrue(statut.Text.Contains("123"), "Le tiers fictif doit être sélectionné.")
                        DirectCast(form.Controls.Find("CreerTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Création TEST refusée")
                        StringAssert.Contains(statut.Text, "Nouvelle saisie")
                        Assert.AreEqual(1, table.Rows.Count, "Le refus ne doit pas altérer les données fictives.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "Le refus local ne doit pas attendre SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub NouvelleSaisieApresSelectionEffaceLeTiersEtSesCoordonneesSansSql()
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
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim table As New System.Data.DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(123, "Association fictive A", "a@example.invalid", "Ville A")
                        table.Rows.Add(456, "Association fictive B", "b@example.invalid", "Ville B")
                        resultats.DataSource = table
                        resultats.ClearSelection()
                        resultats.Rows(1).Selected = True
                        Assert.AreEqual("Association fictive B", nom.Text)
                        Assert.AreEqual("b@example.invalid", email.Text)
                        Assert.AreEqual("Ville B", ville.Text)
                        DirectCast(form.Controls.Find("NouveauTiersTest", True)(0), Button).PerformClick()
                        Assert.IsNull(resultats.DataSource, "Une nouvelle saisie ne doit pas conserver les résultats précédents.")
                        Assert.AreEqual("", nom.Text)
                        Assert.AreEqual("", email.Text)
                        Assert.AreEqual("", ville.Text)
                        Assert.IsFalse(statut.Text.Contains("456"), "L'identifiant fictif précédent ne doit plus apparaître.")
                        Assert.AreEqual(2, table.Rows.Count, "Aucune donnée fictive ne doit être modifiée.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La remise à zéro doit fonctionner sans LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub ChangerLaRechercheInvalideLaSelectionEtInterditUneModificationSansSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim resultats = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim recherche = DirectCast(form.Controls.Find("RechercheTiersTest", True)(0), TextBox)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim table As New System.Data.DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(123, "Tiers fictif A", "a@example.invalid", "Ville A")
                        table.Rows.Add(456, "Tiers fictif B", "b@example.invalid", "Ville B")
                        resultats.DataSource = table
                        resultats.ClearSelection()
                        resultats.Rows(0).Selected = True
                        StringAssert.Contains(statut.Text, "123")

                        recherche.Text = "Autre recherche fictive"
                        Assert.IsNull(resultats.DataSource, "Les résultats obsolètes doivent disparaître.")
                        Assert.AreEqual("", DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox).Text, "Le nom de l'ancien tiers ne doit pas rester prérempli.")
                        Assert.AreEqual("", DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox).Text, "L'email de l'ancien tiers ne doit pas rester prérempli.")
                        Assert.AreEqual("", DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox).Text, "La ville de l'ancien tiers ne doit pas rester préremplie.")
                        StringAssert.Contains(statut.Text, "relancez Rechercher")
                        DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.AreEqual(2, table.Rows.Count, "Les tiers fictifs doivent rester inchangés.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "L'invalidation de sélection ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub RechercheRefuseeApresSelectionEffaceLaFicheSansSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim resultats = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim recherche = DirectCast(form.Controls.Find("RechercheTiersTest", True)(0), TextBox)
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim email = DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox)
                        Dim ville = DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox)
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim table As New System.Data.DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(123, "Tiers fictif A", "a@example.invalid", "Ville A")
                        table.Rows.Add(456, "Tiers fictif B", "b@example.invalid", "Ville B")
                        resultats.DataSource = table
                        resultats.ClearSelection()
                        resultats.Rows(1).Selected = True
                        Assert.AreEqual("Tiers fictif B", nom.Text)
                        ' Le critère est déjà vide : cliquer sur Rechercher ne déclenche pas TextChanged.
                        DirectCast(form.Controls.Find("ChercherTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Recherche TEST refusée")
                        Assert.IsNull(resultats.DataSource)
                        Assert.AreEqual("", nom.Text)
                        Assert.AreEqual("", email.Text)
                        Assert.AreEqual("", ville.Text)
                        DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.AreEqual(2, table.Rows.Count)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La recherche refusée ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub SelectionInvalideApresTiersValideEffaceIdentiteEtCoordonneesSansSql()
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
                        Dim statut = DirectCast(form.Controls.Find("EtatTiersTest", True)(0), Label)
                        Dim table As New System.Data.DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(101, "Tiers fictif A", "a@example.invalid", "Ville A")
                        table.Rows.Add(0, "Tiers invalide B", "b@example.invalid", "Ville B")
                        resultats.DataSource = table
                        resultats.ClearSelection()
                        resultats.Rows(0).Selected = True
                        Assert.AreEqual("Tiers fictif A", nom.Text)
                        Assert.AreEqual("a@example.invalid", email.Text)
                        Assert.AreEqual("Ville A", ville.Text)
                        resultats.Rows(1).Selected = True
                        Assert.AreEqual("", nom.Text, "Un identifiant invalide ne doit pas conserver l'identite precedente.")
                        Assert.AreEqual("", email.Text, "L'email du premier tiers ne doit pas etre repris.")
                        Assert.AreEqual("", ville.Text, "La ville du premier tiers ne doit pas etre reprise.")
                        StringAssert.Contains(statut.Text, "Sélection TEST invalide")
                        DirectCast(form.Controls.Find("ModifierTiersTest", True)(0), Button).PerformClick()
                        StringAssert.Contains(statut.Text, "Sélectionnez")
                        Assert.AreEqual(2, table.Rows.Count, "Les donnees fictives ne doivent pas etre modifiees.")
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La selection invalide ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub

    <TestMethod>
    Public Sub DeuxTiersFictifsSelectionnesSuccessivementConserventLeursCoordonneesDistinctesSansSql()
        Dim failure As Exception = Nothing
        Dim thread As New Thread(
            Sub()
                Try
                    Using form As New FrmTiersTest()
                        form.Show()
                        Dim grille = DirectCast(form.Controls.Find("ResultatsTiersTest", True)(0), DataGridView)
                        Dim nom = DirectCast(form.Controls.Find("NomTiersTest", True)(0), TextBox)
                        Dim email = DirectCast(form.Controls.Find("EmailTiersTest", True)(0), TextBox)
                        Dim ville = DirectCast(form.Controls.Find("VilleTiersTest", True)(0), TextBox)
                        Dim table As New System.Data.DataTable()
                        table.Columns.Add("Id", GetType(Integer))
                        table.Columns.Add("nom", GetType(String))
                        table.Columns.Add("Email", GetType(String))
                        table.Columns.Add("Ville", GetType(String))
                        table.Rows.Add(101, "Tiers fictif A", "a@example.invalid", "Ville A")
                        table.Rows.Add(202, "Tiers fictif B", "b@example.invalid", "Ville B")
                        grille.DataSource = table
                        grille.ClearSelection()
                        For Each index As Integer In New Integer() {0, 1, 0}
                            grille.ClearSelection()
                            grille.Rows(index).Selected = True
                            Assert.AreEqual(CStr(table.Rows(index)("nom")), nom.Text)
                            Assert.AreEqual(CStr(table.Rows(index)("Email")), email.Text)
                            Assert.AreEqual(CStr(table.Rows(index)("Ville")), ville.Text)
                        Next
                        Assert.AreEqual(2, table.Rows.Count)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La sélection fictive ne doit pas attendre LocalDB.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
