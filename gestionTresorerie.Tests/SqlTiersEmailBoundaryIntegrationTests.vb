Option Strict On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SqlTiersEmailBoundaryIntegrationTests
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub LimiteEmailAccepteePuisDepassementRejeteSansEcriturePartielle()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION") <> "1" Then
            Assert.Inconclusive("LocalDB réservé au runner Windows jetable.")
        End If

        Dim store As New SqlTiersTestStore()
        store.Initialize()
        Dim nom As String = "TEST-LIMITE-EMAIL-" & Guid.NewGuid().ToString("N").Substring(0, 12)
        Dim email150 As String = New String("a"c, 137) & "@example.test"
        Dim email151 As String = New String("b"c, 138) & "@example.test"
        Assert.AreEqual(150, email150.Length)
        Assert.AreEqual(151, email151.Length)

        Dim id As Integer = store.CreateTiers(nom, "initial@example.test", "Allègre")
        store.UpdateTiers(id, nom, email150, "Monlet")

        Using lu = New SqlTiersTestStore().ReadTiers(id)
            Assert.AreEqual(1, lu.Rows.Count)
            Assert.AreEqual(email150, CStr(lu.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(lu.Rows(0)("Ville")))
        End Using

        Assert.ThrowsException(Of ArgumentException)(
            Sub() store.UpdateTiers(id, nom, email151, "Ville interdite"))

        Using lu = New SqlTiersTestStore().ReadTiers(id)
            Assert.AreEqual(1, lu.Rows.Count)
            Assert.AreEqual(email150, CStr(lu.Rows(0)("Email")))
            Assert.AreEqual("Monlet", CStr(lu.Rows(0)("Ville")))
        End Using
    End Sub
End Class
