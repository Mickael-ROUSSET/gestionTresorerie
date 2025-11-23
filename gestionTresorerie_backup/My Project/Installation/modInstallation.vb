Module modInstallation
    'Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    '    ' Vérifier si les paramètres sont vides → premier lancement
    '    If String.IsNullOrEmpty(My.Settings.ficBddDonnees) Or
    '       String.IsNullOrEmpty(My.Settings.repRacineAgumaaa) Then

    '        Dim configForm As New frmConfiguration()
    '        If configForm.ShowDialog() = DialogResult.OK Then
    '            My.Settings.Save()
    '            CreerBaseSiInexistante()
    '        Else
    '            ' Annulation → fermer l'app
    '            Application.Exit()
    '        End If
    '    Else
    '        ' Paramètres déjà définis → créer la base si absente
    '        CreerBaseSiInexistante()
    '    End If
    'End Sub

    'Private Sub CreerBaseSiInexistante()
    '    If Not IO.File.Exists(My.Settings.ficBddDonnees) Then
    '        CreerBaseAccess()
    '    End If
    'End Sub
End Module
