Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports PdfiumViewer

Public Class FrmSelectionneDocument
    Private _idDocSel As Integer
    Private viewer As DocumentViewerManager
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
            MessageBox.Show("Veuillez sélectionner un document dans la liste.", "Avertissement",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            ' Récupère le chemin du fichier à partir de la 2e colonne (index 1)
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


    Private Sub OnDocumentLoaded()
        MessageBox.Show("Document affiché avec succès !")
    End Sub

    Private Sub OnDocumentFailed(ex As Exception)
        MessageBox.Show("Échec de l'affichage : " & ex.Message)
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
            lstDocuments.Items.Add(item)
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
        Dim tabDocuments As New List(Of DocumentAgumaaa)()

        ' Convertir txtMontant.Text en Decimal 
        'If Decimal.TryParse(FrmSaisie.txtMontant.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, montant) Then
        ' Utilisation de Using pour garantir la fermeture des objets
        Try
            Using readerDocuments As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqDoc",
                             New Dictionary(Of String, Object) From {{"@numero", numero},
                             {"@montant", montant.ToString("0.00").Replace("."c, ","c)},
                             {"@emetteur", emetteur}}
                             ).ExecuteReader()
                While readerDocuments.Read()
                    Try
                        'idDoc, dateDoc, contenuDoc, cheminDoc, categorieDoc, sousCategorieDoc, idMvtDoc, metaDonnees, dateModif
                        Dim docSel As New DocumentAgumaaaImpl(
                                            If(IsDBNull(readerDocuments.GetValue(0)), 0, readerDocuments.GetInt32(0)),
                                            If(IsDBNull(readerDocuments.GetValue(1)), Date.MinValue, readerDocuments.GetDateTime(1)),
                                            If(IsDBNull(readerDocuments.GetValue(2)), String.Empty, readerDocuments.GetString(2)),
                                            If(IsDBNull(readerDocuments.GetValue(3)), String.Empty, readerDocuments.GetString(3)),
                                            If(IsDBNull(readerDocuments.GetValue(4)), String.Empty, readerDocuments.GetString(4)),
                                            If(IsDBNull(readerDocuments.GetValue(5)), String.Empty, readerDocuments.GetString(5)),
                                            If(IsDBNull(readerDocuments.GetValue(6)), 0, readerDocuments.GetInt32(6)),
                                            If(IsDBNull(readerDocuments.GetValue(7)), String.Empty, readerDocuments.GetString(7)),
                                            If(IsDBNull(readerDocuments.GetValue(8)), Date.MinValue, readerDocuments.GetDateTime(8))
                                        )

                        tabDocuments.Add(docSel)
                    Catch ex As Exception
                        MessageBox.Show("Erreur lors de la lecture des données : " & ex.Message)
                    End Try
                End While
            End Using
        Catch ex As Exception
            MessageBox.Show("Erreur lors de l'exécution de la commande SQL : " & ex.Message)
        End Try
        'End Using
        'Else
        '    MessageBox.Show($"Valeur de montant invalide : {montant}")
        'End If
        Call alimListeDoc(tabDocuments)
    End Sub
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
            lstDocuments.Items.Add(item)
        Next

        ' Gérer l'événement de changement de sélection
        AddHandler lstDocuments.SelectedIndexChanged, AddressOf LstDocuments_SelectedIndexChanged
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
End Class