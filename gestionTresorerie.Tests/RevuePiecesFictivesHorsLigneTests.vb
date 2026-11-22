Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Linq
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RevuePiecesFictivesHorsLigneTests
    <TestMethod>
    Public Sub SelectionHorsLigne_IgnoreLesSousDossiersEtLesFichiersNonDocuments()
        Dim racine = Path.Combine(Path.GetTempPath(), "gestionTresorerie-fictif-" & Guid.NewGuid().ToString("N"))
        Try
            Directory.CreateDirectory(racine)
            Dim enfant = Directory.CreateDirectory(Path.Combine(racine, "sous-dossier")).FullName
            File.WriteAllText(Path.Combine(racine, "facture.pdf"), "PDF fictif")
            File.WriteAllText(Path.Combine(racine, "notes.txt"), "texte fictif")
            File.WriteAllBytes(Path.Combine(racine, "scan-vide.pdf"), Array.Empty(Of Byte)())
            File.WriteAllText(Path.Combine(enfant, "autre.pdf"), "PDF fictif")
            Dim lien = Path.Combine(racine, "lien-externe.pdf")
            Dim cible = Path.Combine(enfant, "autre.pdf")
            Dim lienCree = False
            Try
                File.CreateSymbolicLink(lien, cible)
                lienCree = True
            Catch ex As UnauthorizedAccessException
                ' Certaines machines Windows interdisent les liens symboliques sans droits spécifiques.
            Catch ex As IOException
                ' Le test des fichiers ordinaires reste exécuté sur ces machines.
            End Try
            Dim fichiers = FrmPrincipale.ListerPiecesFictivesTest(racine)
            If lienCree Then Assert.IsFalse(fichiers.Contains(lien), "Un lien symbolique ne doit pas être présenté en TEST.")
            Assert.AreEqual(1, fichiers.Length)
            Assert.AreEqual("facture.pdf", Path.GetFileName(fichiers(0)))
            Assert.IsFalse(fichiers.Any(Function(p) Path.GetFileName(p) = "scan-vide.pdf"))
            Assert.IsTrue(File.Exists(Path.Combine(enfant, "autre.pdf")))
        Finally
            If Directory.Exists(racine) Then Directory.Delete(racine, recursive:=True)
        End Try
    End Sub

    <TestMethod>
    Public Sub SelectionHorsLigne_RefuseUnDossierAbsent()
        Assert.ThrowsExactly(Of ArgumentException)(Function() FrmPrincipale.ListerPiecesFictivesTest(""))
    End Sub
End Class
