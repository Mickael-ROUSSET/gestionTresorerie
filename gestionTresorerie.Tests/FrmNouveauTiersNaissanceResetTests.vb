Option Strict On

Imports System
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmNouveauTiersNaissanceResetTests
    <TestMethod>
    Public Sub ReinitialisationEffaceDateEtLieuNaissanceFictifs()
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    Using formulaire As New FrmNouveauTiers()
                        Dim dateNaissance = DirectCast(formulaire.Controls.Find("txtDateNaissance", True)(0), TextBox)
                        Dim lieuNaissance = DirectCast(formulaire.Controls.Find("txLieuNaissance", True)(0), TextBox)
                        dateNaissance.Text = "01/01/1990"
                        lieuNaissance.Text = "Ville fictive"
                        Dim reset = GetType(FrmNouveauTiers).GetMethod("initChamps", BindingFlags.Instance Or BindingFlags.NonPublic)
                        Assert.IsNotNull(reset)
                        reset.Invoke(formulaire, Nothing)
                        Assert.AreEqual(String.Empty, dateNaissance.Text)
                        Assert.AreEqual(String.Empty, lieuNaissance.Text)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Le test ne doit pas attendre SQL.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
