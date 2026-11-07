Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class OfflineTestWorkspaceTests
    Private Shared Sub InTemporaryDirectory(action As Action(Of String, String))
        Dim container = Path.Combine(Path.GetTempPath(), "GT-workspace-" & Guid.NewGuid().ToString("N"))
        Dim production = Path.Combine(container, "production-fictive")
        Dim testing = Path.Combine(container, "test")
        Directory.CreateDirectory(production)
        File.WriteAllText(Path.Combine(production, "temoin.txt"), "à conserver")
        Try
            action(production, testing)
            Assert.AreEqual("à conserver", File.ReadAllText(Path.Combine(production, "temoin.txt")))
        Finally
            Directory.Delete(container, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub PreparationEtReinitialisationCreentDesSessionsCompletesSansEcrasement()
        InTemporaryDirectory(
            Sub(production, testing)
                Dim workspace As New OfflineTestWorkspace(production, testing)
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub()
                        Dim unused = workspace.ReadMessages()
                    End Sub)
                Dim first = workspace.PrepareNewSession()
                Assert.AreEqual(5, Directory.GetFiles(first, "*.json").Length)
                Assert.AreEqual(DemoDocumentSamples.Invoice,
                                File.ReadAllText(Path.Combine(first, "facture.json")))
                Assert.AreEqual(2, workspace.ReadMessages().Count)
                File.WriteAllText(Path.Combine(first, "facture.json"), "essai modifié")

                Dim second = workspace.PrepareNewSession()
                Assert.AreNotEqual(first, second)
                Assert.AreEqual("essai modifié", File.ReadAllText(Path.Combine(first, "facture.json")))
                Assert.AreEqual(DemoDocumentSamples.Invoice,
                                File.ReadAllText(Path.Combine(second, "facture.json")))
                Assert.AreEqual(5, Directory.GetFiles(second, "*.json").Length)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub DossierInconnuEtSessionIncompleteSontRefuses()
        InTemporaryDirectory(
            Sub(production, testing)
                Directory.CreateDirectory(testing)
                File.WriteAllText(Path.Combine(testing, "fichier-inconnu.txt"), "à conserver")
                Dim workspace As New OfflineTestWorkspace(production, testing)
                Assert.ThrowsExactly(Of FileNotFoundException)(
                    Sub() workspace.PrepareNewSession())
                Assert.AreEqual(1, Directory.GetFiles(testing).Length)
                Directory.Delete(testing, True)
                workspace.PrepareNewSession()
                File.Delete(Path.Combine(workspace.SessionDirectory, "SESSION-PRETE.txt"))
                Assert.ThrowsExactly(Of FileNotFoundException)(
                    Sub()
                        Dim unused = workspace.ReadMessages()
                    End Sub)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub FichierLieVersExterieurEstRefuse()
        InTemporaryDirectory(
            Sub(production, testing)
                Dim workspace As New OfflineTestWorkspace(production, testing)
                workspace.PrepareNewSession()
                Dim invoice = Path.Combine(workspace.SessionDirectory, "facture.json")
                File.Delete(invoice)
                File.CreateSymbolicLink(invoice, Path.Combine(production, "temoin.txt"))
                Try
                    Assert.ThrowsExactly(Of InvalidOperationException)(
                        Sub()
                            Dim unused = workspace.ReadMessages()
                        End Sub)
                Finally
                    File.Delete(invoice)
                End Try
            End Sub)
    End Sub

    <TestMethod>
    Public Sub AliasDeProductionEstRefuseAvantTouteCreation()
        InTemporaryDirectory(
            Sub(production, testing)
                Dim aliasPath = Path.Combine(Path.GetDirectoryName(production), "alias-production")
                Directory.CreateSymbolicLink(aliasPath, production)
                Try
                    Dim workspace As New OfflineTestWorkspace(aliasPath, testing)
                    Assert.ThrowsExactly(Of InvalidOperationException)(
                        Sub() workspace.PrepareNewSession())
                    Assert.IsFalse(Directory.Exists(testing))
                Finally
                    Directory.Delete(aliasPath)
                End Try
            End Sub)
    End Sub


    <TestMethod>
    Public Sub FactureFictive_RepriseIdentiqueAutoriseeMaisFicheModifieeRefusee()
        InTemporaryDirectory(
            Sub(production, testing)
                Dim workspace As New OfflineTestWorkspace(production, testing)
                workspace.PrepareNewSession()
                Dim source = workspace.CreateDocumentTestInvoice()
                Dim proposal = DocumentArchiveProposal.Prepare(
                    source, DocumentArchiveProposal.HashSource(source),
                    workspace.ReadDocumentExtraction(source),
                    workspace.GetDocumentTestDirectory(),
                    AddressOf OfflineTestWorkspace.DocumentArchiveTestSetting)
                proposal.Confirm(AddressOf workspace.SaveDocumentTestRecord)
                Dim record = proposal.Destination & ".fiche-TEST.json"
                Dim original = File.ReadAllText(record)
                Assert.IsTrue(File.Exists(proposal.Destination))
                proposal.Confirm(AddressOf workspace.SaveDocumentTestRecord)
                Assert.AreEqual(original, File.ReadAllText(record))

                File.WriteAllText(record, "{""fiche"":""modifiee""}")
                Assert.ThrowsExactly(Of IOException)(
                    Sub() proposal.Confirm(AddressOf workspace.SaveDocumentTestRecord))
                Assert.AreEqual("{""fiche"":""modifiee""}", File.ReadAllText(record))
                Assert.IsTrue(File.Exists(source))
                Assert.IsTrue(File.Exists(proposal.Destination))
            End Sub)
    End Sub

    Private NotInheritable Class WorkspaceWindow
        Inherits FrmOfflineTestWorkspace

        Public Property Proposals As Integer
        Public Property Rejections As Integer

        Public Sub New(workspace As OfflineTestWorkspace)
            MyBase.New(workspace)
        End Sub

        Protected Overrides Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
            Proposals = DirectCast(formulaire.Controls.Find("grillePropositions", True)(0),
                                   DataGridView).Rows.Count
            Rejections = DirectCast(formulaire.Controls.Find("grilleRejets", True)(0),
                                    DataGridView).Rows.Count
        End Sub
    End Class

    Private NotInheritable Class EntryWindow
        Inherits FrmPrincipale

        Public Property Openings As Integer

        Public Sub New()
            MyBase.New(True)
        End Sub

        Protected Overrides Sub OpenTestWorkspace(sender As Object, e As EventArgs)
            Openings += 1
        End Sub
    End Class

    Private Shared Sub InSta(action As Action)
        Dim failure As Exception = Nothing
        Dim worker As New Thread(
            Sub()
                Try
                    action()
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.IsBackground = True
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(30)), "Le parcours TEST doit se terminer.")
        If failure IsNot Nothing Then ExceptionDispatchInfo.Capture(failure).Throw()
    End Sub

    <TestMethod>
    Public Sub MenuPrincipalOuvreLEspaceTestEtParcoursPrepareChargePuisReinitialise()
        InTemporaryDirectory(
            Sub(production, testing)
                InSta(
                    Sub()
                        Using entry As New EntryWindow()
                            entry.Show()
                            DirectCast(entry.MainMenuStrip.Items.Find(
                                "EspaceTestLocalToolStripMenuItem", True)(0),
                                ToolStripMenuItem).PerformClick()
                            Assert.AreEqual(1, entry.Openings)
                            entry.Close()
                        End Using

                        Dim workspace As New OfflineTestWorkspace(production, testing)
                        Using window As New WorkspaceWindow(workspace)
                            window.Show()
                            StringAssert.Contains(window.Text, "TEST HORS LIGNE")
                            Dim prepare = DirectCast(window.Controls.Find("PreparerSessionTest", True)(0), Button)
                            Dim load = DirectCast(window.Controls.Find("ChargerSessionTest", True)(0), Button)
                            Dim review = DirectCast(window.MainMenuStrip.Items.Find(
                                "RevueDocumentsAnalysesToolStripMenuItem", True)(0), ToolStripMenuItem)
                            Assert.IsFalse(load.Enabled)
                            Assert.IsFalse(review.Enabled)
                            prepare.PerformClick()
                            Assert.IsTrue(load.Enabled)
                            Assert.IsFalse(review.Enabled)
                            Dim first = workspace.SessionDirectory
                            load.PerformClick()
                            Assert.IsTrue(review.Enabled)
                            review.PerformClick()
                            Assert.AreEqual(2, window.Proposals)
                            Assert.AreEqual(3, window.Rejections)
                            prepare.PerformClick()
                            Assert.AreNotEqual(first, workspace.SessionDirectory)
                            Assert.IsFalse(review.Enabled)
                            Assert.IsTrue(Directory.Exists(first))
                            load.PerformClick()
                            Assert.IsTrue(review.Enabled)
                            File.Delete(Path.Combine(workspace.SessionDirectory, "SESSION-PRETE.txt"))
                            load.PerformClick()
                            Assert.IsFalse(review.Enabled)
                            StringAssert.Contains(window.Controls.Find("EtatSessionTest", True)(0).Text,
                                                  "impossible")
                            window.Close()
                        End Using
                    End Sub)
            End Sub)
    End Sub
End Class
