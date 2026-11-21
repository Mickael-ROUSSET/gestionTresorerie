Option Strict On
Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Windows.Forms

Friend NotInheritable Class FrmParametresComptables
    Inherits Form
    Private ReadOnly connexion As SqlConnection
    Private ReadOnly onglets As New TabControl With {.Dock = DockStyle.Fill}
    Private ReadOnly grille As New DataGridView With {.Dock = DockStyle.Fill, .ReadOnly = True,
        .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill}
    Private ReadOnly saisie As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .AutoSize = True}
    Private ReadOnly libelle As New TextBox With {.Width = 170, .MaxLength = 50, .AccessibleName = "Libellé"}
    Private ReadOnly etiquette As New Label With {.Text = "Libellé", .AutoSize = True}
    Private ReadOnly debut As New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Width = 125}
    Private ReadOnly fin As New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Width = 125}
    Private ReadOnly clos As New CheckBox With {.Text = "Exercice clos", .AutoSize = True}
    Private ReadOnly statut As New Label With {.Dock = DockStyle.Bottom, .AutoSize = True}
    Private ReadOnly contenu As New Panel With {.Dock = DockStyle.Fill}

    Public Sub New(cn As SqlConnection, environnement As String)
        connexion = cn
        Text = environnement & " — Paramétrage comptable"
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(960, 490)
        Dim bandeau As New Label With {.Dock = DockStyle.Top, .AutoSize = True, .Padding = New Padding(8),
            .Text = environnement & " — " & cn.Database, .Font = New Font(Font, FontStyle.Bold)}
        Dim ajouter As New Button With {.AutoSize = True, .Text = "Ajouter"}
        saisie.Controls.AddRange(New Control() {etiquette, libelle, New Label With {.Text = "Début", .AutoSize = True}, debut,
            New Label With {.Text = "Fin", .AutoSize = True}, fin, clos, ajouter})
        contenu.Controls.Add(grille)
        contenu.Controls.Add(saisie)
        contenu.Controls.Add(statut)
        For Each titre In {"Événements", "Exercices", "Types de mouvements"}
            onglets.TabPages.Add(titre)
        Next
        Controls.Add(onglets)
        Controls.Add(bandeau)
        AddHandler ajouter.Click, Sub() Enregistrer()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        onglets.SelectedIndex = 0
        AfficherRubrique()
        AddHandler onglets.SelectedIndexChanged, Sub() AfficherRubrique()
    End Sub

    Private ReadOnly Property Rubrique As ParametresComptables.Rubrique
        Get
            Return CType(onglets.SelectedIndex, ParametresComptables.Rubrique)
        End Get
    End Property

    Private Sub AfficherRubrique()
        Dim page = onglets.SelectedTab
        If page Is Nothing Then Return
        page.Controls.Add(contenu)
        Dim exercice = Rubrique = ParametresComptables.Rubrique.Exercices
        etiquette.Visible = Not exercice
        libelle.Visible = Not exercice
        libelle.Clear()
        clos.Visible = exercice
        clos.Checked = False
        debut.ShowCheckBox = Not exercice
        fin.ShowCheckBox = Not exercice
        debut.Value = New Date(Today.Year, 1, 1)
        fin.Value = New Date(Today.Year, 12, 31)
        debut.Checked = exercice
        fin.Checked = exercice
        statut.Text = ""
        Actualiser()
    End Sub

    Private Sub Actualiser()
        grille.DataSource = Nothing
        Try
            grille.DataSource = ParametresComptables.Lire(connexion, Rubrique)
        Catch ex As Exception
            statut.Text = "Lecture impossible. " & ex.Message
        End Try
    End Sub

    Private Sub Enregistrer()
        Try
            Dim premiere As Date? = If(debut.Checked, CType(debut.Value.Date, Date?), Nothing)
            Dim derniere As Date? = If(fin.Checked, CType(fin.Value.Date, Date?), Nothing)
            If premiere.HasValue AndAlso derniere.HasValue AndAlso derniere.Value < premiere.Value Then
                statut.Text = "La date de fin doit être postérieure ou égale à la date de début."
                Return
            End If
            ParametresComptables.Ajouter(connexion, Rubrique, libelle.Text, premiere, derniere, clos.Checked)
            statut.Text = "Enregistrement effectué dans " & connexion.Database & "."
            libelle.Clear()
            Actualiser()
        Catch ex As Exception
            statut.Text = ex.Message
        End Try
    End Sub
End Class
