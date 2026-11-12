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

    <TestMethod>
    Public Sub SaveRefuseUnIdentifiantNegatifSansAccesSql()
        Dim formulaire As New FrmNouveauTiers()
        Dim coordonneesFictives As New Coordonnees()
        coordonneesFictives.IdTiers = -7

        Dim exceptionLevee As Boolean = False
        Try
            formulaire.Save(coordonneesFictives)
        Catch ex As ArgumentOutOfRangeException
            exceptionLevee = True
        End Try

        Assert.IsTrue(exceptionLevee, "Un identifiant négatif doit être rejeté avant toute écriture.")
    End Sub

    <TestMethod>
    Public Sub SaveRefuseDesCoordonneesAbsentesSansAccesSql()
        Dim formulaire As New FrmNouveauTiers()
        Dim exceptionLevee As Boolean = False
        Try
            formulaire.Save(Nothing)
        Catch ex As ArgumentNullException
            exceptionLevee = True
        End Try

        Assert.IsTrue(exceptionLevee, "Des coordonnées absentes doivent être rejetées avant toute écriture.")
    End Sub
End Class
