Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection

Public Class FrmSelectionGenerique

    Private ReadOnly _nomRequete As String
    Private ReadOnly _parametres As Dictionary(Of String, Object)
    Private ReadOnly _multiSelect As Boolean
    Private ReadOnly _lectureSeule As Boolean
    Private ReadOnly _callback As Action(Of List(Of Object))
    Private _listeEntites As IList
    Private ReadOnly _typeEntity As Type
    Private _dataTable As DataTable
    Private _rowCountTotal As Integer = 0

    ' --- Résultats sélectionnés (objets ou DataRow) ---
    Private _resultatsSelectionnes As List(Of Object)
    Public Property ResultatsSelectionnes As List(Of Object)
        Get
            Return _resultatsSelectionnes
        End Get
        Private Set(value As List(Of Object))
            _resultatsSelectionnes = value
        End Set
    End Property

    ' --- Constructeur ---
    Public Sub New(typeEntity As Type,
                   nomRequete As String,
                   Optional parametres As Dictionary(Of String, Object) = Nothing,
                   Optional multiSelect As Boolean = True,
                   Optional lectureSeule As Boolean = True,
                   Optional callback As Action(Of List(Of Object)) = Nothing)
        InitializeComponent()
        _typeEntity = typeEntity
        _nomRequete = nomRequete
        _parametres = parametres
        _multiSelect = multiSelect
        _lectureSeule = lectureSeule
        _callback = callback
        Text = $"Sélection de {_nomRequete}"
    End Sub

    ' --- Chargement ---
    Private Sub FrmSelectionGenerique_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ChargerDonnees()
        ConfigurerGrille()
    End Sub

    Private Sub ConfigurerGrille()
        dgvResultats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvResultats.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResultats.MultiSelect = _multiSelect
        dgvResultats.ReadOnly = _lectureSeule
        dgvResultats.AllowUserToAddRows = False
        dgvResultats.AllowUserToDeleteRows = False
    End Sub

    ' --- Chargement des données ---
    Public Sub ChargerDonnees()
        Try
            ' 🔹 Récupération de la méthode générique
            Dim mi As Reflection.MethodInfo = GetType(SqlCommandBuilder).GetMethod("GetEntities")

            If mi Is Nothing Then
                Throw New MissingMethodException($"Méthode 'GetEntities' introuvable dans SqlCommandBuilder")
            End If

            ' 🔹 Construction dynamique de GetEntities(Of _typeEntity)
            Dim methodGeneric = mi.MakeGenericMethod(_typeEntity)

            ' 🔹 Appel et récupération de la liste
            _listeEntites = CType(methodGeneric.Invoke(Nothing, New Object() {Constantes.bddAgumaaa, _nomRequete, _parametres}), IList)

            ' 🔹 Binding sur la grille
            dgvResultats.DataSource = Nothing
            dgvResultats.AutoGenerateColumns = True
            dgvResultats.DataSource = _listeEntites

            Logger.INFO($"✅ {_listeEntites.Count} élément(s) chargé(s) de type {_typeEntity.Name}")

        Catch ex As TargetInvocationException
            Logger.ERR(
            $"Erreur réflexion lors du chargement de {_typeEntity.Name} : {ex.InnerException?.Message}"
        )

        Catch ex As Exception
            Logger.ERR(
            $"Erreur lors du chargement des données de type {_typeEntity.Name} : {ex.Message}"
        )
        End Try
    End Sub


    ' --- Filtrage (si _dataTable est utilisé) ---
    Private Sub txtFiltre_TextChanged(sender As Object, e As EventArgs) Handles txtFiltre.TextChanged
        If _dataTable Is Nothing Then Return
        Dim dv As DataView = _dataTable.DefaultView

        If txtFiltre.Text.Trim() = "" Then
            dv.RowFilter = ""
        Else
            Dim filtre As String = String.Join(" OR ",
                _dataTable.Columns.Cast(Of DataColumn)().
                Where(Function(c) c.DataType Is GetType(String)).
                Select(Function(c) $"[{c.ColumnName}] LIKE '%{txtFiltre.Text.Replace("'", "''")}%'"))
            dv.RowFilter = filtre
        End If

        lblStatus.Text = $"{dv.Count} / {_rowCountTotal} enregistrement(s)"
    End Sub

    ' --- Actualiser ---
    Private Sub btnActualiser_Click(sender As Object, e As EventArgs) Handles btnActualiser.Click
        Logger.INFO($"Actualisation '{_nomRequete}'...")
        ChargerDonnees()
        txtFiltre.Clear()
    End Sub

    ' --- OK ---
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Dim selection As New List(Of Object)

        For Each row As DataGridViewRow In dgvResultats.SelectedRows
            Dim boundItem = row.DataBoundItem

            If TypeOf boundItem Is DataRowView Then
                ' Cas DataTable
                Dim drv As DataRowView = DirectCast(boundItem, DataRowView)
                selection.Add(drv.Row)
            Else
                ' Cas List(Of T)
                selection.Add(boundItem)
            End If
        Next

        If selection.Count = 0 Then
            Logger.INFO($"Aucune ligne sélectionnée pour '{_nomRequete}' / '{_parametres}'.")
            Return
        End If

        Logger.INFO($"{selection.Count} ligne(s) sélectionnée(s) dans '{_nomRequete}' / '{_parametres}'.")

        ResultatsSelectionnes = selection

        If _callback IsNot Nothing Then
            _callback.Invoke(selection)
        End If

        DialogResult = DialogResult.OK
        Close()
    End Sub

    ' --- Annuler ---
    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        Logger.INFO($"Sélection annulée dans '{_nomRequete}'.")
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

End Class
