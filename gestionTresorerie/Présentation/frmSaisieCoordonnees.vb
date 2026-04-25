Imports System.Text.RegularExpressions

Public Class FrmSaisieCoordonnees
    Public Property Result As Coordonnees
    Public Property _idTiers As Integer      ' transmis par frmSaisie
    Private _coord As Coordonnees           ' l’objet chargé en base

    ' Indique si la validation doit persister en base (true par défaut)
    Public Property EnregistrerOnValidate As Boolean = True

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(idTiers As Integer, Optional enregistrerOnValidate As Boolean = True)
        InitializeComponent()
        _idTiers = idTiers
        Me.EnregistrerOnValidate = enregistrerOnValidate
    End Sub
    Private Shared Function CreateAdresseRepository() As AdresseRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).
                    GetConnexion(Constantes.DataBases.Agumaaa).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New AdresseRepository(executor)
    End Function

    Private Sub frmSaisieCoordonnees_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _idTiers > 0 Then
            ChargerCoordonnees()
        End If
    End Sub

    Private Sub ChargerCoordonnees()
        Try
            Dim lstCoord As List(Of Coordonnees) =
            CreateAdresseRepository().LireParTiers(_idTiers)

            If lstCoord Is Nothing OrElse lstCoord.Count = 0 Then
                _coord = New Coordonnees() With {.IdTiers = _idTiers}
            Else
                _coord = lstCoord(0)
                If _coord Is Nothing Then
                    _coord = New Coordonnees() With {.IdTiers = _idTiers}
                End If
            End If

            PopulateFormFromCoord(_coord)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des coordonnées : {ex.Message}")
        End Try
    End Sub

    ' Construit un objet Coordonnees depuis les champs du formulaire
    Private Function BuildCoordFromForm() As Coordonnees
        Return New Coordonnees With {
            .Rue1 = txtRue1.Text,
            .Rue2 = txtRue2.Text,
            .CodePostal = txtCodePostal.Text,
            .Ville = If(cbVilles.SelectedItem IsNot Nothing, cbVilles.SelectedItem.ToString(), cbVilles.Text),
            .Pays = txtPays.Text,
            .Email = txtEmail.Text,
            .Telephone = txtTelephone.Text,
            .IdTiers = If(_coord IsNot Nothing, _coord.IdTiers, _idTiers),
            .Id = If(_coord IsNot Nothing, _coord.Id, 0)
        }
    End Function

    ' Remplit les contrôles à partir d'un Coordonnees
    Private Sub PopulateFormFromCoord(coord As Coordonnees)
        txtRue1.Text = If(coord.Rue1, String.Empty)
        txtRue2.Text = If(coord.Rue2, String.Empty)
        txtCodePostal.Text = If(coord.CodePostal, String.Empty)
        txtPays.Text = If(coord.Pays, String.Empty)
        txtTelephone.Text = If(coord.Telephone, String.Empty)
        txtEmail.Text = If(coord.Email, String.Empty)

        ' Alimentation de la commune
        cbVilles.Items.Clear()
        Dim ville As String = If(coord.Ville, String.Empty)
        If ville <> String.Empty Then
            cbVilles.Items.Add(ville)
            cbVilles.SelectedItem = ville
        Else
            cbVilles.SelectedIndex = -1
        End If
    End Sub

    ' Centralise le flot : validation, optionnellement sauvegarde, renvoi du résultat et fermeture
    Private Sub ProcessAndClose(coordonnee As Coordonnees, Optional persist As Boolean = True)
        ClearInvalid()
        Try
            coordonnee.Validate()

            If persist Then
                Save(coordonnee)
            End If

            Result = coordonnee
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ArgumentException
            HighlightInvalidField(ex.Message)
            MessageBox.Show(ex.Message, "Champ invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.ERR($"Erreur : {ex.Message}")
        End Try
    End Sub

    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Dim coordonnee = BuildCoordFromForm()
        ' Respecte la préférence d'instance EnregistrerOnValidate
        ProcessAndClose(coordonnee, persist:=EnregistrerOnValidate)
    End Sub

    ' Nouveau bouton : Retour sans enregistrer
    ' Traitement identique à Valider sauf l'appel à Save(coordonnee)
    Private Sub btnRetourSansEnregistrer_Click(sender As Object, e As EventArgs) Handles btnRetourSansEnregistrer.Click
        Dim coordonnee = BuildCoordFromForm()
        ProcessAndClose(coordonnee, persist:=False)
    End Sub

    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    ' --- Mise en surbrillance d’un champ invalide ---
    Private Sub HighlightInvalidField(message As String)
        If message.Contains("Email") Then
            txtEmail.BackColor = Color.LightCoral
        End If

        If message.Contains("Téléphone") Then
            txtTelephone.BackColor = Color.LightCoral
        End If
    End Sub

    ' Réinitialiser les couleurs
    Private Sub ClearInvalid()
        txtEmail.BackColor = Color.White
        txtTelephone.BackColor = Color.White
    End Sub

    ' ─────────── INSERTION EN BASE ───────────
    Public Sub Save(coordonnee As Coordonnees)
        If coordonnee Is Nothing Then
            Throw New ArgumentNullException(NameOf(coordonnee))
        End If

        coordonnee.Validate()

        Dim repo As AdresseRepository = CreateAdresseRepository()

        If coordonnee.Id = 0 Then
            coordonnee.Id = repo.Inserer(coordonnee)
            Logger.INFO($"Coordonnée insérée pour IdTiers={coordonnee.IdTiers}, Id={coordonnee.Id}")
        Else
            Dim nb As Integer = repo.MettreAJour(coordonnee)
            Logger.INFO($"Coordonnée mise à jour pour IdTiers={coordonnee.IdTiers}, lignes affectées={nb}")
        End If

        _coord = coordonnee
    End Sub

    Public Function GetVillesByCodePostal(cp As String) As List(Of String)
        If String.IsNullOrWhiteSpace(cp) OrElse cp.Length < 4 Then
            Return New List(Of String)
        End If

        Try
            Return CreateAdresseRepository().LireVillesParCodePostal(cp)
        Catch ex As Exception
            Logger.ERR($"Erreur GetVillesByCodePostal {ex.Message}")
            Return New List(Of String)
        End Try
    End Function

    Private Sub ChargeNomsCommunes(codePostal As String)
        Dim villes = GetVillesByCodePostal(codePostal)
        Dim cp As String = txtCodePostal.Text.Trim()

        ' --- Validation rapide du CP ---
        If cp.Length < 4 OrElse Not Regex.IsMatch(cp, "^\d{4,5}$") Then
            cbVilles.DataSource = Nothing
            Return
        End If
        cbVilles.DataSource = Nothing
        If villes IsNot Nothing AndAlso villes.Count > 0 Then
            cbVilles.DataSource = villes
        Else
            ' Si aucune ville trouvée, on garde la saisie libre
            cbVilles.Text = ""
        End If
    End Sub

    Private Sub txtCodePostal_LostFocus(sender As Object, e As EventArgs) Handles txtCodePostal.LostFocus
        ChargeNomsCommunes(txtCodePostal.Text)
    End Sub
End Class
