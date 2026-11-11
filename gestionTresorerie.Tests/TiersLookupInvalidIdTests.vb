Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersLookupInvalidIdTests

    <DataTestMethod>
    <DataRow(-1)>
    <DataRow(Integer.MinValue)>
    <DataRow(0)>
    Public Sub RechercheNeRetourneJamaisUnIdentifiantNonPositif(id As Integer)
        Dim fake As New FakeSqlExecutor With {.ScalarResult = id}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(0, repo.GetIdTiersByUser("Exemple", "Alice"))
        Assert.AreEqual("getIdTiersByNomPrenomDate", fake.LastQueryName)
    End Sub

    <TestMethod>
    Public Sub RecherchePreserveUnIdentifiantValide()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 73}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(73, repo.GetIdTiersByUser("Exemple", "Alice"))
    End Sub
End Class
