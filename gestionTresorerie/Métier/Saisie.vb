Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.IO

Public Class FrmSaisie

    Inherits System.Windows.Forms.Form
    Private maCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String
    Private listeTiers As ListeTiers
    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim indTiersDetecte As Integer = 0, indCategorie As Integer = 0
        Dim idCategorie As Integer, idSousCategorie As Integer
        'Détection du tiers
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers(FrmPrincipale.maConnexionDB.getConnexion)
        End If
        indTiersDetecte = DetecteTiers(txtNote.Text)
        If indTiersDetecte > -1 Then
            dgvTiers.Rows.Item(indTiersDetecte).Selected = True
            dgvTiers.FirstDisplayedScrollingRowIndex = indTiersDetecte
        End If
        If dgvCategorie.RowCount = 0 Then
            Call ChargeDgvCategorie(FrmPrincipale.maConn)
        End If
        'TODO, il doit falloir trouver le libellé de la catégorie à partir de l'indice
        idCategorie = Tiers.getCategorieTiers(indTiersDetecte)
        dgvCategorie.Rows.Item(idCategorie).Selected = True
        If dgvSousCategorie.RowCount = 0 Then
            Call ChargeDgvSousCategorie(FrmPrincipale.maConn, idCategorie)
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

        Call ChargeDgvTiers(FrmPrincipale.maConn)
        Call ChargeDgvCategorie(maConn)
        'Chargement du fichier contenant la liste des événements
        Call ChargeFichierTexte(Me.cbEvénement, My.Settings.ficEvénement)
        'Chargement du fichier contenant la liste des types
        Call ChargeFichierTexte(Me.cbType, My.Settings.ficType)
    End Sub
    Private Function DetecteTiers(sNote As String) As Integer
        'Essaie de déterminer le tiers en fonction du contenu de la note
        Dim sMots() As String, sMot As String
        Dim i As Integer = -1
        'Dim sIdentite As String = ""

        sMots = Split(txtNote.Text, Space(1),, CompareMethod.Text)
        'TODO : voir pourquoi il me ramène plein de mots vides
        For Each sMot In sMots
            If Not sMot.Equals("") Then
                i = listeTiers.getIdParRaisonSociale(Strings.UCase(sMot))
                If i > -1 Then
                    Exit For
                End If
                'sIdentite = listeTiers.getIdentiteParId(listeTiers.getIdParRaisonSociale(Strings.UCase(sMot)))
                'If Not sIdentite.Equals("") Then
                '    Exit For
                'End If
            End If
        Next
        'Return sIdentite
        Return i
    End Function
    'Private Function DetecteTiers(sNote As String) As String
    '    'Essaie de déterminer le tiers en fonction du contenu de la note
    '    Dim sMots() As String, sMot As String
    '    Dim i As Integer
    '    Dim sTiersTrouve As String

    '    sMots = Split(txtNote.Text, Space(1))
    '    sTiersTrouve = ""
    '    'TODO : voir pourquoi il me ramène plein de mots vides
    '    For Each sMot In sMots
    '        sMot = Strings.UCase(sMot)
    '        If Not sMot.Equals("") Then
    '            For i = 0 To listeTiers.Count - 1
    '                If Strings.UCase(listeTiers.Item(i).Nom).Equals(sMot) Then
    '                    sTiersTrouve = listeTiers.Item(i).Nom
    '                    Exit For
    '                End If
    '            Next
    '            'Si on ne trouve pas sur le nom, on essaie sur la raison sociale
    '            For i = 0 To listeTiers.Count - 1
    '                If Strings.UCase(listeTiers.Item(i).RaisonSociale).Equals(sMot) Then
    '                    'Le "GIE" de "GIE Klésia est trouvé dans "AFC Hygiène"
    '                    sTiersTrouve = listeTiers.Item(i).RaisonSociale
    '                    Exit For
    '                End If
    '            Next
    '            If Not sTiersTrouve.Equals("") Then
    '                Exit For
    '            End If
    '        End If
    '    Next
    '    Return sTiersTrouve
    'End Function
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

    Private Sub ChargeDgvCategorie(maConn As SqlConnection)
        Dim bindingSource1 = New BindingSource()

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

        Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, libelle FROM SousCategorie where idCategorie = '" & idCategorie & "';", maConn)
        'Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, libelle FROM SousCategorie;", maConn)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connexion dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
            adpt.Fill(dt)
            dgvSousCategorie.DataSource = dt
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("ChargeDgvSousCategorie : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        End Try
        dgvSousCategorie.Columns("id").Visible = True
        dgvSousCategorie.Columns("libelle").Visible = True
    End Sub
    Private Sub CbCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCategorie.SelectedIndexChanged
        Me.cbSousCategorie.Items.Clear()
    End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Dim sCategorie As String, sSousCategorie As String

        'Enregistre les informations sur le mouvement saisi en base de données
        'Dim unMvt As New Mouvements(txtNote.Text, cbCategorie.SelectedItem, cbSousCategorie.SelectedItem, cbTiers.SelectedItem, dateMvt.Value, txtMontant.Text, rbCredit.Checked, rbRapproche.Checked, cbEvénement.SelectedItem, cbType.SelectedItem, False, txtRemise.Text)
        sCategorie = dgvCategorie.Rows(dgvCategorie.SelectedRows(0).Index).Cells(1).Value
        sSousCategorie = dgvSousCategorie.Rows(dgvSousCategorie.SelectedRows(0).Index).Cells(1).Value
        Dim unMvt As New Mouvements(txtNote.Text, sCategorie, sSousCategorie, cbTiers.SelectedItem, dateMvt.Value, txtMontant.Text, rbCredit.Checked, rbRapproche.Checked, cbEvénement.SelectedItem, cbType.SelectedItem, False, txtRemise.Text)

        Try
            maCmd = New SqlCommand
            With maCmd
                .Connection = FrmPrincipale.maConn
                .CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat,événement,type, modifiable,numeroRemise) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement, @type, @modifiable,@numeroRemise);"
            End With
            maCmd = AjouteParam(maCmd, unMvt)
            maCmd.ExecuteNonQuery()
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

    Private Sub dgvTiers_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTiers.CellContentClick
        'Gérer les catégories par défaut
        'cbCategorie.SelectedIndex = IndexSelectionne(cbCategorie, dgvTiers.Rows(dgvTiers.SelectedCells(0).RowIndex).Cells(1).Value.ToString)private void selectedRowsButton_Click(object sender, System.EventArgs e)

        Dim selectedRowCount As Int32 = dgvTiers.Rows.GetRowCount(DataGridViewElementStates.Selected)
        Dim i As Integer, idTiers As Integer

        If (selectedRowCount > 0) Then
            Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
            For i = 0 To selectedRowCount
                sb.Append("Row: ")
                sb.Append(dgvTiers.SelectedRows(i).Index.ToString())
                sb.Append(Environment.NewLine)
                i += 1
            Next
            sb.Append("Total: " + selectedRowCount.ToString())
            'MessageBox.Show(sb.ToString(), "Selected Rows")
        End If
        'TODO : il faut retrouver le Tiers
        dgvTiers.CurrentCell = dgvTiers.SelectedRows(0).Cells(0)
        Call ChargeDgvSousCategorie(FrmPrincipale.maConn, Tiers.getCategorieTiers(dgvTiers.SelectedRows(0).Cells(0).Value))
        'TODO : sélectionner la catégorie et la sous catégorie qui vont bien
        idTiers = Tiers.getCategorieTiers(dgvTiers.SelectedRows(0).Cells(0).Value)
        dgvCategorie.Item(1, idTiers).Selected = True
        dgvCategorie.CurrentCell = dgvCategorie.Rows(idTiers).Cells(0)
        dgvSousCategorie.Item(1, Tiers.getSousCategorieTiers(dgvTiers.SelectedRows(0).Cells(0).Value)).Selected = True
    End Sub
    Private Sub ChargeSousCategorie(indiceCategorie As Integer)
        Dim monReaderSousCategorie As SqlDataReader
        Dim maCmdSousCategorie As SqlCommand

        maCmdSousCategorie = New SqlCommand("SELECT libelle FROM SousCategorie where idCategorie =" & indiceCategorie & ";", FrmPrincipale.maConn)
        monReaderSousCategorie = maCmdSousCategorie.ExecuteReader()
        Do While monReaderSousCategorie.Read()
            Try
                Me.cbSousCategorie.Items.Add(monReaderSousCategorie.GetSqlString(0))
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        Loop
        monReaderSousCategorie.Close()
    End Sub

    Private Sub dgvTiers_UserAddedRow(sender As Object, e As DataGridViewRowEventArgs) Handles dgvTiers.UserAddedRow
        Dim bindingSource1 = New BindingSource()
        Dim id As Integer
        Dim sNom As String, sPrenom As String, sRaisonSociale As String

        id = CInt(dgvTiers.CurrentRow.Cells(0).Value.ToString)
        sNom = dgvTiers.CurrentRow.Cells(0).Value.ToString
        sPrenom = dgvTiers.CurrentRow.Cells(0).Value.ToString
        sRaisonSociale = dgvTiers.CurrentRow.Cells(0).Value.ToString
        Dim command As New System.Data.SqlClient.SqlCommand("insert into Tiers (id, nom, prenom, raisonSoicale) values ('" & id & "', '" & sNom & "' ,'" & sPrenom & "', '" & sRaisonSociale & "';", FrmPrincipale.maConn)

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
        frmNouveauTiers.show
    End Sub
End Class