Imports System.Collections.Specialized.BitVector32
Imports System.Text.RegularExpressions
Imports Windows.Win32.System
Imports System.Data.SqlClient
Imports System.Data.Common
Imports System.Configuration
Imports System.IO


Public Class FrmSaisie

    Inherits System.Windows.Forms.Form
    'Create ADO.NET objects.
    Private myConn As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each Section In GestionFichierIni.SectionNames(sFicParam)
            Me.lstCategorie.Items.Add(Section)
        Next
        'Chargement du fichier contenant la liste des tiers
        Call chargeFichierTexte(Me.lstTiers, My.Settings.ficTiers)
        'Chargement du fichier contenant la liste des événements
        Call chargeFichierTexte(Me.lstEvénement, My.Settings.ficEvénement)
        'Chargement du fichier contenant la liste des types
        Call chargeFichierTexte(Me.lstType, My.Settings.ficType)
    End Sub

    Private Sub chargeFichierTexte(lstBox As ListBox, fichierTexte As String)
        'Throw New NotImplementedException()

        Try
            Dim monStreamReader As New StreamReader(fichierTexte) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            ligne = monStreamReader.ReadLine
            While Not ligne Is Nothing
                lstBox.Items.Add(ligne)
                ligne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : " & fichierTexte, MsgBoxStyle.Critical, "Erreur lors e l'ouverture du fichier conf...")
        End Try
    End Sub
    Private Sub LstCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstCategorie.SelectedIndexChanged
        Me.lstSousCategorie.Items.Clear()
        ChargeListBox(Me.lstSousCategorie, Me.lstCategorie.SelectedItem)
    End Sub
    Private Sub ChargeListBox(listBox As ListBox, categorie As String)
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each valeur In GestionFichierIni.SectionKeys(sFicParam, categorie)
            listBox.Items.Add(GestionFichierIni.ReadValue(sFicParam, Me.lstCategorie.SelectedItem, valeur))
        Next
    End Sub

    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        'Enregistre les informations sur le mouvement saisies
        'myCmd.CommandText = "insert into Mouvements (categorie) values "
        Try
            'Dim myCmd As New SqlCommand
            myCmd = New SqlCommand
            'Call CreeConnexion()
TODO:       'passer une connexion en paramètre
            myCmd.Connection = myConn
            'Call LectureBase() 
            myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat,événement,type, modifiable,numeroRemise) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement, @type, @modifiable,@numeroRemise);"
            myCmd.Parameters.Clear()
            myCmd.Parameters.Add("@note", SqlDbType.NVarChar)
            myCmd.Parameters(0).Value = "Note de test"
            myCmd.Parameters.Add("@categorie", SqlDbType.VarChar)
            myCmd.Parameters(1).Value = lstCategorie.SelectedItem
            myCmd.Parameters.Add("@sousCategorie", SqlDbType.VarChar)
            myCmd.Parameters(2).Value = lstSousCategorie.SelectedItem
            myCmd.Parameters.Add("@tiers", SqlDbType.VarChar)
            myCmd.Parameters(3).Value = lstTiers.SelectedItem
            myCmd.Parameters.Add("@dateCréation", SqlDbType.Date)
            myCmd.Parameters(4).Value = Now.Date
            myCmd.Parameters.Add("@dateMvt", SqlDbType.Date)
            myCmd.Parameters(5).Value = dateMvt.Value
            myCmd.Parameters.Add("@montant", SqlDbType.Decimal)
            myCmd.Parameters(6).Value = txtMontant.Text
            myCmd.Parameters.Add("@sens", SqlDbType.Bit)
            myCmd.Parameters(7).Value = IIf(rbCredit.Checked, 1, 0)
            myCmd.Parameters.Add("@etat", SqlDbType.Bit)
            myCmd.Parameters(8).Value = IIf(rbRapproche.Checked, 1, 0)
            myCmd.Parameters.Add("@événement", SqlDbType.VarChar)
            myCmd.Parameters(9).Value = lstEvénement.SelectedItem
            myCmd.Parameters.Add("@type", SqlDbType.VarChar)
            myCmd.Parameters(10).Value = lstType.SelectedItem
            myCmd.Parameters.Add("@modifiable", SqlDbType.Bit)
            myCmd.Parameters(11).Value = 0
            myCmd.Parameters.Add("@numeroRemise", SqlDbType.Int)
            myCmd.Parameters(12).Value = IIf(txtRemise.Text > "", CInt(txtRemise.Text), 0)

            myCmd.ExecuteNonQuery()
            MsgBox("Ajout effectué avec succès")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            End
        End Try

    End Sub
    Private Sub TxtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        'If Not Regex.Match(txtMontant.Text, "^[0-9]*$", RegexOptions.IgnoreCase).Success Then
        If Not Regex.Match(txtMontant.Text, "^[0-9]+(,[0-9]{0,2})*$", RegexOptions.IgnoreCase).Success Then
            MessageBox.Show("Le montant doit être numérique!")
            'Remet le focus su rla zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub
    Private Sub btnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        frmHistogramme.ShowDialog()
    End Sub
End Class
