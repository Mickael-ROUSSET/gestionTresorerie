Imports System.IO

Public Class FrmChargeRelevé
    Private Sub BtnOuvreFichier_Click(sender As Object, e As EventArgs) Handles btnOuvreFichier.Click
        Call AlimentLst(OuvreFichier())
    End Sub
    Private Sub AlimentLst(sFichier As String)
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture   

            Dim sLigne As String = monStreamReader.ReadLine
            While sLigne IsNot Nothing
                'AjouteLigne(sLigne)
                dgvRelevé.Rows.Add(Split(sLigne, ";"))
                sLigne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur " & ex.Message & " est survenue sur la lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
    'Private Sub AjouteLigne(sligne As String)
    '    'Dim valeurs(3) As String

    '    ''Date
    '    'valeurs(0) = Split(sligne, ";")(0)
    '    ''Note
    '    'valeurs(1) = Split(sligne, ";")(1)
    '    ''Débit
    '    'valeurs(2) = Split(sligne, ";")(2)
    '    ''Crédit
    '    'valeurs(3) = Split(sligne, ";")(3)
    '    ''dgvRelevé.Rows.Add(valeurs)
    '    dgvRelevé.Rows.Add(Split(sligne, ";"))
    'End Sub

    Private Sub DataGridView1_SelectionChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvRelevé.CellContentClick
        Dim sMontant As String

        FrmSaisie.dateMvt.Value = Convert.ToDateTime(dgvRelevé.CurrentRow.Cells.Item(0).FormattedValue)
        FrmSaisie.txtNote.Text = dgvRelevé.CurrentRow.Cells.Item(1).FormattedValue
        If dgvRelevé.CurrentRow.Cells.Item(2).FormattedValue <> "" Then
            sMontant = dgvRelevé.CurrentRow.Cells.Item(2).FormattedValue
            FrmSaisie.rbDebit.Checked = True
        Else
            sMontant = dgvRelevé.CurrentRow.Cells.Item(3).FormattedValue
            FrmSaisie.rbCredit.Checked = True
        End If
        FrmSaisie.txtMontant.Text = sMontant
        FrmSaisie.Show()
    End Sub
End Class