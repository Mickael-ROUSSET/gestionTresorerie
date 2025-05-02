Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports System.Data.SqlClient

Public Class CreePresentation
    Public Shared Sub LectureBase()
        Dim sFicBilan As String = LectureProprietes.GetCheminEtVariable("ficBilan")
        Dim document As WordprocessingDocument = OpenDocument(sFicBilan)
        Dim categories As List(Of Integer) = GetCategories()

        For Each category As String In categories
            ProcessCategory(document, category)
        Next
        document.Save()
        document.Dispose()
        MsgBox($"Génération du fichier bilan : {sFicBilan} terminée")
        Logger.INFO($"Génération du fichier bilan : {sFicBilan} terminée")
    End Sub

    Private Shared Function OpenDocument(sFicBilan As String) As WordprocessingDocument
        CreeOpenXml.creeDoc(sFicBilan)
        Dim document As WordprocessingDocument = WordprocessingDocument.Open(sFicBilan, True)
        Dim styleDefinitionsPart As StyleDefinitionsPart = CreeOpenXml.AddStylesPartToPackage(document)
        CreeOpenXml.CreateAndAddParagraphStyle(styleDefinitionsPart, "monStyle", "monStyle")
        Return document
    End Function

    Private Shared Function GetCategories() As List(Of Integer)
        Dim categories As New List(Of Integer)

        Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("reqCategoriesMouvements").ExecuteReader()
            While reader.Read()
                categories.Add(reader.GetSqlInt32(0))
            End While
        End Using
        Return categories
    End Function

    Private Shared Async Sub ProcessCategory(document As WordprocessingDocument, category As String)
        Dim subCategories As List(Of (Legend As String, Value As Decimal)) = GetSubCategories(category)

        'Récupérer le libellé de la catégorie
        Dim para As Paragraph = CreeOpenXml.ajouteParagraphe(document, Categorie.libelleParId(category))
        'ApplyStyleToParagraph(document, "monStyle", "monStyle", para)
        ApplyStyleToParagraph(document, "Titre1", "Titre1", para)

        If subCategories.Count <> 0 Then
            CreateChartAndAddToDocument(document, category, subCategories)
            CreateTableAndAddToDocument(document, subCategories)
        End If

        Dim sQuestionIA As String = $"Peux-tu me donner un résumé de la catégorie {category} ?"

        ' Appeler la méthode asynchrone et attendre le résultat
        Dim sAnalyse As String = AppelMistral.questionMistral(LectureProprietes.GetVariable("urlMistral"), sQuestionIA, LectureProprietes.GetVariable("cleApiMistral"))
        'Dim sAnalyse = Await sAnalyseT
        para = CreeOpenXml.ajouteParagraphe(document, sAnalyse)
        ApplyStyleToParagraph(document, "monStyle", "monStyle", para)

        ' Ajouter un saut de page
        Dim pageBreak As New Break() With {.Type = BreakValues.Page}
        document.MainDocumentPart.Document.Body.AppendChild(pageBreak)
        'para = CreeOpenXml.ajouteParagraphe(document, Categorie.libelleParId(category))
    End Sub

    Private Shared Function GetSubCategories(category As String) As List(Of (Legend As String, Value As Decimal))
        Dim subCategories As New List(Of (Legend As String, Value As Decimal))

        Using reader As SqlDataReader =
            SqlCommandBuilder.
            CreateSqlCommand("reqSommeCatMouvements",
                             New Dictionary(Of String, Object) From {{"@categorie", category}}
                             ).
            ExecuteReader()

            ' Vérifier si le reader contient des lignes
            If reader.HasRows Then
                While reader.Read()
                    subCategories.Add((reader.GetString(0), reader.GetDecimal(1)))
                End While
                Logger.INFO($"Sous-catégories chargées pour la catégorie '{category}'.")
            Else
                ' Gérer le cas où le reader est vide
                Logger.WARN($"Aucune sous-catégorie trouvée pour la catégorie '{category}'.")
            End If
        End Using

        Return subCategories
    End Function

    Private Shared Sub CreateChartAndAddToDocument(document As WordprocessingDocument, category As String, subCategories As List(Of (Legend As String, Value As Decimal)))
        Dim legends As String() = subCategories.Select(Function(sc) sc.Legend).ToArray()
        Dim values As Decimal() = subCategories.Select(Function(sc) sc.Value).ToArray()

        FrmHistogramme.creeChart($"Montants par sous-catégorie : {category}", values, legends)
        FrmHistogramme.Show()

        Dim imagePath As String = $"{LectureProprietes.GetCheminEtVariable("repFichierBilan")}frmHistogramme{category}.png"
        SaveFormAsImage(FrmHistogramme, imagePath)

        CreeOpenXml.ajouteImage(document, imagePath)
    End Sub

    Private Shared Sub SaveFormAsImage(form As Form, imagePath As String)
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

    Private Shared Sub CreateTableAndAddToDocument(document As WordprocessingDocument, subCategories As List(Of (Legend As String, Value As Decimal)))
        Dim data(1, subCategories.Count - 1) As String
        For i As Integer = 0 To subCategories.Count - 1
            data(0, i) = subCategories(i).Legend
            data(1, i) = subCategories(i).Value.ToString()
        Next

        Dim para As Paragraph = CreeOpenXml.ajouteParagraphe(document, vbCrLf)
        CreeOpenXml.ajouteTableau(document, data)
    End Sub
End Class
