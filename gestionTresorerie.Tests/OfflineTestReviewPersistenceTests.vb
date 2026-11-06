Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Runtime.ExceptionServices
Imports System.Windows.Forms
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class OfflineTestReviewPersistenceTests
    Private Shared Sub WithWorkspace(action As Action(Of String, String, OfflineTestWorkspace))
        Dim root = Path.Combine(Path.GetTempPath(), "GT-review-" & Guid.NewGuid().ToString("N"))
        Dim production = Path.Combine(root, "production-fictive")
        Dim testing = Path.Combine(root, "test")
        Directory.CreateDirectory(production)
        File.WriteAllText(Path.Combine(production, "temoin.txt"), "intact")
        Try
            action(production, testing, New OfflineTestWorkspace(production, testing))
            Assert.AreEqual("intact", File.ReadAllText(Path.Combine(production, "temoin.txt")))
        Finally
            Directory.Delete(root, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub CorrectionEtExamenSontRestauresPuisAnnulationEstConservee()
        WithWorkspace(
            Sub(production, testing, workspace)
                workspace.PrepareNewSession()
                Dim review = workspace.LoadReview()
                Dim original = review.ResultatInitial.Propositions.First()
                Dim correction = PropositionCorrection.Corriger(
                    original, original.DateProposee, 12.75D, original.Sens,
                    "Libellé fictif corrigé", "000001")
                review.SessionRevue.AppliquerCorrection(original.CleSource, correction)
                review.SessionRevue.ConfirmerExamen(original.CleSource, correction.Proposition)
                workspace.SaveReview(review)

                Dim reopened As New OfflineTestWorkspace(production, testing)
                Assert.AreEqual(workspace.SessionDirectory, reopened.ResumeLastSession())
                Dim restored = reopened.LoadReview()
                Dim current = restored.SessionRevue.Propositions(original.CleSource)
                Assert.AreEqual(12.75D, current.Montant)
                Assert.AreEqual("000001", current.Reference)
                Assert.AreEqual("Libellé fictif corrigé", current.Libelle)
                Assert.AreEqual(1, restored.SessionRevue.Audits.Count)
                Assert.IsTrue(restored.SessionRevue.EstExamenConfirme(original.CleSource))
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() restored.SessionRevue.MarquerApprouvee(original.CleSource))
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() restored.SessionRevue.MarquerPersistee(original.CleSource))

                Assert.IsTrue(restored.SessionRevue.AnnulerExamen(original.CleSource, current))
                reopened.SaveReview(restored)
                Dim latest = reopened.LoadReview()
                Assert.AreEqual(0, latest.SessionRevue.NombreExamensConfirmes)
                Assert.AreEqual(1, latest.SessionRevue.Audits.Count)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub ModificationDesDocumentsEtCopieEntreSessionsSontRefusees()
        WithWorkspace(
            Sub(production, testing, workspace)
                workspace.PrepareNewSession()
                Dim review = workspace.LoadReview()
                workspace.SaveReview(review)
                Dim saved = File.ReadAllText(Path.Combine(workspace.SessionDirectory, "REVUE-TEST.json"))
                File.AppendAllText(Path.Combine(workspace.SessionDirectory, "facture.json"), " ")
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() workspace.SaveReview(review))
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub()
                        Dim unused = workspace.LoadReview()
                    End Sub)

                workspace.PrepareNewSession()
                Assert.AreEqual(0, workspace.LoadReview().SessionRevue.NombreExamensConfirmes)
                File.WriteAllText(Path.Combine(workspace.SessionDirectory, "REVUE-TEST.json"), saved)
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub()
                        Dim unused = workspace.LoadReview()
                    End Sub)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub DeuxFenetresNePeuventPasEcraserLeursSauvegardes()
        WithWorkspace(
            Sub(production, testing, workspace)
                workspace.PrepareNewSession()
                Dim first = workspace.LoadReview()
                Dim other As New OfflineTestWorkspace(production, testing)
                other.ResumeLastSession()
                Dim second = other.LoadReview()
                Dim proposal = first.ResultatInitial.Propositions.First()
                first.SessionRevue.ConfirmerExamen(proposal.CleSource, proposal)
                workspace.SaveReview(first)
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() other.SaveReview(second))
                Assert.AreEqual(1, other.LoadReview().SessionRevue.NombreExamensConfirmes)
                Assert.HasCount(0, Directory.GetFiles(workspace.SessionDirectory, "*.tmp"))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub AncienneSessionSansPointeurPeutEtreRepriseEtEtatCorrompuEstRefuse()
        WithWorkspace(
            Sub(production, testing, workspace)
                workspace.PrepareNewSession()
                File.Delete(Path.Combine(testing, "SESSION-ACTIVE.txt"))
                Dim reopened As New OfflineTestWorkspace(production, testing)
                Assert.AreEqual(workspace.SessionDirectory, reopened.ResumeLastSession())
                Dim review = reopened.LoadReview()
                reopened.SaveReview(review)
                File.WriteAllText(Path.Combine(workspace.SessionDirectory, "REVUE-TEST.json"), "{invalide")
                Assert.ThrowsExactly(Of Newtonsoft.Json.JsonReaderException)(
                    Sub()
                        Dim unused = reopened.LoadReview()
                    End Sub)
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() reopened.SaveReview(review))
                File.WriteAllText(Path.Combine(testing, "SESSION-ACTIVE.txt"), "..\production-fictive")
                Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() reopened.ResumeLastSession())
            End Sub)
    End Sub

    <TestMethod>
    Public Sub VerrouEtLienDeSauvegardeSontRefuses()
        WithWorkspace(
            Sub(production, testing, workspace)
                workspace.PrepareNewSession()
                Dim review = workspace.LoadReview()
                Using held As New FileStream(Path.Combine(workspace.SessionDirectory, "REVUE-TEST.lock"),
                                             FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                    Assert.ThrowsExactly(Of IOException)(
                        Sub() workspace.SaveReview(review))
                End Using
                Dim statePath = Path.Combine(workspace.SessionDirectory, "REVUE-TEST.json")
                File.CreateSymbolicLink(statePath, Path.Combine(production, "temoin.txt"))
                Try
                    Assert.ThrowsExactly(Of InvalidOperationException)(
                        Sub() workspace.SaveReview(review))
                Finally
                    File.Delete(statePath)
                End Try
            End Sub)
    End Sub

    Private NotInheritable Class ReviewWindow
        Inherits FrmOfflineTestWorkspace

        Public Property ConfirmOnReview As Boolean
        Public Property RestoredStatus As String

        Public Sub New(workspace As OfflineTestWorkspace)
            MyBase.New(workspace)
        End Sub

        Protected Overrides Sub PresenterRevueDocumentsAnalyses(formulaire As FrmRevuePropositions)
            Dim grid = DirectCast(formulaire.Controls.Find("grillePropositions", True)(0), DataGridView)
            grid.ClearSelection()
            grid.Rows(0).Selected = True
            RestoredStatus = CStr(grid.Rows(0).Cells("Statut").Value)
            If ConfirmOnReview Then Assert.IsTrue(formulaire.ConfirmerExamenSelection())
        End Sub
    End Class

    Private Shared Sub Click(window As Form, name As String)
        DirectCast(window.Controls.Find(name, True)(0), Button).PerformClick()
    End Sub

    Private Shared Sub OpenReview(window As ReviewWindow)
        DirectCast(window.MainMenuStrip.Items.Find("RevueDocumentsAnalysesToolStripMenuItem", True)(0),
                   ToolStripMenuItem).PerformClick()
    End Sub

    <TestMethod>
    Public Sub InterfaceEnregistrePuisReprendEtReinitialiseSansAncienExamen()
        WithWorkspace(
            Sub(production, testing, workspace)
                Dim failure As Exception = Nothing
                Dim worker As New Thread(
                    Sub()
                        Try
                            Using first As New ReviewWindow(workspace)
                                first.Show()
                                Click(first, "PreparerSessionTest")
                                Click(first, "ChargerSessionTest")
                                first.ConfirmOnReview = True
                                OpenReview(first)
                                Click(first, "EnregistrerRevueTest")
                                StringAssert.Contains(first.Controls.Find("EtatSessionTest", True)(0).Text,
                                                      "enregistrée")
                                first.Close()
                            End Using
                            Using second As New ReviewWindow(New OfflineTestWorkspace(production, testing))
                                second.Show()
                                Click(second, "ReprendreSessionTest")
                                OpenReview(second)
                                Assert.AreEqual("Examen confirmé (sans approbation)", second.RestoredStatus)
                                Click(second, "PreparerSessionTest")
                                Click(second, "ChargerSessionTest")
                                OpenReview(second)
                                Assert.AreEqual("En attente de revue humaine", second.RestoredStatus)
                                second.Close()
                            End Using
                        Catch ex As Exception
                            failure = ex
                        End Try
                    End Sub)
                worker.IsBackground = True
                worker.SetApartmentState(ApartmentState.STA)
                worker.Start()
                Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(30)), "Le parcours TEST doit se terminer.")
                If failure IsNot Nothing Then ExceptionDispatchInfo.Capture(failure).Throw()
            End Sub)
    End Sub
End Class
