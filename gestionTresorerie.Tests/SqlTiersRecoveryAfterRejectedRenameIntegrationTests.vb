Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersRecoveryAfterRejectedRenameIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub CorrectionValideApresRenommageRefuseConserveLesDeuxTiers()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim ancienNom As String = "TEST-REPRISE-ANCIEN-" & suffix
        Dim nouveauNom As String = "TEST-REPRISE-NOUVEAU-" & suffix
        Dim temoinNom As String = "TEST-REPRISE-TEMOIN-" & suffix
        Dim id As Integer = store.CreateTiers(ancienNom, "avant@example.test", "Allègre")
        Dim temoinId As Integer = store.CreateTiers(temoinNom, "temoin@example.test", "Monlet")

        Dim session As New SqlTiersTestStore()
        Dim rejet As Boolean = False
        Try
            session.UpdateTiers(id, "   ", "", "")
        Catch ex As ArgumentException
            rejet = True
        End Try
        Assert.IsTrue(rejet, "Le renommage blanc doit être refusé.")

        ' Après le refus, la même session doit pouvoir enregistrer une correction valide.
        session.UpdateTiers(id, nouveauNom, "apres@example.test", "Céaux")

        Using fiche = New SqlTiersTestStore().ReadTiers(id),
              temoin = New SqlTiersTestStore().ReadTiers(temoinId),
              ancienneRecherche = New SqlTiersTestStore().SearchTiers(ancienNom),
              nouvelleRecherche = New SqlTiersTestStore().SearchTiers(nouveauNom)
            Assert.AreEqual(1, fiche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(fiche.Rows(0)("Id")))
            Assert.AreEqual(nouveauNom, CStr(fiche.Rows(0)("nom")))
            Assert.AreEqual("apres@example.test", CStr(fiche.Rows(0)("Email")))
            Assert.AreEqual("Céaux", CStr(fiche.Rows(0)("Ville")))
            Assert.AreEqual(0, ancienneRecherche.Rows.Count)
            Assert.AreEqual(1, nouvelleRecherche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(nouvelleRecherche.Rows(0)("Id")))
            Assert.AreEqual(1, temoin.Rows.Count)
            Assert.AreEqual(temoinId, Convert.ToInt32(temoin.Rows(0)("Id")))
            Assert.AreEqual(temoinNom, CStr(temoin.Rows(0)("nom")))
            Assert.AreEqual("temoin@example.test", CStr(temoin.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(temoin.Rows(0)("Ville")))
        End Using
    End Sub
End Class
