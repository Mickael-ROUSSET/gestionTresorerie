Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class MouvementBuildServiceTests

    <TestMethod>
    Public Sub ConstruireDepuisSaisie_AppliqueValidationEtMetadonnees()
        Dim dto As New SaisieMouvementDto With {
            .DateMvt = New DateTime(2025, 12, 23),
            .Note = " CHEQUE EMIS ",
            .MontantTexte = "67,43",
            .EstDebit = True,
            .IdCategorie = 1,
            .IdSousCategorie = 2,
            .IdTiers = 3,
            .IdDoc = 4
        }

        Dim options As New MouvementBuildOptions With {
            .TiersSelectionne = True,
            .CategorieSelectionnee = True,
            .SousCategorieSelectionnee = True,
            .TypeMvtSelectionne = True,
            .EvenementLibelle = "Marché de Noël",
            .TypeMvtLibelle = "Banque",
            .Rapproche = True,
            .Modifiable = False,
            .NumeroRemise = "R001",
            .Reference = "5784159",
            .TypeReference = "Chèque"
        }

        Dim mvt = MouvementBuildService.ConstruireDepuisSaisie(dto, options)

        Assert.AreEqual("CHEQUE EMIS", mvt.Note)
        Assert.AreEqual(67.43D, mvt.Montant)
        Assert.IsFalse(mvt.Sens)
        Assert.AreEqual("Marché de Noël", mvt.Evenement)
        Assert.AreEqual("Banque", mvt.TypeMouvement)
        Assert.IsTrue(mvt.Etat)
        Assert.IsFalse(mvt.Modifiable)
        Assert.AreEqual("R001", mvt.NumeroRemise)
        Assert.AreEqual("5784159", mvt.reference)
        Assert.AreEqual("Chèque", mvt.typeReference)
    End Sub

End Class