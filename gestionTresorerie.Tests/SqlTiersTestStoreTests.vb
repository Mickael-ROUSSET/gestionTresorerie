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
        Dim tooLongEmail = New String("a"c, 145) & "@test.example"
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.CreateTiers("Fictif", tooLongEmail, "Allègre"))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.UpdateTiers(1, "Fictif", tooLongEmail, "Allègre"))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() store.ReadTiers(0))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() store.ReadTiers(-1))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() store.UpdateTiers(-1, "Fictif", "a@example.test", "Allègre"))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.SearchTiers(" "))
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() store.UpdateTiers(0, "Fictif", "a@example.test", "Allègre"))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.UpdateTiers(1, "", "a@example.test", "Allègre"))
    End Sub

    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub DeuxTiersFictifsConserventLeursCoordonneesSeparees()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If
        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix = Guid.NewGuid().ToString("N").Substring(0, 12)
        ' Une création rejetée ne doit laisser aucun tiers fantôme dans LocalDB.
        Dim rejectedName = "TEST-REJETE-" & suffix
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.CreateTiers(rejectedName, "email-invalide", "Allègre"))
        Using rejected = New SqlTiersTestStore().SearchTiers(rejectedName)
            Assert.AreEqual(0, rejected.Rows.Count)
        End Using
        Dim first = store.CreateTiers("TEST-A-" & suffix, "a@example.test", "Allègre")
        Dim second = store.CreateTiers("TEST-B-" & suffix, "b@example.test", "Monlet")
        Assert.AreNotEqual(first, second)
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.UpdateTiers(first, "   ", "invalid@example.test", "Inconnue"))
        Using unchanged = New SqlTiersTestStore().ReadTiers(first)
            Assert.AreEqual(1, unchanged.Rows.Count)
            Assert.AreEqual("TEST-A-" & suffix, CStr(unchanged.Rows(0)("nom")))
            Assert.AreEqual("a@example.test", CStr(unchanged.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(unchanged.Rows(0)("Ville")))
        End Using
        Assert.ThrowsExactly(Of ArgumentException)(Sub() store.UpdateTiers(second, "TEST-B-INVALIDE-" & suffix, "adresse-invalide", "Inconnue"))
        Using unchangedEmail = New SqlTiersTestStore().ReadTiers(second)
            Assert.AreEqual(1, unchangedEmail.Rows.Count)
            Assert.AreEqual("TEST-B-" & suffix, CStr(unchangedEmail.Rows(0)("nom")))
            Assert.AreEqual("b@example.test", CStr(unchangedEmail.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(unchangedEmail.Rows(0)("Ville")))
        End Using
        store.UpdateTiers(first, "TEST-A-CORRIGE-" & suffix, "corrige@example.test", "Céaux")
        Assert.ThrowsExactly(Of SqlException)(Sub() store.UpdateTiers(Integer.MaxValue, "Absent", "a@example.test", "Allègre"))
        ' Après une écriture refusée, rouvrir le dépôt et vérifier que les deux tiers
        ' fictifs sont intacts, puis qu'une correction ultérieure reste possible.
        Using unchangedA = New SqlTiersTestStore().ReadTiers(first),
              unchangedB = New SqlTiersTestStore().ReadTiers(second)
            Assert.AreEqual(1, unchangedA.Rows.Count)
            Assert.AreEqual(1, unchangedB.Rows.Count)
            Assert.AreEqual("TEST-A-CORRIGE-" & suffix, CStr(unchangedA.Rows(0)("nom")))
            Assert.AreEqual("corrige@example.test", CStr(unchangedA.Rows(0)("Email")))
            Assert.AreEqual("b@example.test", CStr(unchangedB.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(unchangedB.Rows(0)("Ville")))
        End Using
        store.UpdateTiers(second, "TEST-B-REPRIS-" & suffix, "repris@example.test", "Vernassal")
        Using resumed = New SqlTiersTestStore().ReadTiers(second)
            Assert.AreEqual("TEST-B-REPRIS-" & suffix, CStr(resumed.Rows(0)("nom")))
            Assert.AreEqual("repris@example.test", CStr(resumed.Rows(0)("Email")))
            Assert.AreEqual("Vernassal", CStr(resumed.Rows(0)("Ville")))
        End Using
        Using corrected = New SqlTiersTestStore().ReadTiers(first)
            Assert.AreEqual("TEST-A-CORRIGE-" & suffix, CStr(corrected.Rows(0)("nom")))
            Assert.AreEqual("corrige@example.test", CStr(corrected.Rows(0)("Email")))
            Assert.AreEqual("Céaux", CStr(corrected.Rows(0)("Ville")))
        End Using
        Using a = New SqlTiersTestStore().ReadTiers(first), b = New SqlTiersTestStore().ReadTiers(second)
            Assert.AreEqual(1, a.Rows.Count)
            Assert.AreEqual(1, b.Rows.Count)
            Assert.AreEqual("corrige@example.test", CStr(a.Rows(0)("Email")))
            Assert.AreEqual("Céaux", CStr(a.Rows(0)("Ville")))
            Assert.AreEqual("repris@example.test", CStr(b.Rows(0)("Email")))
            Assert.AreEqual("Vernassal", CStr(b.Rows(0)("Ville")))
        End Using
        Using matches = New SqlTiersTestStore().SearchTiers(suffix)
            Assert.AreEqual(2, matches.Rows.Count)
        End Using
        Dim special = store.CreateTiers("TEST-[A]_%" & suffix, "special@example.test", "Allègre")
        Using exactBracket = New SqlTiersTestStore().SearchTiers("[A]_%")
            Assert.AreEqual(1, exactBracket.Rows.Count)
            Assert.AreEqual(special, CInt(exactBracket.Rows(0)("Id")))
        End Using
        Using literalUnderscore = New SqlTiersTestStore().SearchTiers("_%" & suffix)
            Assert.AreEqual(1, literalUnderscore.Rows.Count)
            Assert.AreEqual(special, CInt(literalUnderscore.Rows(0)("Id")))
        End Using
        Using literalWildcard = New SqlTiersTestStore().SearchTiers("TEST-%")
            Assert.AreEqual(0, literalWildcard.Rows.Count)
        End Using
    End Sub
End Class
