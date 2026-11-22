Option Strict On
Imports System.Linq
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class EmlFictifMenuTests
    <TestMethod>
    Public Sub MenuEmlFictifDisponibleEnDemonstrationSansGmail()
        Dim erreur As Exception = Nothing
        Dim thread As New System.Threading.Thread(
            Sub()
                Try
                    Using form As New FrmPrincipale(True)
                        Dim analyse = form.Controls.OfType(Of MenuStrip)().
                            SelectMany(Function(menu) menu.Items.OfType(Of ToolStripMenuItem)()).
                            First(Function(item) item.Name = "AnalyseToolStripMenuItem")
                        Dim eml = analyse.DropDownItems.OfType(Of ToolStripMenuItem)().
                            Single(Function(item) item.Name = "ImporterEmlFictifsTestToolStripMenuItem")
                        Assert.IsNotNull(eml)
                        Assert.IsTrue(eml.Text.Contains("TEST"))
                    End Using
                Catch ex As Exception
                    erreur = ex
                End Try
            End Sub)
        thread.SetApartmentState(System.Threading.ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(10000), "Le test WinForms ne doit pas bloquer.")
        If erreur IsNot Nothing Then Throw erreur
    End Sub
End Class
