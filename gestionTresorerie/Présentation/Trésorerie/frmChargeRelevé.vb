Imports System.Globalization

Public Class FrmChargeRelevé

    Private _iColDate As Integer
    Private _iColNote As Integer
    Private _iColDebit As Integer
    Private _iColCredit As Integer
    Private _iColTraiteImage As Integer

    Private _lignesReleve As New List(Of LigneReleve)
    Private _mouvementsImportes As New List(Of Mouvements)

    Private Shared ReadOnly CultureFr As CultureInfo = CultureInfo.GetCultureInfo("fr-FR")
    Private _importResult As ImportReleveResult

    Public Sub New()
        InitializeComponent()

        _iColDate = dgvRelevé.Columns("dgVDate").Index
        _iColNote = dgvRelevé.Columns("dgVNOte").Index
        _iColDebit = dgvRelevé.Columns("dgVDébit").Index
        _iColCredit = dgvRelevé.Columns("dgVCrédit").Index
        _iColTraiteImage = dgvRelevé.Columns("traiteImage").Index

        AddHandler dgvRelevé.DataError, AddressOf dgvRelevé_DataError
    End Sub

    Private Sub dgvRelevé_DataError(sender As Object, e As DataGridViewDataErrorEventArgs)
        If e.Exception IsNot Nothing Then
            Logger.ERR($"Erreur de données : {e.Exception.Message}")
            MessageBox.Show($"Erreur de données : {e.Exception.Message}",
                            "Erreur",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
            e.ThrowException = False
        End If
    End Sub

    Private Sub BtnOuvreFichier_Click(sender As Object, e As EventArgs) Handles btnOuvreFichier.Click
        Try
            Dim fichier As String = LitRelevé.OuvreFichier()

            If String.IsNullOrWhiteSpace(fichier) Then
                Logger.INFO("Aucun fichier de relevé sélectionné.")
                Exit Sub
            End If

            AlimenteLstMvtCA(fichier)

        Catch ex As Exception
            Logger.ERR($"Erreur ouverture relevé : {ex.Message}")
            MessageBox.Show("Erreur lors de l'ouverture du relevé : " & ex.Message,
                            "Erreur",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub AlimenteLstMvtCA(sFichier As String)
        Try
            If String.IsNullOrWhiteSpace(sFichier) Then
                MessageBox.Show("Aucun fichier sélectionné.",
                            "Relevé",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
                Exit Sub
            End If

            _importResult = ImportReleveService.ChargerDepuisFichier(sFichier)

            If _importResult Is Nothing OrElse _importResult.NbLignes = 0 Then
                dgvRelevé.Rows.Clear()
                MessageBox.Show("Aucune ligne de relevé exploitable trouvée.",
                            "Relevé",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
                Exit Sub
            End If

            _lignesReleve = _importResult.Lignes
            _mouvementsImportes = _importResult.Mouvements

            AfficherLignesReleve(_importResult.Lignes)
            AjouterColonneTraite()

        Catch ex As Exception
            Logger.ERR($"Une erreur {ex.Message} est survenue sur la lecture du relevé : {sFichier}")
            MessageBox.Show($"Une erreur est survenue sur la lecture du relevé : {ex.Message}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub AfficherLignesReleve(lignes As IEnumerable(Of LigneReleve))
        dgvRelevé.Rows.Clear()

        For Each ligne As LigneReleve In lignes
            dgvRelevé.Rows.Add(
                Nothing,
                ligne.DateOperation.ToString("dd/MM/yyyy"),
                ligne.Libelle,
                If(ligne.MontantDebit.HasValue,
                   ligne.MontantDebit.Value.ToString("0.00", CultureFr),
                   String.Empty),
                If(ligne.MontantCredit.HasValue,
                   ligne.MontantCredit.Value.ToString("0.00", CultureFr),
                   String.Empty)
            )
        Next
    End Sub

    Private Sub AjouterColonneTraite()
        For Each row As DataGridViewRow In dgvRelevé.Rows
            If row.IsNewRow Then Continue For

            Try
                Dim mouvement As Mouvements = MouvementDepuisLigneGrille(row)

                If mouvement Is Nothing Then
                    row.Cells(_iColTraiteImage).Value = My.Resources.KO
                Else
                    row.Cells(_iColTraiteImage).Value =
                        If(Mouvements.Existe(mouvement), My.Resources.OK, My.Resources.KO)
                End If

            Catch ex As Exception
                Logger.ERR($"Erreur lors de la définition de l'image traité ligne {row.Index}: {ex.Message}")
                row.Cells(_iColTraiteImage).Value = My.Resources.KO
            End Try
        Next
    End Sub

    Private Function MouvementDepuisLigneGrille(row As DataGridViewRow) As Mouvements
        If row Is Nothing OrElse row.IsNewRow Then Return Nothing

        Dim dateMvt As DateTime
        If Not DateTime.TryParse(row.Cells(_iColDate).Value?.ToString(),
                                 CultureFr,
                                 DateTimeStyles.None,
                                 dateMvt) Then
            Return Nothing
        End If

        Dim libelle As String = If(row.Cells(_iColNote).Value, String.Empty).ToString()
        Dim debitTexte As String = If(row.Cells(_iColDebit).Value, String.Empty).ToString()
        Dim creditTexte As String = If(row.Cells(_iColCredit).Value, String.Empty).ToString()

        Dim ligne As New LigneReleve With {
            .DateOperation = dateMvt,
            .Libelle = libelle
        }

        Dim montant As Decimal

        If Not String.IsNullOrWhiteSpace(debitTexte) AndAlso
           Decimal.TryParse(debitTexte, NumberStyles.Number, CultureFr, montant) Then

            ligne.MontantDebit = montant

        ElseIf Not String.IsNullOrWhiteSpace(creditTexte) AndAlso
               Decimal.TryParse(creditTexte, NumberStyles.Number, CultureFr, montant) Then

            ligne.MontantCredit = montant

        Else
            Return Nothing
        End If

        Return ReleveToMouvementMapper.Mapper(ligne)
    End Function

    Private Sub btnOuvreSaisie_Click(sender As Object, e As EventArgs) Handles btnOuvreSaisie.Click
        If dgvRelevé.CurrentRow Is Nothing OrElse dgvRelevé.CurrentRow.IsNewRow Then
            MessageBox.Show("Veuillez sélectionner une ligne de relevé.",
                            "Sélection",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            Dim cellules As DataGridViewCellCollection = dgvRelevé.CurrentRow.Cells
            Dim sMontant As String

            With frmSaisie
                .dateMvt.Value = Convert.ToDateTime(cellules.Item(_iColDate).FormattedValue, CultureFr)
                .txtNote.Text = cellules.Item(_iColNote).FormattedValue.ToString()

                If cellules.Item(_iColDebit).FormattedValue.ToString() <> String.Empty Then
                    sMontant = cellules.Item(_iColDebit).FormattedValue.ToString()
                    .rbDebit.Checked = True
                Else
                    sMontant = cellules.Item(_iColCredit).FormattedValue.ToString()
                    .rbCredit.Checked = True
                End If

                .txtMontant.Text = sMontant
                .Show()
            End With

        Catch ex As Exception
            Logger.ERR($"Erreur ouverture saisie depuis relevé : {ex.Message}")
            MessageBox.Show("Erreur lors de l'ouverture de la saisie : " & ex.Message,
                            "Erreur",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FrmChargeRelevé_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class