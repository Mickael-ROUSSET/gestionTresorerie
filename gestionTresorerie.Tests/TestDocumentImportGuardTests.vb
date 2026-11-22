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
End Class
