Imports System.Collections.Specialized.BitVector32
Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.IO

Public Class FrmSaisie

    Inherits System.Windows.Forms.Form
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String
    Private listeTiers As List(Of ClsTiers)

    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each Section In GestionFichierIni.SectionNames(sFicParam)
            Me.cbCategorie.Items.Add(Section)
        Next
        listeTiers = GereXml.LitXml(My.Settings.ficTiers).RenvoieListe
        'Chargement du fichier contenant la liste des tiers 
        Call ChargeFichierTiers(Me.cbTiers, My.Settings.ficTiers)
        'Chargement du fichier contenant la liste des événements
        Call ChargeFichierTexte(Me.cbEvénement, My.Settings.ficEvénement)
        'Chargement du fichier contenant la liste des types
        Call ChargeFichierTexte(Me.cbType, My.Settings.ficType)
    End Sub

    Private Sub FrmSaisie_GotFocus(sender As Object, e As EventArgs) Handles Me.GotFocus
        'Détection du tiers
        cbTiers.SelectedItem = DetecteTiers(txtNote.Text)
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'Détection et sélection du tiers
        cbTiers.SelectedItem = cbTiers.Items.IndexOf(DetecteTiers(txtNote.Text))
    End Sub
    'Private Sub form1_GotFocus(sender As Object, e As EventArgs) Handles txtMontant.GotFocus
    'End Sub
    Private Function DetecteTiers(sNote As String) As String
        'Essaie de déterminer le tiers en fonction du contenu de la note
        Dim sMots() As String, sMot As String
        Dim i As Integer
        Dim sTiersTrouve As String

        sMots = Split(txtNote.Text, Space(1))
        sTiersTrouve = ""
        'TODO : voir pourquoi il me ramène plein de mots vides
        For Each sMot In sMots
            sMot = Strings.UCase(sMot)
            If Not sMot.Equals("") Then
                For i = 0 To listeTiers.Count - 1
                    If Strings.UCase(listeTiers.Item(i).Nom).Equals(sMot) Then
                        sTiersTrouve = listeTiers.Item(i).Nom
                        Exit For
                    End If
                Next
                'Si on ne trouve pas sur le nom, on essaie sur la raison sociale
                For i = 0 To listeTiers.Count - 1
                    If Strings.UCase(listeTiers.Item(i).RaisonSociale).Equals(sMot) Then
                        'Le "GIE" de "GIE Klésia est trouvé dans "AFC Hygiène"
                        sTiersTrouve = listeTiers.Item(i).RaisonSociale
                        Exit For
                    End If
                Next
                If Not sTiersTrouve.Equals("") Then
                    Exit For
                End If
            End If
        Next
        Return sTiersTrouve
    End Function
    Private Sub Désélectionne()
        'Désélectionne les items des comboBox

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
        Dim tiers As ClsTiers

        Try
            Dim monStreamReader As New StreamReader(fichierTiers, System.Text.Encoding.Default) 'Stream pour la lecture
            'https://selkis.developpez.com/tutoriels/dotnet/Xmlpart1/
            'For Each tiers In GereXml.LitXml(fichierTiers).RenvoieListe
            For Each tiers In listeTiers
                If Not IsNothing(tiers.RaisonSociale) Then
                    'TODO : tout pourri mais devrait faire l'affaire
                    cbBox.Items.Add(tiers.RaisonSociale & vbTab & tiers.CategorieDefaut & vbTab & tiers.SousCategorieDefaut)
                Else
                    cbBox.Items.Add(tiers.Prénom & " " & tiers.Nom & vbTab & tiers.CategorieDefaut & vbTab & tiers.SousCategorieDefaut)
                End If
            Next
        Catch ex As Exception
            MsgBox("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : " & fichierTiers, MsgBoxStyle.Critical, "Erreur lors e l'ouverture du fichier conf...")
        End Try
    End Sub
    Private Sub CbCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCategorie.SelectedIndexChanged
        Me.cbSousCategorie.Items.Clear()
    End Sub

    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        'Enregistre les informations sur le mouvement saisi en base de données
        Dim unMvt As New Mouvements(txtNote.Text, cbCategorie.SelectedItem, cbSousCategorie.SelectedItem, cbTiers.SelectedItem, dateMvt.Value, txtMontant.Text, rbCredit.Checked, rbRapproche.Checked, cbEvénement.SelectedItem, cbType.SelectedItem, False, txtRemise.Text)

        Try
            myCmd = New SqlCommand
            With myCmd
                .Connection = FrmPrincipale.myConn
                .CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat,événement,type, modifiable,numeroRemise) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement, @type, @modifiable,@numeroRemise);"
            End With
            myCmd = AjouteParam(myCmd, unMvt)
            myCmd.ExecuteNonQuery()
        Catch ex As Exception
            MsgBox("Echec de l'insertion en base" & " " & ex.Message)
            Console.WriteLine(ex.Message)
            End
        End Try
        Me.Hide()
        FrmPrincipale.Show()
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
            'Remet le focus sur la zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub
    Private Sub BtnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        frmHistogramme.ShowDialog()
    End Sub

    Private Sub CbTiers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbTiers.SelectedIndexChanged
        Dim s(2) As String, sCat As String, sousCat As String

        s = Split(cbTiers.SelectedItem, vbTab)
        sCat = s(1)
        sousCat = s(2)
        'Gérer les catégories par défaut
        cbCategorie.SelectedIndex = IndexSelectionne(cbCategorie, sCat)
        'Chargement des sous-catégories correspondant à la catégorie
        ChargeSousCatégorie(cbCategorie.SelectedItem)
        cbSousCategorie.SelectedIndex = IndexSelectionne(cbSousCategorie, sousCat)
    End Sub
    Private Function IndexSelectionne(cbBox As ComboBox, sNiveau As String) As Integer
        Dim iIndex As Integer

        'Pas de catégorie
        iIndex = 0
        If Not sNiveau.Equals("") Then
            iIndex = cbBox.Items.IndexOf(sNiveau)
        End If
        Return iIndex
    End Function
    Private Sub ChargeSousCatégorie(sSection As String)

        For Each clé In GestionFichierIni.SectionKeys(My.Settings.ficRessources, sSection)
            Me.cbSousCategorie.Items.Add(GestionFichierIni.ReadValue(My.Settings.ficRessources, sSection, clé))
        Next
    End Sub
    Private Sub CbTiers_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbTiers.TextChanged
        'Recherche à la volée dans la cb
        With cbTiers
            If .FindString(Me.cbTiers.Text) > 0 Then
                Dim Pos As Int32 = .Text.Length
                .SelectedIndex = Me.cbTiers.FindString(Me.cbTiers.Text)
                .SelectionStart = Pos
                .SelectionLength = Me.cbTiers.Text.Length - Pos
            End If
        End With
    End Sub

End Class