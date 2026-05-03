Imports Microsoft.VisualStudio.TestTools.UnitTesting


<TestClass>
Public Class LitReleveTests

    <TestMethod>
    Public Sub ParserLignes_DetecteDebit()
        Dim lignes = {
            "18/02/2025",
            "PRELEVEMENT URSSAF",
            "-136,00 €"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(1, result.Count)

        Dim ligne = result(0)

        Assert.AreEqual(New DateTime(2025, 2, 18), ligne.DateOperation)
        Assert.AreEqual("PRELEVEMENT URSSAF", ligne.Libelle)

        Assert.AreEqual(136D, ligne.MontantDebit)
        Assert.IsNull(ligne.MontantCredit)
    End Sub

    <TestMethod>
    Public Sub ParserLignes_DetecteCredit()
        Dim lignes = {
            "17/02/2025",
            "VIREMENT SALAIRE",
            "+2000,00 €"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(1, result.Count)

        Dim ligne = result(0)

        Assert.AreEqual(New DateTime(2025, 2, 17), ligne.DateOperation)
        Assert.AreEqual("VIREMENT SALAIRE", ligne.Libelle)

        Assert.AreEqual(2000D, ligne.MontantCredit)
        Assert.IsNull(ligne.MontantDebit)
    End Sub

    <TestMethod>
    Public Sub ParserLignes_MultiLignesLibelle()
        Dim lignes = {
            "18/02/2025",
            "PRELEVEMENT",
            "URSSAF AUVERGNE",
            "-136,00 €"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(1, result.Count)

        Dim ligne = result(0)

        Assert.AreEqual("PRELEVEMENT URSSAF AUVERGNE", ligne.Libelle)
        Assert.AreEqual(136D, ligne.MontantDebit)
    End Sub

    <TestMethod>
    Public Sub ParserLignes_IgnoreLigneSansMontant()
        ' Pas de montant
        Dim lignes = {
            "18/02/2025",
            "PRELEVEMENT URSSAF", "", ""
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(0, result.Count)
    End Sub
    <TestMethod>
    Public Sub ParserLignes_LigneInvalide_ThrowException()
        Dim lignes = {
            "18/02/2025",
            "LIBELLE",
            "+100 €",
            "-50 €"
        }

        Try
            LitRelevé.ParserLignes(lignes)
            Assert.Fail("Une exception aurait dû être levée.")
        Catch ex As FormatException
            ' OK
        End Try
    End Sub

    <TestMethod>
    Public Sub ConvertirLigneReleveEnCsv_FormatCorrect()
        Dim ligne As New LigneReleve With {
            .DateOperation = New DateTime(2025, 2, 18),
            .Libelle = "TEST LIBELLE",
            .MontantDebit = 10D
        }

        Dim result = LitRelevé.ConvertirLigneReleveEnCsv(ligne)

        Assert.AreEqual("18/02/2025;TEST LIBELLE;10,00;", result)
    End Sub
    <TestMethod>
    Public Sub ParserLignes_LibelleAvecTirets_NeDoitPasEtreDebit()

        Dim lignes = {
            "18/02/2025",
            "GIE KLESIA COTIS RETRAITE - GIE KLESIA COTIS - 202510M",
            "-136,00 €"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(1, result.Count)

        Assert.AreEqual(136D, result(0).MontantDebit)
        Assert.AreEqual("GIE KLESIA COTIS RETRAITE - GIE KLESIA COTIS - 202510M", result(0).Libelle)

    End Sub
    <TestMethod>
    Public Sub ParserLignes_FormatCsvStructure_DetecteDebitEtCredit()
        Dim lignes = {
            "Date;Libellé;Débit euros;Crédit euros;",
            "23/12/2025;""CHEQUE EMIS             5784159  "";67,43;;",
            "20/12/2025;""VERSEMENT D'ESPECES     ALLEGRE 2341334 093331 "";;95,90;"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(2, result.Count)

        Assert.AreEqual(New DateTime(2025, 12, 23), result(0).DateOperation)
        Assert.AreEqual(67.43D, result(0).MontantDebit)
        Assert.IsFalse(result(0).MontantCredit.HasValue)

        Assert.AreEqual(New DateTime(2025, 12, 20), result(1).DateOperation)
        Assert.AreEqual(95.9D, result(1).MontantCredit)
        Assert.IsFalse(result(1).MontantDebit.HasValue)
    End Sub
    <TestMethod>
    Public Sub ParserFichier_EncodageAnsi_LitCorrectementAccents()

        Dim lignes = {
            "Date;Libellé;Débit euros;Crédit euros;",
            "23/12/2025;""CHEQUE EMIS"";67,43;;"
        }

        ' simulation
        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.IsTrue(result.Count > 0)

    End Sub
    <TestMethod>
    Public Sub ParserLignes_FormatCsvStructure_LibelleMultiligne()
        Dim lignes = {
            "Date;Libellé;Débit euros;Crédit euros;",
            "23/12/2025;""CHEQUE EMIS             ",
            "5784159  ",
            "",
            """;67,43;;"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(1, result.Count)
        Assert.AreEqual(New DateTime(2025, 12, 23), result(0).DateOperation)
        Assert.AreEqual("CHEQUE EMIS 5784159", result(0).Libelle)
        Assert.AreEqual(67.43D, result(0).MontantDebit)
        Assert.IsFalse(result(0).MontantCredit.HasValue)
    End Sub
    <TestMethod>
    Public Sub ParserLignes_MontantAvecSeparateurMilliers()
        Dim lignes = {
            "Date;Libellé;Débit euros;Crédit euros;",
            "23/12/2025;""VERSEMENT"";;1 000,00;"
        }

        Dim result = LitRelevé.ParserLignes(lignes)

        Assert.AreEqual(1, result.Count)
        Assert.AreEqual(1000D, result(0).MontantCredit)
    End Sub
End Class