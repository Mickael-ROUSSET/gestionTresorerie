Imports System.Windows.Forms

Public Class FrmFilmEdit
    Private _film As Film

    Public Sub New(Optional film As Film = Nothing)
        InitializeComponent()
        _film = If(film, New Film())
    End Sub

    Private Sub FrmFilmEdit_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _film.IdFilm <> 0 Then
            txtTitre.Text = _film.Titre
            txtGenre.Text = _film.Genre
            txtRealisateur.Text = _film.Realisateur
            nudDuree.Value = _film.DureeMinutes
            txtSynopsis.Text = _film.Synopsis
        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        _film.Titre = txtTitre.Text
        _film.Genre = txtGenre.Text
        _film.Realisateur = txtRealisateur.Text
        _film.DureeMinutes = CInt(nudDuree.Value)
        _film.Synopsis = txtSynopsis.Text

        _film.Save()
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Friend WithEvents txtTitre As TextBox
    Friend WithEvents txtGenre As TextBox
    Friend WithEvents lblGenre As Label
    Friend WithEvents lblTitre As Label
    Friend WithEvents txtRealisateur As TextBox
    Friend WithEvents lblRealisateur As Label
    Friend WithEvents lblSynopsis As Label
    Friend WithEvents txtSynopsis As TextBox

    Private Sub InitializeComponent()
        txtTitre = New TextBox()
        txtGenre = New TextBox()
        lblGenre = New Label()
        lblTitre = New Label()
        txtRealisateur = New TextBox()
        lblRealisateur = New Label()
        lblSynopsis = New Label()
        txtSynopsis = New TextBox()
        btnOK = New Button()
        btnAnnuler = New Button()
        nudDuree = New NumericUpDown()
        lblDuree = New Label()
        CType(nudDuree, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' txtTitre
        ' 
        txtTitre.Location = New Point(146, 75)
        txtTitre.Name = "txtTitre"
        txtTitre.Size = New Size(285, 23)
        txtTitre.TabIndex = 0
        ' 
        ' txtGenre
        ' 
        txtGenre.Location = New Point(146, 117)
        txtGenre.Name = "txtGenre"
        txtGenre.Size = New Size(285, 23)
        txtGenre.TabIndex = 1
        ' 
        ' lblGenre
        ' 
        lblGenre.AutoSize = True
        lblGenre.Location = New Point(45, 117)
        lblGenre.Name = "lblGenre"
        lblGenre.Size = New Size(38, 15)
        lblGenre.TabIndex = 2
        lblGenre.Text = "Genre"
        ' 
        ' lblTitre
        ' 
        lblTitre.AutoSize = True
        lblTitre.Location = New Point(45, 75)
        lblTitre.Name = "lblTitre"
        lblTitre.Size = New Size(31, 15)
        lblTitre.TabIndex = 3
        lblTitre.Text = "Titre"
        ' 
        ' txtRealisateur
        ' 
        txtRealisateur.Location = New Point(146, 159)
        txtRealisateur.Name = "txtRealisateur"
        txtRealisateur.Size = New Size(285, 23)
        txtRealisateur.TabIndex = 4
        ' 
        ' lblRealisateur
        ' 
        lblRealisateur.AutoSize = True
        lblRealisateur.Location = New Point(45, 159)
        lblRealisateur.Name = "lblRealisateur"
        lblRealisateur.Size = New Size(64, 15)
        lblRealisateur.TabIndex = 5
        lblRealisateur.Text = "Realisateur"
        ' 
        ' lblSynopsis
        ' 
        lblSynopsis.AutoSize = True
        lblSynopsis.Location = New Point(45, 201)
        lblSynopsis.Name = "lblSynopsis"
        lblSynopsis.Size = New Size(53, 15)
        lblSynopsis.TabIndex = 6
        lblSynopsis.Text = "Synopsis"
        ' 
        ' txtSynopsis
        ' 
        txtSynopsis.AcceptsReturn = True
        txtSynopsis.AcceptsTab = True
        txtSynopsis.AllowDrop = True
        txtSynopsis.Location = New Point(146, 201)
        txtSynopsis.Multiline = True
        txtSynopsis.Name = "txtSynopsis"
        txtSynopsis.ScrollBars = ScrollBars.Both
        txtSynopsis.Size = New Size(579, 23)
        txtSynopsis.TabIndex = 7
        ' 
        ' btnOK
        ' 
        btnOK.Location = New Point(45, 307)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(75, 23)
        btnOK.TabIndex = 8
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' btnAnnuler
        ' 
        btnAnnuler.Location = New Point(146, 307)
        btnAnnuler.Name = "btnAnnuler"
        btnAnnuler.Size = New Size(75, 23)
        btnAnnuler.TabIndex = 9
        btnAnnuler.Text = "Annuler"
        btnAnnuler.UseVisualStyleBackColor = True
        ' 
        ' nudDuree
        ' 
        nudDuree.Location = New Point(146, 243)
        nudDuree.Name = "nudDuree"
        nudDuree.Size = New Size(120, 23)
        nudDuree.TabIndex = 10
        ' 
        ' lblDuree
        ' 
        lblDuree.AutoSize = True
        lblDuree.Location = New Point(45, 243)
        lblDuree.Name = "lblDuree"
        lblDuree.Size = New Size(38, 15)
        lblDuree.TabIndex = 11
        lblDuree.Text = "Durée"
        ' 
        ' FrmFilmEdit
        ' 
        ClientSize = New Size(811, 422)
        Controls.Add(lblDuree)
        Controls.Add(nudDuree)
        Controls.Add(btnAnnuler)
        Controls.Add(btnOK)
        Controls.Add(txtSynopsis)
        Controls.Add(lblSynopsis)
        Controls.Add(lblRealisateur)
        Controls.Add(txtRealisateur)
        Controls.Add(lblTitre)
        Controls.Add(lblGenre)
        Controls.Add(txtGenre)
        Controls.Add(txtTitre)
        Name = "FrmFilmEdit"
        Text = "FrmFilmEdit"
        CType(nudDuree, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnnuler As Button
    Friend WithEvents nudDuree As NumericUpDown
    Friend WithEvents lblDuree As Label
End Class
