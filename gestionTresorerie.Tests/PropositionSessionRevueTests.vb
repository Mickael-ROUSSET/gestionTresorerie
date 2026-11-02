Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PropositionSessionRevueTests
    Private Shared Function Source(Optional cle As String = " Source / AbC ",
                                   Optional avertissements As IEnumerable(Of String) = Nothing) As PropositionMouvement
        Return New PropositionMouvement(cle, "CHEQUE", New DateTime(2026, 1, 1),
            10D, PropositionMouvement.SensMouvement.Debit, "Fictif", "00001", avertissements)
    End Function

    Private Shared Function Corriger(p As PropositionMouvement, montant As Decimal) As PropositionCorrection.Resultat
        Return PropositionCorrection.Corriger(p, p.DateProposee, montant, p.Sens, p.Libelle, p.Reference)
    End Function

    <TestMethod>
    Public Sub Correction_RemplaceSeulementLaSelectionEtConserveLaChaineAudit()
        Dim original = Source()
        Dim autre = Source("autre")
        Dim entrees As New List(Of PropositionMouvement) From {original, autre}
        Dim session As New PropositionSessionRevue(entrees)
        entrees.Clear()
        Dim premiere = Corriger(original, 12.34D)
        session.AppliquerCorrection(original.CleSource, premiere)
        Dim seconde = Corriger(premiere.Proposition, 20D)
        session.AppliquerCorrection(original.CleSource, seconde)
        Assert.AreEqual(2, session.Propositions.Count)
        Assert.AreSame(seconde.Proposition, session.Propositions(original.CleSource))
        Assert.AreSame(autre, session.Propositions(autre.CleSource))
        Assert.AreNotSame(original, premiere.Proposition)
        Assert.AreEqual(10D, original.Montant)
        Assert.AreEqual("00001", seconde.Proposition.Reference)
        Assert.AreEqual(2, session.Audits.Count)
        Assert.AreSame(original, session.Audits(0).Avant)
        Assert.AreSame(premiere.Proposition, session.Audits(1).Avant)
        Assert.AreSame(seconde, session.Audits(1))
        Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(session.Propositions, IDictionary(Of String, PropositionMouvement)).Clear())
        Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(session.Audits, IList(Of PropositionCorrection.Resultat)).Clear())
    End Sub

    <TestMethod>
    Public Sub Annulation_SansEffetEtCorrectionIdentique_NouvellePropositionAuditee()
        Dim p = Source()
        Dim session As New PropositionSessionRevue({p})
        session.AppliquerCorrection(p.CleSource, Nothing)
        Assert.AreSame(p, session.Propositions(p.CleSource))
        Assert.AreEqual(0, session.Audits.Count)
        Dim identique = Corriger(p, p.Montant)
        session.AppliquerCorrection(p.CleSource, identique)
        Assert.AreNotSame(p, session.Propositions(p.CleSource))
        Assert.AreEqual(0, session.Audits(0).ChampsModifies.Count)
        Assert.AreEqual(1, session.Audits.Count)
    End Sub

    <TestMethod>
    Public Sub DoublonsEtIdentitesAmbigues_SontRejetesSansNormaliserLesCles()
        Dim p = Source()
        For Each cle In {p.CleSource, p.CleSource.Trim(), p.CleSource.ToUpperInvariant()}
            Assert.ThrowsExactly(Of ArgumentException)(Sub()
                Dim session As New PropositionSessionRevue({p, Source(cle)})
            End Sub)
        Next
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub()
            Dim session As New PropositionSessionRevue(Nothing)
        End Sub)
        Assert.ThrowsExactly(Of ArgumentException)(Sub()
            Dim session As New PropositionSessionRevue({p, Nothing})
        End Sub)
        Dim vide As New PropositionSessionRevue(Array.Empty(Of PropositionMouvement)())
        Assert.AreEqual(0, vide.Propositions.Count)
    End Sub

    <TestMethod>
    Public Sub SelectionInvalideCorrectionEtrangereOuObsolete_SansEffet()
        Dim p = Source()
        Dim session As New PropositionSessionRevue({p})
        For Each cle In {Nothing, "", "absent", p.CleSource.Trim(), p.CleSource.ToUpperInvariant()}
            Assert.ThrowsExactly(Of ArgumentException)(Sub() session.AppliquerCorrection(cle, Nothing))
            Assert.ThrowsExactly(Of ArgumentException)(Sub() session.Prevalider(cle, 1, Array.Empty(Of String)(), False))
        Next
        Assert.ThrowsExactly(Of ArgumentException)(Sub() session.AppliquerCorrection(p.CleSource, Corriger(Source(), 20D)))
        Assert.ThrowsExactly(Of ArgumentException)(Sub() session.AppliquerCorrection(p.CleSource, Corriger(Source("autre"), 20D)))
        Assert.AreSame(p, session.Propositions(p.CleSource))
        Assert.AreEqual(0, session.Audits.Count)
        Dim correction = Corriger(p, 20D)
        session.AppliquerCorrection(p.CleSource, correction)
        Assert.ThrowsExactly(Of ArgumentException)(Sub() session.AppliquerCorrection(p.CleSource, correction))
        Assert.AreSame(correction.Proposition, session.Propositions(p.CleSource))
        Assert.AreEqual(1, session.Audits.Count)
    End Sub

    <TestMethod>
    Public Sub AvertissementsRetenusEtConfirmationsExplicites_RestentDansAudit()
        Dim p = Source(avertissements:={"Sens incertain", "Inconnu", "Inconnu"})
        Dim session As New PropositionSessionRevue({p})
        Dim correction = Corriger(p, 15D)
        session.AppliquerCorrection(p.CleSource, correction)
        CollectionAssert.AreEqual({"Sens incertain", "Inconnu", "Inconnu"},
            New List(Of String)(session.Propositions(p.CleSource).AvertissementsValidation))
        Assert.IsFalse(session.Prevalider(p.CleSource, 1, Array.Empty(Of String)(), False).EstPrete)
        Dim suivante = PropositionCorrection.Corriger(correction.Proposition, p.DateProposee, 15D,
            p.Sens, p.Libelle, p.Reference,
            {New PropositionCorrection.ConfirmationAvertissement(PropositionCorrection.Champ.Sens, "Sens incertain")})
        session.AppliquerCorrection(p.CleSource, suivante)
        CollectionAssert.AreEqual({"Inconnu", "Inconnu"}, New List(Of String)(suivante.Proposition.AvertissementsValidation))
        Assert.AreEqual(3, p.AvertissementsValidation.Count)
        Assert.AreEqual(1, session.Audits(1).Confirmations.Count)
        Assert.IsFalse(session.Prevalider(p.CleSource, 1, Array.Empty(Of String)(), False).EstPrete)
        Assert.AreEqual(2, session.Audits.Count)
    End Sub

    <TestMethod>
    Public Sub Prevalidation_NeVautJamaisApprobationOuPersistance()
        Dim p = Source()
        Dim session As New PropositionSessionRevue({p})
        Assert.IsTrue(session.Prevalider(p.CleSource, 1, Array.Empty(Of String)(), False).EstPrete)
        Assert.IsFalse(session.Prevalider(p.CleSource, 1, Array.Empty(Of String)()).EstPrete)
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerApprouvee(p.CleSource))
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() session.MarquerPersistee(p.CleSource))
        Assert.AreSame(p, session.Propositions(p.CleSource))
        Assert.AreEqual(PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine, p.Etat)
        Assert.AreEqual(0, session.Audits.Count)
    End Sub
End Class
