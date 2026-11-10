Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersHomonymesIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub DeuxHomonymesRestentDistinctsApresCorrectionEtReouverture()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim sharedName = "TEST-HOMONYMES-" & suffix
        Dim first = store.CreateTiers(sharedName, "premier@example.test", "Allègre")
        Dim second = store.CreateTiers(sharedName, "second@example.test", "Monlet")
        Assert.AreNotEqual(first, second)

        Using initial = New SqlTiersTestStore().SearchTiers(sharedName)
            Assert.AreEqual(2, initial.Rows.Count)
        End Using

        store.UpdateTiers(second, sharedName, "second-corrige@example.test", "Céaux-d'Allègre")

        Using reopenedFirst = New SqlTiersTestStore().ReadTiers(first),
              reopenedSecond = New SqlTiersTestStore().ReadTiers(second)
            Assert.AreEqual(1, reopenedFirst.Rows.Count)
            Assert.AreEqual(1, reopenedSecond.Rows.Count)
            Assert.AreEqual(first, Convert.ToInt32(reopenedFirst.Rows(0)("Id")))
            Assert.AreEqual(second, Convert.ToInt32(reopenedSecond.Rows(0)("Id")))
            Assert.AreEqual(sharedName, CStr(reopenedFirst.Rows(0)("nom")))
            Assert.AreEqual(sharedName, CStr(reopenedSecond.Rows(0)("nom")))
            Assert.AreEqual("premier@example.test", CStr(reopenedFirst.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(reopenedFirst.Rows(0)("Ville")))
            Assert.AreEqual("second-corrige@example.test", CStr(reopenedSecond.Rows(0)("Email")))
            Assert.AreEqual("Céaux-d'Allègre", CStr(reopenedSecond.Rows(0)("Ville")))
        End Using

        Using foundAgain = New SqlTiersTestStore().SearchTiers(sharedName)
            Assert.AreEqual(2, foundAgain.Rows.Count)
        End Using
    End Sub
End Class
