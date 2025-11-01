Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class FrmSelectionneDocument
    Private _idDocSel As Integer
    Private viewer As DocumentViewerManager
    ' --- Déclaration ici, accessible à toutes les procédures ---
    Private anciennesValeurs As New Dictionary(Of String, String)
    Public Property IdDocSelectionne As Integer
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
    End Sub
    Private Sub btnSelDoc_Click(sender As Object, e As EventArgs) Handles btnSelDoc.Click
        ' Vérifier qu'un élément est bien sélectionné
        If lstDocuments.SelectedItems.Count = 0 Then
            Dim unused2 = MessageBox.Show("Veuillez sélectionner un document dans la liste.", "Avertissement",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            ' Récupère le chemin du fichier à partir de la 2e colonne (index 1)
            Dim cheminFichier As String = lstDocuments.SelectedItems(0).SubItems(2).Text

            If Not File.Exists(cheminFichier) Then
                Dim unused1 = MessageBox.Show("Le fichier indiqué est introuvable :" & Environment.NewLine & cheminFichier,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Lire le contenu du fichier en base64
            Dim bytes() As Byte = File.ReadAllBytes(cheminFichier)
            Dim base64Data As String = Convert.ToBase64String(bytes)

            ' Afficher le document dans le viewer
            viewer.AfficherDocumentBase64(base64Data)

        Catch ex As Exception
            Dim unused = MessageBox.Show("Erreur lors du chargement du document : " & ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
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
                Dim unused4 = .Add(doc.CategorieDoc)
                'Eventuellement limiter l'affichage
                Dim unused3 = .Add(doc.CheminDoc)
                Dim unused2 = .Add(doc.DateDoc.ToString("yyyy-MM-dd"))
                Dim unused1 = .Add(doc.SousCategorieDoc)
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
        Return 123 ' Exemple d'idDoc
    End Function


    Public Sub chargeListeDoc(numero As Decimal, montant As Decimal, emetteur As String)
        ' Appelle la version générique avec la requête "reqDoc"
        chargeListeDocInterne("reqDoc", New Dictionary(Of String, Object) From {
        {"@numero", numero},
        {"@montant", montant.ToString("0.00", CultureInfo.InvariantCulture).Replace("."c, ","c)},
        {"@emetteur", emetteur}
    })
    End Sub

    Public Sub chargeListeDoc(montant As Decimal)
        ' Appelle la version générique avec la requête "reqDocMontant"
        chargeListeDocInterne("reqDocMontant", New Dictionary(Of String, Object) From {
        {"@montant", montant.ToString("0.00", CultureInfo.InvariantCulture).Replace("."c, ","c)}
    })
    End Sub

    ' 🔧 Méthode factorisée interne
    Private Sub chargeListeDocInterne(nomRequete As String, parametres As Dictionary(Of String, Object))
        Dim tabDocuments As New List(Of DocumentAgumaaa)()

        Try
            Using readerDocuments As SqlDataReader =
            SqlCommandBuilder.CreateSqlCommand(nomRequete, parametres).ExecuteReader()

                While readerDocuments.Read()
                    Try
                        ' Création d’un DocumentAgumaaaImpl à partir du DataReader
                        Dim docSel As New DocumentAgumaaaImpl(
                        Utilitaires.SafeGetInt(readerDocuments, 0),
                        Utilitaires.SafeGetDate(readerDocuments, 1),
                        Utilitaires.SafeGetString(readerDocuments, 2),
                        Utilitaires.SafeGetString(readerDocuments, 3),
                        Utilitaires.SafeGetString(readerDocuments, 4),
                        Utilitaires.SafeGetString(readerDocuments, 5),
                        Utilitaires.SafeGetInt(readerDocuments, 6),
                        Utilitaires.SafeGetString(readerDocuments, 7),
                        Utilitaires.SafeGetDate(readerDocuments, 8)
                    )

                        tabDocuments.Add(docSel)

                    Catch ex As Exception
                        Logger.ERR($"Erreur lecture document (ligne {tabDocuments.Count}): {ex.Message}")
                    End Try
                End While
            End Using

        Catch ex As Exception
            Dim unused = MessageBox.Show("Erreur SQL : " & ex.Message, "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        ' Alimente la liste
        alimListeDoc(tabDocuments)
    End Sub

    Public Sub alimListeDoc(tabDocuments As List(Of DocumentAgumaaa))
        ' Configurer le ListView
        With lstDocuments
            .View = View.Details
            Dim unused15 = .Columns.Add("IdDoc", 50, HorizontalAlignment.Left)
            Dim unused14 = .Columns.Add("dateDoc", 100, HorizontalAlignment.Left)
            '.Columns.Add("contenuDoc", 100, HorizontalAlignment.Left)
            Dim unused13 = .Columns.Add("cheminDoc", 150, HorizontalAlignment.Left)
            Dim unused12 = .Columns.Add("categorieDoc", 150, HorizontalAlignment.Left)
            Dim unused11 = .Columns.Add("sousCategorieDoc", 150, HorizontalAlignment.Left)
            Dim unused10 = .Columns.Add("idMvtDoc", 150, HorizontalAlignment.Left)
            Dim unused9 = .Columns.Add("metaDonnees", 150, HorizontalAlignment.Left)
            Dim unused8 = .Columns.Add("dateModif", 150, HorizontalAlignment.Left)
            .FullRowSelect = True
        End With

        ' Ajouter chaque document au ListView
        For Each document As DocumentAgumaaa In tabDocuments
            ' Créer une nouvelle ligne pour le ListView
            Dim item As New ListViewItem(document.IdDoc)
            Dim unused7 = item.SubItems.Add(document.DateDoc)
            'item.SubItems.Add(document.ContenuDoc)
            Dim unused6 = item.SubItems.Add(document.CheminDoc)
            Dim unused5 = item.SubItems.Add(document.CategorieDoc)
            Dim unused4 = item.SubItems.Add(document.SousCategorieDoc)
            Dim unused3 = item.SubItems.Add(document.IdMvtDoc)
            Dim unused2 = item.SubItems.Add(document.metaDonnees)
            Dim unused1 = item.SubItems.Add(document.dateModif)
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

            lbl.Location = New Point(0, 3)
            txt.Location = New Point(lbl.Right + 8, 0)

            flpMetaDonnees.Controls.Add(panelLigne)
        Next
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
            Dim cmd As SqlCommand =
                SqlCommandBuilder.CreateSqlCommand("updateDocumentMetaDonnees",
                                                    New Dictionary(Of String, Object) From {
                                                        {"@idDocument", currentDocId},
                                                        {"@metaDonnees", nouveauJson}
                                                    }
                                                )
            ' Exécuter la requête et obtenir le nombre de lignes affectées
            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
            Logger.INFO($"Nombre de lignes mises à jour : {rowsAffected}")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la mise à jour de la base : {ex.Message}")
        End Try
    End Sub
    Private Sub renommeFichier(sender As Object, e As EventArgs)

        Dim txt As TextBox = DirectCast(sender, TextBox)
        Dim nomChamp As String = txt.Tag.ToString()
        Dim nouvelleValeur As String = txt.Text.Trim()

        Dim ancienneValeur As String = ""
        If anciennesValeurs.ContainsKey(nomChamp) Then
            ancienneValeur = anciennesValeurs(nomChamp)
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
            Dim dossier As String = Path.GetDirectoryName(cheminActuel)
            Dim nomFichier As String = Path.GetFileName(cheminActuel)

            If nomFichier.Contains(ancienneValeur) Then
                Dim nouveauNomFichier As String = nomFichier.Replace(ancienneValeur, nouvelleValeur)
                Dim nouveauChemin As String = Path.Combine(dossier, nouveauNomFichier)

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
                Dim unused2 = MessageBox.Show("Veuillez sélectionner un document dans la liste.",
                                "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            ' 🔹 Récupère l’idDoc dans la 1ʳᵉ colonne (SubItem(0))
            Dim idText As String = lstDocuments.SelectedItems(0).SubItems(0).Text

            ' Conversion sécurisée en entier
            Dim id As Integer
            If Not Integer.TryParse(idText, id) Then
                Dim unused1 = MessageBox.Show("Identifiant de document invalide.",
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
End Class