Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq

<TestClass>
Public Class DocumentRepositoryTests

    <TestMethod>
    Public Sub MettreAJourChemin_UtiliseUpdCheminDoc()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New DocumentRepository(fake, Nothing, Nothing)

        repo.MettreAJourChemin(5, "C:\test.pdf")

        Assert.AreEqual("updCheminDoc", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(2, p)

        Assert.AreEqual("@idDoc", p(0).ParameterName)
        Assert.AreEqual(5, p(0).Value)

        Assert.AreEqual("@cheminDoc", p(1).ParameterName)
        Assert.AreEqual("C:\test.pdf", p(1).Value)
    End Sub

    <TestMethod>
    Public Sub MettreAJourMetaDonnees_UtiliseUpdateDocumentMetaDonnees()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New DocumentRepository(fake, Nothing, Nothing)

        repo.MettreAJourMetaDonnees(5, "{""a"":1}")

        Assert.AreEqual("updateDocumentMetaDonnees", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(2, p)

        Assert.AreEqual("@idDocument", p(0).ParameterName)
        Assert.AreEqual(5, p(0).Value)

        Assert.AreEqual("@metaDonnees", p(1).ParameterName)
        Assert.AreEqual("{""a"":1}", p(1).Value)
    End Sub

End Class