Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersInterleavedUpdatesIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub DeuxSessionsModifientDesTiersDistinctsSansMelangerLeursCoordonnees()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim setup As New SqlTiersTestStore()
        setup.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim nomA As String = "TEST-SESSION-A-" & suffix
        Dim nomB As String = "TEST-SESSION-B-" & suffix
        Dim idA As Integer = setup.CreateTiers(nomA, "a-initial@example.test", "Allègre")
        Dim idB As Integer = setup.CreateTiers(nomB, "b-initial@example.test", "Monlet")

        Dim sessionA As New SqlTiersTestStore()
        Dim sessionB As New SqlTiersTestStore()
        sessionA.UpdateTiers(idA, nomA, "a-final@example.test", "Céaux")
        sessionB.UpdateTiers(idB, nomB, "b-final@example.test", "Vernassal")
        sessionA.UpdateTiers(idA, nomA, "a-corrige@example.test", "Allègre")

        Using ficheA = New SqlTiersTestStore().ReadTiers(idA),
              ficheB = New SqlTiersTestStore().ReadTiers(idB),
              rechercheA = New SqlTiersTestStore().SearchTiers(nomA),
              rechercheB = New SqlTiersTestStore().SearchTiers(nomB)
            Assert.AreEqual(1, ficheA.Rows.Count)
            Assert.AreEqual(1, ficheB.Rows.Count)
            Assert.AreEqual(idA, Convert.ToInt32(ficheA.Rows(0)("Id")))
            Assert.AreEqual(idB, Convert.ToInt32(ficheB.Rows(0)("Id")))
            Assert.AreEqual(nomA, CStr(ficheA.Rows(0)("nom")))
            Assert.AreEqual(nomB, CStr(ficheB.Rows(0)("nom")))
            Assert.AreEqual("a-corrige@example.test", CStr(ficheA.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(ficheA.Rows(0)("Ville")))
            Assert.AreEqual("b-final@example.test", CStr(ficheB.Rows(0)("Email")))
            Assert.AreEqual("Vernassal", CStr(ficheB.Rows(0)("Ville")))
            Assert.AreEqual(1, rechercheA.Rows.Count)
            Assert.AreEqual(1, rechercheB.Rows.Count)
            Assert.AreEqual(idA, Convert.ToInt32(rechercheA.Rows(0)("Id")))
            Assert.AreEqual(idB, Convert.ToInt32(rechercheB.Rows(0)("Id")))
        End Using
    End Sub
End Class
