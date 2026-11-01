Imports System.IO
Imports System.Reflection
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class EmailProcessorFileNameTests
    <TestMethod>
    <TestCategory("OfflineWindowsAcceptance")>
    <DataRow("facture été 日本語.pdf", "facture été 日本語.pdf")>
    <DataRow("..\facture.pdf", "facture.pdf")>
    <DataRow("CON.pdf", "CON.pdf")>
    <DataRow("facture.pdf:flux", "facture.pdf:flux")>
    Public Sub PieceJointeMime_AllerRetourHorsLigne_VerifieLeNomEtLeContenu(nom As String, attendu As String)
        ' Public MIME APIs only: no ProcessAttachments, credentials or IMAP client.
        ' BodyBuilder removes directory components but retains reserved/stream names.
        ' Windows sanitization remains covered
        ' by the existing CleanFileName tests below, not by this round-trip test.
        Using message As New MimeKit.MimeMessage()
            Dim builder As New MimeKit.BodyBuilder()
            builder.Attachments.Add(nom, New Byte() {1, 2, 3})
            message.Body = builder.ToMessageBody()
            Using buffer As New MemoryStream()
                message.WriteTo(buffer)
                buffer.Position = 0
                Using relu = MimeKit.MimeMessage.Load(buffer)
                    Dim nombre = 0
                    For Each piece In relu.Attachments
                        Dim fichier = DirectCast(piece, MimeKit.MimePart)
                        Assert.AreEqual(attendu, fichier.FileName)
                        Using contenu As New MemoryStream()
                            fichier.Content.DecodeTo(contenu)
                            CollectionAssert.AreEqual(New Byte() {1, 2, 3}, contenu.ToArray())
                        End Using
                        nombre += 1
                    Next
                    Assert.AreEqual(1, nombre)
                End Using
            End Using
        End Using
    End Sub

    Private Shared Function TraiterMessage(uid As UInteger, processMessage As Action,
                                          markProcessed As Action, reportError As Action(Of String)) As Boolean
        Dim method = GetType(EmailProcessor).GetMethod("TryProcessMessage", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(method)
        Return CBool(method.Invoke(Nothing, New Object() {New MailKit.UniqueId(uid), processMessage, markProcessed, reportError}))
    End Function

    <TestMethod>
    Public Sub TryProcessMessage_Succes_ValideSeulementApresTraitement()
        Dim events As New List(Of String)()
        Assert.IsTrue(TraiterMessage(34UI, Sub() events.Add("traitement"),
                                    Sub() events.Add("journal"), Sub(value) events.Add(value)))
        CollectionAssert.AreEqual(New String() {"traitement", "journal"}, events)
    End Sub

    <TestMethod>
    <DataRow(False)>
    <DataRow(True)>
    Public Sub TryProcessMessage_Echec_ResteReprenableEtNeBloquePasLeSuivant(journalFailure As Boolean)
        Dim events As New List(Of String)()
        Dim errors As New List(Of String)()
        Dim processMessage As Action =
            Sub()
                events.Add("traitement")
                If Not journalFailure Then Throw New IOException("contenu prive" & vbCrLf & "diagnostic injecte")
            End Sub
        Dim markProcessed As Action =
            Sub()
                events.Add("journal")
                Throw New IOException("chemin prive" & vbCrLf & "diagnostic injecte")
            End Sub

        Assert.IsFalse(TraiterMessage(34UI, processMessage, markProcessed, Sub(value) errors.Add(value)))
        CollectionAssert.AreEqual(If(journalFailure, New String() {"traitement", "journal"},
                                    New String() {"traitement"}), events)
        CollectionAssert.AreEqual(New String() {
            "Message UID 34 non traite : traitement ou validation du journal impossible. Reprise au prochain import."}, errors)

        ' Le message suivant et la reprise du message en echec peuvent tous deux aboutir.
        Dim validated As New List(Of UInteger)()
        For Each uid In New UInteger() {35UI, 34UI}
            Assert.IsTrue(TraiterMessage(uid, Sub() events.Add("reprise"),
                                        Sub() validated.Add(uid), Sub(value) errors.Add(value)))
        Next
        CollectionAssert.AreEqual(New UInteger() {35UI, 34UI}, validated)
        Assert.AreEqual(1, errors.Count)
    End Sub

    <TestMethod>
    Public Sub TryProcessMessage_EchecDuRapport_NeBloquePasLesUidSuivants()
        Dim processed As New List(Of UInteger)()
        Dim validated As New List(Of UInteger)()
        Dim reports As Integer = 0
        Dim failingReporter As Action(Of String) =
            Sub(message)
                reports += 1
                Throw New IOException("rapport fictif indisponible")
            End Sub
        Dim failingProcessing As Action =
            Sub()
                Throw New IOException("piece jointe fictive invalide")
            End Sub

        Assert.IsFalse(TraiterMessage(81UI, failingProcessing,
                                     Sub() validated.Add(81UI), failingReporter))
        Assert.AreEqual(1, reports)
        Assert.AreEqual(0, validated.Count)

        ' Le message suivant et une reprise ulterieure restent possibles.
        Assert.IsTrue(TraiterMessage(82UI, Sub() processed.Add(82UI),
                                    Sub() validated.Add(82UI), failingReporter))
        Assert.IsTrue(TraiterMessage(81UI, Sub() processed.Add(81UI),
                                    Sub() validated.Add(81UI), failingReporter))
        CollectionAssert.AreEqual(New UInteger() {82UI, 81UI}, processed)
        CollectionAssert.AreEqual(New UInteger() {82UI, 81UI}, validated)
        Assert.AreEqual(1, reports)
    End Sub

    <TestMethod>
    Public Sub ReportImapFailure_ExceptionAvecDonneesPrivees_NeDivulgueRien()
        Dim privateInfo = "C:\\Users\\prive\\secret.txt" & vbCrLf & "utilisateur@example.invalid"
        Dim reports As New List(Of String)()
        Dim method = GetType(EmailProcessor).GetMethod("ReportImapFailure", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(method)
        For Each failure As Exception In New Exception() {
            New IOException(privateInfo),
            New UnauthorizedAccessException(privateInfo)}
            method.Invoke(Nothing, New Object() {failure, CType(Sub(value As String) reports.Add(value), Action(Of String))})
        Next
        Assert.AreEqual(2, reports.Count)
        For Each message In reports
            Assert.AreEqual("Connexion ou traitement IMAP interrompu. Verifier la configuration Gmail et relancer.", message)
            Assert.IsFalse(message.Contains(privateInfo, StringComparison.Ordinal))
            Assert.IsFalse(message.Contains("example.invalid", StringComparison.Ordinal))
        Next
    End Sub

    Private Shared Function InitialiserJournal(path As String, expected As Boolean, hasEntries As Boolean) As GmailImportJournal
        Dim method = GetType(EmailProcessor).GetMethod("TryInitializeJournal", BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(method)
        Dim report As New System.Text.StringBuilder()
        Dim args As Object() = {path, Nothing, True, report}
        Assert.AreEqual(expected, CBool(method.Invoke(Nothing, args)))
        Assert.AreEqual(hasEntries, CBool(args(2)))
        If expected Then
            Assert.AreEqual("", report.ToString())
            Assert.IsNotNull(args(1))
        Else
            Assert.IsNull(args(1))
            Assert.AreEqual("[ERREUR] Import Gmail bloque : journal invalide ou inaccessible. Verifier gmail_import_journal.txt avant de relancer." & Environment.NewLine, report.ToString())
        End If
        Return DirectCast(args(1), GmailImportJournal)
    End Function

    <TestMethod>
    Public Sub TryInitializeJournal_AbsentVideEtValide_PreserveLesUid()
        Dim folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GmailJournalTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim path = System.IO.Path.Combine(folder, "journal.txt")
            InitialiserJournal(path, True, False)
            Assert.IsFalse(File.Exists(path))
            File.WriteAllText(path, vbCrLf)
            InitialiserJournal(path, True, False)
            Dim key = GmailImportJournal.MakeKey("offline@example.invalid", 12UI, 34UI)
            Dim contents = key & vbCrLf & vbCrLf & key & vbCrLf
            File.WriteAllText(path, contents, New System.Text.UTF8Encoding(True))
            Dim journal = InitialiserJournal(path, True, True)
            Assert.IsTrue(journal.IsProcessed(key))
            Assert.IsFalse(journal.IsProcessed(GmailImportJournal.MakeKey("offline@example.invalid", 13UI, 34UI)))
            Assert.IsFalse(journal.IsProcessed(GmailImportJournal.MakeKey("offline@example.invalid", 12UI, 35UI)))
            Assert.AreEqual(contents, File.ReadAllText(path))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub TryInitializeJournal_IllisibleOuMalforme_BloqueSansDivulgation()
        Dim folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GmailJournalTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim path = System.IO.Path.Combine(folder, "journal.txt")
            Dim key = GmailImportJournal.MakeKey("offline@example.invalid", 12UI, 34UI)
            For Each invalid In New String() {New String("x"c, 10000), " ", "GG:1:2", "61:0:2", "61:1:4294967296", "61:1", "FF:1:2"}
                Dim contents = key & vbCrLf & invalid
                File.WriteAllText(path, contents)
                InitialiserJournal(path, False, False)
                Assert.AreEqual(contents, File.ReadAllText(path))
            Next
            File.WriteAllBytes(path, New Byte() {&HFF, &HFF})
            InitialiserJournal(path, False, False)
            File.WriteAllText(path, key)
            Using locked As New FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                InitialiserJournal(path, False, False)
            End Using
            InitialiserJournal(folder, False, False)
            InitialiserJournal("invalid" & ChrW(0), False, False)
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

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
            Assert.IsFalse(report.ToString().Contains(logPath, StringComparison.Ordinal), "Le chemin prive du journal ne doit pas figurer dans le rapport.")
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
    Public Sub IsImportAllowed_JournalInitialise_PreserveLaRepriseSansDate()
        Assert.IsTrue(EmailProcessor.IsImportAllowed(True, Nothing))
        Assert.IsTrue(EmailProcessor.IsImportAllowed(True, New DateTime(2026, 10, 1)))
    End Sub

    <TestMethod>
    Public Sub ResolveInitialImportCutoff_EnregistreEtRelitLaMemeDate()
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim cutoffFile = Path.Combine(folder, "cutoff.txt")
            Assert.IsFalse(EmailProcessor.ResolveInitialImportCutoff(cutoffFile, Nothing).HasValue)
            Assert.IsFalse(File.Exists(cutoffFile))
            Dim debut As New DateTime(2026, 10, 1)
            Assert.AreEqual(debut, EmailProcessor.ResolveInitialImportCutoff(cutoffFile, debut.AddHours(12)).Value)
            Assert.AreEqual("2026-10-01", File.ReadAllText(cutoffFile))
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

    <TestMethod>
    <DataRow(False)>
    <DataRow(True)>
    Public Sub ResolveInitialImportCutoff_CreationsConcurrentes_PreserventLaDateGagnante(datesDifferentes As Boolean)
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            For iteration = 1 To 20
                Dim cutoffFile = Path.Combine(folder, iteration.ToString() & ".txt")
                Dim dates = {New DateTime(2026, 10, 1), New DateTime(2026, 10, If(datesDifferentes, 2, 1))}
                Using start As New System.Threading.Barrier(2)
                    Dim attempts = Array.ConvertAll(dates,
                        Function(requested) System.Threading.Tasks.Task.Run(
                            Function() As DateTime?
                                Assert.IsTrue(start.SignalAndWait(TimeSpan.FromSeconds(10)))
                                Try
                                    Return EmailProcessor.ResolveInitialImportCutoff(cutoffFile, requested)
                                Catch ex As IOException
                                    ' Le fichier en cours d'ecriture reste inaccessible : echec ferme autorise.
                                    Return Nothing
                                Catch ex As InvalidOperationException
                                    If Not datesDifferentes Then Throw
                                    Return Nothing
                                End Try
                            End Function))
                    System.Threading.Tasks.Task.WaitAll(attempts)
                    Dim stored = EmailProcessor.ResolveInitialImportCutoff(cutoffFile, Nothing).Value
                    Assert.IsTrue(dates.Contains(stored))
                    Assert.IsTrue(attempts.Any(Function(attempt) attempt.Result.HasValue))
                    For Each attempt In attempts
                        If attempt.Result.HasValue Then Assert.AreEqual(stored, attempt.Result.Value)
                    Next
                    Assert.AreEqual(stored.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture), File.ReadAllText(cutoffFile))
                    For Each requested In dates
                        If requested = stored Then
                            Assert.AreEqual(stored, EmailProcessor.ResolveInitialImportCutoff(cutoffFile, requested).Value)
                        Else
                            Assert.ThrowsExactly(Of InvalidOperationException)(
                                Sub() EmailProcessor.ResolveInitialImportCutoff(cutoffFile, requested))
                        End If
                    Next
                End Using
            Next
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
    <DataRow("date invalide")>
    <DataRow("")>
    <DataRow("2026-10-")>
    Public Sub TryResolveInitialImportCutoff_FichierMalforme_BloqueEtRapporteErreur(contents As String)
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim cutoffFile = Path.Combine(folder, "cutoff.txt")
            File.WriteAllText(cutoffFile, contents)
            VerifierCutoffBloque(cutoffFile)
            Assert.AreEqual(contents, File.ReadAllText(cutoffFile))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub TryResolveInitialImportCutoff_NeDivulguePasLeContenuInvalide()
        Dim folder = Path.Combine(Path.GetTempPath(), "GmailCutoffTests_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Try
            Dim cutoffFile = Path.Combine(folder, "cutoff.txt")
            Const contenuPrive As String = "DONNEE_CONFIDENTIELLE_FICTIVE_2026"
            File.WriteAllText(cutoffFile, contenuPrive)
            Dim methode = GetType(EmailProcessor).GetMethod("TryResolveInitialImportCutoff", BindingFlags.NonPublic Or BindingFlags.Static)
            Assert.IsNotNull(methode)
            Dim rapport As New System.Text.StringBuilder()
            Dim arguments As Object() = {cutoffFile, Nothing, Nothing, rapport}
            Assert.IsFalse(CBool(methode.Invoke(Nothing, arguments)))
            Assert.IsNull(arguments(2))
            StringAssert.Contains(rapport.ToString(), "gmail_import_cutoff.txt")
            Assert.IsFalse(rapport.ToString().Contains(contenuPrive, StringComparison.Ordinal))
            Assert.IsFalse(rapport.ToString().Contains(cutoffFile, StringComparison.Ordinal))
            Assert.AreEqual(contenuPrive, File.ReadAllText(cutoffFile))
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
            Assert.IsFalse(report.ToString().Contains(cutoffFile, StringComparison.Ordinal), "Le chemin prive ne doit pas figurer dans le rapport.")
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
    Public Sub CleanFileName_NomsDosCOMLPTAvecChiffresExposants_SontNeutralises()
        ' Windows reserve aussi COM¹, COM², COM³ et LPT¹, LPT², LPT³.
        ' Les chiffres exposes restent valides dans les autres noms.
        For Each prefixe In New String() {"COM", "LPT"}
            For Each chiffre In New Char() {ChrW(&HB9), ChrW(&HB2), ChrW(&HB3)}
                For Each suffixe In New String() {"", ".pdf", ".tar.gz", " .txt"}
                    Dim nom = prefixe.ToLowerInvariant() & chiffre & suffixe
                    Dim attendu = "_" & nom
                    Assert.AreEqual(attendu, AppelerUtilitaire("CleanFileName", nom))
                    Assert.AreEqual(attendu, AppelerUtilitaire("GetAttachmentFileName", nom, Nothing))
                    Assert.AreEqual(attendu, AppelerUtilitaire("CleanFileName", attendu))
                Next
                Assert.AreEqual("facture" & chiffre & ".pdf",
                                AppelerUtilitaire("CleanFileName", "facture" & chiffre & ".pdf"))
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
