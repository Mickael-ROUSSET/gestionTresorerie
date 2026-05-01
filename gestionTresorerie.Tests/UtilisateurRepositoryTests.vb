Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports System.Linq

<TestClass>
Public Class UtilisateurRepositoryTests

    <TestMethod>
    Public Sub ModifierMotDePasse_UtiliseUpdateMotDePasse_EtParametresAttendus()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New UtilisateurRepository(fake)

        Dim nb As Integer = repo.ModifierMotDePasse(12, "HASH_TEST")

        Assert.AreEqual(1, nb)
        Assert.AreEqual("updateMotDePasse", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()

        Assert.AreEqual(2, p.Count)
        Assert.AreEqual("@Id", p(0).ParameterName)
        Assert.AreEqual(12, p(0).Value)

        Assert.AreEqual("@pwd", p(1).ParameterName)
        Assert.AreEqual("HASH_TEST", p(1).Value)
    End Sub

    <TestMethod>
    Public Sub InsererUtilisateur_UtiliseInsertUtilisateur_EtParametresAttendus()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New UtilisateurRepository(fake)

        Dim nb As Integer = repo.InsererUtilisateur("admin", "HASH", "ADMIN", True)

        Assert.AreEqual(1, nb)
        Assert.AreEqual("insertUtilisateur", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()

        Assert.AreEqual("@nom", p(0).ParameterName)
        Assert.AreEqual("admin", p(0).Value)

        Assert.AreEqual("@pwd", p(1).ParameterName)
        Assert.AreEqual("HASH", p(1).Value)

        Assert.AreEqual("@role", p(2).ParameterName)
        Assert.AreEqual("ADMIN", p(2).Value)

        Assert.AreEqual("@actif", p(3).ParameterName)
        Assert.IsTrue(p(3).Value)
    End Sub

    <TestMethod>
    Public Sub MettreAJourActif_UtiliseUpdateUtilisateurActif_EtParametresAttendus()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New UtilisateurRepository(fake)

        Dim nb As Integer = repo.MettreAJourActif(7, False)

        Assert.AreEqual(1, nb)
        Assert.AreEqual("updateUtilisateurActif", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()

        Assert.AreEqual("@Id", p(0).ParameterName)
        Assert.AreEqual(7, p(0).Value)

        Assert.AreEqual("@actif", p(1).ParameterName)
        Assert.IsTrue(p(3).Value)
    End Sub

End Class