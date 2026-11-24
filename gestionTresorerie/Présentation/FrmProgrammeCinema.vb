Imports System.Globalization
Imports System.IO

Friend Class FrmProgrammeCinema
    Inherits Form
    Private ReadOnly grille As New DataGridView With {.Dock=DockStyle.Fill,.AllowUserToAddRows=True,.AllowUserToDeleteRows=True}
    Private ReadOnly generer As New Button With {.Text="Générer le programme Word…",.AutoSize=True}
    Private ReadOnly publier As New Button With {.Text="Publier en ligne…",.AutoSize=True}
    Private occupe As Boolean
    Private Shared ReadOnly france As New CultureInfo("fr-FR")

    Friend Sub New(Optional publicationAutorisee As Boolean=False)
        Text="Programme du cinéma d’Allègre"
        Width=980
        Height=480
        StartPosition=FormStartPosition.CenterParent
        For Each titre In {"Film","Date (jj/mm/aaaa)","Heure (20h30)"}
            grille.Columns.Add("col" & grille.Columns.Count,titre)
        Next
        grille.Columns(0).Width=205
        grille.Columns(0).AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill
        Controls.Add(grille)
        Dim barre As New FlowLayoutPanel With {.Dock=DockStyle.Bottom,.AutoSize=True}
        Dim importer As New Button With {.Text="Importer un fichier de séances…",.AutoSize=True}
        barre.Controls.AddRange(New Control(){importer,generer,publier})
        publier.Enabled=publicationAutorisee
        AddHandler publier.Click,AddressOf Exporter
        Controls.Add(barre)
        AddHandler importer.Click,Sub()
            Using dlg As New OpenFileDialog With {.Filter="Séances (titre;date;heure)|*.txt;*.csv"}
                If dlg.ShowDialog(Me)<>DialogResult.OK Then Return
                Try
                    Dim lignes=File.ReadAllLines(dlg.FileName).Where(Function(l) Not String.IsNullOrWhiteSpace(l)).Select(AddressOf ProgrammeParser.ParseLigne).ToList()
                    If lignes.Any(Function(l) l Is Nothing) Then Throw New InvalidDataException("Une ligne est incomplète. Format attendu : titre;date;heure.")
                    For Each film In lignes
                        grille.Rows.Add(film.Titre,film.DateDiffusion.ToString("dd/MM/yyyy",france),film.HeureDiffusion.ToString("hh\:mm"))
                    Next
                Catch ex As Exception
                    MessageBox.Show(Me,ex.Message,Text)
                End Try
            End Using
        End Sub
        AddHandler generer.Click,AddressOf Exporter
        AddHandler FormClosing,Sub(o,args)
            args.Cancel=occupe
        End Sub
    End Sub

    Private Async Sub Exporter(sender As Object,args As EventArgs)
        grille.EndEdit()
        Dim temp As String=Nothing
        Try
            Dim films As New List(Of FilmSeance)
            For Each ligne As DataGridViewRow In grille.Rows
                If ligne.IsNewRow Then Continue For
                Dim titre=Convert.ToString(ligne.Cells(0).Value).Trim()
                Dim jour As Date, heure As DateTime
                If titre="" OrElse Not Date.TryParseExact(Convert.ToString(ligne.Cells(1).Value),{"d/M/yyyy","yyyy-MM-dd"},france,DateTimeStyles.None,jour) OrElse
                    Not DateTime.TryParseExact(Convert.ToString(ligne.Cells(2).Value).Trim().ToLowerInvariant().Replace("h",":"),{"H:mm","HH:mm"},france,DateTimeStyles.None,heure) Then Throw New ArgumentException("Vérifiez le titre, la date et l’heure de la ligne " & (ligne.Index+1))
                films.Add(New FilmSeance With {.Titre=titre,.DateDiffusion=jour,.HeureDiffusion=heure.TimeOfDay})
            Next
            If films.Count=0 Then Throw New ArgumentException("Ajoutez au moins une séance.")
            films=films.OrderBy(Function(f) f.DateDiffusion+f.HeureDiffusion).ToList()
            Using dlg As New SaveFileDialog With {.Filter="Programme Word|*.docx",.FileName="Programme cinéma " & films(0).DateDiffusion.ToString("yyyy-MM-dd") & ".docx"}
                If sender IsNot publier AndAlso dlg.ShowDialog(Me)<>DialogResult.OK Then Return
                occupe=True
                Enabled=False
                UseWaitCursor=True
                Dim service As New ProgrammeCinemaCreator
                Dim avertissement=""
                Try
                    Await service.EnrichirListeAsync(films)
                Catch ex As Exception
                    avertissement=vbCrLf & "Enrichissement TMDb indisponible."
                End Try
                If sender Is publier Then
                    Await PublicationCinema.Publier(films,Me)
                    Return
                End If
                temp=Path.Combine(Path.GetDirectoryName(dlg.FileName),Guid.NewGuid().ToString("N") & ".docx")
                If Not Await service.GenererDocxAsync(films,temp) Then Throw New IOException("La création du document Word a échoué.")
                File.Move(temp,dlg.FileName,True)
                Dim incomplets=films.Where(Function(f) String.IsNullOrWhiteSpace(f.Synopsis)).Select(Function(f) f.Titre).Distinct().ToArray()
                If incomplets.Length>0 Then avertissement &= vbCrLf & "Synopsis indisponible pour : " & String.Join(", ",incomplets)
                MessageBox.Show(Me,"Programme enregistré dans " & dlg.FileName & avertissement,Text)
            End Using
        Catch ex As Exception
            MessageBox.Show(Me,ex.Message,Text)
        Finally
            occupe=False
            Enabled=True
            UseWaitCursor=False
            If temp IsNot Nothing AndAlso File.Exists(temp) Then File.Delete(temp)
        End Try
    End Sub
End Class