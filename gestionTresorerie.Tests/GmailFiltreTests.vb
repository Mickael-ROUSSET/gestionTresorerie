Option Strict On
Imports gestionTresorerie
Imports MimeKit
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GmailFiltreTests
    <TestMethod>
    Public Sub VerifieAdresseExacteEtFiltreVide()
        Dim filtre = EmailProcessor.ExpediteurPhotocopieur
        Using message As New MimeMessage()
            message.From.Add(New MailboxAddress(filtre, "autre@example.invalid"))
            Assert.IsFalse(EmailProcessor.ExpediteurAccepte(message, filtre))
            Assert.IsTrue(EmailProcessor.ExpediteurAccepte(message, ""))
            message.From.Clear()
            message.From.Add(New MailboxAddress("Copieur", filtre.ToUpperInvariant()))
            Assert.IsTrue(EmailProcessor.ExpediteurAccepte(message, filtre))
        End Using
        Assert.ThrowsExactly(Of ArgumentException)(Function() EmailProcessor.NormaliserExpediteur("adresse incorrecte"))
    End Sub

    <TestMethod>
    Public Sub RelitEtRemplaceLeSecretDuCompteWindows()
        Dim compte = Guid.NewGuid().ToString() & "@example.invalid"
        Dim nom = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(compte)))
        Try
            Assert.IsNull(SecretGmail.Lire(compte))
            SecretGmail.Enregistrer(compte, "fictif-premier")
            Assert.AreEqual("fictif-premier", SecretGmail.Lire(compte.ToUpperInvariant()))
            SecretGmail.Enregistrer(compte, "fictif-second")
            Assert.AreEqual("fictif-second", SecretGmail.Lire(compte))
        Finally
            Using key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\AGUMAAA\GestionTresorerie\Gmail", True)
                key?.DeleteValue(nom, False)
            End Using
        End Try
    End Sub
End Class
