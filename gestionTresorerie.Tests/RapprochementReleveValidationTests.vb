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
    <TestMethod>
    Public Sub ConfirmationAfficheLesIdentifiantsTriesSansDoublon()
        Dim texte = FrmRapprochementReleve.ConfirmationRattachement({42, 7, 42}, 19)
        StringAssert.Contains(texte, "base TEST")
        StringAssert.Contains(texte, "7, 42")
        StringAssert.Contains(texte, "mouvement n° 19")
    End Sub

    <TestMethod>
    Public Sub ConfirmationSansMouvementAfficheUniquementLesPieces()
        Dim texte = FrmRapprochementReleve.ConfirmationRattachement({5}, Nothing)
        StringAssert.Contains(texte, "Justificatif(s) n° 5")
        Assert.IsFalse(texte.Contains("mouvement n°"))
    End Sub
End Class
