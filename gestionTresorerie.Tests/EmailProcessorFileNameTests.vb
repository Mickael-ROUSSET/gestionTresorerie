Imports System.IO
Imports System.Reflection
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class EmailProcessorFileNameTests
    Private Shared Sub EcrireRapportFinal(folder As String, report As String, logger As Action(Of String))
        Dim methode = GetType(EmailProcessor).GetMethod("WriteFinalReport", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(methode)
        methode.Invoke(Nothing, New Object() {folder, report, logger})
    End Sub

    <TestMethod>
    Public Sub SaveFinalReport_EcritDansLeDossierConfigureSansChangerLeContenu()
        Dim folder = Path.Combine(Path.GetTempPath(), "EmailReportTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim report = "[ERREUR] Gmail fictif" & vbCrLf &
                "- Emails analysés : 3" & vbCrLf &
                "- Emails avec PJ  : 2" & vbCrLf &
                "- PJ enregistrées : 4" & vbCrLf
            Dim logged As String = Nothing
            Dim reportPath = Path.Combine(folder, "Dernier_Rapport_Agumaaa.txt")
            File.WriteAllText(reportPath, "Rapport precedent")

            EcrireRapportFinal(folder, report, Sub(value) logged = value)

            Assert.AreEqual(report, File.ReadAllText(reportPath))
            Assert.AreEqual(report, logged)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub SaveFinalReport_EchecsEcritureEtJournal_PreserventExceptionInitiale()
        Dim folder = Path.Combine(Path.GetTempPath(), "EmailReportTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim reportPath = Path.Combine(folder, "Dernier_Rapport_Agumaaa.txt")
            File.WriteAllText(reportPath, "Rapport precedent")
            Dim original As New InvalidOperationException("Echec Gmail fictif")
            Dim loggerCalled As Boolean = False
            Dim logged As String = Nothing
            Dim failingLogger As Action(Of String) =
                Sub(value)
                    loggerCalled = True
                    logged = value
                    Throw New IOException("Echec journal fictif")
                End Sub
            Using locked As New FileStream(reportPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Try
                    Try
                        Throw original
                    Finally
                        EcrireRapportFinal(folder, "[ERREUR] Gmail fictif", failingLogger)
                    End Try
                Catch ex As Exception
                    Assert.AreSame(original, ex)
                End Try
            End Using
            Assert.IsTrue(loggerCalled)
            Assert.AreEqual("[ERREUR] Gmail fictif", logged)
            Assert.AreEqual("Rapport precedent", File.ReadAllText(reportPath))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    Private Shared Sub RapporterErreur(logPath As String, report As System.Text.StringBuilder)
        Dim methode = GetType(EmailProcessor).GetMethod("AppendErrorToLogAndReport", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(methode)
        methode.Invoke(Nothing, New Object() {logPath, "Erreur initiale fictive", report})
    End Sub

    <TestMethod>
    Public Sub AppendErrorToLogAndReport_AjouteAuCheminConfigureSansEcraser()
        Dim folder = Path.Combine(Path.GetTempPath(), "EmailLogTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim logPath = Path.Combine(folder, "journal-configure.log")
            File.WriteAllText(logPath, "Entree precedente" & Environment.NewLine)
            Dim report As New System.Text.StringBuilder()

            RapporterErreur(logPath, report)

            Dim lines = File.ReadAllLines(logPath)
            Assert.AreEqual(2, lines.Length)
            Assert.AreEqual("Entree precedente", lines(0))
            StringAssert.Contains(lines(1), " | CRITIQUE: Erreur initiale fictive")
            Assert.AreEqual("[ERREUR] Erreur initiale fictive" & Environment.NewLine, report.ToString())
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AppendErrorToLogAndReport_FichierVerrouille_ConserveErreurInitialeEtSignaleEchec()
        Dim folder = Path.Combine(Path.GetTempPath(), "EmailLogTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim logPath = Path.Combine(folder, "journal.log")
            File.WriteAllText(logPath, "Entree precedente")
            Dim report As New System.Text.StringBuilder()
            Using locked As New FileStream(logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                RapporterErreur(logPath, report)
            End Using

            StringAssert.StartsWith(report.ToString(), "[ERREUR] Erreur initiale fictive" & Environment.NewLine)
            StringAssert.Contains(report.ToString(), "[ERREUR JOURNAL]")
            Assert.AreEqual("Entree precedente", File.ReadAllText(logPath))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AppendErrorToLogAndReport_CheminAbsentOuInvalide_ConserveErreurInitiale()
        For Each logPath In New String() {Nothing, String.Empty, "invalide" & ChrW(0) & ".log"}
            Dim report As New System.Text.StringBuilder()

            RapporterErreur(logPath, report)

            StringAssert.StartsWith(report.ToString(), "[ERREUR] Erreur initiale fictive" & Environment.NewLine)
            StringAssert.Contains(report.ToString(), "[ERREUR JOURNAL]")
        Next
    End Sub

    <TestMethod>
    Public Sub IsImportAllowed_JournalVideSansDate_BloqueImportInitial()
        Assert.IsFalse(EmailProcessor.IsImportAllowed(False, Nothing))
    End Sub

    <TestMethod>
    Public Sub IsImportAllowed_JournalVideAvecDateExplicite_AutoriseImportInitial()
        Dim cutoff As New DateTime(2026, 10, 1)
        Assert.IsTrue(EmailProcessor.IsImportAllowed(False, cutoff))
    End Sub

    <TestMethod>
    Public Sub IsImportAllowed_JournalInitialiseSansDate_BloqueLaRepriseSansBorne()
        Assert.IsFalse(EmailProcessor.IsImportAllowed(True, Nothing))
        Assert.IsTrue(EmailProcessor.IsImportAllowed(True, New DateTime(2026, 10, 1)))
    End Sub

    <TestMethod>
    Public Sub JournalExistantSansCutoff_ExigeUneDateExpliciteAvantReprise()
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim journalPath = Path.Combine(folder, "gmail_import_journal.txt")
            Dim cutoffPath = Path.Combine(folder, "gmail_import_cutoff.txt")
            File.WriteAllText(journalPath, GmailImportJournal.MakeKey("factice@example.invalid", 1UI, 2UI) & Environment.NewLine)
            Dim journal As New GmailImportJournal(journalPath)
            Assert.IsTrue(journal.IsProcessed(GmailImportJournal.MakeKey("factice@example.invalid", 1UI, 2UI)))

            Dim cutoff = EmailProcessor.ResolveInitialImportCutoff(cutoffPath, Nothing)
            Assert.IsFalse(EmailProcessor.IsImportAllowed(True, cutoff))
            Assert.IsFalse(File.Exists(cutoffPath))

            Dim debut As New DateTime(2026, 10, 1)
            cutoff = EmailProcessor.ResolveInitialImportCutoff(cutoffPath, debut)
            Assert.IsTrue(EmailProcessor.IsImportAllowed(True, cutoff))
            Assert.AreEqual(debut, EmailProcessor.ResolveInitialImportCutoff(cutoffPath, Nothing).Value)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ResolveInitialImportCutoff_EnregistreEtRelitLaMemeDate()
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim cutoffFile = Path.Combine(folder, "cutoff.txt")
            Assert.IsFalse(EmailProcessor.ResolveInitialImportCutoff(cutoffFile, Nothing).HasValue)
            Dim debut As New DateTime(2026, 10, 1)
            Assert.AreEqual(debut, EmailProcessor.ResolveInitialImportCutoff(cutoffFile, debut).Value)
            Assert.AreEqual(debut, EmailProcessor.ResolveInitialImportCutoff(cutoffFile, Nothing).Value)
            Dim raised As Boolean = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(cutoffFile, New DateTime(2026, 9, 1))
            Catch ex As InvalidOperationException
                raised = True
            End Try
            Assert.IsTrue(raised)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    Private Shared Function AppelerUtilitaire(nom As String, ParamArray valeurs As String()) As String
        Dim methode = GetType(EmailProcessor).GetMethod(nom, BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(methode, $"Utilitaire introuvable : {nom}")
        Return CStr(methode.Invoke(Nothing, Array.ConvertAll(valeurs, Function(valeur) CObj(valeur))))
    End Function

    <TestMethod>
    Public Sub TryResolveInitialImportCutoff_FichierMalforme_BloqueEtRapporteErreur()
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim cutoffFile = Path.Combine(folder, "cutoff.txt")
            File.WriteAllText(cutoffFile, "date invalide")
            VerifierCutoffBloque(cutoffFile)
            Assert.AreEqual("date invalide", File.ReadAllText(cutoffFile))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub TryResolveInitialImportCutoff_FichierVerrouille_BloqueEtRapporteErreur()
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim cutoffFile = Path.Combine(folder, "cutoff.txt")
            File.WriteAllText(cutoffFile, "2026-10-01")
            Using locked As New FileStream(cutoffFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                VerifierCutoffBloque(cutoffFile)
            End Using
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    Private Shared Sub VerifierCutoffBloque(cutoffFile As String)
        Dim methode = GetType(EmailProcessor).GetMethod("TryResolveInitialImportCutoff", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(methode)
        For Each requested As DateTime? In New DateTime?() {Nothing, New DateTime(2026, 10, 1)}
            Dim report As New System.Text.StringBuilder()
            Dim arguments As Object() = {cutoffFile, requested, New DateTime(2020, 1, 1), report}
            Assert.IsFalse(CBool(methode.Invoke(Nothing, arguments)))
            Assert.IsNull(arguments(2), "Aucune date de repli ne doit autoriser l'import.")
            StringAssert.Contains(report.ToString(), "[ERREUR] Import Gmail bloque")
            StringAssert.Contains(report.ToString(), "gmail_import_cutoff.txt")
        Next
    End Sub

    <TestMethod>
    Public Sub GetAttachmentFileName_UtiliseUnRepliSurPourLesNomsAbsents()
        Dim nomsAbsents As String() = {Nothing, String.Empty, " " & vbTab & vbCrLf}
        For Each dispositionName In nomsAbsents
            For Each contentTypeName In nomsAbsents
                Dim nom = AppelerUtilitaire("GetAttachmentFileName", dispositionName, contentTypeName)

                Assert.AreEqual("attachment.bin", nom)
                Assert.AreEqual(nom, AppelerUtilitaire("CleanFileName", nom))
                Assert.AreEqual(".bin", Path.GetExtension(nom))
                Assert.AreEqual(-1, nom.IndexOfAny(Path.GetInvalidFileNameChars()))
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub GetAttachmentFileName_PreserveLesNomsEtLaPrioriteMime()
        Const nomOrdinaire As String = " facture 2026.pdf"
        Assert.AreEqual(nomOrdinaire, AppelerUtilitaire("GetAttachmentFileName", nomOrdinaire, "autre.txt"))

        For Each dispositionName In New String() {Nothing, String.Empty, " " & vbTab}
            Assert.AreEqual(nomOrdinaire, AppelerUtilitaire("GetAttachmentFileName", dispositionName, nomOrdinaire))
        Next
    End Sub

    <TestMethod>
    Public Sub CleanFileName_RemplaceLesCaracteresInterdits()
        Dim nettoye = AppelerUtilitaire("CleanFileName", "facture:2026?.pdf")

        Assert.AreEqual("facture_2026_.pdf", nettoye)
    End Sub

    <TestMethod>
    Public Sub CleanFileName_NomsVidesOuPoints_UtilisentUnRepliDeterministe()
        For Each nom In New String() {Nothing, "", " ", ".", "..", "...", " . . "}
            Assert.AreEqual("attachment.bin", AppelerUtilitaire("CleanFileName", nom))
        Next
    End Sub

    <TestMethod>
    Public Sub CleanFileName_NePermetPasDeCheminWindows()
        For Each nom In New String() {"..\facture.pdf", "../../facture.pdf", "C:\temp\facture.pdf",
                                      "\\serveur\partage\facture.pdf", "/facture.pdf", "facture.pdf:flux"}
            Dim nettoye = AppelerUtilitaire("CleanFileName", nom)
            Assert.AreEqual(-1, nettoye.IndexOfAny("/\:".ToCharArray()))
            Assert.AreEqual(nettoye, Path.GetFileName(nettoye))
            Assert.AreEqual(nettoye, AppelerUtilitaire("CleanFileName", nettoye))
            Assert.AreEqual(nettoye, AppelerUtilitaire("GetAttachmentFileName", nom, "autre.pdf"))
            Assert.AreEqual(nettoye, AppelerUtilitaire("GetAttachmentFileName", Nothing, nom))
        Next
    End Sub

    <TestMethod>
    Public Sub CleanFileName_NomsDosReserves_AvecOuSansExtension()
        Dim noms As New List(Of String) From {"CON", "PRN", "AUX", "NUL"}
        For index = 1 To 9
            noms.Add("COM" & index)
            noms.Add("LPT" & index)
        Next
        For Each nom In noms
            For Each suffixe In New String() {"", ".pdf", ".tar.gz", " .txt", ". "}
                Dim entree = nom.ToLowerInvariant() & suffixe
                Dim attendu = "_" & entree.TrimEnd("."c, " "c)
                Assert.AreEqual(attendu, AppelerUtilitaire("CleanFileName", entree))
                Assert.AreEqual(attendu, AppelerUtilitaire("GetAttachmentFileName", entree, Nothing))
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub CleanFileName_PreserveUnicodeEtRetireLesTerminaisonsWindows()
        For Each nom In New String() {"facture été 日本語 😀.pdf", "COM0.txt", "COM10.txt", "LPT0", "CONtrat.pdf"}
            Assert.AreEqual(nom, AppelerUtilitaire("CleanFileName", nom))
            Assert.AreEqual(nom, AppelerUtilitaire("GetAttachmentFileName", Nothing, nom))
            Assert.AreEqual(nom, AppelerUtilitaire("CleanFileName", nom & ". . "))
        Next
        For code = 0 To 31
            Assert.AreEqual("a_b.pdf", AppelerUtilitaire("CleanFileName", "a" & ChrW(code) & "b.pdf"))
        Next
        Assert.AreEqual("a_________b", AppelerUtilitaire("CleanFileName", "a<>:""/\|?*b"))
        Assert.AreEqual("attachment.bin", AppelerUtilitaire("GetAttachmentFileName", "..", "valide.pdf"))
    End Sub

    <TestMethod>
    Public Sub EnsureUniqueFileName_ConserveUnCheminLibre()
        Dim chemin = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".pdf")

        Assert.IsFalse(File.Exists(chemin))
        Assert.AreEqual(chemin, AppelerUtilitaire("EnsureUniqueFileName", chemin))
    End Sub

    <TestMethod>
    Public Sub EnsureUniqueFileName_GenereDesNomsDistinctsSansEcraser()
        ' L'assembly de tests existe deja : aucun fichier n'est cree, modifie ou supprime.
        Dim cheminExistant = GetType(EmailProcessorFileNameTests).Assembly.Location
        Assert.IsTrue(File.Exists(cheminExistant))

        Dim candidat1 = AppelerUtilitaire("EnsureUniqueFileName", cheminExistant)
        Dim candidat2 = AppelerUtilitaire("EnsureUniqueFileName", cheminExistant)

        Assert.AreNotEqual(cheminExistant, candidat1)
        Assert.AreNotEqual(candidat1, candidat2)
        Assert.AreEqual(Path.GetDirectoryName(cheminExistant), Path.GetDirectoryName(candidat1))
        Assert.AreEqual(Path.GetExtension(cheminExistant), Path.GetExtension(candidat1))
        Assert.IsFalse(File.Exists(candidat1))
        Assert.IsFalse(File.Exists(candidat2))
    End Sub
End Class
