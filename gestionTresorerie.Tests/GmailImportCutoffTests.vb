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
            Dim path = Path.Combine(folder, "gmail_import_cutoff.txt")
            Dim rejected = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(path, Nothing)
            Catch ex As InvalidOperationException
                rejected = True
            End Try
            Assert.IsTrue(rejected)
            Assert.IsFalse(File.Exists(path))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub BornePersistante_ConserveDateEtRefuseChangement()
        Dim folder = NouveauDossier()
        Try
            Dim path = Path.Combine(folder, "gmail_import_cutoff.txt")
            Dim startDate As New DateTime(2026, 10, 1)
            Assert.AreEqual(startDate, EmailProcessor.ResolveInitialImportCutoff(path, startDate))
            Assert.AreEqual("2026-10-01", File.ReadAllText(path))
            Assert.AreEqual(startDate, EmailProcessor.ResolveInitialImportCutoff(path, Nothing))
            Dim rejected = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(path, New DateTime(2026, 9, 1))
            Catch ex As InvalidOperationException
                rejected = True
            End Try
            Assert.IsTrue(rejected)
            Assert.AreEqual("2026-10-01", File.ReadAllText(path))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub BorneCorrompue_NePermetPasDeRepli()
        Dim folder = NouveauDossier()
        Try
            Dim path = Path.Combine(folder, "gmail_import_cutoff.txt")
            File.WriteAllText(path, "date invalide")
            Dim rejected = False
            Try
                EmailProcessor.ResolveInitialImportCutoff(path, New DateTime(2026, 10, 1))
            Catch ex As InvalidOperationException
                rejected = True
            End Try
            Assert.IsTrue(rejected)
            Assert.AreEqual("date invalide", File.ReadAllText(path))
        Finally
            Directory.Delete(folder, True)
        End Try
    End Sub
End Class
