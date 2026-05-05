Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SaisieServiceTests

    <TestMethod>
    Public Sub ParserMontant_AccepteVirguleFrancaise()
        Dim result As Decimal = SaisieService.ParserMontant("123,45")

        Assert.AreEqual(123.45D, result)
    End Sub

    <TestMethod>
    Public Sub ParserMontant_AccepteEuroEtEspaces()
        Dim result As Decimal = SaisieService.ParserMontant("1 234,56 €")

        Assert.AreEqual(1234.56D, result)
    End Sub

    <TestMethod>
    Public Sub ParserMontant_MontantVide_LeveException()
        Try
            SaisieService.ParserMontant("")
            Assert.Fail("Une exception était attendue.")
        Catch ex As ArgumentException
            ' OK
        End Try
    End Sub

    <TestMethod>
    Public Sub ParserMontant_MontantInvalide_LeveException()
        Try
            SaisieService.ParserMontant("abc")
            Assert.Fail("Une exception était attendue.")
        Catch ex As FormatException
            ' OK
        End Try
    End Sub

    <TestMethod>
    Public Sub CreerMouvement_Debit_CreeSensFalse()
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

        Dim mvt As Mouvements = SaisieService.CreerMouvement(dto)

        Assert.AreEqual(New DateTime(2025, 12, 23), mvt.DateMvt)
        Assert.AreEqual("CHEQUE EMIS", mvt.Note)
        Assert.AreEqual(67.43D, mvt.Montant)
        Assert.IsFalse(mvt.Sens)
        Assert.AreEqual(1, mvt.Categorie)
        Assert.AreEqual(2, mvt.SousCategorie)
        Assert.AreEqual(3, mvt.Tiers)
        Assert.AreEqual(4, mvt.idDoc)
    End Sub

    <TestMethod>
    Public Sub CreerMouvement_Credit_CreeSensTrue()
        Dim dto As New SaisieMouvementDto With {
            .DateMvt = New DateTime(2025, 12, 20),
            .Note = "VERSEMENT",
            .MontantTexte = "95,90",
            .EstDebit = False
        }

        Dim mvt As Mouvements = SaisieService.CreerMouvement(dto)

        Assert.AreEqual(95.9D, mvt.Montant)
        Assert.IsTrue(mvt.Sens)
    End Sub
    <TestMethod>
    Public Sub ParserMontant_AccepteMontantNegatif()
        Dim result As Decimal = SaisieService.ParserMontant("-67,43")

        Assert.AreEqual(67.43D, result)
    End Sub

    <TestMethod>
    Public Sub CreerMouvement_DtoNull_LeveException()
        Try
            SaisieService.CreerMouvement(Nothing)
            Assert.Fail("Une exception était attendue.")
        Catch ex As ArgumentNullException
            ' OK
        End Try
    End Sub

    <TestMethod>
    Public Sub CreerMouvement_NoteNothing_DonneNoteVide()
        Dim dto As New SaisieMouvementDto With {
            .DateMvt = New DateTime(2025, 12, 20),
            .Note = Nothing,
            .MontantTexte = "95,90",
            .EstDebit = False
        }

        Dim mvt As Mouvements = SaisieService.CreerMouvement(dto)

        Assert.AreEqual(String.Empty, mvt.Note)
    End Sub
End Class