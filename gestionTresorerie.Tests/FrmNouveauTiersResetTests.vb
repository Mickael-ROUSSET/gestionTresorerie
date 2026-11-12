Option Strict On

Imports System
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmNouveauTiersResetTests
    <TestMethod>
    Public Sub ReinitialisationEffaceLesCategoriesFictivesApresCreation()
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    Using formulaire As New FrmNouveauTiers()
                        Dim categorie = DirectCast(formulaire.Controls.Find("txtCategorie", True)(0), TextBox)
                        Dim sousCategorie = DirectCast(formulaire.Controls.Find("txtSousCategorie", True)(0), TextBox)
                        categorie.Text = "Catégorie fictive A"
                        sousCategorie.Text = "Sous-catégorie fictive B"

                        Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic
                        Dim reset = GetType(FrmNouveauTiers).GetMethod("initChamps", flags)
                        Assert.IsNotNull(reset)
                        reset.Invoke(formulaire, Nothing)

                        Assert.AreEqual(String.Empty, categorie.Text)
                        Assert.AreEqual(String.Empty, sousCategorie.Text)
                        Assert.IsNull(GetType(FrmNouveauTiers).GetField("_categorieSelectionne", flags).GetValue(formulaire))
                        Assert.IsNull(GetType(FrmNouveauTiers).GetField("_sousCategorieSelectionne", flags).GetValue(formulaire))
                        Assert.IsNull(GetType(FrmNouveauTiers).GetField("_pendingCoordonnees", flags).GetValue(formulaire))
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Le test de remise à zéro ne doit pas attendre SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
