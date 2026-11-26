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
    Private ReadOnly connexion As SqlConnection
    Private coord As DataTable
    Private participations,pieces As DataTable
    Private chargement As Boolean

    Friend Sub New(cn As SqlConnection, lectureSeule As Boolean)
        connexion=cn
        Text="Tiers et participants — " & cn.Database
        Width=1080
        Height=710
        coord=RevueTiers.Lire(cn)
        ChargerInscriptions()
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
        If Not lectureSeule Then
            Dim ajouter As New Button With {.Text="Ajouter une participation…",.AutoSize=True}
            AddHandler ajouter.Click,AddressOf AjouterParticipation
            filtres.Controls.Add(ajouter)
        End If
        If Not lectureSeule Then
            Dim verifier As New Button With {.Text="Bulletins à vérifier…",.AutoSize=True}
            AddHandler verifier.Click,AddressOf VerifierBulletins
            filtres.Controls.Add(verifier)
        End If
        layout.Controls.Add(liste)
        Dim onglets As New TabControl With {.Dock=DockStyle.Fill}
        Dim adresses As New TabPage("Identité et coordonnées")
        onglets.TabPages.Add(adresses)
        adresses.Controls.Add(details)
        Dim historique As New TabPage("Historique des participations")
        onglets.TabPages.Add(historique)
        historique.Controls.Add(grilleHistorique)
        layout.Controls.Add(onglets)
        Dim justificatifs As New GroupBox With {.Text="Documents du tiers · double-cliquez pour ouvrir",.Dock=DockStyle.Fill}
        justificatifs.Controls.Add(documents)
        layout.Controls.Add(justificatifs)
        mode.Items.AddRange(New Object() {"Synthèse des tiers","Par activité","Par année"})
        annee.Items.AddRange(participations.AsEnumerable().Select(Function(r) CInt(r("Annee")).ToString()).Distinct().ToArray())
        If annee.Items.Count>0 Then annee.SelectedIndex=0
        AddHandler annee.SelectedIndexChanged,Sub()
            If Not chargement Then ActualiserFiltres()
        End Sub
        AddHandler mode.SelectedIndexChanged,Sub()
            If Not chargement Then ActualiserFiltres()
        End Sub
        AddHandler activite.SelectedIndexChanged,Sub() ActualiserRoles()
        AddHandler role.SelectedIndexChanged,Sub() ActualiserListe()
        AddHandler liste.SelectionChanged,Sub() ActualiserDetail()
        AddHandler documents.CellDoubleClick,AddressOf OuvrirPiece
        mode.SelectedIndex=0
    End Sub

    Private Sub VerifierBulletins(sender As Object,e As EventArgs)
        Try
            Using dossiers=Charger(connexion,"SELECT v.DocumentId,v.dateDoc,v.cheminDoc,v.TiersId,d.metaDonnees FROM dbo.VueInscriptionsAVerifier v JOIN dbo.Documents d ON d.idDoc=v.DocumentId ORDER BY v.dateDoc DESC"),
                  fenetre As New Form With {.Text="Bulletins dont la participation reste à vérifier",.Width=850,.Height=420,.StartPosition=FormStartPosition.CenterParent},
                  grille As New DataGridView With {.Dock=DockStyle.Fill,.DataSource=dossiers,.ReadOnly=True,.AllowUserToAddRows=False,.SelectionMode=DataGridViewSelectionMode.FullRowSelect},
                  valider As New Button With {.Dock=DockStyle.Bottom,.Text="Examiner le bulletin sélectionné",.DialogResult=DialogResult.OK}
                fenetre.Controls.Add(grille)
                fenetre.Controls.Add(valider)
                If fenetre.ShowDialog(Me)<>DialogResult.OK OrElse grille.CurrentRow Is Nothing Then Return
                Dim doc=DirectCast(grille.CurrentRow.DataBoundItem,DataRowView)
                Dim lu As New Newtonsoft.Json.Linq.JObject()
                Try
                    lu=Newtonsoft.Json.Linq.JObject.Parse(doc("metaDonnees").ToString())
                Catch ex As Newtonsoft.Json.JsonException
                    ' Les anciennes pièces sans métadonnées restent saisissables.
                End Try
                Dim id=If(IsDBNull(doc("TiersId")),0,Convert.ToInt32(doc("TiersId")))
                Using f As New FrmRevueTiers(coord,lu,Nothing,"Vérifier l’inscription — " & doc("cheminDoc").ToString(),id)
                    If f.ShowDialog(Me)<>DialogResult.OK OrElse String.IsNullOrWhiteSpace(f.Decision.Activite) Then Return
                    Using tx=connexion.BeginTransaction(IsolationLevel.Serializable)
                        If id=0 Then id=f.Decision.Enregistrer(connexion,tx,lu)
                        f.Decision.Rattacher(connexion,tx,CInt(doc("DocumentId")),id)
                        tx.Commit()
                    End Using
                End Using
            End Using
            Recharger()
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,Text)
        End Try
    End Sub

    Private Sub ChargerInscriptions()
        participations=Charger(connexion,"SELECT * FROM dbo.VueParticipantsActivite ORDER BY Annee DESC,Activite,RoleCours")
        pieces=Charger(connexion,"SELECT dt.TiersId,d.idDoc,d.dateDoc,d.categorieDoc,d.cheminDoc,v.Activite,v.Annee,v.RoleCours FROM dbo.DocumentTiers dt JOIN dbo.Documents d ON d.idDoc=dt.DocumentId LEFT JOIN dbo.JustificatifParticipation j ON j.DocumentId=d.idDoc LEFT JOIN dbo.VueParticipantsActivite v ON v.ParticipationId=j.ParticipationId ORDER BY d.dateDoc DESC")
    End Sub

    Private Sub Recharger()
        coord=RevueTiers.Lire(connexion)
        ChargerInscriptions()
        Dim avant=annee.Text
        chargement=True
        annee.Items.Clear()
        annee.Items.AddRange(participations.AsEnumerable().Select(Function(r) r("Annee").ToString()).Distinct().ToArray())
        If annee.Items.Contains(avant) Then
            annee.SelectedItem=avant
        ElseIf annee.Items.Count>0 Then
            annee.SelectedIndex=0
        End If
        chargement=False
        ActualiserFiltres()
    End Sub

    Private Sub AjouterParticipation(sender As Object,e As EventArgs)
        Dim selected=TryCast(liste.CurrentRow?.DataBoundItem,DataRowView)
        If selected Is Nothing Then Return
        Dim id=CInt(selected("Id"))
        Dim rows=coord.AsEnumerable().Where(Function(r) CInt(r("tiersId"))=id).CopyToDataTable()
        Using f As New FrmRevueTiers(rows,New Newtonsoft.Json.Linq.JObject(),Nothing,"Ajouter une participation",id)
            If f.ShowDialog(Me)<>DialogResult.OK Then Return
            If String.IsNullOrWhiteSpace(f.Decision.Activite) Then Return
            Try
                Using tx=connexion.BeginTransaction(IsolationLevel.Serializable)
                    f.Decision.Rattacher(connexion,tx,0,id)
                    tx.Commit()
                End Using
                Recharger()
            Catch ex As Exception
                MessageBox.Show(Me,ex.Message,Text)
            End Try
        End Using
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
