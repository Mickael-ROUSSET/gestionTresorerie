Imports System.Collections.Specialized.BitVector32
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
End Class
