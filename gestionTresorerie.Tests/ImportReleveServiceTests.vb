Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ImportReleveServiceTests

    <TestMethod>
    Public Sub ConstruireDepuisLignes_CreeLignesEtMouvements()
        Dim lignes As New List(Of LigneReleve) From {
            New LigneReleve With {
                .DateOperation = New DateTime(2025, 12, 23),
                .Libelle = "CHEQUE EMIS",
                .MontantDebit = 67.43D
            },
            New LigneReleve With {
                .DateOperation = New DateTime(2025, 12, 20),
                .Libelle = "VERSEMENT",
                .MontantCredit = 95.9D
            }
        }

        Dim result As ImportReleveResult =
            ImportReleveService.ConstruireDepuisLignes(lignes)

        Assert.AreEqual(2, result.NbLignes)
        Assert.AreEqual(2, result.NbMouvements)

        Assert.AreEqual("CHEQUE EMIS", result.Mouvements(0).Note)
        Assert.AreEqual(67.43D, result.Mouvements(0).Montant)
        Assert.IsFalse(result.Mouvements(0).Sens)

        Assert.AreEqual("VERSEMENT", result.Mouvements(1).Note)
        Assert.AreEqual(95.9D, result.Mouvements(1).Montant)
        Assert.IsTrue(result.Mouvements(1).Sens)
    End Sub

    <TestMethod>
    Public Sub ConstruireDepuisLignes_Nothing_LeveException()
        Try
            ImportReleveService.ConstruireDepuisLignes(Nothing)
            Assert.Fail("Une exception était attendue.")
        Catch ex As ArgumentNullException
            ' OK
        End Try
    End Sub

End Class