Imports System.Data.SqlClient
Imports System.Globalization

Public Class FrmSelectionneCheque
    Private _idChqSel As Integer
    Public Property idChqSel() As Integer
        Get
            Return _idChqSel
        End Get
        Set(ByVal value As Integer)
            _idChqSel = value
        End Set
    End Property

    Public Sub New()
        ' Initialisez les composants
        InitializeComponent()
    End Sub
    Public Sub alimlstCheques(cheques() As Cheque)
        For Each chq As Cheque In cheques
            Dim item As New ListViewItem(chq.id)
            With item.SubItems
                .Add(chq.montant_numerique)
                .Add(chq.dateChq)
                .Add(chq.destinataire)
            End With
            lstCheques.Items.Add(item)
        Next
    End Sub
    Private Sub LstCheques_SelectedIndexChanged(sender As Object, e As EventArgs)
        If lstCheques.SelectedItems.Count > 0 Then
            Dim selectedItem As ListViewItem = lstCheques.SelectedItems(0)
            Dim idChq As Integer = Integer.Parse(selectedItem.Text)
            Dim chq As New Cheque

            chq.AfficherImage(idChq, pbCheque)
            _idChqSel = idChq
        End If
    End Sub

    Public Event IdChequeSelectionneChanged(ByVal idCheque As Integer)

    Private Sub btnSelCheque_Click(sender As Object, e As EventArgs)
        ' Supposons que l'idCheque est sélectionné ici
        Dim idCheque As Integer = ObtenirIdChequeSelectionne()

        ' Déclencher l'événement
        RaiseEvent IdChequeSelectionneChanged(idCheque)

        ' Fermer la fenêtre appelée
        Me.Close()
    End Sub

    Private Function ObtenirIdChequeSelectionne() As Integer
        ' Logique pour obtenir l'idCheque sélectionné
        Return 123 ' Exemple d'idCheque
    End Function

    Public Sub chargeListeChq(montant As Decimal)
        Dim tabCheques As New List(Of Cheque)()

        ' Convertir txtMontant.Text en Decimal 
        If Decimal.TryParse(FrmSaisie.txtMontant.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, montant) Then
            ' Utilisation de Using pour garantir la fermeture des objets
            'Using cmdLstCheques As New SqlCommand("SELECT id, numero, date, emetteur, destinataire FROM Cheque WHERE montant = @montant;", ConnexionDB.GetInstance.getConnexion)
            '    cmdLstCheques.Parameters.AddWithValue("@montant", montant)
            Try
                'Using readerChq As SqlDataReader = cmdLstCheques.ExecuteReader()
                Using readerChq As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqChq",
                             New Dictionary(Of String, Object) From {{"@montant", montant}}
                             ).ExecuteReader()
                    While readerChq.Read()
                        Try
                            ' Construire le JSON et créer un objet Cheque
                            '    Dim chqSel As New Cheque(CreerJsonCheque(
                            '    readerChq.GetInt32(0),
                            '    montant,
                            '    readerChq.GetInt32(1),
                            '    readerChq.GetDateTime(2),
                            '    readerChq.GetString(3),
                            '    readerChq.GetString(4)
                            '))
                            Dim chqSel As New Cheque(readerChq.GetInt32(0), CStr(montant), readerChq.GetInt32(1), readerChq.GetDateTime(2), readerChq.GetString(3), readerChq.GetString(4))
                            tabCheques.Add(chqSel)
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
            MessageBox.Show("Valeur de montant invalide.")
        End If
        Call alimListeChq(tabCheques)
    End Sub
    Public Sub alimListeChq(tabCheques As List(Of Cheque))
        ' Configurer le ListView
        lstCheques.View = View.Details
        lstCheques.Columns.Add("ID", 50, HorizontalAlignment.Left)
        lstCheques.Columns.Add("Montant", 100, HorizontalAlignment.Left)
        lstCheques.Columns.Add("Date", 100, HorizontalAlignment.Left)
        lstCheques.Columns.Add("Destinataire", 150, HorizontalAlignment.Left)
        lstCheques.FullRowSelect = True

        ' Ajouter chaque chèque au ListView
        For Each cheque As Cheque In tabCheques
            ' Créer une nouvelle ligne pour le ListView
            Dim item As New ListViewItem(cheque.id.ToString())
            item.SubItems.Add(cheque.montant_numerique.ToString())
            item.SubItems.Add(cheque.dateChq)
            item.SubItems.Add(cheque.destinataire)
            ' Ajouter la ligne au ListView
            lstCheques.Items.Add(item)
        Next

        ' Gérer l'événement de changement de sélection
        AddHandler lstCheques.SelectedIndexChanged, AddressOf LstCheques_SelectedIndexChanged
    End Sub

End Class
