Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports gestionTresorerie

<TestClass>
Public Class TestDocumentImportGuardTests
    <TestMethod>
    Public Sub ProductionSansIsolation_RefuseLesImportsDeDossiers()
        Assert.IsFalse(FrmPrincipale.IsTestDocumentImportAllowed(
            False, False, EnvironmentManager.EnvironnementType.PROD))
    End Sub

    <TestMethod>
    Public Sub TestOuDemonstration_AutoriseLesImportsDeDossiers()
        Assert.IsTrue(FrmPrincipale.IsTestDocumentImportAllowed(
            False, False, EnvironmentManager.EnvironnementType.TEST))
        Assert.IsTrue(FrmPrincipale.IsTestDocumentImportAllowed(
            True, False, EnvironmentManager.EnvironnementType.PROD))
        Assert.IsTrue(FrmPrincipale.IsTestDocumentImportAllowed(
            False, True, EnvironmentManager.EnvironnementType.PROD))
    End Sub
    <TestMethod>
    Public Sub ListeScansTest_ExclutFichiersVidesEtSousDossiers()
        Dim folder = IO.Path.Combine(IO.Path.GetTempPath(), "gestionTresorerie-scans-" & Guid.NewGuid().ToString("N"))
        IO.Directory.CreateDirectory(folder)
        Try
            Dim pdf = IO.Path.Combine(folder, "b.pdf")
            Dim png = IO.Path.Combine(folder, "A.png")
            IO.File.WriteAllBytes(pdf, New Byte() {37, 80, 68, 70})
            IO.File.WriteAllBytes(png, New Byte() {1, 2, 3})
            IO.File.WriteAllBytes(IO.Path.Combine(folder, "vide.pdf"), Array.Empty(Of Byte)())
            IO.File.WriteAllText(IO.Path.Combine(folder, "notes.txt"), "fictif")
            Dim child = IO.Directory.CreateDirectory(IO.Path.Combine(folder, "sous-dossier"))
            IO.File.WriteAllBytes(IO.Path.Combine(child.FullName, "autre.pdf"), New Byte() {37, 80, 68, 70})

            CollectionAssert.AreEqual(New String() {png, pdf}, FrmPrincipale.ListSafeTestScanFiles(folder))
        Finally
            IO.Directory.Delete(folder, recursive:=True)
        End Try
    End Sub

End Class
