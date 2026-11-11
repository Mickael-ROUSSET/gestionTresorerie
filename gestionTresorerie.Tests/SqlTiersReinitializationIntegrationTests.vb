Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersReinitializationIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub ReinitialiserLeSchemaTestNePerdPasDeuxTiersEtLeursCoordonnees()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim initial As New SqlTiersTestStore()
        initial.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim nomA As String = "TEST-REINIT-A-" & suffix
        Dim nomB As String = "TEST-REINIT-B-" & suffix
        Dim idA As Integer = initial.CreateTiers(nomA, "alpha@example.test", "Allègre")
        Dim idB As Integer = initial.CreateTiers(nomB, "beta@example.test", "Monlet")
        Assert.AreNotEqual(idA, idB)

        ' Simule un nouveau lancement de l'application et la vérification du schéma.
        Dim reopened As New SqlTiersTestStore()
        reopened.Initialize()
        Using a = reopened.ReadTiers(idA), b = reopened.ReadTiers(idB)
            Assert.AreEqual(1, a.Rows.Count)
            Assert.AreEqual(1, b.Rows.Count)
            Assert.AreEqual(nomA, CStr(a.Rows(0)("nom")))
            Assert.AreEqual(nomB, CStr(b.Rows(0)("nom")))
            Assert.AreEqual("alpha@example.test", CStr(a.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(a.Rows(0)("Ville")))
            Assert.AreEqual("beta@example.test", CStr(b.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(b.Rows(0)("Ville")))
        End Using

        reopened.UpdateTiers(idA, nomA, "alpha-corrige@example.test", "Céaux")
        Using a = New SqlTiersTestStore().ReadTiers(idA),
              b = New SqlTiersTestStore().ReadTiers(idB),
              matchesA = New SqlTiersTestStore().SearchTiers(nomA),
              matchesB = New SqlTiersTestStore().SearchTiers(nomB)
            Assert.AreEqual("alpha-corrige@example.test", CStr(a.Rows(0)("Email")))
            Assert.AreEqual("Céaux", CStr(a.Rows(0)("Ville")))
            Assert.AreEqual("beta@example.test", CStr(b.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(b.Rows(0)("Ville")))
            Assert.AreEqual(1, matchesA.Rows.Count)
            Assert.AreEqual(1, matchesB.Rows.Count)
            Assert.AreEqual(idA, Convert.ToInt32(matchesA.Rows(0)("Id")))
            Assert.AreEqual(idB, Convert.ToInt32(matchesB.Rows(0)("Id")))
        End Using
    End Sub
End Class
