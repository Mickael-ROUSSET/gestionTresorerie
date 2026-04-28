Imports System.Security.Cryptography
Imports System.Text
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class FrmEditUtilisateur
    Private _id As Integer? = Nothing

    ' Constructeur pour ajout
    Public Sub New()
        InitializeComponent()
        Text = "Nouvel utilisateur"
    End Sub

    ' Constructeur pour modification
    Public Sub New(id As Integer, nom As String, role As String, actif As Boolean)
        InitializeComponent()
        Text = "Modifier l'utilisateur"
        _id = id
        txtNom.Text = nom
        cboRole.Text = role
        chkActif.Checked = actif
    End Sub
    Private Shared Function CreateUtilisateurRepository() As UtilisateurRepository
        Dim executor As ISqlExecutor =
    RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New UtilisateurRepository(executor)
    End Function
    Private Sub FrmEditUtilisateur_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cboRole.Items.Clear()
        cboRole.Items.AddRange({"lecteur", "ecrivain", "admin"})
        cboRole.SelectedIndex = 0
    End Sub

    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        If String.IsNullOrWhiteSpace(txtNom.Text) Then
            Dim unused3 = MessageBox.Show("Nom requis.")
            Exit Sub
        End If

        Dim hashMdp As String = Nothing
        If Not String.IsNullOrWhiteSpace(txtMdp.Text) Then
            hashMdp = HacherMotDePasse(txtMdp.Text)
        End If

        Try
            If _id Is Nothing Then
                Dim nb As Integer =
                CreateUtilisateurRepository().InsererUtilisateur(
                    txtNom.Text.Trim(),
                    hashMdp,
                    cboRole.Text,
                    chkActif.Checked)

                Logger.INFO($"Utilisateur inséré, lignes={nb}")
            Else
                Dim nb As Integer =
                CreateUtilisateurRepository().MettreAJourUtilisateur(
                    CInt(_id),
                    txtNom.Text.Trim(),
                    hashMdp,
                    cboRole.Text,
                    chkActif.Checked)

                Logger.INFO($"Utilisateur mis à jour Id={_id}, lignes={nb}")
            End If

            DialogResult = DialogResult.OK
            Close()

        Catch ex As Exception
            Dim unused = MessageBox.Show("Erreur lors de l'enregistrement : " & ex.Message)
        End Try
    End Sub

    Private Function HacherMotDePasse(mdp As String) As String
        Using sha = SHA256.Create()
            Dim bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(mdp))
            Return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant()
        End Using
    End Function

    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        Close()
    End Sub
End Class
