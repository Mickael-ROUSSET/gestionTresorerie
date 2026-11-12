Option Strict On

Imports System
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmNouveauTiersEtatCoordonneesTests
    <TestMethod>
    Public Sub InitialisationRetablitLesBoutonsEtEffaceLeVerrouFictif()
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    Using formulaire As New FrmNouveauTiers()
                        Dim creer = DirectCast(formulaire.Controls.Find("btnCreerTiers", True)(0), Button)
                        Dim coordonnees = DirectCast(formulaire.Controls.Find("btnCoordonnees", True)(0), Button)
                        Dim verrou = GetType(FrmNouveauTiers).GetField("_tiersCreeAvecCoordonneesEnEchec", BindingFlags.Instance Or BindingFlags.NonPublic)
                        Dim reset = GetType(FrmNouveauTiers).GetMethod("initChamps", BindingFlags.Instance Or BindingFlags.NonPublic)
                        Assert.IsNotNull(verrou)
                        Assert.IsNotNull(reset)
                        verrou.SetValue(formulaire, CType(12345, Integer?))
                        creer.Enabled = False
                        coordonnees.Enabled = False
                        reset.Invoke(formulaire, Nothing)
                        Assert.IsTrue(creer.Enabled)
                        Assert.IsTrue(coordonnees.Enabled)
                        Assert.IsFalse(DirectCast(verrou.GetValue(formulaire), Integer?).HasValue)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "Aucune connexion SQL ne doit être nécessaire.")
        If failure IsNot Nothing Then Throw failure
    End Sub
End Class
