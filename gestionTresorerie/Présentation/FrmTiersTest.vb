Option Strict On
Option Explicit On

Imports System
Imports System.Data
Imports System.Windows.Forms

''' <summary>Gestion de tiers exclusivement fictifs dans bddAgumaaaTest.</summary>
Public NotInheritable Class FrmTiersTest
    Inherits Form

    Private ReadOnly _store As New SqlTiersTestStore()
    Private ReadOnly _search As New TextBox With {.Name = "RechercheTiersTest", .Left = 20, .Top = 25, .Width = 300}
    Private ReadOnly _nom As New TextBox With {.Name = "NomTiersTest", .Left = 20, .Top = 315, .Width = 320}
    Private ReadOnly _email As New TextBox With {.Name = "EmailTiersTest", .Left = 355, .Top = 315, .Width = 280, .MaxLength = 150}
    Private ReadOnly _ville As New TextBox With {.Name = "VilleTiersTest", .Left = 650, .Top = 315, .Width = 190}
    Private ReadOnly _results As New DataGridView With {
        .Name = "ResultatsTiersTest", .Left = 20, .Top = 75, .Width = 820, .Height = 200,
        .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
        .MultiSelect = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill}
    Private ReadOnly _status As New Label With {.Name = "EtatTiersTest", .Left = 20, .Top = 410, .Width = 820, .Height = 65}
    Private _selectedId As Integer

    Public Sub New()
        Text = "Tiers et coordonnées — TEST SQL uniquement"
        Width = 890
        Height = 530
        StartPosition = FormStartPosition.CenterParent
        Controls.AddRange(New Control() {
            New Label With {.Text = "Recherche par nom (littérale)", .Left = 20, .Top = 5, .Width = 250},
            _search,
            _results,
            New Label With {.Text = "Nom fictif", .Left = 20, .Top = 295, .Width = 200},
            _nom,
            New Label With {.Text = "Email fictif", .Left = 355, .Top = 295, .Width = 200},
            _email,
            New Label With {.Text = "Ville fictive", .Left = 650, .Top = 295, .Width = 180},
            _ville,
            _status})
        Dim find As New Button With {.Name = "ChercherTiersTest", .Text = "Rechercher", .Left = 335, .Top = 23, .Width = 125}
        Dim create As New Button With {.Name = "CreerTiersTest", .Text = "Créer un tiers fictif", .Left = 20, .Top = 355, .Width = 190}
        Dim update As New Button With {.Name = "ModifierTiersTest", .Text = "Modifier le tiers choisi", .Left = 225, .Top = 355, .Width = 200}
        Dim clear As New Button With {.Name = "NouveauTiersTest", .Text = "Nouvelle saisie", .Left = 440, .Top = 355, .Width = 155}
        Controls.AddRange(New Control() {find, create, update, clear})
        AddHandler _search.TextChanged, AddressOf InvalidateSearchSelection
        AddHandler find.Click, AddressOf SearchTiers
        AddHandler create.Click, AddressOf CreateTiers
        AddHandler update.Click, AddressOf UpdateTiers
        AddHandler clear.Click, Sub() ClearSelection()
        AddHandler _results.SelectionChanged, AddressOf SelectTiers
        _status.Text = "TEST SQL exclusivement. Saisissez des données fictives ; aucune connexion avant une action."
    End Sub

    Private Sub ClearSelection()
        _selectedId = 0
        _nom.Clear()
        _email.Clear()
        _ville.Clear()
        _results.DataSource = Nothing
        _results.ClearSelection()
        _status.Text = "Nouvelle saisie : création d'un tiers fictif."
    End Sub

    Private Sub SelectTiers(sender As Object, e As EventArgs)
        If _results.SelectedRows.Count <> 1 Then Return
        Dim row = _results.SelectedRows(0)
        If row.DataBoundItem Is Nothing Then Return
        Dim view = TryCast(row.DataBoundItem, DataRowView)
        If view Is Nothing Then Return
        _selectedId = CInt(view("Id"))
        _nom.Text = Convert.ToString(view("nom"))
        _email.Text = If(view.Row.IsNull("Email"), "", Convert.ToString(view("Email")))
        _ville.Text = If(view.Row.IsNull("Ville"), "", Convert.ToString(view("Ville")))
        _status.Text = "Tiers fictif sélectionné : " & _selectedId.ToString()
    End Sub

    Private Sub InvalidateSearchSelection(sender As Object, e As EventArgs)
        ' Les résultats ne correspondent plus au critère affiché : ne jamais modifier l'ancien tiers.
        _selectedId = 0
        _results.DataSource = Nothing
        ' Éviter de réutiliser par erreur les coordonnées du tiers précédemment sélectionné.
        _nom.Clear()
        _email.Clear()
        _ville.Clear()
        _status.Text = "Recherche modifiée : relancez Rechercher avant de sélectionner un tiers TEST."
    End Sub

    Private Sub SearchTiers(sender As Object, e As EventArgs)
        ' Une recherche, même échouée, invalide la sélection précédente.
        _selectedId = 0
        _results.DataSource = Nothing
        ' Une nouvelle recherche annule aussi les champs de la fiche précédemment sélectionnée,
        ' y compris si la recherche est refusée ou échoue avant le chargement SQL.
        _nom.Clear()
        _email.Clear()
        _ville.Clear()
        If String.IsNullOrWhiteSpace(_search.Text) Then
            _status.Text = "Recherche TEST refusée : renseignez un nom fictif à rechercher."
            Return
        End If
        Try
            _store.Initialize()
            _results.DataSource = _store.SearchTiers(_search.Text)
            _results.ClearSelection()
            _selectedId = 0
            _status.Text = "Résultats TEST : " & _results.Rows.Count.ToString() & ". Sélectionnez une ligne pour modifier."
        Catch ex As Exception
            _status.Text = "Recherche TEST impossible : " & ex.Message
        End Try
    End Sub

    Private Sub CreateTiers(sender As Object, e As EventArgs)
        If _selectedId > 0 Then
            _status.Text = "Création TEST refusée : un tiers est sélectionné. Cliquez sur Nouvelle saisie pour créer un autre tiers."
            Return
        End If
        If String.IsNullOrWhiteSpace(_nom.Text) Then
            _status.Text = "Création TEST refusée : renseignez un nom fictif."
            Return
        End If
        Try
            _store.Initialize()
            Dim id = _store.CreateTiers(_nom.Text, _email.Text, _ville.Text)
            _selectedId = 0
            _results.DataSource = Nothing
            _status.Text = "Tiers fictif créé sous l'identifiant " & id.ToString() & ". Recherchez-le pour le modifier."
        Catch ex As Exception
            _status.Text = "Création TEST annulée : " & ex.Message
        End Try
    End Sub

    Private Sub UpdateTiers(sender As Object, e As EventArgs)
        If _selectedId <= 0 Then
            _status.Text = "Sélectionnez d'abord un tiers dans les résultats TEST."
            Return
        End If
        If String.IsNullOrWhiteSpace(_nom.Text) Then
            _status.Text = "Modification TEST refusée : renseignez un nom fictif."
            Return
        End If
        Try
            _store.UpdateTiers(_selectedId, _nom.Text, _email.Text, _ville.Text)
            _status.Text = "Tiers fictif " & _selectedId.ToString() & " modifié et enregistré."
        Catch ex As Exception
            _status.Text = "Modification TEST annulée : " & ex.Message
        End Try
    End Sub
End Class
