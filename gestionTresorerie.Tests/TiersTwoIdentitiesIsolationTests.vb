Option Strict On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersTwoIdentitiesIsolationTests

    <TestMethod>
    Public Sub DeuxInsertionsFictivesNeReutilisentPasLesCoordonneesIdentitairesDuPremierTiers()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 101}
        Dim repo As New TiersRepository(fake)

        Dim premierId = repo.Inserer(" Association Alpha ", " Alice ", " Alpha ", 1, 2)
        Assert.AreEqual(101, premierId)
        Assert.AreEqual("insertTiers", fake.LastQueryName)
        Dim premier = fake.LastParameters.ToDictionary(Function(p) p.ParameterName, Function(p) p.Value)
        Assert.AreEqual("Association Alpha", premier("@raisonSociale"))
        Assert.AreEqual("Alice", premier("@prenom"))
        Assert.AreEqual("Alpha", premier("@nom"))
        Assert.AreEqual(1, premier("@categorieDefaut"))
        Assert.AreEqual(2, premier("@sousCategorieDefaut"))

        fake.ScalarResult = 202
        Dim secondId = repo.Inserer(" Association Beta ", Nothing, " Beta ", Nothing, Nothing)
        Assert.AreEqual(202, secondId)
        Assert.AreEqual("insertTiers", fake.LastQueryName)
        Dim second = fake.LastParameters.ToDictionary(Function(p) p.ParameterName, Function(p) p.Value)
        Assert.AreEqual("Association Beta", second("@raisonSociale"))
        Assert.AreEqual(DBNull.Value, second("@prenom"))
        Assert.AreEqual("Beta", second("@nom"))
        Assert.AreEqual(DBNull.Value, second("@categorieDefaut"))
        Assert.AreEqual(DBNull.Value, second("@sousCategorieDefaut"))

        Assert.AreEqual("Association Alpha", premier("@raisonSociale"), "La seconde insertion ne doit pas altérer les paramètres capturés pour la première.")
        Assert.AreNotEqual(premierId, secondId)
    End Sub
End Class
