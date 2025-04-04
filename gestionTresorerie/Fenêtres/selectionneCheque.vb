Public Class selectionneCheque
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

        ' Configurez le ListView
        lstCheques.View = View.Details
        lstCheques.Columns.Add("ID", 50, HorizontalAlignment.Left)
        lstCheques.Columns.Add("Montant", 100, HorizontalAlignment.Left)
        lstCheques.Columns.Add("Date", 100, HorizontalAlignment.Left)
        lstCheques.Columns.Add("Destinataire", 150, HorizontalAlignment.Left)
        lstCheques.FullRowSelect = True

        '' Ajoutez des exemples de données au ListView
        'Dim item1 As New ListViewItem("1")
        'item1.SubItems.Add("100.00")
        'item1.SubItems.Add("2023-01-01")
        'item1.SubItems.Add("Destinataire 1")
        'lstCheques.Items.Add(item1)

        'Dim item2 As New ListViewItem("2")
        'item2.SubItems.Add("200.00")
        'item2.SubItems.Add("2023-02-01")
        'item2.SubItems.Add("Destinataire 2")
        'lstCheques.Items.Add(item2)

        ' Gérez l'événement de changement de sélection
        AddHandler lstCheques.SelectedIndexChanged, AddressOf LvCheques_SelectedIndexChanged
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
    Private Sub LvCheques_SelectedIndexChanged(sender As Object, e As EventArgs)
        If lstCheques.SelectedItems.Count > 0 Then
            Dim selectedItem As ListViewItem = lstCheques.SelectedItems(0)
            Dim idChq As Integer = Integer.Parse(selectedItem.Text)
            Dim chq As New Cheque

            chq.AfficherImage(idChq, pbCheque)
            _idChqSel = idChq
        End If
    End Sub

    Private Sub btnSelCheque_Click(sender As Object, e As EventArgs) Handles btnSelCheque.Click
        Me.Hide()
    End Sub
End Class
