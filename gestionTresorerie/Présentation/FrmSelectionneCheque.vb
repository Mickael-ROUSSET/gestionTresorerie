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
                Dim unused3 = .Add(chq.montant_numerique)
                Dim unused2 = .Add(chq.dateChq)
                Dim unused1 = .Add(chq.destinataire)
            End With
            Dim unused = lstCheques.Items.Add(item)
        Next
    End Sub
    Private Sub LstCheques_SelectedIndexChanged(sender As Object, e As EventArgs)
        If lstCheques.SelectedItems.Count > 0 Then
            Dim selectedItem As ListViewItem = lstCheques.SelectedItems(0)
            Dim idChq As Integer = Integer.Parse(selectedItem.Text)
            Dim chq As New Cheque

            Cheque.AfficherImage(idChq, pbCheque)
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
        Close()
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
            Try
                Using readerChq As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqChq",
                             New Dictionary(Of String, Object) From {{"@montant", montant}}
                             ).ExecuteReader()
                    While readerChq.Read()
                        Try
                            Dim chqSel As New Cheque(readerChq.GetInt32(0), CStr(montant), readerChq.GetInt32(1), readerChq.GetDateTime(2), readerChq.GetString(3), readerChq.GetString(4))
                            tabCheques.Add(chqSel)
                        Catch ex As Exception
                            Dim unused2 = MessageBox.Show("Erreur lors de la lecture des données : " & ex.Message)
                        End Try
                    End While
                End Using
            Catch ex As Exception
                Dim unused1 = MessageBox.Show("Erreur lors de l'exécution de la commande SQL : " & ex.Message)
            End Try
            'End Using
        Else
            Dim unused = MessageBox.Show("Valeur de montant invalide.")
        End If
        Call alimListeChq(tabCheques)
    End Sub
    Public Sub alimListeChq(tabCheques As List(Of Cheque))
        ' Configurer le ListView
        lstCheques.View = View.Details
        Dim unused7 = lstCheques.Columns.Add("ID", 50, HorizontalAlignment.Left)
        Dim unused6 = lstCheques.Columns.Add("Montant", 100, HorizontalAlignment.Left)
        Dim unused5 = lstCheques.Columns.Add("Date", 100, HorizontalAlignment.Left)
        Dim unused4 = lstCheques.Columns.Add("Destinataire", 150, HorizontalAlignment.Left)
        lstCheques.FullRowSelect = True

        ' Ajouter chaque chèque au ListView
        For Each cheque As Cheque In tabCheques
            ' Créer une nouvelle ligne pour le ListView
            Dim item As New ListViewItem(cheque.id.ToString())
            Dim unused3 = item.SubItems.Add(cheque.montant_numerique.ToString())
            Dim unused2 = item.SubItems.Add(cheque.dateChq)
            Dim unused1 = item.SubItems.Add(cheque.destinataire)
            ' Ajouter la ligne au ListView
            Dim unused = lstCheques.Items.Add(item)
        Next

        ' Gérer l'événement de changement de sélection
        AddHandler lstCheques.SelectedIndexChanged, AddressOf LstCheques_SelectedIndexChanged
    End Sub

End Class