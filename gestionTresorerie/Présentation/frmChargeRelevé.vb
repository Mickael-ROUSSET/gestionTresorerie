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
            Call AjouterColonneTraite()
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur " & ex.Message & " est survenue sur la lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
            Logger.ERR("Une erreur " & ex.Message & " est survenue sur la lecture du relevé : " & sFichier)
        End Try
    End Sub

    Public Sub AjouterColonneTraite()
        ' Ajouter une colonne d'image pour "Traité" : appel de la procédure pour ajouter une colonne d'image
        UtilControles.AjouterColonneImage(dgvRelevé, "traiteImage", "Traité", DataGridViewImageCellLayout.Zoom, 30)

        ' Parcourir les lignes du DataGridView pour définir les images
        For Each row As DataGridViewRow In dgvRelevé.Rows
            'On ne traite pas la 1ère ligne qui est l'entête
            If Not row.IsNewRow AndAlso row.Index > 0 Then
                Try
                    ' Supposons que les colonnes "dateMvt", "montant" et "sens" sont respectivement aux indices 1, 2 et 3
                    Dim dateMvt As Date = CDate(row.Cells(1).Value)
                    Dim montant As Decimal
                    Dim sens As Boolean

                    ' Vérifier la présence du montant dans row.Cells(3) ou row.Cells(4)
                    If Not IsDBNull(row.Cells(3).Value) AndAlso Decimal.TryParse(row.Cells(3).Value.ToString(), montant) Then
                        sens = True
                    ElseIf Not IsDBNull(row.Cells(4).Value) AndAlso Decimal.TryParse(row.Cells(4).Value.ToString(), montant) Then
                        sens = False
                    Else
                        Logger.WARN($"Montant non trouvé dans les cellules de la ligne {row.Index} : {row.ToString}")
                    End If

                    If Mouvements.Existe(dateMvt, montant, sens) Then
                        row.Cells("traiteImage").Value = My.Resources.OK
                    Else
                        row.Cells("traiteImage").Value = My.Resources.KO
                    End If
                Catch ex As Exception
                    Logger.ERR($"Erreur lors de la définition de l'image pour la colonne 'Traité' dans la ligne {row.Index}: {ex.Message}")
                    row.Cells("traiteImage").Value = Nothing ' Par défaut, en cas d'erreur
                End Try
            End If
        Next
    End Sub
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