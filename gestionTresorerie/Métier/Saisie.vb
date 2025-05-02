Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class FrmSaisie
    Inherits System.Windows.Forms.Form
    Private listeTiers As ListeTiers
    Private _Mvt As Mouvements
    Private _dtMvtsIdentiques As DataTable = Nothing
    Public Property Properties As Object
    Private Sub frmSaisie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            InitialiserListeTiers()
            Call initZonesSaisies()
            Dim indTiersDetecte As Integer = DetecteTiers()
            If indTiersDetecte > -1 Then
                SelectionnerTiers(indTiersDetecte)
            End If
            ChargerCategoriesEtSousCategories(indTiersDetecte)
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement du formulaire : {ex.Message}")
        End Try
    End Sub
    Private Sub initZonesSaisies()
        txtRechercheTiers.Text = String.Empty
        dgvCategorie.ClearSelection()
        dgvSousCategorie.ClearSelection()
        dgvTiers.ClearSelection()
        cbEvénement.SelectedItem = Nothing
    End Sub
    Private Sub InitialiserListeTiers()
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers()
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
        Call ChargeDgvTiers()
        'Chargement du fichier contenant la liste des événements 
        Call ChargeFichierTexte(Me.cbEvénement, LectureProprietes.GetCheminEtVariable("ficEvénement"))
        'Chargement du fichier contenant la liste des types 
        Call ChargeFichierTexte(Me.cbType, LectureProprietes.GetCheminEtVariable("ficType"))
    End Sub
    Private Function DetecteTiers() As Integer
        ' Essaie de déterminer le tiers en fonction du contenu de la note
        Dim sMots() As String = txtNote.Text.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim i As Integer = -1

        For Each sMot As String In sMots
            If Not String.IsNullOrWhiteSpace(sMot) Then
                i = listeTiers.getIdParRaisonSociale(sMot.ToUpper())
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
            Logger.ERR("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : ")
        End Try
    End Sub
    Private Sub ChargeDgvTiers()
        Dim command = SqlCommandBuilder.CreateSqlCommand("reqIdentiteCatTiers")
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
    Private Sub ChargeDgvCategorie(Optional debit As Boolean? = Nothing)
        Dim categorie As New Categorie()
        Dim query As String = "reqCategorieTout"
        Dim parameters As New Dictionary(Of String, Object) From {{"@debit", If(debit Is Nothing, DBNull.Value, debit)}}

        ChargerDonneesDansDataGridView(categorie, query, parameters, dgvCategorie)
    End Sub
    Private Sub ChargeDgvSousCategorie(idCategorie As Integer)
        Dim sousCategorie As New SousCategorie()
        Dim query As String = "reqSousCategorie"
        Dim parameters As New Dictionary(Of String, Object) From {{"@idCategorie", idCategorie}}

        ChargerDonneesDansDataGridView(sousCategorie, query, parameters, dgvSousCategorie)
    End Sub
    Private Sub ChargerDonneesDansDataGridView(dataService As IDataService, query As String, parameters As Dictionary(Of String, Object), dataGridView As DataGridView)

        Try
            Dim dt As DataTable = dataService.ExecuterRequete(query, parameters)

            dataGridView.DataSource = dt
            dataGridView.Columns("id").Visible = False
            dataGridView.Columns("libelle").Visible = True

            Logger.INFO($"Chargement des données réussi : {dataGridView.RowCount}")

            ' Vérifie si le DataGridView est vide
            If dataGridView.Rows.Count = 0 Then
                Logger.INFO($"Aucune ligne n'a été trouvée pour la requête spécifiée.")
                Return
            End If
        Catch ex As SqlException
            Logger.ERR($"Erreur SQL lors du chargement des données. Message: {ex.Message}")
            MessageBox.Show($"Une erreur SQL s'est produite lors du chargement des données !{vbCrLf}{ex}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors du chargement des données. Message: {ex.Message}")
            MessageBox.Show($"Une erreur inattendue s'est produite !{vbCrLf}{ex}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Call InsereChq()
        Me.Hide()
        Call initZonesSaisies()
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
        FrmHistogramme.ShowDialog()
    End Sub
    Private Sub dgvTiers_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTiers.CellContentClick
        ' Gérer les catégories et sous-catégories par défaut 

        If dgvTiers.Rows.GetRowCount(DataGridViewElementStates.Selected) > 0 Then
            ' Récupérer la valeur du 4ème champ du tiers sélectionné
            Dim idCategorieDefaut As Object = dgvTiers.SelectedRows(0).Cells(4).Value
            Dim idSousCategorieDefaut As Object = dgvTiers.SelectedRows(0).Cells(5).Value

            ' Charger les catégories  
            ChargeDgvCategorie()
            ' Sélectionner la ligne correspondante dans dgvCategorie
            SelectRowInDataGridView(dgvCategorie, idCategorieDefaut)

            ' Charger les sous-catégories
            majSousCategorie()
            ' Sélectionner la ligne correspondante dans dgvSousCategorie
            SelectRowInDataGridView(dgvSousCategorie, idSousCategorieDefaut)
        End If
    End Sub

    Private Sub SelectRowInDataGridView(dgv As DataGridView, id As Object)
        For Each row As DataGridViewRow In dgv.Rows
            If row.Cells(0).Value IsNot Nothing AndAlso row.Cells(0).Value.Equals(id) Then
                row.Selected = True
                dgv.FirstDisplayedScrollingRowIndex = row.Index
                Exit For
            End If
        Next
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
    Private Sub selectionneLigneParLibelle(dgv As DataGridView, libelle As String)
        ' Sélectionne la ligne dont le libellé correspond au paramètre (sur le nombre de caractères renseignés)
        If libelle.Length > 1 Then
            Dim libelleMajuscule As String = libelle.ToUpper()

            For Each row As DataGridViewRow In dgv.Rows
                If Not row.IsNewRow Then
                    For Each cellIndex As Integer In {1, 3}
                        Dim cellValue As Object = row.Cells(cellIndex).Value
                        If Not IsDBNull(cellValue) AndAlso cellValue.ToString().StartsWith(libelleMajuscule, StringComparison.CurrentCultureIgnoreCase) Then
                            afficheLigneTrouvée(dgv, row.Index)
                            Exit For
                        End If
                    Next
                End If
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
            _Mvt = mouvement
            _dtMvtsIdentiques = Mouvements.ChargerMouvementsSimilaires(mouvement)
            If _dtMvtsIdentiques.Rows.Count > 0 Then
                'Un mouvement identique existe déjà
                Dim frmListe As New FrmListe(_dtMvtsIdentiques)
                AddHandler frmListe.objetSelectionneChanged, AddressOf mvtSelectionneChangedHandler
                frmListe.ShowDialog()
                'frmListe.Show()
                Logger.INFO("Le mouvement existe déjà : " & mouvement.ObtenirValeursConcatenees)
            Else
                InsererMouvementEnBase(mouvement)
                Logger.INFO("Insertion du mouvement pour : " & mouvement.ObtenirValeursConcatenees)
            End If
        Catch ex As Exception
            MsgBox("Echec de l'insertion en base : " & ex.Message)
            Logger.ERR("Erreur lors de l'insertion des données : " & ex.Message)
        End Try
    End Sub
    Private Sub mvtSelectionneChangedHandler(sender As Object, index As Integer)
        Dim rowsAffected As Integer
        ' Vérifier si l'objet peut être converti en Mouvements
        If index = -1 Then
            Logger.INFO("L'objet sélectionné est nul => mouvement à insérer")
            InsererMouvementEnBase(_Mvt)
        Else
            rowsAffected = Mouvements.MettreAJourMouvement(
                     _dtMvtsIdentiques.Rows(index).ItemArray(0),
                     dgvCategorie.Rows(dgvCategorie.SelectedRows(0).Index).Cells(0).Value,
                     dgvSousCategorie.Rows(dgvSousCategorie.SelectedRows(0).Index).Cells(0).Value,
                     txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty),
                     rbCredit.Checked,
                     Convert.ToInt32(dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(0).Value),
                     txtNote.Text,
                     dateMvt.Value,
                     rbRapproche.Checked,
                     cbEvénement.SelectedItem,
                     cbType.SelectedItem,
                     True,
                     GetRemiseValue(txtRemise.Text),
                     0
                     )
            ' Trace indiquant le nombre de lignes mises à jour
            Logger.INFO($"Nombre de mouvements mis à jour : {rowsAffected}")
        End If
    End Sub
    Private Function GetRemiseValue(texteRemise As String) As Integer

        If String.IsNullOrEmpty(texteRemise) Then
            Return 0 ' Retourne 0 si le texte est vide
        End If

        Dim remiseValue As Integer
        If Integer.TryParse(texteRemise, remiseValue) Then
            Return remiseValue ' Retourne la valeur convertie en entier
        Else
            Return 0 ' Retourne 0 si la conversion échoue
        End If
    End Function
    Private Function CreerMouvement() As Mouvements

        Return New Mouvements(
            txtNote.Text,
            dgvCategorie.Rows(dgvCategorie.SelectedRows(0).Index).Cells(0).Value.ToString(),
            dgvSousCategorie.Rows(dgvSousCategorie.SelectedRows(0).Index).Cells(0).Value.ToString(),
            Convert.ToInt32(dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(0).Value),
            dateMvt.Value,
            txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty),
            rbCredit.Checked,
            rbRapproche.Checked,
            cbEvénement.SelectedItem,
            cbType.SelectedItem,
            False,
            txtRemise.Text,
            0
        )
    End Function
    Private Sub InsererMouvementEnBase(mouvement As Mouvements)
        Try
            SqlCommandBuilder.
                CreateSqlCommand("insertMvts",
                                 New Dictionary(Of String, Object) From
                                                         {{"@note", mouvement.Note},
                                                         {"@categorie", mouvement.Categorie},
                                                         {"@sousCategorie", mouvement.SousCategorie},
                                                         {"@tiers", mouvement.Tiers},
                                                         {"@dateCréation", DateTime.Now},
                                                         {"@dateMvt", mouvement.DateMvt},
                                                         {"@montant", Utilitaires.ConvertToDecimal(mouvement.Montant)},
                                                         {"@sens", mouvement.Sens},
                                                         {"@etat", mouvement.Etat},
                                                         {"@événement", mouvement.Événement},
                                                         {"@type", mouvement.Type},
                                                         {"@modifiable", mouvement.Modifiable},
                                                         {"@numeroRemise", mouvement.NumeroRemise},
                                                         {"@idCheque", mouvement.idCheque}}
                             ).ExecuteNonQuery()
            Logger.INFO("Insertion du mouvement réussie : " & mouvement.ObtenirValeursConcatenees)
        Catch ex As SqlException
            Logger.ERR("Erreur SQL lors de l'insertion du mouvement : " & ex.Message & vbCrLf & "Mouvement : " & mouvement.ObtenirValeursConcatenees)
            Throw ' Re-lancer l'exception après l'avoir loggée
        Catch ex As Exception
            Logger.ERR("Erreur générale lors de l'insertion du mouvement : " & ex.Message)
            Throw ' Re-lancer l'exception après l'avoir loggée
        End Try
    End Sub

    'Private Function AjouteParam(cmd As SqlCommand, mouvement As Mouvements) As SqlCommand
    '    Dim montant As Decimal

    '    ' Convertir la chaîne de caractères en Decimal en utilisant la culture française pour gérer les virgules
    '    If Decimal.TryParse(mouvement.Montant, NumberStyles.Currency, CultureInfo.GetCultureInfo("fr-FR"), montant) Then
    '        With cmd.Parameters
    '            .AddWithValue("@note", mouvement.Note)
    '            .AddWithValue("@categorie", mouvement.Categorie)
    '            .AddWithValue("@sousCategorie", mouvement.SousCategorie)
    '            .AddWithValue("@tiers", mouvement.Tiers)
    '            .AddWithValue("@dateCréation", DateTime.Now)
    '            .AddWithValue("@dateMvt", mouvement.DateMvt)
    '            .AddWithValue("@montant", montant) ' Utiliser la variable montant convertie
    '            .AddWithValue("@sens", mouvement.Sens)
    '            .AddWithValue("@etat", mouvement.Etat)
    '            .AddWithValue("@événement", mouvement.Événement)
    '            .AddWithValue("@type", mouvement.Type)
    '            .AddWithValue("@modifiable", mouvement.Modifiable)
    '            .AddWithValue("@numeroRemise", mouvement.NumeroRemise)
    '            .AddWithValue("@idCheque", mouvement.idCheque)
    '        End With
    '        Logger.INFO("Création du mouvement : " & mouvement.ObtenirValeursConcatenees)
    '    Else
    '        Logger.ERR("Erreur de conversion du montant : " & mouvement.Montant)
    '        Throw New InvalidOperationException("Le montant n'est pas un nombre valide.")
    '    End If

    '    Return cmd
    'End Function

    Private Sub btnSelChq_Click(sender As Object, e As EventArgs) Handles btnSelChq.Click
        Dim selectionneCheque As New FrmSelectionneCheque()
        AddHandler selectionneCheque.IdChequeSelectionneChanged, AddressOf IdChequeSelectionneChangedHandler
        selectionneCheque.ShowDialog()

        selectionneCheque.chargeListeChq(CDec(Me.txtMontant.Text))
        selectionneCheque.Show()
    End Sub

    Private Sub IdChequeSelectionneChangedHandler(ByVal idCheque As Integer)
        'Mettre à jour le Mouvement 
        MettreAJourIdCheque(_Mvt.Id, idCheque)
    End Sub
    Public Shared Sub MettreAJourIdCheque(idMouvement As Integer, nouvelIdCheque As Integer)
        Try
            Dim rowsAffected As Integer = SqlCommandBuilder.CreateSqlCommand("updMvtIdChq",
                                      New Dictionary(Of String, Object) From {
                                      {"@nouvelIdCheque", nouvelIdCheque},
                                      {"@idMouvement", idMouvement}}).ExecuteNonQuery
            If rowsAffected > 0 Then
                Logger.INFO($"Mise à jour réussie de idCheque pour le mouvement avec Id = {idMouvement}")
            Else
                Logger.WARN($"Aucune ligne n'a été mise à jour pour le mouvement avec Id = {idMouvement}")
            End If
        Catch ex As SqlException
            Logger.ERR($"Erreur SQL lors de la mise à jour de idCheque. Message: {ex.Message}")
            Throw
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de la mise à jour de idCheque. Message: {ex.Message}")
        Throw
        End Try
    End Sub

    Public Shared Function CreerJsonCheque(_id As Integer, _montant_numerique As Decimal, _numero_du_cheque As String, _dateChq As DateTime, _emetteur_du_cheque As String, _destinataire As String) As String
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

    Private Sub btnCreerTiers_Click(sender As Object, e As EventArgs) Handles btnCreerTiers.Click
        frmNouveauTiers.Show()
    End Sub
End Class