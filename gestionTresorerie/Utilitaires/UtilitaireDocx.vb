Imports System.IO
Imports DocumentFormat
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing

Module UtilitaireDocx
    '-----------------------------------------------------------
    '  1) Détermine le suffixe du fichier (_Nom_Prenom ou _docX)
    '-----------------------------------------------------------
    Private _compteur As Integer = 1

    Private Function GenererSuffixe(dr As DataRow) As String
        Dim nom As String = Nothing
        Dim prenom As String = Nothing

        If dr.Table.Columns.Contains("nom") Then nom = dr("nom").ToString()
        If dr.Table.Columns.Contains("prenom") Then prenom = dr("prenom").ToString()

        If Not String.IsNullOrEmpty(nom) AndAlso Not String.IsNullOrEmpty(prenom) Then
            Return "_" & nom & "_" & prenom
        End If

        Dim s = "_doc" & _compteur
        _compteur += 1
        Return s
    End Function

    '-----------------------------------------------------------
    '  2) Construit le chemin du fichier de sortie
    '-----------------------------------------------------------
    Private Function ConstruireCheminSortie(
        cheminModele As String,
        suffix As String
    ) As String

        Dim dossier = Path.GetDirectoryName(cheminModele)
        Dim nomFichier = Path.GetFileNameWithoutExtension(cheminModele)
        Dim ext = Path.GetExtension(cheminModele)

        Return Path.Combine(dossier, nomFichier & suffix & ext)
    End Function

    '-----------------------------------------------------------
    '  3) Reconstitue le texte complet d’un paragraphe
    '-----------------------------------------------------------
    Private Function LireTexteParagraphe(paragraph As OpenXml.Wordprocessing.Paragraph) As String
        Dim runs = paragraph.Descendants(Of OpenXml.Wordprocessing.Run)().ToList()

        Return String.Concat(
        runs.Select(Function(r)
                        Dim t = r.GetFirstChild(Of OpenXml.Wordprocessing.Text)()
                        If t IsNot Nothing Then Return t.Text Else Return ""
                    End Function)
    )
    End Function

    '-----------------------------------------------------------
    '  4) Applique les remplacements key/colName → dr(colName)
    '-----------------------------------------------------------
    Private Function AppliquerRemplacements(
        texte As String,
        rplct As Dictionary(Of String, String),
        dr As DataRow
    ) As String

        Dim newText As String = texte

        For Each kvp In rplct
            Dim search As String = kvp.Key
            Dim colName As String = kvp.Value

            If dr.Table.Columns.Contains(colName) Then
                Dim replaceValue As String = dr(colName).ToString()
                newText = newText.Replace(search, replaceValue)
            End If
        Next

        Return newText
    End Function

    '-----------------------------------------------------------
    '  5) Réinjecte un texte dans un paragraphe en conservant le style du 1er Run
    '-----------------------------------------------------------
    Private Sub InjecterTexteDansParagraphe(
        paragraph As OpenXml.Wordprocessing.Paragraph,
        texte As String
    )

        Dim runs = paragraph.Descendants(Of OpenXml.Wordprocessing.Run)().ToList()

        For Each r In runs
            Dim t = r.GetFirstChild(Of OpenXml.Wordprocessing.Text)()
            If t IsNot Nothing Then t.Text = ""
        Next

        Dim firstRun = runs.First()
        Dim firstText = firstRun.GetFirstChild(Of OpenXml.Wordprocessing.Text)()

        If firstText Is Nothing Then
            firstText = New OpenXml.Wordprocessing.Text()
            firstRun.Append(firstText)
        End If

        firstText.Text = texte
    End Sub

    '-----------------------------------------------------------
    '  *** 6) Version modulairisée de RemplaceTexteDocx  ***
    '-----------------------------------------------------------
    Public Function RemplaceTexteDocx(
        cheminModele As String,
        rplct As Dictionary(Of String, String),
        dr As DataRow
    ) As String

        ' Suffixe = _Nom_Prenom ou _docX
        Dim suffix = GenererSuffixe(dr)

        ' Construction du nouveau nom
        Dim cheminSortie = ConstruireCheminSortie(cheminModele, suffix)

        ' On copie le modèle
        File.Copy(cheminModele, cheminSortie, True)

        ' Ouverture et traitement
        Using doc As WordprocessingDocument = WordprocessingDocument.Open(cheminSortie, True)

            Dim body = doc.MainDocumentPart.Document.Body

            For Each paragraph In body.Descendants(Of OpenXml.Wordprocessing.Paragraph)()

                Dim fullText = LireTexteParagraphe(paragraph)
                Dim newText = AppliquerRemplacements(fullText, rplct, dr)

                If newText <> fullText Then
                    InjecterTexteDansParagraphe(paragraph, newText)
                End If
            Next

            doc.MainDocumentPart.Document.Save()
        End Using

        Return cheminSortie
    End Function

End Module
