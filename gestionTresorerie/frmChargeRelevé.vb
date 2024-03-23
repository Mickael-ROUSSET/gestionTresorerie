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
                AjouteLigne(sLigne)
                sLigne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue sur a lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Sub AjouteLigne(sligne As String)
        Dim monElem As New ListViewItem
        Dim valeurs(3) As String

        'Date
        'lstMouvements.Items.Add(Split(sligne, ";")(0))
        valeurs(0) = Split(sligne, ";")(0)
        'Note
        valeurs(1) = Split(sligne, ";")(1)
        'Débit
        valeurs(2) = Split(sligne, ";")(2)
        'Crédit
        valeurs(3) = Split(sligne, ";")(3)
        dgvRelevé.Rows.Add(valeurs)
    End Sub

    Private Sub DataGridView1_SelectionChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvRelevé.CellContentClick
        'Dim sLibelle As String
        Dim sMontant As String
        'Dim posRemise As Integer

        FrmSaisie.dateMvt.Value = Convert.ToDateTime(dgvRelevé.CurrentRow.Cells.Item(0).FormattedValue)
        'sLibelle = Trim(dgvRelevé.CurrentRow.Cells.Item(1).FormattedValue)
        'FrmSaisie.txtNote.Text = sLibelle
        FrmSaisie.txtNote.Text = dgvRelevé.CurrentRow.Cells.Item(1).FormattedValue
        'posRemise = InStr(sLibelle, "REMISE DE CHEQUE", CompareMethod.Text)
        'If posRemise > 0 Then
        '    FrmSaisie.txtRemise.Text = Trim(Mid(sLibelle, posRemise + Len("REMISE DE CHEQUE") + 1, Len(sLibelle) - 1))
        'End If
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