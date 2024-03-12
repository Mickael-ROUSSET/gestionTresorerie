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
    End Sub
    Private Sub alimentLst()
        Dim sFichier As String = "C:\Users\User\Downloads\test2.txt"

        Dim sLigne As String = ""
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture  

            sLigne = monStreamReader.ReadLine
            While sLigne IsNot Nothing
                ajouteLigne(sLigne)
                sLigne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue sur a lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Sub initListView()
        'Set to details view.
        lstMouvements.View = View.Details
        lstMouvements.AllowColumnReorder = True
        lstMouvements.GridLines = True
        lstMouvements.AllowDrop = True

        lstMouvements.Columns.Add("Date", 75, HorizontalAlignment.Left)
        lstMouvements.Columns.Add("Libellé", 400, HorizontalAlignment.Left)
        lstMouvements.Columns.Add("Débit", 75, HorizontalAlignment.Left)
        lstMouvements.Columns.Add("Crédit", 75, HorizontalAlignment.Left)
    End Sub
    Private Sub ajouteLigne(sligne As String)
        Dim monElem As New ListViewItem
        Dim sMontant As String
        Dim iNumLigne As Integer

        lstMouvements.Items.Add(Split(sligne, ";")(0))
        iNumLigne = lstMouvements.Items.Count - 1
        lstMouvements.Items(iNumLigne).SubItems().Add(Split(sligne, ";")(1))
        sMontant = Split(sligne, ";")(2)
        If sMontant <> "" Then
            lstMouvements.Items(iNumLigne).SubItems().Add(sMontant)
        End If
        sMontant = Split(sligne, ";")(3)
        If sMontant <> "" Then
            lstMouvements.Items(iNumLigne).SubItems().Add(sMontant)
        End If
    End Sub

    Private Sub lstMouvements_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstMouvements.SelectedIndexChanged
        'ListView1.Items(j).SubItems(11).Text <> ""
        Dim s As String
        Dim n As Integer
        Dim sLibelle As String
        Dim sMontant As String
        Dim posRemise As Integer

        n = lstMouvements.SelectedIndices(0)
        FrmSaisie.dateMvt.Value = Convert.ToDateTime(lstMouvements.Items(n).SubItems(0).Text)
        sLibelle = lstMouvements.Items(n).SubItems(1).Text
        FrmSaisie.txtNote.Text = sLibelle
        posRemise = InStr(sLibelle, "REMISE DE CHEQUE", CompareMethod.Text)
        If posRemise > 0 Then
            FrmSaisie.txtRemise.Text = Trim(Mid(sLibelle, posRemise + Len("REMISE DE CHEQUE") + 1))
            'TODO ; supprimer la quote finale
        End If
        If lstMouvements.Items(n).SubItems(2).Text <> "" Then
            sMontant = lstMouvements.Items(n).SubItems(2).Text
            FrmSaisie.rbCredit.Checked = True
        Else
            sMontant = lstMouvements.Items(n).SubItems(3).Text
            FrmSaisie.rbDebit.Checked = True
        End If
        FrmSaisie.txtMontant.Text = sMontant

        FrmSaisie.Show()
    End Sub

End Class