Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AffectationReleveServiceTests

    <TestMethod>
    Public Sub AppliquerAffectation_AffecteCategorieEtTiers_ParMotCle()
        Dim mouvement As New Mouvements With {
            .Note = "GIE KLESIA COTIS RETRAITE",
            .Sens = False
        }

        Dim regles As New List(Of RegleAffectationReleve) From {
            New RegleAffectationReleve With {
                .MotCle = "KLESIA",
                .IdTiers = 25,
                .IdCategorie = 3,
                .IdSousCategorie = 9,
                .Sens = False,
                .Priorite = 10
            }
        }

        AffectationReleveService.AppliquerAffectation(mouvement, regles)

        Assert.AreEqual(25, mouvement.Tiers)
        Assert.AreEqual(3, mouvement.Categorie)
        Assert.AreEqual(9, mouvement.SousCategorie)
    End Sub

End Class