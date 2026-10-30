Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Grilles en lecture seule ; correction explicite des propositions en mémoire.
''' Sans sources métier fournies et vérifiées, seule la consultation est disponible.
''' Aucun document source ni accès à une ressource externe.
''' </summary>
Public NotInheritable Class FrmRevuePropositions
    Inherits Form

    Private ReadOnly sources As New Dictionary(Of DataGridViewRow, PropositionMouvement)()
    Private ReadOnly session As PropositionSessionRevue
    Private ReadOnly etatCorrections As New Label With {
        .Name = "etatCorrections", .AutoSize = True,
        .AccessibleName = "État des corrections et de leur enregistrement",
        .Text = "Corrections locales : non conservées après fermeture. Aucune opération enregistrée."}
    Private ReadOnly corriger As New Button With {
        .Name = "boutonCorriger", .Text = "Corriger la sélection (sans valider)",
        .AccessibleName = "Corriger la sélection (sans valider)", .AutoSize = True,
        .Enabled = False, .TabIndex = 2}

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
            .AutoSize = True, .Anchor = AnchorStyles.Right, .TabIndex = 3,
            .DialogResult = DialogResult.Cancel}
        AddHandler fermer.Click, Sub(sender As Object, e As EventArgs) Close()
        CancelButton = fermer

        Dim disposition As New TableLayoutPanel With {
            .Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 6, .Padding = New Padding(10)}
        disposition.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.RowStyles.Add(New RowStyle(SizeType.Percent, 65.0F))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.RowStyles.Add(New RowStyle(SizeType.Percent, 35.0F))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.Controls.Add(New Label With {.Text = "Propositions — lecture seule", .AutoSize = True}, 0, 0)
        disposition.Controls.Add(propositions, 0, 1)
        disposition.Controls.Add(New Label With {.Text = "Entrées rejetées — diagnostics uniquement", .AutoSize = True}, 0, 2)
        disposition.Controls.Add(rejets, 0, 3)
        Dim actions As New FlowLayoutPanel With {.AutoSize = True, .Dock = DockStyle.Fill}
        actions.Controls.AddRange(New Control() {corriger, fermer})
        disposition.Controls.Add(actions, 0, 4)
        disposition.Controls.Add(etatCorrections, 0, 5)
        AddHandler propositions.SelectionChanged, Sub() ActualiserAction(propositions)
        AddHandler corriger.Click, Sub() CorrigerSelection()
        Controls.Add(disposition)
    End Sub

    ''' <summary>Sources déjà parsées, dans l'ordre exact de l'instantané, sans identité ambiguë.</summary>
    Public Sub New(revue As PropositionRevue, propositions As IEnumerable(Of PropositionMouvement))
        Me.New(revue, propositions, Nothing)
    End Sub

    ''' <summary>Associe une session en memoire aux corrections effectuees dans la fenetre.
    ''' La session doit contenir les memes instances que l'instantane presente.
    ''' Aucune approbation ou persistance n'est possible depuis cette fenetre.</summary>
    Public Sub New(revue As PropositionRevue, propositions As IEnumerable(Of PropositionMouvement),
                   sessionRevue As PropositionSessionRevue)
        Me.New(revue)
        If propositions Is Nothing Then Throw New ArgumentNullException(NameOf(propositions))
        Dim liste = propositions.ToList()
        Dim identites As New HashSet(Of String)(StringComparer.Ordinal)
        If liste.Count <> revue.Lignes.Count Then Throw New ArgumentException("Sources incompatibles avec la revue.", NameOf(propositions))
        For i As Integer = 0 To liste.Count - 1
            Dim source = liste(i)
            Dim ligne = revue.Lignes(i)
            If source Is Nothing OrElse Not identites.Add(source.CleSource) OrElse
                source.Etat <> PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine OrElse
                Not String.Equals(source.CleSource, ligne.CleSource, StringComparison.Ordinal) OrElse
                Not String.Equals(source.TypeDocument, ligne.TypeDocument, StringComparison.Ordinal) OrElse
                source.DateProposee <> ligne.DateProposee OrElse source.Montant <> ligne.Montant OrElse
                source.Sens <> ligne.Sens OrElse Not String.Equals(source.Libelle, ligne.Libelle, StringComparison.Ordinal) OrElse
                Not String.Equals(source.Reference, ligne.Reference, StringComparison.Ordinal) OrElse
                Not source.AvertissementsValidation.SequenceEqual(ligne.AvertissementsValidation) Then
                Throw New ArgumentException("Correspondance source non prouvée.", NameOf(propositions))
            End If
        Next
        If sessionRevue IsNot Nothing Then
            If sessionRevue.Propositions.Count <> liste.Count Then
                Throw New ArgumentException("Session incompatible avec la revue.", NameOf(sessionRevue))
            End If
            For Each source In liste
                Dim courante As PropositionMouvement = Nothing
                If Not sessionRevue.Propositions.TryGetValue(source.CleSource, courante) OrElse
                   Not Object.ReferenceEquals(courante, source) Then
                    Throw New ArgumentException("Session incompatible avec la revue.", NameOf(sessionRevue))
                End If
            Next
        End If
        session = sessionRevue
        ActualiserEtatCorrections()
        Dim grille = DirectCast(Controls.Find("grillePropositions", True)(0), DataGridView)
        For i As Integer = 0 To liste.Count - 1
            sources.Add(grille.Rows(i), liste(i))
        Next
        ActualiserAction(grille)
    End Sub

    Private Sub ActualiserEtatCorrections()
        If session IsNot Nothing Then
            etatCorrections.Text = $"Corrections tracées en mémoire : {session.Audits.Count}. Aucune opération validée ni enregistrée."
        End If
    End Sub

    Private Sub ActualiserAction(grille As DataGridView)
        corriger.Enabled = grille.SelectedRows.Count = 1 AndAlso sources.ContainsKey(grille.SelectedRows(0))
    End Sub

    ''' <summary>Correction locale ; le présentateur optionnel permet les tests sans dialogue modal.</summary>
    Public Function CorrigerSelection(Optional afficher As Func(Of FrmCorrectionProposition, DialogResult) = Nothing) As Boolean
        Dim grille = DirectCast(Controls.Find("grillePropositions", True)(0), DataGridView)
        ActualiserAction(grille)
        If Not corriger.Enabled Then Return False
        Dim ligne = grille.SelectedRows(0)
        Dim source = sources(ligne)
        If source.Etat <> PropositionMouvement.EtatProposition.EnAttenteDeRevueHumaine Then Return False
        Using correction As New FrmCorrectionProposition(source)
            Dim resultat = If(afficher Is Nothing, correction.ShowDialog(Me), afficher(correction))
            If resultat <> DialogResult.OK OrElse correction.PropositionCorrigee Is Nothing Then Return False
            Dim proposition = correction.PropositionCorrigee
            If session IsNot Nothing Then session.AppliquerCorrection(source.CleSource, correction.ResultatCorrection)
            sources(ligne) = proposition
            ligne.Cells("DateProposee").Value = proposition.DateProposee
            ligne.Cells("Montant").Value = proposition.Montant
            ligne.Cells("Sens").Value = If(proposition.Sens = PropositionMouvement.SensMouvement.Debit, "Débit", "Crédit")
            ligne.Cells("Libelle").Value = proposition.Libelle
            ligne.Cells("Reference").Value = proposition.Reference
            ligne.Cells("Avertissements").Value = String.Join(Environment.NewLine, proposition.AvertissementsValidation)
            ActualiserEtatCorrections()
            Return True
        End Using
    End Function

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
