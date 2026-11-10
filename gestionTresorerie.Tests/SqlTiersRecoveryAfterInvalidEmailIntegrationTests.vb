Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersRecoveryAfterInvalidEmailIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub EmailInvalidePuisCorrectionValideDansLaMemeSessionPreserveLesTiers()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim nom As String = "TEST-EMAIL-REPRISE-" & suffix
        Dim temoinNom As String = "TEST-EMAIL-TEMOIN-" & suffix
        Dim id As Integer = store.CreateTiers(nom, "initial@example.test", "Allègre")
        Dim temoinId As Integer = store.CreateTiers(temoinNom, "temoin@example.test", "Monlet")

        Dim session As New SqlTiersTestStore()
        Dim rejet As Boolean = False
        Try
            session.UpdateTiers(id, nom, New String("x"c, 151), "Ville refusée")
        Catch ex As ArgumentException
            rejet = True
        End Try
        Assert.IsTrue(rejet, "Un email dépassant 150 caractères doit être refusé.")

        Using apresRejet = New SqlTiersTestStore().ReadTiers(id)
            Assert.AreEqual("initial@example.test", CStr(apresRejet.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(apresRejet.Rows(0)("Ville")))
        End Using

        ' La session qui a rejeté la saisie doit rester réutilisable.
        session.UpdateTiers(id, nom, "corrige@example.test", "Céaux")

        Using fiche = New SqlTiersTestStore().ReadTiers(id),
              temoin = New SqlTiersTestStore().ReadTiers(temoinId),
              recherche = New SqlTiersTestStore().SearchTiers(nom)
            Assert.AreEqual(1, fiche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(fiche.Rows(0)("Id")))
            Assert.AreEqual(nom, CStr(fiche.Rows(0)("nom")))
            Assert.AreEqual("corrige@example.test", CStr(fiche.Rows(0)("Email")))
            Assert.AreEqual("Céaux", CStr(fiche.Rows(0)("Ville")))
            Assert.AreEqual(1, recherche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(recherche.Rows(0)("Id")))
            Assert.AreEqual(1, temoin.Rows.Count)
            Assert.AreEqual(temoinId, Convert.ToInt32(temoin.Rows(0)("Id")))
            Assert.AreEqual(temoinNom, CStr(temoin.Rows(0)("nom")))
            Assert.AreEqual("temoin@example.test", CStr(temoin.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(temoin.Rows(0)("Ville")))
        End Using
    End Sub
End Class
