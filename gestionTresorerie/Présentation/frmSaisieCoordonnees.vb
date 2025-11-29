Imports System.Text.RegularExpressions

Public Class FrmSaisieCoordonnees
    Public Property Result As Coordonnees
    Public Property _idTiers As Integer      ' transmis par frmSaisie
    Private _coord As Coordonnees           ' l’objet chargé en base

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(idTiers As Integer)
        InitializeComponent()
        _idTiers = idTiers
    End Sub
    Private Sub frmSaisieCoordonnees_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ChargerCoordonnees()
    End Sub
    Private Sub ChargerCoordonnees()
        Try
            ' --- Préparer le paramètre pour la requête ---
            Dim parametres As New Dictionary(Of String, Object) From {
                {"@IdTiers", _idTiers}
            }
            ' Récupérer les coordonnées du tiers (renvoie Nothing si aucune)
            Dim lstCoord As List(Of Coordonnees) = SqlCommandBuilder.GetEntities(Of Coordonnees)(Constantes.bddAgumaaa, "selCoordonneesByIdTiers", parametres)
            'Une seule coordonnées est présente
            _coord = lstCoord(0)
            If _coord Is Nothing Then
                ' Le tiers n'a pas de coordonnées → créer un objet vide
                _coord = New Coordonnees() With {.IdTiers = _idTiers}
            End If

            ' Remplir les champs du formulaire
            txtRue1.Text = If(_coord.Rue1, "")
            txtRue2.Text = If(_coord.Rue2, "")
            txtCodePostal.Text = If(_coord.CodePostal, "")
            txtPays.Text = If(_coord.Pays, "")
            txtTelephone.Text = If(_coord.Telephone, "")
            txtEmail.Text = If(_coord.Email, "")
            'Alimentation de la commune
            cbVilles.Items.Clear()
            Dim ville As String = If(_coord.Ville, "")
            If ville <> "" Then
                cbVilles.Items.Add(ville)
                cbVilles.SelectedItem = ville
            Else
                cbVilles.SelectedIndex = -1
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des coordonnées : {ex.Message}")
        End Try
    End Sub
    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click

        ' Effacement des surbrillances précédentes
        ClearInvalid()

        Dim coordonnee As New Coordonnees With {
            .Rue1 = txtRue1.Text,
            .Rue2 = txtRue2.Text,
            .CodePostal = txtCodePostal.Text,
            .Ville = cbVilles.SelectedItem,
            .Pays = txtPays.Text,
            .Email = txtEmail.Text,
            .Telephone = txtTelephone.Text
        }

        Try
            ' Validation des emails et téléphones
            coordonnee.Validate()

            ' Si valide, on sauve et on renvoie l'objet
            Save(coordonnee)
            Result = coordonnee
            DialogResult = DialogResult.OK
            Close()

        Catch ex As ArgumentException
            ' Si erreur spécifique d'un champ → mise en surbrillance
            HighlightInvalidField(ex.Message)
            MessageBox.Show(ex.Message, "Champ invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.ERR($"Erreur : {ex.Message}")
        End Try
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

        Validate()

        Dim parametres As New Dictionary(Of String, Object)

        With coordonnee
            parametres.Add("@Id", _coord.Id)
            parametres.Add("@IdTiers", _coord.IdTiers)
            parametres.Add("@Rue1", .Rue1)
            parametres.Add("@Rue2", .Rue2)
            parametres.Add("@CodePostal", .CodePostal)
            parametres.Add("@Ville", .Ville)
            parametres.Add("@Pays", .Pays)
            parametres.Add("@Email", If(String.IsNullOrWhiteSpace(.Email), DBNull.Value, .Email))
            parametres.Add("@Telephone", If(String.IsNullOrWhiteSpace(.Telephone), DBNull.Value, .Telephone))
        End With

        Dim reqSqlCoord As String = If(_coord.Id = 0, "insertCoordonnees", "updateCoordonnees")

        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, reqSqlCoord, parametres)

            ' INSERT uniquement → récupère SCOPE_IDENTITY()
            If coordonnee.Id = 0 Then
                coordonnee.Id = Convert.ToInt32(cmd.ExecuteScalar())
            Else
                cmd.ExecuteNonQuery()
            End If
        End Using
    End Sub
    Public Function GetVillesByCodePostal(cp As String) As List(Of String)
        Dim result As New List(Of String)

        If String.IsNullOrWhiteSpace(cp) OrElse cp.Length < 4 Then
            Return result
        End If

        Try
            Using reader = SqlCommandBuilder.
                CreateSqlCommand(Constantes.bddAgumaaa, "selVillesParCP",
                New Dictionary(Of String, Object) From {{"@CodePostal", cp}}
            ).ExecuteReader()
                While reader.Read()
                    result.Add(reader.GetString(0))
                End While
            End Using
        Catch ex As Exception
            Logger.ERR($"Erreur GetVillesByCodePostal {ex.Message}")
        End Try

        Return result
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
