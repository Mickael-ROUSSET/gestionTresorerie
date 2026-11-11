Option Strict On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersInsertionParametersTests

    <TestMethod>
    Public Sub InsererNormaliseLesChampsEtTransmetLesCategories()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 321}
        Dim repo As New TiersRepository(fake)

        Dim id = repo.Inserer("  Association fictive  ", "  Alice  ", "  Exemple  ", 4, 7)

        Assert.AreEqual(321, id)
        Assert.AreEqual("insertTiers", fake.LastQueryName)
        Dim p = fake.LastParameters.ToDictionary(Function(x) x.ParameterName, Function(x) x.Value)
        Assert.AreEqual(7, p.Count)
        Assert.AreEqual("Association fictive", p("@raisonSociale"))
        Assert.AreEqual("Alice", p("@prenom"))
        Assert.AreEqual("Exemple", p("@nom"))
        Assert.AreEqual(4, p("@categorieDefaut"))
        Assert.AreEqual(7, p("@sousCategorieDefaut"))
        Assert.IsInstanceOfType(p("@dateCreation"), GetType(DateTime))
        Assert.IsInstanceOfType(p("@dateModification"), GetType(DateTime))
    End Sub

    <TestMethod>
    Public Sub InsererConvertitLesChampsVidesEnDbNullEtRefuseUnIdentifiantAbsent()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = DBNull.Value}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(0, repo.Inserer(" ", Nothing, " ", Nothing, Nothing))
        Dim p = fake.LastParameters.ToDictionary(Function(x) x.ParameterName, Function(x) x.Value)
        Assert.AreEqual(DBNull.Value, p("@raisonSociale"))
        Assert.AreEqual(DBNull.Value, p("@prenom"))
        Assert.AreEqual(DBNull.Value, p("@nom"))
        Assert.AreEqual(DBNull.Value, p("@categorieDefaut"))
        Assert.AreEqual(DBNull.Value, p("@sousCategorieDefaut"))
    End Sub

    <TestMethod>
    Public Sub InsererRefuseUnIdentifiantNonNumerique()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = "identifiant-invalide"}
        Dim repo As New TiersRepository(fake)

        Assert.AreEqual(0, repo.Inserer("Tiers fictif", "", "", Nothing, Nothing))
        Assert.AreEqual("insertTiers", fake.LastQueryName)
    End Sub
End Class
