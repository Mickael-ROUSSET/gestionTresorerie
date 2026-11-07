Option Strict On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlDocumentTestStoreTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub CreeLeCatalogueTestEtRelitUneArchiveSansDoublon()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("Test SQL réel réservé au runner jetable.")
        End If

        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "facture.png")
            Using image As New Bitmap(10, 10)
                image.Save(source, Imaging.ImageFormat.Png)
            End Using
            Dim store As New SqlDocumentTestStore()
            store.Initialize()
            store.Initialize()
            Dim proposal = DocumentArchiveProposal.Prepare(source, DocumentArchiveProposal.HashSource(source),
                DemoDocumentSamples.Invoice, root, AddressOf TestSetting)

            proposal.Confirm(AddressOf store.Save)
            proposal.Confirm(AddressOf store.Save)
            Assert.IsTrue(File.Exists(proposal.Destination))
            Assert.IsTrue(File.Exists(source))
            Using documents = store.ReadDocuments()
                Dim rows = documents.Rows.Cast(Of DataRow)().
                    Where(Function(row) CStr(row("cheminDoc")) = proposal.Destination).ToArray()
                Assert.HasCount(1, rows)
                Assert.AreEqual("Facture", CStr(rows(0)("categorieDoc")))
                StringAssert.Contains(CStr(rows(0)("metaDonnees")), "DEMO-001")
            End Using
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    Private Shared Function TestSetting(name As String) As String
        Select Case name
            Case "repRacineComptabilité" : Return "Archives-TEST"
            Case "repFichiersDocumentsFactures" : Return "Factures"
            Case "repFichiersFacturesRecues" : Return "Recues"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(name))
        End Select
    End Function
End Class
