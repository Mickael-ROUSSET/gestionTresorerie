Imports System.Data.OleDb
Imports System.IO
Public Class frmConfiguration
    'Private Sub FormConfiguration_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    '    txtCheminBDD.Text = My.Settings.ficBddDonnees
    '    txtDossierRacine.Text = My.Settings.repRacineAgumaaa
    'End Sub

    'Private Sub btnParcourirBDD_Click(sender As Object, e As EventArgs) Handles btnParcourirBDD.Click
    '    Using sfd As New SaveFileDialog()
    '        sfd.Filter = "Base Access (*.mdb)|*.mdb"
    '        sfd.FileName = "TreasuryData.mdb"
    '        If sfd.ShowDialog() = DialogResult.OK Then
    '            txtCheminBDD.Text = sfd.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnParcourirDossier_Click(sender As Object, e As EventArgs) Handles btnParcourirDossier.Click
    '    Using fbd As New FolderBrowserDialog()
    '        If fbd.ShowDialog() = DialogResult.OK Then
    '            txtDossierRacine.Text = fbd.SelectedPath
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
    '    If String.IsNullOrWhiteSpace(txtCheminBDD.Text) Or
    '       String.IsNullOrWhiteSpace(txtDossierRacine.Text) Then
    '        MessageBox.Show("Veuillez remplir tous les champs.", "Configuration requise",
    '                      MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '        Return
    '    End If

    '    ' Créer le dossier si inexistant
    '    IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(txtCheminBDD.Text))
    '    IO.Directory.CreateDirectory(txtDossierRacine.Text)

    '    ' Enregistrer dans les settings
    '    My.Settings.ficBddDonnees = txtCheminBDD.Text
    '    My.Settings.repRacineAgumaaa = txtDossierRacine.Text

    '    Me.DialogResult = DialogResult.OK
    '    Me.Close()
    'End Sub
    'Private Sub CreerBaseAccess()
    '    Dim catalog As New ADOX.Catalog()
    '    Try
    '        catalog.Create("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & My.Settings.ficBddDonnees)

    '        ' Créer la table Transactions
    '        Dim conn As New OleDb.OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & My.Settings.ficBddDonnees)
    '        conn.Open()
    '        Dim cmd As New OleDb.OleDbCommand("CREATE TABLE Transactions (
    '        ID COUNTER PRIMARY KEY,
    '        DateTrans DATE,
    '        TypeTrans TEXT(20),
    '        Montant CURRENCY,
    '        Libelle TEXT(255)
    '    )", conn)
    '        cmd.ExecuteNonQuery()
    '        conn.Close()

    '        MessageBox.Show("Base de données créée avec succès !", "Succès",
    '                        MessageBoxButtons.OK, MessageBoxIcon.Information)
    '    Catch ex As Exception
    '        MessageBox.Show("Erreur création base : " & ex.Message)
    '    End Try
    'End Sub
End Class