Imports System.Text.RegularExpressions
Imports System.Drawing

Public Class FrmSaisieCoordonnees

    Public Property Result As Coordonnees
    Private _idTiers As Integer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(idTiers As Integer)
        InitializeComponent()
        _idTiers = idTiers
    End Sub
    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click

        ' Effacement des surbrillances précédentes
        ClearInvalid()

        Dim coordonnee As New Coordonnees With {
            .TypeAdresse = txtTypeAdresse.Text,
            .Rue1 = txtRue1.Text,
            .Rue2 = txtRue2.Text,
            .CodePostal = txtCodePostal.Text,
            .Ville = txtVille.Text,
            .Pays = txtPays.Text,
            .Email = txtEmail.Text,
            .Telephone = txtTelephone.Text
        }

        Try
            ' Validation des emails et téléphones
            coordonnee.Validate()

            ' Si valide, on renvoie l'objet
            Result = coordonnee
            DialogResult = DialogResult.OK
            Close()

        Catch ex As ArgumentException
            ' Si erreur spécifique d'un champ → mise en surbrillance
            HighlightInvalidField(ex.Message)
            MessageBox.Show(ex.Message, "Champ invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        Catch ex As Exception
            MessageBox.Show("Erreur : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
        Dim id As Integer

        Dim parametres As New Dictionary(Of String, Object)
        With coordonnee
            parametres.Add("@IdTiers", .IdTiers)
            parametres.Add("@TypeAdresse", .TypeAdresse)
            parametres.Add("@Rue1", .Rue1)
            parametres.Add("@Rue2", .Rue2)
            parametres.Add("@CodePostal", .CodePostal)
            parametres.Add("@Ville", .Ville)
            parametres.Add("@Pays", .Pays)
            parametres.Add("@Email", If(.Email, DBNull.Value))
            parametres.Add("@Telephone", If(.Telephone, DBNull.Value))
        End With

        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "insertCoordonnees", parametres)
            'Récupération SCOPE_IDENTITY() 
            id = Convert.ToInt32(cmd.ExecuteScalar())
        End Using
    End Sub
End Class
