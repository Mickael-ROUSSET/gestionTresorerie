Imports System
Imports System.IO
Imports System.Threading.Tasks
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

' Tests entierement fictifs, sans Gmail, PDF reel, IA, SQL ou donnees comptables.
<TestClass>
Public Class RuleBasedInvoiceExtractionEngineTests
    Private NotInheritable Class FakeTextEngine
        Implements IDocumentExtractionEngine

        Private ReadOnly _result As String

        Public Sub New(text As String, Optional status As String = "Texte_a_verifier")
            _result = New JObject From {{"texte", text}, {"statut_extraction", status}}.ToString()
        End Sub

        Public ReadOnly Property EngineName As String Implements IDocumentExtractionEngine.EngineName
            Get
                Return "texte-factice"
            End Get
        End Property

        Public Function ExtractAsync(documentPath As String) As Task(Of String) Implements IDocumentExtractionEngine.ExtractAsync
            Return Task.FromResult(_result)
        End Function
    End Class

    <TestMethod>
    Public Async Function TotalEtDateEtiquetesSontProposesSansValidation() As Task
        Dim source = "Facture fictive" & vbLf &
                     "Total TTC : 1 234,56 EUR" & vbLf &
                     "Date facture : 15/10/2026"
        Dim engine As New RuleBasedInvoiceExtractionEngine(New FakeTextEngine(source))
        Dim result = JObject.Parse(Await engine.ExtractAsync("factice.pdf"))

        Assert.AreEqual("regles-facture-local/texte-factice", engine.EngineName)
        Assert.AreEqual(1234.56D, result("champs_proposes")("montant_ttc").Value(Of Decimal)())
        Assert.AreEqual("2026-10-15", result("champs_proposes")("date_document").ToString())
        Assert.AreEqual("Champs_a_verifier", result("statut_champs").ToString())
        Assert.AreEqual("Texte_a_verifier", result("statut_extraction").ToString())
        Assert.IsNull(result("decision"))
    End Function

    <TestMethod>
    Public Async Function TotauxContradictoiresNeSontPasArbitres() As Task
        Dim source = "Total TTC : 10,00 EUR" & vbLf & "Montant TTC : 11,00 EUR"
        Dim engine As New RuleBasedInvoiceExtractionEngine(New FakeTextEngine(source))
        Dim result = JObject.Parse(Await engine.ExtractAsync("factice.pdf"))

        Assert.AreEqual("Ambigu_a_verifier", result("statut_champs").ToString())
        Assert.IsNull(result("champs_proposes")("montant_ttc"))
    End Function

    <TestMethod>
    Public Async Function TexteSansLibellesEtDateInvalideNeProduisentPasDeChamps() As Task
        Dim source = "Prix 12,34 EUR" & vbLf & "Date facture : 31/02/2026"
        Dim engine As New RuleBasedInvoiceExtractionEngine(New FakeTextEngine(source))
        Dim result = JObject.Parse(Await engine.ExtractAsync("factice.pdf"))

        Assert.AreEqual("Aucune_donnee_fiable", result("statut_champs").ToString())
        Assert.IsFalse(CType(result("champs_proposes"), JObject).HasValues)
    End Function

    <TestMethod>
    Public Async Function ExtractionTronqueeNeProposePasDeChampsPartiels() As Task
        ' Un total peut etre contredit dans la partie manquante du PDF fictif.
        Dim source = "Total TTC : 42,50 EUR" & vbLf & "Date facture : 12/10/2026"
        Dim engine As New RuleBasedInvoiceExtractionEngine(
            New FakeTextEngine(source, "Texte_tronque_a_verifier"))
        Dim result = JObject.Parse(Await engine.ExtractAsync("factice.pdf"))

        Assert.AreEqual("Texte_tronque_a_verifier", result("statut_extraction").ToString())
        Assert.AreEqual("Extraction_incomplete_a_verifier", result("statut_champs").ToString())
        Assert.IsFalse(CType(result("champs_proposes"), JObject).HasValues)
        Assert.IsNull(result("decision"))
    End Function

    <TestMethod>
    Public Async Function ScannerOptInCreePropositionFactureSansValidationAutomatique() As Task
        Dim folder = Path.Combine(Path.GetTempPath(), "scanner-facture-fictive-" & Guid.NewGuid().ToString("N"))
        Dim imported = Path.Combine(folder, "import")
        Directory.CreateDirectory(imported)
        Try
            ' Fichier fictif : le moteur factice fournit le texte, aucun PDF reel n'est lu.
            Dim documentPath = Path.Combine(imported, "facture-fictive.pdf")
            Dim original = "Document de test sans donnees comptables"
            File.WriteAllText(documentPath, original)
            Dim queue = Path.Combine(imported, "_PropositionsAValider")
            Dim fakeText = "Total TTC : 42,50 EUR" & vbLf & "Date facture : 15/10/2026"
            Dim scanner As New ImportedAttachmentProposalScanner(New FakeTextEngine(fakeText), queue,
                                                                  enableLocalInvoiceRules:=True)

            Dim first = Await scanner.ScanAsync(imported)
            Assert.AreEqual(1, first.Created)
            Assert.AreEqual(0, first.Errors.Count)

            Dim sha = AttachmentStorage.ComputeSha256(documentPath)
            Dim proposal = JObject.Parse(File.ReadAllText(Path.Combine(queue, sha & ".json")))
            Assert.AreEqual("EnAttenteValidation", proposal("statut").ToString())
            Assert.AreEqual("regles-facture-local/texte-factice", proposal("moteur").ToString())
            Assert.AreEqual(42.5D, proposal("extraction")("champs_proposes")("montant_ttc").Value(Of Decimal)())
            Assert.AreEqual("2026-10-15", proposal("extraction")("champs_proposes")("date_document").ToString())
            Assert.AreEqual("Champs_a_verifier", proposal("extraction")("statut_champs").ToString())
            Assert.IsFalse(File.Exists(Path.Combine(queue, sha & ".review.json")))

            Dim second = Await scanner.ScanAsync(imported)
            Assert.AreEqual(0, second.Created)
            Assert.AreEqual(1, second.AlreadyPresent)
            Assert.AreEqual(0, second.Errors.Count)
            Assert.AreEqual(original, File.ReadAllText(documentPath))
        Finally
            If Directory.Exists(folder) Then Directory.Delete(folder, True)
        End Try
    End Function
End Class
