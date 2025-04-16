Imports System.IO

Public Class FrmChargeRelevé
    Private Sub BtnOuvreFichier_Click(sender As Object, e As EventArgs) Handles btnOuvreFichier.Click
        AlimenteLstMvtCA(OuvreFichier)
    End Sub
    Public Sub AlimenteLstMvtCA(sFichier As String)
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture   

            Dim sLigne As String = monStreamReader.ReadLine
            While sLigne IsNot Nothing
                'AjouteLigne(sLigne)
                dgvRelevé.Rows.Add(Split(sLigne, Constantes.pointVirgule))
                sLigne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur " & ex.Message & " est survenue sur la lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
            Logger.GetInstance.ERR("Une erreur " & ex.Message & " est survenue sur la lecture du relevé : " & sFichier)
        End Try
    End Sub

    'Private Sub DataGridView1_SelectionChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvRelevé.CellContentClick
    '    Dim sMontant As String

    '    FrmSaisie.dateMvt.Value = Convert.ToDateTime(dgvRelevé.CurrentRow.Cells.Item(0).FormattedValue)
    '    FrmSaisie.txtNote.Text = dgvRelevé.CurrentRow.Cells.Item(1).FormattedValue
    '    If dgvRelevé.CurrentRow.Cells.Item(2).FormattedValue <> "" Then
    '        sMontant = dgvRelevé.CurrentRow.Cells.Item(2).FormattedValue
    '        FrmSaisie.rbDebit.Checked = True
    '    Else
    '        sMontant = dgvRelevé.CurrentRow.Cells.Item(3).FormattedValue
    '        FrmSaisie.rbCredit.Checked = True
    '    End If
    '    FrmSaisie.txtMontant.Text = sMontant
    '    FrmSaisie.Show()
    'End Sub

    Private Sub btnOuvreSaisie_Click(sender As Object, e As EventArgs) Handles btnOuvreSaisie.Click
        Dim sMontant As String
        Dim cellules As DataGridViewCellCollection

        cellules = dgvRelevé.CurrentRow.Cells
        With FrmSaisie
            .dateMvt.Value = Convert.ToDateTime(cellules.Item(0).FormattedValue)
            .txtNote.Text = cellules.Item(1).FormattedValue
            If cellules.Item(2).FormattedValue <> String.Empty Then
                'Il s'agit d'un débit
                sMontant = cellules.Item(2).FormattedValue
                .rbDebit.Checked = True
            Else
                'Il s'agit d'un crédit
                sMontant = cellules.Item(3).FormattedValue
                .rbCredit.Checked = True
            End If
            .txtMontant.Text = sMontant
            .Show()
        End With
    End Sub
End Class