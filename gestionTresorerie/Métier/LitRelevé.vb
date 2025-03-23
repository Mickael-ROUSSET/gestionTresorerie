Imports System.Security
Imports System.IO
Imports System.Text.RegularExpressions
Module LitRelevé

    Dim WithEvents SelectButton As Button
    ReadOnly openFileDialog1 As OpenFileDialog

    Sub New()
        openFileDialog1 = New OpenFileDialog() With
        {
           .FileName = "Sélectionner un fichier csv",
           .Filter = "Text files (*.csv)|*.csv",
           .Title = "Open csv file"
        }

        SelectButton = New Button() With {.Text = "Select file"}
    End Sub

    Public Function OuvreFichier() As String
        Dim sFicTemp As String = Path.GetTempPath() & "test.txt"
        'Un seul fichier peut être sélectionné
        openFileDialog1.Multiselect = False
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
            Try
                Dim filePath = openFileDialog1.FileName
                'On vide le fichier avant de le recréer 
                Call ViderFichier(sFicTemp)
                TraiteFichierPourri(sFicTemp, filePath)
            Catch SecEx As SecurityException
                MessageBox.Show($"Security error:{vbCrLf}{vbCrLf}{SecEx.Message}{vbCrLf}{vbCrLf}" &
                $"Details:{vbCrLf}{vbCrLf}{SecEx.StackTrace}")
            End Try
        End If
        Return sFicTemp
    End Function
    Public Sub ViderFichier(ByVal cheminFichier As String)
        Try
            ' Ouvrir le fichier en mode écriture pour le vider 
            System.IO.File.WriteAllText(cheminFichier, String.Empty)
            MsgBox("Le fichier a été vidé avec succès.")
        Catch ex As Exception
            ' Gérer les exceptions en cas d'erreur 
            MsgBox("ViderFichier, erreur : " & ex.Message)
        End Try
    End Sub
    Private Sub TraiteFichierCsvMensuel(sFicTemp As String, sFichier As String)
        Dim sLigneEntiere As String = ""
        Dim bLigne1 As Boolean = True
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            Dim file As System.IO.StreamWriter
            file = My.Computer.FileSystem.OpenTextFileWriter(sFicTemp, True)

            ligne = monStreamReader.ReadLine
            While ligne IsNot Nothing
                ' Match ignoring case of letters.
                Dim match As Match = Regex.Match(ligne, "^[0-3][0-9]/[0-9]{2}/[0-9]{4};", RegexOptions.IgnoreCase)
                If match.Success Then
                    'On est au début d'une ligne => on écrit le ligne en cours et on en commence une autre
                    'Mais pas sur la 1ère qui contient des entêtes
                    If bLigne1 = False Then
                        file.WriteLine(sLigneEntiere)
                    Else
                        bLigne1 = False
                    End If
                    sLigneEntiere = ligne
                Else
                    sLigneEntiere &= ligne
                End If
                ligne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
            file.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue sur a lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Sub TraiteFichierPourri(sFicTemp As String, sFichier As String)
        Dim sLigneEntiere As String = ""
        Dim bLigne1 As Boolean = True

        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            Dim file As System.IO.StreamWriter
            file = My.Computer.FileSystem.OpenTextFileWriter(sFicTemp, True)

            ligne = monStreamReader.ReadLine
            While ligne IsNot Nothing
                'Détection de la date du mouvement qui est le début d'une ligne => déclenche l'écriture
                Dim match As Match = Regex.Match(ligne, "^[0-3][0-9]/[0-9]{2}/[0-9]{4}", RegexOptions.IgnoreCase)
                If match.Success Then
                    'On est au début d'une ligne => on écrit la ligne en cours et on en commence une autre 
                    file.WriteLine(sLigneEntiere)
                    sLigneEntiere = formatteLigne(ligne, True)
                Else
                    sLigneEntiere &= formatteLigne(ligne, False)
                End If
                ligne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
            file.Close()
        Catch ex As Exception
            MsgBox("TraiteFichierPourri : Une erreur est survenue sur la lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Function formatteLigne(sLigne As String, bPresenceDate As Boolean) As String
        Dim sLigneFormattee As String = "", sTmp As String

        'Il faut alimenter séparément les débits et les crédits et enlever les signes et la monnaie
        If InStr(sLigne, "-", CompareMethod.Text) > 0 Then
            sTmp = sLigne.Replace(" €", "")
            sLigneFormattee = sLigneFormattee + ";" + sTmp.Replace("-", "") + ";"
        ElseIf InStr(sLigne, "+", CompareMethod.Text) > 0 Then
            sTmp = sLigne.Replace(" €", "")
            sLigneFormattee = sLigneFormattee + ";" + ";" + sTmp.Replace("+", "")
        Else
            If bPresenceDate Then
                sLigneFormattee = sLigneFormattee + sLigne + ";"
            Else
                sLigneFormattee = sLigneFormattee + sLigne + " "
            End If
        End If
        Return sLigneFormattee
    End Function
End Module
