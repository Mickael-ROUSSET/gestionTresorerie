Public Class FrmRemiseChq
    Public Sub initListeChq()
        Dim clstiers = New ListeTiers()
        Dim maListeTiers = clstiers.getListeTiers()

        ' Parcourir la liste de Tiers et ajouter les données au DataGridView
        For Each tiers As Tiers In maListeTiers
            ' Ajouter une nouvelle ligne au DataGridView
            Dim rowIndex As Integer = dgvRemiseChq.Rows.Add()

            ' Définir les valeurs des cellules de la nouvelle ligne
            dgvRemiseChq.Rows(rowIndex).Cells("Id").Value = tiers.Id
            dgvRemiseChq.Rows(rowIndex).Cells("nom").Value = tiers.Nom
            dgvRemiseChq.Rows(rowIndex).Cells("prenom").Value = tiers.Prenom
            dgvRemiseChq.Rows(rowIndex).Cells("raisonSociale").Value = tiers.RaisonSociale
        Next
    End Sub
    Private Sub AjouteLigne(listeTiers As List(Of Tiers))
        ' Effacer les lignes existantes du DataGridView
        dgvRemiseChq.Rows.Clear()

        ' Configurer la colonne ComboBox pour afficher la liste des noms de Tiers
        Dim nomColumn As DataGridViewComboBoxColumn = DirectCast(dgvRemiseChq.Columns("nom"), DataGridViewComboBoxColumn)
        nomColumn.DataSource = listeTiers
        nomColumn.DisplayMember = "Nom" ' Afficher le nom du Tiers
        nomColumn.ValueMember = "Id" ' Utiliser l'ID du Tiers comme valeur

        ' Ajouter une ligne vide au DataGridView pour permettre la sélection
        Dim rowIndex As Integer = dgvRemiseChq.Rows.Add()

        ' Définir les valeurs des cellules de la nouvelle ligne
        dgvRemiseChq.Rows(rowIndex).Cells("Id").Value = DBNull.Value
        dgvRemiseChq.Rows(rowIndex).Cells("nom").Value = DBNull.Value
        dgvRemiseChq.Rows(rowIndex).Cells("prenom").Value = DBNull.Value
        dgvRemiseChq.Rows(rowIndex).Cells("raisonSociale").Value = DBNull.Value
    End Sub

    'Private Sub btnAjoutTiers_Click(sender As Object, e As EventArgs) Handles btnAjoutTiers.Click

    'End Sub

End Class