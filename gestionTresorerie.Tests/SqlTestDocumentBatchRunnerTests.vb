Option Strict On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTestDocumentBatchRunnerTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub LotDeChequesFictifsClasseEtRetourneLesIdentifiantsSqlSansDoublon()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("Intégration LocalDB réservée au runner TEST isolé.")
        End If
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "cheque-fictif.png")
            Using bitmap As New Bitmap(10, 10)
                bitmap.Save(source, Imaging.ImageFormat.Png)
            End Using
            Dim proposal = SqlTestDocumentEntry.Prepare(source,
                DocumentArchiveProposal.HashSource(source), root,
                True, "000078", "Personne fictive", "03/11/2026", "13,40", chequeEmis:=False)
            Dim store As New SqlDocumentTestStore()
            Dim result = SqlTestDocumentBatchRunner.Run({proposal, proposal},
                Function(delta) ThrowUnexpectedReview(), store)
            Assert.AreEqual(1, result.Saved)
            Assert.AreEqual(1, result.Duplicates)
            Assert.AreEqual(0, result.Rejected)
            Assert.AreEqual(1, result.SavedDocumentIds.Count)
            Assert.IsTrue(result.SavedDocumentIds(0) > 0)
            Assert.IsTrue(File.Exists(proposal.Destination))
            Using documents = store.ReadDocuments()
                Dim matches = documents.Rows.Cast(Of DataRow)().
                    Where(Function(row) CStr(row("cheminDoc")) = proposal.Destination).ToArray()
                Assert.HasCount(1, matches)
                Assert.AreEqual(result.SavedDocumentIds(0), CInt(matches(0)("idDoc")))
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    Private Shared Function ThrowUnexpectedReview() As Boolean?
        Throw New AssertFailedException("Un chèque fictif ne doit pas demander de revue de tiers.")
    End Function
End Class
