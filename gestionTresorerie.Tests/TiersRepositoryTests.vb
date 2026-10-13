Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq

<TestClass>
Public Class TiersRepositoryTests

    <TestMethod>
    Public Sub TiersExiste_RetourneTrue_SiCountSup0()
        Dim fake As New FakeSqlExecutor()
        fake.ScalarResult = 1

        Dim repo As New TiersRepository(fake)

        Dim result = repo.TiersExiste("Dupont", "Jean", "")

        Assert.IsTrue(result)
        Assert.AreEqual("cptTiers", fake.LastQueryName)
        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(3, p)
        Assert.AreEqual("@nom", p(0).ParameterName)
        Assert.AreEqual("Dupont", p(0).Value)
        Assert.AreEqual("@prenom", p(1).ParameterName)
        Assert.AreEqual("Jean", p(1).Value)
        Assert.AreEqual("@raisonSociale", p(2).ParameterName)
        Assert.AreEqual(DBNull.Value, p(2).Value)
    End Sub

    <TestMethod>
    Public Sub TiersExiste_RetourneFalse_SiCountZero()
        Dim fake As New FakeSqlExecutor()
        fake.ScalarResult = 0

        Dim repo As New TiersRepository(fake)

        Dim result = repo.TiersExiste("Dupont", "Jean", "")

        Assert.IsFalse(result)
    End Sub

End Class