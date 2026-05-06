Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ImportReleveServiceTests

    <TestMethod>
    Public Sub ConstruireDepuisLignes_CreeLignesImportEtMouvements()
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
            ImportReleveService.ConstruireDepuisLignes(lignes, verifierDoublons:=False)

        Assert.AreEqual(2, result.NbLignes)
        Assert.AreEqual(2, result.NbMouvements)
        Assert.AreEqual(0, result.NbDoublons)

        Assert.AreEqual("CHEQUE EMIS", result.LignesImport(0).Mouvement.Note)
        Assert.AreEqual(67.43D, result.LignesImport(0).Mouvement.Montant)
        Assert.IsFalse(result.LignesImport(0).Mouvement.Sens)

        Assert.AreEqual("VERSEMENT", result.LignesImport(1).Mouvement.Note)
        Assert.AreEqual(95.9D, result.LignesImport(1).Mouvement.Montant)
        Assert.IsTrue(result.LignesImport(1).Mouvement.Sens)
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