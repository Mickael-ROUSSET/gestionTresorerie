Option Strict On

Imports gestionTresorerie
Imports System.Data.SqlClient
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
    Public Sub ConnexionJetableNePointeJamaisSurUnCataloguePersistant()
        Dim name = DisposableLocalDbTestCatalog.NewName()
        Dim connectionString = DisposableLocalDbTestCatalog.ConnectionStringForName(name)
        Dim builder As New SqlConnectionStringBuilder(connectionString)
        Assert.AreEqual(name, builder.InitialCatalog)
        Assert.IsTrue(DisposableLocalDbTestCatalog.IsDisposableName(builder.InitialCatalog))
        For Each forbidden In {"bddAgumaaa", "bddAgumaaaTest", "master", "gtDemoTest_"}
            Assert.ThrowsException(Of ArgumentException)(
                Sub() DisposableLocalDbTestCatalog.ConnectionStringForName(forbidden))
        Next
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
