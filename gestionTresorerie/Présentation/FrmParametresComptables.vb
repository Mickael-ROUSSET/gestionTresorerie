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
    Private idEdition As Integer?
    Private ReadOnly ajouter As New Button With {.AutoSize = True, .Text = "Ajouter"}
    Private ReadOnly statut As New Label With {.Dock = DockStyle.Bottom, .AutoSize = True}
    Private ReadOnly contenu As New Panel With {.Dock = DockStyle.Fill}

    Public Sub New(cn As SqlConnection, environnement As String)
        connexion = cn
        Text = environnement & " — Paramétrage comptable"
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(960, 490)
        Dim bandeau As New Label With {.Dock = DockStyle.Top, .AutoSize = True, .Padding = New Padding(8),
            .Text = environnement & " — " & cn.Database, .Font = New Font(Font, FontStyle.Bold)}
        Dim modifier As New Button With {.AutoSize = True, .Text = "Modifier la ligne"}
        Dim nouveau As New Button With {.AutoSize = True, .Text = "Nouvelle saisie"}
        AddHandler modifier.Click, Sub() ChargerLigne()
        AddHandler grille.CellDoubleClick, Sub(sender, args)
            If args.RowIndex >= 0 Then ChargerLigne()
        End Sub
        AddHandler nouveau.Click, Sub() AfficherRubrique()
        saisie.Controls.AddRange(New Control() {etiquette, libelle, New Label With {.Text = "Début", .AutoSize = True}, debut,
            New Label With {.Text = "Fin", .AutoSize = True}, fin, clos, modifier, ajouter, nouveau})
        contenu.Controls.Add(grille)
        contenu.Controls.Add(saisie)
        contenu.Controls.Add(statut)
        For Each titre In {"Événements", "Exercices", "Types de mouvements"}
            onglets.TabPages.Add(titre)
        Next
#If Not DEMO_BUILD Then
        Dim secrets As New Button With {.Name = "secretsMessagerie", .Text = "Secrets de messagerie…", .AutoSize = True, .Dock = DockStyle.Bottom}
        AddHandler secrets.Click, Sub()
            Using dlg As New FrmSecretMessagerie()
                dlg.ShowDialog(Me)
            End Using
        End Sub
        Controls.Add(secrets)
#End If
        Controls.Add(onglets)
        onglets.SendToBack()
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
        idEdition = Nothing
        ajouter.Text = "Ajouter"
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

    Private Sub ChargerLigne()
        Dim ligne = TryCast(grille.CurrentRow?.DataBoundItem, Data.DataRowView)
        If ligne Is Nothing Then Return
        idEdition = CInt(ligne("Id"))
        If Rubrique = ParametresComptables.Rubrique.Exercices Then
            clos.Checked = CBool(ligne("Clos"))
        Else
            libelle.Text = Convert.ToString(ligne("Libellé"))
        End If
        If Not IsDBNull(ligne("Début")) Then debut.Value = CDate(ligne("Début"))
        If Not IsDBNull(ligne("Fin")) Then fin.Value = CDate(ligne("Fin"))
        debut.Checked = Not IsDBNull(ligne("Début"))
        fin.Checked = Not IsDBNull(ligne("Fin"))
        ajouter.Text = "Enregistrer les modifications"
        statut.Text = "Modification de la ligne n° " & idEdition.Value
    End Sub

    Private Sub Enregistrer()
        Try
            Dim premiere As Date? = If(debut.Checked, CType(debut.Value.Date, Date?), Nothing)
            Dim derniere As Date? = If(fin.Checked, CType(fin.Value.Date, Date?), Nothing)
            ParametresComptables.Ajouter(connexion, Rubrique, libelle.Text, premiere, derniere, clos.Checked, idEdition)
            AfficherRubrique()
            statut.Text = "Enregistrement effectué dans " & connexion.Database & "."
        Catch ex As Exception
            statut.Text = ex.Message
        End Try
    End Sub
End Class
