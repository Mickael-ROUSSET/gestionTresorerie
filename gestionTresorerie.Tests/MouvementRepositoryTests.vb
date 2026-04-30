Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class MouvementRepositoryTests

    <TestMethod>
    Public Sub Supprimer_UtiliseDelMvtAvecId()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New MouvementRepository(fake, Nothing, Nothing)

        Dim nb = repo.Supprimer(42)

        Assert.AreEqual(1, nb)
        Assert.AreEqual("delMvt", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.AreEqual("@Id", p(0).ParameterName)
        Assert.AreEqual(42, p(0).Value)
    End Sub

End Class