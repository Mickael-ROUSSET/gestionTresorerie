Option Strict On

Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class FrmNouveauTiersCoordonneesTests
    <TestMethod>
    Public Sub RefuseLesIdentifiantsNonPersistesAvantTouteSauvegarde()
        Assert.IsFalse(FrmNouveauTiers.IdentifiantTiersValide(0))
        Assert.IsFalse(FrmNouveauTiers.IdentifiantTiersValide(-1))
        Assert.IsTrue(FrmNouveauTiers.IdentifiantTiersValide(42))
    End Sub

    <TestMethod>
    Public Sub SaveRefuseDesCoordonneesSansTiersPersiste()
        Dim formulaire As New FrmNouveauTiers()
        Dim coordonneesFictives As New Coordonnees()
        coordonneesFictives.IdTiers = 0

        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub() formulaire.Save(coordonneesFictives))
    End Sub
End Class
