Option Strict On
Option Explicit On

Imports System
Imports System.Windows.Forms

Public Class FrmOfflineTestWorkspace
    Inherits FrmPrincipale

    Private ReadOnly _workspace As OfflineTestWorkspace
    Private _review As GmailDocumentReviewWorkflow
    Private ReadOnly _save As New Button With {
        .Name = "EnregistrerRevueTest", .Text = "Enregistrer la revue TEST",
        .Left = 575, .Top = 310, .Width = 240, .Enabled = False}
    Private ReadOnly _resume As New Button With {
        .Name = "ReprendreSessionTest", .Text = "Reprendre la dernière session",
        .Left = 830, .Top = 310, .Width = 280}
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
        Controls.AddRange(New Control() {_prepare, _load, _save, _resume, _status})
        AddHandler _prepare.Click, AddressOf PrepareSession
        AddHandler _load.Click, AddressOf LoadSession
        AddHandler _save.Click, AddressOf SaveSessionReview
        AddHandler _resume.Click, AddressOf ResumeSession
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
            Environment.NewLine & "Après examen ou correction, fermez la revue puis cliquez sur Enregistrer la revue TEST." &
            Environment.NewLine & "Reprendre la dernière session restaure les examens enregistrés. Les opérations restent fictives."
        Dim statusBar = DirectCast(Controls.Find("StatusLabelEnv", True)(0), StatusStrip)
        statusBar.Items.Clear()
        statusBar.Items.Add(New ToolStripStatusLabel("TEST HORS LIGNE — fichiers fictifs locaux"))
        MainMenuStrip.Items.Find("DemoDocumentsFictifsToolStripMenuItem", True)(0).Enabled = False
        MainMenuStrip.Items.Find("EspaceTestLocalToolStripMenuItem", True)(0).Enabled = False
        Dim analyse = DirectCast(MainMenuStrip.Items.Find("AnalyseToolStripMenuItem", True)(0), ToolStripMenuItem)
        Dim menuFichiers As New ToolStripMenuItem("Examiner des PDF et des scans de test sur disque…")
        AddHandler menuFichiers.Click, Sub() FrmDocumentFilePreview.OpenFiles(Me, _workspace)
        analyse.DropDownItems.Add(menuFichiers)
        Dim essai As New ToolStripMenuItem("Essayer l’enregistrement et l’archivage avec une facture fictive…")
        AddHandler essai.Click, AddressOf OpenArchiveTestInvoice
        analyse.DropDownItems.Add(essai)
    End Sub

    Private Sub OpenArchiveTestInvoice(sender As Object, e As EventArgs)
        Try
            Dim source = _workspace.CreateDocumentTestInvoice()
            Using preview As New FrmDocumentFilePreview({source}, _workspace)
                preview.ShowDialog(Me)
            End Using
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Facture TEST", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub PrepareSession(sender As Object, e As EventArgs)
        DefinirWorkflowDocumentsAnalyses(Nothing)
        _review = Nothing
        _save.Enabled = False
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
        _review = Nothing
        _save.Enabled = False
        Try
            Dim restored = _workspace.LoadReview()
            DefinirWorkflowDocumentsAnalyses(restored)
            MainMenuStrip.Items.Find("EffacerDocumentsFictifsToolStripMenuItem", True)(0).Enabled = False
            _review = restored
            _save.Enabled = True
            _status.Text = "Documents TEST chargés depuis : " & _workspace.SessionDirectory &
                           Environment.NewLine & "Examens restaurés : " & _review.SessionRevue.NombreExamensConfirmes.ToString() &
                           ". Ouvrez Analyse > Revue des documents préparés."
        Catch ex As Exception
            _status.Text = "Chargement TEST impossible : " & ex.Message
        End Try
    End Sub
    Private Sub SaveSessionReview(sender As Object, e As EventArgs)
        Try
            _workspace.SaveReview(_review)
            _status.Text = "Revue TEST enregistrée : " & _workspace.SessionDirectory &
                           Environment.NewLine & "Corrections et examens fictifs conservés pour la prochaine ouverture."
        Catch ex As Exception
            _status.Text = "Enregistrement TEST impossible : " & ex.Message
        End Try
    End Sub

    Private Sub ResumeSession(sender As Object, e As EventArgs)
        DefinirWorkflowDocumentsAnalyses(Nothing)
        _review = Nothing
        _load.Enabled = False
        _save.Enabled = False
        Try
            _workspace.ResumeLastSession()
            _load.Enabled = True
            LoadSession(sender, e)
        Catch ex As Exception
            _status.Text = "Reprise TEST impossible : " & ex.Message
        End Try
    End Sub
End Class
