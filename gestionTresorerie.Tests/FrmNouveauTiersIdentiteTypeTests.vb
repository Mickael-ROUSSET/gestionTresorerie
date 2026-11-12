Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports gestionTresorerie

<TestClass>
Public Class FrmNouveauTiersIdentiteTypeTests
    <TestMethod>
    Public Sub PersonnePhysiqueExigeNomMemeSiRaisonSocialePresente()
        Assert.IsFalse(FrmNouveauTiers.IdentiteValidePourType(False, "  ", "Société fictive"))
        Assert.IsTrue(FrmNouveauTiers.IdentiteValidePourType(False, "Martin", ""))
    End Sub

    <TestMethod>
    Public Sub PersonneMoraleExigeRaisonSocialeMemeSiNomPresent()
        Assert.IsFalse(FrmNouveauTiers.IdentiteValidePourType(True, "Martin", vbTab))
        Assert.IsTrue(FrmNouveauTiers.IdentiteValidePourType(True, "", "Entreprise fictive"))
    End Sub
End Class
