Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq

<TestClass>
Public Class MouvementRepositoryTests

    <TestMethod>
    Public Sub Supprimer_UtiliseDelMvt_AvecBonId()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New MouvementRepository(fake, Nothing, Nothing)

        Dim nb = repo.Supprimer(42)

        Assert.AreEqual(1, nb)
        Assert.AreEqual("delMvt", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(1, p)

        Assert.AreEqual("@Id", p(0).ParameterName)
        Assert.AreEqual(42, p(0).Value)
    End Sub

    <TestMethod>
    Public Sub MettreAJourIdDoc_UtiliseUpdMvtIdDoc()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New MouvementRepository(fake, Nothing, Nothing)

        Dim nb = repo.MettreAJourIdDoc(10, 99)

        Assert.AreEqual(1, nb)
        Assert.AreEqual("updMvtIdDoc", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(2, p)

        Assert.AreEqual("@IdMvt", p(0).ParameterName)
        Assert.AreEqual(10, p(0).Value)

        Assert.AreEqual("@IdDoc", p(1).ParameterName)
        Assert.AreEqual(99, p(1).Value)
    End Sub

End Class