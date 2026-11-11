Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersInsertionInvalidIdTests

    <DataTestMethod>
    <DataRow(-1)>
    <DataRow(0)>
    <DataRow(-2147483648)>
    Public Sub InsererRefuseLesIdentifiantsNonPositifs(id As Integer)
        Dim fake As New FakeSqlExecutor With {.ScalarResult = id}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(0, repo.Inserer("Association fictive", "Alice", "Exemple", Nothing, Nothing))
        Assert.AreEqual("insertTiers", fake.LastQueryName)
    End Sub

    <TestMethod>
    Public Sub InsererAccepteUnIdentifiantStrictementPositif()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 42}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(42, repo.Inserer("Association fictive", "Alice", "Exemple", Nothing, Nothing))
    End Sub

    <TestMethod>
    Public Sub InsererRefuseUnIdentifiantHorsPlageInteger()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = "2147483648"}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(0, repo.Inserer("Association fictive", "Alice", "Exemple", Nothing, Nothing))
    End Sub
End Class
