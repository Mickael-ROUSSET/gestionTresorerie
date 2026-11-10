Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersUnicodeIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub UnTiersAvecAccentsResteRecherchableApresRenommageEtReouverture()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim suffix As String = Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim ancienNom As String = "TEST-ETABLISSEMENT-" & suffix
        Dim nouveauNom As String = "TEST-Établissement Céaux-" & suffix
        Dim depot As New SqlTiersTestStore()
        depot.Initialize()
        Dim id As Integer = depot.CreateTiers(ancienNom, "contact@example.test", "Monlet")

        Dim modification As New SqlTiersTestStore()
        modification.UpdateTiers(id, nouveauNom, "contact@example.test", "Céaux-d'Allègre")

        Using fiche = New SqlTiersTestStore().ReadTiers(id),
              recherche = New SqlTiersTestStore().SearchTiers(nouveauNom),
              ancienneRecherche = New SqlTiersTestStore().SearchTiers(ancienNom)
            Assert.AreEqual(1, fiche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(fiche.Rows(0)("Id")))
            Assert.AreEqual(nouveauNom, CStr(fiche.Rows(0)("nom")))
            Assert.AreEqual("contact@example.test", CStr(fiche.Rows(0)("Email")))
            Assert.AreEqual("Céaux-d'Allègre", CStr(fiche.Rows(0)("Ville")))
            Assert.AreEqual(1, recherche.Rows.Count)
            Assert.AreEqual(id, Convert.ToInt32(recherche.Rows(0)("Id")))
            Assert.AreEqual(0, ancienneRecherche.Rows.Count)
        End Using
    End Sub
End Class
