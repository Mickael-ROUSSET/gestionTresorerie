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

        Dim exceptionLevee As Boolean = False
        Try
            formulaire.Save(coordonneesFictives)
        Catch ex As ArgumentOutOfRangeException
            exceptionLevee = True
        End Try

        Assert.IsTrue(exceptionLevee, "Save doit refuser un identifiant de tiers non persiste.")
    End Sub
End Class
