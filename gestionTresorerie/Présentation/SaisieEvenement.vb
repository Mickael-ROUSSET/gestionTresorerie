Option Strict On
Imports System.Windows.Forms

Friend Module SaisieEvenement
    Friend Function Demander(owner As Form, suggestion As String, enregistrer As Func(Of String, String)) As String
        Dim valeur = Microsoft.VisualBasic.Interaction.InputBox(
            "Libellé de l’événement (50 caractères maximum). Validez pour l’enregistrer dans le répertoire des événements.",
            "Créer un événement", suggestion)
        If String.IsNullOrWhiteSpace(valeur) Then Return Nothing
        Try
            Return enregistrer(valeur)
        Catch ex As Exception
            MessageBox.Show(owner, ex.Message, "Création de l’événement", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return Nothing
        End Try
    End Function
End Module
