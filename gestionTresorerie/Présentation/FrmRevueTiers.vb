Option Strict On
Imports System.Data
Imports Newtonsoft.Json.Linq

Friend Class FrmRevueTiers
    Inherits Form
    Private ReadOnly choix As New ComboBox With {.Dock=DockStyle.Fill,.DropDownStyle=ComboBoxStyle.DropDownList}
    Private ReadOnly grille As New DataGridView With {.Dock=DockStyle.Fill,.AllowUserToAddRows=False,.AllowUserToDeleteRows=False,.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,.RowHeadersVisible=False}
    Private ReadOnly activite As New ComboBox With {.Width=135}
    Private ReadOnly annee As New TextBox With {.Width=58}
    Private ReadOnly role As New ComboBox With {.Width=125}
    Private ReadOnly debut,fin As New DateTimePicker With {.Width=110,.Format=DateTimePickerFormat.Short,.ShowCheckBox=True}
    Private ReadOnly lignes As New List(Of DataRow)
    Private ReadOnly inscriptions As DataTable
    Private ReadOnly existante As New ComboBox With {.Width=275,.DropDownStyle=ComboBoxStyle.DropDownList}
    Private ReadOnly lu As JObject
    Friend ReadOnly Property Decision As New RevueTiers()

    Friend Sub New(table As DataTable, champsLus As JObject, inscription As JObject, titre As String, Optional tiersFixe As Integer=0, Optional piece As String=Nothing, Optional participations As DataTable=Nothing)
        lu=champsLus
        inscriptions=participations
        Text=titre
        Width=920
        Height=640
        StartPosition=FormStartPosition.CenterParent
        Dim layout As New TableLayoutPanel With {.Dock=DockStyle.Fill,.RowCount=5,.ColumnCount=1,.Padding=New Padding(8)}
        Controls.Add(layout)
        For Each hauteur In {28,36,0,96,42}
            layout.RowStyles.Add(If(hauteur=0,New RowStyle(SizeType.Percent,100),New RowStyle(SizeType.Absolute,hauteur)))
        Next
        layout.Controls.Add(New Label With {.Text="Sélectionnez le tiers et son adresse, ou créez une fiche.",.AutoSize=True})
        layout.Controls.Add(choix)
        layout.Controls.Add(grille)
        For Each nom In {"Champ","En base","Lu sur la pièce","Valeur à retenir"}
            grille.Columns.Add(nom,nom)
            grille.Columns(grille.Columns.Count-1).SortMode=DataGridViewColumnSortMode.NotSortable
        Next
        For i=0 To 2
            grille.Columns(i).ReadOnly=True
        Next
        Dim candidats=table.AsEnumerable().Where(Function(r) RevueTiers.Correspond(r,lu)).ToList()
        Decision.CandidatsInitiaux=candidats.Select(Function(r) CInt(r("tiersId"))).Distinct().OrderBy(Function(n) n).ToArray()
        choix.Items.Add("Créer un tiers")
        lignes.Add(Nothing)
        For Each r In candidats.Concat(table.AsEnumerable().Except(candidats))
            lignes.Add(r)
            choix.Items.Add("#" & r("tiersId").ToString() & "  " & String.Join(" · ", {"raisonSociale","nom","prenom","adresse","ville","mail","telephone"}.Select(Function(k) r(k).ToString()).Where(Function(v) v<>"")))
        Next
        AddHandler choix.SelectedIndexChanged,Sub() AfficherChamps()
        If tiersFixe>0 Then
            choix.SelectedIndex=lignes.FindIndex(Function(r) r IsNot Nothing AndAlso CInt(r("tiersId"))=tiersFixe)
            choix.Enabled=False
            grille.ReadOnly=True
        ElseIf candidats.Count<=1 Then
            choix.SelectedIndex=candidats.Count
        End If
        Dim saisie As New FlowLayoutPanel With {.Dock=DockStyle.Fill}
        layout.Controls.Add(saisie)
        saisie.Controls.Add(New Label With {.Text="Rattacher à une inscription existante",.AutoSize=True})
        saisie.Controls.Add(existante)
        saisie.SetFlowBreak(existante,True)
        AddHandler existante.SelectedIndexChanged,Sub()
            If Not existante.Focused Then Return
            Dim row=TryCast(existante.SelectedItem,DataRowView)
            If row Is Nothing Then Return
            activite.Text=row("Activite").ToString()
            annee.Text=row("Annee").ToString()
            role.Text=row("RoleCours").ToString()
        End Sub
        existante.SelectedIndex=-1
        activite.Items.AddRange(New Object() {"Gym","Marché de Noël"})
        role.Items.AddRange(New Object() {"Gym douce","Gym tonique","Exposant"})
        For Each pair In New Dictionary(Of String,Control) From {{"Activité (facultative)",activite},{"Année de début",annee},{"Cours / rôle",role},{"Début",debut},{"Fin",fin}}
            saisie.Controls.Add(New Label With {.Text=pair.Key,.AutoSize=True,.Margin=New Padding(4,7,2,0)})
            saisie.Controls.Add(pair.Value)
        Next
        debut.Checked=False
        fin.Checked=False
        activite.Text=If(inscription?("activite")?.ToString(),"")
        annee.Text=If(inscription?("annee")?.ToString(),"")
        role.Text=If(inscription?("cours_role")?.ToString(),"")
        Dim actions As New FlowLayoutPanel With {.Dock=DockStyle.Fill,.FlowDirection=FlowDirection.RightToLeft}
        Dim ok As New Button With {.Text="Valider",.AutoSize=True}
        AddHandler ok.Click, AddressOf Valider
        Dim annuler As New Button With {.Text="Annuler",.DialogResult=DialogResult.Cancel}
        Dim garder As New Button With {.Text="Conserver la fiche existante",.AutoSize=True}
        AddHandler garder.Click,Sub()
            If choix.SelectedIndex<=0 Then Return
            For Each r As DataGridViewRow In grille.Rows
                r.Cells(3).Value=r.Cells(1).Value
            Next
        End Sub
        garder.Visible=tiersFixe=0
        actions.Controls.AddRange({ok,annuler,garder})
        If Not String.IsNullOrWhiteSpace(piece) AndAlso IO.File.Exists(piece) AndAlso DocumentFilePreview.Supports(piece) Then
            Dim ouvrir As New Button With {.Text="Voir le document",.AutoSize=True}
            AddHandler ouvrir.Click,Sub()
                Using apercu As New FrmDocumentFilePreview({piece},consultation:=True)
                    apercu.ShowDialog(Me)
                End Using
            End Sub
            actions.Controls.Add(ouvrir)
        End If
        CancelButton=annuler
        layout.Controls.Add(actions)
    End Sub

    Private Sub AfficherChamps()
        grille.Rows.Clear()
        Decision.Avant=lignes(choix.SelectedIndex)
        activite.Text=""
        annee.Clear()
        role.Text=""
        existante.DataSource=Nothing
        If inscriptions IsNot Nothing AndAlso Decision.Avant IsNot Nothing Then
            existante.DisplayMember="libelle"
            existante.DataSource=New DataView(inscriptions,"tiersId=" & Decision.Avant("tiersId").ToString(),"Annee DESC",DataViewRowState.CurrentRows)
            existante.SelectedIndex=-1
        End If
        For i=0 To RevueTiers.Champs.Length-1
            Dim k=RevueTiers.Champs(i)
            Dim avant=If(Decision.Avant Is Nothing,"",Decision.Avant(k).ToString())
            Dim extrait=If(lu(k)?.ToString(),"")
            grille.Rows.Add(RevueTiers.Libelles(i),avant,extrait,If(extrait="",avant,extrait))
        Next
    End Sub

    Private Sub Valider(sender As Object,e As EventArgs)
        If choix.SelectedIndex<0 Then
            MessageBox.Show(Me,"Sélectionnez la personne et l’adresse à retenir.")
            Return
        End If
        grille.EndEdit()
        For i=0 To RevueTiers.Champs.Length-1
            Decision.Valeurs(RevueTiers.Champs(i))=Convert.ToString(grille.Rows(i).Cells(3).Value).Trim()
        Next
        If CStr(Decision.Valeurs("nom"))="" AndAlso CStr(Decision.Valeurs("raisonSociale"))="" Then
            MessageBox.Show(Me,"Renseignez le nom ou la raison sociale.")
            Return
        End If
        Decision.Activite=activite.Text.Trim()
        Decision.RoleCours=role.Text.Trim()
        If Decision.Activite<>"" Then
            Dim reference As Integer
            If Not Integer.TryParse(annee.Text,reference) OrElse reference<1900 OrElse reference>9999 OrElse (debut.Checked AndAlso debut.Value.Year<>reference) OrElse (debut.Checked AndAlso fin.Checked AndAlso fin.Value.Date<debut.Value.Date) Then
                MessageBox.Show(Me,"Vérifiez l’année de référence et les dates de l’édition.")
                Return
            End If
            Decision.Annee=reference
            If debut.Checked Then Decision.Debut=debut.Value.Date
            If fin.Checked Then Decision.Fin=fin.Value.Date
        End If
        DialogResult=DialogResult.OK
    End Sub
End Class
