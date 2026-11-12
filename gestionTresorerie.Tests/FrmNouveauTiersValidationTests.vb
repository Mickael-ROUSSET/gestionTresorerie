Option Strict On

Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmNouveauTiersValidationTests
    <TestMethod>
    Public Sub IdentiteValideRefuseLePrenomSeulEtLesChampsBlancs()
        Assert.IsFalse(FrmNouveauTiers.IdentiteValide(Nothing, Nothing))
        Assert.IsFalse(FrmNouveauTiers.IdentiteValide("  ", " "))
        Assert.IsFalse(FrmNouveauTiers.IdentiteValide("", ""))
    End Sub

    <TestMethod>
    Public Sub IdentiteValideAccepteNomOuRaisonSocialeFictifs()
        Assert.IsTrue(FrmNouveauTiers.IdentiteValide("TEST-ALPHA", Nothing))
        Assert.IsTrue(FrmNouveauTiers.IdentiteValide(Nothing, "ASSOCIATION-TEST"))
    End Sub
End Class
