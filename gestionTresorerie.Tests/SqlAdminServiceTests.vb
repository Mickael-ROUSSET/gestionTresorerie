Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlAdminServiceTests

    <TestMethod>
    Public Sub ConstruireCommandeSauvegarde_ConserveLesSauvegardesExistantes()
        Dim sql As String = SqlAdminService.ConstruireCommandeSauvegarde("TresorerieTest")

        Assert.AreEqual("BACKUP DATABASE [TresorerieTest] TO DISK = @backupPath WITH NOINIT", sql)
        Assert.IsFalse(sql.Contains("WITH INIT", StringComparison.OrdinalIgnoreCase))
    End Sub

    <TestMethod>
    Public Sub ConstruireCommandeSauvegarde_EchappeLesCrochetsDuNom()
        Dim sql As String = SqlAdminService.ConstruireCommandeSauvegarde("test]nom")

        Assert.AreEqual("BACKUP DATABASE [test]]nom] TO DISK = @backupPath WITH NOINIT", sql)
    End Sub

    <TestMethod>
    Public Sub ConstruireCommandeSauvegarde_RefuseUnNomVide()
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() SqlAdminService.ConstruireCommandeSauvegarde(" "))
    End Sub

    <TestMethod>
    Public Sub SauvegarderBase_RefuseCheminVideSansConnexion()
        Dim service As New SqlAdminService(New UnusedConnectionFactory())

        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() service.SauvegarderBase("TresorerieTest", " "))
    End Sub

    <TestMethod>
    Public Sub RestaurerBase_ResteDesactiveeSansConnexion()
        Dim service As New SqlAdminService(New UnusedConnectionFactory())

        Assert.ThrowsExactly(Of NotSupportedException)(
            Sub() service.RestaurerBase("TresorerieTest", "test.bak"))
    End Sub

End Class
