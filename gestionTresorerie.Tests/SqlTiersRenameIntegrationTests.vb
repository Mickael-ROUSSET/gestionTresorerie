Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersRenameIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub RenommerUnTiersMetAJourLaRechercheSansModifierSonIdentifiant()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim ancienNom As String = "TEST-ANCIEN-NOM-" & suffix
        Dim nouveauNom As String = "TEST-NOUVEAU-NOM-" & suffix
        Dim id As Integer = store.CreateTiers(ancienNom, "contact@example.test", "Allègre")

        store.UpdateTiers(id, nouveauNom, "contact@example.test", "Monlet")

        Using fiche = New SqlTiersTestStore().ReadTiers(id)
            Assert.AreEqual(1, fiche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(fiche.Rows(0)("Id")))
            Assert.AreEqual(nouveauNom, CStr(fiche.Rows(0)("nom")))
            Assert.AreEqual("contact@example.test", CStr(fiche.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(fiche.Rows(0)("Ville")))
        End Using

        Using ancienneRecherche = New SqlTiersTestStore().SearchTiers(ancienNom),
              nouvelleRecherche = New SqlTiersTestStore().SearchTiers(nouveauNom)
            Assert.AreEqual(0, ancienneRecherche.Rows.Count, "L'ancien nom ne doit plus trouver le tiers renommé.")
            Assert.AreEqual(1, nouvelleRecherche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(nouvelleRecherche.Rows(0)("Id")))
        End Using
    End Sub
End Class
