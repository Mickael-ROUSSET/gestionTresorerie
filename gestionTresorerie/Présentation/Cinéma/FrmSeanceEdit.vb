Imports System.Data.SqlClient
Imports System.Windows.Forms
Imports DocumentFormat.OpenXml.VariantTypes
Imports FluentAssertions

Public Class FrmSeanceEdit
    Private _seance As Seance

    Public Sub New(Optional seance As Seance = Nothing)
        InitializeComponent()
        _seance = If(seance, New Seance())
    End Sub

    Private Sub FrmSeanceEdit_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ChargerFilms()

        If _seance.IdSeance <> 0 Then
            cboFilm.SelectedValue = _seance.IdFilm
            dtpDate.Value = _seance.DateHeureDebut
            nudTarif.Value = _seance.TarifBase
            txtLangue.Text = _seance.Langue
            txtFormat.Text = _seance.Format
        End If
    End Sub

    Private Sub ChargerFilms()
        cboFilm.DisplayMember = "Titre"
        cboFilm.ValueMember = "IdFilm"
        cboFilm.DataSource = Film.GetAll()
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        _seance.IdFilm = CInt(cboFilm.SelectedValue)
        _seance.DateHeureDebut = dtpDate.Value
        _seance.TarifBase = nudTarif.Value
        _seance.Langue = txtLangue.Text
        _seance.Format = txtFormat.Text

        _seance.Save()
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub InitializeComponent()
        cboFilm = New ComboBox()
        dtpDate = New DateTimePicker()
        nudTarif = New NumericUpDown()
        txtLangue = New TextBox()
        lblLangue = New Label()
        lblFormat = New Label()
        txtFormat = New TextBox()
        btnOK = New Button()
        btnAnnuler = New Button()
        lblFilm = New Label()
        lblDate = New Label()
        lblTarif = New Label()
        CType(nudTarif, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' cboFilm
        ' 
        cboFilm.FormattingEnabled = True
        cboFilm.Location = New Point(231, 40)
        cboFilm.Name = "cboFilm"
        cboFilm.Size = New Size(121, 23)
        cboFilm.TabIndex = 0
        ' 
        ' dtpDate
        ' 
        dtpDate.Location = New Point(231, 87)
        dtpDate.Name = "dtpDate"
        dtpDate.Size = New Size(200, 23)
        dtpDate.TabIndex = 1
        ' 
        ' nudTarif
        ' 
        nudTarif.Location = New Point(231, 130)
        nudTarif.Name = "nudTarif"
        nudTarif.Size = New Size(120, 23)
        nudTarif.TabIndex = 2
        ' 
        ' txtLangue
        ' 
        txtLangue.Location = New Point(139, 180)
        txtLangue.Name = "txtLangue"
        txtLangue.Size = New Size(100, 23)
        txtLangue.TabIndex = 3
        ' 
        ' lblLangue
        ' 
        lblLangue.AutoSize = True
        lblLangue.Location = New Point(60, 182)
        lblLangue.Name = "lblLangue"
        lblLangue.Size = New Size(46, 15)
        lblLangue.TabIndex = 4
        lblLangue.Text = "Langue"
        ' 
        ' lblFormat
        ' 
        lblFormat.AutoSize = True
        lblFormat.Location = New Point(63, 219)
        lblFormat.Name = "lblFormat"
        lblFormat.Size = New Size(45, 15)
        lblFormat.TabIndex = 5
        lblFormat.Text = "Format"
        ' 
        ' txtFormat
        ' 
        txtFormat.Location = New Point(153, 224)
        txtFormat.Name = "txtFormat"
        txtFormat.Size = New Size(100, 23)
        txtFormat.TabIndex = 6
        ' 
        ' btnOK
        ' 
        btnOK.Location = New Point(60, 275)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(75, 23)
        btnOK.TabIndex = 7
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(200, 277)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 8
        btnAnnuler.Text = "Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' lblFilm
        ' 
        lblFilm.AutoSize = True
        lblFilm.Location = New Point(61, 44)
        lblFilm.Name = "lblFilm"
        lblFilm.Size = New Size(30, 15)
        lblFilm.TabIndex = 9
        lblFilm.Text = "Film"
        ' 
        ' lblDate
        ' 
        lblDate.AutoSize = True
        lblDate.Location = New Point(60, 93)
        lblDate.Name = "lblDate"
        lblDate.Size = New Size(31, 15)
        lblDate.TabIndex = 10
        lblDate.Text = "Date"
        ' 
        ' lblTarif
        ' 
        lblTarif.AutoSize = True
        lblTarif.Location = New Point(69, 136)
        lblTarif.Name = "lblTarif"
        lblTarif.Size = New Size(30, 15)
        lblTarif.TabIndex = 11
        lblTarif.Text = "Tarif"
        ' 
        ' FrmSeanceEdit
        ' 
        ClientSize = New Size(496, 341)
        Controls.Add(lblTarif)
        Controls.Add(lblDate)
        Controls.Add(lblFilm)
        Controls.Add(btnAnnuler)
        Controls.Add(btnOK)
        Controls.Add(txtFormat)
        Controls.Add(lblFormat)
        Controls.Add(lblLangue)
        Controls.Add(txtLangue)
        Controls.Add(nudTarif)
        Controls.Add(dtpDate)
        Controls.Add(cboFilm)
        Name = "FrmSeanceEdit"
        Text = "Édition de la séance"
        CType(nudTarif, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Friend WithEvents cboFilm As ComboBox
    Friend WithEvents dtpDate As DateTimePicker
    Friend WithEvents nudTarif As NumericUpDown
    Friend WithEvents txtLangue As TextBox
    Friend WithEvents lblLangue As Label
    Friend WithEvents lblFormat As Label
    Friend WithEvents txtFormat As TextBox
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnnuler As Button
    Friend WithEvents lblFilm As Label
    Friend WithEvents lblDate As Label
    Friend WithEvents lblTarif As Label
End Class
