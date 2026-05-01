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
        Assert.AreEqual("getIdTiersByNomPrenomDate", fake.LastQueryName)
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