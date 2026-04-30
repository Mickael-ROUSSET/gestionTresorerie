Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient

<TestClass>
Public Class UtilisateurRepositoryTests

    <TestMethod>
    Public Sub ModifierMotDePasse_UtiliseLaBonneRequeteEtLesBonsParametres()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New UtilisateurRepository(fake)

        Dim nb = repo.ModifierMotDePasse(12, "HASH_TEST")

        Assert.AreEqual(1, nb)
        Assert.AreEqual("updateMotDePasse", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.AreEqual("@Id", p(0).ParameterName)
        Assert.AreEqual(12, p(0).Value)
        Assert.AreEqual("@pwd", p(1).ParameterName)
        Assert.AreEqual("HASH_TEST", p(1).Value)
    End Sub

End Class