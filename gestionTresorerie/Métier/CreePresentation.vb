Imports DocumentFormat.OpenXml.Drawing.Charts
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports SixLabors.ImageSharp.PixelFormats
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

    Private Shared Sub ProcessCategory(document As WordprocessingDocument, category As String)
        Dim subCategories As List(Of (Legend As String, Value As Decimal)) = GetSubCategories(category)

        'Récupérer le libellé de la catégorie
        Dim para As Paragraph = CreeOpenXml.ajouteParagraphe(document, "Focus sur la catégorie : " & Categorie.libelleParId(category))
        ApplyStyleToParagraph(document, "Titre1", "Titre1", para)

        If subCategories.Count <> 0 Then
            CreateChartAndAddToDocument(document, category, subCategories)
            CreateTableAndAddToDocument(document, subCategories)
        End If

        ' Ajouter un saut de ligne avant para2
        Dim lineBreak As New Paragraph(New Run(New Break()))
        document.MainDocumentPart.Document.Body.AppendChild(lineBreak)

        Dim paraIA As Paragraph = CreeOpenXml.ajouteParagraphe(document, AppelMistral.questionMistral(creeQuestionIA(category, subCategories)))
        formateParagraphe(paraIA, "Arial", 12)

        ' Ajouter un saut de page
        Dim pageBreak As New Break() With {.Type = BreakValues.Page}
        document.MainDocumentPart.Document.Body.AppendChild(pageBreak)
    End Sub
    Private Shared Sub formateParagraphe(para As Paragraph, sPolice As String, iTaille As Integer)
        For Each run As Run In para.Elements(Of Run)()
            run.RunProperties = New RunProperties(New RunFonts() With {
            .Ascii = sPolice
        }, New FontSize() With {
            .Val = Str(2 * iTaille) ' La taille est spécifiée en demi-points, donc 24 demi-points = 12 points
        })
        Next
    End Sub
    Private Shared Function creeQuestionIA(iCategorie As Integer, subCategories As List(Of (Legend As String, Value As Decimal))) As String
        ' Concaténer les éléments de subCategories sous la forme <Legend> : <montant>
        Dim subCategoriesString As String = String.Join(", ", subCategories.Select(Function(subCat) $"{subCat.Legend} : {subCat.Value}"))

        ' Construire la question
        Return $"Peux-tu me donner un résumé de la catégorie {Categorie.libelleParId(iCategorie)} : dont la répartition des montants par sous-catégorie est {subCategoriesString} ? N'invente pas de valeurs non présentes dans la liste. Renvoie chaque phrase à la ligne"
    End Function

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
        Logger.WARN($"Aucune sous-catégorie trouvée pour la catégorie '{Category}'.")
        End If
        End Using

        Return subcategories
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
