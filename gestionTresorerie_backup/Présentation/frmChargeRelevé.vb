Imports System.IO

Public Class FrmChargeRelevé

    ' Utiliser l'index des colonnes du datagridview
    Private _iColDate As Integer
    Private _iColNote As Integer
    Private _iColDebit As Integer
    Private _iColCredit As Integer
    Private _iColTraiteImage As Integer
    Public Sub New()
        InitializeComponent()
        ' Utiliser l'index des colonnes
        _iColDate = dgvRelevé.Columns("dgVDate").Index
        _iColNote = dgvRelevé.Columns("dgVNOte").Index
        _iColDebit = dgvRelevé.Columns("dgVDébit").Index
        _iColCredit = dgvRelevé.Columns("dgVCrédit").Index
        _iColTraiteImage = dgvRelevé.Columns("traiteImage").Index
        ' Ajouter le gestionnaire pour l'événement DataError
        AddHandler dgvRelevé.DataError, AddressOf dgvRelevé_DataError
    End Sub
    Private Sub dgvRelevé_DataError(sender As Object, e As DataGridViewDataErrorEventArgs)
        ' Gérer les erreurs de données
        If e.Exception IsNot Nothing Then
            Dim unused = MsgBox($"Erreur de données : {e.Exception.Message}", MsgBoxStyle.Critical)
            Logger.ERR($"Erreur de données : {e.Exception.Message}")
            ' Empêcher l'erreur de propager
            e.ThrowException = False
        End If
    End Sub
    Private Sub BtnOuvreFichier_Click(sender As Object, e As EventArgs) Handles btnOuvreFichier.Click
        AlimenteLstMvtCA(OuvreFichier)
    End Sub
    Public Sub AlimenteLstMvtCA(sFichier As String)
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture

            Dim sLigne As String = monStreamReader.ReadLine
            While sLigne IsNot Nothing
                ' Ajouter une zone vide pour la première colonne
                Dim valeurs As String() = Split(sLigne, Constantes.pointVirgule)
                Dim nouvelleLigne As Object() = New Object(valeurs.Length) {}
                nouvelleLigne(0) = String.Empty ' Ajouter une zone vide pour la première colonne
                Array.Copy(valeurs, 0, nouvelleLigne, 1, valeurs.Length)
                Dim unused1 = dgvRelevé.Rows.Add(nouvelleLigne)
                sLigne = monStreamReader.ReadLine
            End While
            Call AjouterColonneTraite()
            monStreamReader.Close()
        Catch ex As Exception
            Dim unused = MsgBox($"Une erreur {ex.Message} est survenue sur la lecture du relevé : {sFichier}", MsgBoxStyle.Critical)
            Logger.ERR($"Une erreur {ex.Message} est survenue sur la lecture du relevé : {sFichier}")
        End Try
    End Sub

    Private Sub AjouterColonneTraite()
        ' Parcourir les lignes du DataGridView pour définir les images
        For Each row As DataGridViewRow In dgvRelevé.Rows
            'On ne traite pas la 1ère ligne qui est l'entête mais on lui affecte une image vide
            If row.Index = 0 Then
                row.Cells(_iColTraiteImage).Value = New Bitmap(1, 1)
            End If
            If Not row.IsNewRow AndAlso row.Index > 0 Then
                Try
                    ' Supposons que les colonnes "dateMvt", "montant" et "sens" sont respectivement aux indices 1, 2 et 3
                    Dim dateMvt As Date = CDate(row.Cells(_iColDate).Value)
                    Dim montant As Decimal
                    Dim sens As Boolean

                    ' Vérifier la présence du montant dans row.Cells(iDebit) ou row.Cells(iCredit)
                    If Not IsDBNull(row.Cells(_iColDebit).Value) AndAlso Decimal.TryParse(row.Cells(_iColDebit).Value.ToString(), montant) Then
                        sens = True
                    ElseIf Not IsDBNull(row.Cells(_iColCredit).Value) AndAlso Decimal.TryParse(row.Cells(_iColCredit).Value.ToString(), montant) Then
                        sens = False
                    Else
                        Logger.WARN($"Montant non trouvé dans les cellules de la ligne {row.Index} : {row}")
                    End If

                    row.Cells(_iColTraiteImage).Value = If(Mouvements.Existe(dateMvt, montant, sens), My.Resources.OK, My.Resources.KO)
                Catch ex As Exception
                    Logger.ERR($"Erreur lors de la définition de l'image pour la colonne 'Traité' dans la ligne {row.Index}: {ex.Message}")
                    row.Cells("iTraiteImageIndex").Value = Nothing ' Par défaut, en cas d'erreur
                End Try
            End If
        Next
    End Sub
    Private Sub btnOuvreSaisie_Click(sender As Object, e As EventArgs) Handles btnOuvreSaisie.Click
        Dim sMontant As String
        Dim cellules As DataGridViewCellCollection

        cellules = dgvRelevé.CurrentRow.Cells
        With FrmSaisie
            .dateMvt.Value = Convert.ToDateTime(cellules.Item(_iColDate).FormattedValue)
            .txtNote.Text = cellules.Item(_iColNote).FormattedValue
            If cellules.Item(_iColDebit).FormattedValue <> String.Empty Then
                'Il s'agit d'un débit
                sMontant = cellules.Item(_iColDebit).FormattedValue
                .rbDebit.Checked = True
            Else
                'Il s'agit d'un crédit
                sMontant = cellules.Item(_iColCredit).FormattedValue
                .rbCredit.Checked = True
            End If
            .txtMontant.Text = sMontant
            .Show()
        End With
    End Sub
End Class