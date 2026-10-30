Option Strict On
Option Explicit On

Imports System
Imports System.Globalization
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionMouvementMapperTests
    Private Shared Function FactureFictive() As Facture
        Return New Facture With {.montant_ttc = 120.42D, .montant_ht = 100D,
            .Montant = 999D, .devise = "EUR", .date_facture = "2026-02-03",
            .date_echeance = "2026-03-03", .numero_facture = "FAC-001",
            .fournisseur_nom = "AGUMAAA"}
    End Function

    Private Shared Function ChequeFictif() As Cheque
        Return New Cheque With {.montant_numerique = 42.12D, .montant_lettres = "incohérent",
            .devise = "EUR", .dateChq = "20260203", .numero_du_cheque = "0000123",
            .emetteur_numero_compte = Nothing}
    End Function

    <TestMethod>
    Public Sub Documents_ConserventMontantsReferencesEtSensExplicite()
        For Each sens In {PropositionMouvement.SensMouvement.Debit, PropositionMouvement.SensMouvement.Credit}
            Dim facture = PropositionMouvementMapper.Mapper(FactureFictive(), "facture-unique", sens)
            Dim cheque = PropositionMouvementMapper.Mapper(ChequeFictif(), "cheque-unique", sens)
            Assert.AreEqual(120.42D, facture.Montant)
            Assert.AreEqual(42.12D, cheque.Montant)
            Assert.AreEqual("FAC-001", facture.Reference)
            Assert.AreEqual("0000123", cheque.Reference)
            Assert.AreEqual("FACTURE", facture.TypeDocument)
            Assert.AreEqual("CHEQUE", cheque.TypeDocument)
            Assert.AreEqual("facture-unique", facture.CleSource)
            Assert.AreEqual("cheque-unique", cheque.CleSource)
            For Each proposition In {facture, cheque}
                Assert.AreEqual(sens, proposition.Sens)
                Assert.AreEqual(New DateTime(2026, 2, 3), proposition.DateProposee)
                Assert.IsFalse(String.IsNullOrWhiteSpace(proposition.Libelle))
                Assert.AreEqual(1, proposition.AvertissementsValidation.Count)
                Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub SensConfirme_SupprimeSeulementAvertissement()
        Dim sens = PropositionMouvement.SensMouvement.Credit
        For Each proposition In {PropositionMouvementMapper.Mapper(FactureFictive(), "f", sens, True),
                                PropositionMouvementMapper.Mapper(ChequeFictif(), "c", sens, True)}
            Assert.AreEqual(0, proposition.AvertissementsValidation.Count)
            Assert.AreEqual(sens, proposition.Sens)
            Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, proposition.Etat)
        Next
    End Sub

    <TestMethod>
    Public Sub MontantsNonPositifs_RejetesSansValeurDeSecours()
        For Each montant In {0D, -0.01D, Decimal.MinValue}
            Dim facture = FactureFictive()
            Dim cheque = ChequeFictif()
            facture.montant_ttc = montant
            cheque.montant_numerique = montant
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PropositionMouvementMapper.Mapper(facture, "f", PropositionMouvement.SensMouvement.Debit))
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PropositionMouvementMapper.Mapper(cheque, "c", PropositionMouvement.SensMouvement.Credit))
        Next
    End Sub

    <TestMethod>
    Public Sub DatesAbsentesAmbiguesOuInvalides_Rejetees()
        For Each valeur In New String() {Nothing, "", " ", "03/02/2026", "2026-02-30", "20260230", "0001-01-01", "2026-2-3", "2026-02-03T00:00:00", " 2026-02-03"}
            Dim facture = FactureFictive()
            Dim cheque = ChequeFictif()
            facture.date_facture = valeur
            facture.dateFacture = New DateTime(2026, 1, 1)
            cheque.dateChq = valeur
            cheque.date_impression_cheque = "2026-01-01"
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionMouvementMapper.Mapper(facture, "f", PropositionMouvement.SensMouvement.Debit))
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionMouvementMapper.Mapper(cheque, "c", PropositionMouvement.SensMouvement.Debit))
        Next
    End Sub

    <TestMethod>
    Public Sub ClesSourcesAbsentes_Rejetees()
        For Each cle In New String() {Nothing, "", " ", vbTab}
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionMouvementMapper.Mapper(FactureFictive(), cle, PropositionMouvement.SensMouvement.Debit))
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionMouvementMapper.Mapper(ChequeFictif(), cle, PropositionMouvement.SensMouvement.Debit))
        Next
    End Sub

    <TestMethod>
    Public Sub DevisesAbsentesOuNonEur_Rejetees()
        For Each devise In New String() {Nothing, "", "USD", "GBP", "eur", " EUR "}
            Dim facture = FactureFictive()
            Dim cheque = ChequeFictif()
            facture.devise = devise
            cheque.devise = devise
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionMouvementMapper.Mapper(facture, "f", PropositionMouvement.SensMouvement.Debit))
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PropositionMouvementMapper.Mapper(cheque, "c", PropositionMouvement.SensMouvement.Debit))
        Next
    End Sub

    <TestMethod>
    Public Sub DocumentsNulsEtSensInvalides_Rejetes()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() PropositionMouvementMapper.Mapper(CType(Nothing, Facture), "f", PropositionMouvement.SensMouvement.Debit))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() PropositionMouvementMapper.Mapper(CType(Nothing, Cheque), "c", PropositionMouvement.SensMouvement.Debit))
        For Each valeur In {-1, 2}
            Dim sens = CType(valeur, PropositionMouvement.SensMouvement)
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PropositionMouvementMapper.Mapper(FactureFictive(), "f", sens))
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PropositionMouvementMapper.Mapper(ChequeFictif(), "c", sens))
        Next
    End Sub

    <TestMethod>
    Public Sub Mapping_InvariantRepetableEtSansModificationDuDocument()
        Dim cultureInitiale = CultureInfo.CurrentCulture
        Dim facture = FactureFictive()
        Try
            For Each culture In {"fr-FR", "en-US"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                For Each dateTexte In {"2024-02-29", "20240229"}
                    facture.date_facture = dateTexte
                    Dim premier = PropositionMouvementMapper.Mapper(facture, "source-1", PropositionMouvement.SensMouvement.Debit)
                    Dim second = PropositionMouvementMapper.Mapper(facture, "source-2", PropositionMouvement.SensMouvement.Debit)
                    Assert.AreEqual(New DateTime(2024, 2, 29), premier.DateProposee)
                    Assert.AreEqual(premier.DateProposee, second.DateProposee)
                    Assert.AreEqual(premier.Montant, second.Montant)
                    Assert.AreEqual("source-2", second.CleSource)
                    Assert.AreEqual(dateTexte, facture.date_facture)
                    Assert.AreEqual(120.42D, facture.montant_ttc)
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = cultureInitiale
        End Try
    End Sub
End Class
