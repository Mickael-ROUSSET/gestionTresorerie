Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports gestionTresorerie.Agumaaa

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
        Assert.IsNull(Global.gestionTresorerie.LocalDocumentAnalysis.Classify("Documents à joindre" & vbLf & "Paiement par chèque"))
        Assert.IsNull(Global.gestionTresorerie.LocalDocumentAnalysis.Classify("Facture" & vbLf & "Chèque n° 0012345"))
    End Sub

    <TestMethod>
    Public Sub RecognizesInvoiceAndReusesExistingFields()
        Dim result = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "Facture", "Fournisseur : Société Exemple", "Numéro de facture : F-42", "Date de facture : 10/11/2026", "Total TTC : 50,25 EUR"}))
        Assert.AreEqual("Facture", result("type_document").ToString())
        Assert.AreEqual("F-42", result("donnees_extraites")("transaction")("numero_facture").ToString())
    End Sub
    <TestMethod>
    Public Sub ClassifiesTheThreeOtherKnownDocumentTypes()
        Dim adhesion = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "Bulletin d’adhésion", "Nom de naissance : Exemple", "Prénom(s) : Camille", "Date d’adhésion : 10/11/2026",
            "Paiement par chèque"}))
        Assert.AreEqual("FormulaireAdhesion", adhesion("type_document").ToString())
        Assert.AreEqual("Exemple Camille", adhesion("donnees_extraites")("identite")("nom_complet").ToString())

        Dim medical = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "Questionnaire de santé pour les mineurs", "Nom et prénom : Exemple Camille", "Date de signature : 10/11/2026",
            "Réponse médicale : exemple fictif"}))
        Assert.AreEqual("QuestionnaireMedical", medical("type_document").ToString())
        Assert.IsFalse(medical.ToString().Contains("Réponse médicale", StringComparison.Ordinal))

        Dim receipt = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "Reçu fiscal n° 26-42", "Organisme : Association Exemple", "Date du reçu : 10/11/2026", "Montant reçu : 42,50 €"}))
        Assert.AreEqual("Recu", receipt("type_document").ToString())
        Assert.AreEqual("26-42", receipt("donnees_extraites")("transaction")("reference").ToString())
    End Sub

    <TestMethod>
    Public Sub ArchivesTheNewTypesSeparatelyWithoutExposingNamesInPaths()
        Dim temp = Directory.CreateTempSubdirectory("agumaaa-types-")
        Try
            Dim source = Path.Combine(temp.FullName, "document.pdf")
            File.WriteAllText(source, "document fictif")
            Dim hash = Global.gestionTresorerie.DocumentArchiveProposal.HashSource(source)
            Dim kinds = {TypeDocument.FormulaireAdhesion, TypeDocument.QuestionnaireMedical, TypeDocument.Recu}
            Dim folders = {"Adhesions", "QuestionnairesMedicaux", "Recus"}
            For i = 0 To kinds.Length - 1
                Dim proposal = Global.gestionTresorerie.DocumentArchiveProposal.PrepareAdministrativeTest(source, hash,
                    temp.FullName, kinds(i), "Exemple Camille", "10/11/2026", "26-42", "42,50")
                Assert.IsTrue(proposal.Destination.Contains(Path.Combine(folders(i), "2026"), StringComparison.Ordinal))
                Assert.IsFalse(proposal.Destination.Contains("Camille", StringComparison.Ordinal))
                Dim saved As Global.gestionTresorerie.DocumentAgumaaa = Nothing
                proposal.Confirm(Sub(doc) saved = doc)
                Assert.AreEqual(kinds(i).ToString(), saved.CategorieDoc)
                Assert.AreEqual(kinds(i) = TypeDocument.Recu, saved.metaDonnees.Contains("montant_numerique", StringComparison.Ordinal))
                Assert.IsTrue(File.Exists(proposal.Destination))
            Next
            Assert.IsTrue(File.Exists(source))
        Finally
            temp.Delete(recursive:=True)
        End Try
    End Sub
End Class
