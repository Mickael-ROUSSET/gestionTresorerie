Option Strict On
Imports System.Data
Imports System.Data.SqlClient
Imports System.Diagnostics
Imports System.IO

Friend Class FrmParticipants
    Inherits Form
    Private ReadOnly mode,annee,activite,role As New ComboBox With {.DropDownStyle=ComboBoxStyle.DropDownList,.Width=150}
    Private ReadOnly liste,details,documents As New DataGridView With {.Dock=DockStyle.Fill,.ReadOnly=True,.AllowUserToAddRows=False,.AllowUserToDeleteRows=False,.MultiSelect=False,.SelectionMode=DataGridViewSelectionMode.FullRowSelect,.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells}
    Private ReadOnly total As New Label With {.AutoSize=True,.Margin=New Padding(8)}
    Private ReadOnly coord,participations,pieces As DataTable
    Private chargement As Boolean

    Friend Sub New(cn As SqlConnection)
        Text="Tiers et participants — " & cn.Database
        Width=1080
        Height=710
        coord=RevueTiers.Lire(cn)
        participations=Charger(cn,"SELECT * FROM dbo.VueParticipantsActivite ORDER BY Annee DESC,Activite,RoleCours")
        pieces=Charger(cn,"SELECT dt.TiersId,d.idDoc,d.dateDoc,d.categorieDoc,d.cheminDoc,v.Activite,v.Annee,v.RoleCours FROM dbo.DocumentTiers dt JOIN dbo.Documents d ON d.idDoc=dt.DocumentId LEFT JOIN dbo.JustificatifParticipation j ON j.DocumentId=d.idDoc LEFT JOIN dbo.VueParticipantsActivite v ON v.ParticipationId=j.ParticipationId ORDER BY d.dateDoc DESC")
        Dim layout As New TableLayoutPanel With {.Dock=DockStyle.Fill,.RowCount=4,.ColumnCount=1}
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute,78))
        For Each poids In {50,25,25}
            layout.RowStyles.Add(New RowStyle(SizeType.Percent,poids))
        Next
        Controls.Add(layout)
        Dim filtres As New FlowLayoutPanel With {.Dock=DockStyle.Fill}
        layout.Controls.Add(filtres)
        For Each pair In New Dictionary(Of String,ComboBox) From {{"Vue",mode},{"Année de début",annee},{"Activité",activite},{"Cours / rôle",role}}
            filtres.Controls.Add(New Label With {.AutoSize=True,.Text=pair.Key,.Margin=New Padding(3,7,0,0)})
            filtres.Controls.Add(pair.Value)
        Next
        filtres.Controls.Add(total)
        layout.Controls.Add(liste)
        Dim onglets As New TabControl With {.Dock=DockStyle.Fill}
        Dim adresses=onglets.TabPages.Add("Identité et coordonnées")
        adresses.Controls.Add(details)
        Dim historique=onglets.TabPages.Add("Historique des participations")
        historique.Controls.Add(grilleHistorique)
        layout.Controls.Add(onglets)
        Dim justificatifs As New GroupBox With {.Text="Documents du tiers · double-cliquez pour ouvrir",.Dock=DockStyle.Fill}
        justificatifs.Controls.Add(documents)
        layout.Controls.Add(justificatifs)
        mode.Items.AddRange(New Object() {"Synthèse des tiers","Par activité","Par année"})
        annee.Items.AddRange(participations.AsEnumerable().Select(Function(r) CInt(r("Annee")).ToString()).Distinct().ToArray())
        If annee.Items.Count>0 Then annee.SelectedIndex=0
        AddHandler annee.SelectedIndexChanged,Sub() ActualiserFiltres()
        AddHandler mode.SelectedIndexChanged,Sub() ActualiserFiltres()
        AddHandler activite.SelectedIndexChanged,Sub() ActualiserRoles()
        AddHandler role.SelectedIndexChanged,Sub() ActualiserListe()
        AddHandler liste.SelectionChanged,Sub() ActualiserDetail()
        AddHandler documents.CellDoubleClick,AddressOf OuvrirPiece
        mode.SelectedIndex=0
    End Sub

    Private ReadOnly grilleHistorique As New DataGridView With {.Dock=DockStyle.Fill,.ReadOnly=True,.AllowUserToAddRows=False,.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells}
    Private Shared Function Charger(cn As SqlConnection,sql As String) As DataTable
        Using cmd As New SqlCommand(sql,cn), reader=cmd.ExecuteReader()
            Dim t As New DataTable()
            t.Load(reader)
            Return t
        End Using
    End Function

    Private Function Edition() As IEnumerable(Of DataRow)
        Return participations.AsEnumerable().Where(Function(r) r("Annee").ToString()=annee.Text)
    End Function

    Private Sub ActualiserFiltres()
        chargement=True
        annee.Enabled=mode.SelectedIndex<>0
        activite.Enabled=mode.SelectedIndex=1
        role.Enabled=activite.Enabled
        activite.Items.Clear()
        activite.Items.AddRange(Edition().Select(Function(r) r("Activite").ToString()).Distinct().ToArray())
        If activite.Items.Count>0 Then activite.SelectedIndex=0
        chargement=False
        ActualiserRoles()
    End Sub

    Private Sub ActualiserRoles()
        If chargement Then Return
        chargement=True
        role.Items.Clear()
        role.Items.Add("Tous")
        role.Items.AddRange(Edition().Where(Function(r) r("Activite").ToString()=activite.Text).Select(Function(r) r("RoleCours").ToString()).Where(Function(v) v<>"").Distinct().ToArray())
        role.SelectedIndex=0
        chargement=False
        ActualiserListe()
    End Sub

    Private Sub ActualiserListe()
        If chargement Then Return
        Dim selection=Edition()
        If mode.SelectedIndex=1 Then selection=selection.Where(Function(r) r("Activite").ToString()=activite.Text AndAlso (role.SelectedIndex=0 OrElse r("RoleCours").ToString()=role.Text))
        Dim inscriptions=selection.ToLookup(Function(r) CInt(r("TiersId")))
        Dim resultat As New DataTable()
        For Each k In {"Id","Nom","Prénom","Organisme","Courriel","Téléphone","Activités et cours"}
            resultat.Columns.Add(k,If(k="Id",GetType(Integer),GetType(String)))
        Next
        For Each groupe In coord.AsEnumerable().GroupBy(Function(r) CInt(r("tiersId")))
            If mode.SelectedIndex<>0 AndAlso Not inscriptions.Contains(groupe.Key) Then Continue For
            Dim r=groupe.First()
            Dim activitesDuTiers=If(mode.SelectedIndex=0,participations.AsEnumerable().Where(Function(p) CInt(p("TiersId"))=groupe.Key),inscriptions(groupe.Key))
            resultat.Rows.Add(groupe.Key,r("nom"),r("prenom"),r("raisonSociale"),Joindre(groupe,"mail"),Joindre(groupe,"telephone"),
                String.Join(", ",activitesDuTiers.Select(Function(p) (p("Activite").ToString() & " " & p("Annee").ToString() & " " & p("RoleCours").ToString()).Trim()).Distinct()))
        Next
        liste.DataSource=resultat
        total.Text=resultat.Rows.Count.ToString() & " tiers"
        ActualiserDetail()
    End Sub

    Private Shared Function Joindre(rows As IEnumerable(Of DataRow),champ As String) As String
        Return String.Join(", ",rows.Select(Function(r) r(champ).ToString()).Where(Function(v) v<>"").Distinct())
    End Function

    Private Sub ActualiserDetail()
        Dim ligne=TryCast(liste.CurrentRow?.DataBoundItem,DataRowView)
        Dim id=If(ligne Is Nothing,-1,CInt(ligne("Id")))
        Dim filtre="tiersId=" & id.ToString()
        details.DataSource=New DataView(coord,filtre,"",DataViewRowState.CurrentRows)
        grilleHistorique.DataSource=New DataView(participations,filtre,"Annee DESC",DataViewRowState.CurrentRows)
        documents.DataSource=New DataView(pieces,filtre,"dateDoc DESC",DataViewRowState.CurrentRows)
    End Sub

    Private Sub OuvrirPiece(sender As Object,e As DataGridViewCellEventArgs)
        If e.RowIndex<0 Then Return
        Dim chemin=CStr(documents.Rows(e.RowIndex).Cells("cheminDoc").Value)
        If Not File.Exists(chemin) OrElse Not DocumentFilePreview.Supports(chemin) Then
            MessageBox.Show(Me,"Pièce inaccessible ou format non pris en charge.")
            Return
        End If
        Using fenetre As New FrmDocumentFilePreview({chemin})
            fenetre.ShowDialog(Me)
        End Using
    End Sub
End Class
