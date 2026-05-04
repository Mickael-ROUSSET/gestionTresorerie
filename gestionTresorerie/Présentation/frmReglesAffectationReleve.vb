Public Class FrmReglesAffectationReleve

    Private _regles As List(Of RegleAffectationReleve)
    Private _regleCourante As RegleAffectationReleve

    Private Shared Function CreateRepository() As RegleAffectationReleveRepository
        Dim executor = RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)
        Return New RegleAffectationReleveRepository(executor)
    End Function

    Private Sub FrmReglesAffectationReleve_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cboSens.Items.Clear()
        cboSens.Items.Add("Tous")
        cboSens.Items.Add("Débit")
        cboSens.Items.Add("Crédit")
        cboSens.SelectedIndex = 0

        ChargerRegles()
    End Sub

    Private Sub ChargerRegles()
        _regles = CreateRepository().LireToutes()
        dgvRegles.DataSource = Nothing
        dgvRegles.DataSource = _regles
    End Sub

    Private Sub dgvRegles_SelectionChanged(sender As Object, e As EventArgs) Handles dgvRegles.SelectionChanged
        If dgvRegles.CurrentRow Is Nothing Then Exit Sub

        _regleCourante = TryCast(dgvRegles.CurrentRow.DataBoundItem, RegleAffectationReleve)
        If _regleCourante Is Nothing Then Exit Sub

        txtMotCle.Text = _regleCourante.MotCle
        txtIdTiers.Text = If(_regleCourante.IdTiers.HasValue, _regleCourante.IdTiers.Value.ToString(), "")
        txtIdCategorie.Text = If(_regleCourante.IdCategorie.HasValue, _regleCourante.IdCategorie.Value.ToString(), "")
        txtIdSousCategorie.Text = If(_regleCourante.IdSousCategorie.HasValue, _regleCourante.IdSousCategorie.Value.ToString(), "")
        numPriorite.Value = _regleCourante.Priorite
        chkActif.Checked = _regleCourante.Actif

        If Not _regleCourante.Sens.HasValue Then
            cboSens.SelectedItem = "Tous"
        ElseIf _regleCourante.Sens.Value Then
            cboSens.SelectedItem = "Crédit"
        Else
            cboSens.SelectedItem = "Débit"
        End If
    End Sub

    Private Sub btnNouveau_Click(sender As Object, e As EventArgs) Handles btnNouveau.Click
        _regleCourante = New RegleAffectationReleve With {.Priorite = 100, .Actif = True}
        txtMotCle.Clear()
        txtIdTiers.Clear()
        txtIdCategorie.Clear()
        txtIdSousCategorie.Clear()
        cboSens.SelectedItem = "Tous"
        numPriorite.Value = 100
        chkActif.Checked = True
    End Sub

    Private Sub btnEnregistrer_Click(sender As Object, e As EventArgs) Handles btnEnregistrer.Click
        If String.IsNullOrWhiteSpace(txtMotCle.Text) Then
            MessageBox.Show("Le mot-clé est obligatoire.")
            Exit Sub
        End If

        If _regleCourante Is Nothing Then
            _regleCourante = New RegleAffectationReleve()
        End If

        _regleCourante.MotCle = txtMotCle.Text.Trim()
        _regleCourante.IdTiers = LireNullableInt(txtIdTiers.Text)
        _regleCourante.IdCategorie = LireNullableInt(txtIdCategorie.Text)
        _regleCourante.IdSousCategorie = LireNullableInt(txtIdSousCategorie.Text)
        _regleCourante.Priorite = CInt(numPriorite.Value)
        _regleCourante.Actif = chkActif.Checked

        Select Case cboSens.Text
            Case "Débit"
                _regleCourante.Sens = False
            Case "Crédit"
                _regleCourante.Sens = True
            Case Else
                _regleCourante.Sens = Nothing
        End Select

        If _regleCourante.Id = 0 Then
            CreateRepository().Inserer(_regleCourante)
        Else
            CreateRepository().MettreAJour(_regleCourante)
        End If

        ChargerRegles()
    End Sub

    Private Sub btnSupprimer_Click(sender As Object, e As EventArgs) Handles btnSupprimer.Click
        If _regleCourante Is Nothing OrElse _regleCourante.Id = 0 Then Exit Sub

        If MessageBox.Show("Supprimer cette règle ?", "Confirmation",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question) = DialogResult.Yes Then

            CreateRepository().Supprimer(_regleCourante.Id)
            ChargerRegles()
        End If
    End Sub

    Private Shared Function LireNullableInt(value As String) As Integer?
        Dim i As Integer
        If Integer.TryParse(value, i) Then Return i
        Return Nothing
    End Function

    Private Sub btnFermer_Click(sender As Object, e As EventArgs) Handles btnFermer.Click
        Close()
    End Sub

End Class