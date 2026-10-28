Option Strict On
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Consultation seule d'un instantané PropositionRevue fourni par l'appelant.
''' Les valeurs sont copiées dans les grilles ; aucun document source n'est conservé.
''' Aucun traitement métier ni accès à une ressource externe n'est déclenché.
''' </summary>
Public NotInheritable Class FrmRevuePropositions
    Inherits Form

    Public Sub New(revue As PropositionRevue)
        If revue Is Nothing Then Throw New ArgumentNullException(NameOf(revue))

        Text = "Revue des propositions — lecture seule"
        AccessibleName = Text
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1100, 650)
        MinimumSize = New Size(700, 450)
        StartPosition = FormStartPosition.CenterParent

        Dim propositions = CreerGrille("grillePropositions", "Propositions en lecture seule", 0)
        AjouterColonnes(propositions, {"CleSource", "TypeDocument", "Reference", "DateProposee",
                                      "Montant", "Sens", "Libelle", "Statut", "Avertissements"},
                         {"Identité du document", "Type", "Référence", "Date proposée",
                          "Montant (EUR)", "Débit / Crédit", "Libellé", "Statut", "Avertissements de validation"})
        propositions.Columns("DateProposee").ValueType = GetType(DateTime)
        propositions.Columns("DateProposee").DefaultCellStyle.Format = "dd/MM/yyyy"
        propositions.Columns("Montant").ValueType = GetType(Decimal)
        propositions.Columns("Montant").DefaultCellStyle.Format = "N2"
        For Each ligne In revue.Lignes
            propositions.Rows.Add(ligne.CleSource, ligne.TypeDocument, ligne.Reference,
                                  ligne.DateProposee, ligne.Montant,
                                  If(ligne.Sens = PropositionMouvement.SensMouvement.Debit, "Débit", "Crédit"),
                                  ligne.Libelle, "En attente de revue humaine",
                                  String.Join(Environment.NewLine, ligne.AvertissementsValidation))
        Next

        Dim rejets = CreerGrille("grilleRejets", "Diagnostics des entrées rejetées en lecture seule", 1)
        AjouterColonnes(rejets, {"IndexEntree", "CleSource", "Motif"},
                         {"Index d'entrée (base 0)", "Identité du document", "Motif du rejet"})
        For Each rejet In revue.Rejets
            rejets.Rows.Add(rejet.IndexEntree, rejet.CleSource, rejet.Motif)
        Next

        Dim fermer As New Button With {
            .Name = "boutonFermer", .Text = "&Fermer", .AccessibleName = "Fermer la revue",
            .AutoSize = True, .Anchor = AnchorStyles.Right, .TabIndex = 2,
            .DialogResult = DialogResult.Cancel}
        AddHandler fermer.Click, Sub(sender As Object, e As EventArgs) Close()
        CancelButton = fermer

        Dim disposition As New TableLayoutPanel With {
            .Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 5, .Padding = New Padding(10)}
        disposition.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.RowStyles.Add(New RowStyle(SizeType.Percent, 65.0F))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.RowStyles.Add(New RowStyle(SizeType.Percent, 35.0F))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.Controls.Add(New Label With {.Text = "Propositions — lecture seule", .AutoSize = True}, 0, 0)
        disposition.Controls.Add(propositions, 0, 1)
        disposition.Controls.Add(New Label With {.Text = "Entrées rejetées — diagnostics uniquement", .AutoSize = True}, 0, 2)
        disposition.Controls.Add(rejets, 0, 3)
        disposition.Controls.Add(fermer, 0, 4)
        Controls.Add(disposition)
    End Sub

    Private Shared Function CreerGrille(nom As String, description As String, ordre As Integer) As DataGridView
        Dim grille As New DataGridView With {
            .Name = nom, .AccessibleName = description, .AccessibleDescription = description,
            .Dock = DockStyle.Fill, .TabIndex = ordre, .ReadOnly = True,
            .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
            .AllowUserToOrderColumns = False, .AutoGenerateColumns = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            .RowHeadersVisible = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False, .EditMode = DataGridViewEditMode.EditProgrammatically}
        grille.DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Return grille
    End Function

    Private Shared Sub AjouterColonnes(grille As DataGridView, noms As String(), titres As String())
        For i As Integer = 0 To noms.Length - 1
            grille.Columns.Add(New DataGridViewTextBoxColumn With {
                .Name = noms(i), .HeaderText = titres(i), .ReadOnly = True,
                .SortMode = DataGridViewColumnSortMode.NotSortable})
        Next
    End Sub
End Class
