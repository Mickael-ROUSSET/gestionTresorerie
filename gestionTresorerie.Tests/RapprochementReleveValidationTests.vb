Option Strict On

Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RapprochementReleveValidationTests
    <TestMethod>
    Public Sub RefuseLaValidationSansJustificatifNiMouvement()
        Assert.IsFalse(FrmRapprochementReleve.PeutRattacher(0, False))
    End Sub

    <TestMethod>
    Public Sub AccepteUnOuPlusieursJustificatifs()
        Assert.IsTrue(FrmRapprochementReleve.PeutRattacher(1, False))
        Assert.IsTrue(FrmRapprochementReleve.PeutRattacher(2, False))
    End Sub

    <TestMethod>
    Public Sub AccepteUnMouvementExistantSansJustificatif()
        Assert.IsTrue(FrmRapprochementReleve.PeutRattacher(0, True))
    End Sub
End Class
