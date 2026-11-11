Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersRepositoryEmptyIdentityTests

    <TestMethod>
    Public Sub Inserer_IdentiteVide_NeContactePasSql()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 42}
        Dim repository As New TiersRepository(fake)

        Dim id = repository.Inserer("  ", "Jean", Nothing, Nothing, Nothing)

        Assert.AreEqual(0, id)
        Assert.IsNull(fake.LastQueryName, "Une identité sans nom ni raison sociale ne doit pas déclencher de SQL.")
    End Sub

    <TestMethod>
    Public Sub Inserer_NomRenseigne_AutoriseInsertion()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 42}
        Dim repository As New TiersRepository(fake)

        Dim id = repository.Inserer(Nothing, "Jean", " Dupont ", Nothing, Nothing)

        Assert.AreEqual(42, id)
        Assert.AreEqual("insertTiers", fake.LastQueryName)
    End Sub

    <TestMethod>
    Public Sub Inserer_RaisonSocialeRenseignee_AutoriseInsertion()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 43}
        Dim repository As New TiersRepository(fake)

        Dim id = repository.Inserer("Association fictive", Nothing, Nothing, Nothing, Nothing)

        Assert.AreEqual(43, id)
        Assert.AreEqual("insertTiers", fake.LastQueryName)
    End Sub

End Class
