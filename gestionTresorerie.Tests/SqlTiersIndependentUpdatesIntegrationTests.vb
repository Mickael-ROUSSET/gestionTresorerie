Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersIndependentUpdatesIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub CorrigerUnTiersNeModifiePasLesAutresApresReouverture()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim firstName As String = "TEST-ASSOCIATION-A-" & suffix
        Dim secondName As String = "TEST-ASSOCIATION-B-" & suffix
        Dim firstId As Integer = store.CreateTiers(firstName, "a-initial@example.test", "Allègre")
        Dim secondId As Integer = store.CreateTiers(secondName, "b-initial@example.test", "Monlet")
        Assert.AreNotEqual(firstId, secondId)

        store.UpdateTiers(firstId, firstName, "a-corrige@example.test", "Céaux-d'Allègre")

        Using firstRead = New SqlTiersTestStore().ReadTiers(firstId),
              secondRead = New SqlTiersTestStore().ReadTiers(secondId)
            Assert.AreEqual(1, firstRead.Rows.Count)
            Assert.AreEqual(1, secondRead.Rows.Count)
            Assert.AreEqual(firstId, Convert.ToInt32(firstRead.Rows(0)("Id")))
            Assert.AreEqual(secondId, Convert.ToInt32(secondRead.Rows(0)("Id")))
            Assert.AreEqual(firstName, CStr(firstRead.Rows(0)("nom")))
            Assert.AreEqual(secondName, CStr(secondRead.Rows(0)("nom")))
            Assert.AreEqual("a-corrige@example.test", CStr(firstRead.Rows(0)("Email")))
            Assert.AreEqual("Céaux-d'Allègre", CStr(firstRead.Rows(0)("Ville")))
            Assert.AreEqual("b-initial@example.test", CStr(secondRead.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(secondRead.Rows(0)("Ville")))
        End Using

        Using firstSearch = New SqlTiersTestStore().SearchTiers(firstName),
              secondSearch = New SqlTiersTestStore().SearchTiers(secondName)
            Assert.AreEqual(1, firstSearch.Rows.Count)
            Assert.AreEqual(1, secondSearch.Rows.Count)
            Assert.AreEqual(firstId, Convert.ToInt32(firstSearch.Rows(0)("Id")))
            Assert.AreEqual(secondId, Convert.ToInt32(secondSearch.Rows(0)("Id")))
        End Using
    End Sub
End Class
