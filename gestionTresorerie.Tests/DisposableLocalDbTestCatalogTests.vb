Option Strict On

Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DisposableLocalDbTestCatalogTests
    <TestMethod>
    Public Sub NouveauNomEstUniqueEtValide()
        Dim first = DisposableLocalDbTestCatalog.NewName()
        Dim second = DisposableLocalDbTestCatalog.NewName()
        Assert.IsTrue(DisposableLocalDbTestCatalog.IsDisposableName(first))
        Assert.IsTrue(DisposableLocalDbTestCatalog.IsDisposableName(second))
        Assert.AreNotEqual(first, second)
    End Sub

    <TestMethod>
    Public Sub RefuseLesCataloguesPersistantsEtLesNomsForgés()
        For Each name In {"bddAgumaaa", "bddAgumaaaTest", "master", "", "gtDemoTest_",
                          "gtDemoTest_00000000000000000000000000000000",
                          "gtDemoTest_1234] DROP DATABASE bddAgumaaa;--"}
            Assert.IsFalse(DisposableLocalDbTestCatalog.IsDisposableName(name), name)
        Next
    End Sub
End Class
