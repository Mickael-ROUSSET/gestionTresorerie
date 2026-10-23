Imports System.IO
Imports System.Reflection
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class EmailProcessorFileNameTests
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
        Const nomOrdinaire As String = " facture 2026.pdf "
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
