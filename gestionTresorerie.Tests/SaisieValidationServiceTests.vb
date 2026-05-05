Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SaisieValidationServiceTests

    <TestMethod>
    Public Sub Valider_MontantVide_RetourneErreurMontant()
        Dim dto As New SaisieMouvementDto With {
            .DateMvt = Date.Today,
            .MontantTexte = ""
        }

        Dim result = SaisieValidationService.Valider(dto, True, True, True, True)

        Assert.IsFalse(result.IsValid)
        Assert.IsTrue(result.Erreurs.ContainsKey("Montant"))
    End Sub

    <TestMethod>
    Public Sub Valider_TiersAbsent_RetourneErreurTiers()
        Dim dto As New SaisieMouvementDto With {
            .DateMvt = Date.Today,
            .MontantTexte = "10,00"
        }

        Dim result = SaisieValidationService.Valider(dto, False, True, True, True)

        Assert.IsFalse(result.IsValid)
        Assert.IsTrue(result.Erreurs.ContainsKey("Tiers"))
    End Sub

    <TestMethod>
    Public Sub Valider_FormulaireValide_RetourneOk()
        Dim dto As New SaisieMouvementDto With {
            .DateMvt = Date.Today,
            .MontantTexte = "10,00"
        }

        Dim result = SaisieValidationService.Valider(dto, True, True, True, True)

        Assert.IsTrue(result.IsValid)
        Assert.AreEqual(0, result.Erreurs.Count)
    End Sub

End Class