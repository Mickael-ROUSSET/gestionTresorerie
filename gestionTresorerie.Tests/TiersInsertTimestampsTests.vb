Option Strict On

Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TiersInsertTimestampsTests

    <TestMethod>
    Public Sub InsertionUtiliseLeMemeHorodatagePourCreationEtModification()
        Dim fake As New FakeSqlExecutor With {.ScalarResult = 42}
        Dim repo As New TiersRepository(fake)
        Dim avant As DateTime = DateTime.Now

        Dim id As Integer = repo.Inserer(Nothing, "Alice", "Exemple", Nothing, Nothing)

        Dim apres As DateTime = DateTime.Now
        Assert.AreEqual(42, id)
        Assert.AreEqual("insertTiers", fake.LastQueryName)
        Dim parameters = fake.LastParameters.ToList()
        Dim creation = DirectCast(parameters.Single(Function(p) p.ParameterName = "@dateCreation").Value, DateTime)
        Dim modification = DirectCast(parameters.Single(Function(p) p.ParameterName = "@dateModification").Value, DateTime)
        Assert.AreEqual(creation, modification)
        Assert.IsTrue(creation >= avant AndAlso creation <= apres)
    End Sub

End Class
