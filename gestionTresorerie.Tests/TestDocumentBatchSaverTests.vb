Option Strict On

Imports System
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TestDocumentBatchSaverTests
    <TestMethod>
    Public Sub LotFictifContinueApresErreurEtIgnoreLeDoublon()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "recu.png")
            File.WriteAllBytes(source, {CByte(1), CByte(2), CByte(3)})
            Dim hash = DocumentArchiveProposal.HashSource(source)
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, hash, root, Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "01/11/2026", "TEST-1", "12,50", archiveOnDrive:=True)
            Dim second = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, hash, root, Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "02/11/2026", "TEST-2", "12,50", archiveOnDrive:=True)
            Dim calls As Integer = 0
            Dim result = TestDocumentBatchSaver.Run({proposal, proposal, Nothing, second},
                Function(item)
                    calls += 1
                    If item Is second Then Throw New IOException("Erreur fictive")
                    Return CType(42, Integer?)
                End Function)
            Assert.AreEqual(1, result.Saved)
            CollectionAssert.AreEqual(New Integer() {42}, result.SavedDocumentIds.ToArray())
            Assert.AreEqual(1, result.Duplicates)
            Assert.AreEqual(2, result.Rejected)
            Assert.AreEqual(2, calls)
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AnnulationResteEnAttenteEtPeutEtreReprise()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "bulletin.png")
            File.WriteAllBytes(source, {CByte(5), CByte(6)})
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, DocumentArchiveProposal.HashSource(source), root,
                Global.gestionTresorerie.Agumaaa.TypeDocument.FormulaireAdhesion,
                "Fictif", "03/11/2026", "", "", "Alice", archiveOnDrive:=True)
            Dim first = TestDocumentBatchSaver.Run({proposal}, Function(item) CType(Nothing, Integer?))
            Assert.AreEqual(1, first.Pending)
            Assert.AreEqual(0, first.SavedDocumentIds.Count)
            Dim resumed = TestDocumentBatchSaver.Run({proposal}, Function(item) CType(7, Integer?))
            Assert.AreEqual(1, resumed.Saved)
            CollectionAssert.AreEqual(New Integer() {7}, resumed.SavedDocumentIds.ToArray())
            Assert.AreEqual(0, resumed.Pending)
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub LotFictifReessaieApresAnnulationOuErreurAvantDeMarquerUnDoublon()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "reprise.png")
            File.WriteAllBytes(source, {CByte(7), CByte(8)})
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, DocumentArchiveProposal.HashSource(source), root,
                Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "04/11/2026", "TEST-REPRISE", "9,50", archiveOnDrive:=True)
            Dim calls As Integer = 0
            Dim afterCancellation = TestDocumentBatchSaver.Run({proposal, proposal, proposal},
                Function(item)
                    calls += 1
                    If calls = 1 Then Return CType(Nothing, Integer?)
                    Return CType(81, Integer?)
                End Function)
            Assert.AreEqual(2, calls)
            Assert.AreEqual(1, afterCancellation.Pending)
            Assert.AreEqual(1, afterCancellation.Saved)
            CollectionAssert.AreEqual(New Integer() {81}, afterCancellation.SavedDocumentIds.ToArray())
            Assert.AreEqual(1, afterCancellation.Duplicates)

            calls = 0
            Dim afterFailure = TestDocumentBatchSaver.Run({proposal, proposal, proposal},
                Function(item)
                    calls += 1
                    If calls = 1 Then Throw New IOException("Echec fictif")
                    Return CType(82, Integer?)
                End Function)
            Assert.AreEqual(2, calls)
            Assert.AreEqual(1, afterFailure.Rejected)
            Assert.AreEqual(1, afterFailure.Saved)
            CollectionAssert.AreEqual(New Integer() {82}, afterFailure.SavedDocumentIds.ToArray())
            Assert.AreEqual(1, afterFailure.Duplicates)
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub
    <TestMethod>
    Public Sub ErreurSqlFictiveNInterromptPasLeLotEtPermetLaReprise()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "piece-fictive.png")
            File.WriteAllBytes(source, {CByte(1), CByte(9)})
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, DocumentArchiveProposal.HashSource(source), root,
                Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "05/11/2026", "TEST-SQL", "8,20", archiveOnDrive:=True)
            Dim calls As Integer = 0
            Dim result = TestDocumentBatchSaver.Run({proposal, proposal, proposal},
                Function(item)
                    calls += 1
                    If calls = 1 Then Throw New FakeDbException()
                    Return CType(91, Integer?)
                End Function)
            Assert.AreEqual(2, calls)
            Assert.AreEqual(1, result.Rejected)
            Assert.AreEqual(1, result.Saved)
            Assert.AreEqual(1, result.Duplicates)
            CollectionAssert.AreEqual(New Integer() {91}, result.SavedDocumentIds.ToArray())
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AnnulationDuLotArreteAvantLeDocumentSuivant()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "lot-annule.png")
            File.WriteAllBytes(source, {CByte(2), CByte(4)})
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, DocumentArchiveProposal.HashSource(source), root,
                Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "06/11/2026", "TEST-STOP", "3,00", archiveOnDrive:=True)
            Using cts As New System.Threading.CancellationTokenSource()
                Dim calls As Integer = 0
                ExpectCancellation(
                    Sub()
                        TestDocumentBatchSaver.Run({proposal, proposal},
                            Function(item)
                                calls += 1
                                cts.Cancel()
                                Return CType(55, Integer?)
                            End Function, cts.Token)
                    End Sub)
                Assert.AreEqual(1, calls, "Aucun second enregistrement après annulation")
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AnnulationSqlTestAvantInitialisationNeTouchePasLaBase()
        Using cts As New System.Threading.CancellationTokenSource()
            cts.Cancel()
            Dim approvals As Integer = 0
            ExpectCancellation(
                Sub()
                    SqlTestDocumentBatchRunner.Run(
                        Array.Empty(Of DocumentArchiveProposal)(),
                        Function(message)
                            approvals += 1
                            Return CType(True, Boolean?)
                        End Function,
                        cancellationToken:=cts.Token)
                End Sub)
            Assert.AreEqual(0, approvals)
        End Using
    End Sub

    <TestMethod>
    Public Sub NotificationSeulementPourLesEnregistrementsConfirmes()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "notification-fictive.png")
            File.WriteAllBytes(source, {CByte(2), CByte(8)})
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, DocumentArchiveProposal.HashSource(source), root,
                Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "07/11/2026", "TEST-NOTIF", "2,00", archiveOnDrive:=True)
            Dim ids As New List(Of Integer)()
            Dim calls As Integer = 0
            Dim result = TestDocumentBatchSaver.Run({proposal, proposal, proposal},
                Function(item)
                    calls += 1
                    If calls = 1 Then Return CType(Nothing, Integer?)
                    Return CType(73, Integer?)
                End Function,
                onDocumentSaved:=Sub(id) ids.Add(id))
            Assert.AreEqual(2, calls)
            Assert.AreEqual(1, result.Pending)
            Assert.AreEqual(1, result.Saved)
            Assert.AreEqual(1, result.Duplicates)
            CollectionAssert.AreEqual(New Integer() {73}, ids.ToArray())
            CollectionAssert.AreEqual(result.SavedDocumentIds.ToArray(), ids.ToArray())
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ErreurDeNotificationNeDeclenchePasUneSecondeEcriture()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "notification-erreur.png")
            File.WriteAllBytes(source, {CByte(3), CByte(8)})
            Dim proposal = DocumentArchiveProposal.PrepareAdministrativeTest(
                source, DocumentArchiveProposal.HashSource(source), root,
                Global.gestionTresorerie.Agumaaa.TypeDocument.Recu,
                "Personne fictive", "08/11/2026", "TEST-NOTIF-ERR", "4,00", archiveOnDrive:=True)
            Dim calls As Integer = 0
            Dim result = TestDocumentBatchSaver.Run({proposal, proposal},
                Function(item)
                    calls += 1
                    Return CType(104, Integer?)
                End Function,
                onDocumentSaved:=Sub(id) Throw New IOException("Affichage fictif indisponible"))
            Assert.AreEqual(1, calls, "Le callback ne doit pas provoquer une seconde sauvegarde")
            Assert.AreEqual(1, result.Saved)
            Assert.AreEqual(1, result.Duplicates)
            Assert.AreEqual(0, result.Rejected)
            Assert.AreEqual(1, result.NotificationFailures)
            CollectionAssert.AreEqual(New Integer() {104}, result.SavedDocumentIds.ToArray())
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    Private NotInheritable Class FakeDbException
        Inherits System.Data.Common.DbException
    End Class

    Private Shared Sub ExpectCancellation(action As Action)
        Try
            action()
        Catch ex As OperationCanceledException
            Return
        End Try
        Assert.Fail("Une OperationCanceledException était attendue.")
    End Sub
End Class
