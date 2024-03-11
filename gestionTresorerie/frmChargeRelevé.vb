Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class frmChargeRelevé
    Private Sub frmChargeRelevé_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call initListView()
    End Sub

    Private Sub btnOuvreFichier_Click(sender As Object, e As EventArgs) Handles btnOuvreFichier.Click
        Call OuvreFichier()
        Call alimentLst()
        MsgBox("fin")
    End Sub
    Private Sub alimentLst()
        Dim sFichier As String = "C:\Users\User\Downloads\test2.txt"

        Dim sLigne As String = ""
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture 
            Dim file As System.IO.StreamWriter
            'file = My.Computer.FileSystem.FileReader(sFichier)

            sLigne = monStreamReader.ReadLine
            While sLigne IsNot Nothing
                'file.WriteLine(sLigne)
                ajouteLigne(sLigne)
                sLigne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
            file.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue sur a lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Sub initListView()
        'Set to details view.
        lstMouvements.View = View.Details
        lstMouvements.AllowColumnReorder = True
        lstMouvements.GridLines = True
        lstMouvements.Columns.Add("Date", 100, HorizontalAlignment.Left)
        lstMouvements.Columns.Add("Libellé", 100, HorizontalAlignment.Left)
        lstMouvements.Columns.Add("Débit", 100, HorizontalAlignment.Left)
        lstMouvements.Columns.Add("Crédit", 100, HorizontalAlignment.Left)
    End Sub
    Private Sub ajouteLigne(sligne As String)
        Dim monElem As New ListViewItem
        'MsgBox("E0 :" & Split(sligne, ";")(0) & vbCrLf & " E1 : " & Split(sligne, ";")(1) & vbCrLf & " E2 : " & Split(sligne, ";")(2) & vbCrLf & " E3 : " & Split(sligne, ";")(3))
        monElem.SubItems.Add(Split(sligne, ";")(0))
        monElem.SubItems.Add(Split(sligne, ";")(1))
        monElem.SubItems.Add(Split(sligne, ";")(2))
        monElem.SubItems.Add(Split(sligne, ";")(3))
        lstMouvements.Items.Add(monElem)
    End Sub
End Class