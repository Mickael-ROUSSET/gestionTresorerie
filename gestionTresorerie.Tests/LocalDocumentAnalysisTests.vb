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
    <TestMethod>
    Public Sub ReadsMembershipWithSplitTitleAndAdjacentLabels()
        Dim result = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(String.Join(vbLf, {
            "SPORT VITALITÉ", "Bulletin", "d’adhésion / renouvellement d’adhésion",
            "NOM DE NAISSANCE : EXEMPLE    NOM D’USAGE : EXEMPLE-BIS",
            "PRÉNOM(S) : Camille    DATE DE NAISSANCE : 10/12/1990",
            "ADRESSE : 12 rue du Stade", "VILLE : Allègre    CODE POSTAL : 43270"}))
        Assert.AreEqual("FormulaireAdhesion", result("type_document").ToString())
        Dim identity = result("donnees_extraites")("identite")
        Assert.AreEqual("EXEMPLE", identity("nom").ToString())
        Assert.AreEqual("Camille", identity("prenom").ToString())
        Assert.AreEqual("12 rue du Stade, 43270 Allègre", identity("adresse").ToString())
        Assert.IsNull(result("donnees_extraites")("transaction")("date_document"))
    End Sub

    <TestMethod>
    Public Sub PersistsMembershipFieldsAndRoutesArchivesUnderDriveTestFolder()
        Dim temp = Directory.CreateTempSubdirectory("agumaaa-drive-test-")
        Try
            Dim source = Path.Combine(temp.FullName, "bulletin.pdf")
            File.WriteAllText(source, "bulletin fictif")
            Dim hash = Global.gestionTresorerie.DocumentArchiveProposal.HashSource(source)
            Dim proposal = Global.gestionTresorerie.DocumentArchiveProposal.PrepareAdministrativeTest(source, hash,
                temp.FullName, TypeDocument.FormulaireAdhesion, "Exemple", "10/11/2026", "", "",
                "Camille", "12 rue du Stade, 43270 Allègre", archiveOnDrive:=True)
            Assert.IsTrue(proposal.Destination.StartsWith(Path.Combine(temp.FullName, "TEST", "Adhesions", "2026") &
                Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            Assert.IsFalse(Directory.Exists(Path.Combine(temp.FullName, "TEST")))
            Dim saved As Global.gestionTresorerie.DocumentAgumaaa = Nothing
            proposal.Confirm(Sub(doc) saved = doc)
            Dim data = Newtonsoft.Json.Linq.JObject.Parse(saved.metaDonnees)
            Assert.AreEqual("Exemple", data("nom").ToString())
            Assert.AreEqual("Camille", data("prenom").ToString())
            Assert.AreEqual("12 rue du Stade, 43270 Allègre", data("adresse").ToString())
            Assert.IsTrue(File.Exists(proposal.Destination))
            Assert.IsTrue(File.Exists(source))

            Dim invoice = Global.gestionTresorerie.SqlTestDocumentEntry.Prepare(source, hash, temp.FullName,
                False, "F-42", "Exemple", "10/11/2026", "42,50", archiveOnDrive:=True)
            Assert.IsTrue(invoice.Destination.StartsWith(Path.Combine(temp.FullName, "TEST") &
                Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        Finally
            temp.Delete(recursive:=True)
        End Try
    End Sub
    <TestMethod>
    Public Sub MapsFrenchChequeFieldsToTheArchivedCheque()
        Dim ocr = String.Join(vbLf, {"Payez contre ce chèque", "N° de chèque : 0012345",
            "Tireur : Camille Exemple", "À l’ordre de AGUMAAA", "Fait à Allègre le 3/11/2026", "42,50 €"})
        Dim result = Global.gestionTresorerie.LocalDocumentAnalysis.Classify(ocr)
        Assert.AreEqual("Cheque", result("type_document").ToString())
        Dim fields = result("donnees_extraites")
        Dim identity = fields("identite")
        Dim transaction = fields("transaction")
        Dim temp = Directory.CreateTempSubdirectory("agumaaa-cheque-")
        Try
            Dim source = Path.Combine(temp.FullName, "cheque.pdf")
            File.WriteAllText(source, "chèque fictif")
            Dim hash = Global.gestionTresorerie.DocumentArchiveProposal.HashSource(source)
            Dim proposal = Global.gestionTresorerie.SqlTestDocumentEntry.Prepare(source, hash, temp.FullName, True,
                identity("emetteur_numero_cheque").ToString(), identity("beneficiaire_nom").ToString(),
                transaction("date_emission").ToString(), transaction("montant_numerique").ToString(),
                chequeEmis:=False, archiveOnDrive:=True, issuer:=identity("emetteur_du_cheque").ToString())
            Assert.AreEqual("CHQ_0012345.pdf", Path.GetFileName(proposal.Destination))
            Dim saved As Global.gestionTresorerie.Cheque = Nothing
            proposal.Confirm(Sub(document) saved = DirectCast(document, Global.gestionTresorerie.Cheque))
            Assert.AreEqual("Camille Exemple", saved.emetteur_du_cheque)
            Assert.AreEqual("AGUMAAA", saved.destinataire)
            Assert.AreEqual(42.5D, saved.montant_numerique)
            Assert.AreEqual("2026-11-03", saved.dateChq)
            Assert.IsTrue(File.Exists(proposal.Destination))
        Finally
            temp.Delete(recursive:=True)
        End Try
    End Sub
End Class
