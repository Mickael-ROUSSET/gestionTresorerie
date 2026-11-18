Option Strict On
Option Explicit On

Imports System
Imports System.Windows.Forms

''' <summary>
''' Raccordement TEST de la grille documentaire a un controle WinForms.
''' Ne lance aucun traitement et ne touche ni Gmail, ni SQL, ni PROD.
''' </summary>
Public NotInheritable Class DocumentBatchTestReviewGridBinding
    Private Sub New()
    End Sub

    Public Shared Sub LoadFromFile(grid As DataGridView, path As String)
        If grid Is Nothing Then Throw New ArgumentNullException(NameOf(grid))
        ' Valider le bilan avant de modifier l'affichage existant.
        Dim table = DocumentBatchTestReviewGrid.CreateTableFromFile(path)
        grid.AutoGenerateColumns = True
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        If grid.BindingContext Is Nothing Then grid.BindingContext = New BindingContext()
        grid.DataSource = table
    End Sub
End Class
