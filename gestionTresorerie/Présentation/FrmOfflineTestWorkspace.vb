Option Strict On
Option Explicit On

Imports System
Imports System.Windows.Forms

Public Class FrmOfflineTestWorkspace
    Inherits FrmPrincipale

    Private ReadOnly _workspace As OfflineTestWorkspace
    Private ReadOnly _prepare As New Button With {
        .Name = "PreparerSessionTest", .Text = "Préparer / réinitialiser TEST",
        .Left = 25, .Top = 310, .Width = 260}
    Private ReadOnly _load As New Button With {
        .Name = "ChargerSessionTest", .Text = "Charger les documents TEST",
        .Left = 300, .Top = 310, .Width = 260, .Enabled = False}
    Private ReadOnly _status As New Label With {
        .Name = "EtatSessionTest", .Left = 25, .Top = 355, .Width = 1100, .Height = 110}

    Public Sub New(workspace As OfflineTestWorkspace)
        MyBase.New(True)
        If workspace Is Nothing Then Throw New ArgumentNullException(NameOf(workspace))
        _workspace = workspace
        Controls.AddRange(New Control() {_prepare, _load, _status})
        AddHandler _prepare.Click, AddressOf PrepareSession
        AddHandler _load.Click, AddressOf LoadSession
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Text = "gestionTresorerie — TEST HORS LIGNE"
        DirectCast(Controls.Find("annonceDemonstration", True)(0), Label).Text =
            "TEST HORS LIGNE — documents fictifs enregistrés sur votre ordinateur." &
            Environment.NewLine & "1. Préparer une session. 2. Charger les documents. " &
            "3. Analyse > Revue des documents préparés." &
            Environment.NewLine & "La réinitialisation crée une nouvelle session vierge. " &
            "Les anciennes sessions sont conservées." &
            Environment.NewLine & "Les opérations examinées restent des simulations."
        Dim statusBar = DirectCast(Controls.Find("StatusLabelEnv", True)(0), StatusStrip)
        statusBar.Items.Clear()
        statusBar.Items.Add(New ToolStripStatusLabel("TEST HORS LIGNE — fichiers fictifs locaux"))
        MainMenuStrip.Items.Find("DemoDocumentsFictifsToolStripMenuItem", True)(0).Enabled = False
        MainMenuStrip.Items.Find("EspaceTestLocalToolStripMenuItem", True)(0).Enabled = False
    End Sub

    Private Sub PrepareSession(sender As Object, e As EventArgs)
        DefinirWorkflowDocumentsAnalyses(Nothing)
        _load.Enabled = False
        Try
            Dim session = _workspace.PrepareNewSession()
            _status.Text = "Session prête : " & session & Environment.NewLine &
                           "Cinq documents fictifs. Les essais précédents sont conservés."
            _load.Enabled = True
        Catch ex As Exception
            _status.Text = "Préparation TEST impossible : " & ex.Message
        End Try
    End Sub

    Private Sub LoadSession(sender As Object, e As EventArgs)
        DefinirWorkflowDocumentsAnalyses(Nothing)
        Try
            PreparerRevueMessagesFictifs(_workspace.ReadMessages())
            _status.Text = "Documents TEST chargés depuis : " & _workspace.SessionDirectory &
                           Environment.NewLine & "Ouvrez Analyse > Revue des documents préparés."
        Catch ex As Exception
            _status.Text = "Chargement TEST impossible : " & ex.Message
        End Try
    End Sub
End Class
