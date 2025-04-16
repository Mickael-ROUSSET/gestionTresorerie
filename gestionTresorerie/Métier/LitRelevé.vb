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
        Dim ficRelevéTraité As String = lectureProprietes.GetVariable("ficRelevéTraité")
        'Un seul fichier peut être sélectionné
        openFileDialog1.Multiselect = False
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
            Try
                Dim filePath = openFileDialog1.FileName
                'On vide le fichier avant de le recréer 
                Call ViderFichier(ficRelevéTraité)
                TraiteFichierPourri(ficRelevéTraité, filePath)
            Catch SecEx As SecurityException
                MessageBox.Show($"Security error:{vbCrLf}{SecEx.Message}{vbCrLf}" & $"Details:{vbCrLf}{SecEx.StackTrace}")
                Logger.GetInstance.ERR($"Security error:{SecEx.Message}" & $"Details:{SecEx.StackTrace}")
            End Try
        End If
        Return ficRelevéTraité
    End Function
    Public Sub ViderFichier(ByVal cheminFichier As String)
        Try
            ' Ouvrir le fichier en mode écriture pour le vider 
            System.IO.File.WriteAllText(cheminFichier, String.Empty)
            Logger.GetInstance.INFO("Le fichier " & cheminFichier & " a été vidé avec succès.")
        Catch ex As Exception
            ' Gérer les exceptions en cas d'erreur 
            MsgBox("ViderFichier, erreur : " & ex.Message)
            Logger.GetInstance.ERR("ViderFichier, erreur : " & ex.Message)
        End Try
    End Sub
    Private Sub TraiteFichierCsvMensuel(sFicTemp As String, sFichier As String)
        Dim sLigneEntiere As String = String.Empty
        Dim bLigne1 As Boolean = True
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            Dim file As System.IO.StreamWriter
            file = My.Computer.FileSystem.OpenTextFileWriter(sFicTemp, True)

            ligne = monStreamReader.ReadLine
            While ligne IsNot Nothing
                ' Match ignoring case of letters.
                Dim match As Match = Regex.Match(ligne, Constantes.regDateReleve & ";", RegexOptions.IgnoreCase)
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
            MsgBox("Une erreur est survenue sur la lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
            Logger.GetInstance.ERR("Une erreur est survenue sur la lecture du relevé : " & sFichier)
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
            Logger.GetInstance.INFO("TraiteFichierPourri : traitement du fichier : " & sFicTemp)

            ligne = monStreamReader.ReadLine
            While ligne IsNot Nothing
                'Détection de la date du mouvement qui est le début d'une ligne => déclenche l'écriture
                Dim match As Match = Regex.Match(ligne, Constantes.regDateReleve, RegexOptions.IgnoreCase)
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
            Logger.GetInstance.ERR("TraiteFichierPourri : Une erreur est survenue sur la lecture du relevé : " & sFichier)
        End Try
    End Sub
    Private Function formatteLigne(sLigne As String, bPresenceDate As Boolean) As String
        Dim sLigneFormattee As String = String.Empty, sTmp As String

        'Il faut alimenter séparément les débits et les crédits et enlever les signes et la monnaie
        If InStr(sLigne, Constantes.tiret, CompareMethod.Text) > 0 Then
            sTmp = sLigne.Replace(Constantes.euro, String.Empty)
            sLigneFormattee = sLigneFormattee + Constantes.pointVirgule + sTmp.Replace(Constantes.tiret, String.Empty) + Constantes.pointVirgule
        ElseIf InStr(sLigne, Constantes.plus, CompareMethod.Text) > 0 Then
            sTmp = sLigne.Replace(Constantes.euro, String.Empty)
            sLigneFormattee = sLigneFormattee + Constantes.pointVirgule + Constantes.pointVirgule + sTmp.Replace("+", String.Empty)
        Else
            If bPresenceDate Then
                sLigneFormattee = sLigneFormattee + sLigne + Constantes.pointVirgule
            Else
                sLigneFormattee = sLigneFormattee + sLigne + Constantes.espace
            End If
        End If
        Return sLigneFormattee
    End Function
End Module
