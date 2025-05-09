Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class FrmSaisie
    Inherits System.Windows.Forms.Form

    Private Const Prompt As String = "Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}"
    Private listeTiers As ListeTiers
    Private _Mvt As Mouvements
    Private _dtMvtsIdentiques As DataTable = Nothing
    Public Property Properties As Object
    Private isExpanded As Boolean = True
    Private Sub FrmSaisie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            InitialiserListeTiers()
            Call initZonesSaisies()
            Dim indTiersDetecte As Integer = listeTiers.DetecteTiers(txtNote.Text)
            If indTiersDetecte > -1 Then
                SelectionnerTiers(indTiersDetecte)
            End If
            ChargerCategoriesEtSousCategories(indTiersDetecte)
            'Sélectionne le type de mouvement associé à la note 
            ' Utiliser le dictionnaire pour sélectionner les lignes du DataGridView
            chercheType(Utilitaires.ChargerCriteresDepuisConfig(Constantes.dicoTypeMvt))
            ' Initialiser les boutons
            InitializeToggleButton(btnToggleEvt, pnlDgvEvt, 200, 50)
            InitializeToggleButton(btnToggleType, pnlDgvType, 200, 50)
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement du formulaire : {ex.Message}")
            End
        End Try
    End Sub
    Private Sub initZonesSaisies()
        txtRechercheTiers.Text = String.Empty
        dgvCategorie.ClearSelection()
        dgvSousCategorie.ClearSelection()
        dgvTiers.ClearSelection()
        cbEvénement.SelectedItem = Nothing
        ' Initialiser l'état du bouton
        btnToggleEvt.Text = "Réduire"
    End Sub
    Private Sub InitialiserListeTiers()
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers()
        End If
    End Sub
    Private Sub SelectionnerTiers(indTiersDetecte As Integer)
        Dim idTiers As Integer = UtilitairesDgv.chercheIndiceDvg(indTiersDetecte, dgvTiers)
        dgvTiers.Rows(idTiers).Selected = True
        dgvTiers.FirstDisplayedScrollingRowIndex = idTiers
    End Sub
    Private Sub ChargerCategoriesEtSousCategories(indTiersDetecte As Integer)
        Dim parameters As Dictionary(Of String, Object)

        Dim sRequete As String
        If dgvCategorie.RowCount = 0 Then
            parameters = New Dictionary(Of String, Object) From {{"@debit", If(rbDebit.Checked, 1, 0)}}
            Call UtilitairesDgv.ChargeDgvGenerique(dgvCategorie, Constantes.sqlSelCategoriesTout, parameters)
        End If

        Dim idCategorie As Integer = UtilitairesDgv.chercheIndiceDvg(Tiers.getCategorieTiers(indTiersDetecte), dgvCategorie)
        sRequete = Constantes.sqlSelSousCategories
        parameters = New Dictionary(Of String, Object) From {{"@idCategorie", Tiers.getCategorieTiers(indTiersDetecte)}}
        dgvCategorie.Rows(idCategorie).Selected = True
        dgvCategorie.FirstDisplayedScrollingRowIndex = idCategorie

        If dgvSousCategorie.RowCount = 0 Then
            Call UtilitairesDgv.ChargeDgvGenerique(dgvSousCategorie, sRequete, parameters)
        End If

        Dim idSousCategorie As Integer = UtilitairesDgv.chercheIndiceDvg(Tiers.getSousCategorieTiers(indTiersDetecte), dgvSousCategorie)
        dgvSousCategorie.Rows(idSousCategorie).Selected = True
        dgvSousCategorie.FirstDisplayedScrollingRowIndex = idSousCategorie
    End Sub
    Public Sub chargeListes()
        Call UtilitairesDgv.ChargeDgvGenerique(dgvTiers, Constantes.sqlSelIdentiteCatTiers)
        'Chargement des événements  
        Call UtilitairesDgv.ChargeDgvGenerique(dgvEvenement, Constantes.sqlSelEvenement)
        'Chargement de la liste des types 
        Call UtilitairesDgv.ChargeDgvGenerique(dgvType, Constantes.sqlSelTypes)
    End Sub
    'Private Sub ChargeDgvGenerique(dgv As DataGridView, sRequete As String, Optional parameters As Dictionary(Of String, Object) = Nothing)
    '    Dim utilitairesDgv As New UtilitairesDgv

    '    Try
    '        dgv.DataSource = utilitairesDgv.ExecuterRequete(sRequete, parameters)
    '        Logger.INFO($"Chargement de {dgv.Name} avec la requête {sRequete} réussi. {dgv.Rows.Count} lignes chargées")
    '    Catch ex As SqlException
    '        ' On informe l'utilisateur qu'il y a eu un problème :
    '        MessageBox.Show($"Une erreur s'est produite lors du chargement des données ! : {ex}")
    '        Logger.ERR($"Une erreur s'est produite lors du chargement des données ! : {ex}")
    '    End Try
    'End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Call InsereMouvement()
        Me.Hide()
        Call initZonesSaisies()
        If Not btnNouveauChq.Visible Then
            FrmPrincipale.Show()
        End If
    End Sub
    Private Sub TxtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        If Not Regex.Match(txtMontant.Text, Constantes.regExMontant, RegexOptions.IgnoreCase).Success Then
            MessageBox.Show($"Le montant {txtMontant.Text} doit être numérique!")
            'Remet le focus sur la zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub
    Private Sub dgvTiers_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTiers.CellContentClick
        ' Gérer les catégories et sous-catégories par défaut 

        If dgvTiers.Rows.GetRowCount(DataGridViewElementStates.Selected) > 0 Then
            ' Récupérer la valeur du 4ème champ du tiers sélectionné
            Dim idCategorieDefaut As Object = dgvTiers.SelectedRows(0).Cells(4).Value
            Dim idSousCategorieDefaut As Object = dgvTiers.SelectedRows(0).Cells(5).Value

            ' Charger les catégories   
            UtilitairesDgv.ChargeDgvGenerique(dgvTiers, Constantes.sqlSelCategoriesTout)
            ' Sélectionner la ligne correspondante dans dgvCategorie
            UtilitairesDgv.SelectRowInDataGridView(dgvCategorie, idCategorieDefaut)

            ' Charger les sous-catégories
            majSousCategorie()
            ' Sélectionner la ligne correspondante dans dgvSousCategorie
            UtilitairesDgv.SelectRowInDataGridView(dgvSousCategorie, idSousCategorieDefaut)
        End If
    End Sub
    Private Sub btnInsereTiers_Click(sender As Object, e As EventArgs) Handles btnInsereTiers.Click
        FrmNouveauTiers.Show()
    End Sub
    Private Sub dgvCategorie_DoubleClick(sender As Object, e As EventArgs) Handles dgvCategorie.DoubleClick
        Call majSousCategorie()
    End Sub
    Private Sub dgvCategorie_Click(sender As Object, e As EventArgs) Handles dgvCategorie.Click
        Call majSousCategorie()
    End Sub
    Private Sub majSousCategorie()
        If dgvCategorie.SelectedRows(0).Cells(0) IsNot Nothing Then
            Dim parameters As New Dictionary(Of String, Object) From {{"@idCategorie", dgvCategorie.SelectedRows(0).Cells(0).Value}}
            Call UtilitairesDgv.ChargeDgvGenerique(dgvSousCategorie, Constantes.sqlSelCategoriesTout, parameters)
        End If
    End Sub
    Private Sub txtRechercheTiers_TextChanged(sender As Object, e As EventArgs) Handles txtRechercheTiers.TextChanged
        Utilitaires.selLigneDgvParLibelle(dgvTiers, txtRechercheTiers.Text)
    End Sub
    Private Sub InsereMouvement()
        Dim mouvement As Mouvements
        Try
            'Les infos de création du mouvement sont récupérées sur la fenêtre de saisie
            mouvement = CreerMouvement()
            _Mvt = mouvement
            _dtMvtsIdentiques = Mouvements.ChargerMouvementsSimilaires(mouvement)
            If _dtMvtsIdentiques.Rows.Count > 0 Then
                'Un mouvement identique existe déjà
                Dim frmListe As New FrmListe(_dtMvtsIdentiques)
                AddHandler frmListe.objetSelectionneChanged, AddressOf mvtSelectionneChangedHandler
                frmListe.ShowDialog()
                Logger.INFO("Le mouvement existe déjà : " & mouvement.ObtenirValeursConcatenees)
            Else
                Mouvements.InsererMouvementEnBase(mouvement)
                Logger.INFO("Insertion du mouvement pour : " & mouvement.ObtenirValeursConcatenees)
                Logger.INFO(Prompt)
            End If
        Catch ex As Exception
            MsgBox(Prompt)
            Logger.ERR(Prompt)
        End Try
    End Sub
    'Private Sub mvtSelectionneChangedHandler(sender As Object, index As Integer)
    '    Dim rowsAffected As Integer
    '    ' Vérifier si l'objet peut être converti en Mouvements
    '    If index = -1 Then
    '        Logger.INFO("L'objet sélectionné est nul => mouvement à insérer")
    '        Mouvements.InsererMouvementEnBase(_Mvt)
    '    Else
    '        rowsAffected = Mouvements.MettreAJourMouvement(
    '                 _dtMvtsIdentiques.Rows(index).ItemArray(0),
    '                 dgvCategorie.Rows(dgvCategorie.SelectedRows(0).Index).Cells(0).Value,
    '                 dgvSousCategorie.Rows(dgvSousCategorie.SelectedRows(0).Index).Cells(0).Value,
    '                 txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty),
    '                 rbCredit.Checked,
    '                 Convert.ToInt32(dgvTiers.Rows(dgvTiers.SelectedRows(0).Index).Cells(0).Value),
    '                 txtNote.Text,
    '                 dateMvt.Value,
    '                 rbRapproche.Checked,
    '                 dgvEvenement.SelectedRows(0).Cells(1).Value.ToString(),
    '                 dgvType.SelectedRows(0).Cells(1).Value.ToString(),
    '                 True,
    '                 GetRemiseValue(txtRemise.Text),
    '                 0
    '                 )
    '        ' Trace indiquant le nombre de lignes mises à jour
    '        Logger.INFO($"Nombre de mouvements mis à jour : {rowsAffected}")
    '    End If
    'End Sub
    Private Sub mvtSelectionneChangedHandler(sender As Object, index As Integer)
        Try
            ' Vérifier si l'objet peut être converti en Mouvements
            If index = -1 Then
                Logger.INFO("L'objet sélectionné est nul => mouvement à insérer")
                Mouvements.InsererMouvementEnBase(_Mvt)
            Else
                ' Utiliser des variables intermédiaires pour rendre le code plus lisible
                Dim id As Integer = _dtMvtsIdentiques.Rows(index).ItemArray(0)
                Dim categorie As String = dgvCategorie.SelectedRows(0).Cells(0).Value.ToString()
                Dim sousCategorie As String = dgvSousCategorie.SelectedRows(0).Cells(0).Value.ToString()
                Dim montant As String = txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty)
                Dim credit As Boolean = rbCredit.Checked
                Dim tiers As Integer = Convert.ToInt32(dgvTiers.SelectedRows(0).Cells(0).Value)
                Dim note As String = txtNote.Text
                Dim dateMouvement As Date = dateMvt.Value
                Dim rapproche As Boolean = rbRapproche.Checked
                Dim evenement As String = dgvEvenement.SelectedRows(0).Cells(1).Value.ToString()
                Dim type As String = dgvType.SelectedRows(0).Cells(1).Value.ToString()
                Dim modifiable As Boolean = True
                Dim remise As Integer = GetRemiseValue(txtRemise.Text)
                Dim idCheque As Integer = 0

                ' Mettre à jour le mouvement
                Dim rowsAffected As Integer = Mouvements.MettreAJourMouvement(
                id, categorie, sousCategorie, montant, credit, tiers, note, dateMouvement, rapproche, evenement, type, modifiable, remise, idCheque
            )
                ' Trace indiquant le nombre de lignes mises à jour
                Logger.INFO($"Nombre de mouvements mis à jour : {rowsAffected}")
            End If
        Catch ex As Exception
            ' Log des exceptions
            Logger.ERR($"Erreur lors de la mise à jour du mouvement : {ex.Message}")
        End Try
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
            note:=txtNote.Text,
            categorie:=dgvCategorie.SelectedRows(0).Cells(0).Value.ToString(),
            sousCategorie:=dgvSousCategorie.SelectedRows(0).Cells(0).Value.ToString(),
            tiers:=Convert.ToInt32(dgvTiers.SelectedRows(0).Cells(0).Value),
            dateMvt:=dateMvt.Value,
            montant:=txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty),
            sens:=rbCredit.Checked,
            etat:=rbRapproche.Checked,
            événement:=dgvEvenement.SelectedRows(0).Cells(1).Value.ToString(),
            type:=dgvType.SelectedRows(0).Cells(1).Value.ToString(),
            modifiable:=False,
            numeroRemise:=txtRemise.Text,
            idCheque:=0
            )
    End Function
    Private Sub btnSelChq_Click(sender As Object, e As EventArgs) Handles btnSelChq.Click
        Dim selectionneCheque As New FrmSelectionneCheque()
        AddHandler selectionneCheque.IdChequeSelectionneChanged, AddressOf IdChequeSelectionneChangedHandler
        selectionneCheque.ShowDialog()

        selectionneCheque.chargeListeChq(CDec(Me.txtMontant.Text))
        selectionneCheque.Show()
    End Sub
    Private Sub IdChequeSelectionneChangedHandler(ByVal idCheque As Integer)
        'Mettre à jour le Mouvement 
        Mouvements.MettreAJourIdCheque(_Mvt.Id, idCheque)
    End Sub
    Private Sub btnCreerTiers_Click(sender As Object, e As EventArgs) Handles btnCreerTiers.Click
        frmNouveauTiers.Show()
    End Sub
    Private Sub InitializeToggleButton(btnToggle As Button, pnlDataGridView As Panel, expandedHeight As Integer, reducedHeight As Integer)
        ' Définir l'état initial
        Dim isExpanded As Boolean = True

        ' Ajouter un gestionnaire d'événements pour le bouton
        AddHandler btnToggle.Click, Sub(sender, e)
                                        If isExpanded Then
                                            ' Réduire le Panel
                                            pnlDataGridView.Height = reducedHeight
                                            btnToggle.Text = "Étendre"
                                        Else
                                            ' Étendre le Panel
                                            pnlDataGridView.Height = expandedHeight
                                            btnToggle.Text = "Réduire"
                                        End If
                                        isExpanded = Not isExpanded
                                    End Sub
        ' Déclencher l'événement "réduire" après l'ajout du handler
        If isExpanded Then
            btnToggle.PerformClick()
        End If
    End Sub
    Private Sub txtRemise_TextChanged(sender As Object, e As EventArgs) Handles txtRemise.TextChanged
        'btnListeChqRemise.Visible = Trim(txtRemise.Text) <> String.Empty
        btnNouveauChq.Visible = True
        'Si on n'est pas sur une remise de chèque, on quitte la Frm sur validation
        If txtRemise.Text = String.Empty Then
            btnNouveauChq.Visible = False
        End If
    End Sub
    Private Sub chercheType(dicoTypeMvt As Dictionary(Of String, String))
        ' Vérifier si txtNote.Text est vide ou null
        If Not String.IsNullOrEmpty(txtNote.Text) Then

            ' Parcourir le dictionnaire dicoTypeMvt pour trouver la clé correspondante
            For Each kvp As KeyValuePair(Of String, String) In dicoTypeMvt
                If kvp.Key.Equals(Trim(txtNote.Text), StringComparison.OrdinalIgnoreCase) Then
                    ' Retourner la valeur correspondante si la clé est trouvée
                    UtilitairesDgv.SelectionnerLigneDgvType(dgvType, kvp.Value)
                End If
            Next
        End If
    End Sub
    Private Sub btnNouveauChq_Click(sender As Object, e As EventArgs) Handles btnNouveauChq.Click

    End Sub
End Class