Option Strict On
Imports System.Windows.Forms
Imports Global.Windows.Security.Credentials.UI

Friend NotInheritable Class FrmSecretMessagerie
    Inherits Form
    Private ReadOnly compte As New TextBox With {.Width = 345, .Text = "agumaaa43@gmail.com"}
    Private ReadOnly consulter As New Button With {.Text = "Afficher après vérification…", .AutoSize = True}
    Private ReadOnly valeur As New Label With {.AutoSize = True}
    Private ReadOnly limite As New Timer With {.Interval = 15000}

    Friend Sub New()
        Text = "Secrets de messagerie"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False
        MaximizeBox = False
        ClientSize = New Drawing.Size(400, 188)
        Dim panneau As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(12),
            .FlowDirection = FlowDirection.TopDown, .WrapContents = False}
        panneau.Controls.AddRange(New Control() {New Label With {.Text = "Compte Gmail", .AutoSize = True}, compte, consulter, valeur})
        Controls.Add(panneau)
        AddHandler consulter.Click, AddressOf Afficher
        AddHandler limite.Tick, Sub() Masquer()
        AddHandler compte.TextChanged, Sub() Masquer()
        AddHandler Deactivate, Sub() Masquer()
        AddHandler FormClosed, Sub() limite.Dispose()
    End Sub

    Private Sub Masquer()
        limite.Stop()
        valeur.Text = ""
    End Sub

    Private Async Sub Afficher(sender As Object, e As EventArgs)
        Masquer()
        Dim adresse = compte.Text.Trim()
        If adresse = "" Then Return
        consulter.Enabled = False
        compte.Enabled = False
        Try
            Dim verifie As Boolean
            If OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) AndAlso
                Await UserConsentVerifier.CheckAvailabilityAsync().AsTask() = UserConsentVerifierAvailability.Available Then
                Dim resultat = Await UserConsentVerifierInterop.RequestVerificationForWindowAsync(Handle,
                    "Afficher le mot de passe d’application Gmail pour " & adresse).AsTask()
                verifie = resultat = UserConsentVerificationResult.Verified
            Else
                verifie = VerificationSession.Confirmer(Handle)
            End If
            If IsDisposed OrElse Disposing Then Return
            If Not verifie Then
                valeur.Text = "Identité Windows non vérifiée."
                Return
            End If
            Dim secret = SecretGmail.Lire(adresse)
            If String.IsNullOrEmpty(secret) Then
                valeur.Text = "Aucun mot de passe mémorisé pour ce compte."
            ElseIf ContainsFocus OrElse Form.ActiveForm Is Me Then
                valeur.Text = secret
                limite.Start()
            Else
                valeur.Text = "Revenez dans cette fenêtre puis cliquez sur Afficher."
            End If
        Catch ex As Exception
            If Not IsDisposed Then valeur.Text = "Consultation indisponible. Vérifiez les options de connexion Windows."
        Finally
            If Not IsDisposed Then
                consulter.Enabled = True
                compte.Enabled = True
            End If
        End Try
    End Sub
End Class
