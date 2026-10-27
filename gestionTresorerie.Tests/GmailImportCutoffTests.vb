Imports System
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailImportCutoffTests
    Private Shared Function NouveauDossier() As String
        Dim folder = Path.Combine(Path.GetTempPath(), "gmail-cutoff-test-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(folder)
        Return folder
    End Function

    <TestMethod>
    Public Sub SansDateNiFichier_ImportInitialBloque()
        Dim folder = NouveauDossier()
        Try
            Dim cutoffFile = Path.Combine(folder, "gmail_import_cutoff.txt")
            Dim rejected = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(cutoffFile, Nothing)
            Catch ex As InvalidOperationException
                rejected = True
            End Try
            Assert.IsTrue(rejected)
            Assert.IsFalse(File.Exists(cutoffFile))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub BornePersistante_ConserveDateEtRefuseChangement()
        Dim folder = NouveauDossier()
        Try
            Dim cutoffFile = Path.Combine(folder, "gmail_import_cutoff.txt")
            Dim startDate As New DateTime(2026, 10, 1)
            Assert.AreEqual(startDate, EmailProcessor.ResolveInitialImportCutoff(cutoffFile, startDate))
            Assert.AreEqual("2026-10-01", File.ReadAllText(cutoffFile))
            Assert.AreEqual(startDate, EmailProcessor.ResolveInitialImportCutoff(cutoffFile, Nothing))
            Dim rejected = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(cutoffFile, New DateTime(2026, 9, 1))
            Catch ex As InvalidOperationException
                rejected = True
            End Try
            Assert.IsTrue(rejected)
            Assert.AreEqual("2026-10-01", File.ReadAllText(cutoffFile))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub BorneCorrompue_NePermetPasDeRepli()
        Dim folder = NouveauDossier()
        Try
            Dim cutoffFile = Path.Combine(folder, "gmail_import_cutoff.txt")
            File.WriteAllText(cutoffFile, "date invalide")
            Dim rejected = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(cutoffFile, New DateTime(2026, 10, 1))
            Catch ex As InvalidOperationException
                rejected = True
            End Try
            Assert.IsTrue(rejected)
            Assert.AreEqual("date invalide", File.ReadAllText(cutoffFile))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub
End Class
