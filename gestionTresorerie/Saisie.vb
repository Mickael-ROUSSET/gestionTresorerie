Imports System.Collections.Specialized.BitVector32
Imports System.Text.RegularExpressions
Imports Windows.Win32.System
Imports System.Data.SqlClient
Imports System.Data.Common
Imports System.Configuration
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement


Public Class FrmSaisie

    Inherits System.Windows.Forms.Form
    'Create ADO.NET objects.
    'Private myConn As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each Section In GestionFichierIni.SectionNames(sFicParam)
            Me.cbCategorie.Items.Add(Section)
        Next
        'Chargement du fichier contenant la liste des tiers 
        Call ChargeFichierTiers(Me.cbTiers, My.Settings.ficTiers)
        'Chargement du fichier contenant la liste des événements
        Call ChargeFichierTexte(Me.cbEvénement, My.Settings.ficEvénement)
        'Chargement du fichier contenant la liste des types
        Call ChargeFichierTexte(Me.cbType, My.Settings.ficType)
    End Sub

    Private Sub Désélectionne()
        'Désélectionne les items des listBox

        Me.cbCategorie.SelectedIndex = -1
        cbEvénement.SelectedIndex = -1
        cbSousCategorie.SelectedIndex = -1
        cbTiers.SelectedIndex = -1
        cbType.SelectedIndex = -1
    End Sub
    Private Sub ChargeFichierTexte(cbBox As System.Windows.Forms.ComboBox, fichierTexte As String)
        'Throw New NotImplementedException()

        Try
            Dim monStreamReader As New StreamReader(fichierTexte, System.Text.Encoding.Default) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            ligne = monStreamReader.ReadLine
            While ligne IsNot Nothing
                cbBox.Items.Add(ligne)
                ligne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : " & fichierTexte, MsgBoxStyle.Critical, "Erreur lors e l'ouverture du fichier conf...")
        End Try
    End Sub
    Private Sub ChargeFichierTiers(cbBox As System.Windows.Forms.ComboBox, fichierTiers As String)
        Dim gereXml As New GereXml
        Dim tiers As clsTiers

        Try
            Dim monStreamReader As New StreamReader(fichierTiers, System.Text.Encoding.Default) 'Stream pour la lecture
            For Each tiers In gereXml.LitXml(fichierTiers).RenvoieListe
                If Not IsNothing(tiers.RaisonSociale) Then
                    cbBox.Items.Add(tiers.RaisonSociale)
                Else
                    cbBox.Items.Add(tiers.Prénom & " " & tiers.Nom)
                End If
            Next
        Catch ex As Exception
            MsgBox("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : " & fichierTiers, MsgBoxStyle.Critical, "Erreur lors e l'ouverture du fichier conf...")
        End Try
    End Sub
    Private Sub CbCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCategorie.SelectedIndexChanged
        Me.cbSousCategorie.Items.Clear()
        ChargeListBox(Me.cbSousCategorie, Me.cbCategorie.SelectedItem)
    End Sub
    Private Sub ChargeListBox(listBox As System.Windows.Forms.ComboBox, categorie As String)
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each valeur In GestionFichierIni.SectionKeys(sFicParam, categorie)
            listBox.Items.Add(GestionFichierIni.ReadValue(sFicParam, Me.cbCategorie.SelectedItem, valeur))
        Next
    End Sub

    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        'Enregistre les informations sur le mouvement saisies 
        Dim unMvt As New Mouvements(txtNote.Text, cbCategorie.SelectedItem, cbSousCategorie.SelectedItem, cbTiers.SelectedItem, dateMvt.Value, txtMontant.Text, rbCredit.Checked, rbRapproche.Checked, cbEvénement.SelectedItem, cbType.SelectedItem, False, txtRemise.Text)

        Try
            myCmd = New SqlCommand
            With myCmd
                .Connection = frmPrincipale.myConn
                .CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat,événement,type, modifiable,numeroRemise) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement, @type, @modifiable,@numeroRemise);"
            End With
            myCmd = AjouteParam(myCmd, unMvt)
            myCmd.ExecuteNonQuery()
            MsgBox("Ajout effectué avec succès")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            End
        End Try
        Me.Hide()
        frmPrincipale.Show()
    End Sub
    Private Function AjouteParam(myCmd As SqlCommand, unMvt As Mouvements) As SqlCommand
        With myCmd
            .Parameters.Clear()
            .Parameters.Add("@note", SqlDbType.NVarChar)
            .Parameters(0).Value = unMvt.Note
            .Parameters.Add("@categorie", SqlDbType.VarChar)
            .Parameters(1).Value = unMvt.Categorie
            .Parameters.Add("@sousCategorie", SqlDbType.VarChar)
            .Parameters(2).Value = unMvt.SousCategorie
            .Parameters.Add("@tiers", SqlDbType.VarChar)
            .Parameters(3).Value = unMvt.Tiers
            .Parameters.Add("@dateCréation", SqlDbType.Date)
            .Parameters(4).Value = Now.Date
            .Parameters.Add("@dateMvt", SqlDbType.Date)
            .Parameters(5).Value = unMvt.DateMvt
            .Parameters.Add("@montant", SqlDbType.Decimal)
            .Parameters(6).Value = unMvt.Montant
            .Parameters.Add("@sens", SqlDbType.Bit)
            .Parameters(7).Value = unMvt.Sens
            .Parameters.Add("@etat", SqlDbType.Bit)
            .Parameters(8).Value = unMvt.Etat
            .Parameters.Add("@événement", SqlDbType.VarChar)
            .Parameters(9).Value = unMvt.Événement
            .Parameters.Add("@type", SqlDbType.VarChar)
            .Parameters(10).Value = unMvt.Type
            .Parameters.Add("@modifiable", SqlDbType.Bit)
            .Parameters(11).Value = unMvt.Modifiable
            .Parameters.Add("@numeroRemise", SqlDbType.Int)
            .Parameters(12).Value = unMvt.NumeroRemise
        End With
        Return myCmd
    End Function
    Private Sub TxtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        'If Not Regex.Match(txtMontant.Text, "^[0-9]*$", RegexOptions.IgnoreCase).Success Then
        If Not Regex.Match(txtMontant.Text, "^[0-9]+(,[0-9]{0,2})*$", RegexOptions.IgnoreCase).Success Then
            MessageBox.Show("Le montant doit être numérique!")
            'Remet le focus su rla zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub
    Private Sub BtnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        frmHistogramme.ShowDialog()
    End Sub
    'Private Sub btnValider_LostFocus(sender As Object, e As EventArgs) Handles btnValider.LostFocus
    '    'Désélectionne les items des listBox
    '    Call Désélectionne()
    'End Sub
End Class




'myCmd.Parameters.Clear()
'myCmd.Parameters.Add("@note", SqlDbType.NVarChar)
'myCmd.Parameters(0).Value = txtNote.Text
'myCmd.Parameters.Add("@categorie", SqlDbType.VarChar)
'myCmd.Parameters(1).Value = lstCategorie.SelectedItem
'myCmd.Parameters.Add("@sousCategorie", SqlDbType.VarChar)
'myCmd.Parameters(2).Value = lstSousCategorie.SelectedItem
'myCmd.Parameters.Add("@tiers", SqlDbType.VarChar)
'myCmd.Parameters(3).Value = lstTiers.SelectedItem
'myCmd.Parameters.Add("@dateCréation", SqlDbType.Date)
'myCmd.Parameters(4).Value = Now.Date
'myCmd.Parameters.Add("@dateMvt", SqlDbType.Date)
'myCmd.Parameters(5).Value = dateMvt.Value
'myCmd.Parameters.Add("@montant", SqlDbType.Decimal)
'myCmd.Parameters(6).Value = txtMontant.Text
'myCmd.Parameters.Add("@sens", SqlDbType.Bit)
'myCmd.Parameters(7).Value = IIf(rbCredit.Checked, 1, 0)
'myCmd.Parameters.Add("@etat", SqlDbType.Bit)
'myCmd.Parameters(8).Value = IIf(rbRapproche.Checked, 1, 0)
'myCmd.Parameters.Add("@événement", SqlDbType.VarChar)
'myCmd.Parameters(9).Value = lstEvénement.SelectedItem
'myCmd.Parameters.Add("@type", SqlDbType.VarChar)
'myCmd.Parameters(10).Value = lstType.SelectedItem
'myCmd.Parameters.Add("@modifiable", SqlDbType.Bit)
'myCmd.Parameters(11).Value = 0
'myCmd.Parameters.Add("@numeroRemise", SqlDbType.Int)
''myCmd.Parameters(12).Value = IIf(txtRemise.Text > "", CInt(txtRemise.Text), 0)
'If txtRemise.Text > "" Then
'    myCmd.Parameters(12).Value = CInt(txtRemise.Text)
'Else
'    myCmd.Parameters(12).Value = 0
'End If