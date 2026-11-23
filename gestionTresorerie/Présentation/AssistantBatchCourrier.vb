Option Strict On
Imports System.IO
Imports System.Windows.Forms
Imports Newtonsoft.Json

Friend Class AssistantBatchCourrier
    Inherits Form
    Private ReadOnly pages As New TabControl With {.Dock = DockStyle.Fill}
    Private ReadOnly nom As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly sujet As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly email As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly modele As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly pieces As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly parametres As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly balises As New TextBox With {.Dock = DockStyle.Top}
    Private original As New BatchMailConfig()
    Private fichierOuvert As String
    Private ReadOnly description, balisesSujet, balisesPJ As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly rapport As New TextBox With {.Dock = DockStyle.Top}
    Private ReadOnly requete As New ComboBox With {.Dock = DockStyle.Top, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly suivant As New Button With {.AutoSize = True}
    Private ReadOnly retour As New Button With {.Text = "Précédent", .AutoSize = True}

    Friend Shared Function Requetes() As List(Of String)
        Dim noms As New List(Of String)
        For Each p As System.Configuration.SettingsProperty In My.Settings.Properties
            If p.PropertyType Is GetType(String) AndAlso Convert.ToString(My.Settings(p.Name)).TrimStart().StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase) Then noms.Add(p.Name)
        Next
        noms.Sort()
        Return noms
    End Function

    Friend Sub New()
        Text = "Assistant de création d’un batch de courriers"
        Width = 740
        Height = 570
        StartPosition = FormStartPosition.CenterParent
        Dim page = AjouterPage("1. Destinataires")
        Dim ouvrir As New Button With {.Text = "Reprendre un batch existant…", .AutoSize = True}
        page.Controls.Add(ouvrir)
        AddHandler ouvrir.Click, Sub()
            Using dlg As New OpenFileDialog With {.Filter = "Batch JSON|*.json", .InitialDirectory = Path.Combine(AppContext.BaseDirectory,"Batchs")}
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                Try
                    Charger(dlg.FileName)
                Catch ex As Exception
                    MessageBox.Show(Me, ex.Message, "Lecture du batch")
                End Try
            End Using
        End Sub
        Champ(page, "Nom du batch", nom)
        Champ(page, "Description", description)
        Champ(page, "Requête enregistrée", requete)
        requete.Items.AddRange(Requetes().ToArray())
        requete.SelectedItem = "reqIdentiteCoordonnees"
        email.Text = "Email"
        Champ(page, "Colonne contenant l’adresse électronique", email)
        Multiligne(parametres)
        Champ(page, "Paramètres éventuels de la requête, un par ligne (@Id=12)", parametres)
        page = AjouterPage("2. Courrier")
        Champ(page, "Objet du courrier", sujet)
        Champ(page, "Modèle HTML du corps du courrier", modele)
        Parcourir(page, modele, "Modèle HTML|*.html;*.htm", False)
        Multiligne(balises)
        Multiligne(balisesSujet)
        Champ(page, "Balises de l’objet, par exemple {Prenom}=Prenom", balisesSujet)
        Champ(page, "Balises du corps du message", balises)
        page = AjouterPage("3. Fichiers")
        Multiligne(balisesPJ)
        Champ(page, "Balises des modèles Word", balisesPJ)
        Multiligne(pieces)
        Champ(page, "Pièces jointes facultatives, un chemin par ligne", pieces)
        Parcourir(page, pieces, "Tous fichiers|*.*", True)
        Champ(page, "Chemin de base facultatif des rapports .txt et .json", rapport)
        Dim barre As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft}
        barre.Controls.AddRange(New Control() {suivant, retour})
        Controls.Add(pages)
        Controls.Add(barre)
        AddHandler suivant.Click, AddressOf Avancer
        AddHandler retour.Click, Sub() pages.SelectedIndex -= 1
        AddHandler pages.SelectedIndexChanged, Sub() Actualiser()
        Actualiser()
    End Sub

    Private Function AjouterPage(titre As String) As TableLayoutPanel
        Dim onglet As New TabPage(titre)
        Dim table As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .Padding = New Padding(10), .AutoScroll = True, .AutoSize = False}
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        onglet.Controls.Add(table)
        pages.TabPages.Add(onglet)
        Return table
    End Function

    Private Shared Sub Champ(table As TableLayoutPanel, titre As String, controle As Control)
        table.Controls.Add(New Label With {.Text = titre, .AutoSize = True})
        table.Controls.Add(controle)
    End Sub

    Private Shared Sub Multiligne(champ As TextBox)
        champ.Multiline = True
        champ.Height = 72
        champ.ScrollBars = ScrollBars.Vertical
    End Sub

    Private Shared Sub Parcourir(table As TableLayoutPanel, champ As TextBox, filtre As String, multiple As Boolean)
        Dim bouton As New Button With {.Text = "Parcourir…", .AutoSize = True}
        table.Controls.Add(bouton)
        AddHandler bouton.Click, Sub()
            Using dlg As New OpenFileDialog With {.Filter = filtre, .Multiselect = multiple}
                If dlg.ShowDialog(table.FindForm()) = DialogResult.OK Then champ.Lines = dlg.FileNames
            End Using
        End Sub
    End Sub

    Friend Sub Charger(chemin As String)
        Dim cfg = BatchMailConfig.Load(chemin)
        If cfg Is Nothing OrElse String.IsNullOrWhiteSpace(cfg.NomRequeteSQL) Then Throw New InvalidDataException("Paramétrage de batch non reconnu.")
        original = cfg
        fichierOuvert = chemin
        nom.Text = cfg.NomBatch
        description.Text = cfg.Description
        requete.SelectedItem = cfg.NomRequeteSQL
        email.Text = cfg.ColonneEmail
        sujet.Text = cfg.SujetMail
        modele.Text = cfg.CheminTemplateCorpsEmail
        pieces.Lines = cfg.CheminsPjRessource.ToArray()
        parametres.Lines = cfg.ParametresRequeteSQL.Select(Function(p) p.Key & "=" & Convert.ToString(p.Value)).ToArray()
        balises.Lines = cfg.RplctCorpsMail.Select(Function(p) p.Key & "=" & p.Value).ToArray()
        balisesSujet.Lines = cfg.RplctSujetMail.Select(Function(p) p.Key & "=" & p.Value).ToArray()
        balisesPJ.Lines = cfg.RplctPJ.Select(Function(p) p.Key & "=" & p.Value).ToArray()
        rapport.Text = cfg.CheminRapportTrt
    End Sub

    Private Function ValeurParametre(cle As String, valeur As String) As Object
        Dim avant As Object = Nothing
        If original.ParametresRequeteSQL.TryGetValue(cle, avant) AndAlso Convert.ToString(avant) = valeur Then Return avant
        If avant IsNot Nothing Then Return Convert.ChangeType(valeur, avant.GetType())
        Return valeur
    End Function

    Private Sub Actualiser()
        retour.Enabled = pages.SelectedIndex > 0
        suivant.Text = If(pages.SelectedIndex = 2, "Enregistrer le fichier JSON…", "Suivant")
    End Sub

    Private Shared Function Paires(lignes As String()) As Dictionary(Of String, String)
        Dim valeurs As New Dictionary(Of String, String)
        For Each ligne In lignes.Where(Function(s) Not String.IsNullOrWhiteSpace(s))
            Dim champs = ligne.Split({"="c}, 2)
            If champs.Length <> 2 OrElse champs.Any(Function(s) String.IsNullOrWhiteSpace(s)) Then Throw New ArgumentException("Format attendu : clé=valeur.")
            valeurs.Add(champs(0).Trim(), champs(1).Trim())
        Next
        Return valeurs
    End Function

    Private Sub Avancer(sender As Object, args As EventArgs)
        Try
            If nom.Text.Trim() = "" OrElse requete.SelectedIndex < 0 OrElse email.Text.Trim() = "" Then Throw New ArgumentException("Renseignez le nom du batch, la requête et la colonne e-mail.")
            Dim sqlParams = Paires(parametres.Lines)
            If pages.SelectedIndex >= 1 AndAlso (sujet.Text.Trim() = "" OrElse Not File.Exists(modele.Text.Trim())) Then Throw New ArgumentException("Renseignez l’objet et choisissez un modèle HTML existant.")
            Dim substitutions = Paires(balises.Lines)
            If pages.SelectedIndex < 2 Then
                pages.SelectedIndex += 1
                Return
            End If
            Dim fichiers = pieces.Lines.Select(Function(s) s.Trim()).Where(Function(s) s <> "").ToList()
            For Each fichier In fichiers
                If Not File.Exists(fichier) Then Throw New FileNotFoundException("Fichier introuvable : " & fichier)
            Next
            Dim config As New BatchMailConfig With {.NomBatch = nom.Text.Trim(), .Description = description.Text, .NomRequeteSQL = requete.Text, .ColonneEmail = email.Text.Trim(),
                .ParametresRequeteSQL = sqlParams.ToDictionary(Function(p) p.Key, Function(p) ValeurParametre(p.Key,p.Value)),
                .SujetMail = sujet.Text, .CheminTemplateCorpsEmail = modele.Text.Trim(), .CheminsPjRessource = fichiers,
                .RplctSujetMail = Paires(balisesSujet.Lines), .RplctCorpsMail = substitutions, .RplctPJ = Paires(balisesPJ.Lines), .CheminRapportTrt = rapport.Text.Trim()}
            Using dlg As New SaveFileDialog With {.Filter = "Paramétrage JSON|*.json", .FileName = If(fichierOuvert Is Nothing,"BatchCourriers.json",Path.GetFileNameWithoutExtension(fichierOuvert) & "-modifie.json")}
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(config, Formatting.Indented))
            End Using
            DialogResult = DialogResult.OK
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
