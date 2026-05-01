Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq

<TestClass>
Public Class AdresseRepositoryTests

    <TestMethod>
    Public Sub Supprimer_UtiliseDeleteAvecBonId()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New AdresseRepository(fake)

        Dim nb = repo.Supprimer(123)

        Assert.AreEqual(1, nb)
        Assert.AreEqual("DELETE FROM Coordonnees WHERE Id = @Id", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(1, p)
        Assert.AreEqual("@Id", p(0).ParameterName)
        Assert.AreEqual(123, p(0).Value)
    End Sub

End Class