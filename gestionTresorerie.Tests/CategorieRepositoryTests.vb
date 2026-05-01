Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class CategorieRepositoryTests

    <TestMethod>
    Public Sub LibelleParId_RetourneLibelle()
        Dim fake As New FakeSqlExecutor With {
            .ScalarResult = "Cotisation"
        }

        Dim repo As New CategorieRepository(fake)

        Dim result = repo.LibelleParId(4)

        Assert.AreEqual("Cotisation", result)
        Assert.AreEqual("reqLibCat", fake.LastQueryName)
    End Sub

End Class