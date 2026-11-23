Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.SqlClient

Public Class FrmEditUtilisateur
    Friend Property Connexion As String
    Private roleInitial As String = "lecteur"
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
        roleInitial = role
        chkActif.Checked = actif
    End Sub
    Private Sub FormLoad() Handles MyBase.Load
        cboRole.DropDownStyle = ComboBoxStyle.DropDownList
        cboRole.Items.AddRange({"lecteur","ecrivain","admin"})
        cboRole.SelectedItem = roleInitial
        If Not _id.HasValue Then chkActif.Checked=True
        Button1.Visible=False
        txtMdp.UseSystemPasswordChar=True
        txtMdp.PlaceholderText=If(_id.HasValue,"Conserver","Obligatoire")
        For Each paire In {(txtNom.Top,"Nom"),(txtMdp.Top,"Mot de passe"),(cboRole.Top,"Rôle")}
            Controls.Add(New Label With {.Text=paire.Item2,.Location=New Point(2,paire.Item1),.AutoSize=True})
        Next
    End Sub

    Friend Shared Sub Enregistrer(cs As String, id As Integer?, nom As String, role As String, actif As Boolean, mdp As String)
        If String.IsNullOrWhiteSpace(nom) OrElse Not {"admin","lecteur","ecrivain"}.Contains(role) OrElse (Not id.HasValue AndAlso String.IsNullOrWhiteSpace(mdp)) Then Throw New ArgumentException("Renseignez le nom, le rôle et le mot de passe à la création.")
        Using cn As New SqlConnection(cs)
            cn.Open()
            Using tx=cn.BeginTransaction(IsolationLevel.Serializable), cmd=cn.CreateCommand()
                cmd.Transaction=tx
                cmd.CommandText="IF EXISTS(SELECT 1 FROM dbo.Utilisateurs WHERE NomUtilisateur=@nom AND (@id IS NULL OR Id<>@id)) THROW 51070,N'Nom déjà utilisé.',1; " &
                    "IF @id IS NULL INSERT dbo.Utilisateurs(NomUtilisateur,Role,Actif,MotDePasse) VALUES(@nom,@role,@actif,@pwd) ELSE BEGIN " &
                    "IF EXISTS(SELECT 1 FROM dbo.Utilisateurs WHERE Id=@id AND Role='admin' AND Actif=1) AND (@role<>'admin' OR @actif=0) AND NOT EXISTS(SELECT 1 FROM dbo.Utilisateurs WHERE Id<>@id AND Role='admin' AND Actif=1) THROW 51071,N'Dernier administrateur actif.',1; " &
                    "UPDATE dbo.Utilisateurs SET NomUtilisateur=@nom,Role=@role,Actif=@actif,MotDePasse=COALESCE(@pwd,MotDePasse) WHERE Id=@id; IF @@ROWCOUNT<>1 THROW 51072,N'Utilisateur introuvable.',1; END;"
                cmd.Parameters.Add("@id",SqlDbType.Int).Value=If(id.HasValue,CObj(id.GetValueOrDefault()),DBNull.Value)
                cmd.Parameters.AddWithValue("@nom",nom.Trim())
                cmd.Parameters.AddWithValue("@role",role)
                cmd.Parameters.AddWithValue("@actif",actif)
                cmd.Parameters.Add("@pwd",SqlDbType.NVarChar,256).Value=If(String.IsNullOrEmpty(mdp),CObj(DBNull.Value),Encoder(mdp))
                cmd.ExecuteNonQuery()
                tx.Commit()
            End Using
        End Using
    End Sub

    Friend Shared Function Encoder(mdp As String) As String
        Dim sel=RandomNumberGenerator.GetBytes(12)
        Return "v2$" & Convert.ToBase64String(sel.Concat(Rfc2898DeriveBytes.Pbkdf2(mdp,sel,600000,HashAlgorithmName.SHA256,32)).ToArray())
    End Function

    Friend Shared Function Verifier(mdp As String, empreinte As String) As Boolean
        Dim valeurs=empreinte.Split("$"c)
        If valeurs.Length=2 AndAlso valeurs(0)="v2" Then
            Dim octets=Convert.FromBase64String(valeurs(1))
            Return octets.Length=44 AndAlso CryptographicOperations.FixedTimeEquals(octets.Skip(12).ToArray(),Rfc2898DeriveBytes.Pbkdf2(mdp,octets.Take(12).ToArray(),600000,HashAlgorithmName.SHA256,32))
        End If
        Return String.Equals(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(mdp))),empreinte,StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub Valider() Handles btnValider.Click
        Try
            Enregistrer(Connexion,_id,txtNom.Text,cboRole.Text,chkActif.Checked,txtMdp.Text)
            DialogResult=DialogResult.OK
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,Text)
        End Try
    End Sub
    Private Sub btnAnnuler_Click(sender As Object, e As EventArgs) Handles btnAnnuler.Click
        Close()
    End Sub
End Class
