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
            Dim resumed = TestDocumentBatchSaver.Run({proposal}, Function(item) CType(7, Integer?))
            Assert.AreEqual(1, resumed.Saved)
            Assert.AreEqual(0, resumed.Pending)
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub
End Class
