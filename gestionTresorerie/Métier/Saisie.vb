Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Public Class FrmSaisie

    Inherits System.Windows.Forms.Form
    Private maCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String
    Private listeTiers As ListeTiers
    Private _idCheque As Integer = 0
    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Dim indCategorie As Integer = 0
        Dim idCategorie As Integer, idSousCategorie As Integer
        Dim idTiers As Integer
        'Détection du tiers
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers(connexionDB.GetInstance.getConnexion)
        End If
        Dim indTiersDetecte As Integer = DetecteTiers(txtNote.Text)
        If indTiersDetecte > -1 Then
            'dgvTiers.Rows.Item(indTiersDetecte).Selected = True
            idTiers = chercheIndiceDvg(indTiersDetecte, dgvTiers)
            dgvTiers.Rows.Item(idTiers).Selected = True
            dgvTiers.FirstDisplayedScrollingRowIndex = idTiers
        End If
        If dgvCategorie.RowCount = 0 Then
            Call ChargeDgvCategorie(connexionDB.GetInstance.getConnexion, rbDebit.Checked)
        End If
        'TODO, il doit falloir trouver le libellé de la catégorie à partir de l'indice
        'idCategorie = Tiers.getCategorieTiers(indTiersDetecte)
        idCategorie = chercheIndiceDvg(Tiers.getCategorieTiers(indTiersDetecte), dgvCategorie)
        dgvCategorie.Rows.Item(idCategorie).Selected = True
        dgvCategorie.FirstDisplayedScrollingRowIndex = idCategorie

        If dgvSousCategorie.RowCount = 0 Then
            Call ChargeDgvSousCategorie(connexionDB.GetInstance.getConnexion, Tiers.getCategorieTiers(indTiersDetecte))
        End If
        idSousCategorie = chercheIndiceDvg(Tiers.getSousCategorieTiers(indTiersDetecte), dgvSousCategorie)
        dgvSousCategorie.Rows.Item(idSousCategorie).Selected = True
        dgvSousCategorie.FirstDisplayedScrollingRowIndex = idSousCategorie
    End Sub
    Private Function chercheIndiceDvg(indiceCherche As Integer, dgv As DataGridView) As Integer
        Dim i As Integer, ligneCible As Integer = 0

        For i = 0 To dgv.RowCount
            If dgv.Rows(i).Cells(0).Value = indiceCherche Then
                ligneCible = i
                Exit For
            End If
        Next
        Return ligneCible
    End Function


    Public Sub chargeListes(maConn As SqlConnection)

        Call ChargeDgvTiers(connexionDB.GetInstance.getConnexion)
        Call ChargeDgvCategorie(maConn, rbDebit.Checked)
        'Chargement du fichier contenant la liste des événements
        Call ChargeFichierTexte(Me.cbEvénement, My.Settings.repInstallation & My.Settings.ficEvénement)
        'Chargement du fichier contenant la liste des types
        Call ChargeFichierTexte(Me.cbType, My.Settings.repInstallation & My.Settings.ficType)
    End Sub
    Private Function DetecteTiers(sNote As String) As Integer
        'Essaie de déterminer le tiers en fonction du contenu de la note
        Dim sMots() As String, sMot As String
        Dim i As Integer = -1

        sMots = Split(txtNote.Text, Space(1),, CompareMethod.Text)
        'TODO : voir pourquoi il me ramène plein de mots vides
        For Each sMot In sMots
            If Not sMot.Equals("") Then
                i = listeTiers.getIdParRaisonSociale(Strings.UCase(sMot))
                If i > -1 Then
                    Exit For
                End If
            End If
        Next
        Return i
    End Function
    Private Sub Désélectionne()
        'Désélectionne les items des comboBox

        cbEvénement.SelectedIndex = -1
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

    Private Sub ChargeDgvTiers(maConn As SqlConnection)
        Dim bindingSource1 = New BindingSource()

        Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, nom, prenom, raisonSociale, categorieDefaut, sousCategorieDefaut FROM Tiers;", maConn)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connection dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
            adpt.Fill(dt)
            dgvTiers.DataSource = dt
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("ChargeDgvTiers : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        End Try
        'dgvTiers.Columns("id").Visible = False 
    End Sub

    Private Sub ChargeDgvCategorie(maConn As SqlConnection, debit As Boolean)
        Dim bindingSource1 = New BindingSource()
        'Dim sTopDebit As String

        'Ne charge que les catégories correspondant au sens sélectionné
        'sTopDebit = IIf(debit, 1, 0)
        'Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, libelle FROM Categorie where debit =" & sTopDebit & ";", maConn)
        'TODO : remettre la restriction sur le sens du mouvement
        Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, libelle FROM Categorie;", maConn)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connexion dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
            adpt.Fill(dt)
            dgvCategorie.DataSource = dt
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("ChargeDgvCategorie : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        End Try
        dgvCategorie.Columns("id").Visible = False
        dgvCategorie.Columns("libelle").Visible = True
    End Sub
    Private Sub ChargeDgvSousCategorie(maConn As SqlConnection, idCategorie As Integer)
        Dim bindingSource1 = New BindingSource()

        Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, libelle FROM SousCategorie where idCategorie = @idCategorie;", maConn)
        command.Parameters.AddWithValue("@idCategorie", idCategorie)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connexion dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception.
            adpt.Fill(dt)
            dgvSousCategorie.DataSource = dt

            ' Vérifie si le DataGridView est vide
            If dgvSousCategorie.Rows.Count = 0 Then
                MessageBox.Show("ChargeDgvSousCategorie : aucune ligne n'a été trouvée pour la catégorie spécifiée.")
                MessageBox.Show("Ajouter une entrée dans la table SousCategorie.")
                'TODO : orienter vers une fenêtre qui permettra d'ajouter une entrée
                End
            End If
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("ChargeDgvSousCategorie : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        Catch ex As Exception
            ' Gère toutes les autres erreurs
            MessageBox.Show("ChargeDgvSousCategorie : une erreur inattendue s'est produite !" & vbCrLf & ex.ToString())
        End Try

        dgvSousCategorie.Columns("id").Visible = False
        dgvSousCategorie.Columns("libelle").Visible = True
    End Sub

    'Private Sub CbCategorie_SelectedIndexChanged(sender As Object, e As EventArgs)
    '    cbSousCategorie.Items.Clear
    'End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Call insereChq()
        Me.Hide()
        FrmPrincipale.Show()
    End Sub
    Private Function AjouteParam(myCmd As SqlCommand, unMvt As Mouvements) As SqlCommand
        With myCmd.Parameters
            .Clear()
            .Add("@note", SqlDbType.NVarChar)
            .Item(0).Value = unMvt.Note
            .Add("@categorie", SqlDbType.VarChar)
            .Item(1).Value = unMvt.Categorie
            .Add("@sousCategorie", SqlDbType.VarChar)
            .Item(2).Value = unMvt.SousCategorie
            .Add("@tiers", SqlDbType.Decimal)
            .Item(3).Value = unMvt.Tiers
            .Add("@dateCréation", SqlDbType.Date)
            .Item(4).Value = Now.Date
            .Add("@dateMvt", SqlDbType.Date)
            .Item(5).Value = unMvt.DateMvt
            .Add("@montant", SqlDbType.Decimal)
            .Item(6).Value = unMvt.Montant
            .Add("@sens", SqlDbType.Bit)
            .Item(7).Value = unMvt.Sens
            .Add("@etat", SqlDbType.Bit)
            .Item(8).Value = unMvt.Etat
            .Add("@événement", SqlDbType.VarChar)
            .Item(9).Value = unMvt.Événement
            .Add("@type", SqlDbType.VarChar)
            .Item(10).Value = unMvt.Type
            .Add("@modifiable", SqlDbType.Bit)
            .Item(11).Value = unMvt.Modifiable
            .Add("@numeroRemise", SqlDbType.Int)
            .Item(12).Value = unMvt.NumeroRemise
            .Add("@idCheque", SqlDbType.Int)
            .Item(13).Value = unMvt.idCheque
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

    'Private Sub CbTiers_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
    '    'Recherche à la volée dans la cb
    '    With cbTiers
    '        If .FindString(cbTiers.Text) > 0 Then
    '            Dim Pos = .Text.Length
    '            .SelectedIndex = cbTiers.FindString(cbTiers.Text)
    '            .SelectionStart = Pos
    '            .SelectionLength = cbTiers.Text.Length - Pos
    '        End If
    '    End With
    'End Sub

    Private Sub dgvTiers_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTiers.CellContentClick
        'Gérer les catégories par défaut

        Dim selectedRowCount As Int32 = dgvTiers.Rows.GetRowCount(DataGridViewElementStates.Selected)
        Dim i As Integer, idTiers As Integer

        If (selectedRowCount > 0) Then
            Dim sb As New System.Text.StringBuilder()
            For i = 0 To selectedRowCount
                sb.Append("Row: ")
                sb.Append(dgvTiers.SelectedRows(i).Index)
                sb.Append(Environment.NewLine)
                i += 1
            Next
            sb.Append("Total: " + selectedRowCount.ToString())
            'MessageBox.Show(sb.ToString(), "Selected Rows")
        End If
        'Retrouver le Tiers
        dgvTiers.CurrentCell = dgvTiers.SelectedRows(0).Cells(0)
        idTiers = dgvTiers.SelectedRows(0).Cells(0).Value
    End Sub
    'Private Sub ChargeSousCategorie(indiceCategorie As Integer)
    '    Dim monReaderSousCategorie As SqlDataReader
    '    Dim maCmdSousCategorie As SqlCommand

    '    maCmdSousCategorie = New SqlCommand("SELECT libelle FROM SousCategorie where idCategorie =" & indiceCategorie & ";", connexionDB.GetInstance.getConnexion)
    '    monReaderSousCategorie = maCmdSousCategorie.ExecuteReader()
    '    Do While monReaderSousCategorie.Read()
    '        Try
    '            Me.cbSousCategorie.Items.Add(monReaderSousCategorie.GetSqlString(0))
    '        Catch ex As Exception
    '            msgbox(ex.Message)
    '        End Try
    '    Loop
    '    monReaderSousCategorie.Close()
    'End Sub
    Private Sub dgvTiers_UserAddedRow(sender As Object, e As DataGridViewRowEventArgs) Handles dgvTiers.UserAddedRow
        Dim bindingSource1 = New BindingSource()
        Dim id As Integer
        Dim sNom As String, sPrenom As String, sRaisonSociale As String

        id = CInt(dgvTiers.CurrentRow.Cells(0).Value.ToString)
        sNom = dgvTiers.CurrentRow.Cells(0).Value.ToString
        sPrenom = dgvTiers.CurrentRow.Cells(0).Value.ToString
        sRaisonSociale = dgvTiers.CurrentRow.Cells(0).Value.ToString
        Dim command As New System.Data.SqlClient.SqlCommand("insert into Tiers (id, nom, prenom, raisonSoicale) values ('" & id & "', '" & sNom & "' ,'" & sPrenom & "', '" & sRaisonSociale & "';", connexionDB.GetInstance.getConnexion)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connexion dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
            adpt.Fill(dt)
            dgvCategorie.DataSource = dt
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("dgvTiers_UserAddedRow : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        End Try
    End Sub
    Private Sub btnInsereTiers_Click(sender As Object, e As EventArgs) Handles btnInsereTiers.Click
        frmNouveauTiers.Show()
    End Sub
    Private Sub dgvCategorie_DoubleClick(sender As Object, e As EventArgs) Handles dgvCategorie.DoubleClick
        Call majSousCategorie()
    End Sub
    Private Sub dgvCategorie_Click(sender As Object, e As EventArgs) Handles dgvCategorie.Click
        Call majSousCategorie()
    End Sub
    Private Sub majSousCategorie()
        If dgvCategorie.SelectedRows(0).Cells(0) IsNot Nothing Then
            Call ChargeDgvSousCategorie(connexionDB.GetInstance.getConnexion, dgvCategorie.SelectedRows(0).Cells(0).Value)
        End If
    End Sub

    'Private Sub selectionneLigneParLibelle(libelle As String)
    '    'Sélectionne la ligne dont le libellé correspond au paramètre (sur le nombre de caractères renseignés)
    '    Dim i As Long

    '    If Len(libelle) > 1 Then
    '        For i = 0 To dgvCategorie.RowCount - 1
    '            If Strings.Left(dgvCategorie.SelectedRows(i).Cells(1).Value, Len(libelle)) = libelle Then
    '                dgvCategorie.Rows(i).Selected = True
    '                dgvCategorie.Rows(i).Visible = True
    '            End If
    '        Next
    '    End If
    'End Sub
    Private Sub selectionneLigneParLibelle(dgv As DataGridView, libelle As String)
        'Sélectionne la ligne dont le libellé correspond au paramètre (sur le nombre de caractères renseignés)
        Dim i As Long
        Dim sLibMajuscule As String

        sLibMajuscule = UCase(libelle)
        If Len(libelle) > 1 Then
            'Il faut gérer les cas personnes morales / physiques
            For i = 0 To dgv.RowCount - 1
                Select Case True
                    Case Not IsDBNull(dgv.Rows(i).Cells(1).Value)
                        If UCase(Strings.Left(dgv.Rows(i).Cells(1).Value, Len(libelle))) = sLibMajuscule Then
                            afficheLigneTrouvée(dgv, i)
                        End If
                    Case Not IsDBNull(dgv.Rows(i).Cells(3).Value)
                        If UCase(Strings.Left(dgv.Rows(i).Cells(3).Value, Len(libelle))) = sLibMajuscule Then
                            afficheLigneTrouvée(dgv, i)
                        End If
                    Case Else
                        MsgBox("selectionneLigneParLibelle : erreur critique sur recherche : " & dgv.Name & " libellé : " & libelle)
                        End
                End Select
            Next
        End If
    End Sub
    Private Sub afficheLigneTrouvée(dgv As DataGridView, ligne As Long)
        dgv.Rows(ligne).Selected = True
        dgv.CurrentCell = dgv.SelectedRows(0).Cells(0)
    End Sub
    Private Sub txtRechercheTiers_TextChanged(sender As Object, e As EventArgs) Handles txtRechercheTiers.TextChanged
        selectionneLigneParLibelle(dgvTiers, txtRechercheTiers.Text)
    End Sub
    Private Sub insereChq()
        Dim sCategorie As String, sSousCategorie As String
        Dim idTiers As Integer

        'Dim sNomTiers = dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(1).Value
        'dim sPrenomTiers = dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(2).Value
        'dim sRaisonSocialeTiers = dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(3).Value

        'Enregistre les informations sur le mouvement saisi en base de données
        sCategorie = dgvCategorie.Rows(dgvCategorie.SelectedRows(0).Index).Cells(1).Value
        sSousCategorie = dgvSousCategorie.Rows(dgvSousCategorie.SelectedRows(0).Index).Cells(1).Value
        idTiers = dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(0).Value
        Dim unMvt As New Mouvements(txtNote.Text, sCategorie, sSousCategorie, idTiers, dateMvt.Value, txtMontant.Text.Trim().Replace(" ", ""),
        rbCredit.Checked, rbRapproche.Checked, cbEvénement.SelectedItem, cbType.SelectedItem, False, txtRemise.Text, _idCheque)


        Try
            maCmd = New SqlCommand
            With maCmd
                .Connection = connexionDB.GetInstance.getConnexion
                .CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat,événement,type, modifiable,numeroRemise, idCheque) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement, @type, @modifiable,@numeroRemise, @idCheque);"
            End With
            maCmd = AjouteParam(maCmd, unMvt)
            maCmd.ExecuteNonQuery()
            Logger.GetInstance.INFO("Insertion du mouvement pour : " & unMvt.ObtenirValeursConcatenees)
        Catch ex As Exception
            MsgBox("Echec de l'insertion en base" & " " & ex.Message)
            Logger.GetInstance.ERR("Erreur lors de l'insertion des données : " & ex.Message)
            End
        End Try
    End Sub
    Private Sub btnSelChq_Click(sender As Object, e As EventArgs) Handles btnSelChq.Click
        Dim selectionneCheque As New selectionneCheque()
        AddHandler selectionneCheque.IdChequeSelectionneChanged, AddressOf IdChequeSelectionneChangedHandler
        selectionneCheque.ShowDialog()

        selectionneCheque.chargeListeChq(CDec(Me.txtMontant.Text))
        selectionneCheque.Show()
    End Sub

    Private Sub IdChequeSelectionneChangedHandler(ByVal idCheque As Integer)
        ' Faire quelque chose avec l'idCheque
        'TODO  : reste à trouver le Mouvement
        _idCheque = idCheque
        'Call UpdateIdCheque(idMouvement, idCheque)
    End Sub

    Public Function CreerJsonCheque(_id As Integer, _montant_numerique As Decimal, _numero_du_cheque As String, _dateChq As DateTime, _emetteur_du_cheque As String, _destinataire As String) As String
        ' Créer un objet JObject pour construire le JSON
        Dim jsonObject As New JObject()
        jsonObject("id") = _id
        jsonObject("montant_numerique") = _montant_numerique
        jsonObject("numero_du_cheque") = _numero_du_cheque
        jsonObject("dateChq") = _dateChq.ToString("yyyy-MM-dd") ' Formatage de la date
        jsonObject("emetteur_du_cheque") = _emetteur_du_cheque
        jsonObject("destinataire") = _destinataire

        ' Convertir l'objet JObject en une chaîne JSON
        Dim jsonString As String = jsonObject.ToString(Formatting.Indented)

        Return jsonString
    End Function


    'Private Sub dgvCategorie_SelectionChanged(sender As Object, e As EventArgs) Handles dgvCategorie.SelectionChanged
    '    If Not dgvCategorie.SelectedRows(0).Cells(0) Is Nothing Then
    '        Call ChargeDgvSousCategorie(connexionDB.GetInstance.getConnexion, dgvCategorie.SelectedRows(0).Cells(0).Value)
    '    End If
    'End Sub

    'Private Sub rbDebit_CheckedChanged(sender As Object, e As EventArgs) Handles rbDebit.CheckedChanged
    '    ChargeDgvCategorie(connexionDB.GetInstance.getConnexion, rbDebit.Checked)
    'End Sub

    'Private Sub rbCredit_CheckedChanged(sender As Object, e As EventArgs) Handles rbCredit.CheckedChanged
    '    ChargeDgvCategorie(connexionDB.GetInstance.getConnexion, rbCredit.Checked)
    'End Sub
End Class