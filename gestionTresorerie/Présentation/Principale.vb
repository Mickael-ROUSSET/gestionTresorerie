Imports System.Data.SqlClient
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class FrmPrincipale

    Inherits System.Windows.Forms.Form

    Public Property Properties As Object

    Private Sub BtnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        Call LectureBase()
    End Sub
    Private Sub FrmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            'Charger dgvPrincipale avec le contenu de la table mouvements
            Call ChargerDgvPrincipale()
            ' Chargement des listes dans le formulaire
            FrmSaisie.chargeListes()
        Catch ex As Exception
            ' Gestion des erreurs
            MessageBox.Show("Une erreur est survenue lors de l'initialisation : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.GetInstance.ERR("Une erreur est survenue lors de l'initialisation : " & ex.Message)
        End Try
    End Sub
    Private Sub ChargerDgvPrincipale()
        Try
            ' Définir la requête SQL pour récupérer les données
            Dim query As String = lectureProprietes.GetVariable("sqlSelectMouvementsLibelles")

            ' Utiliser une connexion à la base de données
            Dim conn As SqlConnection = connexionDB.GetInstance.getConnexion
            ' Créer une commande SQL
            Using cmd As New SqlCommand(query, conn)
                ' Créer un DataAdapter pour remplir le DataTable
                Using adapter As New SqlDataAdapter(cmd)
                    ' Créer un DataTable pour stocker les données
                    Dim dataTable As New DataTable()
                    ' Remplir le DataTable avec les données de la base de données
                    adapter.Fill(dataTable)
                    ' Lier le DataTable au DataGridView
                    dgvPrincipale.DataSource = dataTable
                End Using
            End Using
            '' Forcer le rafraîchissement du DataGridView
            'dgvPrincipale.Refresh()
            ' Ajouter la colonne d'image pour l'état du mouvement
            AjouterColonneEtatImage()

            ' Écrire un log d'information
            Logger.GetInstance.INFO("Chargement des données dans dgvPrincipale réussi.")
        Catch ex As SqlException
            ' Écrire un log d'erreur en cas d'exception SQL
            Logger.GetInstance.ERR($"Erreur SQL lors du chargement des données dans dgvPrincipale : {ex.Message}")
        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception générale
            Logger.GetInstance.ERR($"Erreur lors du chargement des données dans dgvPrincipale : {ex.Message}")
        End Try
    End Sub

    Private Sub AjouterColonneEtatImage()
        ' Ajouter une colonne d'image pour l'état
        Dim etatImageColumn As New DataGridViewImageColumn()
        etatImageColumn.Name = "etatImage"
        etatImageColumn.HeaderText = "État"
        dgvPrincipale.Columns.Add(etatImageColumn)

        ' Parcourir les lignes du DataGridView pour définir les images
        For Each row As DataGridViewRow In dgvPrincipale.Rows
            If Not row.IsNewRow Then
                Try
                    '8 correspond à la colonne "etat"
                    Dim etat As Object = row.Cells(8).Value
                    If etat IsNot Nothing AndAlso TypeOf etat Is Boolean Then
                        row.Cells("etatImage").Value = If(CType(etat, Boolean), My.Resources.OK, My.Resources.KO)
                    Else
                        Logger.GetInstance.ERR($"Valeur invalide pour la colonne 'etat' dans la ligne {row.Index}: {etat}")
                        row.Cells("etatImage").Value = My.Resources.KO ' Par défaut, si la valeur est invalide
                    End If
                Catch ex As Exception
                    Logger.GetInstance.ERR($"Erreur lors de la définition de l'image pour la colonne 'etat' dans la ligne {row.Index}: {ex.Message}")
                    row.Cells("etatImage").Value = My.Resources.KO ' Par défaut, en cas d'erreur
                End Try
            End If
        Next
    End Sub

    'Private Sub chargeMouvements()
    '    ' Evite de définir la chaine de connexion à chaque endroit où tu l'utilises : si tu dois la changer,
    '    ' ça fait autant d'endroits à modifier, et ça force à recompiler. Il vaut mieux la définir dans les
    '    ' paramètres de l'application, comme ça si tu dois la changer tu n'auras qu'un seul endroit à modifier.

    '    ' Essaie de taper une apostrophe (') dans TextBox1, et observe le résultat ;)
    '    ' Ensuite, va faire un tour ici pour apprendre à régler le problème :
    '    ' http://johannblais.developpez.com/tutoriel/dotnet/bonnes-pratiques-acces-donnees/#LIV
    '    'Dim command As New System.Data.SqlClient.SqlCommand("SELECT * FROM Mouvements", maConn)
    '    Dim command As New System.Data.SqlClient.SqlCommand("SELECT * FROM Mouvements", connexionDB.GetInstance.getConnexion)

    '    Dim dt As New DataTable
    '    Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

    '    Try
    '        ' Place la connexion dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
    '        adpt.Fill(dt)
    '        dgvPrincipale.DataSource = dt

    '        ' Ajouter la colonne d'image pour l'état du mouvement
    '        AjouterColonneEtatImage()
    '    Catch ex As SqlException
    '        ' On informe l'utilisateur qu'il y a eu un problème :
    '        MessageBox.Show("Une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
    '    Finally
    '        ' Le code du bloc Finally est toujours exécuté, même en cas d'erreur dans le Try 
    '    End Try
    'End Sub

    'Private Sub LectureBase()
    '    Dim monReaderCategorie As SqlDataReader
    '    Dim monReaderSousCategorie As SqlDataReader
    '    Dim tabLegendes() As String = Array.Empty(Of String)()
    '    Dim tabCatégorie() As String = Array.Empty(Of String)()
    '    Dim tabValeurs() As Decimal = Array.Empty(Of Decimal)()
    '    Dim myCmdCategorie As SqlCommand
    '    Dim myCmdSousCategorie As SqlCommand
    '    Dim i As Integer, iNbCat As Integer
    '    Dim sNomImage As String
    '    Dim para As Paragraph

    '    Call creeOpenXml.creeDoc(lectureProprietes.GetVariable("ficBilan"))

    '    'Dim document As WordprocessingDocument = WordprocessingDocument.Open(My.Settings.repInstallation & My.Settings.ficBilan, True)
    '    Dim document As WordprocessingDocument = WordprocessingDocument.Open(lectureProprietes.GetVariable("ficBilan"), True)
    '    Dim styleDefinitionsPart As StyleDefinitionsPart = creeOpenXml.AddStylesPartToPackage(document)
    '    Call creeOpenXml.CreateAndAddParagraphStyle(StyleDefinitionsPart, "monStyle", "monStyle")
    '    'myCmdCategorie = New SqlCommand("SELECT distinct catégorie FROM Mouvements ;", maConn)
    '    myCmdCategorie = New SqlCommand("SELECT distinct catégorie FROM Mouvements ;", connexionDB.GetInstance.getConnexion)
    '    monReaderCategorie = myCmdCategorie.ExecuteReader()
    '    Do While monReaderCategorie.Read()
    '        ReDim Preserve tabCatégorie(UBound(tabCatégorie) + 1)
    '        tabCatégorie(i) = monReaderCategorie.GetSqlString(0)
    '        i += 1
    '    Loop
    '    monReaderCategorie.Close()

    '    For iNbCat = 0 To UBound(tabCatégorie)
    '        ReDim tabLegendes(0)
    '        ReDim tabValeurs(0)
    '        'myCmdSousCategorie = New SqlCommand("SELECT sousCatégorie, sum(montant) FROM Mouvements where catégorie = '" & tabCatégorie(iNbCat) & "' group by sousCatégorie order by sum(montant) desc;", maConn)
    '        myCmdSousCategorie = New SqlCommand("SELECT sousCatégorie, sum(montant) FROM Mouvements where catégorie = '" & tabCatégorie(iNbCat) & "' group by sousCatégorie order by sum(montant) desc;", connexionDB.GetInstance.getConnexion)
    '        Call creeOpenXml.AddSectionBreakToTheDocument(document)
    '        para = creeOpenXml.ajouteParagraphe(document, tabCatégorie(iNbCat))
    '        Call ApplyStyleToParagraph(document, "monStyle", "monStyle", para)
    '        monReaderSousCategorie = myCmdSousCategorie.ExecuteReader()
    '        i = 0

    '        Do While monReaderSousCategorie.Read()
    '            Try
    '                ReDim Preserve tabLegendes(UBound(tabLegendes) + 1)
    '                tabLegendes(i) = CStr(monReaderSousCategorie.GetSqlString(0))
    '                ReDim Preserve tabValeurs(UBound(tabValeurs) + 1)
    '                tabValeurs(i) = monReaderSousCategorie.GetDecimal(1)
    '            Catch ex As Exception
    '                MsgBox(ex.Message)
    '            End Try
    '            i += 1
    '        Loop
    '        monReaderSousCategorie.Close()
    '        Call frmHistogramme.creeChart("Montants par sous-catégorie : " & tabCatégorie(iNbCat), tabValeurs, tabLegendes)
    '        frmHistogramme.Show()
    '        'sNomImage = "C:\Users\User\Downloads\frmHistogramme" & iNbCat & ".png"
    '        sNomImage = lectureProprietes.GetVariable("repFichierBilan") & "frmHistogramme" & iNbCat & ".png"
    '        'URL : https://stackoverflow.com/questions/37825662/how-to-save-the-whole-windows-form-as-image-vb-net
    '        Using bmp = New Bitmap(frmHistogramme.Width, frmHistogramme.Height)
    '            frmHistogramme.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
    '            'Supprime l'image si elle existe déjà
    '            Try
    '                My.Computer.FileSystem.DeleteFile(sNomImage, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing)
    '            Catch
    '                'Ne fait rien quand le fichier à supprimer n'existe pas
    '            End Try
    '            bmp.Save(sNomImage)
    '        End Using
    '        Call creeOpenXml.ajouteImage(document, sNomImage)

    '        'Création du tableau
    '        Dim data(1, UBound(tabValeurs) - 1) As String
    '        For j As Integer = 0 To UBound(tabValeurs) - 1
    '            data(0, j) = tabLegendes(j)
    '            data(1, j) = tabValeurs(j)
    '        Next j
    '        para = creeOpenXml.ajouteParagraphe(document, vbCrLf)
    '        Call creeOpenXml.ajouteTableau(document, data)
    '    Next iNbCat

    '    monReaderCategorie.Close()
    '    document.Save()
    '    document.Dispose()
    'End Sub

    Private Sub LectureBase()
        Dim document As WordprocessingDocument = OpenDocument()
        Dim categories As List(Of String) = GetCategories()

        For Each category As String In categories
            ProcessCategory(document, category)
        Next

        document.Save()
        document.Dispose()
    End Sub

    Private Function OpenDocument() As WordprocessingDocument
        creeOpenXml.creeDoc(lectureProprietes.GetVariable("ficBilan"))
        Dim document As WordprocessingDocument = WordprocessingDocument.Open(lectureProprietes.GetVariable("ficBilan"), True)
        Dim styleDefinitionsPart As StyleDefinitionsPart = creeOpenXml.AddStylesPartToPackage(document)
        creeOpenXml.CreateAndAddParagraphStyle(styleDefinitionsPart, "monStyle", "monStyle")
        Return document
    End Function

    Private Function GetCategories() As List(Of String)
        Dim categories As New List(Of String)
        Dim cmd As New SqlCommand("SELECT DISTINCT catégorie FROM Mouvements;", connexionDB.GetInstance.getConnexion)
        Using reader As SqlDataReader = cmd.ExecuteReader()
            While reader.Read()
                categories.Add(reader.GetSqlString(0))
            End While
        End Using
        Return categories
    End Function

    Private Sub ProcessCategory(document As WordprocessingDocument, category As String)
        Dim subCategories As List(Of (Legend As String, Value As Decimal)) = GetSubCategories(category)
        Dim para As Paragraph = creeOpenXml.ajouteParagraphe(document, category)
        ApplyStyleToParagraph(document, "monStyle", "monStyle", para)

        If subCategories.Count <> 0 Then
            CreateChartAndAddToDocument(document, category, subCategories)
            CreateTableAndAddToDocument(document, subCategories)
        End If
    End Sub

    Private Function GetSubCategories(category As String) As List(Of (Legend As String, Value As Decimal))
        Dim subCategories As New List(Of (Legend As String, Value As Decimal))
        Dim cmd As New SqlCommand("SELECT sousCatégorie, SUM(montant) FROM Mouvements WHERE catégorie = @category GROUP BY sousCatégorie ORDER BY SUM(montant) DESC;", connexionDB.GetInstance.getConnexion)
        cmd.Parameters.AddWithValue("@category", category)
        Using reader As SqlDataReader = cmd.ExecuteReader()
            While reader.Read()
                subCategories.Add((reader.GetSqlString(0), reader.GetDecimal(1)))
            End While
        End Using
        Return subCategories
    End Function

    Private Sub CreateChartAndAddToDocument(document As WordprocessingDocument, category As String, subCategories As List(Of (Legend As String, Value As Decimal)))
        Dim legends As String() = subCategories.Select(Function(sc) sc.Legend).ToArray()
        Dim values As Decimal() = subCategories.Select(Function(sc) sc.Value).ToArray()

        frmHistogramme.creeChart($"Montants par sous-catégorie : {category}", values, legends)
        frmHistogramme.Show()

        Dim imagePath As String = $"{lectureProprietes.GetVariable("repFichierBilan")}frmHistogramme{category}.png"
        SaveFormAsImage(frmHistogramme, imagePath)
        creeOpenXml.ajouteImage(document, imagePath)
    End Sub

    Private Sub SaveFormAsImage(form As Form, imagePath As String)
        Using bmp As New Bitmap(form.Width, form.Height)
            form.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
            Try
                My.Computer.FileSystem.DeleteFile(imagePath, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently, FileIO.UICancelOption.DoNothing)
            Catch
                ' Ne fait rien quand le fichier à supprimer n'existe pas
            End Try
            bmp.Save(imagePath)
        End Using
    End Sub

    Private Sub CreateTableAndAddToDocument(document As WordprocessingDocument, subCategories As List(Of (Legend As String, Value As Decimal)))
        Dim data(1, subCategories.Count - 1) As String
        For i As Integer = 0 To subCategories.Count - 1
            data(0, i) = subCategories(i).Legend
            data(1, i) = subCategories(i).Value.ToString()
        Next

        Dim para As Paragraph = creeOpenXml.ajouteParagraphe(document, vbCrLf)
        creeOpenXml.ajouteTableau(document, data)
    End Sub

    Private Sub FrmMain_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        connexionDB.GetInstance.Dispose()
    End Sub
    Private Sub BtnSaisie_Click(sender As Object, e As EventArgs) Handles btnSaisie.Click
        FrmSaisie.Show()
    End Sub

    Private Sub BtnChargeRelevé_Click(sender As Object, e As EventArgs) Handles btnChargeRelevé.Click
        FrmChargeRelevé.Show()
    End Sub

    Private Sub BtnConsultation_Click(sender As Object, e As EventArgs) Handles btnConsultation.Click
        Call ChargerDgvPrincipale()
    End Sub

    Private Sub FermerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FermerToolStripMenuItem.Click
        Me.Close()
        End
    End Sub

    Private Sub btnCreeBilans_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
        'creeOpenXml.Main()
        MsgBox("fonction désactivée")
    End Sub

    Private Sub dgvPrincipale_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPrincipale.CellContentClick
        Dim dateMvt As Date
        Dim montant As Double
        Dim sens As Boolean, etat As Boolean
        Dim note As String, categorie As String, sousCategorie As String
        Dim tiers As String, rapproche As String, evenement As String, monType As String, remise As String

        With Me.dgvPrincipale.CurrentRow.Cells
            note = .Item(1).Value
            dateMvt = CDate(.Item(5).Value)
            montant = .Item(6).Value
            sens = .Item(7).Value
            categorie = .Item(2).Value
            sousCategorie = .Item(3).Value
            tiers = .Item(4).Value
            rapproche = .Item(8).Value
            evenement = .Item(9).Value
            monType = .Item(10).Value
            remise = .Item(12).Value
            etat = .Item(11).Value
        End With
        With FrmSaisie
            .chargeListes()
            .dateMvt.Value = dateMvt
            .txtNote.Text = note
            .rbDebit.Checked = sens
            .txtMontant.Text = montant
            .txtRemise.Text = remise
            .rbRapproche.Text = rapproche
            .Show()
        End With
    End Sub

    Private Sub btnBatch_Click(sender As Object, e As EventArgs) Handles btnBatch.Click
        Dim batch As New batchAnalyseChq

        Call batch.ParcourirRepertoireEtAnalyser()
    End Sub

    Private Sub btnTraiteRelevé_Click(sender As Object, e As EventArgs) Handles btnTraiteRelevé.Click
        Call FrmChargeRelevé.AlimenteLstMvtCA(lectureProprietes.GetVariable("ficRelevéTraité"))
        FrmChargeRelevé.Show()
    End Sub

    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
    '    'Call CreeBilans()
    '    'Call genereBilans.AjouteImage()
    '    Call genereBilans.GenereBilanStructure()
    'End Sub
End Class