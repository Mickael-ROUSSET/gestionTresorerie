Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class LocalDocumentAnalysisTests
    <TestMethod>
    Public Sub IdentifiesChequeAndPreservesLeadingZeros()
        Dim result = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "Chèque n° 0012345", "Bénéficiaire : AGUMAAA", "Date d’émission : 10/11/2026", "Montant : 42,50 €"}))
        Assert.AreEqual("Cheque", result("type_document").ToString())
        Dim fields = result("donnees_extraites")
        Assert.AreEqual("0012345", fields("identite")("emetteur_numero_cheque").ToString())
        Assert.AreEqual("AGUMAAA", fields("identite")("beneficiaire_nom").ToString())
        Assert.AreEqual("2026-11-10", fields("transaction")("date_emission").ToString())
        Assert.AreEqual(42.5D, CType(fields("transaction")("montant_numerique"), Decimal))
    End Sub

    <TestMethod>
    Public Sub LeavesUnknownAndConflictingDocumentsUnclassified()
        Assert.IsNull(Global.gestionTresorerie.LocalDocumentAnalysis.Classify("Bulletin d’adhésion" & vbLf & "Paiement par chèque"))
        Assert.IsNull(Global.gestionTresorerie.LocalDocumentAnalysis.Classify("Facture" & vbLf & "Chèque n° 0012345"))
    End Sub

    <TestMethod>
    Public Sub RecognizesInvoiceAndReusesExistingFields()
        Dim result = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "Facture", "Fournisseur : Société Exemple", "Numéro de facture : F-42", "Date de facture : 10/11/2026", "Total TTC : 50,25 EUR"}))
        Assert.AreEqual("Facture", result("type_document").ToString())
        Assert.AreEqual("F-42", result("donnees_extraites")("transaction")("numero_facture").ToString())
    End Sub
End Class
