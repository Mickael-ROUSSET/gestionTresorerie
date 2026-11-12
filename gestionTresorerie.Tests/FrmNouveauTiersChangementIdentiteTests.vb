Option Strict On

Imports System
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmNouveauTiersChangementIdentiteTests
    <TestMethod>
    Public Sub ChangerTypeEffaceLesChampsCachesSansAccesSql()
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    Using formulaire As New FrmNouveauTiers()
                        Dim nom = DirectCast(formulaire.Controls.Find("txtNom", True)(0), TextBox)
                        Dim prenom = DirectCast(formulaire.Controls.Find("txtPrenom", True)(0), TextBox)
                        Dim raison = DirectCast(formulaire.Controls.Find("txtRaisonSociale", True)(0), TextBox)
                        Dim changer = GetType(FrmNouveauTiers).GetMethod("UpdateControlVisibility", BindingFlags.Instance Or BindingFlags.NonPublic)
                        Assert.IsNotNull(changer)
                        nom.Text = "Martin"
                        prenom.Text = "Alice"
                        changer.Invoke(formulaire, New Object() {True})
                        Assert.AreEqual(String.Empty, nom.Text)
                        Assert.AreEqual(String.Empty, prenom.Text)
                        raison.Text = "Entreprise fictive"
                        changer.Invoke(formulaire, New Object() {False})
                        Assert.AreEqual(String.Empty, raison.Text)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Le test ne doit pas dépendre de SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
