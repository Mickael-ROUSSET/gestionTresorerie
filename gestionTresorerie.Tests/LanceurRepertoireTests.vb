Imports System
Imports System.IO
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class LanceurRepertoireTests
    <TestMethod>
    Public Sub RepertoireSelectionneEstUtiliseSansRetourAuDefaut()
        Dim dossier = Path.Combine(Path.GetTempPath(), "lanceur-fictif-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(dossier)
        Try
            Dim resultat = Lanceur.ValiderRepertoireSelectionne(dossier)
            Assert.AreEqual(Path.GetFullPath(dossier), resultat)
        Finally
            Directory.Delete(dossier, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub SelectionVideEstRefusee()
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() Lanceur.ValiderRepertoireSelectionne(Nothing))
        Assert.ThrowsExactly(Of ArgumentException)(
            Sub() Lanceur.ValiderRepertoireSelectionne("   "))
    End Sub

    <TestMethod>
    Public Sub RepertoireSupprimeEstRefuseSansRepli()
        Dim absent = Path.Combine(Path.GetTempPath(), "lanceur-absent-" & Guid.NewGuid().ToString("N"))
        Assert.IsFalse(Directory.Exists(absent))
        Assert.ThrowsExactly(Of DirectoryNotFoundException)(
            Sub() Lanceur.ValiderRepertoireSelectionne(absent))
    End Sub
End Class
