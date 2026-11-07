Option Strict On

Imports System
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DocumentArchiveProposalTests
    <TestMethod>
    Public Sub PropositionFacture_CopieEtReferenceSeulementApresConfirmation()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "facture.pdf")
            File.WriteAllText(source, "Pièce fictive")
            Dim proposal = Preparer(source, root)
            Assert.IsFalse(File.Exists(proposal.Destination))
            Assert.AreEqual(Path.Combine(root, "Comptabilite", "Factures", "Recues", "2026",
                                         "EXEMPLE_FICTIF", "FAC_DEMO-001_EXEMPLE_FICTIF.pdf"), proposal.Destination)

            Dim fiche As DocumentAgumaaa = Nothing
            proposal.Confirm(Sub(doc) fiche = doc)
            Assert.IsNotNull(fiche)
            Assert.AreEqual(proposal.Destination, fiche.CheminDoc)
            Assert.AreEqual(New Date(2026, 4, 1), fiche.DateDoc)
            StringAssert.Contains(fiche.metaDonnees, "montant_ttc")
            Assert.AreEqual("Pièce fictive", File.ReadAllText(proposal.Destination))
            Assert.IsTrue(File.Exists(source))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub EchecEnBase_LaCopieIntacteEstRepriseSansEcrasement()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "facture.pdf")
            File.WriteAllText(source, "Pièce fictive")
            Dim proposal = Preparer(source, root)
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() proposal.Confirm(Sub(doc)
                                           Throw New InvalidOperationException("Base indisponible")
                                       End Sub))
            Assert.AreEqual("Pièce fictive", File.ReadAllText(proposal.Destination))

            Dim tentatives As Integer = 0
            proposal.Confirm(Sub(doc) tentatives += 1)
            Assert.AreEqual(1, tentatives)
            File.WriteAllText(proposal.Destination, "Une autre pièce")
            Assert.ThrowsExactly(Of IOException)(Sub() proposal.Confirm(Sub(doc) Assert.Fail("Écriture inattendue")))
            Assert.AreEqual("Une autre pièce", File.ReadAllText(proposal.Destination))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub DocumentModifieOuInconnu_NeCreePasDArchive()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "facture.pdf")
            File.WriteAllText(source, "Version 1")
            Dim proposal = Preparer(source, root)
            File.WriteAllText(source, "Version 2")
            Assert.ThrowsExactly(Of IOException)(Sub() proposal.Confirm(Sub(doc) Assert.Fail("Écriture inattendue")))
            Assert.IsFalse(File.Exists(proposal.Destination))
            Assert.ThrowsExactly(Of ArgumentException)(
                Sub() DocumentArchiveProposal.Prepare(source, DocumentArchiveProposal.HashSource(source),
                                                      DemoDocumentSamples.Unknown, root, AddressOf Reglage))
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub Cheque_ClassementParSensEtAnnee_ConserveLesZerosInitiaux()
        Dim root = Directory.CreateTempSubdirectory().FullName
        Try
            Dim source = Path.Combine(root, "cheque.jpg")
            File.WriteAllText(source, "Chèque fictif")
            Dim json = DemoDocumentSamples.Cheque.Replace("DEMO-002", "000002").Replace(
                """identite"":{", """identite"":{""emetteur_numero_compte"":""COMPTE-TEST"",")
            Dim proposal = DocumentArchiveProposal.Prepare(source, DocumentArchiveProposal.HashSource(source),
                                                           json, root, AddressOf Reglage)
            Assert.AreEqual(Path.Combine(root, "Comptabilite", "Cheques", "2026", "Emis", "CHQ_000002.jpg"),
                            proposal.Destination)
        Finally
            Directory.Delete(root, recursive:=True)
        End Try
    End Sub

    Private Shared Function Preparer(source As String, root As String) As DocumentArchiveProposal
        Return DocumentArchiveProposal.Prepare(source, DocumentArchiveProposal.HashSource(source),
                                               DemoDocumentSamples.Invoice, root, AddressOf Reglage)
    End Function

    Private Shared Function Reglage(nom As String) As String
        Select Case nom
            Case "repRacineComptabilité" : Return "Comptabilite"
            Case "repFichiersDocumentsFactures" : Return "Factures"
            Case "repFichiersFacturesRecues" : Return "Recues"
            Case "repFichiersFacturesEmises" : Return "Emises"
            Case "repFichiersDocumentsChèques" : Return "Cheques"
            Case "numCptAgumaaa" : Return "COMPTE-TEST"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(nom))
        End Select
    End Function
End Class
