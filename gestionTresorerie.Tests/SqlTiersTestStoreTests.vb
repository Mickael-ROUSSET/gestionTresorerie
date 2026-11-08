Option Strict On

Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersTestStoreTests
    <TestMethod>
    Public Sub RefuseUneSaisieInvalideAvantToutAccesSql()
        Dim store As New SqlTiersTestStore()
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.CreateTiers("", "a@example.test", "Allègre"))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.CreateTiers("Fictif", "invalide", "Allègre"))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.CreateTiers(New String("x"c, 51), "", "Allègre"))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() store.ReadTiers(0))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.SearchTiers(" "))
    End Sub

    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub DeuxTiersFictifsConserventLeursCoordonneesSeparees()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If
        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim first = store.CreateTiers("TEST-A-" & suffix, "a@example.test", "Allègre")
        Dim second = store.CreateTiers("TEST-B-" & suffix, "b@example.test", "Monlet")
        Assert.AreNotEqual(first, second)
        Using a = New SqlTiersTestStore().ReadTiers(first), b = New SqlTiersTestStore().ReadTiers(second)
            Assert.AreEqual(1, a.Rows.Count)
            Assert.AreEqual(1, b.Rows.Count)
            Assert.AreEqual("a@example.test", CStr(a.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(a.Rows(0)("Ville")))
            Assert.AreEqual("b@example.test", CStr(b.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(b.Rows(0)("Ville")))
        End Using
        Using matches = New SqlTiersTestStore().SearchTiers(suffix)
            Assert.AreEqual(2, matches.Rows.Count)
        End Using
        Using literalWildcard = New SqlTiersTestStore().SearchTiers("TEST-%")
            Assert.AreEqual(0, literalWildcard.Rows.Count)
        End Using
    End Sub
End Class
