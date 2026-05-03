Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ReleveToMouvementMapperTests

    <TestMethod>
    Public Sub Mapper_Debit_CreeMouvementSensFalse()
        Dim ligne As New LigneReleve With {
            .DateOperation = New DateTime(2025, 2, 18),
            .Libelle = "PRELEVEMENT URSSAF",
            .MontantDebit = 136D
        }

        Dim mvt As Mouvements = ReleveToMouvementMapper.Mapper(ligne)

        Assert.AreEqual(New DateTime(2025, 2, 18), mvt.DateMvt)
        Assert.AreEqual("PRELEVEMENT URSSAF", mvt.Note)
        Assert.AreEqual(136D, mvt.Montant)
        Assert.IsFalse(mvt.Sens)
        Assert.AreEqual("RELEVE", mvt.TypeMouvement)
    End Sub

    <TestMethod>
    Public Sub Mapper_Credit_CreeMouvementSensTrue()
        Dim ligne As New LigneReleve With {
            .DateOperation = New DateTime(2025, 2, 17),
            .Libelle = "VIREMENT",
            .MontantCredit = 200D
        }

        Dim mvt As Mouvements = ReleveToMouvementMapper.Mapper(ligne)

        Assert.AreEqual(200D, mvt.Montant)
        Assert.IsTrue(mvt.Sens)
    End Sub

    <TestMethod>
    Public Sub Mapper_SansMontant_LeveException()
        Dim ligne As New LigneReleve With {
            .DateOperation = Date.Today,
            .Libelle = "LIGNE SANS MONTANT"
        }

        Try
            ReleveToMouvementMapper.Mapper(ligne)
            Assert.Fail("Une exception aurait dû être levée.")
        Catch ex As InvalidOperationException
            ' OK
        End Try
    End Sub

End Class