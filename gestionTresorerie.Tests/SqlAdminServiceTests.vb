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

    <TestMethod>
    Public Sub ConstruireCommandeSauvegarde_EchappeUnNomContenantDuSql()
        Dim sql As String = SqlAdminService.ConstruireCommandeSauvegarde("test]; DROP DATABASE master;--")

        Assert.AreEqual("BACKUP DATABASE [test]]; DROP DATABASE master;--] TO DISK = @backupPath WITH NOINIT", sql)
    End Sub

    <TestMethod>
    Public Sub SauvegarderBase_RefuseNomVideSansConnexion()
        Dim service As New SqlAdminService(New UnusedConnectionFactory())

        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() service.SauvegarderBase(" ", "test.bak"))
    End Sub

    <TestMethod>
    Public Sub Constructeur_RefuseUneFabriqueDeConnexionAbsente()
        Assert.ThrowsExactly(Of ArgumentNullException)(AddressOf CreerServiceSansFabrique)
    End Sub

    Private Sub CreerServiceSansFabrique()
        Dim service As New SqlAdminService(Nothing)
    End Sub

End Class
