Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports PdfiumViewer
Imports System.Windows.Forms

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
    Private Sub btnAfficheDoc_Click(sender As Object, e As EventArgs) Handles btnAfficheDoc.Click
        Dim doc As DocumentAgumaaa
        Dim sContenu As String = doc.ContenuDoc
        viewer.AfficherDocumentBase64(sContenu)
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
                'TODO : vrai uniquement pour un chèque
                .Add(Utilitaires.ExtractStringFromJson(doc.metaDonnees, "montant_numerique"))
                .Add(Utilitaires.ExtractStringFromJson(doc.metaDonnees, "dateChq"))
                .Add(Utilitaires.ExtractStringFromJson(doc.metaDonnees, "destinataire"))
                .Add(doc.ContenuDoc)
            End With
            lstDocuments.Items.Add(item)
        Next
    End Sub
    Private Sub LstDocuments_SelectedIndexChanged(sender As Object, e As EventArgs)
        If lstDocuments.SelectedItems.Count > 0 Then
            Dim selectedItem As ListViewItem = lstDocuments.SelectedItems(0)
            Dim idDoc As Integer = Integer.Parse(selectedItem.Text)

            Cheque.AfficherImage(idDoc, pbDocument)
            _idDocSel = idDoc
        End If
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
        If Decimal.TryParse(FrmSaisie.txtMontant.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, montant) Then
            ' Utilisation de Using pour garantir la fermeture des objets
            Try
                Using readerDocuments As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqDoc",
                             New Dictionary(Of String, Object) From {{"@numero", numero},
                             {"@montant", montant},
                             {"@emetteur", emetteur}}
                             ).ExecuteReader()
                    While readerDocuments.Read()
                        Try
                            Dim chqSel As New Cheque(readerDocuments.GetInt32(0),
                                                     CStr(montant), readerDocuments.GetInt32(1),
                                                     readerDocuments.GetDateTime(2),
                                                     readerDocuments.GetString(3),
                                                     readerDocuments.GetString(4))
                            tabDocuments.Add(chqSel)
                        Catch ex As Exception
                            MessageBox.Show("Erreur lors de la lecture des données : " & ex.Message)
                        End Try
                    End While
                End Using
            Catch ex As Exception
                MessageBox.Show("Erreur lors de l'exécution de la commande SQL : " & ex.Message)
            End Try
            'End Using
        Else
            MessageBox.Show($"Valeur de montant invalide : {montant}")
        End If
        Call alimListeDoc(tabDocuments)
    End Sub
    Public Sub alimListeDoc(tabDocuments As List(Of DocumentAgumaaa))
        ' Configurer le ListView
        With lstDocuments
            .View = View.Details
            .Columns.Add("ID", 50, HorizontalAlignment.Left)
            .Columns.Add("Montant", 100, HorizontalAlignment.Left)
            .Columns.Add("Date", 100, HorizontalAlignment.Left)
            .Columns.Add("Destinataire", 150, HorizontalAlignment.Left)
            .FullRowSelect = True
        End With

        ' Ajouter chaque document au ListView
        For Each document As DocumentAgumaaa In tabDocuments
            ' Créer une nouvelle ligne pour le ListView
            Dim item As New ListViewItem(Utilitaires.ExtractStringFromJson(document.metaDonnees, "id"))
            item.SubItems.Add(Utilitaires.ExtractStringFromJson(document.metaDonnees, "montant_numerique"))
            item.SubItems.Add(Utilitaires.ExtractStringFromJson(document.metaDonnees, "DateDoc"))
            item.SubItems.Add(Utilitaires.ExtractStringFromJson(document.metaDonnees, "destinataire"))
            ' Ajouter la ligne au ListView
            lstDocuments.Items.Add(item)
        Next

        ' Gérer l'événement de changement de sélection
        AddHandler lstDocuments.SelectedIndexChanged, AddressOf LstDocuments_SelectedIndexChanged
    End Sub


End Class