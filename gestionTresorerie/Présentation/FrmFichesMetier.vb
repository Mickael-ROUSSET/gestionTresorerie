Option Strict On
Imports System.Data.SqlClient

Friend Class FrmFichesMetier
    Inherits Form
    Private Shared ReadOnly cles As New Dictionary(Of String,String) From {
        {"Tiers","Id"},{"Coordonnees","Id"},{"Films","IdFilm"},{"Tarifs","IdTarif"},{"Seances","IdSeance"},{"ParticipantMdN","IdParticipant"}}
    Private ReadOnly grille As New DataGridView With {.Dock=DockStyle.Fill,.AllowUserToAddRows=False,.AllowUserToDeleteRows=False,.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells}
    Private ReadOnly source As New BindingSource()
    Private ReadOnly donnees As New DataTable()
    Private ReadOnly cs, table As String

    Public Sub New(connexion As String, nomTable As String, titre As String, lectureSeule As Boolean)
        If Not cles.ContainsKey(nomTable) Then Throw New ArgumentException("Table inconnue.")
        cs=connexion
        table=nomTable
        Text=titre & " — " & New SqlConnectionStringBuilder(cs).InitialCatalog
        Width=1080
        Height=590
        StartPosition=FormStartPosition.CenterParent
        Using cn As New SqlConnection(cs), da As New SqlDataAdapter("SELECT * FROM dbo.[" & table & "] ORDER BY [" & cles(table) & "]",cn)
            da.Fill(donnees)
        End Using
        source.DataSource=donnees
        grille.DataSource=source
        grille.ReadOnly=lectureSeule
        Controls.Add(grille)
        Dim barre As New FlowLayoutPanel With {.Dock=DockStyle.Bottom,.AutoSize=True}
        Dim enregistrer As New Button With {.Text="Enregistrer les modifications",.AutoSize=True,.Enabled=Not lectureSeule}
        Dim recherche As New TextBox With {.Width=240,.PlaceholderText="Rechercher dans les fiches…"}
        barre.Controls.Add(recherche)
        AddHandler recherche.TextChanged, Sub()
            Dim motif=recherche.Text.Replace("'","''").Replace("[","[[]").Replace("*","[*]").Replace("%","[%]")
            Dim champs=donnees.Columns.Cast(Of DataColumn)().Where(Function(col) col.DataType Is GetType(String)).Select(Function(col) "[" & col.ColumnName & "] LIKE '%" & motif & "%'").ToArray()
            If champs.Length>0 Then source.Filter=If(motif.Length=0,"",String.Join(" OR ",champs))
        End Sub
        barre.Controls.Add(enregistrer)
        Controls.Add(barre)
        AddHandler Load, Sub()
            For Each col As DataGridViewColumn In grille.Columns
                If col.DataPropertyName.StartsWith("Id",StringComparison.OrdinalIgnoreCase) OrElse col.DataPropertyName="dateCreation" OrElse col.DataPropertyName="dateModification" Then col.ReadOnly=True
            Next
            If table="Seances" AndAlso grille.Columns.Contains("IdFilm") Then
                Using films As New SqlDataAdapter("SELECT IdFilm,Titre FROM dbo.Films ORDER BY Titre",cs)
                    Dim catalogue As New DataTable()
                    films.Fill(catalogue)
                    grille.Columns.Remove("IdFilm")
                    grille.Columns.Insert(1,New DataGridViewComboBoxColumn With {.Name="Film",.HeaderText="Film",.DataPropertyName="IdFilm",.DataSource=catalogue,.DisplayMember="Titre",.ValueMember="IdFilm",.ReadOnly=lectureSeule})
                End Using
            End If
        End Sub
        AddHandler enregistrer.Click, Sub()
            Try
                If Not grille.EndEdit() Then Return
                source.EndEdit()
                Sauver(cs,table,donnees)
                MessageBox.Show(Me,"Modifications enregistrées.",Text)
            Catch ex As Exception
                MessageBox.Show(Me,ex.Message,Text)
            End Try
        End Sub
        AddHandler FormClosing, Sub(o,args)
            If lectureSeule Then Return
            grille.EndEdit()
            source.EndEdit()
            If donnees.GetChanges() IsNot Nothing Then args.Cancel=MessageBox.Show(Me,"Fermer et abandonner les modifications ?",Text,MessageBoxButtons.YesNo)<>DialogResult.Yes
        End Sub
    End Sub

    Friend Shared Sub Sauver(connexion As String, table As String, donnees As DataTable)
        Dim cle=cles(table)
        Using cn As New SqlConnection(connexion)
            cn.Open()
            Using tx=cn.BeginTransaction()
                For Each row As DataRow In donnees.Select(Nothing,Nothing,DataViewRowState.ModifiedCurrent)
                    Using cmd=cn.CreateCommand()
                        cmd.Transaction=tx
                        Dim sets As New List(Of String)
                        Dim conditions As New List(Of String) From {"[" & cle & "]=@id"}
                        cmd.Parameters.AddWithValue("@id",row(cle,DataRowVersion.Original))
                        For Each col As DataColumn In donnees.Columns
                            Dim avant=row(col,DataRowVersion.Original)
                            If Object.Equals(avant,row(col)) Then Continue For
                            Dim nom=col.ColumnName
                            If nom=cle OrElse nom="dateCreation" OrElse nom="dateModification" Then Throw New ArgumentException("Champ protégé : " & nom)
                            Dim ident="[" & nom.Replace("]","]]") & "]"
                            Dim param="@v" & col.Ordinal
                            sets.Add(ident & "=" & param)
                            cmd.Parameters.AddWithValue(param,row(col))
                            If IsDBNull(avant) Then
                                conditions.Add(ident & " IS NULL")
                            Else
                                conditions.Add(ident & "=" & param & "Orig")
                                cmd.Parameters.AddWithValue(param & "Orig",avant)
                            End If
                        Next
                        If sets.Count=0 Then Continue For
                        If table="Tiers" Then sets.Add("[dateModification]=GETDATE()")
                        cmd.CommandText="UPDATE dbo.[" & table & "] SET " & String.Join(",",sets) & " WHERE " & String.Join(" AND ",conditions)
                        If cmd.ExecuteNonQuery()<>1 Then Throw New DBConcurrencyException("Une fiche a changé depuis son ouverture. Rouvrez la fenêtre pour la relire.")
                    End Using
                Next
                tx.Commit()
            End Using
        End Using
        donnees.AcceptChanges()
    End Sub
End Class