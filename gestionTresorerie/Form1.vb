Imports System.Collections.Specialized.BitVector32
Imports System.Text.RegularExpressions
Imports Windows.Win32.System

Public Class FrmSaisie
    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each Section In GestionFichierIni.SectionNames(sFicParam)
            Me.lstCategorie.Items.Add(Section)
        Next
    End Sub

    Private Sub lstCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstCategorie.SelectedIndexChanged
        Me.lstSousCategorie.Items.Clear()
        chargeListBox(Me.lstSousCategorie, Me.lstCategorie.SelectedItem)
    End Sub
    Private Sub chargeListBox(listBox As ListBox, categorie As String)
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each valeur In GestionFichierIni.SectionKeys(sFicParam, categorie)
            listBox.Items.Add(GestionFichierIni.ReadValue(sFicParam, Me.lstCategorie.SelectedItem, valeur))
        Next
    End Sub

    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        'Enregistre les informations sur le mouvement saisies
    End Sub

    Private Sub txtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        If Not Regex.Match(txtMontant.Text, "^[0-9]*,[0-9]{0,2}$", RegexOptions.IgnoreCase).Success Then
            MessageBox.Show("Le montant doit être numérique!")
            'Remet le focus su rla zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub
End Class
