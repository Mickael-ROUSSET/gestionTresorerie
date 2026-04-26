Imports System.Data.SqlClient
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class FrmSelectionneDocument
    Private _idDocSel As Integer
    Private viewer As DocumentViewerManager
    ' --- Déclaration ici, accessible à toutes les procédures ---
    Private anciennesValeurs As New Dictionary(Of String, String)
    Public Property IdDocSelectionne As Integer
    Private tooltipDoc As New ToolTip()


    ' --- Variables de pagination ---
    Private pageCourante As Integer = 1
    Private taillePage As Integer = 5
    Private totalDocuments As Integer = 0
    Private filtreRecherche As String = ""

    ' --- Variables de tri ---
    Private colonneTri As String = "dateDoc"
    Private triAscendant As Boolean = False
    Public Property idDocSel() As Integer
        Get
            Return _idDocSel
        End Get
        Set(ByVal value As Integer)
            _idDocSel = value
        End Set
    End Property

    Public Sub New()
        ' Initialisez les composants
        InitializeComponent()
    End Sub

    Private Sub FrmSelectionneDocument_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        viewer = New DocumentViewerManager(Me)
        AddHandler viewer.DocumentLoaded, AddressOf OnDocumentLoaded
        AddHandler viewer.DocumentFailed, AddressOf OnDocumentFailed
        AddHandler lstDocuments.MouseMove, AddressOf lstDocuments_MouseMove
        AddHandler lstDocuments.SelectedIndexChanged, AddressOf lstDocuments_SelectedIndexChanged

        ' ============================================
        ' ========== CHARGEMENT INITIAL ==============
        ' ============================================
        InitialiserListView()
        ChargerDocuments()
    End Sub
    Private lastTooltipItem As ListViewItem = Nothing
    Private lastTooltipSubItemIndex As Integer = -1
    Private Shared Function CreateDocumentRepository() As DocumentRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).
                    GetConnexion(Constantes.DataBases.Agumaaa).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New DocumentRepository(executor, factory, provider)
    End Function
    Private Sub lstDocuments_MouseMove(sender As Object, e As MouseEventArgs)
        Dim info As ListViewHitTestInfo = lstDocuments.HitTest(e.Location)

        If info.Item Is Nothing OrElse info.SubItem Is Nothing Then
            tooltipDoc.Hide(lstDocuments)
            lastTooltipItem = Nothing
            lastTooltipSubItemIndex = -1
            Return
        End If

        Dim subItemText As String = info.SubItem.Text

        ' Calcule la largeur du texte affiché
        Using g As Graphics = lstDocuments.CreateGraphics()
            Dim textSize As SizeF = g.MeasureString(subItemText, lstDocuments.Font)

            ' Largeur visible de la cellule
            Dim colWidth As Integer = lstDocuments.Columns(info.Item.SubItems.IndexOf(info.SubItem)).Width

            ' Si le texte dépasse la largeur, on montre une info-bulle
            If textSize.Width > colWidth Then
                ' Évite de redéclencher la bulle inutilement
                If info.Item IsNot lastTooltipItem OrElse info.Item.SubItems.IndexOf(info.SubItem) <> lastTooltipSubItemIndex Then
                    tooltipDoc.Show(subItemText, lstDocuments, e.Location.X + 15, e.Location.Y + 15, 4000)
                    lastTooltipItem = info.Item
                    lastTooltipSubItemIndex = info.Item.SubItems.IndexOf(info.SubItem)
                End If
            Else
                tooltipDoc.Hide(lstDocuments)
                lastTooltipItem = Nothing
                lastTooltipSubItemIndex = -1
            End If
        End Using
    End Sub

    Private Sub btnSelDoc_Click(sender As Object, e As EventArgs) Handles btnSelDoc.Click
        SelectionnerDocument()
    End Sub


    Private Sub OnDocumentLoaded()
        Logger.INFO("Document affiché avec succès !")
    End Sub

    Private Sub OnDocumentFailed(ex As Exception)
        Logger.ERR("Échec de l'affichage : " & ex.Message)
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        viewer.Dispose()
        MyBase.OnFormClosing(e)
    End Sub
    Public Sub alimlstDocuments(docs() As DocumentAgumaaa)
        For Each doc As DocumentAgumaaa In docs
            Dim item As New ListViewItem(doc.IdMvtDoc)
            With item.SubItems
                .Add(doc.CategorieDoc)
                'Eventuellement limiter l'affichage
                .Add(doc.CheminDoc)
                .Add(doc.DateDoc.ToString("yyyy-MM-dd"))
                .Add(doc.SousCategorieDoc)
                '.Add(doc.ContenuDoc)
            End With
            Dim unused = lstDocuments.Items.Add(item)
        Next
    End Sub

    Public Event IdDocSelectionneChanged(ByVal idDoc As Integer)
    Private Sub btnSelCheque_Click(sender As Object, e As EventArgs)
        ' Supposons que l'idDoc est sélectionné ici
        Dim idDoc As Integer = ObtenirIdDocSelectionne()

        ' Déclencher l'événement
        RaiseEvent IdDocSelectionneChanged(idDoc)

        ' Fermer la fenêtre appelée
        Close()
    End Sub

    Private Function ObtenirIdDocSelectionne() As Integer
        ' Logique pour obtenir l'idDoc sélectionné
        MsgBox("Fonction fictive ObtenirIdDocSelectionne appelée")
        Return 123 ' Exemple d'idDoc
    End Function

    Public Sub chargeListeDoc()
        filtreRecherche = ""
        pageCourante = 1
        ChargerDocuments()
    End Sub

    Public Sub chargeListeDoc(montant As Decimal)
        pageCourante = 1
        ChargerDocumentsParMontant(montant)
    End Sub

    Public Sub chargeListeDoc(numero As Decimal, montant As Decimal, emetteur As String)
        pageCourante = 1
        ChargerDocumentsCheque(numero, montant, emetteur)
    End Sub
    Private Sub ChargerDocumentsParMontant(montant As Decimal)
        Try
            Dim dt As DataTable = CreateDocumentRepository().LireParMontant(montant)
            ChargerListViewDepuisDataTable(dt)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des documents par montant : {ex.Message}")
            MessageBox.Show("Erreur lors du chargement des documents par montant : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ChargerDocumentsCheque(numero As Decimal, montant As Decimal, emetteur As String)
        Try
            Dim dt As DataTable = CreateDocumentRepository().LireCheque(numero, montant, emetteur)
            ChargerListViewDepuisDataTable(dt)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des documents chèque : {ex.Message}")
            MessageBox.Show("Erreur lors du chargement des documents chèque : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ChargerListViewDepuisDataTable(dt As DataTable)
        lstDocuments.Items.Clear()

        If dt Is Nothing Then
            MajBoutonsPagination()
            Exit Sub
        End If

        For Each row As DataRow In dt.Rows
            AjouterDocumentDansListe(row)
        Next

        MajBoutonsPagination()
    End Sub

    Private Sub AjouterDocumentDansListe(row As DataRow)
        Dim it As New ListViewItem(GetRowString(row, "idDoc"))

        it.SubItems.Add(GetRowDateString(row, "dateDoc"))
        it.SubItems.Add(GetRowString(row, "cheminDoc"))
        it.SubItems.Add(GetRowString(row, "categorieDoc"))
        it.SubItems.Add(GetRowString(row, "sousCategorieDoc"))
        it.SubItems.Add(GetRowString(row, "idMvtDoc"))
        it.SubItems.Add(GetRowString(row, "metaDonnees"))
        it.SubItems.Add(GetRowDateString(row, "dateModif"))

        lstDocuments.Items.Add(it)
    End Sub

    Private Shared Function GetRowString(row As DataRow, columnName As String) As String
        If row Is Nothing OrElse row.Table Is Nothing OrElse Not row.Table.Columns.Contains(columnName) Then
            Return String.Empty
        End If

        If row(columnName) Is DBNull.Value Then
            Return String.Empty
        End If

        Return row(columnName).ToString()
    End Function

    Private Shared Function GetRowDateString(row As DataRow, columnName As String) As String
        If row Is Nothing OrElse row.Table Is Nothing OrElse Not row.Table.Columns.Contains(columnName) Then
            Return String.Empty
        End If

        If row(columnName) Is DBNull.Value Then
            Return String.Empty
        End If

        Return Convert.ToDateTime(row(columnName)).ToString("dd/MM/yyyy HH:mm")
    End Function
    Public Sub alimListeDoc(tabDocuments As List(Of DocumentAgumaaa))
        ' Configurer le ListView
        With lstDocuments
            .View = View.Details
            .Columns.Add("IdDoc", 50, HorizontalAlignment.Left)
            .Columns.Add("dateDoc", 100, HorizontalAlignment.Left)
            '.Columns.Add("contenuDoc", 100, HorizontalAlignment.Left)
            .Columns.Add("cheminDoc", 150, HorizontalAlignment.Left)
            .Columns.Add("categorieDoc", 150, HorizontalAlignment.Left)
            .Columns.Add("sousCategorieDoc", 150, HorizontalAlignment.Left)
            .Columns.Add("idMvtDoc", 150, HorizontalAlignment.Left)
            .Columns.Add("metaDonnees", 150, HorizontalAlignment.Left)
            .Columns.Add("dateModif", 150, HorizontalAlignment.Left)
            .FullRowSelect = True
        End With

        ' Ajouter chaque document au ListView
        For Each document As DocumentAgumaaa In tabDocuments
            ' Créer une nouvelle ligne pour le ListView
            Dim item As New ListViewItem(document.IdDoc)
            item.SubItems.Add(document.DateDoc)
            'item.SubItems.Add(document.ContenuDoc)
            item.SubItems.Add(document.CheminDoc)
            item.SubItems.Add(document.CategorieDoc)
            item.SubItems.Add(document.SousCategorieDoc)
            item.SubItems.Add(document.IdMvtDoc)
            item.SubItems.Add(document.metaDonnees)
            item.SubItems.Add(document.dateModif)
            ' Ajouter la ligne au ListView
            Dim unused = lstDocuments.Items.Add(item)
        Next

        ' Gérer l'événement de changement de sélection
        AddHandler lstDocuments.SelectedIndexChanged, AddressOf lstDocuments_SelectedIndexChanged
    End Sub

    ' ⚙️ Panneau où seront créés les champs dynamiques
    ' (assure-toi que flpMetaDonnees existe sur le formulaire)
    ' FlowLayoutPanel : Dock = Fill, FlowDirection = TopDown, AutoScroll = True

    ' ID du document actuellement sélectionné
    Private currentDocId As Integer?

    ' 🔹 Lorsqu’un document est sélectionné dans la liste
    Private Sub lstDocuments_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDocuments.SelectedIndexChanged
        If lstDocuments.SelectedItems.Count = 0 Then
            flpMetaDonnees.Controls.Clear()
            currentDocId = Nothing
            Return
        End If

        Dim item = lstDocuments.SelectedItems(0)
        currentDocId = CInt(item.SubItems(0).Text) ' suppose que la 1ʳᵉ colonne contient l’ID du document

        Dim metaDonneesJson As String = item.SubItems(6).Text ' adapte l’index si besoin
        If String.IsNullOrWhiteSpace(metaDonneesJson) Then
            flpMetaDonnees.Controls.Clear()
            Return
        End If

        ' Affiche l'image : appelle directement le bouton "Sélectionner" (même logique qu’un clic)
        SelectionnerDocument()

        Try
            Dim jsonObj As JObject = JObject.Parse(metaDonneesJson)
            AfficherChampsMetaDonnees(jsonObj)
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des métadonnées : {ex.Message}")
        End Try
    End Sub

    ' 🔹 Affiche les champs JSON sous forme Label + TextBox

    Private Sub AfficherChampsMetaDonnees(jsonObj As JObject)
        flpMetaDonnees.Controls.Clear()

        ' --- 1️⃣ Champs des métadonnées JSON ---
        For Each prop As JProperty In jsonObj.Properties
            Dim panelLigne As New Panel With {
            .AutoSize = True,
            .Height = 28,
            .Dock = DockStyle.Top
        }

            Dim lbl As New Label With {
            .Text = prop.Name & " :",
            .Width = 150,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Anchor = AnchorStyles.Left,
            .AutoSize = False
        }

            Dim txt As New TextBox With {
            .Text = prop.Value.ToString(),
            .Width = 250,
            .Tag = prop.Name,
            .Anchor = AnchorStyles.Left
        }

            AddHandler txt.Validated, AddressOf MetaDonnee_Validated

            panelLigne.Controls.Add(lbl)
            panelLigne.Controls.Add(txt)

            lbl.Location = New System.Drawing.Point(0, 3)
            txt.Location = New System.Drawing.Point(lbl.Right + 8, 0)

            flpMetaDonnees.Controls.Add(panelLigne)
        Next

        ' --- 2️⃣ Ligne spéciale : chemin du document et bouton (version visible garantie) ---
        ' --- Version robuste : s'assure que le bouton est complètement visible ---
        If lstDocuments.SelectedItems.Count > 0 Then

            Dim panelChemin As New Panel With {
        .Width = flpMetaDonnees.ClientSize.Width - 25,
        .AutoSize = False,
        .BorderStyle = BorderStyle.None,
        .Margin = New Padding(4),
        .BackColor = Color.Transparent
    }

            ' Label
            Dim lblChemin As New Label With {
        .Text = "Emplacement du fichier :",
        .TextAlign = ContentAlignment.MiddleLeft,
        .Location = New System.Drawing.Point(0, 5),
        .Width = 180,
        .AutoSize = False
    }

            ' TextBox (aligne à droite du label)
            Dim txtChemin As New TextBox With {
                .Location = New System.Drawing.Point(lblChemin.Right + 6, 5),
                .Width = panelChemin.Width - lblChemin.Width - 20,
                .ReadOnly = True,
                .Text = lstDocuments.SelectedItems(0).SubItems(2).Text
            }

            ' Bouton
            Dim btnChanger As New Button With {
        .Text = "Changer / Renommer...",
        .AutoSize = False
    }
            ' Définir explicitement la hauteur (ou utiliser PreferredSize)
            Dim preferred As Size = btnChanger.GetPreferredSize(New Size(0, 0))
            btnChanger.Height = Math.Max(preferred.Height, 28) ' 28 ou 30 est souvent une bonne valeur
            btnChanger.Width = Math.Min(preferred.Width + 20, panelChemin.Width - lblChemin.Width - 20)

            ' Position du bouton : sous le textbox (avec marge)
            btnChanger.Location = New System.Drawing.Point(txtChemin.Left, txtChemin.Bottom + 8)

            ' Evenement clic
            AddHandler btnChanger.Click,
        Sub(sender As Object, e As EventArgs)
            ChangerEmplacementOuNomFichier(txtChemin)
        End Sub

            ' Ajouter les contrôles
            panelChemin.Controls.Add(lblChemin)
            panelChemin.Controls.Add(txtChemin)
            panelChemin.Controls.Add(btnChanger)

            ' Ajuster la hauteur du panel pour contenir le bouton complètement
            panelChemin.Height = btnChanger.Bottom + 8 ' marge inférieure

            ' Ajouter au FlowLayoutPanel
            flpMetaDonnees.Controls.Add(panelChemin)

            ' Forcer recalcul layout
            panelChemin.PerformLayout()
            flpMetaDonnees.PerformLayout()
        End If

    End Sub

    Private Sub ChangerEmplacementOuNomFichier(txtChemin As TextBox)
        Try
            Dim ancienChemin As String = txtChemin.Text
            If String.IsNullOrWhiteSpace(ancienChemin) OrElse Not File.Exists(ancienChemin) Then
                MessageBox.Show("Le fichier source est introuvable.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            ' 🗂️ Ouvre un SaveFileDialog pour choisir emplacement et nom
            Using dlg As New SaveFileDialog
                dlg.Title = "Choisir le nouvel emplacement et nom du fichier"
                dlg.InitialDirectory = IO.Path.GetDirectoryName(ancienChemin)
                dlg.FileName = IO.Path.GetFileName(ancienChemin)
                dlg.Filter = "Tous les fichiers (*.*)|*.*"

                If dlg.ShowDialog() = DialogResult.OK Then
                    Dim nouveauChemin = dlg.FileName

                    ' Si c’est le même → rien à faire
                    If String.Equals(ancienChemin, nouveauChemin, StringComparison.OrdinalIgnoreCase) Then Exit Sub

                    ' Vérification : fichier existant
                    If File.Exists(nouveauChemin) Then
                        Dim rep = MessageBox.Show(
                        "Un fichier du même nom existe déjà à cet emplacement." & vbCrLf &
                        "Souhaitez-vous le remplacer ?",
                        "Fichier existant",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question)
                        If rep = DialogResult.No Then Exit Sub
                    End If

                    ' 🔁 Déplacement (avec remplacement possible)
                    File.Move(ancienChemin, nouveauChemin, True)

                    ' 🧭 Mise à jour du champ
                    txtChemin.Text = nouveauChemin

                    ' 🗄️ MAJ de la base
                    Dim idDoc As Integer = CInt(lstDocuments.SelectedItems(0).Text)
                    Call majCheminDoc(nouveauChemin, idDoc)

                    ' 🪶 MAJ dans la liste
                    lstDocuments.SelectedItems(0).SubItems(2).Text = nouveauChemin

                    MessageBox.Show("✅ Fichier déplacé/renommé et base mise à jour.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("❌ Erreur lors du déplacement ou renommage : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    'Mise à jour du chemin du document
    Private Sub majCheminDoc(nouveauChemin As String, idDoc As Integer)
        CreateDocumentRepository().MettreAJourChemin(idDoc, nouveauChemin)
        Logger.INFO($"Chemin du document {idDoc} mis à jour.")
    End Sub

    ' 🔹 Lorsqu’une zone de saisie perd le focus → mise à jour du JSON et de la base
    Private Sub MetaDonnee_Validated(sender As Object, e As EventArgs)
        If currentDocId Is Nothing OrElse lstDocuments.SelectedItems.Count = 0 Then Return

        Dim txt As TextBox = DirectCast(sender, TextBox)
        Dim champNom As String = txt.Tag.ToString()
        Dim nouvelleValeur As String = txt.Text

        Try
            ' Charger le JSON actuel
            Dim item = lstDocuments.SelectedItems(0)
            Dim metaDonneesJson As String = item.SubItems(6).Text
            Dim jsonObj As JObject = JObject.Parse(metaDonneesJson)

            ' Mettre à jour le champ modifié
            jsonObj(champNom) = nouvelleValeur

            ' Convertir en chaîne compacte
            Dim nouveauJson As String = jsonObj.ToString(Formatting.None)

            ' Mettre à jour le ListView
            item.SubItems(6).Text = nouveauJson

            'Renomme le fichier si besoin
            renommeFichier(sender, e)

            ' 🧠 Mise à jour de la base via CreateSqlCommand
            CreateDocumentRepository().MettreAJourMetaDonnees(currentDocId.Value, jsonObj.ToString(Formatting.None))

        Catch ex As Exception
            Logger.ERR($"Erreur lors de la mise à jour de la base : {ex.Message}")
        End Try
    End Sub
    Private Sub renommeFichier(sender As Object, e As EventArgs)

        Dim txt As TextBox = DirectCast(sender, TextBox)
        Dim nomChamp As String = txt.Tag.ToString()
        Dim nouvelleValeur As String = txt.Text.Trim()

        Dim ancienneValeur As String

        Dim value As String = Nothing

        If anciennesValeurs.TryGetValue(nomChamp, value) Then
            ancienneValeur = value
        Else
            anciennesValeurs(nomChamp) = nouvelleValeur
            Return
        End If

        If String.IsNullOrEmpty(ancienneValeur) OrElse ancienneValeur = nouvelleValeur Then
            anciennesValeurs(nomChamp) = nouvelleValeur
            Return
        End If

        ' On prend la ligne sélectionnée dans lstDocuments
        If lstDocuments.SelectedItems.Count = 0 Then Exit Sub

        Dim item As ListViewItem = lstDocuments.SelectedItems(0)
        Dim cheminActuel As String = item.SubItems(2).Text ' ← 3ᵉ colonne (index 2)

        If String.IsNullOrEmpty(cheminActuel) Then
            anciennesValeurs(nomChamp) = nouvelleValeur
            Return
        End If

        Try
            Dim dossier As String = IO.Path.GetDirectoryName(cheminActuel)
            Dim nomFichier As String = IO.Path.GetFileName(cheminActuel)

            If nomFichier.Contains(ancienneValeur) Then
                Dim nouveauNomFichier As String = nomFichier.Replace(ancienneValeur, nouvelleValeur)
                Dim nouveauChemin As String = IO.Path.Combine(dossier, nouveauNomFichier)

                ' Vérifie que le fichier source existe avant renommage
                If File.Exists(cheminActuel) Then
                    ' Vérifie qu'on n’écrase pas un fichier existant
                    If Not File.Exists(nouveauChemin) Then
                        File.Move(cheminActuel, nouveauChemin)
                        Logger.INFO($"Fichier '{cheminActuel}' renommé en '{nouveauChemin}' car correction utilisateur")
                    Else
                        MessageBox.Show($"Le fichier '{nouveauNomFichier}' existe déjà.", "Renommage impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                Else
                    MessageBox.Show($"Le fichier '{cheminActuel}' est introuvable.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                ' Met à jour la 3ᵉ colonne avec le nouveau chemin
                item.SubItems(2).Text = nouveauChemin
            End If

            ' Mets à jour la valeur mémorisée
            anciennesValeurs(nomChamp) = nouvelleValeur

        Catch ex As Exception
            MessageBox.Show("Erreur lors du renommage du fichier : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        Try
            ' Vérifie qu’un élément est bien sélectionné
            If lstDocuments.SelectedItems.Count = 0 Then
                MessageBox.Show("Veuillez sélectionner un document dans la liste.",
                                "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            ' 🔹 Récupère l’idDoc dans la 1ʳᵉ colonne (SubItem(0))
            Dim idText As String = lstDocuments.SelectedItems(0).SubItems(0).Text

            ' Conversion sécurisée en entier
            Dim id As Integer
            If Not Integer.TryParse(idText, id) Then
                MessageBox.Show("Identifiant de document invalide.",
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

            ' ✅ Sauvegarde dans la propriété publique
            IdDocSelectionne = id

            ' ✅ Ferme la fenêtre avec un résultat positif
            DialogResult = DialogResult.OK
            Close()

        Catch ex As Exception
            Dim unused = MessageBox.Show("Erreur lors de la validation du document : " & ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub lstDocuments_DoubleClick(sender As Object, e As EventArgs) Handles lstDocuments.DoubleClick
        If lstDocuments.SelectedItems.Count > 0 Then
            btnValider.PerformClick()
        End If
    End Sub

    ' ============================================
    ' ========== CONFIGURATION LISTVIEW ===========
    ' ============================================
    Private Sub InitialiserListView()
        lstDocuments.View = View.Details
        lstDocuments.FullRowSelect = True
        lstDocuments.Columns.Clear()

        lstDocuments.Columns.Add("idDoc", "Id", 250)
        lstDocuments.Columns.Add("dateDoc", "Date", 300)
        lstDocuments.Columns.Add("cheminDoc", "Chemin", 150)
        lstDocuments.Columns.Add("categorieDoc", "Catégorie", 150)
        lstDocuments.Columns.Add("sousCategorieDoc", "sous-catégorie", 150)
        lstDocuments.Columns.Add("idMvtDoc", "Id Mvt", 150)
        lstDocuments.Columns.Add("metaDonnees", "meta-données", 150)
        lstDocuments.Columns.Add("dateModif", "Date modif", 150)
    End Sub


    ' ============================================
    ' ========== CHARGEMENT DES DONNÉES ==========
    ' ============================================
    Private Sub ChargerDocuments()
        Try
            totalDocuments = cptDocumentsPagines()

            Dim offset As Integer = (pageCourante - 1) * taillePage

            Dim dt As DataTable =
            CreateDocumentRepository().LireDocumentsPagines(offset, taillePage)

            ChargerListViewDepuisDataTable(dt)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des documents : {ex.Message}")
            MessageBox.Show("Erreur lors du chargement des documents : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub

    'Private Shared Function cptDocumentsPagines(sWhereClause As String) As Integer
    Private Shared Function cptDocumentsPagines() As Integer
        Return CreateDocumentRepository().CompterDocuments()
    End Function
    ' ============================================
    ' ====== MISE À JOUR DES BOUTONS ============
    ' ============================================
    Private Sub MajBoutonsPagination()
        Dim nbPages As Integer = CInt(Math.Ceiling(totalDocuments / taillePage))
        If nbPages = 0 Then nbPages = 1

        If pageCourante > nbPages Then pageCourante = nbPages
        If pageCourante < 1 Then pageCourante = 1

        lblPage.Text = $"Page {pageCourante} / {nbPages}"
        btnPrecedent.Enabled = (pageCourante > 1)
        btnSuivant.Enabled = (pageCourante < nbPages)
    End Sub


    ' ============================================
    ' ====== BOUTONS DE NAVIGATION ===============
    ' ============================================
    Private Sub btnPrecedent_Click(sender As Object, e As EventArgs) Handles btnPrecedent.Click
        pageCourante -= 1
        ChargerDocuments()
    End Sub

    Private Sub btnSuivant_Click(sender As Object, e As EventArgs) Handles btnSuivant.Click
        pageCourante += 1
        ChargerDocuments()
    End Sub


    ' ============================================
    ' ====== RECHERCHE ============================
    ' ============================================
    Private Sub btnRechercher_Click(sender As Object, e As EventArgs) Handles btnRechercher.Click
        filtreRecherche = txtRecherche.Text.Trim()
        pageCourante = 1
        ChargerDocuments()
    End Sub

    Private Sub btnEffacerFiltre_Click(sender As Object, e As EventArgs) Handles btnEffacerFiltre.Click
        txtRecherche.Clear()
        filtreRecherche = ""
        pageCourante = 1
        ChargerDocuments()
    End Sub

    Private Sub txtRecherche_KeyDown(sender As Object, e As KeyEventArgs) Handles txtRecherche.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            btnRechercher.PerformClick()
        End If
    End Sub


    ' ============================================
    ' ====== TRI PAR COLONNE =====================
    ' ============================================
    Private Sub lstDocuments_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles lstDocuments.ColumnClick
        Dim nouvelleColonne As String

        Select Case e.Column
            Case 0
                nouvelleColonne = "idDoc"
            Case 1
                nouvelleColonne = "dateDoc"
            Case 2
                nouvelleColonne = "cheminDoc"
            Case 3
                nouvelleColonne = "categorieDoc"
            Case 4
                nouvelleColonne = "sousCategorieDoc"
            Case 5
                nouvelleColonne = "idMvtDoc"
            Case 6
                nouvelleColonne = "metaDonnees"
            Case 7
                nouvelleColonne = "dateModif"
            Case Else
                nouvelleColonne = "dateDoc"
        End Select

        If nouvelleColonne = colonneTri Then
            triAscendant = Not triAscendant
        Else
            colonneTri = nouvelleColonne
            triAscendant = True
        End If

        pageCourante = 1
        ChargerDocuments()
    End Sub

    Private Sub SelectionnerDocument()
        ' Vérifier qu'un élément est bien sélectionné
        If lstDocuments.SelectedItems.Count = 0 Then
            MessageBox.Show("Veuillez sélectionner un document dans la liste.", "Avertissement",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            ' Récupère le chemin du fichier à partir de la 2e colonne (index 2)
            Dim cheminFichier As String = lstDocuments.SelectedItems(0).SubItems(2).Text

            If Not File.Exists(cheminFichier) Then
                MessageBox.Show("Le fichier indiqué est introuvable :" & Environment.NewLine & cheminFichier,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Lire le contenu du fichier en base64
            Dim bytes() As Byte = File.ReadAllBytes(cheminFichier)
            Dim base64Data As String = Convert.ToBase64String(bytes)

            ' Afficher le document dans le viewer
            viewer.AfficherDocumentBase64(base64Data)

        Catch ex As Exception
            MessageBox.Show("Erreur lors du chargement du document : " & ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnChercheTiers_Click(sender As Object, e As EventArgs) Handles btnChercheTiers.Click
        Try
            ' --- Ouvre la fenêtre de sélection des tiers ---
            Dim frmTiers As New FrmSelectionGenerique(
                                GetType(Tiers),
                                nomRequete:="reqIdentiteCatTiers",  ' Nom de la requête SQL
                                parametres:=Nothing,                ' Paramètres si nécessaires
                                multiSelect:=False,                 ' Sélection unique
                                lectureSeule:=True                  ' Lecture seule
                                ) With {
                .Text = "Sélection du tiers"
                                }

            ' --- Si un tiers est sélectionné ---
            If frmTiers.ShowDialog() = DialogResult.OK AndAlso
           frmTiers.ResultatsSelectionnes IsNot Nothing AndAlso
           frmTiers.ResultatsSelectionnes.Count > 0 Then

                ' Convertit le premier DataRow en objet Tiers
                Dim dr As DataRow = frmTiers.ResultatsSelectionnes(0)
                Dim tiersSelectionne As Tiers = Tiers.FromDataRow(dr)

                Logger.INFO($"Tiers sélectionné : {tiersSelectionne}")

                ' --- Met à jour automatiquement les champs destinataire ---
                MettreAJourChampsDestinataire(tiersSelectionne)

            Else
                Logger.INFO("Aucun tiers sélectionné.")
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur dans btnChercheTiers_Click : {ex.Message}")
        End Try
    End Sub

    Private Sub MettreAJourChampsDestinataire(tiers As Tiers)
        Try
            For Each ctrl As Control In flpMetaDonnees.Controls
                Dim nomCtrl As String = ctrl.Name.ToLowerInvariant()

                If nomCtrl.Contains("destinataire") Then
                    Dim texteTiers As String

                    ' Si le tiers a un prénom/nom
                    If Not String.IsNullOrEmpty(tiers.Prenom) Then
                        texteTiers = $"{tiers.Prenom} {tiers.Nom}"
                    Else
                        texteTiers = tiers.RaisonSociale
                    End If

                    ' Mise à jour selon le type de contrôle
                    Select Case True
                        Case TypeOf ctrl Is TextBox
                            DirectCast(ctrl, TextBox).Text = texteTiers
                        Case TypeOf ctrl Is ComboBox
                            DirectCast(ctrl, ComboBox).Text = texteTiers
                        Case TypeOf ctrl Is Label
                            DirectCast(ctrl, Label).Text = texteTiers
                        Case Else
                            Logger.INFO($"Contrôle ignoré : {ctrl.Name} (type {ctrl.GetType().Name})")
                    End Select

                    Logger.INFO($"Champ '{ctrl.Name}' mis à jour avec le destinataire : {texteTiers}")
                End If
            Next
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la mise à jour des champs destinataire : {ex.Message}")
        End Try
    End Sub

End Class