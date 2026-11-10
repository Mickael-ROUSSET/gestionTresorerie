Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersBidirectionalUpdatesIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub CorrigerSuccessivementDeuxHomonymesConserveLeursCoordonnees()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim sharedName As String = "TEST-DEUX-CORRECTIONS-" & Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim firstId As Integer = store.CreateTiers(sharedName, "premier@example.test", "Allègre")
        Dim secondId As Integer = store.CreateTiers(sharedName, "second@example.test", "Monlet")
        Assert.AreNotEqual(firstId, secondId)

        store.UpdateTiers(firstId, sharedName, "premier-corrige@example.test", "Céaux-d'Allègre")
        store.UpdateTiers(secondId, sharedName, "second-corrige@example.test", "Vernassal")

        Using firstRead = New SqlTiersTestStore().ReadTiers(firstId),
              secondRead = New SqlTiersTestStore().ReadTiers(secondId)
            Assert.AreEqual(1, firstRead.Rows.Count)
            Assert.AreEqual(1, secondRead.Rows.Count)
            Assert.AreEqual(firstId, Convert.ToInt32(firstRead.Rows(0)("Id")))
            Assert.AreEqual(secondId, Convert.ToInt32(secondRead.Rows(0)("Id")))
            Assert.AreEqual(sharedName, CStr(firstRead.Rows(0)("nom")))
            Assert.AreEqual(sharedName, CStr(secondRead.Rows(0)("nom")))
            Assert.AreEqual("premier-corrige@example.test", CStr(firstRead.Rows(0)("Email")))
            Assert.AreEqual("Céaux-d'Allègre", CStr(firstRead.Rows(0)("Ville")))
            Assert.AreEqual("second-corrige@example.test", CStr(secondRead.Rows(0)("Email")))
            Assert.AreEqual("Vernassal", CStr(secondRead.Rows(0)("Ville")))
        End Using

        Using results = New SqlTiersTestStore().SearchTiers(sharedName)
            Assert.AreEqual(2, results.Rows.Count)
            Dim foundFirst As Boolean = False
            Dim foundSecond As Boolean = False
            For Each record As System.Data.DataRow In results.Rows
                Dim id As Integer = Convert.ToInt32(record("Id"))
                If id = firstId Then foundFirst = True
                If id = secondId Then foundSecond = True
            Next
            Assert.IsTrue(foundFirst, "Le premier homonyme doit rester recherchable.")
            Assert.IsTrue(foundSecond, "Le second homonyme doit rester recherchable.")
        End Using
    End Sub
End Class
