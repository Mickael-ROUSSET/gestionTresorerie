Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersBlankRenameIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub NomBlancRejeteSansEffacerLesCoordonneesNiModifierLeTemoin()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim nom As String = "TEST-NOM-OBLIGATOIRE-" & suffix
        Dim temoinNom As String = "TEST-NOM-TEMOIN-" & suffix
        Dim id As Integer = store.CreateTiers(nom, "conserve@example.test", "Allègre")
        Dim temoinId As Integer = store.CreateTiers(temoinNom, "temoin@example.test", "Monlet")

        Dim rejetConfirme As Boolean = False
        Try
            ' La demande de vider les coordonnées ne doit pas être appliquée
            ' lorsque le nouveau nom obligatoire est invalide.
            Dim autreSession As New SqlTiersTestStore()
            autreSession.UpdateTiers(id, "   ", "", "")
        Catch ex As ArgumentException
            rejetConfirme = True
        End Try
        Assert.IsTrue(rejetConfirme, "Un nom composé uniquement d'espaces doit être refusé.")

        Using fiche = New SqlTiersTestStore().ReadTiers(id),
              temoin = New SqlTiersTestStore().ReadTiers(temoinId),
              recherche = New SqlTiersTestStore().SearchTiers(nom)
            Assert.AreEqual(1, fiche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(fiche.Rows(0)("Id")))
            Assert.AreEqual(nom, CStr(fiche.Rows(0)("nom")))
            Assert.AreEqual("conserve@example.test", CStr(fiche.Rows(0)("Email")))
            Assert.AreEqual("Allègre", CStr(fiche.Rows(0)("Ville")))
            Assert.AreEqual(1, recherche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(recherche.Rows(0)("Id")))
            Assert.AreEqual(1, temoin.Rows.Count)
            Assert.AreEqual(temoinNom, CStr(temoin.Rows(0)("nom")))
            Assert.AreEqual("temoin@example.test", CStr(temoin.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(temoin.Rows(0)("Ville")))
        End Using
    End Sub
End Class
