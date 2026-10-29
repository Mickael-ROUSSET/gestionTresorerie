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

    Private ReadOnly sessionCorrection As SessionCorrectionPropositions
    Private ReadOnly propositions As DataGridView
    Private ReadOnly boutonCorriger As Button

    Public Sub New(revue As PropositionRevue, Optional session As SessionCorrectionPropositions = Nothing)
        If revue Is Nothing Then Throw New ArgumentNullException(NameOf(revue))
        If session IsNot Nothing Then
            Dim elements = session.Propositions
            Dim initiales = session.PropositionsInitiales
            If elements.Count <> revue.Lignes.Count OrElse initiales.Count <> revue.Lignes.Count Then
                Throw New ArgumentException("Le lot de correction ne correspond pas à la revue.", NameOf(session))
            End If
            For i As Integer = 0 To elements.Count - 1
                If Not CorrespondA(initiales(i), revue.Lignes(i)) OrElse
                   Not String.Equals(elements(i).CleSource, initiales(i).CleSource, StringComparison.Ordinal) OrElse
                   Not String.Equals(elements(i).TypeDocument, initiales(i).TypeDocument, StringComparison.Ordinal) Then
                    Throw New ArgumentException("Les documents d'origine ne correspondent pas à la revue.", NameOf(session))
                End If
            Next
        End If
        sessionCorrection = session

        Text = If(session Is Nothing, "Revue des propositions — lecture seule",
                  "Revue des propositions — correction en mémoire (sans approbation)")
        AccessibleName = Text
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1100, 650)
        MinimumSize = New Size(700, 450)
        StartPosition = FormStartPosition.CenterParent

        propositions = CreerGrille("grillePropositions", "Propositions en lecture seule", 0)
        AjouterColonnes(propositions, {"CleSource", "TypeDocument", "Reference", "DateProposee",
                                      "Montant", "Sens", "Libelle", "Statut", "Avertissements"},
                         {"Identité du document", "Type", "Référence", "Date proposée",
                          "Montant (EUR)", "Débit / Crédit", "Libellé", "Statut", "Avertissements de validation"})
        propositions.Columns("DateProposee").ValueType = GetType(DateTime)
        propositions.Columns("DateProposee").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss.FFFFFFF"
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
        disposition.Controls.Add(New Label With {
            .Text = If(session Is Nothing, "Propositions — lecture seule",
                       "Propositions — correction en mémoire, sans approbation"),
            .AutoSize = True}, 0, 0)
        disposition.Controls.Add(propositions, 0, 1)
        disposition.Controls.Add(New Label With {.Text = "Entrées rejetées — diagnostics uniquement", .AutoSize = True}, 0, 2)
        disposition.Controls.Add(rejets, 0, 3)
        If sessionCorrection Is Nothing Then
            disposition.Controls.Add(fermer, 0, 4)
        Else
            boutonCorriger = New Button With {
                .Name = "boutonCorriger", .Text = "Corriger la sélection",
                .AccessibleName = "Corriger la proposition sélectionnée en mémoire",
                .AutoSize = True, .Enabled = False}
            AddHandler boutonCorriger.Click, AddressOf OuvrirCorrectionSelectionnee
            AddHandler propositions.SelectionChanged, AddressOf SelectionPropositionChangee
            Dim actions As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill, .AutoSize = True, .FlowDirection = FlowDirection.LeftToRight}
            actions.Controls.Add(boutonCorriger)
            actions.Controls.Add(fermer)
            disposition.Controls.Add(actions, 0, 4)
            propositions.ClearSelection()
        End If
        Controls.Add(disposition)
        ' Le menu peut rouvrir la même session après une correction : afficher
        ' les valeurs actuelles, et non l'instantané initial de la revue.
        If sessionCorrection IsNot Nothing Then ActualiserPropositions()
    End Sub

    ''' <summary>Compare le lot d'origine, jamais les valeurs déjà corrigées en mémoire.</summary>
    Private Shared Function CorrespondA(source As PropositionMouvement, ligne As PropositionRevue.Ligne) As Boolean
        If Not String.Equals(source.CleSource, ligne.CleSource, StringComparison.Ordinal) OrElse
           Not String.Equals(source.TypeDocument, ligne.TypeDocument, StringComparison.Ordinal) OrElse
           source.DateProposee <> ligne.DateProposee OrElse
           source.Montant <> ligne.Montant OrElse source.Sens <> ligne.Sens OrElse
           Not String.Equals(source.Libelle, ligne.Libelle, StringComparison.Ordinal) OrElse
           Not String.Equals(source.Reference, ligne.Reference, StringComparison.Ordinal) OrElse
           source.AvertissementsValidation.Count <> ligne.AvertissementsValidation.Count Then
            Return False
        End If
        For i As Integer = 0 To source.AvertissementsValidation.Count - 1
            If Not String.Equals(source.AvertissementsValidation(i), ligne.AvertissementsValidation(i),
                                 StringComparison.Ordinal) Then Return False
        Next
        Return True
    End Function

    Private Sub SelectionPropositionChangee(sender As Object, e As EventArgs)
        boutonCorriger.Enabled = (propositions.SelectedRows.Count = 1)
    End Sub

    ''' <summary>Prépare la correction de l'identité explicitement sélectionnée.</summary>
    Public Function CreerFormulaireCorrectionSelectionnee() As FrmCorrectionProposition
        If sessionCorrection Is Nothing Then
            Throw New InvalidOperationException("La revue est en lecture seule.")
        End If
        If propositions.SelectedRows.Count <> 1 Then
            Throw New InvalidOperationException("Sélectionner une proposition à corriger.")
        End If
        Dim cleSource = CStr(propositions.SelectedRows(0).Cells("CleSource").Value)
        Return New FrmCorrectionProposition(sessionCorrection, cleSource)
    End Function

    Private Sub OuvrirCorrectionSelectionnee(sender As Object, e As EventArgs)
        Using formulaire = CreerFormulaireCorrectionSelectionnee()
            If formulaire.ShowDialog(Me) = DialogResult.OK Then
                ActualiserPropositions()
            End If
        End Using
    End Sub

    ''' <summary>Actualise uniquement l'affichage depuis la session en mémoire.</summary>
    Public Sub ActualiserPropositions()
        If sessionCorrection Is Nothing Then
            Throw New InvalidOperationException("La revue est en lecture seule.")
        End If
        For Each ligne As DataGridViewRow In propositions.Rows
            Dim source = sessionCorrection.Selectionner(CStr(ligne.Cells("CleSource").Value))
            ligne.Cells("DateProposee").Value = source.DateProposee
            ligne.Cells("Montant").Value = source.Montant
            ligne.Cells("Sens").Value = If(source.Sens = PropositionMouvement.SensMouvement.Debit, "Débit", "Crédit")
            ligne.Cells("Libelle").Value = source.Libelle
            ligne.Cells("Reference").Value = source.Reference
            ligne.Cells("Avertissements").Value = String.Join(Environment.NewLine, source.AvertissementsValidation)
            ligne.Cells("Statut").Value = "En attente de revue humaine"
        Next
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
