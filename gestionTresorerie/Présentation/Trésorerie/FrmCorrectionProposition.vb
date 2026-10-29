Option Strict On
Option Explicit On

Imports System
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

''' <summary>Correction en mémoire uniquement ; aucune approbation ni persistance.</summary>
Public NotInheritable Class FrmCorrectionProposition
    Inherits Form

    Private ReadOnly source As PropositionMouvement
    Private ReadOnly dateSaisie As New TextBox With {.Name = "dateProposee"}
    Private ReadOnly montantSaisi As New TextBox With {.Name = "montant"}
    Private ReadOnly sensSaisi As New ComboBox With {.Name = "sens", .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly libelleSaisi As New TextBox With {.Name = "libelle"}
    Private ReadOnly referenceSaisie As New TextBox With {.Name = "reference"}
    Private ReadOnly erreur As New Label With {.Name = "erreurValidation", .AutoSize = True, .ForeColor = Color.DarkRed}
    Private correction As PropositionCorrection.Resultat

    Public Sub New(proposition As PropositionMouvement)
        If proposition Is Nothing Then Throw New ArgumentNullException(NameOf(proposition))
        source = proposition
        Text = "Correction — sans approbation"
        AccessibleName = Text
        ClientSize = New Size(720, 440)
        AutoScaleMode = AutoScaleMode.Font
        StartPosition = FormStartPosition.CenterParent

        Dim disposition As New TableLayoutPanel With {
            .Dock = DockStyle.Fill, .AutoScroll = True, .ColumnCount = 2, .Padding = New Padding(12)}
        disposition.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        disposition.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        Controls.Add(disposition)
        Ajouter(disposition, "Clé source", New Label With {.Name = "cleSource", .Text = source.CleSource, .AutoSize = True, .UseMnemonic = False})
        Ajouter(disposition, "Type de document", New Label With {.Name = "typeDocument", .Text = source.TypeDocument, .AutoSize = True, .UseMnemonic = False})
        dateSaisie.Text = source.DateProposee.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture)
        montantSaisi.Text = source.Montant.ToString(CultureInfo.CurrentCulture)
        sensSaisi.Items.AddRange(New Object() {"Débit", "Crédit"})
        sensSaisi.SelectedIndex = CInt(source.Sens)
        libelleSaisi.Text = source.Libelle
        referenceSaisie.Text = source.Reference
        Ajouter(disposition, "Date (aaaa-MM-jj, heure facultative)", dateSaisie)
        Ajouter(disposition, "Montant strictement positif", montantSaisi)
        Ajouter(disposition, "Sens", sensSaisi)
        Ajouter(disposition, "Libellé", libelleSaisi)
        Ajouter(disposition, "Référence", referenceSaisie)
        Ajouter(disposition, "État", New Label With {.Text = "En attente de revue humaine — aucune approbation", .AutoSize = True})
        Ajouter(disposition, "Validation", erreur)
        Dim boutons As New FlowLayoutPanel With {.AutoSize = True, .Dock = DockStyle.Fill}
        Dim appliquer As New Button With {.Name = "boutonAppliquer", .Text = "Appliquer la correction", .AutoSize = True}
        Dim annuler As New Button With {.Name = "boutonAnnuler", .Text = "Annuler", .AutoSize = True, .CausesValidation = False}
        AddHandler appliquer.Click, Sub()
                                        If AppliquerCorrection() Then
                                            DialogResult = DialogResult.OK
                                            Close()
                                        End If
                                    End Sub
        AddHandler annuler.Click, Sub() AnnulerCorrection()
        boutons.Controls.AddRange(New Control() {appliquer, annuler})
        Ajouter(disposition, "", boutons)
        AcceptButton = appliquer
        CancelButton = annuler
    End Sub

    Public ReadOnly Property ResultatCorrection As PropositionCorrection.Resultat
        Get
            Return correction
        End Get
    End Property

    Public ReadOnly Property PropositionCorrigee As PropositionMouvement
        Get
            Return If(correction Is Nothing, Nothing, correction.Proposition)
        End Get
    End Property

    ''' <summary>Valide sans dialogue bloquant. Les avertissements restent tous présents.</summary>
    Public Function AppliquerCorrection() As Boolean
        correction = Nothing
        erreur.Text = ""
        Dim dateValeur As DateTime
        If Not DateTime.TryParseExact(dateSaisie.Text, {"yyyy-MM-dd", "yyyy-MM-dd HH:mm",
                                                  "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fffffff"},
                                      CultureInfo.InvariantCulture, DateTimeStyles.None, dateValeur) Then
            erreur.Text = "Date invalide : saisir aaaa-MM-jj."
            Return False
        End If
        Dim montantValeur As Decimal
        If Not Decimal.TryParse(montantSaisi.Text, NumberStyles.Number, CultureInfo.CurrentCulture, montantValeur) Then
            erreur.Text = "Montant invalide."
            Return False
        End If
        Try
            correction = PropositionCorrection.Corriger(source, dateValeur, montantValeur,
                CType(sensSaisi.SelectedIndex, PropositionMouvement.SensMouvement), libelleSaisi.Text,
                If(source.Reference Is Nothing AndAlso referenceSaisie.Text.Length = 0, Nothing, referenceSaisie.Text))
            Return True
        Catch ex As ArgumentException
            erreur.Text = ex.Message
            Return False
        End Try
    End Function

    Public Sub AnnulerCorrection()
        correction = Nothing
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If DialogResult <> DialogResult.OK Then correction = Nothing
        MyBase.OnFormClosing(e)
    End Sub

    Private Shared Sub Ajouter(disposition As TableLayoutPanel, titre As String, controle As Control)
        Dim ligne = disposition.RowCount
        disposition.RowCount += 1
        disposition.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        disposition.Controls.Add(New Label With {.Text = titre, .AutoSize = True, .Anchor = AnchorStyles.Left}, 0, ligne)
        controle.Dock = DockStyle.Fill
        controle.AccessibleName = titre
        disposition.Controls.Add(controle, 1, ligne)
    End Sub
End Class
