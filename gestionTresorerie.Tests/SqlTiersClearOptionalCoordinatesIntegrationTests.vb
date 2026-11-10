Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersClearOptionalCoordinatesIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub EffacerCoordonneesFacultativesNeModifiePasUnAutreTiers()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim firstName As String = "TEST-VIDE-A-" & suffix
        Dim secondName As String = "TEST-VIDE-B-" & suffix
        Dim firstId As Integer = store.CreateTiers(firstName, "premier@example.test", "Allègre")
        Dim secondId As Integer = store.CreateTiers(secondName, "second@example.test", "Monlet")

        store.UpdateTiers(firstId, firstName, "", "  ")

        Using first = New SqlTiersTestStore().ReadTiers(firstId),
              second = New SqlTiersTestStore().ReadTiers(secondId)
            Assert.AreEqual(1, first.Rows.Count)
            Assert.AreEqual(firstId, Convert.ToInt32(first.Rows(0)("Id")))
            Assert.AreEqual(firstName, CStr(first.Rows(0)("nom")))
            Assert.IsTrue(Convert.IsDBNull(first.Rows(0)("Email")), "Email effacé doit être NULL.")
            Assert.IsTrue(Convert.IsDBNull(first.Rows(0)("Ville")), "Ville effacée doit être NULL.")
            Assert.AreEqual(1, second.Rows.Count)
            Assert.AreEqual(secondName, CStr(second.Rows(0)("nom")))
            Assert.AreEqual("second@example.test", CStr(second.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(second.Rows(0)("Ville")))
        End Using

        store.UpdateTiers(firstId, firstName, "retabli@example.test", "Céaux")
        Using restored = New SqlTiersTestStore().ReadTiers(firstId)
            Assert.AreEqual("retabli@example.test", CStr(restored.Rows(0)("Email")))
            Assert.AreEqual("Céaux", CStr(restored.Rows(0)("Ville")))
        End Using
    End Sub
End Class
