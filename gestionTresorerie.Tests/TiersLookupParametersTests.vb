Option Strict On

Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersLookupParametersTests

    <TestMethod>
    Public Sub RechercheParIdentiteNormaliseLesChampsEtLaDate()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 17}
        Dim repo As New TiersRepository(fake)
        Dim naissance As New DateTime(1990, 4, 12, 18, 45, 0)

        Dim id = repo.GetIdTiersByUser("  Exemple  ", "  Alice ", naissance)

        Assert.AreEqual(17, id)
        Assert.AreEqual("getIdTiersByNomPrenomDate", fake.LastQueryName)
        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(3, p)
        Assert.AreEqual("@nom", p(0).ParameterName)
        Assert.AreEqual("Exemple", p(0).Value)
        Assert.AreEqual("@prenom", p(1).ParameterName)
        Assert.AreEqual("Alice", p(1).Value)
        Assert.AreEqual("@dateNaissance", p(2).ParameterName)
        Assert.AreEqual(New DateTime(1990, 4, 12), p(2).Value)
    End Sub

    <TestMethod>
    Public Sub RechercheParIdentiteConvertitLesValeursAbsentesEnDbNull()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 0}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(0, repo.GetIdTiersByUser(" ", Nothing))

        Assert.AreEqual("getIdTiersByNomPrenomDate", fake.LastQueryName)
        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(3, p)
        Assert.AreEqual(DBNull.Value, p(0).Value)
        Assert.AreEqual(DBNull.Value, p(1).Value)
        Assert.AreEqual(DBNull.Value, p(2).Value)
    End Sub
End Class
