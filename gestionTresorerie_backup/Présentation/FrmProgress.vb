Imports System.Threading

Public Class FrmProgress
    Inherits Form

    Private lblTitle As Label
    Private lblStatus As Label
    Private lblCounter As Label
    Private progressBar As ProgressBar
    Private btnCancel As Button

    Private _max As Integer = 100
    Private _value As Integer = 0
    Private _cancelRequested As Boolean = False

    Public Event CancelRequested()

    Public Sub New(Optional title As String = "Progression", Optional showCancel As Boolean = True)
        ' Form basic settings
        Me.Text = title
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Drawing.Size(480, 130)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False

        ' Controls
        lblTitle = New Label With {
            .AutoSize = False,
            .Text = title,
            .Dock = DockStyle.Top,
            .Height = 22,
            .TextAlign = Drawing.ContentAlignment.MiddleLeft,
            .Padding = New Padding(8, 0, 0, 0)
        }

        progressBar = New ProgressBar With {
            .Dock = DockStyle.Top,
            .Height = 24,
            .Minimum = 0,
            .Maximum = _max,
            .Value = 0
        }

        lblStatus = New Label With {
            .AutoSize = False,
            .Text = "",
            .Dock = DockStyle.Top,
            .Height = 20,
            .TextAlign = Drawing.ContentAlignment.MiddleLeft,
            .Padding = New Padding(8, 0, 0, 0)
        }

        lblCounter = New Label With {
            .AutoSize = False,
            .Text = "0 / 0",
            .Dock = DockStyle.Top,
            .Height = 18,
            .TextAlign = Drawing.ContentAlignment.MiddleLeft,
            .Padding = New Padding(8, 0, 0, 0)
        }

        btnCancel = New Button With {
            .Text = "Annuler",
            .Width = 90,
            .Height = 28,
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .Left = Me.ClientSize.Width - 100,
            .Top = Me.ClientSize.Height - 38,
            .Visible = showCancel
        }

        AddHandler btnCancel.Click, AddressOf BtnCancel_Click

        ' Add controls in reverse order to respect Dock order
        Me.Controls.Add(btnCancel)
        Me.Controls.Add(lblCounter)
        Me.Controls.Add(lblStatus)
        Me.Controls.Add(progressBar)
        Me.Controls.Add(lblTitle)
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs)
        _cancelRequested = True
        ' Déclenche l'événement CANCEL (les abonnés feront le reste)
        RaiseEvent CancelRequested()
        ' Désactive le bouton pour éviter plusieurs clics
        SafeAction(Sub() btnCancel.Enabled = False)
    End Sub

    ' Thread-safe invocation helper
    Private Sub SafeAction(action As Action)
        If Me.IsHandleCreated AndAlso Me.InvokeRequired Then
            Try
                Me.BeginInvoke(New Action(Sub() action()))
            Catch
                ' ignore if form closing
            End Try
        Else
            Try
                action()
            Catch
                ' ignore if form closing
            End Try
        End If
    End Sub

    ' --- Public API ---

    Public Sub SetMaximum(max As Integer)
        If max <= 0 Then max = 1
        _max = max
        SafeAction(Sub()
                       progressBar.Maximum = max
                       lblCounter.Text = $"{_value} / {_max}"
                   End Sub)
    End Sub

    Public Sub Report(value As Integer)
        _value = Math.Max(0, Math.Min(value, _max))
        SafeAction(Sub()
                       progressBar.Value = _value
                       lblCounter.Text = $"{_value} / {_max}"
                   End Sub)
    End Sub

    Public Sub Increment(Optional delta As Integer = 1)
        Report(_value + delta)
    End Sub

    Public Sub ReportStatus(text As String)
        SafeAction(Sub() lblStatus.Text = text)
    End Sub

    ' Propriété d'état renommée pour éviter l'ambiguïté
    Public ReadOnly Property IsCancelRequested As Boolean
        Get
            Return _cancelRequested
        End Get
    End Property

    ' Clôture sécurisée du formulaire depuis un thread de fond
    Public Sub CloseSafely()
        SafeAction(Sub()
                       If Not Me.IsDisposed Then
                           Try
                               Me.DialogResult = DialogResult.OK
                               Me.Close()
                           Catch
                           End Try
                       End If
                   End Sub)
    End Sub

    ' Permet d'exposer la ProgressBar si tu veux l'utiliser directement
    Public ReadOnly Property ProgressBarControl As ProgressBar
        Get
            Return progressBar
        End Get
    End Property

End Class
