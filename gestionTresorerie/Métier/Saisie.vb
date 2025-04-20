Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json
Imports System.Globalization
Imports System.Reflection.Metadata

Public Class FrmSaisie
    Inherits System.Windows.Forms.Form
    Private maCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String
    Private listeTiers As ListeTiers
    Private _idCheque As Integer = 0
    Private _dtMvtsIdentiques As DataTable = Nothing
    Public Property Properties As Object

    Private Sub frmSaisie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            InitialiserListeTiers()
            Dim indTiersDetecte As Integer = DetecteTiers()
            If indTiersDetecte > -1 Then
                SelectionnerTiers(indTiersDetecte)
            End If
            ChargerCategoriesEtSousCategories(indTiersDetecte)
        Catch ex As Exception
            Logger.GetInstance.ERR($"Erreur lors du chargement du formulaire : {ex.Message}")
        End Try
    End Sub

    Private Sub InitialiserListeTiers()
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers(connexionDB.GetInstance.getConnexion)
        End If
    End Sub

    Private Sub SelectionnerTiers(indTiersDetecte As Integer)
        Dim idTiers As Integer = chercheIndiceDvg(indTiersDetecte, dgvTiers)
        dgvTiers.Rows(idTiers).Selected = True
        dgvTiers.FirstDisplayedScrollingRowIndex = idTiers
    End Sub

    Private Sub ChargerCategoriesEtSousCategories(indTiersDetecte As Integer)
        If dgvCategorie.RowCount = 0 Then
            ChargeDgvCategorie(rbDebit.Checked)
        End If

        Dim idCategorie As Integer = chercheIndiceDvg(Tiers.getCategorieTiers(indTiersDetecte), dgvCategorie)
        dgvCategorie.Rows(idCategorie).Selected = True
        dgvCategorie.FirstDisplayedScrollingRowIndex = idCategorie

        If dgvSousCategorie.RowCount = 0 Then
            ChargeDgvSousCategorie(Tiers.getCategorieTiers(indTiersDetecte))
        End If

        Dim idSousCategorie As Integer = chercheIndiceDvg(Tiers.getSousCategorieTiers(indTiersDetecte), dgvSousCategorie)
        dgvSousCategorie.Rows(idSousCategorie).Selected = True
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

    Public Sub chargeListes()

        Call ChargeDgvTiers(connexionDB.GetInstance.getConnexion)
        'Chargement du fichier contenant la liste des événements 
        Call ChargeFichierTexte(Me.cbEvénement, lectureProprietes.GetVariable("ficEvénement"))
        'Chargement du fichier contenant la liste des types 
        Call ChargeFichierTexte(Me.cbType, lectureProprietes.GetVariable("ficType"))
    End Sub
    Private Function DetecteTiers() As Integer
        'Essaie de déterminer le tiers en fonction du contenu de la note
        Dim sMots() As String, sMot As String
        Dim i As Integer = -1

        sMots = Split(txtNote.Text, Space(1),, CompareMethod.Text)
        'TODO : voir pourquoi il me ramène plein de mots vides
        For Each sMot In sMots
            If Not sMot.Equals(String.Empty) Then
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
            Logger.GetInstance.ERR("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : ")
        End Try
    End Sub

    Private Sub ChargeDgvTiers(maConn As SqlConnection)
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

    Private Sub ChargeDgvCategorie(debit As Boolean)
        Dim query As String = "SELECT id, libelle FROM Categorie WHERE debit = @debit;"
        Dim dt As New DataTable

        Try
            Dim conn As SqlConnection = connexionDB.GetInstance.getConnexion
            Using command As New SqlCommand(query, conn)
                'TODO : vilain false => 0, true (-1) => 1 avec math.abs
                command.Parameters.AddWithValue("@debit", Math.Abs(CInt(debit)))

                Using adpt As New SqlDataAdapter(command)
                    adpt.Fill(dt)
                End Using
            End Using

            dgvCategorie.DataSource = dt
            dgvCategorie.Columns("id").Visible = False
            dgvCategorie.Columns("libelle").Visible = True

            Logger.GetInstance.INFO("Chargement des catégories réussi : " & dgvCategorie.RowCount)
        Catch ex As SqlException
            Logger.GetInstance.ERR($"Erreur lors du chargement des catégories. Message: {ex.Message}")
            MessageBox.Show("ChargeDgvCategorie : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        End Try
    End Sub
    Private Sub ChargeDgvSousCategorie(idCategorie As Integer)

        Dim command As New System.Data.SqlClient.SqlCommand("SELECT id, libelle FROM SousCategorie where idCategorie = @idCategorie;", connexionDB.GetInstance.getConnexion)
        command.Parameters.AddWithValue("@idCategorie", idCategorie)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connexion dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception.
            adpt.Fill(dt)
            dgvSousCategorie.DataSource = dt
            Logger.GetInstance.INFO("Chargement des sous catégories réussi : " & dgvSousCategorie.RowCount)

            ' Vérifie si le DataGridView est vide
            If dgvSousCategorie.Rows.Count = 0 Then
                Logger.GetInstance.INFO("ChargeDgvSousCategorie : aucune ligne n'a été trouvée pour la catégorie spécifiée.")
                Logger.GetInstance.INFO("Ajouter une entrée dans la table SousCategorie.")
                'TODO : orienter vers une fenêtre qui permettra d'ajouter une entrée
                End
            End If
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            Logger.GetInstance.ERR("ChargeDgvSousCategorie : une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        Catch ex As Exception
            ' Gère toutes les autres erreurs
            Logger.GetInstance.ERR("ChargeDgvSousCategorie : une erreur inattendue s'est produite !" & vbCrLf & ex.ToString())
        End Try

        dgvSousCategorie.Columns("id").Visible = False
        dgvSousCategorie.Columns("libelle").Visible = True
    End Sub

    'Private Sub CbCategorie_SelectedIndexChanged(sender As Object, e As EventArgs)
    '    cbSousCategorie.Items.Clear
    'End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Call InsereChq()
        Me.Hide()
        FrmPrincipale.Show()
    End Sub

    Private Sub TxtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        If Not Regex.Match(txtMontant.Text, Constantes.regExMontant, RegexOptions.IgnoreCase).Success Then
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
            sb.Append("Total: " & selectedRowCount.ToString())
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
            Call ChargeDgvSousCategorie(dgvCategorie.SelectedRows(0).Cells(0).Value)
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
    Private Sub InsereChq()
        Try
            'Les infos de création du mouvement sont récupérées sur la fenêtre de saisie
            Dim mouvement As Mouvements = CreerMouvement()
            _dtMvtsIdentiques = Mouvements.ChargerMouvementsSimilaires(mouvement)
            If _dtMvtsIdentiques.Rows.Count > 0 Then
                'Un mouvement identique existe déjà
                Dim frmListe As New frmListe(_dtMvtsIdentiques)
                AddHandler frmListe.objetSelectionneChanged, AddressOf mvtSelectionneChangedHandler
                frmListe.ShowDialog()
                'frmListe.Show()
                Logger.GetInstance.INFO("Le mouvement existe déjà : " & mouvement.ObtenirValeursConcatenees)
            Else
                InsererMouvementEnBase(mouvement)
                Logger.GetInstance.INFO("Insertion du mouvement pour : " & mouvement.ObtenirValeursConcatenees)
            End If
        Catch ex As Exception
            MsgBox("Echec de l'insertion en base : " & ex.Message)
            Logger.GetInstance.ERR("Erreur lors de l'insertion des données : " & ex.Message)
        End Try
    End Sub

    Private Sub mvtSelectionneChangedHandler(sender As Object, index As Integer)
        ' Vérifier si l'objet peut être converti en Mouvements
        If index = -1 Then
            Logger.GetInstance.ERR("L'objet sélectionné est nul => mouvement à insérer")
        Else
            With _dtMvtsIdentiques.Rows(index)
                'Dim mvtModif As New Mouvements(.ItemArray(6), .ItemArray(1), .ItemArray(2), .ItemArray(5), .ItemArray(7), .ItemArray(3), .ItemArray(4), .ItemArray(10), .ItemArray(11), .ItemArray(12), .ItemArray(13), .ItemArray(14), .ItemArray(15))
                'Mouvements.MettreAJourMouvement(mvtModif.Categorie, mvtModif.SousCategorie, mvtModif.Montant, mvtModif.Sens, mvtModif.Tiers, mvtModif.Note, mvtModif.DateCréation, mvtModif.Etat, mvtModif.Événement, mvtModif.Type, mvtModif.Modifiable, mvtModif.NumeroRemise, mvtModif.idCheque)
                'TODO : récupérer les valeurs dans la fenêtre
                Mouvements.MettreAJourMouvement(.ItemArray(1), .ItemArray(2), .ItemArray(3), .ItemArray(4), .ItemArray(5), .ItemArray(6), .ItemArray(7), .ItemArray(10), .ItemArray(11), .ItemArray(12), .ItemArray(13), .ItemArray(14), .ItemArray(15))
            End With
        End If
    End Sub

    Private Function CreerMouvement() As Mouvements
        Dim iCategorie As Integer = dgvCategorie.Rows(dgvCategorie.SelectedRows(0).Index).Cells(0).Value.ToString()
        Dim iSousCategorie As Integer = dgvSousCategorie.Rows(dgvSousCategorie.SelectedRows(0).Index).Cells(0).Value.ToString()
        Dim idTiers As Integer = Convert.ToInt32(dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(0).Value)

        Return New Mouvements(
            txtNote.Text,
            iCategorie,
            iSousCategorie,
            idTiers,
            dateMvt.Value,
            txtMontant.Text.Trim().Replace(" ", ""),
            rbCredit.Checked,
            rbRapproche.Checked,
            cbEvénement.SelectedItem,
            cbType.SelectedItem,
            False,
            txtRemise.Text,
            _idCheque
        )
    End Function

    Private Sub InsererMouvementEnBase(mouvement As Mouvements)
        Dim query As String = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers, dateCréation, dateMvt, montant, sens, etat, événement, type, modifiable, numeroRemise, idCheque) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement, @type, @modifiable, @numeroRemise, @idCheque);"

        Try
            Dim conn As SqlConnection = connexionDB.GetInstance.getConnexion
            Dim cmd As New SqlCommand(query, conn)
            cmd = AjouteParam(cmd, mouvement)
            cmd.ExecuteNonQuery()
            Logger.GetInstance.INFO("Insertion du mouvement réussie : " & mouvement.ObtenirValeursConcatenees)
        Catch ex As SqlException
            Logger.GetInstance.ERR("Erreur SQL lors de l'insertion du mouvement : " & ex.Message & vbCrLf & "Mouvement : " & mouvement.ObtenirValeursConcatenees)
            Throw ' Re-lancer l'exception après l'avoir loggée
        Catch ex As Exception
            Logger.GetInstance.ERR("Erreur générale lors de l'insertion du mouvement : " & ex.Message)
            Throw ' Re-lancer l'exception après l'avoir loggée
        End Try
    End Sub

    Private Function AjouteParam(cmd As SqlCommand, mouvement As Mouvements) As SqlCommand
        Dim montant As Decimal

        ' Convertir la chaîne de caractères en Decimal en utilisant la culture française pour gérer les virgules
        If Decimal.TryParse(mouvement.Montant, NumberStyles.Currency, CultureInfo.GetCultureInfo("fr-FR"), montant) Then
            With cmd.Parameters
                .AddWithValue("@note", mouvement.Note)
                .AddWithValue("@categorie", mouvement.Categorie)
                .AddWithValue("@sousCategorie", mouvement.SousCategorie)
                .AddWithValue("@tiers", mouvement.Tiers)
                .AddWithValue("@dateCréation", DateTime.Now)
                .AddWithValue("@dateMvt", mouvement.DateMvt)
                .AddWithValue("@montant", montant) ' Utiliser la variable montant convertie
                .AddWithValue("@sens", mouvement.Sens)
                .AddWithValue("@etat", mouvement.Etat)
                .AddWithValue("@événement", mouvement.Événement)
                .AddWithValue("@type", mouvement.Type)
                .AddWithValue("@modifiable", mouvement.Modifiable)
                .AddWithValue("@numeroRemise", mouvement.NumeroRemise)
                .AddWithValue("@idCheque", mouvement.idCheque)
            End With
            Logger.GetInstance.INFO("Création du mouvement : " & mouvement.ObtenirValeursConcatenees)
        Else
            Logger.GetInstance.ERR("Erreur de conversion du montant : " & mouvement.Montant)
            Throw New InvalidOperationException("Le montant n'est pas un nombre valide.")
        End If

        Return cmd
    End Function

    'Private Function AjouteParam(myCmd As SqlCommand, unMvt As Mouvements) As SqlCommand
    '    With myCmd.Parameters
    '        .Clear()
    '        .Add("@note", SqlDbType.NVarChar)
    '        .Item(0).Value = unMvt.Note
    '        .Add("@categorie", SqlDbType.VarChar)
    '        .Item(1).Value = unMvt.Categorie
    '        .Add("@sousCategorie", SqlDbType.VarChar)
    '        .Item(2).Value = unMvt.SousCategorie
    '        .Add("@tiers", SqlDbType.Decimal)
    '        .Item(3).Value = unMvt.Tiers
    '        .Add("@dateCréation", SqlDbType.Date)
    '        .Item(4).Value = Now.Date
    '        .Add("@dateMvt", SqlDbType.Date)
    '        .Item(5).Value = unMvt.DateMvt
    '        .Add("@montant", SqlDbType.Decimal)
    '        .Item(6).Value = unMvt.Montant
    '        .Add("@sens", SqlDbType.Bit)
    '        .Item(7).Value = unMvt.Sens
    '        .Add("@etat", SqlDbType.Bit)
    '        .Item(8).Value = unMvt.Etat
    '        .Add("@événement", SqlDbType.VarChar)
    '        .Item(9).Value = unMvt.Événement
    '        .Add("@type", SqlDbType.VarChar)
    '        .Item(10).Value = unMvt.Type
    '        .Add("@modifiable", SqlDbType.Bit)
    '        .Item(11).Value = unMvt.Modifiable
    '        .Add("@numeroRemise", SqlDbType.Int)
    '        .Item(12).Value = unMvt.NumeroRemise
    '        .Add("@idCheque", SqlDbType.Int)
    '        .Item(13).Value = unMvt.idCheque
    '    End With
    '    Return myCmd
    'End Function

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

End Class