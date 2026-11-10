Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersRejectedUpdateIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub ModificationInvalideNeChangePasLesCoordonneesPersistantes()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim name As String = "TEST-REJET-MAJ-" & Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim firstId As Integer = store.CreateTiers(name, "original@example.test", "Allègre")
        Dim secondId As Integer = store.CreateTiers(name, "temoin@example.test", "Monlet")

        Dim rejected As Boolean = False
        Try
            store.UpdateTiers(firstId, name, New String("a"c, 151) & "@example.test", "Ville modifiée")
        Catch ex As ArgumentException
            rejected = True
        End Try
        Assert.IsTrue(rejected, "Une adresse email trop longue doit être refusée.")

        Using firstRead = New SqlTiersTestStore().ReadTiers(firstId),
              secondRead = New SqlTiersTestStore().ReadTiers(secondId)
            Assert.AreEqual(1, firstRead.Rows.Count)
            Assert.AreEqual(1, secondRead.Rows.Count)
            Assert.AreEqual("original@example.test", CStr(firstRead.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(firstRead.Rows(0)("Ville")))
            Assert.AreEqual("temoin@example.test", CStr(secondRead.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(secondRead.Rows(0)("Ville")))
        End Using
    End Sub
End Class
