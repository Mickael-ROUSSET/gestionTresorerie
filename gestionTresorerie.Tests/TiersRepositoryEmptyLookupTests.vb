Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq

<TestClass>
Public Class TiersRepositoryEmptyLookupTests

    <TestMethod>
    Public Sub TiersExiste_IdentiteVide_NeDeclenchePasSql()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 1}
        Dim repository As New TiersRepository(fake)

        Assert.IsFalse(repository.TiersExiste("  ", "Prénom seul", Nothing))
        Assert.IsNull(fake.LastQueryName, "Aucune requête SQL ne doit être exécutée pour une identité vide.")
    End Sub

    <TestMethod>
    Public Sub TiersExiste_RaisonSocialeSansNom_InterrogeSql()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 1}
        Dim repository As New TiersRepository(fake)

        Assert.IsTrue(repository.TiersExiste(Nothing, Nothing, " Association fictive "))
        Assert.AreEqual("cptTiers", fake.LastQueryName)
        Dim parameters = fake.LastParameters.ToList()
        Assert.AreEqual(DBNull.Value, parameters(0).Value)
        Assert.AreEqual("Association fictive", parameters(2).Value)
    End Sub

    <TestMethod>
    Public Sub TiersExiste_NomSansRaisonSociale_InterrogeSql()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 0}
        Dim repository As New TiersRepository(fake)

        Assert.IsFalse(repository.TiersExiste(" Dupont ", " Alice ", Nothing))
        Assert.AreEqual("cptTiers", fake.LastQueryName)
        Dim parameters = fake.LastParameters.ToList()
        Assert.AreEqual("Dupont", parameters(0).Value)
        Assert.AreEqual("Alice", parameters(1).Value)
    End Sub

End Class
