Imports System
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

        Public Sub New(text As String)
            _result = New JObject From {{"texte", text}, {"statut_extraction", "Texte_a_verifier"}}.ToString()
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
End Class
