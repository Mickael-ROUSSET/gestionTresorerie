Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RevueNavigationLotTests
    <TestMethod>
    Public Sub PieceSuivante_ParcourtLeLotEtRevientAuPremierSansAnalyser()
        Dim dossier = Path.Combine(Path.GetTempPath(), "gestionTresorerie-navigation-" & Guid.NewGuid().ToString("N"))
        Dim erreur As Exception = Nothing
        Try
            Directory.CreateDirectory(dossier)
            Dim premier = Path.Combine(dossier, "01.pdf")
            Dim second = Path.Combine(dossier, "02.pdf")
            File.WriteAllText(premier, "Document fictif 1")
            File.WriteAllText(second, "Document fictif 2")
            Dim thread As New Thread(
                Sub()
                    Try
                        Using fenetre As New FrmDocumentFilePreview(New String() {premier, second})
                            Dim bouton = fenetre.Controls.Find("PieceSuivanteTest", True).OfType(Of Button)().Single()
                            Dim liste = fenetre.Controls.OfType(Of TableLayoutPanel)().
                                SelectMany(Function(p) p.Controls.OfType(Of GroupBox)()).
                                SelectMany(Function(g) g.Controls.OfType(Of ListBox)()).Single()
                            Assert.IsTrue(bouton.Enabled, "La navigation doit être active pour deux pièces.")
                            Assert.AreEqual(0, liste.SelectedIndex)
                            bouton.PerformClick()
                            Assert.AreEqual(1, liste.SelectedIndex, "Le clic doit sélectionner la deuxième pièce.")
                            bouton.PerformClick()
                            Assert.AreEqual(0, liste.SelectedIndex, "La navigation doit revenir à la première pièce.")
                        End Using
                    Catch ex As Exception
                        erreur = ex
                    End Try
                End Sub)
            thread.SetApartmentState(ApartmentState.STA)
            thread.Start()
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(15)), "La revue WinForms ne doit pas bloquer.")
            If erreur IsNot Nothing Then Throw erreur
            CollectionAssert.AreEquivalent(New String() {"01.pdf", "02.pdf"},
                Directory.GetFiles(dossier).Select(Function(p) Path.GetFileName(p)).ToArray(),
                "Naviguer ne doit ni déplacer ni supprimer les pièces.")
        Finally
            If Directory.Exists(dossier) Then Directory.Delete(dossier, True)
        End Try
    End Sub
End Class
