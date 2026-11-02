Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DemonstrationWindowsTests
    Private NotInheritable Class PrincipaleFictive
        Inherits FrmPrincipale

        Public Sub New()
            MyBase.New(True)
        End Sub

        Public Property NombrePropositions As Integer
        Public Property NombreRejets As Integer
        Public Property NombreOuvertures As Integer

        Public Sub ChargerSansAfficher()
            MyBase.OnLoad(EventArgs.Empty)
        End Sub

        Protected Overrides Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
            NombreOuvertures += 1
            NombrePropositions = DirectCast(
                formulaire.Controls.Find("grillePropositions", True)(0), DataGridView).Rows.Count
            NombreRejets = DirectCast(
                formulaire.Controls.Find("grilleRejets", True)(0), DataGridView).Rows.Count
        End Sub
    End Class

    Private Shared Sub EnSta(action As Action)
        Dim erreur As Exception = Nothing
        Dim thread As New Thread(Sub()
                                     Try
                                         action()
                                     Catch ex As Exception
                                         erreur = ex
                                     End Try
                                 End Sub)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        thread.Join()
        If erreur IsNot Nothing Then ExceptionDispatchInfo.Capture(erreur).Throw()
    End Sub

    ' Compile uniquement dans la variante DEMO_BUILD : le constructeur False
    ' ne doit jamais permettre de revenir aux accès comptables de production.
#If DEMO_BUILD Then
    Private NotInheritable Class PrincipaleSansDemandeDeDemo
        Inherits FrmPrincipale

        Public Sub New()
            MyBase.New(False)
        End Sub

        Public Sub ChargerSansAfficher()
            MyBase.OnLoad(EventArgs.Empty)
        End Sub
    End Class

    <TestMethod>
    Public Sub DemoBuild_ForceLeModeFictif_MemeSansOptionDemo()
        EnSta(Sub()
                  Using fenetre As New PrincipaleSansDemandeDeDemo()
                      fenetre.ChargerSansAfficher()
                      StringAssert.Contains(fenetre.Text, "DÉMONSTRATION HORS LIGNE")
                      Assert.AreEqual(1, fenetre.Controls.Find("annonceDemonstration", True).Length)
                      Assert.IsFalse(fenetre.Controls.Find("dgvPrincipale", True)(0).Enabled)
                      Assert.IsFalse(fenetre.IsHandleCreated)
                  End Using
              End Sub)
    End Sub
#End If

    <TestMethod>
    Public Sub Demo_DemarreSansInitialisationComptable_EtNeutraliseLesMenusReels()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      StringAssert.Contains(fenetre.Text, "DÉMONSTRATION HORS LIGNE")
                      Assert.AreEqual(1, fenetre.Controls.Find("annonceDemonstration", True).Length)
                      Dim menus = fenetre.MainMenuStrip.Items.OfType(Of ToolStripMenuItem)().ToArray()
                      Assert.AreEqual(1, menus.Count(Function(m) m.Enabled))
                      Assert.AreEqual("AnalyseToolStripMenuItem",
                                      menus.Single(Function(m) m.Enabled).Name)
                      Dim analyse = menus.Single(Function(m) m.Enabled)
                      Dim actions = analyse.DropDownItems.OfType(Of ToolStripMenuItem)().
                          Where(Function(m) m.Enabled).Select(Function(m) m.Name).ToArray()
                      CollectionAssert.AreEquivalent(
                          New String() {"DemoDocumentsFictifsToolStripMenuItem"}, actions)
                      Assert.IsFalse(DirectCast(fenetre.Controls.Find("dgvPrincipale", True)(0),
                                                DataGridView).Enabled)
                      Assert.IsFalse(fenetre.IsHandleCreated)
                  End Using
              End Sub)
    End Sub

    <TestMethod>
    Public Sub Demo_ChargeFactureChequeEtTroisRejets_PuisReinitialiseSansFichier()
        EnSta(Sub()
                  Using fenetre As New PrincipaleFictive()
                      fenetre.ChargerSansAfficher()
                      Dim demo = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "DemoDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim revue = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Dim effacer = DirectCast(fenetre.MainMenuStrip.Items.Find(
                          "EffacerDocumentsFictifsToolStripMenuItem", True)(0), ToolStripMenuItem)
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      demo.PerformClick()
                      Assert.AreEqual(1, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                      Assert.IsTrue(revue.Enabled)
                      Assert.IsTrue(effacer.Enabled)
                      effacer.PerformClick()
                      Assert.IsFalse(revue.Enabled)
                      Assert.IsFalse(effacer.Enabled)
                      demo.PerformClick()
                      Assert.AreEqual(2, fenetre.NombreOuvertures)
                      Assert.AreEqual(2, fenetre.NombrePropositions)
                      Assert.AreEqual(3, fenetre.NombreRejets)
                      Assert.IsFalse(fenetre.IsHandleCreated)
                  End Using
              End Sub)
    End Sub
End Class
