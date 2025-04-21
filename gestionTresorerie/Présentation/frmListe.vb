Public Class frmListe

    Public Property SelectedObject As Object

    Public Sub New(dataTable As DataTable)
        InitializeComponent()
        dgvListe.DataSource = dataTable
    End Sub

    Private Sub dgvData_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvListe.CellDoubleClick
        If e.RowIndex >= 0 Then
            SelectedObject = dgvListe.Rows(e.RowIndex).DataBoundItem
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

    Public Event objetSelectionneChanged As EventHandler(Of Integer)

    Private Sub btnSel_Click(sender As Object, e As EventArgs) Handles btnSel.Click
        ' Vérifier si une ligne est sélectionnée
        If dgvListe.SelectedRows.Count > 0 Then
            ' Déclencher l'événement avec l'index de la ligne sélectionnée
            RaiseEvent objetSelectionneChanged(sender, dgvListe.SelectedRows(0).Index)
        Else
            ' Déclencher l'événement avec -1 si aucune ligne n'est sélectionnée
            RaiseEvent objetSelectionneChanged(sender, -1)
        End If

        ' Fermer la fenêtre appelée
        Me.Close()
    End Sub


    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub
End Class