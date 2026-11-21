Option Strict On
Imports System.Data
Imports System.IO
Imports System.Security.Cryptography
Imports System.Windows.Forms

Friend NotInheritable Class FrmRapprochementReleve
    Inherits Form
    Private ReadOnly operations As New ListBox With {.Dock=DockStyle.Fill}
    Private ReadOnly pieces As New CheckedListBox With {.Dock=DockStyle.Fill,.CheckOnClick=True,.HorizontalScrollbar=True}
    Private ReadOnly mouvements As New ComboBox With {.Dock=DockStyle.Top,.DropDownStyle=ComboBoxStyle.DropDownList}
    Private ReadOnly lignes As List(Of LigneReleve)
    Private ReadOnly hash As String

    Public Shared Sub Ouvrir(owner As IWin32Window)
        Using picker As New OpenFileDialog With {.Filter="Relevé bancaire CSV (*.csv)|*.csv",.Title="Analyser un relevé bancaire en TEST"}
            If picker.ShowDialog(owner) <> DialogResult.OK Then Return
            Try
                Dim parsed As List(Of LigneReleve), empreinte As String
                Using locked = File.Open(picker.FileName,FileMode.Open,FileAccess.Read,FileShare.Read)
                    empreinte = Convert.ToHexString(SHA256.HashData(locked))
                    parsed = LitRelevé.ParserFichier(picker.FileName)
                End Using
                If parsed.Count=0 Then Throw New FormatException("Aucune opération lue dans le relevé.")
                Dim store As New SqlDocumentTestStore()
                store.Initialize()
                Using form As New FrmRapprochementReleve(parsed,empreinte)
                    form.ShowDialog(owner)
                End Using
            Catch ex As Exception
                MessageBox.Show(owner,ex.Message,"Lecture du relevé TEST")
            End Try
        End Using
    End Sub

    Private Sub New(parsed As List(Of LigneReleve), empreinte As String)
        lignes=parsed
        hash=empreinte
        Text="TEST — Relevé bancaire et justificatifs"
        Width=1100
        Height=620
        Dim split As New SplitContainer With {.Dock=DockStyle.Fill,.Width=1050,.SplitterDistance=410}
        Dim save As New Button With {.Text="Rattacher les pièces cochées",.Dock=DockStyle.Bottom}
        split.Panel1.Controls.Add(operations)
        split.Panel2.Controls.Add(pieces)
        split.Panel2.Controls.Add(mouvements)
        split.Panel2.Controls.Add(save)
        Controls.Add(split)
        For Each ligne In lignes
            Dim m=ReleveToMouvementMapper.Mapper(ligne)
            operations.Items.Add($"{m.DateMvt:dd/MM/yyyy} · {If(m.Sens,"Crédit","Débit")} {m.Montant:N2} € · {m.Note}")
        Next
        AddHandler operations.SelectedIndexChanged, Sub() Afficher()
        AddHandler pieces.ItemCheck, Sub(sender, args)
            Dim view=TryCast(pieces.Items(args.Index),DataRowView)
            If view IsNot Nothing AndAlso CInt(view("lie"))=1 Then args.NewValue=CheckState.Checked
        End Sub
        AddHandler save.Click, Sub() Enregistrer()
        operations.SelectedIndex=0
    End Sub

    Private Sub Afficher()
        pieces.Items.Clear()
        mouvements.Items.Clear()
        If operations.SelectedIndex < 0 OrElse operations.SelectedIndex >= lignes.Count Then Return
        mouvements.Items.Add("Rattacher à la ligne du relevé uniquement")
        mouvements.SelectedIndex=0
        Try
            Dim index=operations.SelectedIndex, ligne=lignes(index)
            Using docs=SqlDocumentTestStore.JustificatifsReleve(hash,index,ligne)
                pieces.DisplayMember="affichage"
                docs.Columns.Add("affichage",GetType(String))
                For Each row As DataRow In docs.Rows
                    Dim motif=If(CInt(row("lie"))=1,"Déjà rattaché",If(CInt(row("refOk"))=1,"Référence concordante",If(CInt(row("montantOk"))=1,"Même montant",If(Not row.IsNull("partenaire"),"Somme exacte avec pièce n° " & Convert.ToString(row("partenaire")),"Date proche"))))
                    row("affichage")=$"N° {row("idDoc")} · {motif} · {row("dateDoc"):dd/MM/yyyy} · {row("categorieDoc")} · {row("montant")} · {Path.GetFileName(CStr(row("cheminDoc")))}"
                    pieces.Items.Add(row.Table.DefaultView(row.Table.Rows.IndexOf(row)),CInt(row("lie"))=1)
                Next
            End Using
            Dim m=ReleveToMouvementMapper.Mapper(ligne)
            Using choices=SqlDocumentTestStore.RelatedMovements(m.DateMvt,m.Montant)
                mouvements.DisplayMember="description"
                For Each view As DataRowView In choices.DefaultView
                    If CBool(view("sens"))=m.Sens Then mouvements.Items.Add(view)
                Next
            End Using
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,"Recherche des justificatifs TEST")
        End Try
    End Sub

    Friend Shared Function PeutRattacher(nombreJustificatifs As Integer, mouvementSelectionne As Boolean) As Boolean
        Return nombreJustificatifs > 0 OrElse mouvementSelectionne
    End Function

    Friend Shared Function ConfirmationRattachement(ids As IEnumerable(Of Integer), mouvementId As Integer?) As String
        Dim numeros = ids.Distinct().OrderBy(Function(n) n).ToArray()
        Dim details = If(numeros.Length = 0, "Aucun justificatif", "Justificatif(s) n° " & String.Join(", ", numeros))
        If mouvementId.HasValue Then details &= " ; mouvement n° " & mouvementId.Value.ToString()
        Return "Rattachement enregistré dans la base TEST." & Environment.NewLine & details
    End Function

    Private Sub Enregistrer()
        Try
            Dim index=operations.SelectedIndex
            If index < 0 OrElse index >= lignes.Count Then Return
            If Not PeutRattacher(pieces.CheckedItems.Count, TypeOf mouvements.SelectedItem Is DataRowView) Then
                MessageBox.Show(Me,"Sélectionnez au moins un justificatif ou un mouvement existant avant de valider.","Rattachement TEST")
                Return
            End If
            Dim ids=pieces.CheckedItems.Cast(Of DataRowView)().Select(Function(v) CInt(v("idDoc"))).ToArray()
            Dim chosen=TryCast(mouvements.SelectedItem,DataRowView)
            Dim id As Integer?=Nothing
            If chosen IsNot Nothing Then id=CInt(chosen("Id"))
            SqlDocumentTestStore.RattacherJustificatifs(hash,index,lignes(index),ids,id)
            MessageBox.Show(Me,ConfirmationRattachement(ids,id),"Rattachement TEST")
            Afficher()
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,"Rattachement TEST")
        End Try
    End Sub
End Class
