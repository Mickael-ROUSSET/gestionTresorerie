Imports IronOcr
Imports System.Text.RegularExpressions
Imports System.Drawing

Module ChequeOCR
    Sub litImage()
        ' Charger l'image du chèque
        Dim imgPath As String = "C:\Users\User\Downloads\SKM_C25825022819230.tif"

        ' Initialiser IronOCR avec un OCR optimisé
        Dim Ocr As New IronTesseract()
        'Ocr.Language = OcrLanguage.French
        Ocr.Configuration.ReadBarCodes = False

        ' Exécuter l'OCR en spécifiant une zone pour le montant (haut droit du chèque)
        Dim Result As OcrResult
        Using Input As New OcrInput()
            Input.LoadImage(imgPath)
            Input.Deskew()  ' Correction d'angle
            Input.ToGrayScale()
            Input.DeNoise()
            Input.Sharpen()
            Input.Contrast(1.5) ' Augmente le contraste
            Input.EnhanceResolution(300) ' Augmente la résolution pour OCR
            Result = Ocr.Read(Input)
        End Using

        ' Définir les zones spécifiques pour le montant et le numéro du chèque
        Dim montantZone As New Rectangle(400, 50, 200, 100) ' Modifier selon l'image
        ' Add image
        Dim imageInput = New OcrImageInput(imgPath, ContentArea:=montantZone)
        ' Perform OCR
        Dim OcrResult As OcrResult = Ocr.Read(imageInput)
        ' Output the result to console
        MsgBox(OcrResult.Text)

        Dim numeroChequeZone As New Rectangle(50, 300, 200, 50) ' Modifier selon l'image
        ' Add image
        Dim imageNumeroCheque = New OcrImageInput(imgPath, ContentArea:=numeroChequeZone)
        ' Perform OCR
        OcrResult = Ocr.Read(imageNumeroCheque)
        ' Output the result to console
        MsgBox(OcrResult.Text)


        ' Extraction du texte complet
        Dim texteCheque As String = Result.Text
        MsgBox("Texte extrait : " & texteCheque)

        ' Extraire le montant en ciblant une zone spécifique
        Dim montant As String = ExtraireMontant(texteCheque, montantZone)
        MsgBox("Montant détecté : " & montant)

        ' Extraire le numéro du chèque en ciblant une zone spécifique
        Dim numeroCheque As String = ExtraireNumeroCheque(texteCheque, numeroChequeZone)
        MsgBox("Numéro de chèque détecté : " & numeroCheque)
    End Sub

    ' Fonction pour extraire le montant en euros avec OCR optimisé pour chiffres
    Function ExtraireMontant(texte As String, zone As Rectangle) As String
        Dim regexMontant As New Regex("\b\d{1,3}(?:[.,]\d{2})?\s?[€EUR]\b")
        Dim match As Match = regexMontant.Match(texte)
        Return If(match.Success, match.Value, "Montant non trouvé")
    End Function

    ' Fonction pour extraire le numéro du chèque en ciblant une zone spécifique (bas gauche)
    Function ExtraireNumeroCheque(texte As String, zone As Rectangle) As String
        Return texte.Trim()
    End Function

End Module
