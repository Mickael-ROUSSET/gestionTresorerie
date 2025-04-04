Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports gestionTresorerie.My

Public Class FrmPrincipale

    Inherits System.Windows.Forms.Form
    'Create ADO.NET objects.
    Public maConnexionDB As connexionDB
    Public Shared maConn As SqlConnection
    Private maCmd As SqlCommand
    Private monReader As SqlDataReader
    Private results As String

    Public Property Properties As Object

    Private Sub BtnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        Call LectureBase()
    End Sub
    Private Sub FrmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Chargement des listes dans le formulaire
            FrmSaisie.chargeListes(connexionDB.GetInstance.getConnexion)
        Catch ex As Exception
            ' Gestion des erreurs
            MessageBox.Show("Une erreur est survenue lors de l'initialisation : " & ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.GetInstance.ERR("Une erreur est survenue lors de l'initialisation : " & ex.Message)
        End Try
    End Sub

    Private Sub LectureBase()
        Dim monReaderCategorie As SqlDataReader
        Dim monReaderSousCategorie As SqlDataReader
        Dim tabLegendes() As String = Array.Empty(Of String)()
        Dim tabCatégorie() As String = Array.Empty(Of String)()
        Dim tabValeurs() As Decimal = Array.Empty(Of Decimal)()
        Dim myCmdCategorie As SqlCommand
        Dim myCmdSousCategorie As SqlCommand
        Dim i As Integer, iNbCat As Integer
        Dim sNomImage As String
        Dim para As Paragraph

        'Call creeOpenXml.creeDoc(My.Settings.repInstallation & My.Settings.ficBilan)
        'TODO : utiliser lectureProprietes
        Call creeOpenXml.creeDoc(lectureProprietes.GetVariable("ficBilan"))

        Dim document As WordprocessingDocument = WordprocessingDocument.Open(My.Settings.repInstallation & My.Settings.ficBilan, True)
        Dim styleDefinitionsPart As StyleDefinitionsPart = creeOpenXml.AddStylesPartToPackage(document)
        Call creeOpenXml.CreateAndAddParagraphStyle(StyleDefinitionsPart, "monStyle", "monStyle")
        myCmdCategorie = New SqlCommand("SELECT distinct catégorie FROM Mouvements ;", maConn)
        monReaderCategorie = myCmdCategorie.ExecuteReader()
        Do While monReaderCategorie.Read()
            ReDim Preserve tabCatégorie(UBound(tabCatégorie) + 1)
            tabCatégorie(i) = monReaderCategorie.GetSqlString(0)
            i += 1
        Loop
        monReaderCategorie.Close()

        For iNbCat = 0 To UBound(tabCatégorie)
            ReDim tabLegendes(0)
            ReDim tabValeurs(0)
            myCmdSousCategorie = New SqlCommand("SELECT sousCatégorie, sum(montant) FROM Mouvements where catégorie = '" & tabCatégorie(iNbCat) & "' group by sousCatégorie order by sum(montant) desc;", maConn)
            Call creeOpenXml.AddSectionBreakToTheDocument(document)
            para = creeOpenXml.ajouteParagraphe(document, tabCatégorie(iNbCat))
            Call ApplyStyleToParagraph(document, "monStyle", "monStyle", para)
            monReaderSousCategorie = myCmdSousCategorie.ExecuteReader()
            i = 0
            'Supprime tous les controls de la fenêtre (=> les images précédentes)
            'Call SupprimeControlesfenetre(frmHistogramme)
            Do While monReaderSousCategorie.Read()
                Try
                    ReDim Preserve tabLegendes(UBound(tabLegendes) + 1)
                    tabLegendes(i) = CStr(monReaderSousCategorie.GetSqlString(0))
                    ReDim Preserve tabValeurs(UBound(tabValeurs) + 1)
                    tabValeurs(i) = monReaderSousCategorie.GetDecimal(1)
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
                i += 1
            Loop
            monReaderSousCategorie.Close()
            Call frmHistogramme.creeChart("Montants par sous-catégorie : " & tabCatégorie(iNbCat), tabValeurs, tabLegendes)
            frmHistogramme.Show()
            sNomImage = "C:\Users\User\Downloads\frmHistogramme" & iNbCat & ".png"
            'URL : https://stackoverflow.com/questions/37825662/how-to-save-the-whole-windows-form-as-image-vb-net
            Using bmp = New Bitmap(frmHistogramme.Width, frmHistogramme.Height)
                frmHistogramme.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                'Supprime l'image si elle existe déjà
                Try
                    My.Computer.FileSystem.DeleteFile(sNomImage, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing)
                Catch
                    'Ne fait rien quand le fichier à supprimer n'existe pas
                End Try
                bmp.Save(sNomImage)
            End Using
            Call creeOpenXml.ajouteImage(document, sNomImage)

            'Création du tableau
            Dim data(1, UBound(tabValeurs) - 1) As String
            For j As Integer = 0 To UBound(tabValeurs) - 1
                data(0, j) = tabLegendes(j)
                data(1, j) = tabValeurs(j)
            Next j
            para = creeOpenXml.ajouteParagraphe(document, vbCrLf)
            Call creeOpenXml.ajouteTableau(document, data)
        Next iNbCat

        monReaderCategorie.Close()
        document.Save()
        document.Dispose()
    End Sub
    Private Sub FrmMain_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Try
            Call SuprimeConnexion()
        Catch
        End Try
    End Sub
    Private Sub CreeConnexion()
        'Open the connection.
        maConn.Open()
    End Sub
    Private Sub SuprimeConnexion()
        'Close the reader and the database connection. 
        Try
            maConn.Close()
        Catch ex As Exception
            Debug.Print(ex.Message)
        End Try
    End Sub
    Private Sub BtnSaisie_Click(sender As Object, e As EventArgs) Handles btnSaisie.Click
        FrmSaisie.Show()
    End Sub

    Private Sub BtnChargeRelevé_Click(sender As Object, e As EventArgs) Handles btnChargeRelevé.Click
        FrmChargeRelevé.Show()
    End Sub

    Private Sub BtnConsultation_Click(sender As Object, e As EventArgs) Handles btnConsultation.Click
        ' Evite de définir la chaine de connexion à chaque endroit où tu l'utilises : si tu dois la changer,
        ' ça fait autant d'endroits à modifier, et ça force à recompiler. Il vaut mieux la définir dans les
        ' paramètres de l'application, comme ça si tu dois la changer tu n'auras qu'un seul endroit à modifier.

        ' Essaie de taper une apostrophe (') dans TextBox1, et observe le résultat ;)
        ' Ensuite, va faire un tour ici pour apprendre à régler le problème :
        ' http://johannblais.developpez.com/tutoriel/dotnet/bonnes-pratiques-acces-donnees/#LIV
        Dim command As New System.Data.SqlClient.SqlCommand("SELECT * FROM Mouvements", maConn)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connection dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
            adpt.Fill(dt)
            DataGridView1.DataSource = dt
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("Une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        Finally
            ' Le code du bloc Finally est toujours exécuté, même en cas d'erreur dans le Try
            ' On y place donc la fermeture de la connection :
            'myConn.Close()
        End Try
    End Sub

    Private Sub FermerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FermerToolStripMenuItem.Click
        Me.Close()
        End
    End Sub

    Private Sub btnCreeBilans_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
        'creeOpenXml.Main()
        MsgBox("fonction désactivée")
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        Dim dateMvt As Date
        Dim montant As Double
        Dim sens As Boolean
        Dim note As String
        Dim bExiste As Boolean
        Dim categorie As String
        Dim sousCategorie As String
        Dim tiers As String
        Dim rapproche As String
        Dim evenement As String
        Dim monType As String
        Dim remise As String

        With Me.DataGridView1.CurrentRow.Cells
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
        End With
        bExiste = Mouvements.existe(dateMvt, montant, sens)
        With FrmSaisie
            .chargeListes(maConn)
            .dateMvt.Value = dateMvt
            .txtNote.Text = note
            .rbDebit.Checked = sens
            .txtMontant.Text = montant
            .txtRemise.Text = remise
            .rbRapproche.Text = rapproche
            'End If
            .Show()
        End With
    End Sub

    Private Sub btnBatch_Click(sender As Object, e As EventArgs) Handles btnBatch.Click
        Dim batch As New batchAnalyseChq

        Call batch.ParcourirRepertoireEtAnalyser()
    End Sub

    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
    '    'Call CreeBilans()
    '    'Call genereBilans.AjouteImage()
    '    Call genereBilans.GenereBilanStructure()
    'End Sub
End Class