Imports System.Text.RegularExpressions
Imports IronOcr

Module ChequeOCR2
    Public Sub litImage()
        ' Charger l'image du chèque
        'Dim imgPath As String = "C:\Users\User\Downloads\IMG_20250224_214706.jpg"
        'Dim imgPath As String = "C:\Users\User\Downloads\chq.png"
        Dim imgPath As String = "C:\Users\User\Downloads\SKM_C25825022819230.tif"


        ' Initialiser IronOCR avec un OCR optimisé pour documents
        Dim Ocr As New IronTesseract()
        Ocr.Language = OcrLanguage.English
        'Ocr.Language = OcrLanguage.french
        'MsgBox("Langues disponibles : " & String.Join(", ", Ocr.Language))

        ' Exécuter l'OCR
        Dim Result As OcrResult
        Using Input As New OcrInput()
            Input.LoadImage(imgPath)
            'Input.Deskew()
            'Input.ToGrayScale()
            'Input.DeNoise()
            Input.Sharpen()
            'Input.EnhanceResolution()
            'Input.Contrast(1.5)
            'Input.Invert()
            Input.Binarize()
            Result = Ocr.Read(Input)
        End Using

        ' Extraction des informations du chèque
        Dim texteCheque As String = Result.Text
        MsgBox("Texte extrait : " & texteCheque)

        ' Extraire des valeurs spécifiques (Montant, Date, Émetteur)
        Dim montant As String = ExtraireMontant(texteCheque)
        Dim dateCheque As String = ExtraireDate(texteCheque)
        Dim emetteur As String = ExtraireEmetteur(texteCheque)

        ' Affichage des résultats
        MsgBox("Montant: " & montant)
        MsgBox("Date: " & dateCheque)
        MsgBox("Émetteur: " & emetteur)
    End Sub

    ' Fonction pour extraire le montant en euros
    Function ExtraireMontant(texte As String) As String
        Dim regexMontant As New Regex("\b\d{1,3}(?:[.,]\d{2})?\b")
        Dim match As System.Text.RegularExpressions.Match = regexMontant.Match(texte)
        Return If(match.Success, match.Value, "Non trouvé")
    End Function

    ' Fonction pour extraire une date (format DD/MM/YYYY ou DD.MM.YYYY)
    Function ExtraireDate(texte As String) As String
        Dim regexDate As New System.Text.RegularExpressions.Regex("\d{1,2}[\/\.]\d{1,2}[\/\.]\d{2,4}")
        Dim match As System.Text.RegularExpressions.Match = regexDate.Match(texte)
        Return If(match.Success, match.Value, "Non trouvé")
    End Function

    ' Fonction pour extraire le nom de l’émetteur (première ligne)
    Function ExtraireEmetteur(texte As String) As String
        Dim lignes As String() = texte.Split(vbCrLf)
        Return If(lignes.Length > 0, lignes(0), "Non trouvé")
    End Function

End Module
