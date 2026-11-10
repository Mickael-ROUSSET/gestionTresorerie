Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersRenameAndClearIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub RenommageEtEffacementSuccessifsRestentPersistantsApresReouverture()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim oldName As String = "TEST-RENOM-ANCIEN-" & suffix
        Dim newName As String = "TEST-RENOM-NOUVEAU-" & suffix
        Dim otherName As String = "TEST-RENOM-TEMOIN-" & suffix
        Dim id As Integer = store.CreateTiers(oldName, "initial@example.test", "Allègre")
        Dim otherId As Integer = store.CreateTiers(otherName, "temoin@example.test", "Monlet")

        ' Deux opérations successives depuis des instances distinctes simulent
        ' la fermeture et la réouverture de la fiche entre les corrections.
        Dim renameStore As New SqlTiersTestStore()
        renameStore.UpdateTiers(id, newName, "nouveau@example.test", "Céaux")
        Dim clearStore As New SqlTiersTestStore()
        clearStore.UpdateTiers(id, newName, "", " ")

        Using fiche = New SqlTiersTestStore().ReadTiers(id),
              temoin = New SqlTiersTestStore().ReadTiers(otherId),
              oldSearch = New SqlTiersTestStore().SearchTiers(oldName),
              newSearch = New SqlTiersTestStore().SearchTiers(newName)
            Assert.AreEqual(1, fiche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(fiche.Rows(0)("Id")))
            Assert.AreEqual(newName, CStr(fiche.Rows(0)("nom")))
            Assert.IsTrue(Convert.IsDBNull(fiche.Rows(0)("Email")))
            Assert.IsTrue(Convert.IsDBNull(fiche.Rows(0)("Ville")))
            Assert.AreEqual(0, oldSearch.Rows.Count)
            Assert.AreEqual(1, newSearch.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(newSearch.Rows(0)("Id")))
            Assert.AreEqual(1, temoin.Rows.Count)
            Assert.AreEqual("temoin@example.test", CStr(temoin.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(temoin.Rows(0)("Ville")))
        End Using
    End Sub
End Class
