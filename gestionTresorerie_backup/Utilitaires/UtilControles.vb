Public Class UtilControles
    Public Shared Sub AjouterColonneImage(dgv As DataGridView, nomColonne As String, texteEnTete As String, layoutImage As DataGridViewImageCellLayout, largeurColonne As Integer)
        ' Créer une nouvelle colonne d'image
        Dim imageColumn As New DataGridViewImageColumn With {
        .Name = nomColonne,
        .HeaderText = texteEnTete,
        .ImageLayout = layoutImage,
        .Width = largeurColonne
    }

        ' Insérer la colonne en première position
        dgv.Columns.Insert(0, imageColumn)

        ' Redimensionner le DataGridView pour afficher toutes les colonnes
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        dgv.AutoResizeColumns()
    End Sub

End Class
